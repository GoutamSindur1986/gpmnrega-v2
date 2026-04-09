/**
 * agencyhome.js v2.0
 * Migrated for .NET 8 / Razor Pages (DeptHome.cshtml)
 *
 * Key changes from v1.x:
 *  1. All $.ajax URLs updated from ../api/*.aspx to /api/proxy/*
 *  2. Cookie-based loadData() replaced — user data now injected via
 *     Razor variables (BLOCK_CODE, AGENCY, USER_EMAIL, etc.)
 *     and server-side claims.
 *  3. GIF loading replaced with gpShowLoading() / gpHideLoading()
 *  4. Old table-based NMR UI replaced with card-based #ulNmrList
 *  5. Template URLs updated from ../templates/ to /templates/
 *  6. PDF calls routed through pdfMake (unchanged) with generateNewPdf()
 *  7. Alerts replaced with gpToast() where available
 *  8. datasync.s3kn.com calls preserved for data sync
 *
 * Globals expected from DeptHome.cshtml <script> block:
 *   BLOCK_CODE, AGENCY, USER_EMAIL, _API
 */

var WageList = '';
var FTO = '';
var downloadFtoBtn = false;

// ── Report data (same structure as original) ─────────────────────
var reportData = {
    userName: '',
    stateName: 'KARNATAKA',
    stateNameRegional: '',
    state_code: '15',
    state_shortname: 'KN',
    districtName: '',
    districtNameRegional: '',
    district_code: '',
    blockName: '',
    blockNameRegional: '',
    block_code: '',
    panchayatName: '',
    panchayat_NameRegional: '',
    panchayat_code: '',
    workcode: '',
    workName: '',
    startdate: '',
    workCategory: '',
    workYear: '',
    workCostTotal: '',
    executionAgency: '',
    executionLevel: '',
    DPRFrozen: '',
    agencyCode: '',
    workStatus: '',
    finSanctionNo: '',
    finSanctionDate: '',
    techSanctionNo: '',
    techSanctionDate: '',
    UskilledExp: '',
    MaterialCost: '',
    SkilledCost: '',
    Material: [],
    fincialYear: '2025-2026',
    FormHTML: '',
    technicalstaff: '',
    LokSabha: '',
    VidhanSabha: '',
    NMRS: [],
    LineDeptName: '',
    LineDeptNameRegional: '',
    LineDeptHeader: '',
    LineDeptSign: ''
};

// ── API base (set in DeptHome.cshtml, fallback here) ─────────────
var _API = typeof _API !== 'undefined' ? _API : '/api/proxy/';

// ── PostMessage listener for blank NMR (WASM / extension bridge) ─
window.addEventListener("message", function (evt) {
    if (evt.data.task === 'dataready') {
        buildBlankNMR(evt.data.jcAccData, evt.data.WkJc, evt.data.otherData);
    } else if (evt.data.task === 'Failed') {
        gpHideLoading();
        gpToast('Failed to load blank NMR. Please try again.', 'error');
    }
});

// ── NMR table row template (for card-based list in new UI) ───────
function buildNmrCard(nmrNo, nmrLink, index) {
    return '<li class="nmr-item" data-nmrno="' + nmrNo + '">' +
        '<div class="nmr-meta">' +
        '<i class="bi bi-file-earmark-text" style="font-size:1.2rem;color:#6366f1"></i>' +
        '<span>NMR: ' + nmrNo + '</span>' +
        '</div>' +
        '<div class="nmr-actions">' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="filledNmr">Filled NMR</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="Form6">Form 6 GP</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="Form6Agency">Form 6</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="Form8">Form 8</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="Form8+">Form 8+</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="Form9">Form 9</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="WageList">WageList</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="FTO">FTO</button>' +
        '<button class="nmr-btn" data-link="' + nmrLink + '" data-nmrno="' + nmrNo + '" data-index="' + index + '" onclick="nmrBtnClicked(this)" name="blknmr">Blank NMR</button>' +
        '</div>' +
        '</li>';
}

// Legacy NMR row template (kept for compatibility with old table-based pages if needed)
var nmrtablerow = '<tr style="padding-top:10px">' +
    '<td><span>NMR No: @nmrNo</span></td>' +
    '<td>' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form6" value="Form 6 GP" title="Form 6 GP" style="width:85px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form6Agency" value="Form 6" title="Form 6 Agency" style="width:85px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form8" value="Form 8" title="Form 8" style="width:65px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form8+" value="Form 8+" title="Form 8+" style="width:65px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form9" value="Form 9" title="Form 9" style="width:65px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="WageList" value="WageList" title="WageList" style="width:85px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="FTO" value="FTO" title="FTO" style="width:65px" />' +
    '<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="blknmr" value="Blank NMR" title="Blank NMR" style="width:85px" />' +
    '</td></tr>';

// ── Form 6 row template ──────────────────────────────────────────
var form6rows = '<tr>' +
    '<td><span style="font-size: 16px;"> @SINO </span></td>' +
    '<td><span style="font-family: tunga; font-size: 16px;"> @ApplicantName </span></td>' +
    '<td><span style="font-family: tunga; font-size: 16px;"> @Address </span></td>' +
    '<td><span style="font-size: 16px;"> @JobCardNo </span></td>' +
    '<td><span style="font-size: 16px;"> @FromDate </span></td>' +
    '<td><span style="font-size: 16px;"> @toDate </span></td>' +
    '<td><span style="font-size: 16px;">  </span></td>' +
    '<td><span style="font-size: 16px;">  </span></td>' +
    '</tr>';

