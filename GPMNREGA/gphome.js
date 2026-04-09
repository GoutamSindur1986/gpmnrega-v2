var WageList = '';
var FTO = '';
var downloadFtoBtn = false;
var jobCardsHH = {};
var statworker;
//<input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="blknmr" value="blank nmr" title="Blnak NMR" style="margin-left:1%;width:85px" />
//helper class
function check_cookie_name(name) {
    var match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    if (match) {
        return match[2]
    }
    else {
        return ('--something went wrong---');
    }
}
//hepler class end

window.addEventListener("message", function (evt) {
    if (evt.data.task === 'dataready') {
        buildBlankNMR(evt.data.jcAccData, evt.data.WkJc, evt.data.otherData);
    }
    else if (evt.data.task == 'Failed') {
        $('#progressBar').hide();
        $('#imgDown').hide();
        alert('Failed to load blank NMR please try again. Make sure you are logged into DEO Nrega site and not closing background page');
    }
    else {
        return;
    }
});
var form6rows = `<tr>
    <td><span style="font-size: 16px;"> @SINO </span></td>
    <td><span style="font-family: tunga; font-size: 16px;"> @ApplicantName </span></td>
    <td><span style="font-family: tunga; font-size: 16px;"> @Address </span></td>
    <td><span style="font-size: 16px;"> @JobCardNo </span></td>
    <td><span style="font-size: 16px;"> @FromDate </span></td>
    <td><span style="font-size: 16px;"> @toDate </span></td>
    <td><span style="font-size: 16px;">  </span></td>
    <td><span style="font-size: 16px;">  </span></td>
</tr>`;

var nmrtablerow = `<li class="nmr-item">
    <div class="nmr-meta">
        <span>NMR No: @nmrNo</span>
        <img src="../Content/download-arrow.jpg" class="nmr-download workbtn" style="background:transparent;border:none;" data-link="@nmrLink" onclick="nmrBtnClicked(this)" name="filledNmr" value="filledNMR" title="Download Filled NMR" />
    </div>
    <div class="nmr-actions">
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form6" value="Form 6" title="Form 6" style="width:85px" />
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form8" value="Form 8" title="Form 8" style="width:85px" />
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form8+" value="Form 8+" title="Form 8+" style="width:85px" />
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="Form9" value="Form 9" title="Form 9" style="width:85px" />
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="blknmr" value="blank nmr" title="Blank NMR" style="width:85px" />
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="WageList" value="Wage List" title="Wage List" style="width:85px" />
        <input type="button" data-link="@nmrLink" data-nmrno="@nmrNo" data-index="@Index" onclick="nmrBtnClicked(this)" class="workbtn" name="FTO" value="FTO" title="FTO" style="width:85px" />
    </div>
</li>`;

form8footer = ` </form>
</body>
</html>`;

var _pageDigest = 'S3KN';
var resultsFound = false;
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
    NMRS: [],
    Material: { Materials: [] },
    AssetLink: ''
}

$(document).ready(function () {

    var images = ['gandhi2.jpg', 'gandhi3.jpg', 'gandhi4.png', 'gandhi5.jpg', 'gandhiimg.JPG'];
    $('#resultPanel').css({ 'background-image': 'url(../templates/homebgimgs/' + images[Math.floor(Math.random() * images.length)] + ')' });
    //Load user data
    loadData();

    $("#btnSearch").click(function (e) {
        $('#tblWorkInfo').hide();
        $('#tblNMR').html('');
        if (reportData.panchayat_code === findgp())
            searchTextCode($('#txtSearchWorkCode').val().trim().toUpperCase())
        else
            alert('Wrong gp.');

        e.preventDefault();
        return false;
    });

    reportData.fincialYear = $('#ddlyear :selected').text().trim();
    $('#spnfinyear').text($('#ddlyear :selected').text().trim());
    $('#spnSearchPanchyat').text(reportData.panchayatName);

    $("#ddlyear").change(function () {
        reportData.fincialYear = workData.fincialYear = $('option:selected', this).text().trim();
        $('#spnfinyear').text(reportData.fincialYear);
    });

    $('.workbtn').click(function (elm) {
        $('#progressBar').show();
        $('#imgDown').show();
        downloadPage(elm);
    });
    var JobCardWebWorker;
    if (localStorage["dateDataLoaded"] === "undefined" || localStorage["dateDataLoaded"] === undefined) {
        localStorage["dateDataLoaded"] = new Date();
        if (typeof (Worker) !== 'undefined' || typeof (Worker) !== undefined) {
            if (typeof (JobCardWebWorker) === 'undefined' || typeof (JobCardWebWorker) === undefined) {
                var allcomplete = 0;
                JobCardWebWorker = new Worker("../Scripts/LoadStats.js");
                JobCardWebWorker.postMessage({ data: reportData, message: "DPRLoad" });
                JobCardWebWorker.postMessage({ data: reportData, message: "LoadHH" });
                JobCardWebWorker.postMessage({ data: reportData, message: "JobCardsDetails" });
                JobCardWebWorker.postMessage({ data: reportData.district_code, message: "LoadVendor" });

                JobCardWebWorker.onmessage = function (event) {
                    if (event.data.message === "DPRLoad") {
                        if (event.data.data !== '') {
                            addDPRFReport(reportData.panchayat_code, event.data.data);

                            allcomplete += 1;
                        }
                        else {
                            localStorage["dateDataLoaded"] = undefined;
                            alert('Error Loading Work Details Please reload the page after sometime. Check NREGA Report site')
                        }
                    }
                    else if (event.data.message === "JobCardsDetails") {
                        if (event.data.data !== '') {
                            localStorage["JobCardsDetails"] = event.data.data;
                            JobCardsDetails(event.data.data);
                            allcomplete += 1;
                        }
                        else {
                            localStorage["dateDataLoaded"] = undefined;
                            alert('Error Loading Work Details Please reload the page after sometime. Check NREGA Report site')
                        }
                    }
                    else if (event.data.message === "LoadHH") {
                        if (event.data.data !== '') {
                            localStorage["LoadHH"] = event.data.data;
                            loadHH(event.data.data);
                            allcomplete += 1;
                        }
                        else {
                            localStorage["dateDataLoaded"] = undefined;
                            alert('Error Loading Work Details Please reload the page after sometime. Check NREGA Report site')
                        }
                    }
                    else if (event.data.message === "LoadVendor") {
                        if (event.data.data !== '') {
                            localStorage["LocalVendor"] = event.data.data;

                        }
                        else {

                            alert('Error Loading Work Details Please reload the page after sometime. Check NREGA Report site')
                        }
                    }
                    if (allcomplete === 3) {
                        JobCardWebWorker.terminate();
                        JobCardWebWorker = undefined;
                    }
                }
                JobCardWebWorker.onerror = function (error) {
                    console.log(error);
                    JobCardWebWorker.terminate();
                    JobCardWebWorker = undefined;
                }
            }

        } else {
            alert('Unable to load data please try again');
            JobCardWebWorker.terminate();
            JobCardWebWorker = undefined;
        }
    }
    else {
        if (localStorage["DPRLoad"] !== undefined)
            readDPRStatus(reportData.panchayat_code.toLowerCase());
        if (localStorage["LoadHH"] !== undefined)
            loadHH(localStorage["LoadHH"])
        if (localStorage["JobCardsDetails"] !== undefined)
            JobCardsDetails(localStorage["JobCardsDetails"])

        date1 = new Date(localStorage["dateDataLoaded"])
        date2 = new Date();
        var hours = Math.abs(date1 - date2) / 36e5;
        if (hours > 24.0) {
            localStorage["dateDataLoaded"] = undefined;
            var req = indexedDB.deleteDatabase("DPRFTable");
            req.onsuccess = function () {
                console.log('deleted old database.')
            }
            req.onerror = function (error) {
                console.log(error);
            }
        }
    }
});

