//https://nregastrep.nic.in/netnrega/srch_wg_dtl.aspx?state_code=&district_code=1507&state_name=KARNATAKA&district_name=VIJAYPURA&block_code=1507002&wg_no=1507002WL026669&short_name=KN&fin_year=2020-2021&mode=wg&Digest=ejjf

var wagelistUrl = "https://nregastrep.nic.in/netnrega/srch_wg_dtl.aspx?";

var ftoUrl = "https://nregastrep.nic.in/netnrega/FTO/fto_trasction_dtl.aspx?";

var jcrurl = "https://nregastrep.nic.in/netnrega/state_html/jcr.aspx?"
var exceptionlist = {
    "KN-23-002-028-001":"1523002040",
    "KN-06-001-035-001":"1506001039",
    "KN-16-003-027-011":"1516003034",
    "KN-07-001-019-001":"1507001011",
    "KN-07-001-039-001": "1507001041",
    "KN-20-003-018-005":"1520003031"
}
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
                url: "../api/wagelistdata",
                type: "POST",
                dataType: "html",
                JC: jobC,
                index: i,
                data: "https://nregastrep.nic.in/netnrega/writereaddata/state_out/jobcardreg_" + reportData.panchayat_code + "_eng.html",
                success: function (resp) {
                    var tempjc = $(resp).find('tr');
                    if (tempjc.length > 2) {
                        for (d = 4; d < tempjc.length; d++) {
                            if (tempjc[d].querySelectorAll('a')[0].innerText.trim() === this.JC) {
                                hhName = $(data).find('#lbl_house')[0].textContent;
                            }
                        }
                    }
                    if (hhName !== undefined) {

                        var str = "/templates/agency/form8?JobC=" + this.JC + "&appN=" + hhName + "&ltrNo=" + (j + 1) + "&frm=" + startDate + "&to=" + endDate + "&village=" + $(data)[3].querySelectorAll('table')[1].querySelectorAll('tr')[10].cells[1].innerText.trim();
                        $.ajax({
                            url: str,
                            type: "post",
                            data: JSON.stringify([{
                                'blockNameRegional': reportData.blockNameRegional, 'districtNameRegional': reportData.districtNameRegional,
                                'panchayat_NameRegional': reportData.panchayat_NameRegional, 'workName': reportData.workName, 'workcode': reportData.workcode, 'LineDeptNameRegional': reportData.LineDeptNameRegional, 'ExeAgency': reportData.executionAgency
                            }, familymembers]),
                            contentType: "application/json",
                            xhrFields: {
                                responseType: 'html'
                            },
                            async: false,

                            success: function (data) {
                                html += $(data)[5].innerHTML;
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
    var html = { gpdata: gpData, jcdata: data, workdata: { 'workName': reportData.workName, 'workcode': reportData.workcode }, LineDeptName: reportData.LineDeptNameRegional, ExeAgency: reportData.executionAgency };

    var str = "/templates/agency/form9";
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
    var str = "/templates/agency/form6";
    var data = reportData.NMRS.filter(e => e.NMRNO == elm.dataset.nmrno);
    gpData.panchayat_NameRegional = reportData.panchayat_NameRegional;
    var html = { gpdata: gpData, jcdata: data, LineDeptName: reportData.LineDeptNameRegional };
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
    var alljcs = [];
    var tablerows = [];
    var gpcodes = [];
    for (i = 0; i < table.length; i++) {
        jcd = table[i].JCNO.trim();
        jca = table[i].appName.trim();
        jcac = table[i].category;
        jcp = table[i].appAddress.trim();
        jcbankName = table[i].BankName.trim();
        tablerows.push({ SiNo: i + 1, JobCardNo: jcd, ApplicantName: jca, Place: jcp, category: jcac, bkname: jcbankName, bkacc: 'XXXXXXXXXXXX', hh: '' });
        var gpcode = '15' + jcd.split('-')[1] + jcd.split('-')[2] + jcd.split('-')[3]
        if (gpcodes.indexOf(gpcode) === -1)
            gpcodes.push(gpcode);
        if (exceptionlist[jcd.split('/')[0]] !== null )
            if (gpcodes.indexOf(exceptionlist[jcd.split('/')[0]]) === -1)
                gpcodes.push(exceptionlist[jcd.split('/')[0]]);
    }
    if (gpcodes.indexOf(reportData.panchayat_code) === -1)
        gpcodes.push(reportData.panchayat_code);

    var j = 0;
    try {

        $.ajax({
            url: "../api/jobCardHHagency?lflag=eng&District_Code=" + reportData.district_code + "&district_name=" + reportData.districtName + "&state_name=KARNATAKA&state_Code=15&block_name=" + reportData.blockName + "&block_code=" + reportData.block_code + "&fin_year=2024-2025&check=1",
            type: "post",
            dataType: "html",
            data: JSON.stringify(gpcodes),
            contentType: "application/json", 
            xhrFields: {
                responseType: 'html'
            },
            async: false,
            success: function (resp) {
                var tempjc = $(resp).find('a');
                if (tempjc.length > 2) {
                    for (i = 0; i < tablerows.length; i++) {
                        var jc = tablerows[i].JobCardNo.trim();
                        for (d = 0; d < tempjc.length; d++) {
                            const urlParams = new URLSearchParams(tempjc[d].search);;
                            if (urlParams.get('reg_no') !== null && urlParams.get('reg_no')!==undefined) {
                                if (urlParams.get('reg_no').trim() === jc) {
                                    tablerows[i].hh = $(tempjc[d]).parent().parent().find('td')[2].innerText.trim();
                                        j++;
                                        if (j === tablerows.length)
                                            break;
                                    }
                            }
                        }

                    }
                    if (j === tablerows.length) {
                        buildBlankNMR(JSON.stringify(tablerows), JSON.stringify({
                            startDate: startDate, endDate: endDate, msrno: msrno, finSanNo: finSanNo, finSanDate: finSanDate, techSanNo: techSanNo, techSanDate: techSanDate
                            , finYear: finYear, panchayat: reportData.panchayatName, distName: reportData.districtName, state: reportData.stateName, block: reportData.blockName,
                            workname: reportData.workName, workcode: reportData.workcode, agency: reportData.executionAgency, wkdays: wkdays, techStaff: reportData.technicalstaff
                        }));
                    }
                }


            },
            error: function (msg) {

            }
        });

    } catch (e) {
        console.log(e);
    }



}

function buildBlankNMR(jcdata, workdata) {
    var allData = [jcdata, workdata];
    var str = "/templates/agency/blknmr1";
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
    //var fin_year = ['2024-2025', '2023-2024', '2021-2022', '2020-2021', '2019-2020', '2022-2023', '2018-2019', '2017-2018'];
    $.ajax({
        url: "../api/wagelistdata",
        type: 'POST',
        data: data[0].url,
        startDate: data[0].DateFrom,
        success: function (resp) {
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
                var datecreditedID;
                $(table[0].cells).each((i, e) => {
                    if (e.textContent.trim() == 'Wagelist No.') wageListId = i; if (e.textContent.trim() == 'A/c Credited Date') datecreditedID = i; })
                //var wageListId = form[0].querySelectorAll("[id$='ContentPlaceHolder1_grdShowRecords'] tr")[0].cells.length - 5;
                //var datecreditedID = form[0].querySelectorAll("[id$='ContentPlaceHolder1_grdShowRecords'] tr")[0].cells.length - 3;
                var validwages = [];
              
                for (i = 1; i < table.length - 1; i++) {
                    wagelist = table[i].cells[wageListId].textContent.trim()
                    datecredited = table[i].cells[datecreditedID].textContent.trim()
                    if (validwages.indexOf(wagelist) == -1 && wagelist.length > 2) {
                       
                        validwages.push(wagelist);
                    }
                }
                var startDate = this.startDate;
                if (validwages.length !== 0) {
                    validwages.forEach(function (e) {
                        str = "state_code=" + reportData.state_code + "&district_code=" + reportData.district_code + "&state_name=" + reportData.stateName + "&district_name=" + reportData.districtName + "&block_code=" + reportData.block_code + "&srch=" + startDate + "&wageList=" + e + "&short_name=" + reportData.state_shortname;
                        $.ajax({
                            url: "../api/getAgencyWageList?" + str,
                            type: "POST",
                            dataType: "html",
                            data: wagelistUrl + str,
                            wagelistnumber: e,
                            success: function (data) {
                                var form = $(data).filter((i, e) => e.id == 'form1');
                                if (form.length == 0)
                                    form = $(data).filter((i, e) => e.id == 'aspnetForm');
                                if (form[0] !== undefined)
                                    if (form[0].querySelectorAll('table')[0].querySelectorAll('tr').length > 1) {
                                        wagelist = e;

                                        if (elm.name == "FTO") {
                                            const urlParams = new URLSearchParams(form[0].action);
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
                else {
                    alert("No WageList or FTO found");
                    $('#imgDown').hide();
                }
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
        str = "state_code=" + reportData.state_code + "&district_code=" + reportData.district_code + "&state_name=" + reportData.stateName + "&district_name=" + reportData.districtName + "&block_code=" + reportData.block_code + "&srch=" + finyear + "&ftoNo=" + e + "&short_name=" + reportData.state_shortname;
        $.ajax({
            url: "../api/getAgencyFto?" + str,
            type: "GET",
            ftonumber: e,
            success: function (data) {
                var form = $(data).filter((i, e) => e.id == 'form1');
                if (form.length == 0)
                    form = $(data).filter((i, e) => e.id == 'aspnetForm');

                $(form).find('table')[0].remove();
                $(form).find('table')[0].remove();
                $(form).find('table')[0].remove();

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
    var nmrLink = elm.dataset.link.replaceAll('nregastrep.nic.in','mnregaweb4.nic.in');
    $.ajax({
        url: "../api/wagelistdata",
        type: 'POST',
        data: nmrLink,
        success: function (data) {
            d = $(data).find("[id$='ContentPlaceHolder1_divprint']").children();
            downloadForms(d[1].outerHTML + d[2].innerHTML, "filledNmr");

        },
        error: function (error) {

            $('#imgDown').hide();
            setTimeout(function () { $('#downFail').hide(); $('#progressBar').hide(); }, 3000);
            console.log(error);
        }
    })

}