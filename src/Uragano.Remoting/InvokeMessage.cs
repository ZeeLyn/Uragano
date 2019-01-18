using System.Collections.Generic;

namespace Uragano.Remoting
{
	public class InvokeMessage
	{
		public string Route { get; set; }

		public object[] Args { get; set; }

		public Dictionary<string, string> Meta { get; set; }
	}
}
