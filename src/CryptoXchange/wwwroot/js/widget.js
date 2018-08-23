; (function (MYNAMESPACE, undefined) {
    var jQuery;
    var options;
    var overlay, overlay_selector, container_selector, alert_selector, container, alert_container;
    var browser = {};
    var browserGEO = { latitude: 0, longitude: 0 };

    MYNAMESPACE.Widget = function (opts) {
        options = opts;
        overlay_selector = '#_bcWOverlay';
        container_selector = '#_bcWc';
        alert_selector = '#_bcMSGBox';
        container = '_bcWc';
        alert_container = '_bcMSGBox';
        overlay = '_bcWOverlay';
    };

    MYNAMESPACE.initialize = function () {
        initializejQuery();
    };

    function initializejQuery() {
        if (window.jQuery === undefined || minVersion('1.9.1') === false) {
            var script_tag = document.createElement('script');
            script_tag.setAttribute("type", "text/javascript");
            script_tag.setAttribute("src", getProtocol() + "ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js");
            (document.getElementsByTagName("head")[0] || document.documentElement).appendChild(script_tag);
            if (script_tag.attachEvent) {
                script_tag.onreadystatechange = function () {
                    if (this.readyState == 'complete' || this.readyState == 'loaded') {
                        this.onreadystatechange = null;
                        scriptLoadHandler();
                    }
                };
            } else {
                script_tag.onload = scriptLoadHandler;
            }
        } else {
            jQuery = window.jQuery;
            main();
        }
    }

    function getProtocol() {
        return ('https:' == document.location.protocol ? 'https://' : 'http://');
    }

    function minVersion(version) {
        var $vrs = window.jQuery.fn.jquery.split('.'),
            min = version.split('.'),
            prevs = [];
        for (var i = 0, len = $vrs.length; i < len; i++) {
            if (min[i] && parseInt($vrs[i]) < parseInt(min[i])) {
                if (!prevs[i - 1] || prevs[i - 1] == 0)
                    return false;
            } else {
                if (parseInt($vrs[i]) > parseInt(min[i]))
                    prevs[i] = 1;
                else
                    prevs[i] = 0;
            }
        }
        return true;
    }

    function scriptLoadHandler() {
        jQuery = window.jQuery.noConflict();
        main();
    }

    function main() {
        jQuery(document).ready(function () {
            if (options && options.token) {

                defineOldBrowserFunctions();
                detectBrowserSettings();

                if (typeof options.use_uicss === "undefined") options.use_uicss = true;

                loadWidgetCss();
                initializeWidgetContainers();

            }
        });
    }

    function defineOldBrowserFunctions() {
        if (!String.prototype.trim) {
            String.prototype.trim = function () {
                return this.replace(/^\s+|\s+$/g, '');
            }
        }
        if (!Array.prototype.indexOf) {
            Array.prototype.indexOf = function (elt /*, from*/) {
                var len = this.length;

                var from = Number(arguments[1]) || 0;
                from = (from < 0)
                    ? Math.ceil(from)
                    : Math.floor(from);
                if (from < 0)
                    from += len;

                for (; from < len; from++) {
                    if (from in this &&
                        this[from] === elt)
                        return from;
                }
                return -1;
            };
        }
    }

    function detectBrowserSettings() {
        if (/(chrome\/[0-9]{2})/i.test(navigator.userAgent)) {
            browser.agent = navigator.userAgent.match(/(chrome\/[0-9]{2})/i)[0].split("/")[0];
            browser.version = parseInt(navigator.userAgent.match(/(chrome\/[0-9]{2})/i)[0].split("/")[1]);
        } else if (/(firefox\/[0-9]{2})/i.test(navigator.userAgent)) {
            browser.agent = navigator.userAgent.match(/(firefox\/[0-9]{2})/i)[0].split("/")[0];
            browser.version = parseInt(navigator.userAgent.match(/(firefox\/[0-9]{2})/i)[0].split("/")[1]);
        } else if (/(MSIE\ [0-9]{1})/i.test(navigator.userAgent)) {
            browser.agent = navigator.userAgent.match(/(MSIE\ [0-9]{1})/i)[0].split(" ")[0];
            browser.version = parseInt(navigator.userAgent.match(/(MSIE\ [0-9]{1})/i)[0].split(" ")[1]);
        } else if (/(Opera\/[0-9]{1})/i.test(navigator.userAgent)) {
            browser.agent = navigator.userAgent.match(/(Opera\/[0-9]{1})/i)[0].split("/")[0];
            browser.version = parseInt(navigator.userAgent.match(/(Opera\/[0-9]{1})/i)[0].split("/")[1]);
        } else if (/(Trident\/[7]{1})/i.test(navigator.userAgent)) {
            browser.agent = "MSIE";
            browser.version = 11;
        } else {
            browser.agent = false;
            browser.version = false;
        }

        if (/(Windows\ NT\ [0-9]{1}\.[0-9]{1})/.test(navigator.userAgent)) {
            browser.os = "Windows";
            switch (parseFloat(navigator.userAgent.match(/(Windows\ NT\ [0-9]{1}\.[0-9]{1})/)[0].split(" ")[2])) {
                case 6.0:
                    browser.osversion = "Vista";
                    break;
                case 6.1:
                    browser.osversion = "7";
                    break;
                case 6.2:
                    browser.osversion = "8";
                    break;
                default:
                    browser.osversion = false;
            }
        } else if (/(OS\ X\ [0-9]{2}\.[0-9]{1})/.test(navigator.userAgent)) {
            browser.os = "OS X";
            browser.osversion = navigator.userAgent.match(/(OS\ X\ [0-9]{2}\.[0-9]{1})/)[0].split(" ")[2];
        } else if (/(Linux)/.test(navigator.userAgent)) {
            browser.os = "Linux";
            browser.osversion = false;
        }
    }

    function loadWidgetCss() {
        if (options.use_uicss)
            //jQuery('head').append('<link href="' + getProtocol() + 'btc.betchip.io/css/widget.css?v=' + encodeURIComponent(options.token) + '" rel="stylesheet" type="text/css">');
            jQuery('head').append('<link href="http://localhost:50120/css/widget.css?v=' + encodeURIComponent(options.token) + '" rel="stylesheet" type="text/css">');
    }

    function initializeWidgetContainers() {
        if (jQuery(container_selector).size() === 0) {
            jQuery('body').prepend('<div id="' + container + '"></div>');
        }
        if (jQuery(alert_selector).size() === 0) {
            jQuery('body').prepend('<div id="' + alert_container + '"></div>');
        }
        //if (jQuery(overlay_selector).size() === 0) {
        //    jQuery('body').append('<div id="' + overlay + '"></div>');
        //    jQuery(overlay_selector).css({
        //        'display': 'none',
        //        'position': 'fixed',
        //        'background': '#000',
        //        'z-index': 10999,
        //        'left': '0px',
        //        'height': '100%',
        //        'width': '100%',
        //        'top': '0px'
        //    }).click(close_modal);
        //}

        if (jQuery('._bcFixedButton').size() === 0) {
            var html = getMobileFixedShareButtonLayout();
            jQuery('body').append(html);
            jQuery('body').css("padding-bottom", jQuery('div._bcFixedButton').css('height'));
        }

        var theMobileShareBarLabel = 'Betchip';

        $shareButton = jQuery('._bcFixedButton');
        $shareButton.find('._bcShD').html(theMobileShareBarLabel);
        $shareButton.find('._bcSwB').click(function () {
            $shareButton.toggleClass('_bcCollapsed');
        });
        $shareButton.find('._bcShB,._bcShD').click(function () {
            e.click();
        });
    }

    function getMobileFixedShareButtonLayout() {
        return ['<div class="_bcFixedButton _bcTable">',
            '<div class="_bcTableCell"><span class="_bcSwB">&nbsp;</span></div>',
            '<div class="_bcTableCell"><span class="_bcShB">&nbsp;</span></div>',
            '<div class="_bcTableCell"><span class="_bcShD">Share Today!</span></div>',
            '</div>'].join("");
    }

    function getRandomToken() {
        var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXTZabcdefghiklmnopqrstuvwxyz";
        var string_length = 8;
        var randomstring = '';
        for (var i = 0; i < string_length; i++) {
            var rnum = Math.floor(Math.random() * chars.length);
            randomstring += chars.substring(rnum, rnum + 1);
        }
        return randomstring;
    }

    MYNAMESPACE.initialize();

})(window.Betchip = window.Betchip || {});