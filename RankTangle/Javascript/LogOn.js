jQuery(window).load(function() {
    $('#Email').focus();

    $('form[name="logon-player"]').on('submit', function (e) {
        var email = $('#Email').val();
        var pw = $('#Password').val();

        // Validate emails field
        if (!!email === false || !!pw === false) {
            displayErrorMessage("Please provide both an email and a password.", "All");
            $('.Auth').remove();
        } else {
            clearErrorMessage("All");
        }

        // Check if errors occured 
        if (errorState()) {
            e.preventDefault();
        }
    });
});