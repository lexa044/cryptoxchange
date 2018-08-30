using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using CryptoXchange.Scheduler;
using CryptoXchange.Infrastructure;
using CryptoXchange.Exchanges;
using CryptoXchange.Services;
using CryptoXchange.Persistence.Postgres.Repositories;
using CryptoXchange.DaemonInterface;
using CryptoXchange.Payments;
using CryptoXchange.Persistence;
using CryptoXchange.Persistence.Postgres;
using CryptoXchange.Persistence.Repositories;
using CryptoXchange.Notifications;
using System.Linq;
using CryptoXchange.Middlewares;

namespace CryptoXchange
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            IJobManager jobManager = new JobManager();
            IExchangeRateProvider exchangeRateProvider = new BitfinexExchangeRateProvider();
            IContextHolder contextHolder = new ContextHolder(Configuration, jsonSerializerSettings);
            DaemonClientFactory daemonClientFactory = new DaemonClientFactory(contextHolder, jsonSerializerSettings);
            IConnectionFactory dbFactory = new PgConnectionFactory(contextHolder);
            ITransferRequestRepository transferRequestRepository = new TransferRequestRepository();
            ITransferRepository transferRepository = new TransferRepository();
            NotificationService notificationService = new NotificationService(contextHolder, jsonSerializerSettings);

            services.AddSingleton<IContextHolder>(contextHolder);
            services.AddSingleton<IExchangeRateProvider>(exchangeRateProvider);
            services.AddSingleton<IJobManager>(jobManager);
            services.AddSingleton<JsonSerializerSettings>(jsonSerializerSettings);
            services.AddSingleton<DaemonClientFactory>(daemonClientFactory);
            services.AddSingleton<IConnectionFactory>(dbFactory);
            services.AddSingleton<ITransferRequestRepository>(transferRequestRepository);
            services.AddSingleton<ITransferRepository>(transferRepository);
            services.AddSingleton<NotificationService>(notificationService);

            jobManager.Start();

            //Lazy load, requires explicit instantiation
            ExchangeRateUpdater exchangeRateUpdater = new ExchangeRateUpdater(jobManager, exchangeRateProvider, contextHolder);
            PayoutManager payoutManager = new PayoutManager(jobManager, contextHolder, transferRequestRepository, transferRepository, dbFactory, jsonSerializerSettings, daemonClientFactory, notificationService);

            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });
            services.AddSingleton<PayoutManager>(payoutManager);
            services.RegisterServices();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.1#set-the-preflight-expiration-time
            //app.UseCorsMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            // Patch path base with forwarded path
            app.Use(async (context, next) =>
            {
                var forwardedPath = context.Request.Headers["X-Forwarded-Path"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath))
                {
                    context.Request.PathBase = forwardedPath;
                }

                await next();
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
