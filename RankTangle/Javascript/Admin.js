﻿jQuery(window).load(function () {

    /* ******************************************************************
     * Admin View
     */
    var $selectPlayer = $('#select-player');
    var $copyProdToStaging = $('#copy-prod-to-staging');
    var $replayMatches = $('#replay-matches');
    var $listEmails = $('#list-player-emails');
    var $appNameTextBox = $('#Settings_Name');

    $selectPlayer.on('change', function () {
        $.ajax({
            type: 'get',
            url: '/Account/Edit/',
            data: { id: $(this).children(':selected').attr('id') },
            success: function (data) {
                $('#player-data').html(data);
            }
        });
    });

    $copyProdToStaging.on('click', function (e) {
        e.preventDefault();
        toggleOverlay();

        $.ajax({
            type: 'post',
            url: '/Admin/CopyProdData/',
            success: function () {
                toggleOverlay();
                alert('Data has been copied.');
            }
        });
    });

    $replayMatches.on('click', function (e) {
        e.preventDefault();
        toggleOverlay();

        $.ajax({
            type: 'post',
            url: '/Admin/ReplayMatches/',
            success: function () {
                toggleOverlay();
                alert('Matches has been replayed.');
            }
        });
    });

    $listEmails.on('click', function (e) {
        e.preventDefault();
        toggleOverlay();

        $.ajax({
            type: 'get',
            url: '/Admin/GetPlayerEmails/',
            success: function (data) {
                toggleOverlay();
                var $textareaWrapper = $('#list-of-emails')
                        .html('')
                        .append('<textarea></textarea>');
                for (var index in data) {
                    $textareaWrapper.find('textarea').append(data[index]+',').focus();
                }
            }
        });
    });

    $appNameTextBox.focus();
});

function toggleOverlay() {
    var overlay = $('#overlay');

    if (overlay.size() === 0) {

        var htmlHeight = $('html').innerHeight();
        $('body').append('<div id="overlay"></div>');

        overlay = $('#overlay');

        overlay.css({
            'height': htmlHeight + 'px',
            'background-color': 'rgba(0, 0, 0, 0.6)',
            'left': '0',
            'position': 'absolute',
            'top': '0',
            'width': '100%',
            'z-index': '1000',
        });

        overlay.append('<img src="/Content/images/ajax-loader.gif"/>');

        overlay.on('click', function () {
            toggleOverlay();
        });

    } else {
        overlay.remove();
    }
}