/**
 * wasm-bridge.js
 * Loaded in authenticated pages. Connects jQuery/pdfmake to the Blazor WASM
 * PDF generator component. jQuery calls window.wasmGeneratePdf(config),
 * WASM checks subscription and calls back to executePdfGeneration().
 */

var _wasmPdfHandler = null;

// Called by Blazor WASM on init to register itself
window.registerWasmPdfHandler = function(dotnetRef) {
    _wasmPdfHandler = dotnetRef;
    console.log('[WASM] PDF handler registered');
};

/**
 * Main entry point called by all existing jQuery PDF generation code.
 * Replace all direct pdfmake calls in gphome.js, cashbook.js etc. with this.
 *
 * config = { type: 'form6'|'form8'|'nmr'|'cashbook'|..., data: {...} }
 */
window.wasmGeneratePdf = function(config) {
    if (!_wasmPdfHandler) {
        console.warn('[WASM] Handler not ready, falling back to direct pdfmake');
        // Fallback: generate without watermark check (WASM not loaded)
        executePdfGeneration(JSON.stringify(config), false);
        return;
    }
    _wasmPdfHandler.invokeMethodAsync('GeneratePdfAsync', JSON.stringify(config))
        .catch(function(err) {
            console.error('[WASM] PDF generation error:', err);
            window.onWasmPdfError && window.onWasmPdfError(err.message || 'PDF generation failed');
        });
};

/**
 * Check subscription without generating PDF.
 * Returns Promise<bool>
 */
window.wasmCheckSubscription = function() {
    if (!_wasmPdfHandler) return Promise.resolve(false);
    return _wasmPdfHandler.invokeMethodAsync('CheckSubscriptionAsync');
};

/**
 * Parse job-card (JC) rows from a NIC muster roll HTML string.
 * Mirrors the parsing in the original gpmnrega2 gphome.js.
 * Returns array of { JCNO, appName, appAddress, category, wageList, BankName, DateCreated }
 */
function _parseJCFromHtml(html) {
    try {
        var parser = new DOMParser();
        var doc = parser.parseFromString(html, 'text/html');
        var rows = doc.querySelectorAll('#ctl00_ContentPlaceHolder1_grdShowRecords tr');
        console.log('[WASM bridge] _parseJCFromHtml: rows found =', rows ? rows.length : 0);
        if (!rows || rows.length < 2) {
            console.warn('[WASM bridge] _parseJCFromHtml: grid not found or empty. HTML snippet:', html ? html.substring(0, 300) : '(null)');
            return [];
        }

        var headerCells = rows[0].cells.length;
        var wageListId  = headerCells - 5;
        var bankNameId  = headerCells - 8;
        var dateCreatedId = headerCells - 3;

        var JC = [];
        for (var j = 1; j < rows.length - 1; j++) {
            try {
                var cells = rows[j].cells;
                if (!cells || cells.length < 4) continue;

                // Applicant name: strip leading <font> tag and trailing (relation) info
                var rawName = cells[1].innerHTML.split('<br>')[0];
                var prefix  = '<font face="Verdana" color="#284775" size="2">';
                var appName = rawName.indexOf(prefix) === 0
                    ? rawName.substr(prefix.length)
                    : rawName;
                appName = appName.indexOf('(') !== -1
                    ? appName.substring(0, appName.indexOf('(')).trim()
                    : appName.trim();
                appName = appName.replace('\\', '');

                var anchor  = cells[1].querySelector('a');
                var jcno    = anchor ? anchor.textContent.trim() : '';
                var catText = cells[2].textContent.trim();
                var category = catText.length > 3 ? catText.substr(0, 3) : catText;

                JC.push({
                    JCNO:        jcno,
                    appName:     appName,
                    appAddress:  cells[3].textContent.trim(),
                    category:    category,
                    wageList:    cells[wageListId]  ? cells[wageListId].textContent.trim()  : '',
                    BankName:    cells[bankNameId]  ? cells[bankNameId].textContent.trim()  : '',
                    DateCreated: cells[dateCreatedId] ? cells[dateCreatedId].textContent.trim() : ''
                });
            } catch(rowErr) { /* skip malformed row */ }
        }
        return JC;
    } catch(e) {
        console.warn('[WASM bridge] _parseJCFromHtml error:', e);
        return [];
    }
}

