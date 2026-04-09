/**
 * gphome.js v2.5
 * Key changes from v2.0:
 *  1. Added initReportData() — populates reportData from Razor-injected globals
 *     (replaces old UserData cookie parsing)
 *  2. Added loadStatsAndAutocomplete() — loads DPR + job card stats on page init
 *  3. Added bindautocomplete() — jQuery UI autocomplete on work code search
 *  4. Added JobCardsDetails() — parses NIC HTML for sidebar stats
 *
 * Changes from v1.x (carried over from v2.0):
 *  1. All $.ajax URLs updated from /api/*.aspx to /api/proxy/*
 *  2. GIF loading replaced with gpShowLoading() / gpHideLoading() CSS spinner
 *  3. PDF calls routed through window.wasmGeneratePdf() (WASM bridge)
 *  4. Error handling improved — alerts replaced with gpToast()
 */
// ── GP UI utilities ──────────────────────────────────────────────────────────
// Defined here so they are always available regardless of which layout is used.
// The layout's inline copies (if present) are no-ops because these run first.

function gpShowLoading(label) {
    var el  = document.getElementById('gpLoading');
    var lbl = document.getElementById('gpLoadingLabel');
    if (el)  el.classList.add('show');
    if (lbl) lbl.textContent = label || 'Loading…';
}
function gpHideLoading() {
    var el = document.getElementById('gpLoading');
    if (el) el.classList.remove('show');
}
function gpShowProgress(label, pct) {
    var ov    = document.getElementById('gpProgressOverlay');
    var lbl   = document.getElementById('gpProgressLabel');
    var fill  = document.getElementById('gpProgressFill');
    var pctEl = document.getElementById('gpProgressPct');
    if (lbl)  lbl.textContent  = label || 'Please wait…';
    var p = Math.min(Math.max(pct || 0, 0), 100);
    if (fill)  fill.style.width = p + '%';
    if (pctEl) pctEl.textContent = p + '%';
    if (ov)    ov.classList.add('show');
}
function gpUpdateProgress(label, pct) {
    var lbl   = document.getElementById('gpProgressLabel');
    var fill  = document.getElementById('gpProgressFill');
    var pctEl = document.getElementById('gpProgressPct');
    if (lbl)  lbl.textContent  = label || '';
    var p = Math.min(Math.max(pct || 0, 0), 100);
    if (fill)  fill.style.width = p + '%';
    if (pctEl) pctEl.textContent = p + '%';
}
function gpHideProgress() {
    var ov    = document.getElementById('gpProgressOverlay');
    var fill  = document.getElementById('gpProgressFill');
    var pctEl = document.getElementById('gpProgressPct');
    if (fill)  fill.style.width = '100%';
    if (pctEl) pctEl.textContent = '100%';
    setTimeout(function () {
        if (ov)    ov.classList.remove('show');
        if (fill)  fill.style.width = '0%';
        if (pctEl) pctEl.textContent = '0%';
    }, 400);
}
function gpToast(message, type) {
    var icons = {
        success: 'bi-check-circle-fill',
        error:   'bi-x-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        info:    'bi-info-circle-fill'
    };
    var container = document.getElementById('gpToastContainer');
    if (!container) { console.warn('[gpToast]', type, message); return; }
    var t = document.createElement('div');
    t.className = 'gp-toast ' + (type || 'info');
    t.innerHTML =
        '<i class="bi ' + (icons[type] || icons.info) + ' toast-icon"></i>' +
        '<span class="toast-msg">' + message + '</span>' +
        '<button class="toast-close" onclick="this.parentElement.remove()">&#x2715;</button>';
    container.appendChild(t);
    setTimeout(function () { if (t.parentElement) t.remove(); }, 4000);
}

