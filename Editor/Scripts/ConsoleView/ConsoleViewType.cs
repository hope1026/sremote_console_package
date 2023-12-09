// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal enum ConsoleViewType
    {
        LOG = 0,
        COMMAND,
        PREFERENCES,
        APPLICATIONS,
    }

    internal static class ConsoleViewTypeUtil
    {
        private const ConsoleViewType MAX = ConsoleViewType.APPLICATIONS + 1;
        public const int COUNT = (int)MAX + 1;
    }
}