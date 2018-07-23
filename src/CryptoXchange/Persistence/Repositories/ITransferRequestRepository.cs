using System.Data;

using CryptoXchange.Persistence.Model;

namespace CryptoXchange.Persistence.Repositories
{
    public interface ITransferRequestRepository
    {
        void Add(IDbConnection con, IDbTransaction tx, TransferRequest request);
        void Update(IDbConnection con, IDbTransaction tx, TransferRequest request);
        void Delete(IDbConnection con, IDbTransaction tx, TransferRequest request);
        TransferRequest[] GetPendingRequestsForCoin(IDbConnection con, string coinId);
    }
}