var DPRresponse = {};
function loadDPRWorkLinks(response) {

    DPRresponse = JSON.parse(response);
    bindautocomplete(DPRresponse);
}

//Work code class
function searchTextCode(data) {
    //set default.................
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
    reportData.AssetLink = '';

    //end set default

    $('#imgSearch').show();
    $('#tblNMR').innerHTML = "";
    try {
        $.ajax({
            url: "https://datasync.s3kn.com/api/getworkdata?distcode=" + reportData.district_code + "&gpcode=" + reportData.panchayat_code + "&workcode=" + data,
            type: "GET",
            success: function (respnse) {
                parseWorkCodeResponse(respnse, data);
            },
            error: function (error) {
                $('#imgSearch').hide();
                if (!resultsFound) {
                    $('#imgResNotFound').show();
                    setTimeout(function () { $('#imgResNotFound').hide(); }, 2000);
                }
            }
        });
    } catch (e) {
        console.log(JSON.stringify(e));
    }


}

function parseWorkCodeResponse(response, data) {
    try {
        var wd, nmr, wdetails;
        var b = JSON.parse(JSON.stringify(response));
        wd = JSON.parse(b);

        if (wd.WorkData.length !== 0) {
            wdetails = JSON.parse(wd.WorkData[0].WorkJson.replaceAll("'", '"').replace(/\n/g, '').replace(/\t/g, '').replace(/\\/g, ''));
            if (wdetails.executionAgency.indexOf("Panchayat") == -1) {
                alert("ಈ ವರ್ಕ್ ಕೋಡ್ ಲೈನ್ ಡಿಪಾರ್ಟ್ಮೆಂಟ್ ಸೇರಿದೆ.");
                $('#imgSearch').hide();
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

                reportData.Material = wdetails.Material;

                if (wdetails.AssetLink !== null && wdetails.AssetLink !== undefined && wdetails.AssetLink !== 'undefined' && wdetails.AssetLink !== "") {
                    const urlParams = new URLSearchParams(wdetails.AssetLink);
                    wsStatus = urlParams.get('ws');
                    if (wsStatus.trim() === 'Approved')
                        reportData.AssetLink = ''
                    else
                        reportData.AssetLink = wdetails.AssetLink
                }

                bindWorkData();
                checkNmrwithAsset(reportData, wd);
            }
        }
        else {
            try {
                $('#imgSearch').show();
                if (data.indexOf('/IAY/') > 0 || data.indexOf('/gpmnrega')>0) {
                    reportData.workcode = data;
                    loadNMR(data);
                }
                else {
                    $.ajax({
                        url: "../api/wagelistdata?dpr=true",
                        type: "POST",
                        data: DPRresponse[data][2],
                        dataType: "html",
                        success: function (response) {

                            Bind_UpdateWorkDetails(response, reportData, data);

                        },
                        error: function (error) {
                            $('#imgSearch').hide();
                            if (!resultsFound) {
                                console.log(error);
                                $('#imgResNotFound').show();
                                setTimeout(function () { $('#imgResNotFound').hide(); }, 2000);
                            }
                        }
                    });
                }

            } catch (e) {
                $('#imgSearch').hide();
                alert("Unable to fetch work data: Main server running slow.")
            }
        }



        $('#imgSearch').hide();
    } catch (e) {
        console.log(`${e}`);
        $('#imgSearch').hide();
    }
}


