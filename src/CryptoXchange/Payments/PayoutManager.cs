using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

using CryptoXchange.Configuration;
using CryptoXchange.DaemonInterface;
using CryptoXchange.Extensions;
using CryptoXchange.Infrastructure;
using CryptoXchange.Persistence;
using CryptoXchange.Persistence.Model;
using CryptoXchange.Persistence.Repositories;
using CryptoXchange.Scheduler;
using CryptoXchange.Notifications;

namespace CryptoXchange.Payments
{
    public class PayoutManager : IJob
    {
        private readonly IJobManager _jobManager;
        private readonly IContextHolder _contextHolder;
        private readonly ITransferRequestRepository _iTransferRequestRepository;
        private readonly ITransferRepository _iTransferRepository;
        private readonly IConnectionFactory _iConnectionFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly DaemonClientFactory _daemonClientFactory;
        private readonly NotificationService _notificationService;
        private volatile bool _updatingThroughput;

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public PayoutManager(IJobManager jobManager, 
            IContextHolder contextHolder, 
            ITransferRequestRepository iTransferRequestRepository,
            ITransferRepository iTransferRepository,
            IConnectionFactory iConnectionFactory,
            JsonSerializerSettings jsonSerializerSettings,
            DaemonClientFactory daemonClientFactory,
            NotificationService notificationService)
        {
            _jobManager = jobManager;
            _contextHolder = contextHolder;
            _iTransferRequestRepository = iTransferRequestRepository;
            _iTransferRepository = iTransferRepository;
            _iConnectionFactory = iConnectionFactory;
            _jsonSerializerSettings = jsonSerializerSettings;
            _daemonClientFactory = daemonClientFactory;
            _notificationService = notificationService;
            _updatingThroughput = false;
            LastExecuted = DateTime.Now.AddSeconds(-RepetitionIntervalInSeconds * 2);

            _jobManager.Add(this);
        }

        #region [ IJob - Implementation ]

        public DateTime LastExecuted { get; set; }

        public bool IsRepeatable
        {
            get { return false; }
        }

        public int RepetitionIntervalInSeconds
        {
            get { return 60 * 3; }//Every 3 minutes
        }

        public int FailedAttemptCount { get; set; }

        public void Execute()
        {
            try
            {
                if (!_updatingThroughput)
                {
                    ProcessTransferRequests();

                    LastExecuted = DateTime.Now;
                    _jobManager.Add(this);
                }
            }
            catch (Exception ex)
            {
                //Re-schedule inmediately on error
                _updatingThroughput = false;
                _logger.Error(ex);
                _jobManager.Add(this);
            }
        }
        #endregion

        public void ProcessTransferRequests(TransferRequest[] requests)
        {
            InternalProcessTransferRequests(requests);
        }

        public void ProcessTransferRequests()
        {
            TransferRequest[] requests = _iConnectionFactory.Run(con => _iTransferRequestRepository.GetPendingRequestsForCoin(con, CoinType.BTC.ToString()));

            InternalProcessTransferRequests(requests);
        }

        private void InternalProcessTransferRequests(TransferRequest[] requests)
        {
            if (!_updatingThroughput)
            {
                _updatingThroughput = true;
                _logger.Debug("Executing Payout Manager.");

                if (null != requests && requests.Length > 0)
                {
                    DaemonClient fromDaemonClient = _daemonClientFactory.GetDaemonClient(CoinType.BTC);
                    DaemonClient toDaemonClient = _daemonClientFactory.GetDaemonClient(CoinType.BTP);
                    DateTime timeStamp = DateTime.Now;
                    foreach (TransferRequest request in requests)
                    {
                        UpdateTransferRequestStatus(request, fromDaemonClient, timeStamp);

                        PayoutTransferRequest(request, fromDaemonClient, toDaemonClient, timeStamp);
                    }
                }

                _updatingThroughput = false;
            }
        }

        private void UpdateTransferRequestStatus(TransferRequest request, DaemonClient daemonClient, DateTime timeStamp)
        {
            if (null != daemonClient)
            {
                object[] args = new object[]
                {
                    request.FromAddress,// address
                    (request.Status == (int)TransferStatus.Unknown)? 0: request.ConfirmationRequired // minconf
                };

                var balanceResult = daemonClient.ExecuteCmdSingleAsync<object>(BlockchainConstants.BitcoinCommands.GetReceivedByAddress, args, _jsonSerializerSettings).Result;
                if (null == balanceResult.Error)
                {
                    decimal amount = (decimal)(double)balanceResult.Response;
                    if(request.Status == (int)TransferStatus.Unknown && amount > 0)
                    {
                        request.Amount = amount;
                        request.Status = (int)TransferStatus.Pending;
                        request.ConfirmationRequired = CalculateConfirmationRequired(amount);
                        request.Updated = timeStamp;
                    }
                    else if (request.Status == (int)TransferStatus.Pending && amount > 0)
                    {
                        request.Status = (int)TransferStatus.Confirmed;
                        request.Updated = timeStamp;
                    }
                }
                else
                {
                    _logger.Error($"Daemon returned error: {balanceResult.Error.Message} code {balanceResult.Error.Code}");
                }
            }
        }