// ─────────────────────────────────────────────────────────────────────────────
var WageList = '';
var FTO = '';
var downloadFtoBtn = false;
var gpData = {};       // secondary copy (used by form templates)
var jobCardsHH = {};   // household data keyed by job card number
var DPRresponse = {};  // DPR work dictionary { "WORK_CODE": ["name","status","url","code"] }
var reportData = {
    userName: '', stateName: '', stateNameRegional: '',
    districtName: '', districtNameRegional: '',
    blockName: '', blockNameRegional: '',
    panchayat_NameRegional: '', panchayatName: '',
    panchayat_code: '', district_code: '', block_code: '',
    state_code: '', state_shortname: '',
    fincialYear: '',
    LokSabha: '', VidhanSabha: '',
    workcode: '', workName: '', startdate: '',
    workCategory: '', workYear: '', workCostTotal: '',
    executionAgency: '', executionLevel: '',
    sanctionAmount: '', sanctionDate: '',
    finSanctionNo: '', finSanctionDate: '',
    techSanctionNo: '', techSanctionDate: '',
    UskilledExp: '', MaterialCost: '', SkilledCost: '',
    workStatus: '', agencyCode: '', DPRFrozen: '',
    NMRS: [],
    Material: { Materials: [] },
    AssetLink: ''
};
// ── API base path (updated from *.aspx to clean API routes) ───────
var _API = '/api/proxy/';
// ── Initialize reportData from Razor globals ──────────────────────
// Replaces original loadData() which parsed the UserData cookie.
// Called from Home.cshtml $(document).ready().
function initReportData(geo) {
    if (!geo) return;
    var fields = [
        'panchayat_code', 'panchayatName', 'panchayat_NameRegional',
        'districtName', 'district_code', 'districtNameRegional',
        'blockName', 'block_code', 'blockNameRegional',
        'VidhanSabha', 'LokSabha', 'userName',
        'stateName', 'stateNameRegional', 'state_code', 'state_shortname',
        'fincialYear'
    ];
    for (var i = 0; i < fields.length; i++) {
        var f = fields[i];
        if (geo[f]) {
            reportData[f] = geo[f];
            gpData[f] = geo[f];
        }
    }
}
// ── LocalStorage helpers ─────────────────────────────────────────
var _LS_DPR_PREFIX = 'dpr_v2_';     // key: dpr_v2_<gpcode>_<finyear>
var _LS_JC_PREFIX = 'jc_v2_';      // key: jc_v2_<gpcode>_<finyear>
var _LS_TTL_MS = 8 * 60 * 60 * 1000; // 8 hours — refresh from NIC after this
function _lsSet(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify({ ts: Date.now(), v: value }));
    } catch (e) { console.log('[LS] write error:', e); }
}
function _lsGet(key) {
    try {
        var raw = localStorage.getItem(key);
        if (!raw) return null;
        var obj = JSON.parse(raw);
        if (!obj || !obj.ts || !obj.v) return null;
        if (Date.now() - obj.ts > _LS_TTL_MS) {
            localStorage.removeItem(key);
            return null; // expired
        }
        return obj.v;
    } catch (e) { return null; }
}
// ── Stats & Autocomplete loading on page init ────────────────────
// Flow: localStorage → IndexedDB → NIC (each is a fallback for the previous)
function loadStatsAndAutocomplete() {
    var finYear = reportData.fincialYear || '2025-2026';
    var gpCode = reportData.panchayat_code;
    console.log('[GP Home] Loading stats — GP:', gpCode,
        'District:', reportData.district_code, 'Block:', reportData.block_code);
    // ── 1. DPR Works ────────────────────────────────────────────
    // Try localStorage first, then IndexedDB, then NIC
    var dprLsKey = _LS_DPR_PREFIX + gpCode + '_' + finYear;
    var cachedDpr = _lsGet(dprLsKey);
    if (cachedDpr) {
        console.log('[GP Home] DPR: loaded from localStorage cache');
        try {
            var parsed = typeof cachedDpr === 'string' ? JSON.parse(cachedDpr) : cachedDpr;
            bindautocomplete(parsed);
        } catch (e) { console.log('[GP Home] DPR cache parse error:', e); }
    } else if (typeof readDPRStatus === 'function' && localStorage['DPRLoad'] === 'success') {
        // IndexedDB has data — readDPRStatus will call loadDPRWorkLinks which calls bindautocomplete
        console.log('[GP Home] DPR: trying IndexedDB cache');
        readDPRStatus(gpCode);
        // Still fire NIC in background to refresh if IndexedDB data is stale
        _fetchDprFromNic(gpCode, finYear, dprLsKey, true);
    } else {
        // No cache — fetch fresh from NIC
        console.log('[GP Home] DPR: no cache, fetching from NIC');
        _fetchDprFromNic(gpCode, finYear, dprLsKey, false);
    }
    // ── 2. Job Card Stats ────────────────────────────────────────
    // Try localStorage first, then NIC
    var jcLsKey = _LS_JC_PREFIX + gpCode + '_' + finYear;
    var cachedJc = _lsGet(jcLsKey);
    if (cachedJc) {
        console.log('[GP Home] JobCards: loaded from localStorage cache');
        if (typeof cachedJc === 'string') {
            JobCardsDetails(cachedJc);
        }
    } else {
        console.log('[GP Home] JobCards: no cache, fetching from NIC');
        _fetchJobCardsFromNic(gpCode, finYear, jcLsKey);
    }
}
// ── Fetch DPR from NIC and cache result ─────────────────────────
function _fetchDprFromNic(gpCode, finYear, lsKey, isBackgroundRefresh) {
    $.ajax({
        url: _API + 'getworkdata',
        type: 'GET',
        data: {
            state_name: 'KARNATAKA',
            state_code: '15',
            district_name: reportData.districtName,
            district_code: reportData.district_code,
            block_name: reportData.blockName,
            block_code: reportData.block_code,
            panchayat_name: reportData.panchayatName,
            panchayat_code: gpCode,
            fin_year: finYear,
            short_name: 'KN',
            work_name: '',
            source: '',
            Digest: 's3kn'
        },
        success: function (resp) {
            if (resp && typeof resp === 'object' && !resp.error && Object.keys(resp).length > 0) {
                console.log('[GP Home] DPR: fetched', Object.keys(resp).length, 'works from NIC');
                // Cache in localStorage (fast) and IndexedDB (persistent)
                _lsSet(lsKey, JSON.stringify(resp));
                if (typeof addDPRFReport === 'function') {
                    addDPRFReport(gpCode, JSON.stringify(resp));
                }
                // Only update UI if this was not a background refresh (IndexedDB already rendered)
                if (!isBackgroundRefresh) {
                    bindautocomplete(resp);
                }
            } else if (!isBackgroundRefresh) {
                console.log('[GP Home] DPR: empty response from NIC');
                ['appWork', 'ongoingWork', 'completedWork'].forEach(function (id) {
                    var el = document.getElementById(id);
                    if (el) el.textContent = '0';
                });
            }
        },
        error: function (xhr) {
            if (!isBackgroundRefresh) {
                console.log('[GP Home] DPR load failed:', xhr.status);
                ['appWork', 'ongoingWork', 'completedWork'].forEach(function (id) {
                    var el = document.getElementById(id);
                    if (el) el.textContent = 'NIC error';
                });
            }
        }
    });
}
// ── Fetch Job Card stats from NIC and cache result ───────────────
function _fetchJobCardsFromNic(gpCode, finYear, lsKey) {
    $.ajax({
        url: _API + 'getcatwisejobcards',
        type: 'GET',
        dataType: 'text',
        data: {
            Panchayat_Code: gpCode,
            block_code: reportData.block_code,
            dist_code: reportData.district_code,
            block_name: reportData.blockName,
            dist_name: reportData.districtName,
            panchayatname: reportData.panchayatName,
            fin_year: finYear
        },
        success: function (resp) {
            if (resp && typeof resp === 'string' && resp.length > 10) {
                console.log('[GP Home] JobCards: fetched from NIC, length:', resp.length);
                _lsSet(lsKey, resp);   // cache raw text
                JobCardsDetails(resp);
            }
        },
        error: function (xhr) {
            console.log('[GP Home] JobCards load failed:', xhr.status);
            ['totaljobcard', 'totalPersondays', 'totalpersoncard', 'disabledjobcard', 'issuedNmr'].forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.textContent = 'NIC error';
            });
        }
    });
}
// parseDPRFromHtml removed — server now returns JSON dictionary directly
// (multi-step NIC crawl happens server-side in ProxyController.GetWorkData)
// ── Bind autocomplete & work counts ───────────────────────────────
// Mirrors original bindautocomplete() from gphome.js v1.x
function bindautocomplete(dprdetails) {
    // Store globally so _loadWorkDataFromNic can look up the NIC detail URL
    DPRresponse = dprdetails || {};
    var sourceItems = [];
    var ac = 0, cc = 0, oc = 0;
    for (var key in dprdetails) {
        if (dprdetails.hasOwnProperty(key)) {
            if (dprdetails[key][1] === 'Approved')
                ac += 1;
            else if (dprdetails[key][1] === 'Completed')
                cc += 1;
            else
                oc += 1;
            sourceItems.push({
                value: key,
                label: key + ' (' + dprdetails[key][0] + ')'
            });
        }
    }
    // Update stats sidebar — work counts
    if (typeof window.onStatsLoaded === 'function') {
        window.onStatsLoaded(
            { approved: ac, ongoing: oc, completed: cc },
            null, // employment — filled by JobCardsDetails
            null  // emuster — filled by JobCardsDetails
        );
    }
    // Set up jQuery UI autocomplete on work code search input
    if ($.fn.autocomplete && sourceItems.length > 0) {
        $("#txtSearchWork").autocomplete({
            source: function (request, response) {
                response($.grep(sourceItems, function (item) {
                    // Match last 5 chars of work code (original behavior)
                    var srch = item.value.substr(item.value.length - 5, 5);
                    if (srch.indexOf(request.term) > -1) {
                        return item;
                    }
                }));
            },
            minLength: 3,
            select: function (event, ui) {
                $('#txtSearchWork').val(ui.item.value);
                searchWork();
                return false;
            }
        });
    }
}
// ── loadDPRWorkLinks — called by db.js readDPRStatus (IndexedDB cache)
function loadDPRWorkLinks(response) {
    try {
        var dprData = JSON.parse(response);
        bindautocomplete(dprData);
    } catch (e) {
        console.log('loadDPRWorkLinks parse error:', e);
    }
}
// ── Job Card Stats parsing ────────────────────────────────────────
// Mirrors original JobCardsDetails() from gphome.js v1.x
// Parses $$-separated NIC HTML for sidebar stats
function JobCardsDetails(rawResponse) {
    try {
        if (!rawResponse || typeof rawResponse !== 'string') return;
        var sections = rawResponse.split('$$');
        if (sections.length < 4) {
            console.log('[GP Home] JobCardsDetails: Expected 4 sections, got', sections.length);
            return;
        }
        var e = sections[0];      // regcat — last <tr> of skyblue table
        var mustroll = sections[1]; // muster roll movement HTML
        var disabled = sections[2]; // disabled persons HTML
        var scst = sections[3];     // SCST employment HTML
        // ── Parse regcat section (job card counts by category) ──
        // Original: $(e)[0].cells[N].innerText
        try {
            if (e && e.trim()) {
                var $row = $(e);
                if ($row.length > 0 && $row[0].cells) {
                    var cells = $row[0].cells;
                    if ($('#scjobcard').length) $('#scjobcard').text((cells[3]?.innerText?.trim() || '0') + ' & ' + (cells[4]?.innerText?.trim() || '0'));
                    if ($('#stjobcard').length) $('#stjobcard').text((cells[5]?.innerText?.trim() || '0') + ' & ' + (cells[6]?.innerText?.trim() || '0'));
                    if ($('#otherjobcard').length) $('#otherjobcard').text((cells[7]?.innerText?.trim() || '0') + ' & ' + (cells[8]?.innerText?.trim() || '0'));
                    if ($('#jobcardMen').length) $('#jobcardMen').text(cells[9]?.innerText?.trim() || '0');
                    if ($('#jobcardWomen').length) $('#jobcardWomen').text(cells[10]?.innerText?.trim() || '0');
                    if ($('#totaljobcard').length) $('#totaljobcard').text((cells[1]?.innerText?.trim() || '0') + ' & ' + (cells[2]?.innerText?.trim() || '0'));
                    console.log('[GP Home] Regcat stats parsed');
                }
            }
        } catch (ex) { console.log('[GP Home] regcat parse error:', ex); }
        // ── Parse SCST section (persondays by category) ──
        // Original: $(scst).find('table')[3].querySelectorAll('tr')
        var employment = {
            totalPersondays: 0, scPersondays: 0, stPersondays: 0, womenPersondays: 0, otherPersondays: 0,
            scPersoncard: 0, stPersoncard: 0, otherPersoncard: 0, totalPersoncard: 0
        };
        try {
            if (scst && scst.trim()) {
                var $scst = $(scst);
                var scstTables = $scst.find('table');
                if (scstTables.length >= 4) {
                    $(scstTables[3]).find('tr').each(function (i, row) {
                        if (i > 3) {
                            var tds = $(row).find('td');
                            if (tds.length > 1 && tds.eq(1).text().trim() === reportData.panchayatName.trim()) {
                                employment.totalPersondays = tds.eq(14).text().trim();
                                employment.scPersondays = tds.eq(11).text().trim();
                                employment.stPersondays = tds.eq(12).text().trim();
                                employment.womenPersondays = tds.eq(15).text().trim();
                                employment.otherPersondays = tds.eq(13).text().trim();
                                employment.scPersoncard = tds.eq(6).text().trim();
                                employment.stPersoncard = tds.eq(7).text().trim();
                                employment.otherPersoncard = tds.eq(8).text().trim();
                                employment.totalPersoncard = tds.eq(9).text().trim();
                            }
                        }
                    });
                    // Update DOM elements if they exist (matches original IDs)
                    if ($('#totalPersondays').length) $('#totalPersondays').text(employment.totalPersondays);
                    if ($('#scPersondays').length) $('#scPersondays').text(employment.scPersondays);
                    if ($('#stPersondays').length) $('#stPersondays').text(employment.stPersondays);
                    if ($('#womenPersondays').length) $('#womenPersondays').text(employment.womenPersondays);
                    if ($('#otherPersondays').length) $('#otherPersondays').text(employment.otherPersondays);
                    if ($('#scpersoncard').length) $('#scpersoncard').text(employment.scPersoncard);
                    if ($('#stpersoncard').length) $('#stpersoncard').text(employment.stPersoncard);
                    if ($('#otherpersoncard').length) $('#otherpersoncard').text(employment.otherPersoncard);
                    if ($('#totalpersoncard').length) $('#totalpersoncard').text(employment.totalPersoncard);
                    console.log('[GP Home] SCST employment stats parsed');
                }
            }
        } catch (ex) { console.log('[GP Home] scst parse error:', ex); }
        // ── Parse muster roll section (NMR issued/filled/zero) ──
        // Original: $(mustroll).find('a').filter(...)
        var emuster = { issued: 0, filled: 0, zeroAttendance: 0 };
        try {
            if (mustroll && mustroll.trim()) {
                $(mustroll).find('a').each(function () {
                    try {
                        var href = $(this).attr('href') || '';
                        var url = new URL(href, 'https://nregastrep.nic.in');
                        var type = url.searchParams.get('type');
                        var pc = (url.searchParams.get('panchayat_code') || '').trim();
                        if (pc === reportData.panchayat_code) {
                            var val = parseInt($(this).text().trim()) || 0;
                            if (type === '9') { emuster.issued = val; if ($('#issuedNmr').length) $('#issuedNmr').text(val); }
                            if (type === '10') { emuster.filled = val; if ($('#filledNmr').length) $('#filledNmr').text(val); }
                            if (type === '11') { emuster.zeroAttendance = val; if ($('#zeroattendence').length) $('#zeroattendence').text(val); }
                        }
                    } catch (ex) { }
                });
                console.log('[GP Home] Muster roll stats parsed:', emuster);
            }
        } catch (ex) { console.log('[GP Home] muster parse error:', ex); }
        // ── Parse disabled section ──
        // Original: $(disabled).find('table')[4].querySelectorAll('tr')
        try {
            if (disabled && disabled.trim()) {
                var $disabled = $(disabled);
                var disTables = $disabled.find('table');
                if (disTables.length >= 5) {
                    $(disTables[4]).find('tr').each(function (i, row) {
                        if (i > 1) {
                            var tds = $(row).find('td');
                            if (tds.length > 1 && tds.eq(1).text().trim() === reportData.panchayatName.trim()) {
                                if ($('#disabledjobcard').length) $('#disabledjobcard').text(tds.eq(2).text().trim());
                                if ($('#disabledperswork').length) $('#disabledperswork').text(tds.eq(3).text().trim());
                                if ($('#disabledwork').length) $('#disabledwork').text(tds.eq(4).text().trim());
                            }
                        }
                    });
                    console.log('[GP Home] Disabled stats parsed');
                }
            }
        } catch (ex) { console.log('[GP Home] disabled parse error:', ex); }
        // All span IDs already set directly above — matching original gphome.js behaviour.
        // onStatsLoaded is only used for the Works card (DPR phase).
        console.log('[GP Home] JobCardsDetails complete');
    } catch (ex) {
        console.log('[GP Home] JobCardsDetails error:', ex);
    }
}
// updateEmploymentStats / updateEmusterStats removed —
// JobCardsDetails sets all span IDs directly (matching original gphome.js).
// parseJobCardHtml removed — same reason.
// ── findgp — extract GP code from work code ───────────────────────
function findgp() {
    var pcode = $('#txtSearchWork').val().trim().toUpperCase().split('/')[0].trim();
    return pcode;
}
// ── Work search ────────────────────────────────────────────────────
function loadAllWorks(gpCode, finYear) {
    if (!gpCode) { gpToast('Panchayat code not set', 'warning'); return; }
    gpShowLoading('Loading works…');
    $.ajax({
        url: _API + 'getworkdata',
        type: 'GET',
        data: {
            district_code: reportData.district_code,
            block_code: reportData.block_code,
            panchayat_code: gpCode,
            fin_year: finYear
        },
        success: function (data) {
            gpHideLoading();
            if (data && !data.error) {
                if (typeof window.onWorkDataLoaded === 'function')
                    window.onWorkDataLoaded(data);
                // loadAllWorks shows all works for the GP — no single NMR list to build here
            } else {
                gpToast(data.error || 'Failed to load work data', 'error');
            }
        },
        error: function (xhr) {
            gpHideLoading();
            var msg = xhr.responseJSON && xhr.responseJSON.error
                ? xhr.responseJSON.error : 'Network error. Please try again.';
            gpToast(msg, 'error');
        }
    });
}
function loadWorkData(workCode, finYear) {
    if (!workCode) return;
    gpShowProgress('Searching work…', 10);
    try { var _c = JSON.parse((document.getElementById('gp-home-config') || {}).textContent || '{}'); localStorage.setItem('lastWorkCode_' + (_c.gpCode || ''), workCode); } catch (e) { }
    // Reset all work-specific fields before loading new work
    $.extend(reportData, {
        workcode: '', workName: '', startdate: '', workCategory: '', workYear: '',
        workCostTotal: '', executionAgency: '', executionLevel: '', workStatus: '',
        finSanctionNo: '', finSanctionDate: '', techSanctionNo: '', techSanctionDate: '',
        UskilledExp: '', MaterialCost: '', SkilledCost: '',
        NMRS: [], Material: { Materials: [] }, AssetLink: ''
    });
    var fy = finYear || '2025-2026';
    // ── Step 1: Try datasync DB first ───────────────────────────
    // Response shape: { WorkData: [{ WorkJson: "..." }], MsrData: [{ MsrJson: "..." }] }
    $.ajax({
        url: 'https://datasync.s3kn.com/api/getworkdata' +
            '?distcode=' + encodeURIComponent(reportData.district_code) +
            '&gpcode=' + encodeURIComponent(reportData.panchayat_code) +
            '&workcode=' + encodeURIComponent(workCode),
        type: 'GET',
        success: function (rawData) {
            // datasync returns a raw JSON string (no Content-Type: application/json),
            // so jQuery doesn't auto-parse it. Mirror original parseWorkCodeResponse:
            // JSON.parse(JSON.stringify(response)) → JSON.parse(b)
            var data = rawData;
            if (typeof data === 'string') {
                try { data = JSON.parse(data); } catch (e) { data = null; }
            }
            if (data && !data.error && data.WorkData && data.WorkData.length > 0) {
                console.log('[loadWorkData] loaded from datasync DB');
                try {
                    // WorkJson is a stringified JSON object (original format)
                    var wdetails = JSON.parse(
                        data.WorkData[0].WorkJson
                            .replaceAll("'", '"')
                            .replace(/[\n\r\t]/g, '')
                            .replace(/\\/g, '')
                    );
                    // Populate reportData from parsed work details
                    $.extend(reportData, {
                        workcode: wdetails.workcode || workCode,
                        workName: wdetails.workName || '',
                        startdate: wdetails.startdate || '',
                        workCategory: wdetails.workCategory || '',
                        workYear: wdetails.workYear || '',
                        workCostTotal: wdetails.workCostTotal || '',
                        executionAgency: wdetails.executionAgency || '',
                        executionLevel: wdetails.executionLevel || '',
                        workStatus: wdetails.workStatus || '',
                        finSanctionNo: wdetails.finSanctionNo || '',
                        finSanctionDate: wdetails.finSanctionDate || '',
                        techSanctionNo: wdetails.techSanctionNo || '',
                        techSanctionDate: wdetails.techSanctionDate || '',
                        UskilledExp: wdetails.UskilledExp || '',
                        MaterialCost: wdetails.MaterialCost || '',
                        SkilledCost: wdetails.SkilledCost || '',
                        Material: wdetails.Material || { Materials: [] }
                    });
                    // Load + render pre-existing NMRs from datasync MsrData
                    // (mirrors original checkNmrwithAsset → bindNMRs flow)
                    if (data.MsrData && data.MsrData.length > 0) {
                        reportData.NMRS = [];
                        var list = $('#ulNmrList');
                        list.find('.nmr-empty').remove();
                        data.MsrData.forEach(function (e) {
                            try {
                                var nmrdata = JSON.parse(
                                    e.MsrJson.replaceAll("'", '"').replace(/[\n\r\t]/g, '')
                                );
                                var nmrObj = {
                                    NMRNO: nmrdata.NMRNO || '',
                                    DateFrom: nmrdata.DateFrom || '',
                                    DateTo: nmrdata.DateTo || '',
                                    url: (nmrdata.url || '').replace('mnregaweb4.nic.in', 'nregastrep.nic.in'),
                                    JC: nmrdata.JC || []
                                };
                                if (nmrObj.NMRNO) {
                                    reportData.NMRS.push(nmrObj);
                                    _renderNmrItem(list, nmrObj, false);
                                }
                            } catch (ex) { }
                        });
                    }
                    gpHideLoading();
                    gpUpdateProgress('Work data loaded…', 40);
                    if (typeof window.onWorkDataLoaded === 'function')
                        window.onWorkDataLoaded(reportData);
                    // Now fetch from NIC — buildNmrList will only add/update delta NMRs
                    loadNmrForWork(workCode, fy);
                } catch (parseErr) {
                    console.log('[loadWorkData] datasync WorkJson parse error:', parseErr);
                    _loadWorkDataFromNic(workCode, fy);
                }
            } else {
                // DB returned empty / no match — fall through to NIC
                console.log('[loadWorkData] datasync: no data, falling back to NIC');
                _loadWorkDataFromNic(workCode, fy);
            }
        },
        error: function () {
            console.log('[loadWorkData] datasync: error, falling back to NIC');
            _loadWorkDataFromNic(workCode, fy);
        }
    });
}
// ── Step 2 (fallback): load single work detail from NIC via proxy ─
// Uses the NIC detail URL stored in DPRresponse[workCode][2]
// (loaded at page init by loadStatsAndAutocomplete → bindautocomplete)
function _loadWorkDataFromNic(workCode, finYear) {
    var nicDetailUrl = DPRresponse[workCode] && DPRresponse[workCode][2];
    if (!nicDetailUrl) {
        // DPR not yet loaded or work belongs to a different year —
        // construct the NIC work-detail URL directly from known reportData codes.
        if (reportData.district_code && reportData.block_code && reportData.panchayat_code) {
            nicDetailUrl =
                'https://nregastrep.nic.in/netnrega/DPCApprove_Workdetail.aspx' +
                '?state_code=15&state_name=KARNATAKA' +
                '&work_code=' + encodeURIComponent(workCode) +
                '&fin_year=' + encodeURIComponent(finYear || reportData.fincialYear || '2025-2026') +
                '&district_code=' + encodeURIComponent(reportData.district_code) +
                '&block_code=' + encodeURIComponent(reportData.block_code) +
                '&panchayat_code=' + encodeURIComponent(reportData.panchayat_code) +
                '&lflag=eng';
        } else {
            gpHideLoading();
            gpHideProgress();
            gpToast('Work code not found. Please verify and try again.', 'error');
            return;
        }
    }
    $.ajax({
        url: _API + 'workdetail',
        type: 'GET',
        data: { nic_url: nicDetailUrl },
        success: function (data) {
            gpHideLoading();
            gpUpdateProgress('Parsing work details…', 40);
            if (data && !data.error) {
                $.extend(reportData, {
                    workcode: workCode,
                    workName: (DPRresponse[workCode] && DPRresponse[workCode][0]) || '',
                    startdate: data.startdate || '',
                    workCategory: data.workCategory || '',
                    workYear: data.workYear || '',
                    workCostTotal: data.workCostTotal || '',
                    executionAgency: data.executingAgency || '',
                    executionLevel: data.executionLevel || '',
                    workStatus: data.workStatus || '',
                    finSanctionNo: data.finSanctionNo || '',
                    finSanctionDate: data.finSanctionDate || '',
                    techSanctionNo: data.techSanctionNo || '',
                    techSanctionDate: data.techSanctionDate || '',
                    UskilledExp: data.UskilledExp || '',
                    MaterialCost: data.MaterialCost || '',
                    SkilledCost: data.SkilledCost || '',
                    Material: data.Material || { Materials: [] }
                });
                if (typeof window.onWorkDataLoaded === 'function')
                    window.onWorkDataLoaded(reportData);
                // ── Persist to datasync DB (mirrors original bindWorkData POST) ──
                // This populates the DB so future searches for this work code
                // come from datasync instead of hitting NIC again.
                _saveWorkDataToDatasync(workCode);
                loadNmrForWork(workCode, finYear);
            } else {
                gpHideProgress();
                gpToast((data && data.error) || 'Work not found on NIC', 'error');
            }
        },
        error: function (xhr) {
            gpHideLoading();
            gpHideProgress();
            gpToast((xhr.responseJSON && xhr.responseJSON.error) || 'Work not found', 'error');
        }
    });
}
// ── Persist work data to datasync DB ─────────────────────────────────
// Mirrors original bindWorkData() POST to datasync.s3kn.com/api/workdata.
// Called after a successful NIC fetch so future searches hit datasync first.
// Fire-and-forget — no UI dependency on the response.
function _saveWorkDataToDatasync(workCode) {
    if (!workCode || !reportData.district_code || !reportData.panchayat_code) return;
    // Build the same payload shape as original bindWorkData()
    var payload = JSON.stringify(reportData).replace(/"/g, "'");
    $.ajax({
        type: 'POST',
        url: 'https://datasync.s3kn.com/api/workdata' +
            '?distcode=' + encodeURIComponent(reportData.district_code) +
            '&gpcode=' + encodeURIComponent(reportData.panchayat_code) +
            '&workcode=' + encodeURIComponent(workCode.toUpperCase()),
        contentType: 'application/json',
        data: '"' + payload + '"',
        error: function () { /* silent — best-effort save */ }
    });
}
// ── NMR loading ────────────────────────────────────────────────────
function loadNmrForWork(workCode, finYear) {
    gpUpdateProgress('Loading NMR list…', 60);
    $.ajax({
        url: _API + 'getnmrdata',
        type: 'GET',
        data: {
            work_code: workCode,
            fin_year: finYear,
            district_code: reportData.district_code || '',
            district_name: reportData.districtName || ''
        },
        success: function (data) {
            gpUpdateProgress('Rendering NMRs…', 85);
            // Store the asset URL so subsequent loads can skip the ViewState crawl
            if (data && data.assetUrl) {
                reportData.AssetLink = data.assetUrl;
            }
            buildNmrList(data);
            gpHideProgress();
        },
        error: function () {
            gpHideProgress();
            $('#ulNmrList').html('<li class="nmr-empty"><i class="bi bi-exclamation-circle"></i>Failed to load NMR list</li>');
        }
    });
}
function buildNmrList(data) {
    var list = $('#ulNmrList');
    if (!data || !data.nmrList || data.nmrList.length === 0) {
        // Only show empty state if nothing was pre-loaded from DB
        if (list.find('.nmr-item').length === 0) {
            list.html('<li class="nmr-empty"><i class="bi bi-inbox"></i>No NMR records found for this work</li>');
        }
        return;
    }
    // ── Snapshot the NMR numbers already in memory BEFORE this NIC load ──
    // These are NMRs pre-loaded from datasync DB (if any).
    // checkdelta will use this snapshot so it can find truly new issued NMRs on NIC.
    reportData.NMRS = reportData.NMRS || [];
    var preLoadedNos = reportData.NMRS.map(function (e) { return e.NMRNO; });
    // Remove loading placeholder but leave any pre-rendered DB items in place
    list.find('.nmr-empty').remove();
    $.each(data.nmrList, function (i, nmr) {
        // Server returns camelCase; normalise to ALLCAPS keys so WASM bridge stays happy
        var nmrObj = {
            NMRNO: nmr.nmrNo || nmr.NMRNO || '',
            DateFrom: nmr.fromDate || nmr.DateFrom || '',
            DateTo: nmr.toDate || nmr.DateTo || '',
            url: nmr.nmrLink || nmr.url || '',
            status: nmr.status || ''
        };
        if (!nmrObj.NMRNO) return;
        var alreadyLoaded = preLoadedNos.indexOf(nmrObj.NMRNO) !== -1;
        if (!alreadyLoaded) {
            // ── New NMR from NIC not yet in our DB snapshot ──
            // Render immediately (without dates), then hydrate via Musternew.aspx
            // to get DateFrom, DateTo and JC — mirrors original parseWorkNmrs + getform8data flow.
            reportData.NMRS.push(nmrObj);
            _renderNmrItem(list, nmrObj, false);
            if (nmrObj.url) {
                (function (obj) {
                    $.ajax({
                        url: _API + 'getform8data',
                        type: 'GET',
                        data: { nmr_link: obj.url },
                        timeout: 900000,
                        success: function (resp) {
                            _hydrateNmrFromHtml(resp.html || '', obj.url, function (hydrated) {
                                if (!hydrated || !hydrated.NMRNO) return;
                                // Update the in-memory object with real dates + JC
                                var idx = reportData.NMRS.findIndex(function (x) { return x.NMRNO === obj.NMRNO; });
                                if (idx !== -1) $.extend(reportData.NMRS[idx], hydrated);
                                // Inject date span into the already-rendered li
                                var _dateRx = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
                                if (hydrated.DateFrom && _dateRx.test(hydrated.DateFrom.trim())) {
                                    var li = list.find('[data-nmrno="' + obj.NMRNO + '"]');
                                    li.find('.nmr-period').remove();
                                    li.find('.nmr-meta span:first').after(
                                        $('<span class="nmr-period">' + hydrated.DateFrom + ' — ' + hydrated.DateTo + '</span>')
                                    );
                                }
                                // Save fully hydrated NMR to datasync
                                if (reportData.workcode) {
                                    $.ajax({
                                        type: 'POST',
                                        url: 'https://datasync.s3kn.com/api/msrdata' +
                                            '?distcode=' + reportData.district_code +
                                            '&msrno=' + hydrated.NMRNO +
                                            '&workcode=' + encodeURIComponent((reportData.workcode || '').toUpperCase()),
                                        contentType: 'application/json',
                                        data: '"' + JSON.stringify(hydrated).replace(/"/g, "'") + '"',
                                        error: function () { }
                                    });
                                }
                            });
                        },
                        error: function () { /* hydration is best-effort */ }
                    });
                })(nmrObj);
            }
        } else {
            // ── Already in DB snapshot: refresh the NIC URL in datasync ──
            // Mirrors original parseWorkNmrs msrupdate call for known NMRs
            if (nmrObj.url && reportData.workcode) {
                $.ajax({
                    type: 'POST',
                    url: 'https://datasync.s3kn.com/api/msrupdate' +
                        '?distcode=' + reportData.district_code +
                        '&msrno=' + nmrObj.NMRNO +
                        '&workcode=' + encodeURIComponent((reportData.workcode || '').toUpperCase()),
                    contentType: 'application/json',
                    data: '"' + JSON.stringify(nmrObj).replace(/"/g, "'") + '"',
                    error: function () { }
                });
            }
        }
    });
    // ── "Issued delta": check NIC for NMRs newly issued since our DB snapshot ──
    // Pass the PRE-NIC snapshot so the proxy can return any NMRs on NIC not yet in our DB list.
    // (mirrors original checkdelta(lmr) call at end of parseWorkNmrs)
    if (reportData.workcode) {
        var snapshotNos = preLoadedNos.join(',');
        checkdelta(snapshotNos);
    }
}
// ── Render a single NMR list item ─────────────────────────────────────
function _renderNmrItem(list, nmr, prepend) {
    var li = $('<li class="nmr-item">').attr('data-nmrno', nmr.NMRNO);
    var meta = $('<div class="nmr-meta">');
    meta.append('<span>NMR #' + nmr.NMRNO + '</span>');
    // Only show date range if DateFrom looks like an actual date (d/M/yyyy or dd/MM/yyyy).
    // Guards against stale datasync entries that accidentally stored the raw msrno string
    // (e.g. "874(3672),906(3672),...") in the DateFrom field.
    var _dateRx = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
    if (nmr.DateFrom && _dateRx.test((nmr.DateFrom || '').trim()))
        meta.append('<span class="nmr-period">' + nmr.DateFrom + ' — ' + nmr.DateTo + '</span>');
    if (nmr.status) {
        var isOpen = nmr.status.toLowerCase().indexOf('open') >= 0;
        meta.append('<span class="status-badge ' + (isOpen ? 'status-active' : 'status-closed') + '">' +
            '<i class="bi bi-circle-fill" style="font-size:.5rem"></i> ' + nmr.status + '</span>');
    }
    var actions = $('<div class="nmr-actions">');
    var dlIcon = $('<button class="nmr-dl-icon btn btn-link p-0" title="Download filled NMR">')
        .html('<i class="bi bi-download"></i>')
        .on('click', (function (n) { return function () { nmrBtnClicked(this, n, 'filledNmr'); }; })(nmr));
    actions.append(dlIcon);
    var btns = [
        { name: 'Form 6', key: 'Form6' },
        { name: 'Form 8', key: 'Form8' },
        { name: 'Form 8+', key: 'Form8+' },
        { name: 'Form 9', key: 'Form9' },
        { name: 'Blank NMR', key: 'blknmr' },
        { name: 'Wage List', key: 'WageList' },
        { name: 'FTO', key: 'FTO' }
    ];
    $.each(btns, function (_, btn) {
        var b = $('<button class="nmr-btn">' + btn.name + '</button>')
            .on('click', (function (n, k) { return function () { nmrBtnClicked(this, n, k); }; })(nmr, btn.key));
        actions.append(b);
    });
    li.append(meta).append(actions);
    if (prepend) list.prepend(li); else list.append(li);
}
// ── Delta check — find newly ISSUED NMRs on NIC not yet in our list ───
// v2 equivalent of original checkdelta() — mirrors deltacheck.aspx.cs crawl via DeltaCheckController
// Finds NMRs that are ISSUED on NIC but not yet in our datasync DB snapshot.
// loadedNmrNos: comma-separated string of already-known NMR numbers (sent as POST body)
function checkdelta(loadedNmrNos) {
    if (!reportData.workcode) return;
    // Build query params matching DeltaCheckController signature
    // (mirrors original: state_code, dist_code, block_code, panch_code, names, fin_year, work_code)
    var qs = 'work_code=' + encodeURIComponent(reportData.workcode) +
        '&fin_year=' + encodeURIComponent(reportData.fincialYear || '2025-2026') +
        '&dist_code=' + encodeURIComponent(reportData.district_code || '') +
        '&block_code=' + encodeURIComponent(reportData.block_code || '') +
        '&panch_code=' + encodeURIComponent(reportData.panchayat_code || '') +
        '&dist_name=' + encodeURIComponent(reportData.districtName || '') +
        '&block_name=' + encodeURIComponent(reportData.blockName || '') +
        '&panch_name=' + encodeURIComponent(reportData.panchayatName || '') +
        '&state_code=15&state_name=KARNATAKA';
    $.ajax({
        url: _API + 'deltacheck?' + qs,
        type: 'POST',
        contentType: 'text/plain',
        data: loadedNmrNos,
        success: function (resp) {
            if (!resp || !resp.newNmrs || resp.newNmrs.length === 0) return;
            var list = $('#ulNmrList');
            list.find('.nmr-empty').remove();
            $.each(resp.newNmrs, function (_, n) {
                var nmrObj = {
                    NMRNO: n.nmrNo || '',
                    DateFrom: n.fromDate || '',
                    DateTo: n.toDate || '',
                    url: n.nmrLink || '',
                    status: n.status || 'Issued',
                    JC: n.jc || []   // full JC data from server parse
                };
                if (reportData.NMRS.some(function (e) { return e.NMRNO === nmrObj.NMRNO; })) return;
                reportData.NMRS.unshift(nmrObj);
                _renderNmrItem(list, nmrObj, true);
                // Persist to datasync (fire-and-forget, mirrors original checkdelta POST)
                $.ajax({
                    type: 'POST',
                    url: 'https://datasync.s3kn.com/api/msrdata?distcode=' +
                        reportData.district_code + '&msrno=' + nmrObj.NMRNO +
                        '&workcode=' + encodeURIComponent((reportData.workcode || '').toUpperCase()),
                    contentType: 'application/json',
                    data: '"' + JSON.stringify(nmrObj).replace(/"/g, "'") + '"',
                    error: function () { }
                });
            });
        },
        error: function () { /* silent — delta check is best-effort */ }
    });
}
// ── NMR button handler ─────────────────────────────────────────────
function nmrBtnClicked(btn, nmr, action) {
    window.onDownloadStart && window.onDownloadStart('Generating ' + action + '…');
    gpShowLoading('Fetching data…');
    var handlers = {
        'Form6': function () { loadAndGenerateForm6(nmr); },
        'Form8': function () { loadAndGenerateForm8(nmr); },
        'Form8+': function () { loadAndGenerateForm8Plus(nmr); },
        'Form9': function () { loadAndGenerateForm9(nmr); },
        'blknmr': function () { loadAndGenerateBlankNmr(nmr); },
        'WageList': function () { loadAndGenerateWageList(nmr); },
        'FTO': function () { loadAndGenerateFto(nmr); },
        'filledNmr': function () { loadAndGenerateFilledNmr(nmr); }
    };
    var handler = handlers[action];
    if (handler) {
        handler();
    } else {
        gpHideLoading();
        gpToast('Unknown action: ' + action, 'warning');
    }
}
// ── Form generators — use wasmGeneratePdf so WASM handles watermark ─
function loadAndGenerateForm6(nmr) {
    $.ajax({
        url: _API + 'getnmrdata',
        type: 'GET',
        // Use reportData for work_code and fin_year; nmr.NMRNO for the NMR number
        data: { work_code: reportData.workcode, fin_year: reportData.fincialYear, nmr_no: nmr.NMRNO },
        success: function (data) {
            gpHideLoading();
            window.wasmGeneratePdf({ type: 'form6', data: $.extend({}, reportData, data, { nmr: nmr }) });
        },
        error: function () {
            gpHideLoading();
            gpToast('Failed to load Form 6 data', 'error');
        }
    });
}
function loadAndGenerateForm8(nmr) {
    $.ajax({
        url: _API + 'getform8data',
        type: 'GET',
        data: { nmr_link: nmr.url },   // nmr.url is the NIC muster roll URL
        success: function (data) {
            gpHideLoading();
            window.wasmGeneratePdf({ type: 'form8', data: $.extend({}, reportData, data, { nmr: nmr }) });
        },
        error: function () { gpHideLoading(); gpToast('Failed to load Form 8 data', 'error'); }
    });
}
function loadAndGenerateForm8Plus(nmr) {
    $.ajax({
        url: _API + 'getform8data',
        type: 'GET',
        data: { nmr_link: nmr.url },   // nmr.url is the NIC muster roll URL
        success: function (data) {
            gpHideLoading();
            window.wasmGeneratePdf({ type: 'form8plus', data: $.extend({}, reportData, data, { nmr: nmr }) });
        },
        error: function () { gpHideLoading(); gpToast('Failed to load Form 8+ data', 'error'); }
    });
}
function loadAndGenerateForm9(nmr) {
    gpHideLoading();
    window.wasmGeneratePdf({ type: 'form9', data: $.extend({}, reportData, { nmr: nmr }) });
}
function loadAndGenerateBlankNmr(nmr) {
    $.ajax({
        url: _API + 'jobcardhh',
        type: 'GET',
        // fin_year comes from reportData, not the NMR object
        data: { panchayat_code: reportData.panchayat_code, fin_year: reportData.fincialYear },
        success: function (data) {
            gpHideLoading();
            window.wasmGeneratePdf({ type: 'nmr', data: $.extend({}, reportData, data, { nmr: nmr }) });
        },
        error: function () { gpHideLoading(); gpToast('Failed to load blank NMR data', 'error'); }
    });
}
function loadAndGenerateWageList(nmr) {
    // Delegate to mr.js loadWageListData which handles the full multi-step NIC fetch
    var el = document.createElement('button');
    el.name = 'WageList';
    el.dataset.link = nmr.url || '';
    el.dataset.nmrno = nmr.NMRNO;
    if (typeof loadWageListData === 'function') {
        loadWageListData(el);
    } else {
        gpHideLoading();
        gpToast('Wage list generator not loaded', 'error');
    }
}
function loadAndGenerateFto(nmr) {
    // Delegate to mr.js loadWageListData with elm.name='FTO' so it routes to loadFTOData
    var el = document.createElement('button');
    el.name = 'FTO';
    el.dataset.link = nmr.url || '';
    el.dataset.nmrno = nmr.NMRNO;
    if (typeof loadWageListData === 'function') {
        loadWageListData(el);
    } else {
        gpHideLoading();
        gpToast('FTO generator not loaded', 'error');
    }
}
function loadAndGenerateFilledNmr(nmr) {
    var link = nmr.url || nmr.nmrLink || '';
    if (link) window.open(link, '_blank');
    else gpToast('No filled NMR link available', 'warning');
    gpHideLoading();
}
// ── Tab-level document download ────────────────────────────────────────
// Mirrors original downloadPage() for the work-level buttons:
//   CoverPage, WorkOrder, completion → call JS generator from Formtemplates/
//   checklist                        → generatechecklist()
//   musterrolemov                    → generateMusterRollMoventSlip()
//   geotag                           → fetch geotag data, then generategeotag(resp)
function downloadPage(name) {
    gpShowLoading('Generating ' + name + '…');
    switch (name) {
        case 'CoverPage':
            gpHideLoading();
            if (typeof generatecoverpage === 'function') generatecoverpage();
            else gpToast('Cover page generator not loaded', 'error');
            break;
        case 'WorkOrder':
            gpHideLoading();
            if (typeof generateworkorder === 'function') generateworkorder();
            else gpToast('Work order generator not loaded', 'error');
            break;
        case 'completion':
            gpHideLoading();
            if (typeof generatecompletion === 'function') generatecompletion();
            else gpToast('Work completion generator not loaded', 'error');
            break;
        case 'checklist':
            gpHideLoading();
            if (typeof generatechecklist === 'function') generatechecklist();
            else gpToast('Checklist generator not loaded', 'error');
            break;
        case 'musterrolemov':
            gpHideLoading();
            if (typeof generateMusterRollMoventSlip === 'function') generateMusterRollMoventSlip();
            else gpToast('Muster roll movement generator not loaded', 'error');
            break;
        case 'geotag':
            loadAndGenerateGeotag();
            break;
        default:
            gpHideLoading();
            gpToast('Unknown document type: ' + name, 'warning');
    }
}
// ── GeoTag — fetch data from NIC then generate PDF ─────────────────────
// Mirrors original: GET /templates/geotag?workcode=...  → generategeotag(resp)
function loadAndGenerateGeotag() {
    var url = _API + 'geotag?work_code=' + encodeURIComponent(reportData.workcode || '') +
        '&district_code=' + encodeURIComponent(reportData.district_code || '') +
        '&block_code=' + encodeURIComponent(reportData.block_code || '') +
        '&panchayat_code=' + encodeURIComponent(reportData.panchayat_code || '') +
        '&block_name=' + encodeURIComponent(reportData.blockName || '') +
        '&panchayat_name=' + encodeURIComponent(reportData.panchayatName || '') +
        '&district_name=' + encodeURIComponent(reportData.districtName || '') +
        '&fin_year=' + encodeURIComponent(reportData.fincialYear || '2025-2026') +
        '&work_name=' + encodeURIComponent(reportData.workName || '');
    $.ajax({
        url: url,
        type: 'GET',
        success: function (resp) {
            gpHideLoading();
            if (typeof generategeotag === 'function') generategeotag(resp);
            else gpToast('GeoTag generator not loaded', 'error');
        },
        error: function () {
            gpHideLoading();
            gpToast('Failed to load GeoTag data', 'error');
        }
    });
}
// ════════════════════════════════════════════════════════════════════════════
// Page-level logic — reads #gp-home-config JSON data island (set by Home.cshtml)
// Mirrors the pattern from original gpmnrega2 gphome.js + Home.aspx
// ════════════════════════════════════════════════════════════════════════════
// ── Search: load a single work by work code ───────────────────────────────
// Mirrors original searchTextCode() validation flow:
//   1. Validate work code is non-empty
//   2. Validate that the GP segment of the entered code matches the logged-in GP
//   3. Hide the overview card, clear the NMR list, then call loadWorkData
function searchWork() {
    var code = $('#txtSearchWork').val().trim();
    var year = $('#ddlyear').val();
    if (!code) { gpToast('Enter a work code to search', 'warning'); return; }
    // Validate GP segment — first token before '/' must match panchayat_code
    var gpSegment = findgp(); // extracts first segment of the entered work code
    if (reportData.panchayat_code && gpSegment && gpSegment !== reportData.panchayat_code) {
        gpToast('Work code does not belong to your Panchayat (' + reportData.panchayat_code + ')', 'error');
        return;
    }
    // Hide previous results and reset DOM before new search (mirrors original)
    document.getElementById('workOverviewCard').style.display = 'none';
    document.getElementById('emptyState').style.display = 'none';
    $('#workFormsBar').hide();
    // Clear NMR list — leave just the placeholder so the card stays clean
    $('#ulNmrList').html('<li class="nmr-empty"><i class="bi bi-search"></i>Search for a work code above to load NMR details</li>');
    loadWorkData(code, year);
}
// ── Load all works for the logged-in GP ──────────────────────────────────
function loadFromGpCode() {
    var cfg = {};
    try { var _el = document.getElementById('gp-home-config'); if (_el) cfg = JSON.parse(_el.textContent); } catch (e) { }
    if (!cfg.gpCode) { gpToast('Panchayat code not found. Please re-login.', 'error'); return; }
    gpShowLoading('Loading works…');
    loadAllWorks(cfg.gpCode, $('#ddlyear').val());
}
// ── Work data loaded callback (invoked by loadWorkData / _loadWorkDataFromNic) ──
window.onWorkDataLoaded = function (data) {
    gpHideLoading();
    if (!data || data.error) {
        gpToast((data && data.error) || 'Failed to load work data', 'error');
        return;
    }
    // Show card, show work-forms-bar, hide empty state (mirrors original show/hide flow)
    document.getElementById('emptyState').style.display = 'none';
    document.getElementById('workOverviewCard').style.display = '';
    $('#workFormsBar').show();
    // Clear NMR list DOM — datasync items were already rendered before this callback;
    // calling it here ensures any stale items from a *previous* search are removed.
    // The pre-loaded datasync NMRs were pushed to reportData.NMRS AND rendered
    // inside loadWorkData before onWorkDataLoaded fires, so they are NOT lost.
    // We rebuild from reportData.NMRS to keep DOM in sync.
    var nmrListEl = $('#ulNmrList');
    nmrListEl.empty();
    if (reportData.NMRS && reportData.NMRS.length > 0) {
        $.each(reportData.NMRS, function (_, nmr) { _renderNmrItem(nmrListEl, nmr, false); });
    } else {
        nmrListEl.html('<li class="nmr-empty"><i class="bi bi-search"></i>Search for a work code above to load NMR details</li>');
    }
    // Clear search box after a successful work load (mirrors original behaviour:
    // search box is cleared so next Enter/click loads fresh rather than re-searching same code)
    $('#txtSearchWork').val('');
    // Helper: safe value — read from data (which is reportData), then em-dash
    function val(key) {
        var v = data[key];
        return (v && v !== '' && v !== 'undefined') ? v : '—';
    }
    // Combine Fin & Tech sanction No + Date into single display values
    var finNo = val('finSanctionNo');
    var finDt = val('finSanctionDate');
    var finNoDate = (finDt !== '—') ? finNo + ' / ' + finDt : finNo;
    var techNo = val('techSanctionNo');
    var techDt = val('techSanctionDate');
    var techNoDate = (techDt !== '—') ? techNo + ' / ' + techDt : techNo;
    var woGrid = document.getElementById('woGrid');
    woGrid.innerHTML = [
        { label: 'Work Name', value: val('workName'), icon: 'bi-card-text', cls: 'wo-full' },
        // Row 1 — 3 equal columns (each spans 2 of the 6 grid tracks)
        { label: 'Work Code', value: val('workcode'), icon: 'bi-hash', cls: 'wo-third' },
        { label: 'Work Category', value: val('workCategory'), icon: 'bi-tag', cls: 'wo-third' },
        { label: 'Execution Agency', value: val('executionAgency'), icon: 'bi-building', cls: 'wo-third' },
        // Row 2 — 2 equal columns (each spans 3 of the 6 grid tracks)
        { label: 'Fin. Sanction No & Date', value: finNoDate, icon: 'bi-file-text', cls: 'wo-half' },
        { label: 'Tech. Sanction No & Date', value: techNoDate, icon: 'bi-tools', cls: 'wo-half' }
    ].map(function (row) {
        return '<div class="wo-item ' + row.cls + '">' +
            '<div class="wo-label"><i class="bi ' + row.icon + ' me-1" style="opacity:.7"></i>' + row.label + '</div>' +
            '<div class="wo-value">' + row.value + '</div></div>';
    }).join('') +
        '<div class="wo-item wo-full" style="padding-top:8px">' +
        '<i class="bi bi-file-earmark-bar-graph me-1" style="opacity:.8"></i>' +
        '<a href="javascript:buildCmpStatement()" style="color:#fff;font-weight:600;font-size:.85rem;text-decoration:underline">' +
        'Invitation / Comparative Statement / Supply Order' +
        '</a>' +
        '</div>';
};
// ── Stats loaded callback (invoked by loadStatsAndAutocomplete) ───────────
window.onStatsLoaded = function (works) {
    if (!works) return;
    var map = {
        appWork: works.approved || 0,
        ongoingWork: works.ongoing || 0,
        completedWork: works.completed || 0
    };
    Object.keys(map).forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.textContent = map[id];
    });
};
// ── Page init ─────────────────────────────────────────────────────────────
$(document).ready(function () {
    var cfg = {};
    try {
        var cfgEl = document.getElementById('gp-home-config');
        if (cfgEl && cfgEl.textContent.trim()) {
            cfg = JSON.parse(cfgEl.textContent);
        }
    } catch (ex) {
        console.error('[GP Home] Failed to parse gp-home-config:', ex);
    }
    console.log('[GP Home] Init — gpCode:', cfg.gpCode,
        'districtCode:', cfg.districtCode,
        'blockCode:', cfg.blockCode);
    // Populate reportData from config
    if (typeof initReportData === 'function') {
        initReportData({
            panchayat_code: cfg.gpCode,
            panchayatName: cfg.gpName,
            panchayat_NameRegional: cfg.gpNameRegional,
            districtName: cfg.districtName,
            district_code: cfg.districtCode,
            districtNameRegional: cfg.districtNameRegional,
            blockName: cfg.blockName,
            block_code: cfg.blockCode,
            blockNameRegional: cfg.blockNameRegional,
            VidhanSabha: cfg.vidhanSabha,
            LokSabha: cfg.lokSabha,
            userName: cfg.userName,
            stateName: 'KARNATAKA',
            stateNameRegional: 'ಕರ್ನಾಟಕ',
            state_code: '15',
            state_shortname: 'KN',
            fincialYear: $('#ddlyear :selected').text().trim()
        });
    }
    // Bind search button and Enter key on text input
    $('#btnSearch').on('click', function (e) { e.preventDefault(); searchWork(); });
    $('#txtSearchWork').on('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); searchWork(); } });
    // Bind "All Works" button
    $('#btnAllWorks').on('click', function () { loadFromGpCode(); });
    // Sync financial year into reportData on change
    $('#ddlyear').on('change', function () {
        var fy = $('option:selected', this).text().trim();
        if (typeof reportData !== 'undefined') reportData.fincialYear = fy;
    });
    // Load stats + autocomplete when full session info is present
    if (cfg.gpCode && cfg.districtCode && cfg.blockCode) {
        if (typeof loadStatsAndAutocomplete === 'function') {
            loadStatsAndAutocomplete();
        } else {
            console.error('[GP Home] loadStatsAndAutocomplete not found — check gphome.js loaded');
        }
    } else {
        var missingHtml = '<span style="color:#ef4444;font-size:.82rem">' +
            '<i class="bi bi-exclamation-circle me-1"></i>' +
            'Session expired. <a href="/Auth/Logout">Log in again</a>.</span>';
        ['appWork', 'totaljobcard', 'totalPersondays', 'totalpersoncard',
            'disabledjobcard', 'issuedNmr'].forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.innerHTML = missingHtml;
            });
    }
    // Restore last searched work code for this GP
    var lastCode = localStorage.getItem('lastWorkCode_' + cfg.gpCode);
    if (lastCode) $('#txtSearchWork').val(lastCode);
});
/*
 * ========================= MIGRATION FIXES =========================
 * Added to preserve original gphome.js behavior for:
 *  - checkNmrwithAsset
 *  - loadNMR
 *  - parseWorkNmrs
 *  - bindNMRs
 *  - checkdelta (JC hydration for delta NMRs)
 *  - PMIAY / IAY handling
 *  - legacy form generators expecting elm.dataset.*
 *
 * These wrappers keep the new /api/proxy endpoints, but restore the
 * original client-side data contract so existing form scripts keep working.
 */
function bindNMRs() {
    var list = $('#ulNmrList');
    list.empty();
    if (!reportData.NMRS || reportData.NMRS.length === 0) {
        list.html('<li class="nmr-empty"><i class="bi bi-inbox"></i>No NMR records found for this work</li>');
        return;
    }
    $.each(reportData.NMRS, function (_, nmr) {
        _renderNmrItem(list, nmr, false);
    });
}
function checkNmrwithAsset(BetaData, wd) {
    if (wd && wd.MsrData && wd.MsrData.length !== 0) {
        reportData.NMRS = [];
        wd.MsrData.forEach(function (e) {
            try {
                var nmrdata = JSON.parse(String(e.MsrJson || '').replaceAll("'", '"').replace(/[\n\r\t]/g, ''));
                reportData.NMRS.push({
                    NMRNO: nmrdata.NMRNO || '',
                    DateFrom: nmrdata.DateFrom || '',
                    DateTo: nmrdata.DateTo || '',
                    url: (nmrdata.url || '').replace('mnregaweb4.nic.in', 'nregastrep.nic.in'),
                    JC: nmrdata.JC || []
                });
            } catch (ex) { }
        });
        bindNMRs();
    }
    var beta = BetaData || {};
    beta.AssetLink = reportData.AssetLink || '';
    loadNMR(beta);
    bindNMRs();
}
function PMIAY(res, data) {
    reportData.MaterialCost = 0;
    reportData.SkilledCost = 0;
    reportData.executionAgency = 'Gram Panchayat';
    reportData.executionLevel = 'GP';
    var textfonts = $(res).find('font');
    for (var i = 10; i < textfonts.length; i++) {
        var txt = (textfonts[i].innerText || $(textfonts[i]).text()).trim();
        if (txt.toLowerCase() === 'work name')
            reportData.workName = (textfonts[i + 2] ? (textfonts[i + 2].innerText || $(textfonts[i + 2]).text()).trim() : '');
        if (txt.toLowerCase() === 'work start date') {
            reportData.startdate = (textfonts[i + 1] ? (textfonts[i + 1].innerText || $(textfonts[i + 1]).text()).trim() : '');
            reportData.workYear = (reportData.startdate.split('/')[2] || '').trim();
        }
        if (txt.toLowerCase() === 'work purpose status')
            reportData.workCategory = (textfonts[i + 1] ? (textfonts[i + 1].innerText || $(textfonts[i + 1]).text()).trim() : '');
        if (txt === 'Estimated Cost (In Lakhs)')
            reportData.workCostTotal = parseFloat((textfonts[i + 1] ? (textfonts[i + 1].innerText || $(textfonts[i + 1]).text()).trim() : '0')) * 100000;
        if (txt === 'Work Status')
            reportData.workStatus = (textfonts[i + 1] ? (textfonts[i + 1].innerText || $(textfonts[i + 1]).text()).trim() : '');
        if (txt === 'Sanction No. and Sanction Date') {
            reportData.finSanctionNo = reportData.techSanctionNo = (textfonts[i + 1] ? (textfonts[i + 1].innerText || $(textfonts[i + 1]).text()).trim() : '');
            reportData.finSanctionDate = reportData.techSanctionDate = (textfonts[i + 2] ? (textfonts[i + 2].innerText || $(textfonts[i + 2]).text()).trim() : '');
        }
        if (txt === 'Unskilled' && i < 50)
            reportData.UskilledExp = (textfonts[i + 6] ? (textfonts[i + 6].innerText || $(textfonts[i + 6]).text()).trim() : '');
    }
    reportData.workcode = typeof data === 'string' ? data : reportData.workcode;
}
function loadNMR(BetaData) {
    var assetLink = BetaData && BetaData.AssetLink ? BetaData.AssetLink : '';
    // IAY / PMIAY path uses citizenworkasset and local HTML parsing like original.
    // Only /IAY/ codes take this path — NOT regular /GPMNREGA works (every work code
    // contains /GPMNREGA so that check would incorrectly route all works here).
    if (reportData.workcode && reportData.workcode.indexOf('/IAY/') > 0) {
        _loadIAYWorkData(reportData.workcode, reportData.fincialYear || '2025-2026');
        return;
    }
    // If no asset link, fall back to new proxy NMR list endpoint.
    if (!assetLink || assetLink === 'undefined') {
        loadNmrForWork(reportData.workcode, reportData.fincialYear || '2025-2026');
        return;
    }
    // AssetLink exists → keep original-style behavior and parse returned asset HTML.
    $.ajax({
        url: _API + 'wagelistdata',
        type: 'POST',
        dataType: 'html',
        data: assetLink,
        timeout: 900000,
        success: function (response) {
            parseWorkNmrs(response);
        },
        error: function (error) {
            console.log('loadNMR assetLink error', error);
            loadNmrForWork(reportData.workcode, reportData.fincialYear || '2025-2026');
        }
    });
}
function _parseApplicantNameFromCellHtml(cellHtml) {
    try {
        var name = String(cellHtml || '').split('<br>')[0];
        var prefix = '<font face="Verdana" color="#284775" size="2">';
        var applicantname = name.indexOf(prefix) === 0 ? name.substr(prefix.length) : $(name).text();
        applicantname = applicantname.indexOf('(') !== -1 ? applicantname.substring(0, applicantname.indexOf('(')).trim() : applicantname.trim();
        applicantname = applicantname.replace('\\', '');
        return applicantname;
    } catch (e) {
        return '';
    }
}
function _hydrateNmrFromHtml(respnse, nmrUrl, onDone) {
    try {
        var form = $(respnse).filter(function (i, e) { return e.id === 'form1'; });
        if (form.length === 0)
            form = $(respnse).filter(function (i, e) { return e.id === 'aspnetForm'; });
        if (!form.length || !form[0]) {
            onDone && onDone(null);
            return;
        }
        var startDate = form[0].querySelectorAll('#ctl00_ContentPlaceHolder1_lbldatefrom')[0]?.innerText || '';
        var endDate = form[0].querySelectorAll('#ctl00_ContentPlaceHolder1_lbldateto')[0]?.innerText || '';
        var msrno = form[0].querySelectorAll('#ctl00_ContentPlaceHolder1_lblMsrNo2')[0]?.innerText || '';
        var JC = [];
        var table = form[0].querySelectorAll('#ctl00_ContentPlaceHolder1_grdShowRecords tr');
        if (!table || table.length < 2) {
            onDone && onDone({ NMRNO: msrno, DateFrom: startDate, DateTo: endDate, url: nmrUrl, JC: [] });
            return;
        }
        var wageListId = table[0].cells.length - 5;
        var bankNameId = table[0].cells.length - 8;
        var dateCreatedId = table[0].cells.length - 3;
        for (var j = 1; j < table.length - 1; j++) {
            JC.push({
                JCNO: table[j].cells[1].querySelectorAll('a')[0]?.innerText.trim() || '',
                appName: _parseApplicantNameFromCellHtml(table[j].cells[1].innerHTML),
                appAddress: table[j].cells[3]?.textContent.trim() || '',
                category: (table[j].cells[2]?.innerText || '').length > 3 ? (table[j].cells[2].innerText || '').substr(0, 3) : (table[j].cells[2]?.innerText || ''),
                wageList: table[j].cells[wageListId]?.textContent.trim() || '',
                BankName: table[j].cells[bankNameId]?.textContent.trim() || '',
                DateCreated: table[j].cells[dateCreatedId]?.textContent.trim() || ''
            });
        }
        onDone && onDone({
            NMRNO: msrno,
            DateFrom: startDate,
            DateTo: endDate,
            url: nmrUrl,
            JC: JC
        });
    } catch (ex) {
        console.log('_hydrateNmrFromHtml parse error', ex);
        onDone && onDone(null);
    }
}
function _saveNmrToDatasync(nmrObj) {
    if (!nmrObj || !nmrObj.NMRNO || !reportData.workcode) return;
    $.ajax({
        type: 'POST',
        url: 'https://datasync.s3kn.com/api/msrdata' +
            '?distcode=' + reportData.district_code +
            '&msrno=' + nmrObj.NMRNO +
            '&workcode=' + encodeURIComponent((reportData.workcode || '').toUpperCase()),
        contentType: 'application/json',
        data: '"' + JSON.stringify(nmrObj).replace(/"/g, "'") + '"',
        error: function () { }
    });
}
function _updateNmrUrlInDatasync(nmrObj) {
    if (!nmrObj || !nmrObj.NMRNO || !reportData.workcode) return;
    $.ajax({
        type: 'POST',
        url: 'https://datasync.s3kn.com/api/msrupdate' +
            '?distcode=' + reportData.district_code +
            '&msrno=' + nmrObj.NMRNO +
            '&workcode=' + encodeURIComponent((reportData.workcode || '').toUpperCase()),
        contentType: 'application/json',
        data: '"' + JSON.stringify(nmrObj).replace(/"/g, "'") + '"',
        error: function () { }
    });
}
function parseWorkNmrs(response) {
    try {
        var loadedNmrs = [];
        for (var k = 0; k < reportData.NMRS.length; k++)
            loadedNmrs.push(reportData.NMRS[k].NMRNO);
        var newLinks = [];
        $(response).find('a').each(function (i, a) {
            try {
                var href = a.href || $(a).attr('href') || '';
                var urlParams = new URLSearchParams(href.indexOf('?') >= 0 ? href.substring(href.indexOf('?')) : '');
                var msrno = urlParams.get('msrno');
                if (msrno === null || typeof msrno === 'undefined') return;
                msrno = msrno.trim();
                var existingIndex = loadedNmrs.indexOf(msrno);
                var nmrUrl = 'https://mnregaweb4.nic.in/netnrega/citizen_html/Musternew.aspx' + (href.indexOf('?') >= 0 ? href.substring(href.indexOf('?')) : '');
                if (existingIndex !== -1) {
                    if (reportData.NMRS[existingIndex]) {
                        reportData.NMRS[existingIndex].url = nmrUrl;
                        _updateNmrUrlInDatasync(reportData.NMRS[existingIndex]);
                    }
                } else {
                    loadedNmrs.push(msrno);
                    newLinks.push({ msrno: msrno, url: nmrUrl });
                }
            } catch (ex) { }
        });
        if (newLinks.length === 0) {
            console.log('No Delta filled');
        } else {
            newLinks.forEach(function (lnk) {
                // getform8data fetches the Musternew.aspx page and returns { html }
                $.ajax({
                    url: _API + 'getform8data',
                    type: 'GET',
                    data: { nmr_link: lnk.url },
                    timeout: 900000,
                    success: function (respnse) {
                        // respnse is auto-parsed JSON: { html: '...' }
                        _hydrateNmrFromHtml(respnse.html || '', lnk.url, function (nmrObj) {
                            if (!nmrObj || !nmrObj.NMRNO) return;
                            reportData.NMRS.push(nmrObj);
                            bindNMRs();
                            _saveNmrToDatasync(nmrObj);
                        });
                    },
                    error: function (error) {
                        console.log('parseWorkNmrs getnmrdata error', error);
                    }
                });
            });
        }
        var lmr = loadedNmrs.join(',');
        checkdelta(lmr);
    } catch (e) {
        console.log(String(e));
    }
}
function _makeLegacyElm(nmr, action) {
    var el = document.createElement('button');
    el.name = action;
    el.dataset.link = nmr.url || '';
    el.dataset.nmrno = nmr.NMRNO || '';
    el.dataset.index = String(reportData.NMRS.findIndex(function (x) { return x.NMRNO === nmr.NMRNO; }));
    return el;
}
function _ensureNmrHydrated(nmr, done) {
    if (nmr && Array.isArray(nmr.JC) && nmr.JC.length > 0) {
        done && done(nmr);
        return;
    }
    if (!nmr || !nmr.url) {
        done && done(nmr);
        return;
    }
    $.ajax({
        url: _API + 'getform8data',
        type: 'GET',
        data: { nmr_link: nmr.url },
        success: function (html) {
            // getform8data returns { html: '...' } (auto-parsed JSON)
            _hydrateNmrFromHtml(html.html || '', nmr.url, function (fullNmr) {
                if (fullNmr && fullNmr.NMRNO) {
                    var idx = reportData.NMRS.findIndex(function (x) { return x.NMRNO === fullNmr.NMRNO; });
                    if (idx !== -1) reportData.NMRS[idx] = $.extend({}, reportData.NMRS[idx], fullNmr);
                    nmr = idx !== -1 ? reportData.NMRS[idx] : fullNmr;
                    _saveNmrToDatasync(nmr);
                }
                done && done(nmr);
            });
        },
        error: function () { done && done(nmr); }
    });
}
// Override action handlers to prefer legacy generators if present.
loadAndGenerateForm6 = function (nmr) {
    _ensureNmrHydrated(nmr, function (hydrated) {
        gpHideLoading();
        if (typeof generateform6 === 'function') {
            generateform6(_makeLegacyElm(hydrated, 'Form6'));
        } else {
            window.wasmGeneratePdf({ type: 'form6', data: { nmr: hydrated } });
        }
    });
};
loadAndGenerateForm8Plus = function (nmr) {
    _ensureNmrHydrated(nmr, function (hydrated) {
        gpHideLoading();
        if (typeof generate8plus === 'function') {
            generate8plus(_makeLegacyElm(hydrated, 'Form8+'));
        } else {
            window.wasmGeneratePdf({ type: 'form8plus', data: { nmr: hydrated } });
        }
    });
};
loadAndGenerateForm9 = function (nmr) {
    _ensureNmrHydrated(nmr, function (hydrated) {
        gpHideLoading();
        if (typeof generateform9 === 'function') {
            generateform9(_makeLegacyElm(hydrated, 'Form9'));
        } else {
            window.wasmGeneratePdf({ type: 'form9', data: { nmr: hydrated } });
        }
    });
};
loadAndGenerateBlankNmr = function (nmr) {
    var person = window.prompt('Please enter Name of technical staff responsible for measurement:', ' ');
    if (person == null || String(person).trim() === '') {
        gpHideLoading();
        gpToast('Name of technical staff responsible for measurement is required.', 'warning');
        return;
    }
    reportData.technicalstaff = String(person).trim().toUpperCase();
    _ensureNmrHydrated(nmr, function (hydrated) {
        var jcList = hydrated.JC || [];
        if (!jcList.length) {
            gpHideLoading();
            gpToast('No job card data found for this NMR', 'warning');
            return;
        }
        // Compute working days from DateFrom/DateTo (format: D/M/YYYY)
        var startDate = hydrated.DateFrom || '';
        var endDate = hydrated.DateTo || '';
        var wkdays = 1;
        try {
            var sd = startDate.split('/'), ed = endDate.split('/');
            var d1 = new Date(parseInt(sd[2]), parseInt(sd[1]) - 1, parseInt(sd[0]));
            var d2 = new Date(parseInt(ed[2]), parseInt(ed[1]) - 1, parseInt(ed[0]));
            wkdays = Math.round(Math.abs(d2 - d1) / 86400000) + 1;
        } catch (e) { }

        // Build row array matching buildBlankNMR expectations
        var tablerows = jcList.map(function (jc, i) {
            return {
                SiNo: i + 1,
                JobCardNo: jc.JCNO || '',
                ApplicantName: jc.appName || '',
                hh: jc.appName || '',   // head-of-household fallback to applicant name
                Place: jc.appAddress || '',
                bkname: jc.BankName || '',
                bkacc: 'XXXXXXXXXXXX',
                category: jc.category || ''
            };
        });

        // Build meta object matching buildBlankNMR's jcdata expectations
        var meta = {
            msrno: hydrated.NMRNO,
            startDate: startDate,
            endDate: endDate,
            wkdays: wkdays,
            finSanNo: reportData.finSanctionNo || '',
            finSanDate: reportData.finSanctionDate || '',
            techSanNo: reportData.techSanctionNo || '',
            techSanDate: reportData.techSanctionDate || '',
            panchayat: reportData.panchayatName || '',
            distName: reportData.districtName || '',
            state: reportData.stateName || '',
            block: reportData.blockName || '',
            workname: reportData.workName || '',
            workcode: reportData.workcode || '',
            agency: reportData.executionAgency || '',
            techStaff: reportData.technicalstaff || ''
        };

        gpHideLoading();
        window.wasmGeneratePdf({ type: 'nmr', data: { accdata: tablerows, jcdata: meta } });
    });
};
loadAndGenerateFilledNmr = function (nmr) {
    var link = nmr.url || nmr.nmrLink || '';
    if (!link) {
        gpHideLoading();
        gpToast('No filled NMR link available', 'warning');
        return;
    }
    gpHideLoading();
    if (typeof printFilledNMR === 'function') {
        printFilledNMR(_makeLegacyElm(nmr, 'filledNmr'));
    } else {
        window.open(link, '_blank');
    }
};
// ── PDF generation helper ─────────────────────────────────────────────────
// All Formtemplates (form6, form8, form8plus, form9, MusterRollMovement, etc.)
// call generateNewPdf(). It was previously only defined in agencyhome.js /
// codef.js, which are NOT loaded on the GP Home page. Defining it here ensures
// every template on Home.cshtml can generate PDFs without a ReferenceError.
// ── downloadForms ─────────────────────────────────────────────────────────────
// Mirrors original downloadForms(html, form, wage, fto) which POSTed HTML to
// /templates/converter (EvoPDF).  In v2 it POSTs to /api/proxy/converter which
// uses SelectPdf (same result, no EvoPDF dependency).
//
// Usage:
//   downloadForms(html, 'WageList', wageListNo, '')
//   downloadForms(html, 'FTO', '', ftoNo)
//   downloadForms(html, 'filledNmr')
//   downloadForms(html, 'GeoTag')
function downloadForms(html, form, wage, fto) {
    var wc = reportData.workcode || '';
    var sn = reportData.stateName || '';
    var wl = wage || '';
    var fn = fto || '';
    var url = '/api/proxy/converter?page=' + encodeURIComponent(form) +
        '&workcode=' + encodeURIComponent(wc) +
        '&stateName=' + encodeURIComponent(sn) +
        '&WageList=' + encodeURIComponent(wl) +
        '&FTO=' + encodeURIComponent(fn);
    $.ajax({
        url: url,
        type: 'POST',
        data: html,
        contentType: 'text/html; charset=utf-8',
        xhrFields: { responseType: 'blob' },
        success: function (blob, status, xhr) {
            try {
                var filename = form + '_' + Date.now() + '.pdf';
                var disp = xhr.getResponseHeader('Content-Disposition');
                if (disp) {
                    var m = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disp);
                    if (m && m[1]) filename = decodeURIComponent(m[1].replace(/['"]/g, ''));
                }
                var a = document.createElement('a');
                var bUrl = window.URL.createObjectURL(blob);
                a.href = bUrl;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                a.remove();
                // Revoke after a short delay — revoking immediately can abort the download
                setTimeout(function () { window.URL.revokeObjectURL(bUrl); }, 200);
                gpHideLoading();
                gpToast('Downloaded: ' + filename, 'success');
            } catch (e) {
                gpHideLoading();
                gpToast('Error saving PDF: ' + e.message, 'error');
            }
        },
        error: function (xhr) {
            gpHideLoading();
            gpToast('PDF generation failed (' + form + '): ' + (xhr.statusText || 'network error'), 'error');
        }
    });
}

function generateNewPdf(docDefinition, fileName) {
    if (typeof pdfMake === 'undefined') {
        gpToast('PDF engine not loaded. Please refresh the page.', 'error');
        return;
    }
    // Use Tunga font registered in vfs_fonts.js (loaded before this script).
    // Mirrors agencyhome.js implementation.
    pdfMake.fonts = {
        tunga: {
            normal: 'tunga.ttf',
            bold: 'tungab.ttf'
        },
        // Alias so any docDefinition using the pdfmake default 'Roboto' name still works
        Roboto: {
            normal: 'tunga.ttf',
            bold: 'tungab.ttf',
            italics: 'tunga.ttf',
            bolditalics: 'tungab.ttf'
        }
    };
    // Use getBlob() instead of download() so the toast + save only fire
    // after the PDF bytes are actually ready (download() is fire-and-forget).
    pdfMake.createPdf(docDefinition).getBlob(function (blob) {
        try {
            var bUrl = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = bUrl;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            setTimeout(function () { URL.revokeObjectURL(bUrl); }, 200);
            gpHideLoading();
            if (typeof hideDownloadProgress === 'function') hideDownloadProgress();
            gpToast('PDF downloaded: ' + fileName, 'success');
        } catch (e) {
            gpHideLoading();
            if (typeof hideDownloadProgress === 'function') hideDownloadProgress();
            gpToast('Error saving PDF: ' + e.message, 'error');
        }
    });
}

// Keep original-style function names for any external callers.
window.checkNmrwithAsset = checkNmrwithAsset;
window.loadNMR = loadNMR;
window.parseWorkNmrs = parseWorkNmrs;
window.bindNMRs = bindNMRs;
window.PMIAY = PMIAY;
