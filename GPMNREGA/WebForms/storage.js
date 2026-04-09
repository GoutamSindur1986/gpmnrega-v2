//Copy right S3KN Software solutions

workData = {
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
    UskilledExp: '',
    MaterialCost: '',
    SkilledCost: ''
}
gpData = {
    userName: '',
    stateName: '',
    stateNameRegional: '',
    districtName: '',
    districtNameRegional: '',
    blockName: '',
    blockNameRegional: '',
    panchayat_NameRegional: '',
    panchayatName: '',
    state_shortname: '',
    state_code: '',
    district_code: '',
    block_code: '',
    panchayat_code: '',
    LokSabha: '',
    VidhanSabha: '',
                     
}

function addWorkDataToLocalStorage(workcode, workdata) {
    localStorage.setItem(workcode, workdata);
}

function readWorkData(workcode) {
    if (localStorage[workcode] !== null && localStorage[workcode] !== undefined) {
        _workdata = JSON.parse(localStorage[workcode]);

        reportData.workCategory = workData.workCategory = _workdata.workCategory;

        reportData.executionLevel = workData.executionLevel = _workdata.executionLevel;

        reportData.executionAgency = workData.executionAgency = _workdata.executionAgency;

        reportData.startdate = workData.startdate = _workdata.startdate;

        reportData.techSanctionNo = workData.techSanctionNo = _workdata.techSanctionNo;

        reportData.techSanctionDate = workData.techSanctionDate = _workdata.techSanctionDate;

        reportData.workStatus = workData.workStatus = _workdata.workStatus;

        reportData.UskilledExp = workData.UskilledExp = _workdata.UskilledExp;

        reportData.workCostTotal = workData.workCostTotal = _workdata.workCostTotal;

        reportData.finSanctionNo = workData.finSanctionNo = _workdata.finSanctionNo;

        reportData.finSanctionDate = workData.finSanctionDate = _workdata.finSanctionDate;

        reportData.workYear = workData.workYear = _workdata.workYear;

        reportData.MaterialCost = workData.MaterialCost = _workdata.MaterialCost;

        reportData.SkilledCost = workData.SkilledCost = _workdata.SkilledCost;

        reportData.workcode = workData.workcode = _workdata.workcode;

        reportData.workName = workData.workName = _workdata.workName;

        reportData.NMRS = _workdata.NMRS;

        reportData.panchayatName = _workdata.panchayatName;
        reportData.panchayat_code = _workdata.panchayat_code;
        reportData.panchayat_NameRegional = _workdata.panchayat_NameRegional;

        bindWorkData();
        bindNMRs();
    }
    if(typeof reportData.LineDeptName === "undefined"){
     searchTextCode($('#txtSearchWorkCode').val().trim());
    }
    else {
        searchWorkCode();
    }

}




