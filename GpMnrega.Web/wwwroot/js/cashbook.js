/**
 * cashbook.js v2.1
 * Shared utility functions for Cashbook & Registers page.
 *
 * Key changes from v2.0:
 *   - Removed WASM stubs (loadCashbookData, generateRegister, $(document).ready())
 *     → button hooks now live in home1.js v3.0
 *   - Removed duplicate $(document).ready() button handlers
 *   - Restored full appendWorksToDoc() + getContentPartsForAsset() implementations
 *     (used by register5.js → getRegister5Data → generateRegister5Form)
 *   - Restored full updateApplicants() sort + family-days accumulation logic
 *     (the v2.0 stub was missing applicants.sort() and the second each() pass)
 *
 * URLs already correct from v2.0:
 *   - /api/proxy/register3workmap   (getWorkCodeMap)
 *   - /api/proxy/register3paymentmap (getPaymentDate)
 *
 * Functions exported (consumed by per-button JS files):
 *   parseWageList, getWorkCodeMap, getPaymentDate, updateApplicants,
 *   appendWorksToDoc, getContentPartsForAsset
 */

var workAllotmentData = {};
var currentFileData = '';

function errorHandler(e) {
    console.error('Cashbook error:', e);
    gpToast('An error occurred. Please try again.', 'error');
}

// ── Wage list parser ───────────────────────────────────────────────
function parseWageList(payResponse) {
    var wageTable = $(payResponse).find('#dtWorkerPayment');
    var applicants = [];
    $(wageTable.find('tr')).each(function (i, wageRow) {
        if (i > 3) {
            var tableColumns = $(wageRow).find('td');
            applicants.push({
                'sr_no':               $(tableColumns[0]).text().trim(),
                'job_card_no':         $(tableColumns[1]).text().trim(),
                'bank_account_number': $(tableColumns[2]).text().trim(),
                'worker_name':         $(tableColumns[3]).text().trim(),
                'house_owner_name':    $(tableColumns[4]).text().trim(),
                'work_name':           $(tableColumns[5]).text().trim(),
                'muster_roll_number':  $(tableColumns[6]).text().trim(),
                'work_start_date':     $(tableColumns[7]).text().trim(),
                'work_end_date':       $(tableColumns[8]).text().trim(),
                'no_of_days':          $(tableColumns[9]).text().trim(),
                'wages_paid':          $(tableColumns[10]).text().trim()
            });
        }
    });
    return applicants;
}

// ── Work code map ──────────────────────────────────────────────────
// v2.0: /Registers/Register3Workmap → /api/proxy/register3workmap
function getWorkCodeMap(callback) {
    var workList;
    $.get('/api/proxy/register3workmap'
        + '?state_name=KARNATAKA&state_Code=15'
        + '&panchayat_code=' + reportData.panchayat_code
        + '&fin_year=' + reportData.fincialYear
        + '&dist_code=' + reportData.district_code
        + '&dist_name=' + reportData.districtName
        + '&block_code=' + reportData.block_code
        + '&block_name=' + reportData.blockName
        + '&pname' + reportData.panchayatName,
        function (response) {
            workList = $(response);
            var workListTable = $(workList).find('table')[1];
            var workNameCodeMap = {};
            var workCodeNameMap = {};
            $(workListTable).find('tr').each(function (i, workRow) {
                if (i > 0) {
                    var ia = $($(workRow).find('td')[4]).text().trim();
                    var workData = $($(workRow).find('td')[5]).text().trim();
                    var sIndex = workData.lastIndexOf('(') + 1;
                    var eIndex = workData.indexOf(')', sIndex);
                    var workCode = workData.substring(sIndex, eIndex).trim();
                    var workName = workData.substring(0, sIndex - 1).trim();
                    if (ia.indexOf('Gram Panchayat') === -1) {
                        workCode += 'IA__';
                    }
                    workNameCodeMap[workName] = workCode;
                    workCodeNameMap[workCode] = workName;
                }
            });
            callback(workNameCodeMap, workCodeNameMap);
        }
    );
}

