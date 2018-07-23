using System.Data;
using System.Linq;

using Dapper;

using CryptoXchange.Persistence.Model;
using CryptoXchange.Persistence.Repositories;

namespace CryptoXchange.Persistence.Postgres.Repositories
{
    public class TransferRequestRepository : ITransferRequestRepository
    {
        public TransferRequestRepository()
        {
        }

        public void Add(IDbConnection con, IDbTransaction tx, TransferRequest request)
        {
            var query =
                "INSERT INTO transferrequests(fromCoin, fromAddress, toCoin, toAddress, status, amount, confirmationRequired, confirmationProgress, created, updated) " +
                "VALUES(@fromCoin, @fromAddress, @toCoin, @toAddress, @status, @amount, @confirmationRequired, @confirmationProgress, @created, @updated) RETURNING id;";

            request.Id = con.Query<int>(query, request, tx).Single();
        }

        public void Update(IDbConnection con, IDbTransaction tx, TransferRequest request)
        {
            var query = "UPDATE transferrequests SET status=@status, amount = @amount, confirmationRequired = @confirmationRequired, updated = @updated WHERE id = @id;";

            con.Execute(query, request, tx);
        }

        public void Delete(IDbConnection con, IDbTransaction tx, TransferRequest request)
        {
            var query = "DELETE FROM transferrequests WHERE id = @id;";

            con.Execute(query, request, tx);
        }

        public TransferRequest[] GetPendingRequestsForCoin(IDbConnection con, string coinId)
        {
            var query = "SELECT * FROM transferrequests WHERE fromCoin = @coinId;";
            return con.Query<TransferRequest>(query, new { coinId }).ToArray();
        }
    }
}
