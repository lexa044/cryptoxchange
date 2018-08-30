; (function (MYNAMESPACE, undefined) {
    var jQuery;
    var options;
    var overlay, overlay_selector, container_selector, alert_selector, container, alert_container;
    var browser = {};
    var browserGEO = { latitude: 0, longitude: 0 };
    var _tradeInfo = null;
    var receiver;
    var target_origin = 'https://btc.betchip.io';

    MYNAMESPACE.Widget = function (opts) {
        options = opts;
        overlay_selector = '#dlgExchangeConfirm';
        container_selector = '#_bcWc';
        alert_selector = '#_bcMSGBox';
        container = '_bcWc';
        alert_container = '_bcMSGBox';
        overlay = 'dlgExchangeConfirm';
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
                initializeJQueryPlugins();
            }
        });
    }

    function Modal(element, options) {
        this.options = options
        this.$body = jQuery(document.body)
        this.$element = jQuery(element)
        this.$dialog = this.$element.find('.modal-dialog')
        this.$backdrop = null
        this.isShown = null
        this.originalBodyPad = null
        this.scrollbarWidth = 0
        this.ignoreBackdropClick = false

        if (this.options.remote) {
            this.$element
                .find('.modal-content')
                .load(this.options.remote, jQuery.proxy(function () {
                    this.$element.trigger('loaded.bs.modal')
                }, this))
        }
    }

    Modal.VERSION = '3.3.7'
    Modal.TRANSITION_DURATION = 300
    Modal.BACKDROP_TRANSITION_DURATION = 150
    Modal.DEFAULTS = {
        backdrop: true,
        keyboard: true,
        show: true
    }

    Modal.prototype = {
        toggle: function (_relatedTarget) {
            return this.isShown ? this.hide() : this.show(_relatedTarget)
        },
        show: function (_relatedTarget) {
            var that = this
            var e = jQuery.Event('show.bs.modal', { relatedTarget: _relatedTarget })

            this.$element.trigger(e)

            if (this.isShown || e.isDefaultPrevented()) return

            this.isShown = true

            this.checkScrollbar()
            this.setScrollbar()
            this.$body.addClass('modal-open')
            this.escape()
            this.resize()

            this.$element.on('click.dismiss.bs.modal', '[data-dismiss="modal"]', jQuery.proxy(this.hide, this))
            this.$dialog.on('mousedown.dismiss.bs.modal', function () {
                that.$element.one('mouseup.dismiss.bs.modal', function (e) {
                    if (jQuery(e.target).is(that.$element)) that.ignoreBackdropClick = true
                })
            })

            this.backdrop(function () {
                var transition = jQuery.support.transition && that.$element.hasClass('fade')

                if (!that.$element.parent().length) {
                    that.$element.appendTo(that.$body) // don't move modals dom position
                }

                that.$element
                    .show()
                    .scrollTop(0)

                that.adjustDialog()

                if (transition) {
                    that.$element[0].offsetWidth // force reflow
                }

                that.$element.addClass('in')

                that.enforceFocus()

                var e = jQuery.Event('shown.bs.modal', { relatedTarget: _relatedTarget })

                transition ?
                    that.$dialog // wait for modal to slide in
                        .one('bsTransitionEnd', function () {
                            that.$element.trigger('focus').trigger(e)
                        })
                        .emulateTransitionEnd(Modal.TRANSITION_DURATION) :
                    that.$element.trigger('focus').trigger(e)
            })
        },
        hide: function (e) {
            if (e) e.preventDefault()

            e = jQuery.Event('hide.bs.modal')

            this.$element.trigger(e)

            if (!this.isShown || e.isDefaultPrevented()) return

            this.isShown = false

            this.escape()
            this.resize()

            jQuery(document).off('focusin.bs.modal')

            this.$element
                .removeClass('in')
                .off('click.dismiss.bs.modal')
                .off('mouseup.dismiss.bs.modal')

            this.$dialog.off('mousedown.dismiss.bs.modal')

            jQuery.support.transition && this.$element.hasClass('fade') ?
                this.$element
                    .one('bsTransitionEnd', jQuery.proxy(this.hideModal, this))
                    .emulateTransitionEnd(Modal.TRANSITION_DURATION) :
                this.hideModal()
        },
        enforceFocus: function () {
            jQuery(document)
                .off('focusin.bs.modal') // guard against infinite focus loop
                .on('focusin.bs.modal', jQuery.proxy(function (e) {
                    if (document !== e.target &&
                        this.$element[0] !== e.target &&
                        !this.$element.has(e.target).length) {
                        this.$element.trigger('focus')
                    }
                }, this))
        },
        escape: function () {
            if (this.isShown && this.options.keyboard) {
                this.$element.on('keydown.dismiss.bs.modal', jQuery.proxy(function (e) {
                    e.which == 27 && this.hide()
                }, this))
            } else if (!this.isShown) {
                this.$element.off('keydown.dismiss.bs.modal')
            }
        },
        resize: function () {
            if (this.isShown) {
                jQuery(window).on('resize.bs.modal', jQuery.proxy(this.handleUpdate, this))
            } else {
                jQuery(window).off('resize.bs.modal')
            }
        },
        hideModal: function () {
            var that = this
            this.$element.hide()
            this.backdrop(function () {
                that.$body.removeClass('modal-open')
                that.resetAdjustments()
                that.resetScrollbar()
                that.$element.trigger('hidden.bs.modal')
            })
        },
        removeBackdrop: function () {
            this.$backdrop && this.$backdrop.remove()
            this.$backdrop = null
        },
        backdrop: function (callback) {
            var that = this
            var animate = this.$element.hasClass('fade') ? 'fade' : ''

            if (this.isShown && this.options.backdrop) {
                var doAnimate = jQuery.support.transition && animate

                this.$backdrop = jQuery(document.createElement('div'))
                    .addClass('modal-backdrop ' + animate)
                    .appendTo(this.$body)

                this.$element.on('click.dismiss.bs.modal', jQuery.proxy(function (e) {
                    if (this.ignoreBackdropClick) {
                        this.ignoreBackdropClick = false
                        return
                    }
                    if (e.target !== e.currentTarget) return
                    this.options.backdrop == 'static'
                        ? this.$element[0].focus()
                        : this.hide()
                }, this))

                if (doAnimate) this.$backdrop[0].offsetWidth // force reflow

                this.$backdrop.addClass('in')

                if (!callback) return

                doAnimate ?
                    this.$backdrop
                        .one('bsTransitionEnd', callback)
                        .emulateTransitionEnd(Modal.BACKDROP_TRANSITION_DURATION) :
                    callback()

            } else if (!this.isShown && this.$backdrop) {
                this.$backdrop.removeClass('in')

                var callbackRemove = function () {
                    that.removeBackdrop()
                    callback && callback()
                }
                jQuery.support.transition && this.$element.hasClass('fade') ?
                    this.$backdrop
                        .one('bsTransitionEnd', callbackRemove)
                        .emulateTransitionEnd(Modal.BACKDROP_TRANSITION_DURATION) :
                    callbackRemove()

            } else if (callback) {
                callback()
            }
        },
        handleUpdate: function () {
            this.adjustDialog()
        },
        adjustDialog: function () {
            var modalIsOverflowing = this.$element[0].scrollHeight > document.documentElement.clientHeight

            this.$element.css({
                paddingLeft: !this.bodyIsOverflowing && modalIsOverflowing ? this.scrollbarWidth : '',
                paddingRight: this.bodyIsOverflowing && !modalIsOverflowing ? this.scrollbarWidth : ''
            })
        },
        resetAdjustments: function () {
            this.$element.css({
                paddingLeft: '',
                paddingRight: ''
            })
        },
        checkScrollbar: function () {
            var fullWindowWidth = window.innerWidth
            if (!fullWindowWidth) { // workaround for missing window.innerWidth in IE8
                var documentElementRect = document.documentElement.getBoundingClientRect()
                fullWindowWidth = documentElementRect.right - Math.abs(documentElementRect.left)
            }
            this.bodyIsOverflowing = document.body.clientWidth < fullWindowWidth
            this.scrollbarWidth = this.measureScrollbar()
        },
        setScrollbar: function () {
            var bodyPad = parseInt((this.$body.css('padding-right') || 0), 10)
            this.originalBodyPad = document.body.style.paddingRight || ''
            if (this.bodyIsOverflowing) this.$body.css('padding-right', bodyPad + this.scrollbarWidth)
        },
        resetScrollbar: function () {
            this.$body.css('padding-right', this.originalBodyPad)
        },
        measureScrollbar: function () { // thx walsh
            var scrollDiv = document.createElement('div')
            scrollDiv.className = 'modal-scrollbar-measure'
            this.$body.append(scrollDiv)
            var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth
            this.$body[0].removeChild(scrollDiv)
            return scrollbarWidth
        }
    }

    function ModalPlugin(option, _relatedTarget) {
        return this.each(function () {
            var $this = jQuery(this)
            var data = $this.data('bs.modal')
            var options = jQuery.extend({}, Modal.DEFAULTS, $this.data(), typeof option == 'object' && option)

            if (!data) $this.data('bs.modal', (data = new Modal(this, options)))
            if (typeof option == 'string') data[option](_relatedTarget)
            else if (options.show) data.show(_relatedTarget)
        })
    }

    function transitionEnd() {
        var el = document.createElement('bootstrap')

        var transEndEventNames = {
            WebkitTransition: 'webkitTransitionEnd',
            MozTransition: 'transitionend',
            OTransition: 'oTransitionEnd otransitionend',
            transition: 'transitionend'
        }

        for (var name in transEndEventNames) {
            if (el.style[name] !== undefined) {
                return { end: transEndEventNames[name] }
            }
        }

        return false // explicit for ie8 (  ._.)
    }

    function initializeJQueryPlugins() {

        var old = jQuery.fn.modal
        jQuery.fn.modal = ModalPlugin
        jQuery.fn.modal.Constructor = Modal

        // http://blog.alexmaccaw.com/css-transitions
        jQuery.fn.emulateTransitionEnd = function (duration) {
            var called = false
            var $el = this
            jQuery(this).one('bsTransitionEnd', function () { called = true })
            var callback = function () { if (!called) jQuery($el).trigger(jQuery.support.transition.end) }
            setTimeout(callback, duration)
            return this
        }

        jQuery.support.transition = transitionEnd()

        jQuery.event.special.bsTransitionEnd = {
            bindType: jQuery.support.transition.end,
            delegateType: jQuery.support.transition.end,
            handle: function (e) {
                if (jQuery(e.target).is(this)) return e.handleObj.handler.apply(this, arguments)
            }
        }

        jQuery.fn.modal.noConflict = function () {
            jQuery.fn.modal = old
            return this
        }

        jQuery(document).on('click.bs.modal.data-api', '[data-toggle="modal"]', function (e) {
            var $this = jQuery(this)
            var href = $this.attr('href')
            var $target = jQuery($this.attr('data-target') || (href && href.replace(/.*(?=#[^\s]+$)/, ''))) // strip for ie7
            var option = $target.data('bs.modal') ? 'toggle' : jQuery.extend({ remote: !/#/.test(href) && href }, $target.data(), $this.data())

            if ($this.is('a')) e.preventDefault()

            $target.one('show.bs.modal', function (showEvent) {
                if (showEvent.isDefaultPrevented()) return // only register focus restorer if modal will actually get shown
                $target.one('hidden.bs.modal', function () {
                    $this.is(':visible') && $this.trigger('focus')
                })
            })
            ModalPlugin.call($target, option, this)
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
            jQuery('head').append('<link href="' + getProtocol() + 'btc.betchip.io/css/widget.css?v=' + encodeURIComponent(options.token) + '" rel="stylesheet" type="text/css">');
    }

    function initializeWidgetContainers() {
        if (options && options.shareSelector) {
            jQuery(options.shareSelector).attr("data-toggle", "modal").attr("data-target", "#dlgExchangeConfirm");
        }
        else if (jQuery('._bcFixedButton').size() === 0) {
            var html = getLinkerFixedButtonLayout();
            jQuery('body').append(html);
        }

        if (jQuery(overlay_selector).size() === 0) {
            var html = getSettingDialogLayout();
            jQuery('body').append(html);
            jQuery('body').append('<iframe id="_bcifrm" src="https://btc.betchip.io/parent.html" style="width:0;height:0;border: 0;border: none;"></iframe>');

            jQuery("#btnConfirmTransfer").on("click", function (e) {
                e.preventDefault();

                var receivingAddress = jQuery("#txtReceivingAddress").val();
                var fundingAddress = _tradeInfo.fromAddress;
                if (receivingAddress.length < 26 || receivingAddress.length > 35) {
                    alert('Invalid receiving address, please enter a valid one.');
                    return false;
                }

                let re = /^[A-Z0-9]+$/i;
                if (!re.test(receivingAddress)) {
                    alert('Invalid receiving address, please enter a valid one.');
                    return false;
                }

                var receiver = document.getElementById('_bcifrm').contentWindow;
                receiver.postMessage({ 'task': '2', 't1': fundingAddress, 't2': receivingAddress }, target_origin);
            });

            jQuery('#dlgExchangeConfirm').on('show.bs.modal', function (event) {
                if (_tradeInfo == null) {
                    var receiver = document.getElementById('_bcifrm').contentWindow;
                    receiver.postMessage({ 'task': '1' }, target_origin);
                }
            })

            initializePostMessage();

            //setTimeout(function () {
            //    var receiver = document.getElementById('_bcifrm').contentWindow;
            //    receiver.postMessage({ 'task': '1' }, target_origin);
            //}, 1000);
        }
    }

    function handleMessage(e) {
        var task = e.data['task']; // task received in postMessage
        if (task === 'r1') {
            _tradeInfo = { fromAddress: e.data['r'] };
            jQuery("#txtFundingAddress").val(_tradeInfo.fromAddress);
            jQuery("#imgFundingAddress").attr("src", "https://zxing.org/w/chart?cht=qr&chs=200x200&chld=L&choe=UTF-8&chl=" + _tradeInfo.fromAddress).attr("alt", _tradeInfo.fromAddress);
        }
        else if (task === 'r2') {
            _tradeInfo = { fromAddress: e.data['r'] };
            jQuery("#txtFundingAddress").val(_tradeInfo.fromAddress);
            jQuery("#imgFundingAddress").attr("src", "https://zxing.org/w/chart?cht=qr&chs=200x200&chld=L&choe=UTF-8&chl=" + _tradeInfo.fromAddress).attr("alt", _tradeInfo.fromAddress);
            jQuery("#txtReceivingAddress").val("");
            jQuery('#dlgExchangeConfirm').modal('hide');
        }
    }

    function initializePostMessage() {
        if (window.addEventListener) {
            window.addEventListener("message", handleMessage);
        } else {
            window.attachEvent("onmessage", handleMessage);
        }
    }

    function getLinkerFixedButtonLayout() {
        return ['<a class="_bcFixedButton" data-toggle="modal" data-target="#dlgExchangeConfirm">',
            '<span class="bcglyphicon bcglyphicon-btc" aria-hidden="true"></span>',
            '</a>'].join("");
    }

    function getSettingDialogLayout() {
        return ['<div class="modal fade" id="dlgExchangeConfirm" tabindex="-1" role="dialog" aria-labelledby="dlgExchangeConfirmLabel" aria-hidden="true">',
            '<div class="modal-dialog" role="document">',
            '<div class="modal-content">',
            '<div class="modal-header">',
            '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>',
            '<h5 class="modal-title" id="dlgExchangeConfirmLabel">Transfer Confirmation</h5>',
            '</div>',
            '<div class="modal-body">',
            '<form>',
            '<fieldset>',
            '<p>',
            '<label for="txtFundingAddress">Fund the following Bitcoin address (BTC)</label>',
            '<input type="text" id="txtFundingAddress" placeholder="BTC" readonly="readonly" class="form-control">',
            '</p>',
            '<p>',
            '<img id="imgFundingAddress" width="200" height="200" title="Fund this Bitcoin address (BTC)"/>',
            '</p>',
            '<p>',
            '<label for="txtReceivingAddress">Enter your receiving Betchip address (BTP)</label>',
            '<input type="text" id="txtReceivingAddress" placeholder="BTP" class="form-control">',
            '</p>',
            '</fieldset>',
            '</form>',
            '</div>',
            '<div class="modal-footer">',
            '<button type="button" class="btn btn-default" id="btnConfirmTransfer">Submit</button>',
            '<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>',
            '</div>',
            '</div>',
            '</div>',
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