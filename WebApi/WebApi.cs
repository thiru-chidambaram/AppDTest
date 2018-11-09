using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NLog;
using Shared.MassTransit;

namespace WebApi
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class WebApi : StatelessService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly BusHost _rabbitMqHost;

        public WebApi(StatelessServiceContext context, RabbitMqBusSettings rabbitMqSettings)
            : base(context)
        {
            Log.Info("WebApi Service: Created");
            _rabbitMqHost = BusFactory.CreateUsingRabbitMq(rabbitMqSettings);
            _rabbitMqHost.StartBus();
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            Log.Info("WebApi Service: Created Listeners");

            return new[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                            .UseKestrel()
                            .ConfigureServices(services =>
                            {
                                services.AddSingleton(serviceContext);
                                services.AddSingleton(_rabbitMqHost);
                            })
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseStartup<Startup>()
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .UseUrls(url)
                            .Build();
                    }))
            };
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            Log.Info("Stopping Bus");
            await _rabbitMqHost.StopBus();
        }
    }
}