// ── Document ready ───────────────────────────────────────────────
$(document).ready(function () {
    // Load user data from claims (injected by Razor)
    loadData();
});

// ── Load user data from cookie (same as original) ───────────────
// In .NET 8, the UserData cookie is still set during login for JS compatibility.
// Razor globals (BLOCK_CODE, AGENCY, USER_EMAIL) serve as primary source.
function loadData() {
    var loggedCookie = window.getCookie('UserData');

    // If cookie exists, parse it (same format as original)
    if (loggedCookie) {
        var loggedinUser = loggedCookie.split('@');
        var tempblockcode;
        for (var i = 0; i < loggedinUser.length; i++) {
            var key = loggedinUser[i].split(':')[0];
            var val = loggedinUser[i].split(':')[1] ? loggedinUser[i].split(':')[1].trim() : '';

            if (key === "UserName") reportData.userName = val;
            if (key === "PanchyatCode") reportData.panchayat_code = val;
            if (key === "PanchyatName") reportData.panchayatName = val;
            if (key === "PanchayatNameRegional") reportData.panchayat_NameRegional = val;
            if (key === "TalukName") reportData.blockName = val;
            if (key === "TalukCode") {
                if (val === '15290021') {
                    tempblockcode = '15290021';
                    reportData.block_code = '1529002';
                } else {
                    reportData.block_code = val;
                }
            }
            if (key === "DistrictName") reportData.districtName = val;
            if (key === "DistrictCode") reportData.district_code = val;
            if (key === "StateName") reportData.stateName = val;
            if (key === "StateCode") reportData.state_code = val;
            if (key === "short_name") reportData.state_shortname = val;
            if (key === "DistrictNameRegional") reportData.districtNameRegional = val;
            if (key === "StateNameRegional") reportData.stateNameRegional = val;
            if (key === "TalukNameRegional") {
                if (tempblockcode === '15290021')
                    reportData.blockNameRegional = '\u0C95\u0CA8\u0C95\u0CAA\u0CC1\u0CB0';
                else
                    reportData.blockNameRegional = val;
            }
            if (key === "LokSabhaRegional") reportData.LokSabha = val;
            if (key === "vidhanSabhaRegional") reportData.VidhanSabha = val;
            if (key === "LineDeptName") {
                reportData.LineDeptName = val;
                if (tempblockcode === '15290021')
                    reportData.LineDeptNameRegional = '\u0CB5\u0CB2\u0CAF \u0C85\u0CB0\u0CA3\u0CCD\u0CAF\u0CBE\u0CA7\u0CBF\u0C95\u0CBE\u0CB0\u0CBF\u0C97\u0CB3\u0CC1 \u0CAA\u0CCD\u0CB0\u0CBE\u0CA6\u0CC7\u0CB6\u0CBF\u0C95 \u0CB5\u0CB2\u0CAF, \u0CB8\u0CBE\u0CA4\u0CA8\u0CC2\u0CB0\u0CC1';
                else
                    reportData.LineDeptNameRegional = (typeof deptname !== 'undefined') ? deptname[val] : '';
            }
            if (key === "LineDeptHeader") {
                reportData.LineDeptHeader = (val === undefined || val === 'undefined' || val === '' || val === null)
                    ? ((typeof deptname !== 'undefined') ? deptname[reportData.LineDeptName] : '')
                    : val;
            }
            if (key === "LineDeptSign") {
                reportData.LineDeptSign = (val === undefined || val === 'undefined' || val === '' || val === null)
                    ? ((typeof deptnamesign !== 'undefined') ? deptnamesign[reportData.LineDeptName] : '')
                    : val;
            }
        }
    }

    // Override with Razor-injected globals (always available, more reliable)
    if (typeof BLOCK_CODE !== 'undefined' && BLOCK_CODE) reportData.block_code = BLOCK_CODE;
    if (typeof AGENCY !== 'undefined' && AGENCY) reportData.LineDeptName = AGENCY;
}

// ── Search work by code ──────────────────────────────────────────
// Called from DeptHome.cshtml inline script searchAgencyWork()
// This function is the core data-fetching function called by the page

function searchWorkTextCode(data) {
    // Reset reportData fields
    reportData.workcode = '';
    reportData.workName = '';
    reportData.startdate = '';
    reportData.workCategory = '';
    reportData.workYear = '';
    reportData.workCostTotal = '';
    reportData.executionAgency = '';
    reportData.executionLevel = '';
    reportData.DPRFrozen = '';
    reportData.agencyCode = '';
    reportData.workStatus = '';
    reportData.finSanctionNo = '';
    reportData.finSanctionDate = '';
    reportData.techSanctionNo = '';
    reportData.techSanctionDate = '';
    reportData.UskilledExp = '';
    reportData.MaterialCost = '';
    reportData.SkilledCost = '';
    reportData.NMRS = [];
    reportData.Material = [];
    reportData.panchayatName = '';
    reportData.panchayat_NameRegional = '';
    reportData.panchayat_code = '';

    gpShowLoading('Searching work...');

    try {
        $.ajax({
            // Updated: ../api/getAgencyworkdata → datasync.s3kn.com (primary)
            url: "https://datasync.s3kn.com/api/AgencyWorkData?distcode=" + reportData.district_code + "&workcode=" + data,
            type: "GET",
            success: function (response) {
                parseDataResponse(response);
            },
            error: function (error) {
                gpHideLoading();
                gpToast('Work not found. Trying alternate search...', 'warning');
                // Fallback to NIC proxy search
                searchWorkCode();
            }
        });
    } catch (e) {
        console.log(JSON.stringify(e));
        gpHideLoading();
    }
}

