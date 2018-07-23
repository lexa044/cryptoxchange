using System.Data;
using System.Linq;

using Dapper;

using CryptoXchange.Persistence.Model;
using CryptoXchange.Persistence.Repositories;

namespace CryptoXchange.Persistence.Postgres.Repositories
{
    public class TransferRepository: ITransferRepository
    {
        public TransferRepository()
        {
        }

        public void Add(IDbConnection con, IDbTransaction tx, Transfer request)
        {
            var query =
                "INSERT INTO transfers(fromCoin, fromAddress, toCoin, toAddress, status, bidAmount, tradeAmount, exchangeRate, reference, created, updated) " +
                "VALUES(@fromCoin, @fromAddress, @toCoin, @toAddress, @status, @bidAmount, @tradeAmount, @exchangeRate, @reference, @created, @updated) RETURNING id;";

            request.Id = con.Query<int>(query, request, tx).Single();
        }
    }
}
