// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;

namespace SPlugin
{
    internal class ConsoleViewSelectMenu
    {
        private ConsoleViewMain _consoleViewMainRef = null;
        private readonly RectOffset _menuTapMargin = new RectOffset();
        private AppAbstract _currentAppRef = null;

        public void Initialize(ConsoleViewMain consoleViewMain_, AppAbstract currentApp_)
        {
            _consoleViewMainRef = consoleViewMain_;
            _currentAppRef = currentApp_;
        }

        public void Terminate()
        {
            _consoleViewMainRef = null;
        }

        public void OnChangedCurrentApp(AppAbstract currentApp_)
        {
            _currentAppRef = currentApp_;
        }

        public void OnGuiCustom()
        {
            OnGuiBackgroundBox();

            GUILayout.BeginHorizontal();

            SGuiStyle.MenuTapNormalStyle.margin = _menuTapMargin;
            SGuiStyle.MenuTapSelectedStyle.margin = _menuTapMargin;

            OnGuiLogViewToggle();
            OnGuiCommandViewToggle();
            OnGuiRemoteViewToggle();
            OnGuiPreferenceViewToggle();
            OnGuiRemoteAppInfo();

            GUILayout.EndHorizontal();
        }

        private void OnGuiBackgroundBox()
        {
            GUI.Box(ConsoleViewLayoutDefines.ViewSelectMenu.areaRect, "", SGuiStyle.ButtonStyle);
        }

        private void OnGuiLogViewToggle()
        {
            GUIContent content = new GUIContent();
            content.text = SGuiUtility.ReplaceBoldString("LogView");

            GUILayoutOption layoutOptionWidth = GUILayout.Width(120f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.ViewSelectMenu.areaRect.height);
            bool latestShowingLog = _consoleViewMainRef.CurrentConsoleViewType == ConsoleViewType.LOG;
            GUIStyle guiStyle = latestShowingLog ? SGuiStyle.MenuTapSelectedStyle : SGuiStyle.MenuTapNormalStyle;
            bool showingLog = GUILayout.Toggle(latestShowingLog, content, guiStyle, layoutOptionWidth, layoutOptionHeight);
            if (latestShowingLog == false && showingLog == true)
            {
                _consoleViewMainRef.ShowView(ConsoleViewType.LOG);
            }
        }

        private void OnGuiCommandViewToggle()
        {
            GUIContent content = new GUIContent();
            content.text = SGuiUtility.ReplaceBoldString("CommandView");

            GUILayoutOption layoutOptionWidth = GUILayout.Width(120f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.ViewSelectMenu.areaRect.height);
            bool latestShowingCommand = _consoleViewMainRef.CurrentConsoleViewType == ConsoleViewType.COMMAND;
            GUIStyle guiStyle = latestShowingCommand ? SGuiStyle.MenuTapSelectedStyle : SGuiStyle.MenuTapNormalStyle;
            bool showingLog = GUILayout.Toggle(latestShowingCommand, content, guiStyle, layoutOptionWidth, layoutOptionHeight);
            if (latestShowingCommand == false && showingLog == true)
            {
                _consoleViewMainRef.ShowView(ConsoleViewType.COMMAND);
            }
        }

        private void OnGuiPreferenceViewToggle()
        {
            GUIContent content = new GUIContent(SGuiResources.OptionIconTexture);
            content.tooltip = "Open the preferences windows.";

            GUILayoutOption layoutOptionWidth = GUILayout.Width(30f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.height);

            bool latestShowingPreferences = _consoleViewMainRef.CurrentConsoleViewType == ConsoleViewType.PREFERENCES;
            GUIStyle guiStyle = latestShowingPreferences ? SGuiStyle.MenuTapSelectedStyle : SGuiStyle.MenuTapNormalStyle;
            bool showingPreferences = GUILayout.Toggle(latestShowingPreferences, content, guiStyle, layoutOptionWidth, layoutOptionHeight);
            if (latestShowingPreferences == false && showingPreferences == true)
            {
                _consoleViewMainRef.ShowView(ConsoleViewType.PREFERENCES);
            }
        }

        private void OnGuiRemoteViewToggle()
        {
            GUIContent content = new GUIContent();
            content.text = "ApplicationsView";

            GUILayoutOption layoutOptionWidth = GUILayout.Width(120f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.ViewSelectMenu.areaRect.height);

            bool latestShowingRemote = _consoleViewMainRef.CurrentConsoleViewType == ConsoleViewType.APPLICATIONS;
            GUIStyle guiStyle = latestShowingRemote ? SGuiStyle.MenuTapSelectedStyle : SGuiStyle.MenuTapNormalStyle;
            bool showingRemote = GUILayout.Toggle(latestShowingRemote, content, guiStyle, layoutOptionWidth, layoutOptionHeight);
            if (latestShowingRemote == false && showingRemote == true)
            {
                _consoleViewMainRef.ShowView(ConsoleViewType.APPLICATIONS);
            }
        }

        private void OnGuiRemoteAppInfo()
        {
            if (_currentAppRef == null)
                return;

            GUIContent content = new GUIContent();
            content.text = $"{_currentAppRef.systemInfoContext.DeviceName}({_currentAppRef.systemInfoContext.RuntimePlatform})-{_currentAppRef.IpAddressString}:{_currentAppRef.AppConnectionStateType.ToString()})";

            GUILayoutOption layoutOptionWidth = GUILayout.ExpandWidth(expand: true);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.ViewSelectMenu.areaRect.height);
            if (_currentAppRef.HasConnected())
            {
                SGuiStyle.ConnectedAppAreaStyle.fontStyle = FontStyle.Bold;
                SGuiStyle.ConnectedAppAreaStyle.fontSize = 11;
                GUILayout.Label(content, SGuiStyle.ConnectedAppAreaStyle, layoutOptionWidth, layoutOptionHeight);
            }
            else
            {
                SGuiStyle.DisConnectedAppAreaStyle.fontSize = 10;
                content.text = SGuiUtility.ReplaceColorString(content.text, Color.gray);
                GUILayout.Label(content, SGuiStyle.DisConnectedAppAreaStyle, layoutOptionWidth, layoutOptionHeight);
            }
        }
    }
}