//https://mnregaweb4.nic.in/netnrega/srch_wg_dtl.aspx?state_code=&district_code=1507&state_name=KARNATAKA&district_name=VIJAYPURA&block_code=1507002&wg_no=1507002WL026669&short_name=KN&fin_year=2020-2021&mode=wg&Digest=ejjf





var wagelistUrl = "https://nregastrep.nic.in/netnrega/srch_wg_dtl.aspx?";

var ftoUrl = "https://nregastrep.nic.in/netnrega/FTO/fto_trasction_dtl.aspx?";

var jcrurl = "https://nregastrep.nic.in/netnrega/state_html/jcr.aspx?"

//var editorExtensionId = "lkpnabgphhcecnpidbipnfdaekkkagbp";
var editorExtensionId = "mhjeiodpgdlaooedhfiokmifcpmakpjc";

function loadNMRDataForForm8(elm) {
    var nmr = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    var table = nmr[0].JC;
    var startDate = nmr[0].DateFrom
    var endDate = nmr[0].DateTo;
    var jobcards = [];


    var html = "";
    var j = 0;
    for (i = 0; i < table.length; i++) {
        var jobC = table[i].JCNO;
        if ($.inArray(jobC, jobcards) == -1) {

            var hhName;
            jobcards.push(jobC);
            $.ajax({
                url: 'https://nregastrep.nic.in/netnrega/state_html/jcr.aspx?reg_no=' + jobC + '&panchayat_code=' + reportData.panchayat_code + '&Digest=s3kn',
                type: "GET",
                dataType: "html",

                success: function (data) {

                    var form = $(data).filter((i, e) => e.id == 'form1');
                    if (form.length == 0)
                        form = $(data).filter((i, e) => e.id == 'aspnetForm');
                    var families = table.filter(e => e.JCNO == form[0].querySelectorAll('td')[4].innerText.trim());
                    var familymembers = '';
                    families.forEach(function (t) { familymembers += t.appName + "," });
                    if (data !== undefined) {
                        hhName = $(data).find("[id$='lbl_house']")[0].textContent;
                        var str = "/templates/" + reportData.stateName + "/form8?JobC=" + form[0].querySelectorAll('td')[4].innerText.trim() + "&appN=" + hhName + "&ltrNo=" + (j + 1) + "&frm=" + startDate + "&to=" + endDate + "&village=" + form[0].querySelectorAll('table')[1].querySelectorAll('tr')[10].cells[1].innerText.trim();
                        $.ajax({
                            url: str,
                            type: "post",
                            data: JSON.stringify([{
                                'blockNameRegional': reportData.blockNameRegional, 'districtNameRegional': reportData.districtNameRegional,
                                'panchayat_NameRegional': reportData.panchayat_NameRegional, 'workName': reportData.workName, 'workcode': reportData.workcode
                            }, familymembers]),
                            contentType: "application/json",
                            xhrFields: {
                                responseType: 'html'
                            },
                            async: false,

                            success: function (data) {
                                var form = $(data).filter((i, e) => e.id == 'form1');
                                if (form.length == 0)
                                    form = $(data).filter((i, e) => e.id == 'aspnetForm');
                                html += form[0].innerHTML;
                            },
                            error: function (error) {

                            }
                        });


                    }
                    if (j === jobcards.length - 1) {

                        html = html.replaceAt(html.lastIndexOf("page-break-after"), "page-berak-after");
                        downloadForms(form8main + html + form8footer, "Form8");
                    }

                    j++;

                },
                error: function (error) {
                    alert("error downloading form8:" + error);
                }
            });

        }
    }

}

function loadNMRDataForForm9(elm) {
    var data = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    var html = { gpdata: gpData, jcdata: data, workdata: { 'workName': reportData.workName, 'workcode': reportData.workcode } };

    var str = "/templates/" + reportData.stateName + "/form9";
    $.ajax({
        url: str,
        type: "post",
        data: JSON.stringify(html),
        contentType: "application/json",
        xhrFields: {
            responseType: 'html'
        },
        async: false,

        success: function (data) {
            html = data;
        },
        error: function (error) {

        }
    });

    downloadForms(html, "Form9");
}

