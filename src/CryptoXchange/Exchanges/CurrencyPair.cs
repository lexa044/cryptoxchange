namespace CryptoXchange.Exchanges
{
    public class CurrencyPair
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal LastPrice { get; set; }
    }
}
