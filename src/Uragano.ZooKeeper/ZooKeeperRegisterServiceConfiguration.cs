using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.ZooKeeper
{
    public class ZooKeeperRegisterServiceConfiguration : IServiceRegisterConfiguration
    {
        public string Id { get; set; } = EnvironmentVariableReader.Get<string>(EnvironmentVariables.uragano_service_id);
        public string Name { get; set; } = EnvironmentVariableReader.Get<string>(EnvironmentVariables.uragano_service_name);

        public override string ToString()
        {
            return $"/{Name}/{Id}";
        }
    }
}
