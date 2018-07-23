using System.Data;

namespace CryptoXchange.Persistence
{
    public interface IConnectionFactory
    {
        IDbConnection OpenConnection();
    }
}
