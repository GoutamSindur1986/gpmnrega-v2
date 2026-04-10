/**
 * mr.js v2.0
 * Muster Roll / Wage List / FTO helpers
 *
 * v2.0 changes from v1.x:
 *  - All ../api/*.aspx URLs updated to /api/proxy/* routes
 *  - alert() replaced with gpToast()
 *  - $('#progressBar').show() / $('#imgDown').show() replaced with gpShowLoading() / gpHideLoading()
 *  - Form 6/8/9/blk functions now delegate to gphome.js loadAndGenerate* (WASM bridge)
 *  - loadWageListData, loadFTOData, printFilledNMR updated to proxy paths
 *  - localStorage caching added for wage list and FTO data
 */

var wagelistUrl = "https://nregastrep.nic.in/netnrega/srch_wg_dtl.aspx?";
var ftoUrl      = "https://nregastrep.nic.in/netnrega/FTO/fto_trasction_dtl.aspx?";
var jcrurl      = "https://nregastrep.nic.in/netnrega/state_html/jcr.aspx?";

var _MR_API = '/api/proxy/';
var _MR_LS_TTL = 4 * 60 * 60 * 1000;  // 4-hour cache for wage/FTO data

function _mrLsSet(key, value) {
    try { localStorage.setItem(key, JSON.stringify({ ts: Date.now(), v: value })); } catch(e) {}
}
function _mrLsGet(key) {
    try {
        var raw = localStorage.getItem(key);
        if (!raw) return null;
        var obj = JSON.parse(raw);
        if (!obj || !obj.ts || (Date.now() - obj.ts > _MR_LS_TTL)) { localStorage.removeItem(key); return null; }
        return obj.v;
    } catch(e) { return null; }
}


// ── Form 6 — delegate to WASM bridge (gphome.js) ─────────────────
function loadNMRDataForForm6(elm) {
    var nmr = reportData.NMRS.filter(function(e) { return e.NMRNO == elm.dataset.nmrno; });
    if (!nmr.length) { gpToast('NMR data not found', 'warning'); return; }
    if (typeof loadAndGenerateForm6 === 'function') {
        loadAndGenerateForm6(nmr[0]);
    } else {
        gpToast('Form 6 generator not loaded', 'error');
    }
}

// ── Form 8 — delegate to WASM bridge (gphome.js) ─────────────────
function loadNMRDataForForm8(elm) {
    var nmr = reportData.NMRS.filter(function(e) { return e.NMRNO == elm.dataset.nmrno; });
    if (!nmr.length) { gpToast('NMR data not found', 'warning'); return; }
    if (typeof loadAndGenerateForm8 === 'function') {
        loadAndGenerateForm8(nmr[0]);
    } else {
        gpToast('Form 8 generator not loaded', 'error');
    }
}

// ── Form 9 — delegate to WASM bridge (gphome.js) ─────────────────
function loadNMRDataForForm9(elm) {
    var nmr = reportData.NMRS.filter(function(e) { return e.NMRNO == elm.dataset.nmrno; });
    if (!nmr.length) { gpToast('NMR data not found', 'warning'); return; }
    if (typeof loadAndGenerateForm9 === 'function') {
        loadAndGenerateForm9(nmr[0]);
    } else {
        gpToast('Form 9 generator not loaded', 'error');
    }
}

