using System;
using System.Data;

using CryptoXchange.Persistence;

namespace CryptoXchange.Extensions
{
    public static class ConnectionFactoryExtensions
    {
        public static void Run(this IConnectionFactory factory, Action<IDbConnection> action)
        {
            using (var con = factory.OpenConnection())
            {
                action(con);
            }
        }

        public static T Run<T>(this IConnectionFactory factory, Func<IDbConnection, T> func)
        {
            using (var con = factory.OpenConnection())
            {
                return func(con);
            }
        }

        public static void RunTx(this IConnectionFactory factory,
                    Action<IDbConnection, IDbTransaction> action,
                    bool autoCommit = true, IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            using (var con = factory.OpenConnection())
            {
                using (var tx = con.BeginTransaction(isolation))
                {
                    try
                    {
                        action(con, tx);

                        if (autoCommit)
                            tx.Commit();
                    }

                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public static T RunTx<T>(this IConnectionFactory factory,
            Func<IDbConnection, IDbTransaction, T> func,
            bool autoCommit = true, IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            using (var con = factory.OpenConnection())
            {
                using (var tx = con.BeginTransaction(isolation))
                {
                    try
                    {
                        var result = func(con, tx);

                        if (autoCommit)
                            tx.Commit();

                        return result;
                    }

                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
