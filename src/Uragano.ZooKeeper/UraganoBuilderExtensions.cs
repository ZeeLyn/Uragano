using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.ZooKeeper
{
    public static class UraganoBuilderExtensions
    {
        public static void AddZooKeeper(this IUraganoBuilder builder, ZooKeeperClientConfigure zookeeperClientConfiguration)
        {
            builder.ServiceCollection.AddSingleton(zookeeperClientConfiguration);
            builder.AddServiceDiscovery<ZooKeeperServiceDiscovery>(zookeeperClientConfiguration);
        }

        public static void AddZooKeeper(this IUraganoBuilder builder, IConfigurationSection clientConfigurationSection)
        {
            builder.AddZooKeeper(CommonMethods.ReadZooKeeperClientConfigure(clientConfigurationSection));
        }

        public static void AddZooKeeper(this IUraganoBuilder builder, ZooKeeperClientConfigure zookeeperClientConfiguration, ZooKeeperRegisterServiceConfiguration zookeeperAgentServiceConfiguration)
        {
            builder.ServiceCollection.AddSingleton(zookeeperClientConfiguration);
            builder.AddServiceDiscovery<ZooKeeperServiceDiscovery>(zookeeperClientConfiguration, zookeeperAgentServiceConfiguration);
        }

        public static void AddZooKeeper(this IUraganoBuilder builder, IConfigurationSection clientConfigurationSection,
            IConfigurationSection serviceConfigurationSection)
        {
            var service = CommonMethods.ReadRegisterServiceConfiguration(serviceConfigurationSection);
            var client = CommonMethods.ReadZooKeeperClientConfigure(clientConfigurationSection);
            builder.AddZooKeeper(client, service);
        }

        public static void AddZooKeeper(this IUraganoSampleBuilder builder)
        {
            var client = builder.Configuration.GetSection("Uragano:ServiceDiscovery:ZooKeeper:Client");
            var service = builder.Configuration.GetSection("Uragano:ServiceDiscovery:ZooKeeper:Service");
            if (service.Exists())
                builder.AddZooKeeper(client, service);
            else
                builder.AddZooKeeper(client);
        }

        //private static void CreateZooKeeperClient(IUraganoBuilder builder, ZooKeeperClientConfigure zookeeperClientConfiguration)
        //{
        //    var client = new org.apache.zookeeper.ZooKeeper(zookeeperClientConfiguration.ConnectionString, zookeeperClientConfiguration.SessionTimeout, new UraganoWatcher(), zookeeperClientConfiguration.SessionId, string.IsNullOrWhiteSpace(zookeeperClientConfiguration.SessionPassword) ? null : Encoding.UTF8.GetBytes(zookeeperClientConfiguration.SessionPassword), zookeeperClientConfiguration.CanBeReadOnly);
        //    builder.ServiceCollection.AddSingleton(client);
        //}
    }
}
