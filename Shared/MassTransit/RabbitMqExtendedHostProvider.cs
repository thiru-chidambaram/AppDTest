using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Shared.MassTransit
{
    public class RabbitMqExtendedHostProvider : IExtendedHost
    {
        private readonly IRabbitMqHost _host;

        protected List<HostReceiveEndpointHandle> ActiveReceiveEndpoints = new List<HostReceiveEndpointHandle>();

        public IHost Host => _host;

        public RabbitMqExtendedHostProvider(IRabbitMqHost host)
        {
            _host = host;
        }

        public async Task<HostReceiveEndpointHandle> ConnectReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configure = null)
        {
            var handle = _host.ConnectReceiveEndpoint(queueName, configurator => configure?.Invoke(configurator));
            ActiveReceiveEndpoints.Add(handle);
            return handle;
        }

        public async Task<HostReceiveEndpointHandle> ConnectReceiveEndpoint(Action<IReceiveEndpointConfigurator> configure = null)
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            var queueName = $"{assemblyName}-{Guid.NewGuid()}";
            var handle = _host.ConnectReceiveEndpoint(queueName, configurator => configure?.Invoke(configurator));
            ActiveReceiveEndpoints.Add(handle);
            return handle;
        }

        public async Task DisconnectEndpoint(HostReceiveEndpointHandle handle)
        {
            await handle.StopAsync();
            if (ActiveReceiveEndpoints.Contains(handle)) ActiveReceiveEndpoints.Remove(handle);
        }

        public async Task DisconnectAllEndpoints()
        {
            foreach (var endpoint in new List<HostReceiveEndpointHandle>(ActiveReceiveEndpoints))
            {
                await endpoint.StopAsync();
            }
        }
    }
}