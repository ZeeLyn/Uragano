using System.Collections.Generic;
using MessagePack;

namespace Uragano.Abstractions
{
    [MessagePackObject]
    public class InvokeMessage : IInvokeMessage
    {
        public InvokeMessage()
        {
        }

        public InvokeMessage(string route, object[] args, Dictionary<string, string> meta)
        {
            Route = route;
            Meta = meta;
            Args = args;
        }

        [Key(0)]
        public string Route { get; set; }


        [Key(1)]
        public object[] Args { get; set; }


        [Key(2)]
        public Dictionary<string, string> Meta { get; set; }
    }
}
