using Microsoft.Extensions.DependencyInjection;

using CryptoXchange.Persistence.Repositories;

namespace CryptoXchange.Persistence.Postgres.Repositories
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionFactory, PgConnectionFactory>();
            services.AddSingleton<ITransferRequestRepository, TransferRequestRepository>();
            return services;
        }
    }
}
