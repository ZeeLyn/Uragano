using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Uragano.Abstractions.ServiceDiscovery
{
    public class ServiceDiscoveryBase
    {
        public ServiceDiscoveryBase(string address, int port, bool enableTls, IDictionary<string, string> meta)
        {
            Address = address;
            Port = port;
            EnableTls = enableTls;
            Meta = meta;
        }

        public string Address { get; }

        public int Port { get; }

        public bool EnableTls { get; }

        public IDictionary<string, string> Meta { get; }
    }

    public class ServiceDiscoveryInfo : ServiceDiscoveryBase
    {
        public ServiceDiscoveryInfo(string serviceId, string address, int port, int weight, bool enableTls, IDictionary<string, string> meta) : base(address, port, enableTls, meta)
        {
            ServiceId = serviceId;
            Weight = weight;
        }

        public string ServiceId { get; }

        public int Weight { get; }
    }

    public class ServiceNodeInfo : ServiceDiscoveryInfo
    {
        public ServiceNodeInfo(string serviceId, string address, int port, int weight, bool enableTls, IDictionary<string, string> meta) : base(serviceId, address, port, weight, enableTls, meta)
        {

        }



        public ConcurrentDictionary<string, object> Attach { get; } = new ConcurrentDictionary<string, object>();
    }
}
