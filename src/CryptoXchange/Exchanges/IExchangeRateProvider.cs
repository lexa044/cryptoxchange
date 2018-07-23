namespace CryptoXchange.Exchanges
{
    public interface IExchangeRateProvider
    {
        //Remarks: check providers on https://github.com/butor/blackbird
        CurrencyPair GetCurrentRate();
    }
}
