using Microsoft.Extensions.Configuration;

namespace Uragano.Caching.Redis
{
    internal class CommonMethods
    {
        internal static RedisOptions ReadRedisConfiguration(IConfigurationSection configurationSection)
        {
            return configurationSection.Get<RedisOptions>();
        }
    }
}
