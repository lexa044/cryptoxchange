var Xchange = {};

Xchange.Client = function (options) {
    this.URI = options.URI || '/api/';
};

Xchange.Client.prototype.request = function (options, callback) {
    var method = options.method || 'GET';
    var endpoint = options.endpoint;
    var body = options.body || {};
    var qs = options.qs || {};
    var headers = options.headers || {};

    var uri = this.URI + endpoint;
    var callOptions = {
        method: method,
        url: uri,
        data: body,
        qs: qs,
        headers: {
            'User-Agent': 'Xchange-Client 1.0.0'
        }
    };

    for (var prop in headers) {
        callOptions.headers[prop] = headers[prop];
    }

    var request = $.ajax(callOptions);
    request.done(function (data) {
        callback(false, data);
    });

    request.fail(function (jqXHR, textStatus) {
        //alert("Request failed: " + textStatus);
        callback(true, textStatus);
    });
};

var LocationService = function () {
    var locationServiceFactory = {};
    var _location_timeout;
    var options = {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0
    };
    var _callback;
    var _getLocation = function (callback) {
        _callback = callback;
        if (navigator.geolocation) {
            _location_timeout = setTimeout(function () {
                clearTimeout(_location_timeout);
                _handleError({ code: 'TIMEOUT' });
            }, 10000);
            navigator.geolocation.getCurrentPosition(_handleSuccess, _handleError, options);
        } else {
            log('Geolocation is not supported by this browser.');
            _handleError({ code: 'TIMEOUT' });
        }
    };
    var _handleSuccess = function (position) {
        clearTimeout(_location_timeout);
        _callback(position.coords.latitude, position.coords.longitude);
    };
    var _handleError = function (error) {
        switch (error.code) {
            case error.PERMISSION_DENIED:
                log('User denied the request for Geolocation.');
                break;
            case error.POSITION_UNAVAILABLE:
                log('Location information is unavailable.');
                break;
            case error.TIMEOUT:
                log('The request to get user location timed out.');
                break;
            case error.UNKNOWN_ERROR:
                log('An unknown error occurred.');
                break;
        }
        _callback(0, 0);
    }

    locationServiceFactory.getLocation = _getLocation;

    return locationServiceFactory;
}();

var ExchangeService = function () {
    var serviceFactory = {};
    var _fromInput;
    var _toInput;
    var _exchange = 1;
    var _symbol = 'btcusd';
    var _client = new Xchange.Client({});
    var _tradeInfo;

    var _inputOn = function (event) {
        var result = parseFloat(_fromInput.val()) * _exchange;
        _toInput.val(result.toFixed(3));
    };

    var _inputOff = function (event) {

    };

    var _updateTradeParams = function () {

    };

    var _init = function (fromInput, toInput, btcusdRate) {
        _fromInput = $(fromInput);
        _toInput = $(toInput);
        _exchange = btcusdRate;

        _fromInput.off("keyup", _inputOff);
        _fromInput.on("keyup", _inputOn);

        var targetEndpoint = "/api/Exchanges?key=";
        var context = this;

        var options = {
            method: 'GET',
            endpoint: 'Exchanges/' + _symbol
        };
        _client.request(options, function (err, data) {
            if (!err) {
                _tradeInfo = data;
                $("#txtFundingAddress").val(data.fromAddress);
                $("#imgFundingAddress").attr("src", "data:image/png;base64," + data.fromAddressBase64).attr("alt", data.fromAddress);
            }
        });

        $("#btnConfirmTransfer").on("click", function (e) {
            e.preventDefault();

            var receivingAddress = $("#txtReceivingAddress").val();
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

            var options = {
                method: 'POST',
                endpoint: 'Exchanges/transfer',
                body: { 'FromAddress': fundingAddress, 'ToAddress': receivingAddress }
            };

            _client.request(options, function (err, data) {
                if (!err) {
                    if (data.fromAddressBase64) {
                        _tradeInfo = data;
                        $("#txtFundingAddress").val(data.fromAddress);
                        $("#imgFundingAddress").attr("src", "data:image/png;base64," + data.fromAddressBase64).attr("alt", data.fromAddress);
                        $('#dlgExchangeConfirm').modal('hide');
                    }
                }
            });
        });
    };
    
    //http://bootstrap-ecommerce.com/
    serviceFactory.init = _init;

    return serviceFactory;
}();