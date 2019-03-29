using System.Collections.Generic;

namespace Uragano.Abstractions.ConsistentHash
{
    public interface IConsistentHash<T>
    {
        void SetVirtualReplicationCount(int count);

        List<T> GetAllNodes();

        void AddNode(T node, string key);

        void RemoveNode(string key);

        T GetNodeForKey(string key);
    }
}
