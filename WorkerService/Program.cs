using System;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using NLog;
using Shared;
using Shared.MassTransit;

namespace WorkerService
{
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main(string[] args)
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                var rabbitMqSettings = new RabbitMqBusSettings
                {
                    Host = "rabbitmq://localhost/",
                    Username = "admin",
                    Password = "admin"
                };

                //To run the Service without Service Fabric, run from a console and add the args: --noSF
                if (args.Any(s => s.Contains("--noSF")))
                {
                    var rabbitMqHost = BusFactory.CreateUsingRabbitMq(rabbitMqSettings);
                    rabbitMqHost.StartBus();

                    var listener = new MassTransitEndpointListener<Consumer>(rabbitMqHost.Host, new Consumer(),
                        Constants.ServiceQueue, autoDelete: false);
                    listener.OpenAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    ServiceRuntime.RegisterServiceAsync("WorkerServiceType",
                        context => new WorkerService(context, rabbitMqSettings)).GetAwaiter().GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(WorkerService).Name);

                }

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error Starting Service");
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
