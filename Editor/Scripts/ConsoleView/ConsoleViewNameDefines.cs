// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal static class ConsoleViewNameDefines
    {
        public class Window
        {
            public const string TITLE = "SConsole";
            public const string NETWORK = "Network";
            public const string SYSTEM_MESSAGE = "SystemMessage";
        }


        public class NetworkView
        {
            public const string BT_CLIENT = "Client";
            public const string BT_HOST = "Host";
            public const string LB_MAX_CONNECTION = "MaxConnection";
            public const string LB_LOCAL_IP = "LocalIP";
            public const string LB_PORT = "Port";
        }
        
        public class GuiControllerUniqueName
        {
            public class PreferenceName
            {
                public const string TEXT_FIELD_ADD_SEARCH = "PreferenceTextFieldAddSearch";
            }

            public class SearchAndExcludeMenu
            {
                public const string TEXT_FIELD_SEARCH = "SearchAndExcludeMenuSearchTextField";
                public const string TEXT_FIELD_EXCLUDE = "SearchAndExcludeMenuExcludeTextField";
            }

            public class ExtendLogEditor
            {
                public const string TEXT_FIELD_SEARCH = "ExtendLogEditorSearchTextField";
            }
        }
    }
}