// ── Blank NMR — build accdata/jcdata from already-loaded reportData.NMRS ──
// Mirrors original GPMNREGA loadNMRDataForFormblk which reads NMR.JC directly.
// Do NOT call loadAndGenerateBlankNmr here — that causes an infinite loop.
function loadNMRDataForFormblk(elm) {
    var nmrArr = reportData.NMRS.filter(function(e) { return e.NMRNO == elm.dataset.nmrno; });
    if (!nmrArr.length) { gpToast('NMR data not found', 'warning'); return; }
    var nmr    = nmrArr[0];
    var jcList = nmr.JC || [];

    // Compute working days from DateFrom/DateTo (format: D/M/YYYY)
    var startDate = nmr.DateFrom || '';
    var endDate   = nmr.DateTo   || '';
    var wkdays = 1;
    try {
        var sd = startDate.split('/'), ed = endDate.split('/');
        var d1 = new Date(parseInt(sd[2]), parseInt(sd[1]) - 1, parseInt(sd[0]));
        var d2 = new Date(parseInt(ed[2]), parseInt(ed[1]) - 1, parseInt(ed[0]));
        wkdays = Math.round(Math.abs(d2 - d1) / 86400000) + 1;
    } catch (e) {}

    var tablerows = jcList.map(function (jc, i) {
        return {
            SiNo:          i + 1,
            JobCardNo:     jc.JCNO        || '',
            ApplicantName: jc.appName     || '',
            hh:            jc.appName     || '',   // head-of-household fallback to applicant name
            Place:         jc.appAddress  || '',
            bkname:        jc.BankName    || '',
            bkacc:         'XXXXXXXXXXXX',
            category:      jc.category    || ''
        };
    });

    var meta = {
        msrno:       nmr.NMRNO,
        startDate:   startDate,
        endDate:     endDate,
        wkdays:      wkdays,
        finSanNo:    reportData.finSanctionNo   || '',
        finSanDate:  reportData.finSanctionDate || '',
        techSanNo:   reportData.techSanctionNo  || '',
        techSanDate: reportData.techSanctionDate|| '',
        panchayat:   reportData.panchayatName   || '',
        distName:    reportData.districtName    || '',
        state:       reportData.stateName       || '',
        block:       reportData.blockName       || '',
        workname:    reportData.workName        || '',
        workcode:    reportData.workcode        || '',
        agency:      reportData.executionAgency || '',
        techStaff:   reportData.technicalstaff  || ''
    };

    gpHideLoading();
    window.wasmGeneratePdf({ type: 'nmr', data: { accdata: tablerows, jcdata: meta } });
}

// ── Wage List (direct port of old GPMNREGA loadWageListData) ──────
// Step 1: GET NMR page via proxy → extract wage list IDs from table
// Step 2: for each ID, GET wage list detail via proxy (4-step NIC crawl)
// Step 3: if FTO button → extract FTO numbers → fetch each FTO detail
//         if WageList button → convert to PDF via converter
function loadWageListData(elm) {
    var data = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    if (!data.length) { gpToast('NMR data not found', 'warning'); return; }

    gpShowLoading('Fetching Wage List…');
    $.ajax({
        url: _MR_API + 'getwagelist',
        type: 'GET',
        data: { nmr_link: data[0].url },
        startDate: data[0].DateFrom,
        success: function(resp) {
            var html = resp && resp.html ? resp.html : resp;
            // block_code comes from the NMR URL query string
            var blockcode = new URLSearchParams((data[0].url || '').split('?')[1] || '').get('block_code') || reportData.block_code;

            var form = $(html).filter((i, e) => e.id == 'form1');
            if (form.length == 0)
                form = $(html).filter((i, e) => e.id == 'aspnetForm');

            var table = form[0] && form[0].querySelectorAll("[id$='ContentPlaceHolder1_grdShowRecords'] tr");
            if (!table || table.length === 0) {
                gpHideLoading();
                gpToast('No Wagelist found.', 'warning');
                return;
            }

            var wageListId;
            $(table[0].cells).each((i, e) => {
                if (e.textContent.trim() == 'Wagelist No.') wageListId = i;
            });

            var validwages = [];
            for (var i = 1; i < table.length - 1; i++) {
                var wagelist = table[i].cells[wageListId] && table[i].cells[wageListId].textContent.trim();
                if (wagelist && validwages.indexOf(wagelist) == -1 && wagelist.length > 2)
                    validwages.push(wagelist);
            }

            if (!validwages.length) {
                gpHideLoading();
                gpToast('No Wagelist found.', 'warning');
                return;
            }

            var startDate = data[0].DateFrom;
            validwages.forEach(function(wlNo) {
                var str = 'state_code=' + reportData.state_code +
                    '&district_code=' + reportData.district_code +
                    '&state_name=' + reportData.stateName +
                    '&district_name=' + reportData.districtName +
                    '&block_code=' + blockcode +
                    '&srch=' + startDate +
                    '&wageList=' + wlNo +
                    '&short_name=' + reportData.state_shortname;
                $.ajax({
                    url: _MR_API + 'getwagelist',
                    type: 'GET',
                    data: { nmr_link: wagelistUrl + str },
                    wagelistnumber: wlNo,
                    success: function(resp) {
                        var wlHtml = resp && resp.html ? resp.html : resp;
                        var form = $(wlHtml).filter((i, e) => e.id == 'form1');
                        if (form.length == 0)
                            form = $(wlHtml).filter((i, e) => e.id == 'aspnetForm');
                        // $(wlHtml)[5] is the form element in NIC's page structure;
                        // mirrors the original check before accessing form.action
                        if ($(wlHtml)[5] !== undefined && form[0] &&
                            form[0].querySelectorAll('table').length > 0 &&
                            form[0].querySelectorAll('table')[0].querySelectorAll('tr').length > 1) {
                            if (elm.name == 'FTO') {
                                var urlParams = new URLSearchParams($(wlHtml)[5].action || '');
                                var finyear = urlParams.get('fin_year');
                                loadFTOData(wlHtml, finyear);
                            } else {
                                downloadForms(wlHtml, 'WageList', this.wagelistnumber, '');
                            }
                        }
                    },
                    error: function() {
                        gpHideLoading();
                        gpToast('Error downloading wagelist: ' + wlNo, 'error');
                    }
                });
            });
        },
        error: function() {
            gpHideLoading();
            gpToast('Error fetching wage list.', 'error');
        }
    });
}