// ── Payment date map ───────────────────────────────────────────────
// v2.0: /Registers/register3paymentmap → /api/proxy/register3paymentmap
function getPaymentDate(callback) {
    $.get('/api/proxy/register3paymentmap'
        + '?state_name=KARNATAKA&state_Code=15'
        + '&panchayat_code=' + reportData.panchayat_code
        + '&fin_year=' + reportData.fincialYear
        + '&dist_code=' + reportData.district_code
        + '&dist_name=' + reportData.districtName
        + '&block_code=' + reportData.block_code
        + '&block_name=' + reportData.blockName
        + '&pname' + reportData.panchayatName,
        function (response) {
            var workList = $(response);
            var workListTable = workList.find('table')[4];
            var paymentMap = {};
            $(workListTable).find('tr').each(function (i, tRow) {
                var tCols = $(tRow).find('td');
                if (tCols.length > 1) {
                    paymentMap[$(tCols[0]).text().trim()] = $(tCols[1]).text().trim();
                }
            });
            callback(paymentMap);
        }
    );
}

// ── Update applicants ──────────────────────────────────────────────
// Mirrors original exactly: assigns work_code/payment_date, sorts by
// muster_roll_number + start_date, then accumulates no_of_family_days.
function updateApplicants(applicants, workNameCodeMap, paymentMap) {
    var anchorIndex = 0;
    $(applicants).each(function (i, applicant) {
        applicant['work_code']    = workNameCodeMap[applicant['work_name']];
        applicant['payment_date'] = paymentMap[applicant['muster_roll_number']];
        if (!applicant['job_card_no']) {
            if (i > 0) {
                applicant['job_card_no'] = applicants[anchorIndex]['job_card_no'];
            }
        } else {
            anchorIndex = i;
        }
    });

    applicants.sort(function (a, b) {
        if (a.muster_roll_number == b.muster_roll_number) {
            if (a.work_start_date != b.work_start_date) {
                return moment(a.work_start_date, 'DD/MM/YYYY') - moment(b.work_start_date, 'DD/MM/YYYY');
            } else {
                return a.muster_roll_number = b.muster_roll_number;
            }
        } else {
            return a.muster_roll_number - b.muster_roll_number;
        }
    });

    anchorIndex = 0;
    $(applicants).each(function (i, applicant) {
        if (i > 0) {
            if (applicant['job_card_no'] == applicants[i - 1]['job_card_no']) {
                if (applicant['no_of_days']) {
                    applicants[anchorIndex]['no_of_family_days'] += parseInt(applicant['no_of_days']);
                }
            } else {
                applicant['no_of_family_days'] = parseInt(applicant['no_of_days']);
                anchorIndex = i;
            }
        } else {
            applicant['no_of_family_days'] = parseInt(applicant['no_of_days']);
            anchorIndex = i;
        }
    });
}

// ── Asset Register helpers (used by register5.js) ─────────────────

function appendWorksToDoc(docDefinition, assetList, workHash) {
    var pIndex = 0;
    $(assetList).each(function (i, asset) {
        if (workHash[asset.workCode]) {
            var cParts = getContentPartsForAsset(asset, workHash, (pIndex + 1));
            $(cParts).each(function (i, cPart) {
                docDefinition.content.push(cPart);
            });
            pIndex = pIndex + 1;
        }
    });
}

