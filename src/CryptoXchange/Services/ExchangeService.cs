using System;

using Newtonsoft.Json;

using CryptoXchange.DaemonInterface;
using CryptoXchange.Infrastructure;
using CryptoXchange.Models;
using CryptoXchange.Persistence.Repositories;
using CryptoXchange.Persistence.Model;
using CryptoXchange.Persistence;
using CryptoXchange.Configuration;
using CryptoXchange.Extensions;
using CryptoXchange.Payments;
using CryptoXchange.Scheduler;

namespace CryptoXchange.Services
{
    public class ExchangeService : BaseService
    {
        private readonly IContextHolder _contextHolder;
        private readonly IConnectionFactory _iConnectionFactory;
        private readonly ITransferRequestRepository _iTransferRequestRepository;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly DaemonClientFactory _daemonClientFactory;
        private readonly PayoutManager _payoutManager;
        private readonly IJobManager _jobManager;
        private DateTime _lastUpdated;

        public ExchangeService(IContextHolder contextHolder, 
            ITransferRequestRepository iTransferRequestRepository, 
            IConnectionFactory iConnectionFactory, 
            JsonSerializerSettings jsonSerializerSettings,
            DaemonClientFactory daemonClientFactory,
            PayoutManager payoutManager,
            IJobManager jobManager)
        {
            _contextHolder = contextHolder;
            _iTransferRequestRepository = iTransferRequestRepository;
            _iConnectionFactory = iConnectionFactory;
            _jsonSerializerSettings = jsonSerializerSettings;
            _daemonClientFactory = daemonClientFactory;
            _payoutManager = payoutManager;
            _jobManager = jobManager;
        }

        private void OnHashTxReceived(string tx)
        {
            //Need to slow it down a bit
            if (_lastUpdated.AddSeconds(5) < DateTime.Now)
            {
                _lastUpdated = DateTime.Now;
                _payoutManager.ProcessTransferRequests();
                _lastUpdated = DateTime.Now;
            }
        }

        private void OnHashTxError()
        {
            _logger.Error("Error reading ZMQ:hashtx");
        }

        internal ServiceResponse<TransferModel> GetTransferRequestForSymbol(string symbol)
        {
            Func<TransferModel> func = delegate
            {
                string btcAddress = "17QnVor1B6oK1rWnVVBrdX9gFzVkZZbhDm";
                DaemonClient coinClient = _daemonClientFactory.GetDaemonClient(CoinType.BTC);
                if(null != coinClient)
                {
                    var daemonResponse = coinClient.ExecuteCmdSingleAsync<string>(BlockchainConstants.BitcoinCommands.GetNewAddress, null, _jsonSerializerSettings).Result;
                    if (null == daemonResponse.Error)
                    {
                        btcAddress = daemonResponse.Response;
                    }
                    else
                    {
                        _logger.Error($"Daemon returned error: {daemonResponse.Error.Message} code {daemonResponse.Error.Code}");
                    }
                }

                TransferModel response = new TransferModel();
                response.FromAddress = btcAddress;
                if (null != _contextHolder.ExchangeRate)
                    response.ExchangeRate = _contextHolder.ExchangeRate.Bid;

                return response;
            };

            return this.Execute(func);
        }

        internal ServiceResponse<TransferModel> HandleTransfer(TransferModel request)
        {
            Func<TransferModel> func = delegate
            {
                TransferModel response = new TransferModel()
                {
                    FromAddress = request.FromAddress
                };
                try
                {
                    TransferRequest transfer = new TransferRequest()
                    {
                        Amount = 0,
                        ConfirmationProgress = 0,
                        ConfirmationRequired = 0,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        FromAddress = request.FromAddress,
                        FromCoin = CoinType.BTC.ToString(),
                        Status = (int)TransferStatus.Unknown,
                        ToAddress = request.ToAddress,
                        ToCoin = CoinType.BTP.ToString()
                    };

                    _iConnectionFactory.Run(con => _iTransferRequestRepository.Add(con, null, transfer));
                    //https://stackoverflow.com/questions/47487241/rawtx-rawblock-zmq-at-the-same-time
                    _jobManager.Add(new Trigger
                    {
                        StartTime = DateTime.Now.AddSeconds(25),
                        TaskAction = () =>
                        {
                            _payoutManager.ProcessTransferRequests(new TransferRequest[] { transfer });
                        }
                    });

                    response = GetTransferRequestForSymbol("btcusd").Result;
                }
                catch (Exception ex)
                {
                    response.FromAddressBase64 = string.Empty;
                    _logger.Error(ex);
                }

                return response;
            };

            return this.Execute(func);
        }
    }
}
