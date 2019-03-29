using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uragano.Abstractions.ConsistentHash
{
    public class ConsistentHash<T> : IConsistentHash<T>
    {
        private SortedDictionary<int, T> Ring { get; } = new SortedDictionary<int, T>();

        private int VirtualReplication { get; set; }

        public ConsistentHash(int virtualReplication = 200)
        {
            VirtualReplication = virtualReplication;
        }

        public void SetVirtualReplicationCount(int count)
        {
            VirtualReplication = count;
        }

        public List<T> GetAllNodes()
        {
            return Ring.Select(p => p.Value).Distinct().ToList();
        }

        public void AddNode(T node, string key)
        {
            for (var i = 0; i < VirtualReplication; i++)
            {
                var hash = HashAlgorithm.Hash(key + i, 0);
                Ring.Add(hash, node);
            }
        }

        public void RemoveNode(string key)
        {
            for (var i = 0; i < VirtualReplication; i++)
            {
                var hash = HashAlgorithm.Hash(key + i, 0);
                Ring.Remove(hash);
            }
        }

        public T GetNodeForKey(string key)
        {
            if (!Ring.Any())
                throw new InvalidOperationException("Can not find the available nodes, please call the AddNode method to add nodes.");

            var hash = HashAlgorithm.Hash(key, 0);
            if (Ring.ContainsKey(hash))
                return Ring[hash];
            var node = Ring.Where(p => p.Key > hash).OrderBy(i => i.Key).Select(p => p.Value).FirstOrDefault();
            if (node != null)
                return node;
            return Ring.FirstOrDefault().Value;
        }
    }

    internal class HashAlgorithm
    {
        private const uint m = 0x5bd1e995;
        private const int r = 24;
        public static int Hash(byte[] data, uint seed = 0xc58f1a7b)
        {
            var length = data.Length;
            if (length == 0)
                return 0;

            var h = seed ^ (uint)length;
            var c = 0;
            while (length >= 4)
            {
                var k = (uint)(
                    data[c++]
                    | data[c++] << 8
                    | data[c++] << 16
                    | data[c++] << 24);
                k *= m;
                k ^= k >> r;
                k *= m;
                h *= m;
                h ^= k;
                length -= 4;
            }
            switch (length)
            {
                case 3:
                    h ^= (ushort)(data[c++] | data[c++] << 8);
                    h ^= (uint)(data[c] << 16);
                    h *= m;
                    break;
                case 2:
                    h ^= (ushort)(data[c++] | data[c] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[c];
                    h *= m;
                    break;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;
            return (int)h;
        }

        public static int Hash(string data, uint seed)
        {
            return Hash(Encoding.UTF8.GetBytes(data), seed);
        }
    }
}
