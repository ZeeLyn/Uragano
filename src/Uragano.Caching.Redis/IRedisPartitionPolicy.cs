using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.Caching.Redis
{
    public interface IRedisPartitionPolicy
    {
        string Policy(string key, IEnumerable<string> connectionStrings);
    }
}
