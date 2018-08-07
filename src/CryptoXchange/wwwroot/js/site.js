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
        qs: qs
    };

    for (var prop in headers) {
        callOptions.headers[prop] = headers[prop];
    }

    var request = $.ajax(callOptions);
    request.done(function (data) {
        callback(false, data);
    });

    request.fail(function (jqXHR, textStatus) {
        callback(true, textStatus);
    });
};

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

        var context = this;
        var options = {
            method: 'GET',
            endpoint: 'Exchanges/' + _symbol
        };
        _client.request(options, function (err, data) {
            if (!err) {
                _tradeInfo = data;
                $("#txtFundingAddress").val(data.fromAddress);
                $("#imgFundingAddress").attr("src", "https://zxing.org/w/chart?cht=qr&chs=200x200&chld=L&choe=UTF-8&chl=" + data.fromAddress).attr("alt", data.fromAddress);
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
                endpoint: 'Exchanges',
                headers: { 'Content-Type': 'application/json' },
                body: { 'FromAddress': fundingAddress, 'ToAddress': receivingAddress }
            };

            _client.request(options, function (err, data) {
                if (!err) {
                    if (data.fromAddressBase64) {
                        _tradeInfo = data;
                        $("#txtFundingAddress").val(data.fromAddress);
                        $("#imgFundingAddress").attr("src", "https://zxing.org/w/chart?cht=qr&chs=350x350&chld=L&choe=UTF-8&chl=" + data.fromAddress).attr("alt", data.fromAddress);
                        $('#dlgExchangeConfirm').modal('hide');
                    }
                }
            });
        });
    };
    
    serviceFactory.init = _init;

    return serviceFactory;
}();