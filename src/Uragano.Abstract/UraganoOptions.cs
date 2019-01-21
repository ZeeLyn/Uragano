using System;

namespace Uragano.Abstractions
{
	public class UraganoOptions
	{
		public static UraganoOption<Type> Client_LoadBalancing { get; } = new UraganoOption<Type>(LoadBalancing.ILoadBalancing.Polling);

		public static UraganoOption<TimeSpan> Client_Node_Status_Refresh_Interval { get; } = new UraganoOption<TimeSpan>(TimeSpan.FromSeconds(10));

		public static UraganoOption<int> Server_DotNetty_Channel_SoBacklog { get; } = new UraganoOption<int>(100);

		public static UraganoOption<TimeSpan> DotNetty_Connect_Timeout { get; } = new UraganoOption<TimeSpan>(TimeSpan.FromSeconds(1));

		public static UraganoOption<bool> DotNetty_Enable_Libuv { get; } = new UraganoOption<bool>(false);


		public static void SetOption<T>(UraganoOption<T> option, T value)
		{
			option.Value = value;
		}


	}
	public class UraganoOption<T>
	{
		public UraganoOption(T value)
		{
			Value = value;
		}

		public T Value { get; internal set; }
	}
}