function parseDataResponse(response) {
    try {
        var wd, wdetails;
        var b = JSON.parse(JSON.stringify(response));
        wd = JSON.parse(b);

        if (wd.WorkData.length !== 0) {
            wdetails = JSON.parse(wd.WorkData[0].WorkJson.replaceAll("'", '"').replace(/\n/g, '').replace(/\t/g, '').replace(/\\/g, ''));

            if (wdetails.LineDeptName.toLowerCase() !== reportData.LineDeptName.toLowerCase() && reportData.block_code !== wdetails.block_code) {
                gpToast('\u0C88 \u0CB5\u0CB0\u0CCD\u0C95\u0CCD \u0C95\u0CCB\u0CA1\u0CCD \u0CAC\u0CC6\u0CB0\u0CC6 \u0CB2\u0CC8\u0CA8\u0CCD \u0CA1\u0CBF\u0CAA\u0CBE\u0CB0\u0CCD\u0C9F\u0CCD\u0CAE\u0CC6\u0C82\u0C9F\u0CCD \u0CB8\u0CC7\u0CB0\u0CBF\u0CA6\u0CC6.', 'error');
                gpHideLoading();
                return false;
            } else {
                reportData.startdate = wdetails.startdate;
                reportData.workCategory = wdetails.workCategory;
                reportData.workYear = wdetails.workYear;
                reportData.workCostTotal = wdetails.workCostTotal;
                reportData.executionAgency = wdetails.executionAgency;
                reportData.executionLevel = wdetails.executionLevel;
                reportData.workStatus = wdetails.workStatus;
                reportData.finSanctionNo = wdetails.finSanctionNo;
                reportData.finSanctionDate = wdetails.finSanctionDate;
                reportData.techSanctionNo = wdetails.techSanctionNo;
                reportData.techSanctionDate = wdetails.techSanctionDate;
                reportData.UskilledExp = wdetails.UskilledExp;
                reportData.MaterialCost = wdetails.MaterialCost;
                reportData.SkilledCost = wdetails.SkilledCost;
                reportData.workcode = wdetails.workcode;
                reportData.workName = wdetails.workName;
                reportData.Material = wdetails.Materials;
                reportData.panchayat_NameRegional = wdetails.panchayat_NameRegional;
                reportData.panchayat_code = wdetails.panchayat_code;

                bindWorkData();
            }
        } else {
            searchWorkCode();
        }

        if (wd.MsrData.length !== 0) {
            reportData.NMRS = [];
            wd.MsrData.forEach(function (e) {
                var nmrdata = JSON.parse(e.MsrJson.replaceAll("'", '"').replace(/[\n\r\t]/g, ''));
                reportData.NMRS.push({
                    'NMRNO': nmrdata.NMRNO,
                    'DateFrom': nmrdata.DateFrom,
                    'DateTo': nmrdata.DateTo,
                    'url': nmrdata.url.replace('mnregaweb4.nic.in', 'nregastrep.nic.in'),
                    'JC': nmrdata.JC
                });
            });

            bindNMRs();
        }

        if (wdetails === null || wdetails === undefined || wdetails === 'undefined')
            searchWorkCode();
        else {
            if (wdetails.AssetLink == null || wdetails.AssetLink === undefined || wdetails.AssetLink === 'undefined')
                searchWorkCode();
            else
                loadNMR(wdetails.AssetLink);
        }

        gpHideLoading();
    } catch (e) {
        console.log('parseDataResponse error: ' + e);
        gpHideLoading();
    }
}

