using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoXchange.Exchanges
{
    public class PoloniexExchangeRateProvider : IExchangeRateProvider
    {
        private const string API_ENDPOINT = "https://poloniex.com/public?command=returnTicker";
        private readonly HttpClient _client;

        public PoloniexExchangeRateProvider()
        {
            _client = new HttpClient();
        }

        public CurrencyPair GetCurrentRate()
        {
            var exchangeJson = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(_client.GetStringAsync(API_ENDPOINT).Result);
            CurrencyPair response = new CurrencyPair
            {
                From = "BTC",
                To = "USD"
            };
            if (null != exchangeJson && exchangeJson.ContainsKey("USDT_BTC"))
            {
                response.Ask = decimal.Parse(exchangeJson["USDT_BTC"]["lowestAsk"].ToString());
                response.Bid = decimal.Parse(exchangeJson["USDT_BTC"]["highestBid"].ToString());
                response.LastPrice = decimal.Parse(exchangeJson["USDT_BTC"]["highestBid"].ToString());
            }

            return response;
        }
    }
}
