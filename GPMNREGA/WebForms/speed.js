var speedData = {
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
    NMRS: []
}
function LoadFirstTimeData() {
    var DPRFrozenUrl = "https://mnregaweb4.nic.in/netnrega/state_html/work_cat_freeze_detail.aspx?state_name=" + speedData.stateName + "&state_code=" + speedData.state_code + "&district_name=" + speedData.districtName + "&district_code=" + speedData.district_code + "&block_name=" + speedData.blockName + "&block_code=" + speedData.block_code + "&panchayat_name=" + speedData.panchayatName + "&panchayat_code=" + speedData.panchayat_code + "&fin_year=" + speedData.fincialYear + "&source=&Digest=s3kn"
    var lastindex = 0;
    $.ajax({
        url: DPRFrozenUrl,
        type: "GET",
        dataType: "html",
        success: function (data) {
            var trs = $(data).find('table')[3].querySelectorAll('tr');
            localStorage['TotalCount'] = trs.length;
            for (i = 1; i < trs.length; i++) {
                if (trs[i].cells[2].innerText.trim() == 'Ongoing') {
                    var a = $(trs[i].cells[1]).find('a')[0].search;
                    $.ajax({
                        url: "https://mnregaweb4.nic.in/netnrega/state_html/DPCApprove_Workdetail.aspx" + a,
                        type: "GET",
                        dataType: "html",
                        success: function (respnse) {
                            for (i = 0; i < html.length; i++) {
                                if (html[i].id.indexOf("ContentPlaceHolder1_catlbl")
                                    speedData.workCategory  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_ExeLevel_text")
                                    speedData.executionLevel  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_lbl_agency_text")
                                    speedData.executionAgency  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_LblWrksdate")
                                    speedData.startdate  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_lblsanctionno_text")
                                    speedData.techSanctionNo  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_lblsandate_text")
                                    speedData.techSanctionDate  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_lblworkstatus_text")
                                    speedData.workStatus  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_lblSanc_Tech_Labr_Unskilled")
                                    speedData.UskilledExp  = html[i].innerText;
                                if (html[i].id.indexOf("ContentPlaceHolder1_Lblfin_total")
                                    speedData.workCostTotal = html[i].innerText;
                                if (html[i].id == "ContentPlaceHolder1_Lblsanc_fin_no")
                                    speedData.finSanctionNo = html[i].innerText;
                                if (html[i].id == "ContentPlaceHolder1_Lblsanc_fin_dt")
                                    speedData.finSanctionDate = html[i].innerText;
                                if (html[i].id == "ContentPlaceHolder1_lblfin_text")
                                    speedData.workYear = html[i].innerText;
                                if (html[i].id == "ContentPlaceHolder1_lblEst_Cost_Material")
                                    speedData.MaterialCost = html[i].innerText;
                                if (html[i].id == "ContentPlaceHolder1_Lblskill")
                                    speedData.SkilledCost = html[i].innerText;
                                if (html[i].id == "ContentPlaceHolder1_wrklbl") {
                                    startIdx = html[i].innerText.indexOf("(");
                                    speedData.workcode = html[i].innerText.substr(0, startIdx - 1);
                                }
                                if (html[i].id == "ContentPlaceHolder1_wrklbl") {
                                    startIdx = html[i].innerText.indexOf("(");
                                    speedData.workName = html[i].innerText.substr(startIdx + 1, html[i].innerText.length - (startIdx + 2))
                                }
                            }

                        },
                        error: function (error) {
                           
                        }
                    });
                }
            }
        },
        error: function (error) {
            
        }
    });
}

function LoadRemainingdata(index) {

}