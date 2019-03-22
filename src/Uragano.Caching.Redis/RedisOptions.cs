using System;
using System.Collections.Generic;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class RedisOptions : ICachingOptions
    {
        public int ExpireSeconds { get; set; } = 3600;

        public string KeyPrefix { get; set; } = "Uragano";

        public IEnumerable<RedisConnection> ConnectionStrings { get; set; }
    }

    public class PartitionRedisOptions : RedisOptions
    {
        public Func<string, IEnumerable<RedisConnection>, RedisConnection> PartitionPolicy { get; set; }
    }


    public class RedisConnection
    {
        public RedisConnection()
        {
        }

        public RedisConnection(string host, int port, string password, bool ssl = false, int defaultDatabase = 1)
        {
            Host = host;
            Port = port;
            Password = password;
            SSL = ssl;
            DefaultDatabase = defaultDatabase;
        }

        public string Host { get; set; }

        public int Port { get; set; } = 6379;

        public string Password { get; set; }

        public int DefaultDatabase { get; set; }

        public int PoolSize { get; set; } = 50;

        public bool SSL { get; set; }

        public int ConnectionTimeout { get; set; } = -1;

        public bool PreHeat { get; set; } = true;

        public int WriteBuffer { get; set; } = 10240;

        public int TryIt { get; set; } = 0;

        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Host}:{Port},password={Password},defaultDatabase={DefaultDatabase},poolsize={PoolSize},connectTimeout={ConnectionTimeout},preheat={PreHeat},ssl={SSL},writeBuffer={WriteBuffer},tryit={TryIt},name={Name}";
        }
    }
}
