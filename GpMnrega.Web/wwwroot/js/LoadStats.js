/**
 * LoadStats.js — Web Worker for async stats loading
 * Updated for v2: API URLs changed from ../api/*.aspx to /api/proxy/*
 *
 * NOTE: In v2.1, stats are primarily loaded via AJAX in gphome.js
 * (loadStatsAndAutocomplete). This web worker is kept for backward
 * compatibility and can be used as a fallback for heavy data loading.
 */

async function JobCardsDetails(reportData) {
    try {
        var resp = await fetch("/api/proxy/getcatwisejobcards?Panchayat_Code=" + reportData.panchayat_code + "&block_code=" + reportData.block_code + "&dist_code=" + reportData.district_code + "&block_name=" + reportData.blockName + "&dist_name=" + reportData.districtName + "&panchayatname=" + reportData.panchayatName);
        var body = await resp.text();
        return body;
    } catch (e) {
        return '';
    }
}

async function LoadDpr(reportData) {
    try {
        var queryParams = 'state_name=KARNATAKA' +
            '&state_code=15' +
            '&district_name=' + reportData.districtName +
            '&district_code=' + reportData.district_code +
            '&block_name=' + reportData.blockName +
            '&block_code=' + reportData.block_code +
            '&panchayat_name=' + reportData.panchayatName +
            '&panchayat_code=' + reportData.panchayat_code +
            '&fin_year=' + reportData.fincialYear +
            '&short_name=KN' +
            '&work_name=' +
            '&source=' +
            '&Digest=s3kn';
        var resp = await fetch("/api/proxy/getworkdata?" + queryParams);
        var body = await resp.text();
        return body;
    } catch (e) {
        return '';
    }
}

async function LoadHH(reportData) {
    try {
        var resp = await fetch("/api/proxy/jobcardhh?lflag=eng&District_Code=" + reportData.district_code + "&district_name=" + reportData.districtName + "&state_name=KARNATAKA&state_Code=15&block_name=" + reportData.blockName + "&block_code=" + reportData.block_code + "&fin_year=" + (reportData.fincialYear || "2025-2026") + "&check=1&Panchayat_name=" + reportData.panchayatName + "&Panchayat_Code=" + reportData.panchayat_code);
        var body = await resp.text();
        return body;
    } catch (e) {
        return '';
    }
}

async function LoadRegisteredVendor(distCode) {
    try {
        var resp = await fetch("/api/proxy/LoadVendorDetails?dist_code=" + distCode);
        var statebody = await resp.text();
        return statebody;
    } catch (e) {
        return '';
    }
}

onmessage = async function (data) {

    if (data.data.message === "JobCardsDetails") {
        var resp = await JobCardsDetails(data.data.data);
        this.postMessage({ data: resp, message: "JobCardsDetails" });
    }
    else if (data.data.message === "LoadHH") {
        var resp = await LoadHH(data.data.data);
        this.postMessage({ data: resp, message: "LoadHH" });
    }
    else if (data.data.message === "DPRLoad") {
        var dprresp = await LoadDpr(data.data.data);
        this.postMessage({ data: dprresp, message: "DPRLoad" });
    }
    else if (data.data.message === "LoadVendor") {
        var vendor = await LoadRegisteredVendor(data.data.data);
        this.postMessage({ data: vendor, message: "LoadVendor" });
    }
    else
        this.postMessage("No message sent");
}
