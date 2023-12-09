// 
// Copyright 2015 https://github.com/hope1026

using SPlugin.Network;

namespace SPlugin
{
    internal static class SConsoleNetworkUtil
    {
        public const int PORT_MIN = 31000;
        public const int PORT_MAX = 31200;
        public const int DEFAULT_MAX_CONNECTION = 10;
        private static string _localIpAddress = null;
        private static string _localIpStartAddress = null;
        private static string _localIpEndAddress = null;
        private static string _localIpSubnetMaskWithoutLastDot = null;

        public static string GetLocalIpAddress()
        {
            if (string.IsNullOrEmpty(_localIpAddress))
            {
                _localIpAddress = Util.GetLocalIpAddress();
            }
            return _localIpAddress;
        }

        public static string GetLocalIpStartAddress()
        {
            if (string.IsNullOrEmpty(_localIpStartAddress))
            {
                _localIpStartAddress = $"{GetLocalIpSubnetMaskWidthOutLastDot()}.0";
            }
            return _localIpStartAddress;
        }

        public static string GetLocalIpEndAddress()
        {
            if (string.IsNullOrEmpty(_localIpEndAddress))
            {
                _localIpEndAddress = $"{GetLocalIpSubnetMaskWidthOutLastDot()}.255";
            }
            return _localIpEndAddress;
        }

        public static string GetLocalIpSubnetMaskWidthOutLastDot()
        {
            if (string.IsNullOrEmpty(_localIpSubnetMaskWithoutLastDot))
            {
                _localIpSubnetMaskWithoutLastDot = GetLocalIpAddress();
                if (string.IsNullOrEmpty(_localIpSubnetMaskWithoutLastDot))
                    return string.Empty;

                _localIpSubnetMaskWithoutLastDot = _localIpSubnetMaskWithoutLastDot.Substring(0, _localIpSubnetMaskWithoutLastDot.LastIndexOf('.'));
            }

            return _localIpSubnetMaskWithoutLastDot;
        }
    }
}