using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace CryptoXchange.Exchanges
{
    public class BitfinexExchangeRateProvider : IExchangeRateProvider
    {
        private const string API_ENDPOINT = "https://api.bitfinex.com/v1/ticker/btcusd";
        private readonly HttpClient _client;

        public BitfinexExchangeRateProvider()
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
                Ask = decimal.Parse(exchangeJson["ask"].ToString()),
                Bid = decimal.Parse(exchangeJson["bid"].ToString()),
                LastPrice = decimal.Parse(exchangeJson["last_price"].ToString())
            };

            return response;
        }
    }
}
