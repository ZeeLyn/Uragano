using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.ZooKeeper
{
    public class ZooKeeperClientConfigure : IServiceDiscoveryClientConfiguration
    {
        /// <summary>
        /// comma separated host:port pairs, each corresponding to a zk server.
        /// </summary>
        public string ConnectionString { get; set; } =
            EnvironmentVariableReader.Get(EnvironmentVariables.uragano_zk_addr, "127.0.0.1:2181");

        /// <summary>
        /// session timeout in milliseconds
        /// </summary>
        public int SessionTimeout { get; set; } = EnvironmentVariableReader.Get(EnvironmentVariables.uragano_zk_session_timeout, 1000 * 10);


        /// <summary>
        /// (added in 3.4) whether the created client is allowed to go to read-only mode in case of
        /// partitioning. Read-only mode basically means that if the client can't find any majority servers but there's partitioned
        /// server it could reach, it connects to one in read-only mode, i.e. read requests are allowed while write requests are not.
        /// It continues seeking for majority in the background.
        /// </summary>
        public bool CanBeReadOnly { get; set; } = EnvironmentVariableReader.Get(EnvironmentVariables.uragano_zk_readonly, false);
    }
}
