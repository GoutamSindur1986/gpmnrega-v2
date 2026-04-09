var validate = [];
$(document).ready(function () {

    $(".trigger_popup_fricc").click(function () {
        $('.hover_bkgr_fricc').show();
    });
    $('.hover_bkgr_fricc').click(function () {
        $('.hover_bkgr_fricc').hide();
    });
    $('.popupCloseButton').click(function () {
        $('.hover_bkgr_fricc').hide();
    });
    $("#ddlstate").change(function () {

        callDistAjax("/pullgpcodes?pull=district&code=" + $('option:selected', this).val());

    });

   

    $('#txtblockcode').blur(function () {

        callBlockAjax("/pullgpcodes?pull=block&code=" + $(this).val());
    })


    $('#btnRegister').click(function (e) {

        validate = [];
        
        $('#txtblockcode').text('');
        $('#lblUserName').text('');
        $('#lblEmailerror').text('');
        $('#lblPassword').text('');
        $('#lblPhone').text('');
        $('#lblAgency').text('');
        $('#lblTerms').text('');
        $('#lblPrivacy').text('');
        //registerValidation();
        if (/^[a-zA-Z0-9- ]*$/.test($('#txtUserName').val()) == false || $('#txtUserName').val().length < 1 || $('#txtUserName').val().length > 50) {
            validate.push('lblUserName:Invalid User Name.');
        }

        if (/^\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b$/i.test($('#txtEmail').val()) == false) {

            validate.push('lblEmailerror:Please enter valid email address');
        }

        if ($('#txtPassword').val().length < 6 || $('#txtPassword').val().length > 12 || $('#txtPassword').val().indexOf(" ") > -1) {
            validate.push('lblPassword:Password must be between 6-12 characters')
        }

        if (/^\d{10}$/.test($('#txtPhone').val()) == false) {
            validate.push('lblPhone:Invalid Phone Number');
        }

        if ($('#ddlAgency').val() ==='--Select--') {
            validate.push('lblAgency:Must select agency.');
        }

        

        if ($('#chkTerms').is(':checked') == false) {
            validate.push('lblTerms:Must Agree to Terms');
        }
       
        if ($('#chkPrivacy').is(':checked') == false) {
            validate.push('lblPrivacy:Must Agree to Privacy');
        }

        callBlockAjax("/pullgpcodes?pull=block&code=" + $('#txtblockcode').val());

        if (validate.length > 0) {

            for (i = 0; i < validate.length; i++) {

                $('#' + validate[i].split(':')[0]).text(validate[i].split(':')[1]);

            }
            e.preventDefault();
            return false;
        }
    });

});

function callDistAjax(_url) {

    $.ajax({
        url: _url,
        type: 'GET',

        dataType: 'html',
        success: function (response, textStatus, jqXHR) {
            $("#ddldistrict").empty();
            $("#ddldistrict").append('<option value="Select">Select District</option>');
            $("#ddldistrict").append($(response)[3].querySelectorAll('select')[0].innerHTML);

        },
        error: function (jqXHR, textStatus, errorThrown) {
            return errorThrown;
        }
    });
}

function callBlockAjax(_url) {
    $.ajax({
        url: _url,
        type: 'GET',
        async: false,
        dataType: 'html',
        success: function (response, textStatus, jqXHR) {
            if ($(response)[3].querySelectorAll('label')[0].innerText == "Error" || $(response)[3].querySelectorAll('label')[0].innerText == '') {
                validate.push("lblblock:Invalid Block Code");
                $('#lblpanchayat').css('color', 'red');
            }
            else {
                $('#lblblock').css('color', 'blue');
                $('#lblblock').text('Block ' + $(response)[3].querySelectorAll('label')[0].innerText);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            return errorThrown;
        }
    });

}



function callPanchayatAjax(_url) {
    $.ajax({
        url: _url,
        type: 'GET',
        async: false,
        dataType: 'html',
        success: function (response, textStatus, jqXHR) {
            if ($(response)[3].querySelectorAll('label')[0].innerText == "Error" || $(response)[3].querySelectorAll('label')[0].innerText=='') {
                validate.push("lblpanchayat:Invalid Panchyat Code");
                $('#lblpanchayat').css('color', 'red');
            }
            else {
                $('#lblpanchayat').css('color','blue');
                $('#lblpanchayat').text('Gram Panchayat '+$(response)[3].querySelectorAll('label')[0].innerText);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            return errorThrown;
        }
    });
}



function checkboxchange(ele) {
    if (ele.checked) {
        document.getElementById('divfronttext').style.display = 'none';
        document.getElementById('divbacktext').style.display = '';
    }
    if (!ele.checked) {
        document.getElementById('divfronttext').style.display = '';
        document.getElementById('divbacktext').style.display = 'none';
    }
}

function registerValidation() {


}