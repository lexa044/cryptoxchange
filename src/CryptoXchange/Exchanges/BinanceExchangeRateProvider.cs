using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace CryptoXchange.Exchanges
{
    public class BinanceExchangeRateProvider : IExchangeRateProvider
    {
        private const string API_ENDPOINT = "https://api.binance.com/api/v3/ticker/bookTicker?symbol=BTCUSDT";
        private readonly HttpClient _client;

        public BinanceExchangeRateProvider()
        {
            _client = new HttpClient();
        }

        public CurrencyPair GetCurrentRate()
        {
            var exchangeJson = JObject.Parse(_client.GetStringAsync(API_ENDPOINT).Result);
            CurrencyPair response = new CurrencyPair
            {
                From = "BTC",
                To = "USD",
                Ask = decimal.Parse(exchangeJson["askPrice"].ToString()),
                Bid = decimal.Parse(exchangeJson["bidPrice"].ToString()),
                LastPrice = decimal.Parse(exchangeJson["bidPrice"].ToString())
            };

            return response;
        }
    }
}