// ── Fallback search via NIC proxy ────────────────────────────────
function searchWorkCode() {
    var resultsFound = false;
    gpShowLoading('Searching NIC...');

    var searchVal = document.getElementById('txtSearchWork')
        ? document.getElementById('txtSearchWork').value.trim()
        : (document.getElementById('txtSearchWorkCode')
            ? document.getElementById('txtSearchWorkCode').value.trim() : '');

    // Updated: ../api/getAgencyAsset → /api/proxy/getagencyasset
    $.ajax({
        url: _API + "getagencyasset",
        type: "GET",
        data: {
            work_code: searchVal.toLowerCase(),
            fin_year: reportData.fincialYear || '2025-2026'
        },
        success: function (resp) {
            var data = resp.html || resp;
            var BetaData = { Materials: [] };

            try {
                var $data = $(data);
                var links = $data.find('a');
                if (links.length < 3) {
                    gpHideLoading();
                    gpToast('Work not found', 'error');
                    return;
                }

                var urlParams = new URLSearchParams(links[2].search.replace('?', ''));

                var pcode = BetaData['panchayat_code'] = reportData.panchayat_code = urlParams.get('Panchayat_Code');
                BetaData['panchayatName'] = reportData.panchayatName = urlParams.get('panchayat_name');
                var wkcode = BetaData['workcode'] = reportData.workcode = urlParams.get('wkcode');
                var block_code = urlParams.get('block_code');
                var blockName = urlParams.get('block_name');

                var linedeptname = $($($data.find('tr')[2]).find('td')[6]).text().trim().toLowerCase();
                linedeptname = linedeptname === "r.d.deptt." ? "r.d.deptt" : linedeptname;

                if ((reportData.LineDeptName.toLowerCase().includes(linedeptname) ||
                    linedeptname.includes(reportData.LineDeptName.toLowerCase())) &&
                    (reportData.block_code === block_code || reportData.blockName === blockName)) {

                    BetaData['panchayat_NameRegional'] = reportData.panchayat_NameRegional =
                        (typeof gpname !== 'undefined') ? gpname[pcode] : '';
                    BetaData['LineDeptName'] = reportData.LineDeptName;
                    BetaData['block_code'] = block_code;
                    BetaData['AssetLink'] = "https://mnregaweb4.nic.in/netnrega/citizen_html/workasset.aspx" + links[2].search;

                    reportData.fincialYear = document.getElementById('ddlyear')
                        ? document.getElementById('ddlyear').value : '2025-2026';

                    var tempWkname = $data.find('b')[14] ? $data.find('b')[14].textContent : '';
                    var wkname = reportData.workName = BetaData['workName'] = tempWkname.substr(tempWkname.indexOf('(') + 1, tempWkname.length - (tempWkname.indexOf('(') + 2));

                    var link = encodeURI("district_code=" + reportData.district_code + "&block_code=" + reportData.block_code + "&panchayat_code=" + pcode + "&work_code=" + wkcode);

                    searchTextCode(link, BetaData);
                    loadNMR("https://mnregaweb4.nic.in/netnrega/citizen_html/workasset.aspx" + links[2].search);
                } else {
                    gpToast('Data not found for this department.', 'error');
                }
            } catch (parseErr) {
                console.log('searchWorkCode parse error: ' + parseErr);
                gpToast('Failed to parse work data', 'error');
            }

            gpHideLoading();
        },
        error: function () {
            gpHideLoading();
            gpToast('Work not found on NIC server', 'error');
        }
    });
}

function searchTextCode(link, BetaData) {
    try {
        // Updated: ../api/getAgencyworkdata → /api/proxy/getagencyworkdata
        $.ajax({
            url: _API + "getagencyworkdata?" + link,
            type: "GET",
            success: function (resp) {
                var html = resp.html || resp;
                parseWorkCodeResponse(html, BetaData);
            },
            error: function () {
                gpHideLoading();
                gpToast('Unable to fetch work data', 'error');
            }
        });
    } catch (e) {
        gpToast("Unable to fetch work data: Main server running slow.", 'error');
    }
}

