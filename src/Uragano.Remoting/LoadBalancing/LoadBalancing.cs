using System;

namespace Uragano.Remoting.LoadBalancing
{
    public class LoadBalancing
    {
        private LoadBalancing()
        {
        }

        public static Type ConsistentHash { get; } = typeof(LoadBalancingConsistentHash);

        public static Type Polling { get; } = typeof(LoadBalancingPolling);

        public static Type WeightedPolling { get; } = typeof(LoadBalancingWeightedPolling);

        public static Type Random { get; } = typeof(LoadBalancingRandom);

        public static Type WeightedRandom { get; } = typeof(LoadBalancingWeightedRandom);
    }
}