function loadNMRDataForForm6(elm) {
    var str = "/templates/" + reportData.stateName + "/form6";
    var data = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    var html = { gpdata: gpData, jcdata: data };
    $.ajax({
        url: str,
        type: "post",
        contentType: "application/x-www-form-urlencoded",
        data: JSON.stringify(html),
        xhrFields: {
            responseType: 'html'
        },
        async: false,

        success: function (data) {
            html = data;
        },
        error: function (error) {

        }
    });

    downloadForms(html, "Form6");
}

function loadNMRDataForFormblk(elm) {
    var data = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    var tabledata = {
        'SiNo': '',
        'JobCardNo': '',
        'HeadofFamily': '',
        'ApplicantName': '',
        'Place': '',
        'BankName': '',
        'AccNo': '',
        'category': ''
    };
    var tablerows = [];
    var jobCardslist = [];
    var checkjobcards = [];
    var startDate = data[0].DateFrom;
    var endDate = data[0].DateTo;
    var table = data[0].JC;
    var msrno = data[0].NMRNO;
    var finSanNo = reportData.finSanctionNo;
    var finSanDate = reportData.finSanctionDate;
    var techSanDate = reportData.techSanctionDate;
    var techSanNo = reportData.techSanctionNo;
    var finYear = finSanDate.split('/')[2] + '-' + (parseInt(finSanDate.split('/')[2]) + 1);
    var wkdays = moment(endDate, 'D/M/YYYY').diff(moment(startDate, 'D/M/YYYY'), 'days') + 1;
    var html = "";

    for (i = 0; i < table.length; i++) {
        jcd = table[i].JCNO.trim();
        jca = table[i].appName.trim();
        jcac = table[i].category;
        jcp = table[i].appAddress.trim();
        hhn = jobCardsHH[jcd] == undefined ? jca : jobCardsHH[jcd.replaceAll('*').trim()];
        bankname = table[i].BankName;

        tablerows.push({ SiNo: i + 1, JobCardNo: jcd, ApplicantName: jca, Place: jcp, category: jcac, bkname: bankname, bkacc: 'XXXXXXXXXXXX', hh: hhn });
    }

    buildBlankNMR(JSON.stringify(tablerows), JSON.stringify({
        startDate: startDate, endDate: endDate, msrno: msrno, finSanNo: finSanNo, finSanDate: finSanDate, techSanNo: techSanNo, techSanDate: techSanDate
        , finYear: finYear, panchayat: reportData.panchayatName, distName: reportData.districtName, state: reportData.stateName, block: reportData.blockName,
        workname: reportData.workName, workcode: reportData.workcode, agency: reportData.executionAgency, wkdays: wkdays, techStaff: reportData.technicalstaff
    }));
}

function buildBlankNMR(accdata, jcdata, workdata) {
    var allData = [accdata, jcdata, workdata];
    var str = "/templates/" + reportData.stateName + "/blknmr1";
    $.ajax({
        url: str,
        type: "post",
        contentType: "application/x-www-form-urlencoded",
        data: JSON.stringify(allData),
        xhrFields: {
            responseType: 'html'
        },
        async: false,
        success: function (data) {

            downloadForms(data, 'blknmr');
            // addBlankData(workdata.workcode + workdata.msrno, data);
        },
        error: function (error) {

        }
    });

}

