using System;
using System.Data;

using Npgsql;

using CryptoXchange.Infrastructure;
using CryptoXchange.Configuration;

namespace CryptoXchange.Persistence.Postgres
{
    public class PgConnectionFactory : IConnectionFactory
    {

        private readonly IContextHolder _contextHolder;
        private readonly string _connectionString;

        public PgConnectionFactory(IContextHolder contextHolder)
        {
            _contextHolder = contextHolder;

            DatabaseConfig pgConfig = contextHolder.Config.Persistence.Postgres;
            // validate config
            if (string.IsNullOrEmpty(pgConfig.Host))
                throw new Exception("Postgres configuration: invalid or missing 'host'");

            if (pgConfig.Port == 0)
                throw new Exception("Postgres configuration: invalid or missing 'port'");

            if (string.IsNullOrEmpty(pgConfig.Database))
                throw new Exception("Postgres configuration: invalid or missing 'database'");

            if (string.IsNullOrEmpty(pgConfig.User))
                throw new Exception("Postgres configuration: invalid or missing 'user'");

            // build connection string
            _connectionString = $"Server={pgConfig.Host};Port={pgConfig.Port};Database={pgConfig.Database};User Id={pgConfig.User};Password={pgConfig.Password};CommandTimeout=900;";
        }


        public IDbConnection OpenConnection()
        {
            var con = new NpgsqlConnection(_connectionString);
            con.Open();
            return con;
        }
    }
}
