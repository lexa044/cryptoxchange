using System;

using NLog;

using CryptoXchange.Infrastructure;
using CryptoXchange.Scheduler;
using CryptoXchange.Extensions;

namespace CryptoXchange.Exchanges
{
    public class ExchangeRateUpdater : IJob
    {
        private readonly IJobManager _jobManager;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly IContextHolder _contextHolder;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public ExchangeRateUpdater(IJobManager jobManager, IExchangeRateProvider exchangeRateProvider, IContextHolder contextHolder)
        {
            _jobManager = jobManager;
            _exchangeRateProvider = exchangeRateProvider;
            _contextHolder = contextHolder;
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
            get { return 60; }
        }

        public int FailedAttemptCount { get; set; }

        public void Execute()
        {
            try
            {
                _contextHolder.ExchangeRate = _exchangeRateProvider.GetCurrentRate();

                if (null != _contextHolder.ExchangeRate)
                {
                    //Adjust Bid Price to BTP to USD Exchange Rate
                    decimal btpusd = _contextHolder.Config.ExchangeValue;
                    _contextHolder.ExchangeRate.Bid = (_contextHolder.ExchangeRate.Bid / btpusd);//.TruncateDecimalPlaces(8);
                }

                LastExecuted = DateTime.Now;
                _jobManager.Add(this);
            }
            catch(Exception ex)
            {
                //Re-schedule inmediately on error
                _logger.Error(ex);
                _jobManager.Add(this);
            }
        }
        #endregion
    }
}
