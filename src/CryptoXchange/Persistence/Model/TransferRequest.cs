using System;

namespace CryptoXchange.Persistence.Model
{
    public class TransferRequest
    {
        public int Id { get; set; }
        public string FromCoin { get; set; }
        public string FromAddress { get; set; }
        public string ToCoin { get; set; }
        public string ToAddress { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public int ConfirmationRequired { get; set; }
        public int ConfirmationProgress { get; set; }
    }
}
