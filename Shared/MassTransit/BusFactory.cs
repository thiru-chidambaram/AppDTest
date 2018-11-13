using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.NLogIntegration;
using MassTransit.RabbitMqTransport;
using NLog;

namespace Shared.MassTransit
{
    public class RabbitMqBusSettings
    {
        public string Host { get; set; } = "rabbitmq://localhost/";

        public string Username { get; set; } = "admin";

        public string Password { get; set; } = "admin";
    }

    public class BusFactory
    {
        public static BusHost CreateUsingRabbitMq(RabbitMqBusSettings rabbitMqSettings,
            Action<IRabbitMqBusFactoryConfigurator> configure = null)
        {
            IExtendedHost host = null;
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var rabbitMqHost = cfg.Host(new Uri(rabbitMqSettings.Host), h =>
                {
                    h.Username(rabbitMqSettings.Username);
                    h.Password(rabbitMqSettings.Password);
                });
                host = new RabbitMqExtendedHostProvider(rabbitMqHost);
                cfg.UseNLog(new LogFactory());
                configure?.Invoke(cfg);
            });

            var busHost = new BusHost(busControl, host);
            return busHost;
        }
    }

    public class BusHost : IDisposable
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public IBusControl Bus { get; protected set; }

        public IExtendedHost Host { get; protected set; }

        public BusHost(IBusControl bus, IExtendedHost host = null)
        {
            Bus = bus;
            Host = host;
        }

        public void StartBus()
        {
            Bus.Start();
        }

        public async Task StopBus()
        {
            if (Host != null) await Host.DisconnectAllEndpoints();
            if (Bus != null) await Bus.StopAsync();

            Host = null;
            Bus = null;
        }

        public void Dispose()
        {
            try
            {
                StopBus().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Disposing Bus");
            }
        }
    }
}
