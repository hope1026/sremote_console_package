// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;

namespace SPlugin.RemoteConsole.Editor
{
    internal class ConsoleEditorPrefsIds
    {
        public static readonly string OPTION_FLAG = "SPlugin.RemoteConsoleNetwork.OptionFlag" + PlayerSettings.productGUID;

        public static readonly string SEARCH_LIST_STRING = "SRemoteConsole.SearchListString" + PlayerSettings.productGUID;
        public static readonly string SEARCH_LIST_DO_SEARCHING = "SRemoteConsole.SearchListDoSearching" + PlayerSettings.productGUID;
        public static readonly string SEARCH_LIST_COUNT = "SRemoteConsole.SearchListCount" + PlayerSettings.productGUID;

        public static readonly string PROFILE_REFRESH_INTERVAL_TIME_S = "SPlugin.RemoteConsoleNetwork.ProfileRefreshIntervalTimeSeconds" + PlayerSettings.productGUID;

        public static readonly string SKIP_STACK_FRAME_COUNT = "SPlugin.RemoteConsoleNetwork.skipStackFrameCount" + PlayerSettings.productGUID;

        public static readonly string LOG_VIEW_TIME_WIDTH = "SPlugin.RemoteConsoleNetwork.LogViewTimeWidth" + PlayerSettings.productGUID;
        public static readonly string LOG_VIEW_FRAME_COUNT_WIDTH = "SPlugin.RemoteConsoleNetwork.LogViewFrameCountWidth" + PlayerSettings.productGUID;
        public static readonly string LOG_VIEW_OBJECT_NAME_WIDTH = "SPlugin.RemoteConsoleNetwork.LogViewObjectNameWidth" + PlayerSettings.productGUID;
        public static readonly string LOG_DIRECTORY_PATH_A = "SPlugin.RemoteConsoleNetwork.LogDirectoryPathA" + PlayerSettings.productGUID;
        public static readonly string LOG_FILE_TYPE = "SPlugin.RemoteConsoleNetwork.LogFileType" + PlayerSettings.productGUID;

        public static readonly string BACKGROUND_COLOR = "SRemoteConsole.BackgroundColor" + PlayerSettings.productGUID;
        public static readonly string TEXT_COLOR = "SRemoteConsole.TextColor" + PlayerSettings.productGUID;
        public static readonly string LOG_VIEW_BACKGROUND1_COLOR = "SRemoteConsole.LogViewBackground1Color" + PlayerSettings.productGUID;
        public static readonly string LOG_VIEW_BACKGROUND2_COLOR = "SRemoteConsole.LogViewBackground2Color" + PlayerSettings.productGUID;
        public static readonly string LOG_VIEW_SELECTED_BACKGROUND_COLOR = "SRemoteConsole.LogViewSelectedBackgroundColor" + PlayerSettings.productGUID;
        public static readonly string LOG_TEXT_COLOR = "SRemoteConsole.LogTextColor" + PlayerSettings.productGUID;
        public static readonly string WARNING_TEXT_COLOR = "SRemoteConsole.WarningTextColor" + PlayerSettings.productGUID;
        public static readonly string ERROR_TEXT_COLOR = "SRemoteConsole.ErrorTextColor" + PlayerSettings.productGUID;
    }
}