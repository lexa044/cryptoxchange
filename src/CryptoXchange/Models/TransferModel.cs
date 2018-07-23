namespace CryptoXchange.Models
{
    public class TransferModel
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string FromAddressBase64 { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
