using System;
using System.Collections.Generic;
using System.Text;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.ZooKeeper
{
    public class ZooKeeperRegisterServiceConfiguration : IServiceRegisterConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"/{Name}/{Id}";
        }
    }
}
