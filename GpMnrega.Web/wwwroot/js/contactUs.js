$(document).ready(function () {
    const captcha = new Captcha($('#canvas'), {

        length: 4,
        clickRefresh: true,
        caseSensitive: true

    });

    $('#btnSumit').on('click', function (e) {

        const ans = captcha.valid($('input[name="code"]').val());

        if (!ans) {
            $('#spnInvalidCaptch').show();
            return false;
            e.preventDefault();
        }
        else {
            
            if ($('form').find(":invalid").length==0)
            setTimeout(function () { $('#MainContent_thankYou').show(); }, 3000);
        }

        
    });


});