using System.Linq;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class InitializationRedis : IStartUpTask
    {
        private RedisOptions RedisOptions { get; }

        public InitializationRedis(UraganoSettings uraganoSettings)
        {
            RedisOptions = (RedisOptions)uraganoSettings.CachingOptions;

        }
        public void Execute()
        {
            RedisHelper.Initialization(new CSRedis.CSRedisClient(RedisOptions.ConnectionStrings.First().ToString()));
        }
    }
}
