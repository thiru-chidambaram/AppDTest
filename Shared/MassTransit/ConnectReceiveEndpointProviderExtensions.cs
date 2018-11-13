using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Shared.MassTransit
{
    public static class ConnectReceiveEndpointProviderExtensions
    {
        public static IExtendedHost ToReceiveEndpointProviderExtensions(this IRabbitMqHost host)
        {
            return new RabbitMqExtendedHostProvider(host);
        }

        public static void ConfigureExchange(this IReceiveEndpointConfigurator configurator, bool durable, bool autoDelete)
        {
            var exchangeConfigurator = configurator as IExchangeConfigurator;
            if (exchangeConfigurator != null)
            {
                exchangeConfigurator.AutoDelete = autoDelete;
                exchangeConfigurator.Durable = durable;
            }
        }
    }
}