function _parseNicHtml(html) {
    // DOMParser reliably handles full HTML documents (<!DOCTYPE html><html>...</html>)
    // unlike jQuery $(html).filter() which can miss elements not at top level.
    // Used by printFilledNMR.
    var parser = new DOMParser();
    var doc = parser.parseFromString(html, 'text/html');
    var form = doc.getElementById('form1') || doc.getElementById('aspnetForm');
    return { doc: doc, form: form };
}

// ── FTO data (direct port of old GPMNREGA loadFTOData) ────────────
// Called with raw HTML string from the wage list detail page.
// Extracts FTO numbers from column 13, fetches each FTO detail via
// proxy, then passes the centre-div HTML to downloadForms for PDF.
function loadFTOData(data, finyear) {
    var html = (data && data.html) ? data.html : data;
    var form = $(html).filter((i, e) => e.id == 'form1');
    if (form.length == 0)
        form = $(html).filter((i, e) => e.id == 'aspnetForm');
    if (form.length == 0)
        $(html).filter((i, e) => e.id == 'aspnetForm');

    if (!finyear) {
        var d = new URLSearchParams((form[0] && form[0].action) || '');
        finyear = d.get('fin_year');
    }

    var ftos = form[0] && form[0].querySelectorAll('td:nth-child(13)');
    var validfto = [];
    for (var i = 1; ftos && i < ftos.length; i++) {
        var ftoid = ftos[i].textContent.replaceAll(' ', '').replaceAll('\n', '');
        if (validfto.indexOf(ftoid) < 0 && ftoid.length > 3)
            validfto.push(ftoid);
    }

    if (!validfto.length) { gpToast('No FTO numbers found.', 'warning'); return; }

    gpToast('Fetching ' + validfto.length + ' FTO(s)…', 'info');
    validfto.forEach(function(ftoNo) {
        // Build the full NIC FTO URL and pass it directly to the proxy —
        // mirrors old code: data: ftoUrl + str → ../api/getftoDetails
        var str = 'page=p&state_code=' + reportData.state_code +
            '&state_name=' + reportData.stateName +
            '&district_code=' + reportData.district_code +
            '&district_name=' + reportData.districtName +
            '&block_code=' + reportData.block_code +
            '&block_name=' + reportData.blockName +
            '&panchayat_code=' + reportData.panchayat_code +
            '&panchayat_name=' + reportData.panchayatName +
            '&flg=W&fin_year=' + finyear +
            '&fto_no=' + ftoNo +
            '&source=national&Digest=FlTx';
        $.ajax({
            url: _MR_API + 'getftodetails',
            type: 'GET',
            data: { fto_link: ftoUrl + str },
            ftonumber: ftoNo,
            success: function(resp) {
                var ftoHtml = resp && resp.html ? resp.html : resp;
                var form = $(ftoHtml).filter((i, e) => e.id == 'form1');
                if (form.length == 0)
                    form = $(ftoHtml).filter((i, e) => e.id == 'aspnetForm');
                var centers = form[0] && form[0].querySelectorAll('center');
                var printHtml = (centers && centers[0]) ? centers[0].outerHTML : ftoHtml;
                downloadForms(printHtml, 'FTO', '', this.ftonumber);
            },
            error: function() {
                gpHideLoading();
                gpToast('Error downloading fto: ' + ftoNo, 'error');
            }
        });
    });
}

