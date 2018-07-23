using CryptoXchange.Configuration;
using CryptoXchange.Exchanges;

namespace CryptoXchange.Infrastructure
{
    public interface IContextHolder
    {
        CurrencyPair ExchangeRate { get; set; }
        CXConfig Config { get;}
    }
}