function getContentPartsForAsset(asset, workHash, index) {
    var indParts = [
        {
            pageBreak: 'before',
            pageOrientation: 'portrait',
            text: 'ಗ್ರಾಮ ಪಂಚಾಯತಿ, ' + reportData.panchayat_NameRegional + ', ತಾ|| ' + reportData.blockNameRegional + ', ಜಿ|| ' + reportData.districtNameRegional,
            style: { alignment: 'center', fontSize: 16, bold: true },
            margin: [10, 10, 10, 0]
        },
        {
            canvas: [
                { type: 'line', x1: 10, y1: 5, x2: 560, y2: 5, lineWidth: 1 }
            ]
        },
        {
            text: 'ಗ್ರಾಮದ ಹೆಸರು:',
            style: { alignment: 'left', fontSize: 14, bold: true },
            margin: [10, 5, 0, 0]
        },
        {
            text: 'ವಿಳಾಸ: ',
            style: { alignment: 'left', fontSize: 14, bold: true },
            margin: [10, 0, 0, 0]
        },
        {
            text: 'ವಿಭಾಗ : ಎ: ಆಸ್ತಿಯ ವಿವರಗಳು ',
            style: { alignment: 'center', fontSize: 16, bold: true },
            margin: [10, 0, 0, 0]
        },
        {
            style: { alignment: 'center' },
            table: {
                widths: [250, 120, '*'],
                body: [
                    [
                        { text: '1. ಆಸ್ತಿಯ ಗುರುತಿನ ಸಂಕೇತ(ನರೇಗಾ ತಂತ್ರಾಂಶದಿಂದ ಲಭ್ಯವಾದ ಆಸ್ತಿಯ ವಿಶೇಷ ಗುರುತು ಸಂಕೇತ)', style: { fontSize: 10, font: 'tunga' }, margin: [0, 0, 0, 0] },
                        { text: '15004548500', style: { fontSize: 10, font: 'tunga', bold: true }, colSpan: 2, margin: [0, 0, 0, 0] },
                        ''
                    ],
                    [
                        { text: '2. ಆಸ್ತಿಯ ಹೆಸರು', style: { fontSize: 10, font: 'tunga' }, margin: [0, 0, 0, 0] },
                        { text: ' ', style: { fontSize: 10, font: 'tunga', bold: true }, colSpan: 2, margin: [0, 0, 0, 0] },
                        ''
                    ],
                    [
                        { text: '3. ಆಸ್ತಿಯ ಪ್ರವರ್ಗ', style: { fontSize: 10, font: 'tunga' }, margin: [0, 0, 0, 0] },
                        { text: ' ', style: { fontSize: 10, font: 'tunga', bold: true }, colSpan: 2, margin: [0, 0, 0, 0] },
                        ''
                    ],
                    [
                        { text: '4. ಆಸ್ತಿಯ ವಿವರಗಳು', style: { fontSize: 10, font: 'tunga' }, margin: [0, 0, 0, 0] },
                        { text: '\t', style: { fontSize: 10, font: 'tunga', bold: true }, colSpan: 2, margin: [0, 0, 0, 0] },
                        ''
                    ],
                    [
                        { text: '5. ಪ್ರಾಥಮಿಕ ಆಸ್ತಿ ಪೂರ್ಣಗೊಂಡ ದಿನಾಂಕ', style: { fontSize: 10, font: 'tunga' }, margin: [0, 0, 0, 0] },
                        { text: '01/06/2020', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [0, 0, 0, 0] },
                        { text: '2020', style: { fontSize: 10, font: 'tunga', bold: true, width: 200 }, width: 200, margin: [0, 0, 0, 0] }
                    ],
                    [
                        { text: '6. ವ್ಯಕ್ತಿಗತ ಫಲಾನುಭವಿ (ಅನ್ವಯವಾಗುವಲ್ಲ)', style: { fontSize: 10, font: 'tunga' }, rowSpan: 2, margin: [0, 15, 0, 0] },
                        { text: 'ಹೆಸರು:', style: { fontSize: 9, font: 'tunga', bold: true }, margin: [0, 0, 0, 0] },
                        { text: 'ಉದ್ಯೋಗ ಚೀಟಿ ಸಂಖ್ಯೆ:', style: { fontSize: 9, font: 'tunga', bold: true }, margin: [0, 0, 0, 0] }
                    ],
                    [
                        '',
                        { text: '     ', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [0, 0, 0, 0] },
                        { text: '     ', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [0, 0, 0, 0] }
                    ],
                    [
                        { text: '7. ಒಗ್ಗೂಡಿಸುವಿಕೆಯಲ್ಲಿ ಒಳಗೊಂಡ ಯೋಜನೆ(ಗಳ)ಯ ಹೆಸರು (ಕೇಂದ್ರ/ರಾಜ್ಯ ಸರ್ಕಾರದ ಯೋಜನೆಗಳು,ಅನುಷ್ಠಾನ ಇಲಾಖೆಗಳು, 14ನೇ ಆರ್ಥಿಕ ಆಯೋಗ... ಇತ್ಯಾದಿ ಯೋಜನೆಗಳೊಂದಿಗೆ ಒಗ್ಗೂಡಿಸುವಿಕೆಯಡಿ ತೆಗೆದುಕೊಂಡಿದ್ದಲ್ಲಿ', style: { fontSize: 9, font: 'tunga' }, margin: [0, 0, 0, 0] },
                        { text: '', style: { fontSize: 10, font: 'tunga', bold: true }, colSpan: 2, margin: [0, 0, 0, 0] },
                        ''
                    ]
                ]
            },
            margin: [10, 0, 10, 10]
        },
        {
            text: 'ವಿಭಾಗ - ಬಿ: ಕಾಮಗಾರಿ ಆಸ್ತಿಯ ವಿವರಗಳು',
            style: { alignment: 'center', fontSize: 16, bold: true },
            margin: [10, 10, 10, 0]
        },
        {
            style: { alignment: 'center' },
            table: {
                widths: [25, 70, 65, 'auto', 40, 40, 50, 30, 30],
                body: [
                    [
                        { text: 'ಕ್ರ ಸಂ',               style: { fontSize: 8, font: 'tunga' }, rowSpan: 2, margin: [5, 5, 5, 5] },
                        { text: 'ಕಾಮಗಾರಿ ಸಂಕೇತ',         style: { fontSize: 8, font: 'tunga' }, rowSpan: 2, margin: [5, 5, 5, 5] },
                        { text: 'ಕಾಮಗಾರಿ ಮುಕ್ತಾಯ ದಿನಾಂಕ', style: { fontSize: 8, font: 'tunga' }, rowSpan: 2, margin: [5, 5, 5, 5] },
                        { text: 'ಕಾಮಗಾರಿಯ ಹೆಸರು',        style: { fontSize: 8, font: 'tunga' }, rowSpan: 2, margin: [5, 5, 5, 5] },
                        { text: 'ಮಾನವ ದಿನಗಳು',           style: { fontSize: 8, font: 'tunga' }, rowSpan: 2, margin: [5, 5, 5, 5] },
                        { text: 'ವೆಚ್ಚ(ರೂಗಳಲ್ಲಿ)',          style: { fontSize: 8, font: 'tunga' }, rowSpan: 1, colSpan: 3, margin: [5, 5, 5, 5] },
                        '',
                        '',
                        { text: 'ಹೆಸರು ಮತ್ತು ಸಹಿ',       style: { fontSize: 8, font: 'tunga' }, rowSpan: 2, margin: [5, 5, 5, 5] }
                    ],
                    [
                        '', '', '', '', '',
                        { text: 'ಅಕುಶಲ ಕೂಲಿ',                    style: { fontSize: 8, font: 'tunga' }, margin: [5, 5, 5, 5] },
                        { text: 'ಸಾಮಗ್ರಿ, ಅರೆಕುಶಲ / ಕುಶಲ ಕೂಲಿ', style: { fontSize: 8, font: 'tunga' }, margin: [5, 5, 5, 5] },
                        { text: 'ಇತರೆ ನಿಧಿಗಳು',                  style: { fontSize: 8, font: 'tunga' }, margin: [2, 5, 2, 5] },
                        ''
                    ],
                    [
                        { text: '1', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',  style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',  style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',  style: { fontSize: 8,  font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',  style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',  style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '0', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '0', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',  style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] }
                    ],
                    [
                        { text: '2',      style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '\n\n\n', style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 8,  font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] },
                        { text: '',       style: { fontSize: 10, font: 'tunga', bold: true }, margin: [5, 5, 5, 5] }
                    ]
                ]
            },
            margin: [10, 0, 10, 0]
        },
        {
            text: '\n\n',
            style: { alignment: 'right', fontSize: 14, bold: true },
            margin: [10, 10, 10, 0]
        },
        {
            text: 'ಅಧಿಕೃತ ವ್ಯಕ್ತಿಯ ಸಹಿ    \nಹೆಸರು ಮತ್ತು ದಿನಾಂಕ',
            style: { alignment: 'right', fontSize: 14, bold: true },
            margin: [10, 10, 20, 0]
        },
        {
            text: 'ಪುಟ ಸಂಖ್ಯೆ: __________ ',
            style: { alignment: 'left', fontSize: 14, bold: true },
            margin: [10, 10, 10, 0]
        }
    ];

    // Populate Section A (indParts[5] — the first table)
    indParts[5].table.body[0][1].text = asset.assetId;
    indParts[5].table.body[1][1].text = workHash[asset.workCode].workName;
    indParts[5].table.body[2][1].text = asset.assetCategory;
    indParts[5].table.body[3][1].text = asset.assetDescription;

    indParts[5].table.body[4][1].text = '';
    indParts[5].table.body[4][2].text = '';
    indParts[10].text = 'ಪುಟ ಸಂಖ್ಯೆ: ' + index;

    if (workHash[asset.workCode]) {
        console.log('Got for this code - ' + asset.workCode);
        indParts[5].table.body[4][1].text = workHash[asset.workCode].compDate;
        indParts[5].table.body[4][2].text = workHash[asset.workCode].compDate.substring(workHash[asset.workCode].compDate.lastIndexOf('/') + 1);
    }

    // Populate Section B (indParts[7] — the second table)
    indParts[7].table.body[2][1].text = asset.workCode;
    if (workHash[asset.workCode]) {
        indParts[7].table.body[2][2].text = workHash[asset.workCode].compDate;
        indParts[7].table.body[2][5].text = workHash[asset.workCode].labourExpense;
        indParts[7].table.body[2][6].text = workHash[asset.workCode].materialExpense;
    }
    indParts[7].table.body[2][3].text = workHash[asset.workCode].workName;

    return indParts;
}
