// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    internal class ConsoleEditorPrefsSearchContext : IComparable<ConsoleEditorPrefsSearchContext>
    {
        public string SearchString { get; set; }
        public bool DoSearching { get; set; }
        public int SearchCount { get; set; }
        public int CompareTo(ConsoleEditorPrefsSearchContext obj_)
        {
            return String.Compare(SearchString, obj_.SearchString, StringComparison.Ordinal);
        }
    }
}