// ── Print Filled NMR ─────────────────────────────────────────────
function printFilledNMR(elm) {
    var nmrLink = elm.dataset.link || elm.dataset.nmrlink || '';
    if (!nmrLink) { gpToast('NMR link not found', 'warning'); return; }

    gpShowLoading('Loading filled NMR…');
    $.ajax({
        url: _MR_API + 'getwagelist',
        type: 'GET',
        data: { nmr_link: nmrLink },
        success: function(resp) {
            gpHideLoading();
            var html = resp && resp.html ? resp.html : resp;
            // Use DOMParser for reliable full-document parsing
            var p    = _parseNicHtml(html);
            var divPrint = p.doc.getElementById('ctl00_ContentPlaceHolder1_divprint');
            if (!divPrint || !divPrint.children.length) {
                gpToast('Could not parse filled NMR — opening in new tab', 'warning');
                window.open(nmrLink, '_blank');
                return;
            }
            var children = divPrint.children;
            // Patch work name label if present
            var lbl = divPrint.querySelector('#ctl00_ContentPlaceHolder1_lblWorkName');
            if (lbl) lbl.innerText = reportData.workName || '';
            // children[1] = header section, children[2] = table body (mirrors original)
            var out = (children[1] ? children[1].outerHTML : '') +
                      (children[2] ? children[2].innerHTML : '');
            if (typeof downloadForms === 'function') {
                downloadForms(out, 'filledNmr');
            } else {
                gpToast('downloadForms not available', 'error');
            }
        },
        error: function() {
            gpHideLoading();
            gpToast('Error loading filled NMR', 'error');
        }
    });
}

