using System;
using System.IO;
using System.Drawing;

using QRCoder;
using static QRCoder.PayloadGenerator;

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
        private readonly QRCodeGenerator _qrService;
        private readonly IContextHolder _contextHolder;
        private readonly IConnectionFactory _iConnectionFactory;
        private readonly ITransferRequestRepository _iTransferRequestRepository;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly DaemonClientFactory _daemonClientFactory;
        private readonly PayoutManager _payoutManager;
        private readonly IJobManager _jobManager;

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
            _qrService = new QRCodeGenerator();
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
                BitcoinAddress generator = new PayloadGenerator.BitcoinAddress(response.FromAddress, null);
                string payload = generator.ToString();
                try
                {
                    QRCodeData qrCodeData = _qrService.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);
                    using (Bitmap bitmap = qrCode.GetGraphic(20))
                    {
                        using (var ms = new MemoryStream())
                        {
                            // save to stream as PNG
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            response.FromAddressBase64 = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.FromAddressBase64 = string.Empty;
                    _logger.Error("GetExchangeForSymbol", ex);
                }

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

                    _jobManager.Add(new Trigger
                    {
                        StartTime = DateTime.Now.AddSeconds(20),
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
                    _logger.Error("GetExchangeForSymbol", ex);
                }

                return response;
            };

            return this.Execute(func);
        }
    }
}
