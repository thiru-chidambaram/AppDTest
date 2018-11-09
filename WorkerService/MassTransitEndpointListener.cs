using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NLog;
using Shared.MassTransit;

namespace WorkerService
{
    public class MassTransitEndpointListener<TConsumer> : ICommunicationListener where TConsumer : class, IConsumer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IExtendedHost _extendedHost;
        private readonly TConsumer _consumer;
        private readonly string _queueName;

        private readonly Action<IReceiveEndpointConfigurator> _configure;
        private readonly bool _autoDelete;
        private HostReceiveEndpointHandle _handle;

        public MassTransitEndpointListener(IExtendedHost extendedHost, TConsumer consumer,
            string queueName, Action<IReceiveEndpointConfigurator> configure = null, bool autoDelete = true)
        {
            _extendedHost = extendedHost;
            _consumer = consumer;
            _queueName = queueName;
            _configure = configure;
            _autoDelete = autoDelete;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                _handle = await _extendedHost.ConnectReceiveEndpoint(_queueName, configurator =>
                {
                    configurator.Consumer(() => _consumer);
                    if (_autoDelete) configurator.ConfigureExchange(false, true);
                    _configure?.Invoke(configurator);
                });

                var address = _handle.Ready.Result.InputAddress.ToString();
                Log.Info($"MassTransit Listening on: {address}");
                return address;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error Configuring MassTransit Queue: {_queueName}");
                throw;
            }

        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (_handle != null) await _handle.StopAsync(cancellationToken).ConfigureAwait(false);
            _handle = null;
        }

        public void Abort()
        {
            _handle?.StopAsync().Wait(1000);
            _handle = null;
        }
    }
}