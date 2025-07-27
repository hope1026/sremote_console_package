// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class ConsolePrefsSearchContext : IComparable<ConsolePrefsSearchContext>
    {
        public string SearchString { get; set; }
        public bool DoSearching { get; set; }
        public int SearchCount { get; set; }
        public int CompareTo(ConsolePrefsSearchContext obj_)
        {
            return String.Compare(SearchString, obj_.SearchString, StringComparison.Ordinal);
        }
    }
}