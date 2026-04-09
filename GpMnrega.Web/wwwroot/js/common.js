/**
 * common.js v1.3
 * Shared utilities for Cashbook & Registers page.
 * Extracted from original home1.js (CashbookRegisters/scripts/home1.js).
 *
 * loadData() reads the #gp-home-config JSON data island injected by the Razor Page.
 * The data island is built from ASP.NET auth cookie claims which are populated at
 * login time by AuthenticateUser SP → BuildClaimsPrincipal. No extra DB call needed.
 *
 * Functions included: monthNames, monthDays, loadData(), checkLeapYear(), getCookie()
 */

const monthNames = ["January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];
const monthDays = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

var reportData = {
    userName: '',
    stateName: '',
    stateNameRegional: '',
    districtName: '',
    districtNameRegional: '',
    blockName: '',
    blockNameRegional: '',
    panchayat_NameRegional: '',
    panchayatName: '',
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
    fincialYear: '',
    state_shortname: '',
    state_code: '',
    district_code: '',
    block_code: '',
    panchayat_code: '',
    UskilledExp: '',
    MaterialCost: '',
    SkilledCost: '',
    LokSabha: '',
    VidhanSabha: '',
    isSubscribed: false,
    NRMUsedNo: [],
    NMRLinks: [],
    FormHTML: '',
    technicalstaff: ''
};

/**
 * Load GP data into reportData from the #gp-home-config data island.
 *
 * The data island is a <script type="application/json"> block injected by the
 * Razor Page. It is built from ASP.NET auth cookie claims set at login time by
 * AuthenticateUser SP → BuildClaimsPrincipal. Claims persist for the full session
 * so the data island is always fully populated — no extra API call or cookie
 * parsing needed.
 *
 * Key mapping (data island → reportData):
 *   panchayat_code         → panchayat_code
 *   panchayatName          → panchayatName
 *   panchayat_NameRegional → panchayat_NameRegional
 *   districtName           → districtName
 *   district_code          → district_code
 *   districtNameRegional   → districtNameRegional
 *   blockName              → blockName
 *   block_code             → block_code
 *   blockNameRegional      → blockNameRegional
 *   stateName              → stateName
 *   state_code             → state_code
 *   state_shortname        → state_shortname
 *   stateNameRegional      → stateNameRegional
 *   userName               → userName
 *   vidhanSabha            → VidhanSabha
 *   lokSabha               → LokSabha
 */
function loadData() {
    try {
        var el = document.getElementById('gp-home-config');
        if (!el || !el.textContent) {
            console.warn('[common.js] #gp-home-config element not found');
            return;
        }
        var geo = JSON.parse(el.textContent);
        if (!geo) return;

        if (geo.panchayat_code)         reportData.panchayat_code         = geo.panchayat_code;
        if (geo.panchayatName)          reportData.panchayatName           = geo.panchayatName;
        if (geo.panchayat_NameRegional) reportData.panchayat_NameRegional  = geo.panchayat_NameRegional;
        if (geo.districtName)           reportData.districtName            = geo.districtName;
        if (geo.district_code)          reportData.district_code           = geo.district_code;
        if (geo.districtNameRegional)   reportData.districtNameRegional    = geo.districtNameRegional;
        if (geo.blockName)              reportData.blockName               = geo.blockName;
        if (geo.block_code)             reportData.block_code              = geo.block_code;
        if (geo.blockNameRegional)      reportData.blockNameRegional       = geo.blockNameRegional;
        if (geo.stateName)              reportData.stateName               = geo.stateName;
        if (geo.state_code)             reportData.state_code              = geo.state_code;
        if (geo.state_shortname)        reportData.state_shortname         = geo.state_shortname;
        if (geo.stateNameRegional)      reportData.stateNameRegional       = geo.stateNameRegional;
        if (geo.userName)               reportData.userName                = geo.userName;
        if (geo.vidhanSabha)            reportData.VidhanSabha             = geo.vidhanSabha;
        if (geo.lokSabha)               reportData.LokSabha                = geo.lokSabha;
        reportData.isSubscribed = geo.isSubscribed === true;

        reportData.fincialYear = $('#ddlyear option:selected').val() || '';
    } catch (e) {
        console.warn('[common.js] loadData failed to parse #gp-home-config', e);
    }
}

/**
 * Returns 1 if the given year is a leap year, 0 otherwise.
 * Mirrors checkLeapYear() in original home1.js exactly.
 */
function checkLeapYear(year) {
    if ((0 == year % 4) && (0 != year % 100) || (0 == year % 400)) {
        return 1;
    } else {
        return 0;
    }
}

/**
 * Read a cookie by name.
 * Mirrors getCookie() in original home1.js exactly.
 */
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return '';
}

/**
 * Fetch a single NIC work-detail link via the proxy.
 * Mirrors fetchWorkLink() in original cashbook.js (line 2818).
 * URL updated: ../api/wagelistdata → /api/proxy/wagelistdata
 */
function fetchWorkLink(link, linkRespCallback, retry) {
    if (retry === undefined) retry = 3;
    $.ajax({
        type: "POST",
        url: "/api/proxy/wagelistdata",
        data: link,
        success: function (data) {
            linkRespCallback(data);
        },
        error: function (xhr, status, error) {
            console.error("fetchWorkLink failed:", status, error);
            if (retry > 0) {
                console.log("Retrying... attempts left:", retry);
                setTimeout(function () {
                    fetchWorkLink(link, linkRespCallback, retry - 1);
                }, 1000);
            } else {
                console.error("All retries exhausted for:", link);
            }
        }
    });
}

/**
 * Recursively fetch all links in sequence, collecting responses into linkResponseHash.
 * Calls postCallback(linkResponseHash) once all links are fetched.
 * Mirrors fetchAllLinks() in original cashbook.js (line 2801).
 */
function fetchAllLinks(links, lIndex, rCount, linkResponseHash, postCallback) {
    var link = links[lIndex];
    if (link) {
        fetchWorkLink(link, function (r) {
            linkResponseHash[link] = r;
            rCount = rCount + 1;
            if (rCount == links.length - 1) {
                postCallback(linkResponseHash);
            } else {
                fetchAllLinks(links, lIndex + 1, rCount, linkResponseHash, postCallback);
            }
        });
    }
}