// ── buildBlankNMR ─────────────────────────────────────────────────────────────
// Called by wasm-bridge for type:'nmr' after loadAndGenerateBlankNmr prepares data.
// Mirrors blknmr1.aspx layout: A3 landscape table with dynamic day-columns.
//
// Layout (columns, 0-indexed):
//   0       SI No
//   1       Job Card No / Name
//   2       Head of Family
//   3       Applicant Name
//   4       Village
//   5       Bank Name + Account
//   6..5+N  Daily attendance (N = wkdays, one blank col per day)
//   6+N     Total Attendance
//   7+N     Daily Wage
//   8+N     Wages as per Attendance
//   9+N     Travel Allowance (row-1) / Category (row-2 & data rows)
//   10+N    Implements / Sharpening
//   11+N    Total Cash Payment
//   12+N    Applicant Signature
//   Total columns: 13 + N
//
function buildBlankNMR(accdata, jcdata, workdata) {
    try {
        // Accept both JSON string (from gphome.js) and parsed object
        var rows = typeof accdata === 'string' ? JSON.parse(accdata) : (Array.isArray(accdata) ? accdata : []);
        var meta = typeof jcdata  === 'string' ? JSON.parse(jcdata)  : (jcdata && typeof jcdata === 'object' ? jcdata : {});

        if (!meta || !meta.msrno) {
            if (typeof gpToast === 'function') gpToast('Blank NMR: missing metadata', 'warning');
            return;
        }

        var N       = Math.max(1, parseInt(meta.wkdays) || 1);  // working days
        var NCOLS   = 13 + N;
        var today   = new Date();
        var printDt = today.getDate() + '/' + (today.getMonth() + 1) + '/' + today.getFullYear();

        // ── Header row 1 (rowspan/colspan headers) ─────────────────────────
        // Cells with rowSpan:2 need a {} placeholder in row 2 at the same position.
        // The colspan cell (Daily Attendance) spans N columns; needs N-1 {} after it.
        var hR1 = [
            {text: 'ಕ್ರ.ಸಂ',                         rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'},
            {text: 'ಹೆಸರು/\nನೋಂದಣಿ ಸಂಖ್ಯೆ',          rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'},
            {text: 'ಕುಟುಂಬದ\nಮುಖ್ಯಸ್ಥರ ಹೆಸರು',      rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'},
            {text: 'ಅರ್ಜಿದಾರರ\nಹೆಸರು',              rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'},
            {text: 'ಹಳ್ಳಿ',                           rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'},
            {text: 'ಖಾತೆ ಸಂಖ್ಯೆ',                   rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'},
            {text: 'ದಿನವಹಿ ಹಾಜರಾತಿ', colSpan: N,    fontSize: 5, bold: true, alignment: 'center', font: 'tunga'}
        ];
        for (var _c = 0; _c < N - 1; _c++) hR1.push({});   // colSpan placeholders
        hR1.push({text: 'ಒಟ್ಟು\nಹಾಜರಾತಿ',            rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'});
        hR1.push({text: 'ಒಂದು ದಿನದ\nವೇತನ',          rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'});
        hR1.push({text: 'ಹಾಜರಾತಿ ತಕ್ಕಂತೆ\nಬಾಕಿ ಹಣ', rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'});
        hR1.push({text: 'ಪ್ರಯಾಣ ವೆಚ್ಚ',              /* rowspan=1 — row 2 shows Category here */
                  fontSize: 5, bold: true, alignment: 'center', font: 'tunga'});
        hR1.push({text: 'Implements /\nSharpening',   rowSpan: 2, fontSize: 5, bold: true, alignment: 'center'});
        hR1.push({text: 'ಒಟ್ಟು ನಗದು\nಪಾವತಿ',         rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'});
        hR1.push({text: 'ಅರ್ಜಿದಾರರ ಸಹಿ/\nಹೆಬ್ಬೆಟ್ಟಿನ ಗುರುತು',
                  rowSpan: 2, fontSize: 5, bold: true, alignment: 'center', font: 'tunga'});

        // ── Header row 2 (day numbers + Category) ──────────────────────────
        var hR2 = [{}, {}, {}, {}, {}, {}];   // rowspan placeholders for cols 0-5
        for (var _d = 1; _d <= N; _d++) {
            hR2.push({text: String(_d), fontSize: 5, bold: true, alignment: 'center'});
        }
        hR2.push({});   // Total Att rowspan placeholder
        hR2.push({});   // Daily Wage rowspan placeholder
        hR2.push({});   // Wages rowspan placeholder
        hR2.push({text: 'Category', fontSize: 5, bold: true, alignment: 'center'}); // col 9+N
        hR2.push({});   // Implements rowspan placeholder
        hR2.push({});   // Total Cash rowspan placeholder
        hR2.push({});   // Signature rowspan placeholder

        var tableBody = [hR1, hR2];

        // ── Data rows ───────────────────────────────────────────────────────
        for (var _r = 0; _r < rows.length; _r++) {
            var row = rows[_r];
            var dr = [
                {text: String(row.SiNo          || ''), fontSize: 5, alignment: 'center'},
                {text: String(row.JobCardNo      || ''), fontSize: 5, alignment: 'left'},
                {text: String(row.hh             || ''), fontSize: 5, font: 'tunga', alignment: 'center'},
                {text: String(row.ApplicantName  || ''), fontSize: 5, font: 'tunga', alignment: 'center'},
                {text: String(row.Place          || ''), fontSize: 5, font: 'tunga', alignment: 'center'},
                {text: (row.bkname || '') + '\n' + (row.bkacc || ''), fontSize: 4, alignment: 'left'}
            ];
            // N blank attendance cells
            for (var _a = 0; _a < N; _a++) dr.push({text: ' ', fontSize: 5});
            // 7 trailing cells — position 3 (0-based) is Category
            for (var _t = 0; _t < 7; _t++) {
                dr.push(_t === 3
                    ? {text: String(row.category || ''), fontSize: 5, alignment: 'center'}
                    : {text: ' ', fontSize: 5});
            }
            tableBody.push(dr);
        }

        // ── Footer row ──────────────────────────────────────────────────────
        var footerRow = [
            {text: 'ಕಾಮಗಾರಿ ಆರಂಭ ದಿನಾಂಕ :', colSpan: 5, fontSize: 5, bold: true, font: 'tunga', alignment: 'left'},
            {}, {}, {}, {},
            {text: 'ಒಟ್ಟು', fontSize: 5, bold: true, font: 'tunga', alignment: 'right'}
        ];
        for (var _a2 = 0; _a2 < N; _a2++) footerRow.push({text: ' ', fontSize: 5});
        for (var _t2 = 0; _t2 < 7; _t2++) footerRow.push({text: ' ', fontSize: 5});
        tableBody.push(footerRow);

        // ── Column widths (points) ──────────────────────────────────────────
        // A3 landscape usable ≈ 1160pt — allocate fixed cols then day cols
        var colWidths = [12, 52, 38, 38, 32, 44]; // cols 0-5
        for (var _w = 0; _w < N; _w++) colWidths.push(10);  // day cols
        colWidths = colWidths.concat([18, 18, 18, 22, 22, 22, 38]); // cols 6+N to 12+N

        // ── Doc definition ──────────────────────────────────────────────────
        var docDef = {
            pageSize:        'A3',
            pageOrientation: 'landscape',
            pageMargins:     [10, 10, 10, 10],
            content: [
                // Info row 1: State / NMR No / Print Date / District / Block / Panchayat / Fin Year
                {
                    columns: [
                        {text: [{text: 'ರಾಜ್ಯ : ',              bold: true, font: 'tunga'}, {text: meta.state    || ''}], fontSize: 7, width: 'auto'},
                        {text: [{text: 'ಮಸ್ಟರ್ ರೋಲ್ ಸಂಖ್ಯೆ : ', bold: true, font: 'tunga'}, {text: meta.msrno   || ''}], fontSize: 7, width: '*'},
                        {text: [{text: 'Printing date: ',         bold: true},                 {text: printDt}],            fontSize: 7, width: 'auto'},
                        {text: [{text: 'ಜಿಲ್ಲೆ: ',               bold: true, font: 'tunga'}, {text: meta.distName || ''}], fontSize: 7, width: 'auto'},
                        {text: [{text: 'ತಾಲ್ಲೂಕು: ',             bold: true, font: 'tunga'}, {text: meta.block   || ''}], fontSize: 7, width: 'auto'},
                        {text: [{text: 'ಪಂಚಾಯತಿ: ',              bold: true, font: 'tunga'}, {text: meta.panchayat|| ''}], fontSize: 7, width: 'auto'},
                        {text: [{text: 'Financial Year: ',        bold: true},                 {text: meta.finYear || ''}], fontSize: 7, width: 'auto'}
                    ],
                    columnGap: 5,
                    margin: [0, 0, 0, 3]
                },
                // Info row 2: Work Code / Work Name
                {
                    columns: [
                        {text: [{text: 'ಕಾಮಗಾರಿ ಸಂಕೇತ ಸಂಖ್ಯೆ : ', bold: true, font: 'tunga'}, {text: meta.workcode || ''}],                            fontSize: 7, width: 200},
                        {text: [{text: 'ಕಾಮಗಾರಿ ಹೆಸರು : ',          bold: true, font: 'tunga'}, {text: meta.workname || '', font: 'tunga'}],             fontSize: 7, width: '*'}
                    ],
                    margin: [0, 0, 0, 2]
                },
                // Info row 3: Dates / Agency / Sanction numbers
                {
                    columns: [
                        {text: [{text: 'ದಿನಾಂಕದಿಂದ: ',            bold: true, font: 'tunga'}, {text: (meta.startDate || '') + '   '},
                                {text: 'ದಿನಾಂಕದ ವರೆಗೆ: ',         bold: true, font: 'tunga'}, {text: meta.endDate || ''}],  fontSize: 7, width: 230},
                        {text: [{text: 'ಕಾರ್ಯನಿರ್ವಹಣಾ ಇಲಾಖೆ : ',  bold: true, font: 'tunga'}, {text: (meta.agency || '').replace('(3)', '').trim()}],  fontSize: 7, width: 180},
                        {text: [{text: 'Technical Sanction No & Date: ', bold: true},
                                {text: (meta.techSanNo || '') + ' (' + (meta.techSanDate || '') + ')   '},
                                {text: 'Financial Sanction No & Date: ',  bold: true},
                                {text: (meta.finSanNo  || '') + ' (' + (meta.finSanDate  || '') + ')'}],  fontSize: 7, width: '*'}
                    ],
                    margin: [0, 0, 0, 2]
                },
                // Info row 4: Technical staff
                {
                    text: [{text: 'Name of technical staff responsible for measurement: ', bold: true},
                           {text: meta.techStaff || ''}],
                    fontSize: 7,
                    margin: [0, 0, 0, 4]
                },
                // Main muster roll table
                {
                    table: {
                        headerRows: 2,
                        widths:     colWidths,
                        body:       tableBody
                    },
                    layout: {
                        hLineWidth: function () { return 0.5; },
                        vLineWidth: function () { return 0.5; },
                        hLineColor: function () { return '#000000'; },
                        vLineColor: function () { return '#000000'; }
                    }
                },
                // Signature row
                {
                    margin: [0, 10, 0, 0],
                    columns: [
                        {text: 'ಹಾಜರ ಪಡೆದವರ ಹೆಸರು (ಸಹಿ)',  fontSize: 7, font: 'tunga', alignment: 'center', width: '50%'},
                        {text: 'ಪರಿಶೀಲನೆ ಮಾಡಿದವರ ಸಹಿ',     fontSize: 7, font: 'tunga', alignment: 'center', width: '50%'}
                    ]
                }
            ],
            defaultStyle: { font: 'tunga', fontSize: 6 }
        };

        // generateNewPdf (gphome.js) handles watermark + pdfMake font registration
        if (typeof generateNewPdf === 'function') {
            generateNewPdf(docDef, 'BlankNMR_' + (meta.msrno || '') + '.pdf');
        } else {
            pdfMake.createPdf(docDef).download('BlankNMR_' + (meta.msrno || '') + '.pdf');
        }

    } catch (ex) {
        console.error('[buildBlankNMR] Error:', ex);
        if (typeof gpToast === 'function') gpToast('Failed to generate Blank NMR: ' + ex.message, 'error');
        if (typeof gpHideLoading  === 'function') gpHideLoading();
        if (typeof gpHideProgress === 'function') gpHideProgress();
    }
}
