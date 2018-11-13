using System;
using System.Threading.Tasks;
using MassTransit;

namespace Shared.MassTransit
{
    public interface IExtendedHost
    {
        IHost Host { get; }
        Task<HostReceiveEndpointHandle> ConnectReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configure = null);
        Task<HostReceiveEndpointHandle> ConnectReceiveEndpoint(Action<IReceiveEndpointConfigurator> configure = null);
        Task DisconnectEndpoint(HostReceiveEndpointHandle handle);
        Task DisconnectAllEndpoints();
    }
}