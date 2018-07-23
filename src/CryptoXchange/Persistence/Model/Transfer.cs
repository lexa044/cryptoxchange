using System;

namespace CryptoXchange.Persistence.Model
{
    public class Transfer
    {
        public int Id { get; set; }
        public string FromCoin { get; set; }
        public string FromAddress { get; set; }
        public string ToCoin { get; set; }
        public string ToAddress { get; set; }
        public int Status { get; set; }
        public decimal BidAmount { get; set; }
        public decimal TradeAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Reference { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
