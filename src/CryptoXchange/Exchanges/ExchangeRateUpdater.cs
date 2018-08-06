using System;

using CryptoXchange.Infrastructure;
using CryptoXchange.Scheduler;

namespace CryptoXchange.Exchanges
{
    public class ExchangeRateUpdater : IJob
    {
        private readonly IJobManager _jobManager;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly IContextHolder _contextHolder;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ExchangeRateUpdater));

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
