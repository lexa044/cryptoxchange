using System.Data;

using CryptoXchange.Persistence.Model;

namespace CryptoXchange.Persistence.Repositories
{
    public interface ITransferRepository
    {
        void Add(IDbConnection con, IDbTransaction tx, Transfer request);
    }
}
