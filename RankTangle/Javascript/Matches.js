jQuery(window).load(function () {
    /* ******************************************************************
     * Match View
     */
    var $r1 = $('#red-player-1'),
        $b1 = $('#blue-player-1'),
        $playerSelects = $('.select-player'),
        $openForm = $('#open-submit-match-form'),
        $submitMatchForm = $('#submit-match-form');

    $openForm.on('click', function () {
        $submitMatchForm.slideToggle('fast', function() {
            $submitMatchForm.css({'overflow':''});
        });
    });

    $('#create-match-button').on('click', function (e) {
        var errm = "";
        if (!!$r1.val() === false || !!$b1.val() === false) {
            errm = 'Choose at least a Player 1 on each team';
        }
        if (!!errm === true) {
            e.preventDefault();
            displayErrorMessage(errm);
        }
    });

    var valueBeforeChange;
    var $thisSelect;
    
    $playerSelects.on('focus', function (e) {
        $thisSelect = $(e.target);
        valueBeforeChange = $thisSelect.find(':selected').val();
    }).on('change', function () {
        var $thisOption = $thisSelect.find(':selected'),
            $redPlayer1 = $('#red-player-1'),
            $bluePlayer1 = $('#blue-player-1'),
            $teamScores = $('.team-scores');

        // reset options 
        $.each($('option[value="' + valueBeforeChange + '"]').not($thisOption), function (idx, element) {
            $(element).removeAttr('disabled');
        });
        
        // if the chosen option is default (empty) 
        if (!$thisOption.val() === false) {
            $.each($('option[value="' + $thisOption.val() + '"]').not($thisOption), function (idx, element) {
                $(element).attr('disabled','disabled');
            });
        }
        
        // If at least one player on each team is selected, then show the score sliders
        if (!!$redPlayer1.val() && !!$bluePlayer1.val()) {
            $teamScores.slideDown('fast', function () {
                $teamScores.css({ 'overflow': '' });
            });
        } else {
            $teamScores.slideUp('fast', function () {
                $teamScores.css({ 'overflow': '' });
            });
        }
        
        valueBeforeChange = $thisSelect.find(':selected').val();
    });

    $('.delete').on('click', 'a', function (e) {
        if (confirm("Delete this match?") === false ) {
            e.preventDefault();
            return false;
        }
        return undefined;
    });
    
    // Add a tag on the players that match the current user
    var currentUserId = $("#current-user-id").val();
    if (!!currentUserId === true) {
        $("." + currentUserId).append("<sup>(me)</sup>");
    }
    
    /* ******************************************************************
     * SaveMatchResult View
     */
    // Validation
    $('#submit-score-button').on('click', function (e) {
        var errm = "",
            $teamRedScore = $('#team-red-score'),
            $teamBlueScore = $('#team-blue-score');
        
        clearErrorMessage();

        if (!!$r1.val() === false || !!$b1.val() === false) {
            errm = 'Choose at least a Player 1 on each team';
        }

        if (!!errm === true) {
            e.preventDefault();
            displayErrorMessage(errm);
        }
        
        if ($teamRedScore.val() === $teamBlueScore.val()) {
            displayErrorMessage("A FoosBall Fight must have a winner and a loser. Please resolve.");
        }

        if (errorState()) {
            e.preventDefault();
        }
    });

    
    $('#team-red-score-slider').on('change', function (e) {
        $('#team-red-score').val(e.target.value);
    });
    
    $('#team-blue-score-slider').on('change', function (e) {
        $('#team-blue-score').val(e.target.value);
    });
    


});
