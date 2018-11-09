using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;
using Shared.MassTransit;

namespace WebApi
{
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
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

                ServiceRuntime.RegisterServiceAsync("WebApiType",
                    context => new WebApi(context, rabbitMqSettings)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(WebApi).Name);

                // Prevents this host process from terminating so services keeps running. 
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
