var rows = `<tr>
    <td><span style="font-size: 16px;"> @SINO </span></td>
    <td><span style="font-family: tunga; font-size: 16px;"> @ApplicantName </span></td>
    <td><span style="font-family: tunga; font-size: 16px;"> @Address </span></td>
    <td><span style="font-size: 16px;"> @JobCardNo </span></td>
    <td><span style="font-size: 16px;"> @FromDate </span></td>
    <td><span style="font-size: 16px;"> @toDate </span></td>
    <td><span style="font-size: 16px;">  </span></td>
    <td><span style="font-size: 16px;">  </span></td>
</tr>`;

$(document).ready(function () {

    nmrLink = $('#lblMnrLink').val();
    for (i = 0; i < 3; i++) {
        $.ajax({
            url: nmrLink,
            type: "GET",
            dataType: "html",
            success: function (data) {
                loadNMRData(data);
                break;
            },
            error: function (error) {

            }
        });
    }

})

function loadNMRData(data) {
    var startDate = $(data)[10].querySelectorAll('#ContentPlaceHolder1_lbldatefrom')[0].innerText;
    var endDate = $(data)[10].querySelectorAll('#ContentPlaceHolder1_lbldateto')[0].innerText;
    var address = $('#gpNamereg').val();
    var table = $(data)[10].querySelectorAll('table#ContentPlaceHolder1_grdShowRecords td:nth-child(2)');
    var html = "";
    for (i = 0; i < table.length - 1; i++) {
        var applicantname = table[i].innerHTML.split('<br>')[0].indexOf('(') != -1 ? table[i].innerHTML.split('<br>')[0].substring(0, table[i].innerHTML.split('<br>')[0].indexOf('(')) : table[i].innerHTML.split('<br>')[0];
        html += rows.replaceAll("@SINO", i + 1).replaceAll("@ApplicantName", applicantname)
            .replaceAll("@Address", address).replaceAll("@JobCardNo", table[i].querySelectorAll('a')[0].innerText).replaceAll("@FromDate", startDate).
            replaceAll("@toDate", endDate);
    }

    $('#tbllistofusers > tbody:last-child').append(html);

}