/**
 * Extract DateFrom / DateTo from a NIC muster roll HTML string.
 */
function _parseNmrDatesFromHtml(html) {
    try {
        var parser = new DOMParser();
        var doc = parser.parseFromString(html, 'text/html');
        var fromEl = doc.querySelector('#ctl00_ContentPlaceHolder1_lbldatefrom');
        var toEl   = doc.querySelector('#ctl00_ContentPlaceHolder1_lbldateto');
        return {
            DateFrom: fromEl ? fromEl.textContent.trim() : '',
            DateTo:   toEl   ? toEl.textContent.trim()   : ''
        };
    } catch(e) { return { DateFrom: '', DateTo: '' }; }
}

/**
 * Called back BY WASM with the isTrialUser flag.
 * This is where pdfmake actually runs, using the existing JS templates.
 *
 * config     : JSON string of PDF config from jQuery
 * addWatermark: boolean — true if trial user
 */
window.executePdfGeneration = function(configJson, addWatermark) {
    try {
        var config = typeof configJson === 'string' ? JSON.parse(configJson) : configJson;
        var type   = config.type;
        var data   = config.data || {};

        // Expose watermark flag as a global so form templates can read it.
        // addWatermark comes from the WASM subscription check (server-side) —
        // true = trial user → watermark must appear; false = paid → no watermark.
        window._gpAddWatermark = !!addWatermark;

        // Add watermark to docDefinition if trial
        if (addWatermark && config.docDefinition) {
            config.docDefinition.watermark = {
                text: 'TRIAL VERSION',
                color: '#3730a3',
                opacity: 0.08,
                bold: true,
                fontSize: 60,
                angle: -45
            };
        }

        // ── Forms that use elm.dataset.nmrno + reportData.NMRS[x].JC ──
        // Actual function names (note: genereateform8 has a typo with double-e — preserved as-is)
        var jcFormFns = {
            'form6':    'generateform6',
            'form8':    'genereateform8',
            'form8plus':'generate8plus',
            'form9':    'generateform9'
        };

        if (jcFormFns[type]) {
            var fnName = jcFormFns[type];
            var fn = (typeof window[fnName] === 'function') ? window[fnName] : null;
            if (!fn) {
                console.warn('[WASM bridge] Generator function not found:', fnName);
                window.onWasmPdfError && window.onWasmPdfError('Generator not loaded: ' + fnName);
                return;
            }

            var nmr   = data.nmr || {};
            var nmrno = String(nmr.NMRNO || nmr.nmrno || '');

            // Parse JC from NIC HTML and populate into reportData.NMRS entry
            if (data.html && nmrno) {
                var jcData = _parseJCFromHtml(data.html);
                var dates  = _parseNmrDatesFromHtml(data.html);
                var nmrEntry = null;
                for (var i = 0; i < reportData.NMRS.length; i++) {
                    if (String(reportData.NMRS[i].NMRNO) === nmrno) {
                        nmrEntry = reportData.NMRS[i];
                        break;
                    }
                }
                if (nmrEntry) {
                    // Only overwrite existing JC if we actually parsed something.
                    // Protects against proxy returning wrong/error HTML that yields [].
                    if (jcData.length > 0 || !Array.isArray(nmrEntry.JC) || nmrEntry.JC.length === 0) {
                        nmrEntry.JC = jcData;
                    }
                    if (dates.DateFrom) nmrEntry.DateFrom = dates.DateFrom;
                    if (dates.DateTo)   nmrEntry.DateTo   = dates.DateTo;
                } else {
                    // NMR not in NMRS list — add minimal entry so template can find it
                    nmrEntry = {
                        NMRNO:    nmrno,
                        JC:       jcData,
                        DateFrom: dates.DateFrom,
                        DateTo:   dates.DateTo,
                        url:      nmr.url || ''
                    };
                    reportData.NMRS.push(nmrEntry);
                }

                // ── Persist updated NMR (with JC) back to datasync ──────────────
                // Mirrors original gpmnrega2: after parsing JC from Musternew.aspx,
                // the old code immediately POSTed { NMRNO, DateFrom, DateTo, url, JC }
                // to datasync so future work-code loads return JC from DB without
                // hitting NIC again. V2 must do the same after parsing JC here.
                if (jcData.length > 0 && typeof reportData !== 'undefined' &&
                    reportData.district_code && reportData.workcode && nmrEntry) {
                    (function(entrySnapshot) {
                        $.ajax({
                            type: 'POST',
                            url: 'https://datasync.s3kn.com/api/msrdata' +
                                '?distcode=' + encodeURIComponent(reportData.district_code) +
                                '&msrno='    + encodeURIComponent(entrySnapshot.NMRNO) +
                                '&workcode=' + encodeURIComponent((reportData.workcode || '').toUpperCase()),
                            contentType: 'application/json',
                            data: '"' + JSON.stringify(entrySnapshot).replace(/"/g, "'") + '"',
                            error: function () { /* silent — best-effort save */ }
                        });
                    })(JSON.parse(JSON.stringify(nmrEntry))); // deep-copy snapshot
                }
            }

            // Create synthetic element — template reads elm.dataset.nmrno
            var elm = document.createElement('span');
            elm.dataset.nmrno = nmrno;
            fn(elm);
            window.onDownloadComplete && window.onDownloadComplete();
            return;
        }

        // ── Blank NMR ────────────────────────────────────────────────
        if (type === 'nmr') {
            if (typeof buildBlankNMR === 'function') {
                buildBlankNMR(data.accdata || data, data.jcdata || null, data.workdata || null);
            }
            window.onDownloadComplete && window.onDownloadComplete();
            return;
        }

        // ── Wage List / FTO — synthetic elm ──────────────────────────
        if (type === 'wagelist' || type === 'fto') {
            var wfFnName = type === 'wagelist' ? 'generatewagelist' : 'generatefto';
            var wfFn = (typeof window[wfFnName] === 'function') ? window[wfFnName] : null;
            if (wfFn) {
                var wfElm = document.createElement('span');
                if (data.nmr) wfElm.dataset.nmrno = String(data.nmr.NMRNO || '');
                wfFn(wfElm);
            } else if (config.docDefinition) {
                pdfMake.createPdf(config.docDefinition).download(config.filename || 'report.pdf');
            } else {
                console.warn('[WASM bridge] Generator not found:', wfFnName);
            }
            window.onDownloadComplete && window.onDownloadComplete();
            return;
        }

        // ── Other types: cashbook, workorder, checklist, completion, coverpage, direct docDef ──
        // Normalise type to lowercase so button casing ('CoverPage', 'WorkOrder') doesn't matter
        type = type ? type.toLowerCase() : type;
        var otherGenerators = {
            'cashbook':   typeof generateCashbookPdf   === 'function' ? generateCashbookPdf   : null,
            'workorder':  typeof generateworkorder      === 'function' ? generateworkorder      : null,
            'checklist':  typeof generatechecklist      === 'function' ? generatechecklist      : null,
            'completion': typeof generatecompletion     === 'function' ? generatecompletion     : null,
            'coverpage':  typeof generatecoverpage      === 'function' ? generatecoverpage      : null,
        };

        var gen = otherGenerators[type];
        if (gen) {
            gen(data, addWatermark);
        } else if (config.docDefinition) {
            // Direct docDefinition passed — use pdfmake directly
            pdfMake.createPdf(config.docDefinition).download(config.filename || 'report.pdf');
        } else {
            console.warn('[WASM bridge] Unknown PDF type:', type);
        }

        window.onDownloadComplete && window.onDownloadComplete();

    } catch(err) {
        console.error('[WASM bridge] executePdfGeneration error:', err);
        window.onWasmPdfError && window.onWasmPdfError(err.message);
    }
};

window.onWasmPdfError = function(msg) {
    console.error('[WASM] Error:', msg);
    if (typeof gpToast === 'function') {
        gpToast('PDF generation failed: ' + msg, 'error');
    }
    if (typeof onDownloadError === 'function') {
        onDownloadError(msg);
    }
};
