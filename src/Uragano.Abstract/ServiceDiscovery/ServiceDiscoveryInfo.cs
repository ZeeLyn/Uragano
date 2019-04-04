using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Uragano.Abstractions.ServiceDiscovery
{
    public class ServiceDiscoveryBase
    {
        public ServiceDiscoveryBase(string address, int port, IDictionary<string, string> meta)
        {
            Address = address;
            Port = port;
            Meta = meta;
        }

        public string Address { get; }

        public int Port { get; }

        public IDictionary<string, string> Meta { get; }
    }

    public class ServiceDiscoveryInfo : ServiceDiscoveryBase
    {
        public ServiceDiscoveryInfo(string serviceId, string address, int port, int weight, IDictionary<string, string> meta) : base(address, port, meta)
        {
            ServiceId = serviceId;
            Weight = weight;
        }

        public string ServiceId { get; }

        public int Weight { get; }
    }

    public class ServiceNodeInfo : ServiceDiscoveryInfo
    {
        public ServiceNodeInfo(string serviceId, string address, int port, int weight, IDictionary<string, string> meta) : base(serviceId, address, port,weight, meta)
        {
           
        }

       

        public ConcurrentDictionary<string, object> Attach { get; } = new ConcurrentDictionary<string, object>();
    }
}