function checkNmrwithAsset(BetaData, wd) {

    if (wd.MsrData.length !== 0) {
        reportData.NMRS = [];
        wd.MsrData.forEach((e, i) => {
            let nmrdata = JSON.parse(e.MsrJson.replaceAll("'", '"').replace(/[\n\r\t]/g, ''));
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
    if (reportData.AssetLink !== '') {
        BetaData["AssetLink"] = reportData.AssetLink;
        loadNMR(BetaData);
        bindNMRs();

    }
    else {
        BetaData["AssetLink"] = '';
        loadNMR(BetaData);
        bindNMRs();
    }

}
function loadNMR(BetaData) {
    var str = '';

    if (BetaData.AssetLink === null || BetaData.AssetLink === '' || BetaData.AssetLink === undefined || BetaData.AssetLink === 'undefined') {
        str = "https://nregastrep.nic.in/netnrega/citizen_html/workasset.aspx?block_code=" + reportData.block_code + "&Panchayat_Code=" + reportData.panchayat_code + "&wkcode=" + reportData.workcode + "&state_name=" + reportData.stateName + "&district_name=" + reportData.districtName + "&block_name=" + reportData.blockName + "&panchayat_name=" + reportData.panchayatName + "&Digest=s3kn"
        $.ajax({
            url: "../api/getworkasset?block_code=" + reportData.block_code + "&Panchayat_Code=" + reportData.panchayat_code + "&wkcode=" + reportData.workcode + "&state_name=" + reportData.stateName + "&district_name=" + reportData.districtName + "&block_name=" + reportData.blockName + "&panchayat_name=" + reportData.panchayatName + "&District_code=" + reportData.district_code,
            type: "POST",
            dataType: "html",
            timeout: 900000,
            data: encodeURI(str),
            success: function (response) {

                respnse = response.split('`')[0];
                reportData.AssetLink = response.split('`')[1];
                if (reportData.workcode.indexOf('/IAY/') > 0)
                    PMIAY(respnse,BetaData);

                bindWorkData();
                parseWorkNmrs(respnse);
            },
            error: function (error) {
                console.log(`Error ${error}`);
            }
        });
    } else {
        try {

            str = BetaData.AssetLink;
            $.ajax({
                url: "../api/wagelistdata",
                type: "post",
                dataType: "html",
                data: BetaData.AssetLink,
                timeout: 900000,
                success: function (respnse) {
                    parseWorkNmrs(respnse);
                },
                error: function (error) {
                    console.log(`Error ${error}`);
                }
            });

        } catch (e) {

        }

    }
}

function parseWorkNmrs(response) {
    try {


        var loadedNmrs = [];

        for (k = 0; k < reportData.NMRS.length; k++) {
            loadedNmrs.push(reportData.NMRS[k].NMRNO);
        }

        var html = $(response).find('a').filter(function (i, a) {
            const urlParams = new URLSearchParams(a.href);
            if (urlParams.get('msrno') !== null && typeof (urlParams.get('msrno')) !== "undefined") {
                var loadIndex = loadedNmrs.indexOf(urlParams.get('msrno').trim());
                if (loadIndex !== -1) {
                    if (reportData.NMRS[loadIndex] !== null && reportData.NMRS[loadIndex] !== undefined && reportData.NMRS[loadIndex] !== 'undefined') {
                        reportData.NMRS[loadIndex].url = "https://mnregaweb4.nic.in/netnrega/citizen_html/Musternew.aspx" + a.search

                        $.ajax({
                            type: 'post',
                            url: "https://datasync.s3kn.com/api/msrupdate?distcode=" + reportData.district_code + "&msrno=" + urlParams.get('msrno').trim() + "&workcode=" + decodeURI(reportData.workcode.toUpperCase()),
                            contentType: 'application/json',
                            data: "\"" + JSON.stringify(reportData.NMRS[loadIndex]).replaceAll('"', "'") + "\"",
                            success: function (s, r, j) {
                                console.log(s)
                            },
                            error: function (error) {
                                console.log(error);
                            }
                        });
                    }
                    else {

                    }
                }
                else if (loadedNmrs.indexOf(urlParams.get('msrno').trim()) == -1) {
                    loadedNmrs.push(urlParams.get('msrno').trim());
                    return a;
                }
            }
        });

        if (html.length == 0) {
            console.log('No Delta filled')
        }
        else {
            for (i = 0; i < html.length; i++) {
                $.ajax({
                    url: "../api/getnmrdata",
                    type: "POST",
                    dataType: "html",
                    timeout: 900000,
                    data: "https://mnregaweb4.nic.in/netnrega/citizen_html/Musternew.aspx" + html[i].search,
                    success: function (respnse) {
                        var form = $(respnse).filter((i, e) => e.id == 'form1');
                        if (form.length == 0)
                            form = $(respnse).filter((i, e) => e.id == 'aspnetForm');
                        const urlParams = new URLSearchParams(form[0].action);
                        fin_year = urlParams.get('finyear');
                        var startDate = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_lbldatefrom")[0].innerText;
                        var endDate = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_lbldateto")[0].innerText;
                        var msrno = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_lblMsrNo2")[0].innerText;
                        var JC = [];
                        var table = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_grdShowRecords tr");
                        var nmrbcs = table[1].cells[1].querySelectorAll('a')[0].innerText.split('-');
                        var nmrbccode = '15' + nmrbcs[1] + nmrbcs[2];
                        var wageListId = form[0].querySelectorAll("#ctl00_ContentPlaceHolder1_grdShowRecords tr")[0].cells.length - 5;
                        var BankName = form[0].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr')[0].cells.length - 8;
                        var dateCreated = form[0].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr')[0].cells.length - 3;

                        for (j = 1; j < table.length - 1; j++) {
                            var applicantname; //= table[i].cells[1].innerHTML.split('<br>')[0].indexOf('(') != -1 ? table[i].cells[1].innerHTML.split('<br>')[0].substring(0, table[i].cells[1].innerHTML.split('<br>')[0].indexOf('(')) : table[i].cells[1].innerHTML.split('<br>')[0].replace('<font face="Verdana" color="#284775" size="2">',;

                            var name = table[j].cells[1].innerHTML.split('<br>')[0];
                            var applicantname = name.substr('<font face="Verdana" color="#284775" size="2">'.length, name.length - '<font face="Verdana" color="#284775" size="2">'.length);
                            applicantname = applicantname.indexOf('(') != -1 ? applicantname.substring(0, applicantname.indexOf('(')).trim() : applicantname.trim();
                            applicantname = applicantname.replace('\\', '');
                            JC.push({ 'JCNO': table[j].cells[1].querySelectorAll('a')[0].innerText.trim(), 'appName': applicantname, 'appAddress': table[j].cells[3].textContent.trim(), 'category': table[j].cells[2].innerText.length > 3 ? table[j].cells[2].innerText.substr(0, 3) : table[j].cells[2].innerText, 'wageList': table[j].cells[wageListId].textContent.trim(), 'BankName': table[j].cells[BankName].textContent.trim(), 'DateCreated': table[j].cells[dateCreated].textContent.trim() });
                        }
                        reportData.NMRS.push({
                            'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate, 'url': this.data, 'JC': JC
                        });

                        var innerhtml = nmrtablerow.replaceAll("@nmrNo", msrno).replaceAll("@Index", i).replaceAll("@nmrLink", this.data);

                        $('#tblNMR').append(innerhtml);

                        $.ajax({
                            type: 'post',
                            url: "https://datasync.s3kn.com/api/msrdata?distcode=" + reportData.district_code + "&msrno=" + msrno + "&workcode=" + decodeURI(reportData.workcode.toUpperCase()),
                            contentType: 'application/json',
                            data: "\"" + (JSON.stringify({
                                'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate, 'url': this.data, 'JC': JC
                            })).replaceAll('"', "'") + "\"",
                            success: function (s, r, j) { },
                            error: function (error) { }
                        });
                    },
                    error: function (error) {
                        console.log(`Error ${error}`);
                    }
                });

            }
        }
        //calling delta issue after delta filled and db nmrs loaded;
        var lmr = "";
        for (var i = 0; i < loadedNmrs.length; i++) {
            lmr += loadedNmrs[i] + ",";
        }
        checkdelta(lmr);


    } catch (e) {
        console.log(`${e}`);
    }
}
//Work code class 

function bindNMRs() {
    $('#tblNMR').innerHTML = "";
    var innerhtm = "";
    for (i = 0; i < reportData.NMRS.length; i++) {
        innerhtm += nmrtablerow.replaceAll("@nmrNo", reportData.NMRS[i].NMRNO).replaceAll("@Index", i).replaceAll("@nmrLink", reportData.NMRS[i].url);
    }
    $('#tblNMR').html(innerhtm);

}

function loadData() {

    var loggedinUser;

    loggedinUser = check_cookie_name('UserData').split('@');

    for (i = 0; i < loggedinUser.length; i++) {

        if (loggedinUser[i].split(':')[0] == "UserName")
            reportData.userName = gpData.userName = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "PanchyatCode")
            reportData.panchayat_code = gpData.panchayat_code = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "PanchyatName")
            reportData.panchayatName = gpData.panchayatName = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "PanchayatNameRegional")
            reportData.panchayat_NameRegional = gpData.panchayat_NameRegional = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "TalukName")
            reportData.blockName = gpData.blockName = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "TalukCode")
            reportData.block_code = gpData.block_code = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "DistrictName")
            reportData.districtName = gpData.districtName = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "DistrictCode")
            reportData.district_code = gpData.district_code = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "StateName")
            reportData.stateName = gpData.stateName = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "StateCode")
            reportData.state_code = gpData.state_code = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "short_name")
            reportData.state_shortname = gpData.state_shortname = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "DistrictNameRegional")
            reportData.districtNameRegional = gpData.districtNameRegional = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "StateNameRegional")
            reportData.stateNameRegional = gpData.stateNameRegional = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "TalukNameRegional")
            reportData.blockNameRegional = gpData.blockNameRegional = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "LokSabhaRegional")
            reportData.LokSabha = gpData.LokSabha = loggedinUser[i].split(':')[1].trim();
        if (loggedinUser[i].split(':')[0] == "vidhanSabhaRegional")
            reportData.VidhanSabha = gpData.VidhanSabha = loggedinUser[i].split(':')[1].trim();

    }
}

