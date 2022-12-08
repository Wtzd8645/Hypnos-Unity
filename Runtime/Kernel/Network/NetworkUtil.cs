using System.Net;

namespace Morpheus.Network
{
    public static class NetworkUtil
    {
        // NOTE: 只是使用簡易判斷
        public static string GetLocalPrivateIp()
        {
            // Private IP Range
            // 10.0.0.0 - 10.255.255.255(10 / 8 prefix)
            // 172.16.0.0 - 172.31.255.255(172.16 / 12 prefix)
            // 192.168.0.0 - 192.168.255.255(192.168 / 16 prefix)
            const string LocalIpPrefix = "192.168";

            string localIp;
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostEntry.AddressList.Length; ++i)
            {
                localIp = ipHostEntry.AddressList[i].MapToIPv4().ToString();
                if (localIp.StartsWith(LocalIpPrefix))
                {
                    return localIp;
                }
            }

            return string.Empty;
        }
    }
}