        private void PayoutTransferRequest(TransferRequest request, DaemonClient fromDaemonClient, DaemonClient toDaemonClient, DateTime timeStamp)
        {
            if (null != fromDaemonClient && null != toDaemonClient)
            {
                if (request.Status == (int)TransferStatus.Confirmed && request.Updated == timeStamp)
                {
                    decimal exchangeRate = _contextHolder.ExchangeRate.Bid;
                    if(exchangeRate > 0)
                    {
                        decimal tradeAmount = (request.Amount * exchangeRate);
                        decimal feed = (tradeAmount * 0.05m);
                        decimal amount = (tradeAmount - feed);

                        Transfer transfer = new Transfer
                        {
                            BidAmount = request.Amount,
                            Created = request.Created,
                            Updated = timeStamp,
                            ExchangeRate = exchangeRate,
                            FromAddress = request.FromAddress,
                            FromCoin = request.FromCoin,
                            Status = request.Status,
                            ToAddress = request.ToAddress,
                            ToCoin = request.ToCoin,
                            TradeAmount = amount
                        };

                        transfer.Reference = PayoutToAddress(request.ToAddress, amount, toDaemonClient);

                        if (!string.IsNullOrEmpty(transfer.Reference))
                        {
                            PersistPayment(request, transfer);
                        }
                    }
                }
                else if (request.Status == (int)TransferStatus.Pending && request.Updated == timeStamp)
                {
                    _iConnectionFactory.Run(con => _iTransferRequestRepository.Update(con, null, request));
                }
            }
        }

        private string PayoutToAddress(string address, decimal amount, DaemonClient daemonClient)
        {
            string txId = string.Empty;
            object[] args = new object[]
                {
                    address,    // address
                    amount,     // amount
                    "CX Trade", // comment
                    "Payout",   // comment_to
                    false       // subtractfeefromamount  
                };

            bool didUnlockWallet = false;
            // send command
            tryTransfer:
            var result = daemonClient.ExecuteCmdSingleAsync<string>(BlockchainConstants.BitcoinCommands.SendToAddress, args, _jsonSerializerSettings).Result;

            if (result.Error == null)
            {
                SafeLockWallet(daemonClient, didUnlockWallet);

                // check result
                txId = result.Response;

                if (string.IsNullOrEmpty(txId))
                    _logger.Error($"{BlockchainConstants.BitcoinCommands.SendToAddress} did not return a transaction id!");
            }
            else
            {
                if (result.Error.Code == (int)BlockchainConstants.BitcoinRPCErrorCode.RPC_WALLET_UNLOCK_NEEDED && !didUnlockWallet)
                {
                    if (!string.IsNullOrEmpty(daemonClient.WalletPassword))
                    {
                        _logger.Info("Unlocking wallet");

                        var unlockResult = daemonClient.ExecuteCmdSingleAsync<JToken>(BlockchainConstants.BitcoinCommands.WalletPassphrase, new[]
                        {
                            (object) daemonClient.WalletPassword,
                            (object) 5  // unlock for N seconds
                        }).Result;

                        if (unlockResult.Error == null)
                        {
                            didUnlockWallet = true;
                            goto tryTransfer;
                        }
                        else
                            _logger.Error($"{BlockchainConstants.BitcoinCommands.WalletPassphrase} returned error: {result.Error.Message} code {result.Error.Code}");
                    }

                    else
                        _logger.Error($"Wallet is locked but walletPassword was not configured. Unable to send funds.");
                }
                else if (result.Error.Code == (int)BlockchainConstants.BitcoinRPCErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS)
                {
                    SafeLockWallet(daemonClient, didUnlockWallet);
                    _notificationService.NotifyPaymentFailure(address, amount, "Not enough funds in wallet or account.");
                }
                else
                {
                    _logger.Error($"{BlockchainConstants.BitcoinCommands.SendMany} returned error: {result.Error.Message} code {result.Error.Code}");
                }
            }

            return txId;
        }

        private static void SafeLockWallet(DaemonClient daemonClient, bool didUnlockWallet)
        {
            if (didUnlockWallet)
            {
                // lock wallet
                _logger.Info("Locking wallet");
                var walletLockResponse = daemonClient.ExecuteCmdSingleAsync<JToken>(BlockchainConstants.BitcoinCommands.WalletLock).Result;
            }
        }

        private void PersistPayment(TransferRequest request, Transfer response)
        {
            _iConnectionFactory.RunTx((con, tx) =>
            {
                _iTransferRepository.Add(con, tx, response);
                _iTransferRequestRepository.Delete(con, tx, request);
            });
        }

        private int CalculateConfirmationRequired(decimal amount)
        {
            int response = 1;

            if (amount > 50)
                response = 6;
            else if (amount > 10)
                response = 4;
            else if (amount >= 1)
                response = 2;
                    
            return response;
        }
    }
}