function nmrBtnClicked(btn) {
    $('#progressBar').show();
    $('#imgDown').show();
    downloadPage(btn);
}

function checkdelta(nmrLoaded) {

    var deltaexists = false;
    var url = "../templates/deltacheck?state_code=" + reportData.state_code + "&dist_code=" + reportData.district_code + "&block_code=" + reportData.block_code + "&panch_code=" + reportData.panchayat_code + "&fin_year=" + reportData.fincialYear + "&work_code=" + $('#txtSearchWorkCode').val().trim().toLowerCase() + "&state_name=" + reportData.stateName + "&dist_name=" + reportData.districtName + "&block_name=" + reportData.blockName + "&panch_name=" + reportData.panchayatName;
    $.ajax({
        url: url,
        type: "POST",
        dataType: "text",
        data: nmrLoaded,
        success: function (data) {
            var delta = data.split('UpdatePanel2|\r\n');
            var innerhtm = "";

            if (delta.length > 1) {
                for (i = 1; i < delta.length; i++) {
                    var msrno = $(delta[i])[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lblMsrNo2')[0].innerText;

                    if (reportData.NMRS.filter(e => e.NMRNO == msrno).length == 0) {
                        deltaexists = true;
                        var startDate = $(delta[i])[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lbldatefrom')[0].innerText;
                        var endDate = $(delta[i])[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lbldateto')[0].innerText;

                        var JC = [];
                        var table = $(delta[i])[2].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr');
                        var nmrbcs = table[1].cells[1].querySelectorAll('a')[0].innerText.split('-');
                        var nmrbccode = '15' + nmrbcs[1] + nmrbcs[2];
                        var wageListId = $(delta[i])[2].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr')[0].cells.length - 5;
                        var BankName = $(delta[i])[2].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr')[0].cells.length - 8;
                        var dateCreated = $(delta[i])[2].querySelectorAll('table#ctl00_ContentPlaceHolder1_grdShowRecords tr')[0].cells.length - 3;
                        for (j = 1; j < table.length - 1; j++) {
                            var applicantname = table[j].cells[1].innerHTML.split('<br>')[0].indexOf('(') != -1 ? table[j].cells[1].innerHTML.split('<br>')[0].substring(0, table[j].cells[1].innerHTML.split('<br>')[0].indexOf('(')) : table[j].cells[1].innerHTML.split('<br>')[0];
                            var appBankName = table[j].cells[BankName].textContent.trim();
                            dateWageCreated = table[j].cells[dateCreated].textContent.trim();
                            JC.push({ 'JCNO': table[j].cells[1].querySelectorAll('a')[0].innerText.trim(), 'appName': applicantname, 'appAddress': table[j].cells[3].textContent.trim(), 'category': table[j].cells[2].innerText.length > 3 ? table[j].cells[2].innerText.substr(0, 3) : table[j].cells[2].innerText, 'wageList': table[j].cells[wageListId].textContent.trim(), 'BankName': appBankName, 'DateCreated': dateWageCreated });
                        }

                        innerhtm += nmrtablerow.replaceAll("@nmrNo", msrno).replaceAll("@Index", i).replaceAll("@nmrLink", this.data);

                        reportData.NMRS.unshift({ 'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate, 'url': this.data, 'JC': JC });

                        $.ajax({
                            type: 'post',
                            url: "https://datasync.s3kn.com/api/msrdata?distcode=" + reportData.district_code + "&msrno=" + msrno + "&workcode=" + decodeURI(reportData.workcode.toUpperCase()),
                            contentType: 'application/json',
                            data: "\"" + (JSON.stringify({
                                'NMRNO': msrno, 'DateFrom': startDate, 'DateTo': endDate, 'url': '', 'JC': JC
                            })).replaceAll('"', "'") + "\"",
                            success: function (s, r, j) { },
                            error: function (error) { }
                        });
                    }

                }
                if (deltaexists) {
                    $("#tblNMR").prepend(innerhtm);
                }

            }

        },
        error: function (error) {

        }
    });
}

function Bind_UpdateWorkDetails(response, BetaData, data) {

    try {
        var form = $(response).filter((i, e) => e.id == 'form1');

        if (form.length === 0)
            form = $(response).filter((i, e) => e.id == 'aspnetForm');

        if (form[0].querySelector("#ctl00_ContentPlaceHolder1_lbl_agency_text").innerText.indexOf("Panchayat") < 0) {

            alert("ಈ ವರ್ಕ್ ಕೋಡ್ ಲೈನ್ ಡಿಪಾರ್ಟ್ಮೆಂಟ್ ಸೇರಿದೆ.");
            $('#imgSearch').hide();
            if (!resultsFound) {
                $('#imgResNotFound').show();
                setTimeout(function () { $('#imgResNotFound').hide(); }, 2000);
            }
        }
        else {

            if (form.length == 0) {
                $('#imgSearch').hide();
                alert('ದಯವಿಟ್ಟು ವರ್ಕ್‌ಕೋಡ್ ಮತ್ತು ನೆಟ್‌ವರ್ಕ್ ಅನ್ನು ಪರಿಶೀಲಿಸಿ.')

            }
            else {
                reportData.workName = BetaData['workName'] = DPRresponse[data][0].replaceAll('"', '');

                reportData.startdate = BetaData['startDate'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_LblWrksdate').text().trim().trim();

                reportData.workCategory = BetaData['workCategory'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_catlbl').text().trim().trim();

                reportData.workYear = BetaData['workYear'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblfin_text').text().trim().trim();

                reportData.workCostTotal = BetaData['workCostTotal'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblfin_total').text().trim().trim();

                reportData.executionAgency = BetaData['executionAgency'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lbl_agency_text').text().trim().trim();

                reportData.executionLevel = BetaData['executionLevel'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_ExeLevel_text').text().trim();

                reportData.workStatus = BetaData['workStatus'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblworkstatus_text').text().trim();

                reportData.finSanctionNo = BetaData['finSanctionNo'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblsanc_fin_no').text().trim();

                reportData.finSanctionDate = BetaData['finSanctionDate'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblsanc_fin_dt').text().trim();

                reportData.techSanctionNo = BetaData['techSanctionNo'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblsanctionno_text').text().trim();

                reportData.techSanctionDate = BetaData['techSanctionDate'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblsandate_text').text().trim();

                BetaData['panchayat_code'] = reportData.panchayat_code;

                reportData.UskilledExp = BetaData['UskilledExp'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblSanc_Tech_Labr_Unskilled').text().trim();

                reportData.MaterialCost = BetaData['MaterialCost'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_lblEst_Cost_Material').text().trim();

                reportData.SkilledCost = BetaData['SkilledCost'] = $(form[0]).find('#ctl00_ContentPlaceHolder1_Lblskill').text().trim();

                reportData.workcode = BetaData['workCode'] = data;



                $(form[0]).find('#ctl00_ContentPlaceHolder1_GridView1 tr').each((i, e) => {
                    if (i !== 0) {
                        try {
                            reportData.Material.push({
                                'Material': e.cells[1].innerText.trim().replaceAll('"', '').replaceAll("'", ''),
                                'Quantity': e.cells[2].innerText.trim(),
                                //'BalanceQuantity': e.cells[3].innerText.trim(), 
                                'UnitPrice': e.cells[3].innerText.trim(),
                                'Total': e.cells[4].innerText.trim()
                            })
                        }
                        catch (err) {
                            console.log('error in material list workcode:' + data);
                        }
                    }
                });

                loadNMR(reportData);
            }
        }
    } catch (e) {

        console.log(`${e}`);
    }
}

function bindWorkData() {
    $('#wrkName').text(reportData.workName);
    $('#wrkCode').text(reportData.workcode);
    $('#wrkCategory').text(reportData.workCategory);
    $('#exeAgency').text(reportData.executionAgency);
    $('#finNoDate').text(reportData.finSanctionNo + " & " + reportData.finSanctionDate);
    $('#techNoDate').text(reportData.techSanctionNo + " & " + reportData.techSanctionDate);
    $('#tblWorkInfo').show();
    $('#btnLoadIssued').hide();
    $('#imgfilled').show();
    if ($("#tblWorkInfo").is(":visible")) {
        $('#resultPanel').css({ 'background-image': '' });
    }

    $.ajax({
        type: 'post',
        url: "https://datasync.s3kn.com/api/workdata?distcode=" + reportData.district_code + "&gpcode=" + reportData.panchayat_code + "&workcode=" + reportData.workcode.toUpperCase(),
        contentType: 'application/json',
        data: "\"" + (JSON.stringify(reportData)).replaceAll('"', "'") + "\"",
        success: function (s, r, j) { console.log("data loaded") },
        error: function (error) { console.log("Error while loading workcode details...") }
    });

    $('#imgSearch').hide();
}

function downloadPage(elm) {
    try {
        var strurl;
        var validForms = false;

        if (elm.name == "filledNmr" || elm.name == "Form6" || elm.name == "Form9" || elm.name == "Form8" || elm.name == "Form8+" || elm.name == "FTO" || elm.name == "WageList" || elm.name == "blknmr") {

            //strurl = "/templates/converter?page=" + elm.name + "&FormIndex=" + elm.dataset.index;
            validForms = false;
        }
        else {
            if (elm.currentTarget.name == "geotag") {
                validForms = false;
            }
            else if (elm.currentTarget.name == "checklist") {
                validForms = false;
                generatechecklist();
                return;
            }
            else if (elm.currentTarget.name == "musterrolemov") {
                generateMusterRollMoventSlip()
            }
            else {
                strurl = "/templates/" + reportData.stateName + "/" + elm.currentTarget.name;
                validForms = true;
                reportData.FormHTML = '';
            }
        }

        if (validForms) {
            var settings = {
                "url": strurl,
                "method": "POST",
                "timeout": 0,
                "headers": {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                contentType: "application/x-www-form-urlencoded",
                data: $.param(reportData),
                xhrFields: {
                    responseType: 'html'
                }
            }
            $.ajax(settings).done(function (html, status, xhr) {
                try {
                    downloadForms(html, elm.currentTarget.name);

                } catch (e) {
                    console.log(`Error ${e}`);
                    $('#imgDown').hide();
                    $('#downFail').show();
                    setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
                }
            }).fail(function (error) {
                console.log(`Error ${error}`);
                $('#imgDown').hide();
                $('#downFail').show();
                setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
            }).always(function () {
                //$('#imgDown').hide();
                $('#downFail').hide();

            });
        }
        if (elm.name == "Form6") {
            generateform6(elm);

        }
        if (elm.name == "Form8") {
            genereateform8(elm);
        }

        if (elm.name == "Form8+") {
            generate8plus(elm);
        }
        if (elm.name == "Form9") {
            generateform9(elm);
        }
        if (elm.name == "blknmr") {

            let person = prompt("Please enter Name of technical staff responsible for measurement:", " ");
            if (person == null || person == " ") {
                alert("Name of technical staff responsible for measurement is required.");
                $('#imgDown').hide();
            } else {
                reportData.technicalstaff = person.toUpperCase();
                // urlParams = new URLSearchParams(elm.dataset.link);
                loadNMRDataForFormblk(elm);
            }


        }
        if (elm.name == "FTO") {
            loadWageListData(elm);
        }
        if (elm.name == "WageList") {
            loadWageListData(elm);
        }
        if (elm.name === "filledNmr") {
            printFilledNMR(elm);
        }
        if (elm.currentTarget !== null && elm.currentTarget !== undefined)
            if (elm.currentTarget.name === "geotag") {
                $.ajax({
                    url: "../templates/geotag?workcode=" + reportData.workcode + "&district_code=" + reportData.district_code + "&block_code=" + reportData.block_code + "&panchayat_code=" + reportData.panchayat_code + "&bn=" + reportData.blockName + "&pn=" + reportData.panchayatName + "&ds=" + reportData.districtName + "&fin=" + reportData.workYear + "&WorkName=" + reportData.workName,
                    type: 'GET',
                    success: function (resp) {
                        generategeotag(resp);
                    },
                    error: function (error) {

                    }
                });
            }
    } catch (e) {
        $('#imgDown').hide();
    }
}

function downloadForms(html, form) {

    strurl = "/templates/converter?page=" + form + "&stateName=" + reportData.stateName + "&workcode=" + reportData.workcode + "&WageList=" + WageList + "&FTO=" + FTO;
    $.ajax({
        url: strurl,
        type: "post",
        data: html,
        contentType: "application/x-www-form-urlencoded",
        xhrFields: {
            responseType: 'blob'
        },
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
                $('#imgDown').hide();
                $('#downSuc').show();
                setTimeout(function () { $('#downSuc').hide(); $('#progressBar').hide(); }, 3000);


            } catch (e) {
                alet("Error occured while downloading form. Please try again sometime later.")
                $('#imgDown').hide();
                setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
                console.log(e);
            }

        },
        error: function (error) {
            $('#imgDown').hide();
            setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
            console.log(error);
        }
    });

}

function generateNewPdf(docDefinition, fileName) {
    pdfMake.fonts = {
        tunga: {
            normal: 'tunga.ttf',
            bold: 'tungab.ttf'
        }, Roboto: {
            normal: 'Roboto-Regular.ttf',
            bold: 'Roboto-Medium.ttf',
            italics: 'Roboto-Italic.ttf',
            bolditalics: 'Roboto-Medium.ttf'
        }
    }
    pdfMake.createPdf(docDefinition).download(fileName);
    $('#imgDown').hide();
    $('#downSuc').show();
    setTimeout(function () { $('#downSuc').hide(); $('#progressBar').hide(); }, 3000);
}

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

async function bindautocomplete(dprdetails) {
    var sourceItems = [];
    // var str = 'https://nregastrep.nic.in/netnrega/state_html/work_cat_freeze_detail.aspx?state_name=KARNATAKA&state_code=15&district_name=' + reportData.districtName + '&district_code=' + reportData.district_code + '&block_name=' + reportData.blockName + '&block_code=' + reportData.block_code + '&panchayat_name=' + reportData.panchayatName + '&panchayat_code=' + reportData.panchayat_code + '&fin_year=' + reportData.fincialYear + '&source=national&Digest=s3kn'
    var ac = 0;
    var cc = 0;
    var oc = 0;
    for (var key in dprdetails) {
        if (dprdetails.hasOwnProperty(key)) {
            if (dprdetails[key][1] == 'Approved')
                ac += 1;
            else if (dprdetails[key][1] == 'Completed')
                cc += 1;

            else
                oc += 1;

            sourceItems.push({ value: key, label: key + ' (' + dprdetails[key][0] + ')' });
        }
    }

    $('#appWork').text(ac);
    $('#ongoingWork').text(oc);
    $('#completedWork').text(cc);
    $("#txtSearchWorkCode").autocomplete({
        source: function (request, response) {
            response($.grep(sourceItems, function (item) {
                var srch = item.value.substr(item.value.length - 5, 5);
                if (srch.indexOf(request.term) > -1) {
                    return item;
                }
            }));
        },
        minLength: 3
    });

}

async function loadHH(response) {

    $($(response).find('table')[3]).find('tr').each((i, e) => jobCardsHH[e.cells[1].innerText.trim()] = e.cells[2].innerText.trim())
}

async function JobCardsDetails(res) {

    if (res !== undefined && res.split('$$').length == 4) {
        var e = res.split('$$')[0];
        var scst = res.split('$$')[3];
        var mustroll = res.split('$$')[1];
        var disabled = res.split('$$')[2];

        $('#scjobcard')[0].innerText = $(e)[0].cells[3].innerText.trim() + " & " + $(e)[0].cells[4].innerText.trim();
        $('#stjobcard')[0].innerText = $(e)[0].cells[5].innerText.trim() + " & " + $(e)[0].cells[6].innerText.trim();
        $('#otherjobcard')[0].innerText = $(e)[0].cells[7].innerText.trim() + " & " + $(e)[0].cells[8].innerText.trim();
        $('#jobcardMen')[0].innerText = $(e)[0].cells[9].innerText.trim();
        $('#jobcardWomen')[0].innerText = $(e)[0].cells[10].innerText.trim();
        $('#totaljobcard')[0].innerText = $(e)[0].cells[1].innerText.trim() + " & " + $(e)[0].cells[2].innerText.trim();

        //................. scst..................................

        $(scst).find('table')[3].querySelectorAll('tr').forEach((e, i) => {
            if (i > 3) {
                if ($(e)[0].querySelectorAll('td')[1].innerText.trim() === reportData.panchayatName.trim()) {
                    $('#totalPersondays')[0].innerText = $(e)[0].querySelectorAll('td')[14].innerText;
                    $('#scPersondays')[0].innerText = $(e)[0].querySelectorAll('td')[11].innerText;
                    $('#stPersondays')[0].innerText = $(e)[0].querySelectorAll('td')[12].innerText;
                    $('#womenPersondays')[0].innerText = $(e)[0].querySelectorAll('td')[15].innerText;
                    $('#otherPersondays')[0].innerText = $(e)[0].querySelectorAll('td')[13].innerText;
                    $('#scpersoncard')[0].innerText = $(e)[0].querySelectorAll('td')[6].innerText;
                    $('#stpersoncard')[0].innerText = $(e)[0].querySelectorAll('td')[7].innerText;
                    $('#otherpersoncard')[0].innerText = $(e)[0].querySelectorAll('td')[8].innerText;
                    $('#totalpersoncard')[0].innerText = $(e)[0].querySelectorAll('td')[9].innerText;

                }
            }
        })

        //.................scstend.....................................


        //mustrollmovment..............................................
        $(mustroll).find('a').filter((i, e) => {
            const url = new URL(e.href)
            const urlParams = new URLSearchParams(url.search);
            if (urlParams.get('type') == '9' && urlParams.get('panchayat_code').trim() == reportData.panchayat_code)
                $('#issuedNmr')[0].innerText = e.innerText;
            if (urlParams.get('type') == '10' && urlParams.get('panchayat_code').trim() == reportData.panchayat_code)
                $('#filledNmr')[0].innerText = e.innerText;
            if (urlParams.get('type') == '11' && urlParams.get('panchayat_code').trim() == reportData.panchayat_code)
                $('#zeroattendence')[0].innerText = e.innerText;
        })

        //mustrollend..................................................

        //...................disabled............................................
        $(disabled).find('table')[4].querySelectorAll('tr').forEach((e, i) => {
            if (i > 1) {
                if ($(e)[0].querySelectorAll('td')[1].innerText.trim() === reportData.panchayatName.trim()) {
                    $('#disabledjobcard')[0].innerText = $(e)[0].querySelectorAll('td')[2].innerText;
                    $('#disabledperswork')[0].innerText = $(e)[0].querySelectorAll('td')[3].innerText;
                    $('#disabledwork')[0].innerText = $(e)[0].querySelectorAll('td')[4].innerText;
                }
            }
        })

        //....................disabled.............................................
    }
    else
        console.log("unable to fetch data please try again..")

}

function findgp() {
    var pcode = $('#txtSearchWorkCode').val().trim().toUpperCase().split('/')[0].trim();
    return pcode;
}

function downloadForms(html, form, wage, fto) {

    strurl = "/templates/converter?page=" + form + "&stateName=" + reportData.stateName + "&workcode=" + reportData.workcode + "&WageList=" + wage + "&FTO=" + fto;
    $.ajax({
        url: strurl,
        type: "post",
        data: html,
        contentType: "application/x-www-form-urlencoded",
        xhrFields: {
            responseType: 'blob'
        },
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
                $('#imgDown').hide();
                $('#downSuc').show();
                setTimeout(function () { $('#downSuc').hide(); $('#progressBar').hide(); }, 3000);


            } catch (e) {
                alet("Error occured while downloading form. Please try again sometime later.")
                $('#imgDown').hide();
                setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
                console.log(e);
            }

        },
        error: function (error) {
            $('#imgDown').hide();
            setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
            console.log(error);
        }
    });

}

function escapeSpecialChars(text) {
    return text.replace(/[+\\]/g, '\\$&');
}

function PMIAY(res,data) {

    reportData.MaterialCost = 0;

    reportData.SkilledCost = 0;

    reportData.executionAgency = "Gram Panchayat";

    reportData.executionLevel = "GP"

    textfonts = $(res).find('font');

    for (i = 10; i < textfonts.length; i++) {
        if (textfonts[i].innerText.trim().toLowerCase() === "work name")
            reportData.workName = textfonts[i + 2].innerText.trim();
        if (textfonts[i].innerText.trim().toLowerCase() === "work start date") {
            reportData.startdate = textfonts[i + 1].innerText.trim();
            reportData.workYear = reportData.startdate.split('/')[2]
        }


        if (textfonts[i].innerText.trim().toLowerCase() === "work purpose status")
            reportData.workCategory = textfonts[i + 1].innerText.trim();

        if (textfonts[i].innerText.trim() === "Estimated Cost (In Lakhs)")
            reportData.workCostTotal = textfonts[i + 1].innerText.trim() * 100000;

        if (textfonts[i].innerText.trim() === "Work Status")
            reportData.workStatus = textfonts[i + 1].innerText.trim();

        if (textfonts[i].innerText.trim() === "Sanction No. and Sanction Date") {
            reportData.finSanctionNo = reportData.techSanctionNo = textfonts[i + 1].innerText.trim();

            reportData.finSanctionDate = reportData.techSanctionDate = textfonts[i + 2].innerText.trim();

        }
        if (textfonts[i].innerText.trim() === "Unskilled" && i<50)
            reportData.UskilledExp = textfonts[i + 6].innerText.trim();

        
    }

    reportData.workcode =  data;
}