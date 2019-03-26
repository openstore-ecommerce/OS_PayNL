

$(document).ready(function () {

    $('#os_paynl_cmdSave').unbind("click");
    $('#os_paynl_cmdSave').click(function () {
        $('.processing').show();
        $('.actionbuttonwrapper').hide();
        // lower case cmd must match ajax provider ref.
        nbxget('os_paynl_savesettings', '.OS_PayNLdata', '.OS_PayNLreturnmsg');
    });

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.actionbuttonwrapper').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        // lower case cmd must match ajax provider ref.
        nbxget('os_paynl_selectlang', '.OS_PayNLdata', '.OS_PayNLdata');
    });


    $(document).on("nbxgetcompleted", os_paynl_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function os_paynl_nbxgetCompleted(e) {

        $('.processing').hide();
        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        if (e.cmd == 'os_paynl_selectlang') {
                        
        }

    };

});

