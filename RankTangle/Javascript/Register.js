jQuery(window).load(function () {
    $('#Player_Email').focus();

    // Form Validation
    var $AllInputFieldsNotHidden = $('input').not('[type="hidden"]');
    
    $('form[name="register-player"]').on('submit', function (e) {
        var email = $('#Player_Email').val();
        var name = $('#Player_Name').val();
        var password = $('#Player_Password').val();
        var repeatPassword = $('#Player_RepeatPassword').val();
        var missingFields = false;

        $.each($('input[type="text"], input[type="email"]'), function () {
            var $this = $(this);
            $this.val($this.val().trim());
        });

        // Validate that all fields are filled out
        $.each($AllInputFieldsNotHidden, function () {
            var $this = $(this);
            if (!$this.val()) {
                missingFields = true;
                return;
            }
        });

        if (missingFields) {
            displayErrorMessage("All fields are required to register.","All");
        } else {
            clearErrorMessage("All");
        }

        // Validate emails field
        if (emailExists(email) === false) {
            displayErrorMessage("You must submit a valid trustpilot email.", "Email");
        }

        // Validate Name field
        nameExists(name);

        // Validate password fields
        if (password !== repeatPassword) {
            displayErrorMessage("Your passwords do not match.", "Password");
        } else {
            clearErrorMessage("Password");
        }
        
        // Check if errors occured 
        if (errorState()) {
            e.preventDefault();
        }
    });

    $('#Player_Email').on('change', function () {
        var playerEmail = $(this).val();
        
        if (!playerEmail) {
            $.ajax({
                type: "get",
                url: "/Account/GetGravatarUrl/",
                data: { emailPrefix: playerEmail},
                success: function (jsonObj) {
                    $("#profile-gravatar").attr("src", jsonObj.url);
                }
            });
        }

        emailExists(playerEmail);
    });
});

// Synch call to server to check if email is alredy registered
function emailExists(email) {
    $.ajax({
        type: 'post',
        url: '/Account/PlayerEmailExists',
        cache: false,
        data: { email: email || "" },
        dataType: 'json',
        async: false,
        success: function (response) {
            if (response.Exists) {
                displayErrorMessage("A player with this email already exists ("+response.Name+")", "Email");
            } else {
                clearErrorMessage("Email");
            }
        }
    });
}
