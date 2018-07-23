using System.Collections.Concurrent;
using System.Collections.Generic;

using Newtonsoft.Json;

using CryptoXchange.Configuration;
using CryptoXchange.Infrastructure;

namespace CryptoXchange.DaemonInterface
{
    public class DaemonClientFactory
    {
        private readonly IDictionary<CoinType, DaemonClient> _coinClientMap;

        public DaemonClientFactory(IContextHolder contextHolder, JsonSerializerSettings jsonSerializerSettings)
        {
            _coinClientMap = new ConcurrentDictionary<CoinType, DaemonClient>();

            if (null != contextHolder.Config.Coins && contextHolder.Config.Coins.Length > 0)
            {
                foreach (CoinConfig config in contextHolder.Config.Coins)
                {
                    DaemonClient coinClient = new DaemonClient(jsonSerializerSettings, config.WalletPassword);
                    coinClient.Configure(config.Daemons);
                    _coinClientMap[config.Type] = coinClient;
                }
            }
        }

        public DaemonClient GetDaemonClient(CoinType coin)
        {
            DaemonClient coinClient = null;
            _coinClientMap.TryGetValue(coin, out coinClient);
            return coinClient;
        }
    }
}
