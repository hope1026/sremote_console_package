// 
// Copyright 2015 https://github.com/hope1026

using System.Net;
using System.Net.Sockets;

namespace SPlugin.FrameWork.Network
{
    internal static class Util
    {
        public static string GetLocalIpAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork && false == IPAddress.IsLoopback(ipAddress))
                {
                    return ipAddress.ToString();
                }
            }

            return IPAddress.Loopback.ToString();
        }
    }
}