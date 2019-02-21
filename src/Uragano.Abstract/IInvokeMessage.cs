using System.Collections.Generic;
using MessagePack;

namespace Uragano.Abstractions
{
    [Union(0, typeof(InvokeMessage))]
    public interface IInvokeMessage
    {

        [Key(0)]
        string Route { get; set; }

        [Key(1)]
        object[] Args { get; set; }

        [Key(2)]
        Dictionary<string, string> Meta { get; set; }
    }
}