function loadWageListData(elm) {
    var data = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    // var finyear = endDate + "-" + (parseInt(endDate) + 1);
    var fin_year = ['2024-2025', '2023-2024', '2021-2022', '2020-2021', '2019-2020', '2022-2023', '2018-2019', '2017-2018'];
    $.ajax({
        url: "../api/wagelistdata",
        type: 'POST',
        data: data[0].url,
        startDate: data[0].DateFrom,
        success: function (resp, success, url) {
            let searchParams = new URLSearchParams(this.data);
            var blockcode = searchParams.get('block_code')
            var form = $(resp).filter((i, e) => e.id == 'form1');
            if (form.length == 0)
                form = $(resp).filter((i, e) => e.id == 'aspnetForm');
            var table = form[0].querySelectorAll("[id$='ContentPlaceHolder1_grdShowRecords'] tr");
            if (table.length === 0) {
                alert('No Wagelist found.')
                $('#imgDown').hide();
            }
            else {
                var wageListId;
                $(table[0].cells).each((i, e) => {
                    if (e.textContent.trim() == 'Wagelist No.') wageListId = i; if (e.textContent.trim() == 'A/c Credited Date') datecreditedID = i;
                })

                var validwages = [];

                for (i = 1; i < table.length - 1; i++) {
                    wagelist = table[i].cells[wageListId].textContent.trim()
                    //datecredited = table[i].cells[datecreditedID].textContent.trim()
                    if (validwages.indexOf(wagelist) == -1 && wagelist.length > 2) {

                        validwages.push(wagelist);
                    }
                }
                var startDate = this.startDate;
                validwages.forEach(function (e) {
                    str = "state_code=" + reportData.state_code + "&district_code=" + reportData.district_code + "&state_name=" + reportData.stateName + "&district_name=" + reportData.districtName + "&block_code=" + blockcode + "&srch=" + startDate + "&wageList=" + e + "&short_name=" + reportData.state_shortname;
                    $.ajax({
                        url: "../api/getWageListData?" + str,
                        type: "POST",
                        dataType: "html",
                        data: wagelistUrl + str,
                        wagelistnumber: e,
                        success: function (data) {
                            var form = $(data).filter((i, e) => e.id == 'form1');
                            if (form.length == 0)
                                form = $(data).filter((i, e) => e.id == 'aspnetForm');
                            if ($(data)[5] !== undefined)
                                if (form[0].querySelectorAll('table')[0].querySelectorAll('tr').length > 1) {
                                    wagelist = e;

                                    if (elm.name == "FTO") {
                                        const urlParams = new URLSearchParams($(data)[5].action);
                                        finyear = urlParams.get('fin_year');
                                        loadFTOData(data, finyear);
                                    }
                                    else {
                                        downloadForms(data, 'WageList', this.wagelistnumber, '');
                                    }
                                }
                        },
                        error: function (error) {

                            alert("error downloading wagelist:" + e);
                        }
                    });
                })
            }
        },
        error: function (e) {

        }
    })


}
function loadFTOData(data) {
    var form = $(data).filter((i, e) => e.id == 'form1');
    if (form.length == 0)
        form = $(data).filter((i, e) => e.id == 'aspnetForm');
    if (form.length == 0)
        $(data).filter((i, e) => e.id == 'aspnetForm');
    d = new URLSearchParams(form[0].action)
    finyear = d.get('fin_year');
    var ftos = form[0].querySelectorAll('td:nth-child(13)');
    var validfto = [];
    for (i = 1; i < ftos.length; i++) {
        ftoid = ftos[i].textContent.replaceAll(' ', '').replaceAll('\n', '')
        if (validfto.indexOf(ftoid) < 0 && ftoid.length > 3) {
            validfto.push(ftoid);
        }
    }

    validfto.forEach(function (e) {
        str = "page=p&state_code=" + reportData.state_code + "&state_name=" + reportData.stateName + "&district_code=" + reportData.district_code + "&district_name=" + reportData.districtName + "&block_code=" + reportData.block_code + "&block_name=" + reportData.blockName + "&panchayat_code=" + reportData.panchayat_code + "&panchayat_name=" + reportData.panchayatName + "&flg=W&fin_year=" + finyear + "&fto_no=" + e + "&source=national&Digest=FlTx";
        $.ajax({
            url: "../api/getftoDetails?" + str,
            type: "POST",
            dataType: "html",
            data: ftoUrl + str,
            ftonumber: e,
            success: function (data) {
                var form = $(data).filter((i, e) => e.id == 'form1');
                if (form.length == 0)
                    form = $(data).filter((i, e) => e.id == 'aspnetForm');
                FTO = e;
                html = form[0].querySelectorAll('center')[0].outerHTML;
                //addFtoData(workcode, nmrno, e, html);
                downloadForms(html, 'FTO', '', this.ftonumber);
            },
            error: function (error) {

                alert("error downloading fto:" + e);
            }
        });
    });
}

function printFilledNMR(elm) {
    $('#progressBar').show();
    $('#imgDown').show();
    var nmrLink = elm.dataset.link;
    $.ajax({
        url: "../api/wagelistdata",
        type: 'POST',
        data: nmrLink,
        success: function (data) {

            d = $(data).find("#ctl00_ContentPlaceHolder1_divprint").children();
            d[2].querySelectorAll('#ctl00_ContentPlaceHolder1_lblWorkName')[0].innerText = reportData.workName;
            downloadForms(d[1].outerHTML + d[2].innerHTML, "filledNmr");

        },
        error: function (error) {

            $('#imgDown').hide();
            setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
            console.log(error);
        }
    })

}