using Microsoft.Extensions.Configuration;

namespace Uragano.ZooKeeper
{
    internal class CommonMethods
    {
        internal static ZooKeeperClientConfigure ReadZooKeeperClientConfigure(IConfigurationSection configurationSection)
        {
            var client = new ZooKeeperClientConfigure();
            if (configurationSection.Exists())
            {
                var connection = configurationSection.GetSection("ConnectionString");
                if (connection.Exists())
                    client.ConnectionString = connection.Value;
                var sessionTimeout = configurationSection.GetSection("SessionTimeout");
                if (sessionTimeout.Exists())
                    client.SessionTimeout = int.Parse(sessionTimeout.Value);

                var canBeReadOnly = configurationSection.GetSection("CanBeReadOnly");
                if (canBeReadOnly.Exists())
                    client.CanBeReadOnly = bool.Parse(canBeReadOnly.Value);
            }
            return client;
        }

        internal static ZooKeeperRegisterServiceConfiguration ReadRegisterServiceConfiguration(IConfigurationSection configurationSection)
        {
            var service = new ZooKeeperRegisterServiceConfiguration();

            var idSection = configurationSection.GetSection("id");
            if (idSection.Exists())
                service.Id = idSection.Value;
            var nameSection = configurationSection.GetSection("name");
            if (nameSection.Exists())
                service.Name = nameSection.Value;
            //var hcSection = configurationSection.GetSection("HealthCheckInterval");
            //if (hcSection.Exists())
            //    service.HealthCheckInterval =
            //        TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("HealthCheckInterval"));

            //service.EnableTagOverride = configurationSection.GetValue<bool>("EnableTagOverride");
            //service.Meta = configurationSection.GetValue<Dictionary<string, string>>("meta");
            //service.Tags = configurationSection.GetValue<string[]>("tags");
            return service;
        }
    }
}
