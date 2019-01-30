using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Uragano.Abstractions
{
	public static class IPHelper
	{
		private static long ip_a_begin, ip_a_end, ip_b_begin, ip_b_end, ip_c_begin, ip_c_end;
		static IPHelper()
		{
			ip_a_begin = ConvertIpToNumber(IPAddress.Parse("192.168.0.0"));
			ip_a_end = ConvertIpToNumber(IPAddress.Parse("192.168.255.255"));

			ip_b_begin = ConvertIpToNumber(IPAddress.Parse("172.16.0.0"));
			ip_b_end = ConvertIpToNumber(IPAddress.Parse("172.31.255.255"));

			ip_c_begin = ConvertIpToNumber(IPAddress.Parse("10.0.0.0"));
			ip_c_end = ConvertIpToNumber(IPAddress.Parse("10.255.255.255"));
		}

		public static IPAddress GetLocalInternetIP()
		{
			return NetworkInterface
				.GetAllNetworkInterfaces()
				.Select(p => p.GetIPProperties())
				.SelectMany(p =>
					p.UnicastAddresses
				).FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address) && InternalIP(p.Address))?.Address;
		}

		private static long ConvertIpToNumber(IPAddress ipAddress)
		{
			var bytes = ipAddress.GetAddressBytes();
			return bytes[0] * 256 * 256 * 256 + bytes[1] * 256 * 256 + bytes[2] * 256 + bytes[3];
		}

		private static bool InternalIP(IPAddress iPAddress)
		{
			var num = ConvertIpToNumber(iPAddress);
			return num >= ip_a_begin && num <= ip_a_end || num >= ip_b_begin && num <= ip_b_end ||
				   num >= ip_c_begin && num <= ip_c_end;
		}

		public static string ReplaceIPPlaceholder(this string text)
		{
			return text.Replace("{LocalIP}", GetLocalInternetIP().ToString());
		}
	}
}
