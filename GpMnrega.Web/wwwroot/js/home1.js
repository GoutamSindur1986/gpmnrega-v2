/**
 * home1.js v3.0
 * Button hooks ONLY for Cashbook & Registers page.
 *
 * Key changes from v2.x:
 *   - reportData object and loadData() moved to common.js
 *   - PDF generation functions moved to per-button JS files:
 *       ftowagecash.js  → generateFTOWageListForm()
 *       ftomatcash.js   → generateFTOMaterialListForm()
 *       register3_form.js → generateRegister3Form()
 *       register5.js    → generateRegister5Form()
 *       register4.js    → getRegister4Data()
 *       register1b.js   → register1b()
 *       register6.js    → register6()
 *       register7.js    → register7()
 *   - Per-button validations added:
 *       Monthly buttons → month required (#ddlMonth != '00')
 *       Quarterly buttons → quarter required (#ddlQtr != '00')
 *       Yearly buttons → no period validation
 *
 * This file mirrors the $(document).ready() block in original home1.js,
 * updating button IDs to match the 8 original Cashbook.aspx buttons.
 */

$(document).ready(function () {
    loadData();
    reportData.fincialYear = $("#ddlyear").val();

    // ── FTO Wage Cash Book (Monthly) ─────────────────────────────────────
    $('#FTOWageCash').click(function () {
        if ($('#ddlMonth').val() == '00') {
            alert('Please select a month to generate FTO Wage Cash Book');
            return;
        }
        showDownloadProgress('FTO Wage Cash Book', 'Fetching data from NIC…');
        var startDate = moment([$('#ddlyear').val().split('-')[0], $('#ddlMonth').val() - 1]);
        if ($('#ddlMonth').val() == '01' || $('#ddlMonth').val() == '02' || $('#ddlMonth').val() == '03') {
            startDate = moment([$('#ddlyear').val().split('-')[1], $('#ddlMonth').val() - 1]);
        }
        var endDate = moment(startDate).endOf('month');
        var request = {
            command: "generate_fto_wage_list",
            ftoWageListParams: {
                endArrears: 0,
                end_date: endDate,
                fileName: "",
                month_digits: $('#ddlMonth').val(),
                month_name: reportData.fincialYear + " " + monthNames[parseInt($('#ddlMonth').val()) - 1],
                startArrears: 0,
                start_date: startDate
            }
        };
        generateFTOWageListForm(request);
    });

    // ── FTO Material Cash Book (Monthly) ─────────────────────────────────
    $('#FTOMatCash').click(function () {
        if ($('#ddlMonth').val() == '00') {
            alert('Please select a month to generate FTO Material Cash Book');
            return;
        }
        showDownloadProgress('FTO Material Cash Book', 'Fetching data from NIC…');
        var startDate = moment([$('#ddlyear').val().split('-')[0], $('#ddlMonth').val() - 1]);
        if ($('#ddlMonth').val() == '01' || $('#ddlMonth').val() == '02' || $('#ddlMonth').val() == '03') {
            startDate = moment([$('#ddlyear').val().split('-')[1], $('#ddlMonth').val() - 1]);
        }
        var endDate = moment(startDate).endOf('month');
        var request = {
            endArrears: 0,
            end_date: endDate,
            fileName: "",
            month_digits: $('#ddlMonth').val(),
            month_name: reportData.fincialYear + " " + monthNames[parseInt($('#ddlMonth').val()) - 1],
            startArrears: 0,
            start_date: startDate
        };
        generateFTOMaterialListForm(request);
    });

    // ── Register 1B (Quarterly) ───────────────────────────────────────────
    $('#Register1').click(function () {
        if ($('#ddlQtr').val() == "00") {
            alert("Please select a quarter to generate Register 1B");
            return;
        }
        showDownloadProgress('Register 1B', 'Generating PDF…');
        register1b();
    });

    // ── Register 3 (Monthly) ─────────────────────────────────────────────
    $('#Register3').click(function () {
        if ($('#ddlMonth').val() == '00') {
            alert('Please select a month to generate Register 3');
            return;
        }
        showDownloadProgress('Register 3', 'Fetching data from NIC…');

        var startDate, endDate;
        if ($('#ddlMonth').val() == '01' || $('#ddlMonth').val() == '02' || $('#ddlMonth').val() == '03') {
            startDate = moment([$('#ddlyear').val().split('-')[1], $('#ddlMonth').val() - 1]).format('DD/MM/YYYY');
            endDate   = monthDays[$('#ddlMonth').val() - 1] + "/" + $('#ddlMonth').val() + "/" + startDate.split('/')[2];
        } else {
            startDate = moment([$('#ddlyear').val().split('-')[0], $('#ddlMonth').val() - 1]).format('DD/MM/YYYY');
            endDate   = monthDays[$('#ddlMonth').val() - 1] + "/" + $('#ddlMonth').val() + "/" + startDate.split('/')[2];
        }

        var sevenRegisterParams = {
            end_date:     endDate,
            fileName:     "Register3 " + monthNames[$('#ddlMonth').val() - 1] + " (" + reportData.fincialYear + ").pdf",
            month_name:   monthNames[parseInt($('#ddlMonth').val()) - 1] + " (" + reportData.fincialYear + ")",
            needCoverPage: true,
            start_date:   startDate
        };
        generateRegister3Form(jQuery.extend(true, {}, sevenRegisterParams));
    });

    // ── Register 4 (Quarterly) ───────────────────────────────────────────
    $('#Register4').click(function () {
        if ($('#ddlQtr').val() == "00") {
            alert("Please select a quarter to generate Register 4");
            return;
        }
        showDownloadProgress('Register 4', 'Fetching data from NIC…');

        var qtr = $('#ddlQtr').val();
        var yr0 = $('#ddlyear').val().split('-')[0];
        var yr1 = $('#ddlyear').val().split('-')[1];
        var startDate, endDate;
        if (qtr == "0") { startDate = "01/04/" + yr0; endDate = "30/06/" + yr0; }
        if (qtr == "1") { startDate = "01/07/" + yr0; endDate = "30/09/" + yr0; }
        if (qtr == "2") { startDate = "01/10/" + yr0; endDate = "31/12/" + yr0; }
        if (qtr == "3") { startDate = "01/01/" + yr1; endDate = "31/03/" + yr1; }

        var sevenRegisterParams = {
            end_date:   endDate,
            fileName:   "Register4 " + $('#ddlQtr option:selected').text() + ".pdf",
            month_name: monthNames[parseInt($('#ddlMonth').val()) - 1] + " (" + reportData.fincialYear + ")",
            needCoverPage: true,
            start_date: startDate,
            quater:     qtr,
            finyear:    $('#ddlyear').val()
        };
        getRegister4Data(jQuery.extend(true, {}, sevenRegisterParams));
    });

    // ── Register 5 (Yearly) ──────────────────────────────────────────────
    $('#Register5').click(function () {
        showDownloadProgress('Register 5', 'Fetching data from NIC…');
        // generateRegister5Form uses global reportData for all NIC params;
        // sevenRegisterParams only carries fin_year context here.
        var sevenRegisterParams = {
            finyear: $('#ddlyear').val()
        };
        generateRegister5Form(jQuery.extend(true, {}, sevenRegisterParams));
    });

    // ── Register 6 (Yearly) ──────────────────────────────────────────────
    $('#Register6').click(function () {
        showDownloadProgress('Register 6', 'Generating PDF…');
        register6();
    });

    // ── Register 7 (Yearly) ──────────────────────────────────────────────
    $('#Register7').click(function () {
        showDownloadProgress('Register 7', 'Generating PDF…');
        register7();
    });

    // ── Financial year change ─────────────────────────────────────────────
    $("#ddlyear").change(function (e) {
        reportData.fincialYear = e.currentTarget.selectedOptions[0].value;
    });
});
