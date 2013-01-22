jQuery(document).ready(function() {
    $.globals = {
        errorState: {}
    };
    
    /* ******************************************************************
     * jQuery Ajax default configuration
     */
    $.ajaxSetup({
        error: function (jqHxr, statusText, errorThrown) {
            displayErrorMessage(statusText + ': ' + errorThrown);
        }
    });
    
    /* ******************************************************************
     * Menu highlighting and animation 
     */
    var activeTab = $('#page').attr('class');
    var $menu = $('#main-menu');
    
    if (!!activeTab !== false) {
        $menu.find('li').removeClass('selected');
        $('.' + activeTab, $menu).addClass('selected');
    }

    $('.menu-list-button').on('click', function(e) {
        e.preventDefault();
        $menu.slideToggle(200);
    });
});

/* ******************************************************************
 * Custom js functions
 */

function displayErrorMessage(errorMessage, selector) {
    $.globals.errorState[selector] = true;
    var $container = (!!selector === true) ? $(".validation-message." + selector) : $(".validation-message.All");
    if ($container.size() !== false) {
        $container.html(errorMessage).show();
    } else {
        alert(selector + " error: " + errorMessage);
    }
}

function clearErrorMessage(selector) {
    $.globals.errorState[selector] = false;
    var $container = (!!selector === true) ? $(".validation-message." + selector) : $(".validation-message");
    $container.html("").hide();
}


function errorState() {
    var state = false;

    $.each($.globals.errorState, function (key, value) {
        if (value) {
            return state = true;
        }
    });
    
    return state;
}

// Shorthand logging
function log(str) {
    console.log(str);
}

// Fine grained timing function. Returns time in milliseconds from window.open event is fired
performance.now = (function (window) {
    return window.performance.now ||
           window.performance.mozNow ||
           window.performance.msNow ||
           window.performance.oNow ||
           window.performance.webkitNow ||
           function () {
               return new Date().getTime();
           };
})(window);

// Shorthand method for window.performance.now()
function now() {
    return window.performance.now();
}