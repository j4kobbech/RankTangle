jQuery(window).load(function() {
    $('#Email').focus();

    // Form Validation
    var $inputFields = $('input').not('[type="hidden"]');
    
    $('form[name="register-player"]').on('submit', function (e) {
        var email = $('#Email').val(),
            name = $('#Player_Name').val(),
            password = $('#Player_Password').val(),
            repeatPassword = $('#Player_RepeatPassword').val(),
            missingFields = false;

        $.each($('input[type="text"], input[type="email"]'), function () {
            var $this = $(this);
            $this.val($this.val().trim());
        });

        // Validate that all fields are filled out
        $.each($inputFields, function () {
            var $this = $(this);
            if (!!$this.val() === false) {
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
        if (emailIsValid(email) === false) {
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

    $('#Email').on('change', function() {
        var playerEmail = $(this).val();

        var $nickName = $('#NickName');
        if (!!$nickName.val() === false) {
            $nickName.val(playerEmail.substr(0,5));
        }
        
        if (!playerEmail === false) {
            $.ajax({
                type: "get",
                url: "/Account/GetGravatarUrl/",
                data: { emailPrefix: playerEmail},
                success: function (jsonObj) {
                    $("#profile-gravatar").attr("src",jsonObj.url);
                }
            });
        }
    });

    $('#delete-player').on('click', 'a', function (e) {
        if (confirm("Delete your Player?") === false) {
            e.preventDefault();
            return false;
        }
    });
});

// Synch call to server to check if email is alredy registered
function emailIsValid(email) {
    $.ajax({
        type: 'post',
        url: '/Account/PlayerEmailIsValid',
        cache: false,
        data: { email: email || "" },
        dataType: 'json',
        async: false,
        success: function (response) {
            if (response.isValid) {
                displayErrorMessage("A player with this email already exists ("+response.Name+")", "Email");
            } else {
                clearErrorMessage("Email");
            }
        }
    });
}

// Synch call to server to check if name is alredy registered
function nameExists(name) {
    $.ajax({
        type: 'post',
        url: '/Account/PlayerNameExists',
        cache: false,
        data: { name: name },
        dataType: 'json',
        async: false,
        success: function (response) {
            if (response.Exists) {
                displayErrorMessage("A player with this name already exists (" + response.Email + ")", "Name");
            } else {
                clearErrorMessage("Name");
            }
        }
    });
}