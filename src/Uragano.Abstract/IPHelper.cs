using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Uragano.Abstractions
{
    public static class IpHelper
    {
        private static readonly long IpABegin, IpAEnd, IpBBegin, IpBEnd, IpCBegin, IpCEnd;
        private const string LocalIp = "{LOCALIP}";
        static IpHelper()
        {
            IpABegin = ConvertIpToNumber(IPAddress.Parse("192.168.0.0"));
            IpAEnd = ConvertIpToNumber(IPAddress.Parse("192.168.255.255"));

            IpBBegin = ConvertIpToNumber(IPAddress.Parse("172.16.0.0"));
            IpBEnd = ConvertIpToNumber(IPAddress.Parse("172.31.255.255"));

            IpCBegin = ConvertIpToNumber(IPAddress.Parse("10.0.0.0"));
            IpCEnd = ConvertIpToNumber(IPAddress.Parse("10.255.255.255"));
        }

        public static IPAddress GetLocalInternetIp()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Select(p => p.GetIPProperties())
                .SelectMany(p =>
                    p.UnicastAddresses
                ).FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address) && InternalIp(p.Address))?.Address;
        }

        private static long ConvertIpToNumber(IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();
            return bytes[0] * 256 * 256 * 256 + bytes[1] * 256 * 256 + bytes[2] * 256 + bytes[3];
        }

        private static bool InternalIp(IPAddress iPAddress)
        {
            var num = ConvertIpToNumber(iPAddress);
            return num >= IpABegin && num <= IpAEnd || num >= IpBBegin && num <= IpBEnd ||
                   num >= IpCBegin && num <= IpCEnd;
        }

        public static string ReplaceIpPlaceholder(this string text)
        {
            if (!text.Contains(LocalIp))
                return text;
            return text.Replace(LocalIp, GetLocalInternetIp().ToString());
        }
    }
}