function parseWorkCodeResponse(response, BetaData) {
    try {
        var form = $(response).filter(function (i, e) { return e.id === 'form1'; });
        if (form.length === 0)
            form = $(response).filter(function (i, e) { return e.id === 'aspnetForm'; });

        reportData.startdate = BetaData['startDate'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_LblWrksdate').text().trim();
        reportData.workCategory = BetaData['workCategory'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_catlbl').text().trim();
        reportData.workYear = BetaData['workYear'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblfin_text').text().trim();
        reportData.workCostTotal = BetaData['workCostTotal'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblfin_total').text().trim();
        reportData.executionAgency = BetaData['executionAgency'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lbl_agency_text').text().trim();
        reportData.executionLevel = BetaData['executionLevel'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_ExeLevel_text').text().trim();
        reportData.workStatus = BetaData['workStatus'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblworkstatus_text').text().trim();
        reportData.finSanctionNo = BetaData['finSanctionNo'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblsanc_fin_no').text().trim();
        reportData.finSanctionDate = BetaData['finSanctionDate'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblsanc_fin_dt').text().trim();
        reportData.techSanctionNo = BetaData['techSanctionNo'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblsanctionno_text').text().trim();
        reportData.techSanctionDate = BetaData['techSanctionDate'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblsandate_text').text().trim();
        reportData.UskilledExp = BetaData['UskilledExp'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblSanc_Tech_Labr_Unskilled').text().trim();
        reportData.MaterialCost = BetaData['MaterialCost'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblEst_Cost_Material').text().trim();
        reportData.SkilledCost = BetaData['SkilledCost'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblskill').text().trim();

        BetaData['msrCount'] = 0;
        reportData.workcode = BetaData['workcode'];

        $(form[0]).find('#ctl00_ContentPlaceHolder1_GridView1 tr').each(function (i, e) {
            if (i !== 0) {
                try {
                    BetaData.Materials.push({
                        'Material': e.cells[1].innerText.trim().replaceAll('"', '').replaceAll("'", ''),
                        'Quantity': e.cells[2].innerText.trim(),
                        'UnitPrice': e.cells[3].innerText.trim(),
                        'Total': e.cells[4].innerText.trim()
                    });
                } catch (err) {
                    console.log('error in material list workcode:' + BetaData.workCode);
                }
            }
        });

        reportData.Material = BetaData.Materials;
        bindWorkData();

        // Sync to datasync.s3kn.com
        $.ajax({
            type: 'post',
            url: "https://datasync.s3kn.com/api/workdata?distcode=" + reportData.district_code + "&gpcode=" + reportData.panchayat_code + "&workcode=" + BetaData["workcode"].toUpperCase(),
            contentType: 'application/json',
            data: '"' + (JSON.stringify(BetaData)).replaceAll('"', "'") + '"',
            success: function () { console.log("data synced"); },
            error: function () { console.log("Error while syncing workcode details..."); }
        });

        gpHideLoading();
    } catch (e) {
        gpToast("Unable to fetch work data: Main server running slow.", 'error');
    }
}

// ── Load NMR data ────────────────────────────────────────────────
function loadNMR(str) {
    // Updated: ../api/wagelistdata → /api/proxy/getwagelist
    $.ajax({
        url: _API + "getwagelist",
        type: "GET",
        data: { nmr_link: str },
        timeout: 120000,
        success: function (resp) {
            var html = resp.html || resp;
            parseWorkNmrs(html);
        },
        error: function (error) {
            console.log('loadNMR Error: ' + error);
        }
    });
}

function parseWorkNmrs(response) {
    try {
        var loadedNmrs = [];

        for (var k = 0; k < reportData.NMRS.length; k++) {
            loadedNmrs.push(reportData.NMRS[k].NMRNO);
        }

        var html = $(response).find('a').filter(function (i, a) {
            var urlParams = new URLSearchParams(a.href);
            if (urlParams.get('msrno') !== null && typeof (urlParams.get('msrno')) !== "undefined") {
                var loadIndex = loadedNmrs.indexOf(urlParams.get('msrno').trim());
                if (loadIndex !== -1 && reportData.NMRS[loadIndex].url === '') {
                    reportData.NMRS[loadIndex].url = "https://mnregaweb4.nic.in/netnrega/citizen_html/Musternew.aspx" + a.search;
                    // Sync updated URL
                    $.ajax({
                        type: 'post',
                        url: "https://datasync.s3kn.com/api/msrupdate?distcode=" + reportData.district_code + "&msrno=" + urlParams.get('msrno').trim() + "&workcode=" + decodeURI(reportData.workcode.toUpperCase()),
                        contentType: 'application/json',
                        data: '"' + JSON.stringify(reportData.NMRS[loadIndex]).replaceAll('"', "'") + '"',
                        success: function () { },
                        error: function () { }
                    });
                } else if (loadedNmrs.indexOf(urlParams.get('msrno').trim()) === -1) {
                    loadedNmrs.push(urlParams.get('msrno').trim());
                    return a;
                }
            }
        });

        if (html.length === 0) {
            console.log('No Delta filled');
        } else {
            for (var i = 0; i < html.length; i++) {
                // Updated: ../api/getnmrdata → /api/proxy/getnmrdata
                $.ajax({
                    url: _API + "getnmrdata",
                    type: "GET",
                    data: { work_code: reportData.workcode, nmr_link: "https://mnregaweb4.nic.in/netnrega/citizen_html/Musternew.aspx" + html[i].search },
                    timeout: 900000,
                    success: function (resp) {
                        var respHtml = resp.html || resp;
                        var form = $(respHtml).filter(function (idx, e) { return e.id === 'form1'; });
                        if (form.length === 0)
                            form = $(respHtml).filter(function (idx, e) { return e.id === 'aspnetForm'; });

                        var urlParams = new URLSearchParams(form[0].action);
                        var fin_year = urlParams.get('finyear');
                        var startDate = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_lbldatefrom")[0].innerText;
                        var endDate = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_lbldateto")[0].innerText;
                        var msrno = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_lblMsrNo2")[0].innerText;
                        var JC = [];
                        var table = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_grdShowRecords tr");
                        var nmrbcs = table[1].cells[1].querySelectorAll('a')[0].innerText.split('-');
                        var nmrbccode = '15' + nmrbcs[1] + nmrbcs[2];
                        var wageListId = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_grdShowRecords tr")[0].cells.length - 5;

                        for (var j = 1; j < table.length - 1; j++) {
                            var name = table[j].cells[1].innerHTML.split('<br>')[0];
                            var applicantname = name.substr('<font face="Verdana" color="#284775" size="2">'.length, name.length - '<font face="Verdana" color="#284775" size="2">'.length);
                            applicantname = applicantname.indexOf('(') !== -1 ? applicantname.substring(0, applicantname.indexOf('(')).trim() : applicantname.trim();
                            var bankid = table[j].cells.length - 8;

                            JC.push({
                                'JCNO': table[j].cells[1].querySelectorAll('a')[0].innerText.trim(),
                                'appName': applicantname,
                                'appAddress': table[j].cells[3].textContent.trim(),
                                'category': table[j].cells[2].innerText.length > 3 ? table[j].cells[2].innerText.substr(0, 3) : table[j].cells[2].innerText,
                                'wageList': table[j].cells[wageListId].textContent.trim(),
                                'BankName': table[j].cells[bankid].innerText.trim()
                            });
                        }

                        reportData.NMRS.push({
                            'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate,
                            'url': this.data ? this.data.nmr_link : '', 'JC': JC
                        });

                        // Append NMR card to list
                        var nmrHtml = buildNmrCard(msrno, this.data ? this.data.nmr_link : '', reportData.NMRS.length - 1);
                        $('#ulNmrList').append(nmrHtml);

                        // Sync
                        $.ajax({
                            type: 'post',
                            url: "https://datasync.s3kn.com/api/msrdata?distcode=" + reportData.district_code + "&msrno=" + msrno + "&workcode=" + decodeURI(reportData.workcode.toUpperCase()),
                            contentType: 'application/json',
                            data: '"' + (JSON.stringify({
                                'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate,
                                'url': this.data ? this.data.nmr_link : '', 'JC': JC
                            })).replaceAll('"', "'") + '"',
                            success: function () { },
                            error: function () { }
                        });
                    },
                    error: function () {
                        console.log('Error loading NMR data');
                    }
                });
            }
        }

        // Check for delta (newly issued NMRs)
        var lmr = "";
        for (var m = 0; m < loadedNmrs.length; m++) {
            lmr += loadedNmrs[m] + ",";
        }
        checkdelta(lmr);

    } catch (e) {
        gpToast("Unable to fetch NMR data: Main server running slow.", 'error');
    }
}

// ── Bind work data to new UI ─────────────────────────────────────
function bindWorkData() {
    // For the new card-based UI (DeptHome.cshtml)
    var woGrid = document.getElementById('woGrid');
    if (woGrid) {
        woGrid.innerHTML = [
            ['Work Name', reportData.workName],
            ['Work Code', reportData.workcode],
            ['Category', reportData.workCategory],
            ['Execution Agency', reportData.executionAgency],
            ['Fin. Sanction', reportData.finSanctionNo + ' / ' + reportData.finSanctionDate],
            ['Tech. Sanction', reportData.techSanctionNo + ' / ' + reportData.techSanctionDate],
            ['Total Cost', reportData.workCostTotal],
            ['Status', reportData.workStatus]
        ].map(function (pair) {
            return '<div class="wo-item"><div class="wo-label">' + pair[0] + '</div>' +
                '<div class="wo-value">' + pair[1] + '</div></div>';
        }).join('');
    }

    // Also bind to old selectors if they exist (backward compatibility)
    if ($('#wrkName').length) {
        $('#wrkName').text(reportData.workName);
        $('#wrkCode').text(reportData.workcode);
        $('#wrkCategory').text(reportData.workCategory);
        $('#exeAgency').text(reportData.executionAgency);
        $('#finNoDate').text(reportData.finSanctionNo + " & " + reportData.finSanctionDate);
        $('#techNoDate').text(reportData.techSanctionNo + " & " + reportData.techSanctionDate);
        $('#tblWorkInfo').show();
    }

    // Show work overview card
    var workCard = document.getElementById('workOverviewCard');
    if (workCard) workCard.style.display = '';
    var emptyState = document.getElementById('emptyState');
    if (emptyState) emptyState.style.display = 'none';

    // Update stats sidebar
    var statsCard = document.getElementById('statsCard');
    if (statsCard) statsCard.style.display = '';
    var skeleton = document.getElementById('sidebarSkeleton');
    if (skeleton) skeleton.style.display = 'none';
}

// ── Bind NMRs to new card-based UI ──────────────────────────────
function bindNMRs() {
    var ulNmrList = document.getElementById('ulNmrList');
    if (ulNmrList) {
        var html = '';
        for (var i = 0; i < reportData.NMRS.length; i++) {
            html += buildNmrCard(reportData.NMRS[i].NMRNO, reportData.NMRS[i].url, i);
        }
        ulNmrList.innerHTML = html;
    }

    // Also populate legacy table if present
    if ($('#tblNMR').length) {
        var innerhtm = "";
        for (var j = 0; j < reportData.NMRS.length; j++) {
            innerhtm += nmrtablerow
                .replaceAll("@nmrNo", reportData.NMRS[j].NMRNO)
                .replaceAll("@Index", j)
                .replaceAll("@nmrLink", reportData.NMRS[j].url);
        }
        $('#tblNMR').html(innerhtm);
    }
}

// ── Expose buildAgencyNmrList for DeptHome.cshtml inline script ──
function buildAgencyNmrList(nmrList, targetSelector) {
    if (nmrList && nmrList.length) {
        reportData.NMRS = nmrList;
        bindNMRs();
    }
}

// ── NMR button click handler ─────────────────────────────────────
function nmrBtnClicked(btn) {
    gpShowLoading('Generating...');
    downloadPage(btn);
}

// ── Download / generate forms ────────────────────────────────────
function downloadPage(elm) {
    try {
        var strurl;
        var validForms = false;

        if (elm.name === "filledNmr" || elm.name === "Form6" || elm.name === "Form9" ||
            elm.name === "Form8" || elm.name === "Form8+" || elm.name === "FTO" ||
            elm.name === "WageList" || elm.name === "blknmr" || elm.name === 'Form6Agency') {
            validForms = false;
        } else {
            if (elm.currentTarget && elm.currentTarget.name === "geotag") {
                validForms = false;
            }
            if (elm.currentTarget && elm.currentTarget.name === "completion") {
                validForms = false;
                generateCompletion();
            } else if (elm.currentTarget && elm.currentTarget.name === "checklist") {
                generatechecklist();
            } else if (elm.currentTarget && elm.currentTarget.name === "musterrolemov") {
                generateMusterRollMoventSlip();
            } else if (elm.currentTarget) {
                // Updated: /templates/AGENCY/ → /templates/AGENCY/
                strurl = "/templates/AGENCY/" + elm.currentTarget.name;
                validForms = true;
                reportData.FormHTML = '';
            }
        }

        if (validForms) {
            $.ajax({
                url: strurl,
                method: "POST",
                timeout: 0,
                contentType: "application/x-www-form-urlencoded",
                data: $.param(reportData),
                xhrFields: { responseType: 'html' }
            }).done(function (html) {
                try {
                    downloadForms(html, elm.currentTarget.name);
                } catch (e) {
                    console.log('downloadPage error: ' + e);
                    gpHideLoading();
                    gpToast('Error generating form', 'error');
                }
            }).fail(function () {
                gpHideLoading();
                gpToast('Error generating form', 'error');
            });
        }

        // Dispatch to specific form generators
        if (elm.name === "Form6") generateform6gp(elm);
        if (elm.name === "Form6Agency") generateform6(elm);
        if (elm.name === "Form8") genereateform8(elm);
        if (elm.name === "Form8+") generate8plus(elm);
        if (elm.name === "Form9") generateform9(elm);

        if (elm.name === "blknmr") {
            var person = prompt("Please enter Name of technical staff responsible for measurement:", " ");
            if (person == null || person === " ") {
                gpToast("Name of technical staff responsible for measurement is required.", 'warning');
                gpHideLoading();
            } else {
                reportData.technicalstaff = person.toUpperCase();
                loadNMRDataForFormblk(elm);
            }
        }

        if (elm.name === "FTO") loadWageListData(elm);
        if (elm.name === "WageList") loadWageListData(elm);

        if (elm.name === "filledNmr") printFilledNMR(elm);

        if (elm.currentTarget && elm.currentTarget.name === "geotag") {
            gpShowLoading('Loading GeoTag...');
            // Updated: ../templates/geotag → /templates/geotag
            $.ajax({
                url: "/templates/geotag?workcode=" + reportData.workcode +
                    "&district_code=" + reportData.district_code +
                    "&block_code=" + reportData.block_code +
                    "&panchayat_code=" + reportData.panchayat_code +
                    "&bn=" + reportData.blockName +
                    "&pn=" + reportData.panchayatName +
                    "&ds=" + reportData.districtName +
                    "&fin=" + reportData.fincialYear +
                    "&WorkName=" + reportData.workName,
                type: 'GET',
                success: function (resp) {
                    generategeotag(resp);
                },
                error: function () {
                    gpHideLoading();
                    gpToast('Failed to load GeoTag data', 'error');
                }
            });
        }
    } catch (e) {
        console.log('downloadPage error: ' + e);
        gpHideLoading();
    }
}

// ── Download forms (PDF via server) ──────────────────────────────
function downloadForms(html, form, wage, fto) {
    // Updated: /templates/converter → /templates/converter
    var strurl = "/templates/converter?page=" + form +
        "&stateName=" + reportData.stateName +
        "&workcode=" + reportData.workcode +
        "&WageList=" + (wage || WageList) +
        "&FTO=" + (fto || FTO);

    $.ajax({
        url: strurl,
        type: "post",
        data: html,
        contentType: "application/x-www-form-urlencoded",
        xhrFields: { responseType: 'blob' },
        success: function (blob, status, xhr) {
            try {
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                    var matches = filenameRegex.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }
                var a = document.createElement('a');
                var url = window.URL.createObjectURL(blob);
                a.href = url;
                a.download = filename;
                document.body.append(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);

                gpHideLoading();
                gpToast('Download complete', 'success');
            } catch (e) {
                gpHideLoading();
                gpToast("Error downloading form. Please try again.", 'error');
                console.log(e);
            }
        },
        error: function () {
            gpHideLoading();
            gpToast('Download failed', 'error');
        }
    });
}

// ── Check for delta (newly issued NMRs) ──────────────────────────
function checkdelta(nmrLoaded) {
    var searchVal = document.getElementById('txtSearchWork')
        ? document.getElementById('txtSearchWork').value.trim()
        : (document.getElementById('txtSearchWorkCode')
            ? document.getElementById('txtSearchWorkCode').value.trim() : '');

    // Updated: ../templates/checkdeltaagency → /templates/checkdeltaagency
    var url = "/templates/checkdeltaagency?state_code=" + reportData.state_code +
        "&dist_code=" + reportData.district_code +
        "&block_code=" + reportData.workcode.substr(0, 7) +
        "&panch_code=" + reportData.panchayat_code +
        "&fin_year=" + reportData.fincialYear +
        "&work_code=" + searchVal.toLowerCase() +
        "&state_name=" + reportData.stateName +
        "&dist_name=" + reportData.districtName +
        "&block_name=" + reportData.blockName +
        "&panch_name=" + reportData.panchayatName +
        "&IsAgency=true";

    $.ajax({
        url: url,
        type: "POST",
        dataType: "text",
        data: nmrLoaded,
        success: function (res) {
            var delta = res.split('UpdatePanel2|\r\n');
            var deltaexists = false;

            if (delta.length > 1) {
                for (var i = 1; i < delta.length; i++) {
                    try {
                        var msrno = $(delta[i])[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lblMsrNo2')[0].innerText;

                        if (reportData.NMRS.filter(function (e) { return e.NMRNO === msrno; }).length === 0) {
                            deltaexists = true;
                            var startDate = $(delta[i])[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lbldatefrom')[0].innerText;
                            var endDate = $(delta[i])[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lbldateto')[0].innerText;

                            var JC = [];
                            var table = $(delta[i])[2].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr');
                            var nmrbcs = table[1].cells[1].querySelectorAll('a')[0].innerText.split('-');
                            var nmrbccode = '15' + nmrbcs[1] + nmrbcs[2];
                            var wageListId = table[0].cells.length - 4;
                            var bankid = table[0].cells.length - 7;

                            for (var j = 1; j < table.length - 1; j++) {
                                var applicantname = table[j].cells[1].innerHTML.split('<br>')[0];
                                applicantname = applicantname.indexOf('(') !== -1
                                    ? applicantname.substring(0, applicantname.indexOf('('))
                                    : applicantname;

                                JC.push({
                                    'JCNO': table[j].cells[1].querySelectorAll('a')[0].innerText.trim(),
                                    'appName': applicantname,
                                    'appAddress': table[j].cells[3].textContent.trim(),
                                    'category': table[j].cells[2].innerText.length > 3 ? table[j].cells[2].innerText.substr(0, 3) : table[j].cells[2].innerText,
                                    'wageList': table[j].cells[wageListId].textContent.trim(),
                                    'BankName': table[j].cells[bankid].textContent.trim()
                                });
                            }

                            var nmrUrl = 'https://nregastrep.nic.in/netnrega/citizen_html/musternew.aspx?lflag=&id=1' +
                                '&state_name=' + reportData.stateName +
                                '&district_name=' + reportData.districtName +
                                '&block_name=' + reportData.blockName +
                                '&panchayat_name=' + reportData.panchayatName +
                                '&block_code=' + nmrbccode +
                                '&msrno=' + msrno +
                                '&finyear=' + reportData.fincialYear +
                                '&workcode=' + reportData.workcode +
                                '&dtfrm=' + startDate +
                                '&dtto=' + endDate +
                                '&wn=' + encodeURI(reportData.workName) +
                                '&Digest=S3KN';

                            reportData.NMRS.unshift({
                                'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate,
                                'url': nmrUrl, 'JC': JC
                            });

                            // Sync delta NMR
                            $.ajax({
                                type: 'post',
                                url: "https://datasync.s3kn.com/api/msrdata?distcode=" + reportData.district_code + "&msrno=" + msrno + "&workcode=" + decodeURI(reportData.workcode.toUpperCase()),
                                contentType: 'application/json',
                                data: '"' + (JSON.stringify({
                                    'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate,
                                    'url': nmrUrl, 'JC': JC
                                })).replaceAll('"', "'") + '"',
                                success: function () { },
                                error: function () { }
                            });
                        }
                    } catch (parseErr) {
                        console.log('Delta parse error: ' + parseErr);
                    }
                }

                if (deltaexists) {
                    // Re-render full NMR list
                    bindNMRs();
                    localStorage.setItem(reportData.workcode.toLowerCase(), JSON.stringify(reportData));
                }
            }

            if (!deltaexists) {
                console.log('No new issued NMR found.');
            }
        },
        error: function () {
            console.log('checkdelta error');
        }
    });
}

// ── Load fresh from server ───────────────────────────────────────
function loadFreshFromServer() {
    var str = 'https://nregastrep.nic.in/netnrega/citizen_html/workasset.aspx?block_code=' +
        reportData.block_code +
        "&Panchayat_Code=" + reportData.panchayat_code +
        "&wkcode=" + reportData.workcode +
        "&state_name=" + reportData.stateName +
        "&district_name=" + reportData.districtName +
        "&block_name=" + reportData.blockName +
        "&panchayat_name=" + reportData.panchayatName +
        "&Digest=s3kn";
    loadNMR(str);
}

// ── Cookie helper (kept for backward compatibility) ──────────────
window.getCookie = function (name) {
    var match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    if (match) return match[2];
};

// ── PDF generation via pdfMake ───────────────────────────────────
function generateNewPdf(docDefinition, fileName) {
    pdfMake.fonts = {
        tunga: {
            normal: 'tunga.ttf',
            bold: 'tungab.ttf'
        }
    };
    pdfMake.createPdf(docDefinition).download(fileName);
    gpHideLoading();
    gpToast('PDF downloaded: ' + fileName, 'success');
}

// ── String helper ────────────────────────────────────────────────
String.prototype.replaceAt = function (index, replacement) {
    if (index !== undefined && replacement !== undefined)
        return this.substr(0, index) + replacement + this.substring(index + replacement.length);
};

function escapeRegExp(text) {
    return text.replace(/[+\\]/g, '\\$&');
}

// ── gpToast fallback (if not defined in layout) ──────────────────
if (typeof gpToast === 'undefined') {
    window.gpToast = function (msg, type) {
        if (type === 'error') console.error(msg);
        else if (type === 'warning') console.warn(msg);
        else console.log(msg);
        alert(msg);
    };
}

// ── gpShowLoading / gpHideLoading fallback ───────────────────────
if (typeof gpShowLoading === 'undefined') {
    window.gpShowLoading = function (msg) {
        console.log('Loading: ' + (msg || ''));
    };
    window.gpHideLoading = function () { };
}
