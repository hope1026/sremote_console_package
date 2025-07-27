// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin.RemoteConsole.Runtime
{
    internal enum ConsoleViewType
    {
        LOG = 0,
        COMMAND,
        PREFERENCES
    }

    internal static class ConsoleViewTypeUtil
    {
        public const int COUNT = (int)ConsoleViewType.PREFERENCES + 1;
    }
}