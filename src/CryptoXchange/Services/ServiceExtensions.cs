using Microsoft.Extensions.DependencyInjection;

namespace CryptoXchange.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<ExchangeService, ExchangeService>();
            return services;
        }
    }
}
