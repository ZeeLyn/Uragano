using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.Abstractions.ConsistentHash
{
    public interface IConsistentHash<T>
    {
        List<T> GetAllNodes();

        void AddNode(T node, string key);

        void RemoveNode(string key);

        T GetNodeForKey(string key);
    }
}
