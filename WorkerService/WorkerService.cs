using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NLog;
using Shared;
using Shared.MassTransit;

namespace WorkerService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WorkerService : StatelessService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly BusHost _rabbitMqHost;

        public WorkerService(StatelessServiceContext context, RabbitMqBusSettings rabbitMqSettings)
            : base(context)
        {
            Log.Info("Worker Service: Created");
            _rabbitMqHost = BusFactory.CreateUsingRabbitMq(rabbitMqSettings);
            _rabbitMqHost.StartBus();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            Log.Info("Worker Service: Created Listeners");

            return new[]
            {
                new ServiceInstanceListener(context =>
                    new MassTransitEndpointListener<Consumer>(_rabbitMqHost.Host, new Consumer(),
                        Constants.ServiceQueue, autoDelete: false))
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            Log.Info("Worker Service: Running");

            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            Log.Info("Stopping Bus");
            await _rabbitMqHost.StopBus();
        }
    }
}
