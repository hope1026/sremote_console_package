// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class ConsoleViewMain
    {
        private SConsoleEditorWindow _consoleEditorWindow = null;

        private ConsoleViewSelectMenu _consoleViewSelectMenu = null;
        private SystemMessageView _systemLogView = null;

        private string _notifyMessage = string.Empty;
        private bool _forceChangeWindow = false;
        private readonly ConsoleViewAbstract[] _consoleViews = new ConsoleViewAbstract[ConsoleViewTypeUtil.COUNT];
        private ConsoleViewType _currentConsoleViewType = ConsoleViewType.LOG;
        public ConsoleViewType CurrentConsoleViewType => _currentConsoleViewType;

        #region SINGLETON

        private static ConsoleViewMain _instance = null;
        private static readonly object _lockObject = new object();

        public static ConsoleViewMain Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConsoleViewMain();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        private ConsoleViewMain() { }

        public void Initialize(SConsoleEditorWindow editorWindow_)
        {
            _consoleEditorWindow = editorWindow_;
            _consoleEditorWindow.position.Set(_consoleEditorWindow.position.x, _consoleEditorWindow.position.y, ConsoleViewLayoutDefines.windowSize.x, ConsoleViewLayoutDefines.windowSize.y);
            _consoleEditorWindow.wantsMouseMove = true;
            _consoleEditorWindow.titleContent = new GUIContent(ConsoleViewNameDefines.Window.TITLE);

            ConsoleEditorPrefs.ReadPrefs();

            AppManager.Instance.Initialize();

            _consoleViewSelectMenu = new ConsoleViewSelectMenu();
            _consoleViewSelectMenu.Initialize(this, AppManager.Instance.GetActivatedApp());

            _consoleViews[(int)ConsoleViewType.LOG] = new LogView();
            _consoleViews[(int)ConsoleViewType.COMMAND] = new CommandView();
            _consoleViews[(int)ConsoleViewType.PREFERENCES] = new PreferencesView();
            _consoleViews[(int)ConsoleViewType.APPLICATIONS] = new ApplicationsView();

            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                consoleView?.Initialize(this, AppManager.Instance.GetActivatedApp());
            }

            ShowView(_currentConsoleViewType);

            _systemLogView = new SystemMessageView();

            RemoteConsoleLocalEditorBridge.Instance.RegisterUpdateInEditorDelegate();
        }

        public void Terminate()
        {
            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                consoleView?.Terminate();
            }

            _consoleViewSelectMenu?.Terminate();
        }

        public void ShowView(ConsoleViewType showingConsoleViewType_)
        {
            if (showingConsoleViewType_ == _currentConsoleViewType)
                return;

            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                if (consoleView == null)
                    continue;

                if (consoleView.ConsoleViewType == showingConsoleViewType_)
                {
                    consoleView.Show();
                }
                else
                {
                    consoleView.Hide();
                }
            }

            _currentConsoleViewType = showingConsoleViewType_;
        }

        public void OnGUICustom()
        {
            if (null == _consoleEditorWindow) return;

            ChangeWindowSizeIfChangedWindowEditor();
            _consoleEditorWindow.BeginWindows();

            GUILayout.BeginArea(ConsoleViewLayoutDefines.ViewSelectMenu.areaRect);
            _consoleViewSelectMenu?.OnGuiCustom();
            GUILayout.EndArea();

            GUILayout.BeginArea(ConsoleViewLayoutDefines.View.areaRect);
            _consoleViews[(int)_currentConsoleViewType]?.OnGuiCustom();
            GUILayout.EndArea();

            _systemLogView?.OnGuiCustom();

            _consoleEditorWindow.EndWindows();

            _consoleEditorWindow.Repaint();

            if (true == SGuiStyle.RequestUpdateColors)
            {
                SGuiStyle.UpdateColor();
                SGuiStyle.RequestUpdateColors = false;
            }
        }

        public void UpdateCustom()
        {
            RemoteConsoleLocalEditorBridge.Instance.UpdateInEditor();
            AppManager.Instance.UpdateCustom();
            _consoleViews[(int)_currentConsoleViewType]?.UpdateCustom();
            _systemLogView?.UpdateCustom();

            ShowNotifyDialogIfExistMessage();
        }

        private void ShowNotifyDialogIfExistMessage()
        {
            if (false == string.IsNullOrEmpty(_notifyMessage))
            {
                EditorUtility.DisplayDialog("Notify", _notifyMessage, "ok");
                _notifyMessage = string.Empty;
            }
        }

        private void ChangeWindowSizeIfChangedWindowEditor()
        {
            if (null != _consoleEditorWindow)
            {
                if (float.Epsilon < Math.Abs(ConsoleViewLayoutDefines.windowSize.x - _consoleEditorWindow.position.width) ||
                    float.Epsilon < Math.Abs(ConsoleViewLayoutDefines.windowSize.y - _consoleEditorWindow.position.height) ||
                    true == _forceChangeWindow)
                {
                    ConsoleViewLayoutDefines.OnChangeWindowSize(_consoleEditorWindow.position.width, _consoleEditorWindow.position.height);
                    _forceChangeWindow = false;
                }
            }
        }

        public void OnChangedCurrentApp(AppAbstract currentApp_)
        {
            _consoleViewSelectMenu?.OnChangedCurrentApp(currentApp_);
            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                consoleView?.OnChangeCurrentApp(currentApp_);
            }
        }

        public void OnReceivedLogErrorInActivatedApp()
        {
            if (ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.ERROR_PAUSE) &&
                _consoleViews[(int)ConsoleViewType.LOG] is LogView logView)
            {
                logView.ForcePause(isPause_: true, canStep_: false);
            }
        }

        public void ShowNotification(string message_)
        {
            if (_consoleEditorWindow != null)
            {
                _consoleEditorWindow.ShowNotification(new GUIContent(message_), 2f);
            }
        }
    }
}