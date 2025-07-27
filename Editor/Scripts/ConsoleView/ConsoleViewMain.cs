// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Editor
{
    internal class ConsoleViewMain
    {
        private SConsoleEditorWindow _consoleEditorWindow = null;
        
        private ConsoleViewSelectMenu _consoleViewSelectMenu = null;
        private SystemMessageView _systemMessageView = null;

        private string _notifyMessage = string.Empty;
        private readonly ConsoleViewAbstract[] _consoleViews = new ConsoleViewAbstract[ConsoleViewTypeUtil.COUNT];
        private ConsoleViewType _currentConsoleViewType = ConsoleViewType.LOG;
        public ConsoleViewType CurrentConsoleViewType => _currentConsoleViewType;

        private VisualElement _uiToolkitContainer;
        private VisualElement _currentUIToolkitView;

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
            //_consoleEditorWindow.position.Set(_consoleEditorWindow.position.x, _consoleEditorWindow.position.y);
            _consoleEditorWindow.wantsMouseMove = true;
            _consoleEditorWindow.titleContent = new GUIContent("SConsole");

            ConsoleEditorPrefs.ReadPrefs();

            AppManager.Instance.Initialize();

            // Initialize all views
            _consoleViews[(int)ConsoleViewType.LOG] = new LogView();
            _consoleViews[(int)ConsoleViewType.COMMAND] = new CommandView();
            _consoleViews[(int)ConsoleViewType.PREFERENCES] = new PreferencesView();
            _consoleViews[(int)ConsoleViewType.APPLICATIONS] = new ApplicationsView();

            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                consoleView?.Initialize(this, AppManager.Instance.GetActivatedApp());
            }

            ShowView(_currentConsoleViewType);

            RemoteConsoleToLocalApplicationBridge.Instance.RegisterUpdateInEditorApplicationDelegate();
        }

        public void InitializeUI(VisualElement container_)
        {
            _uiToolkitContainer = container_;
            
            // Create menu and system view
            _consoleViewSelectMenu = new ConsoleViewSelectMenu();
            _consoleViewSelectMenu.Initialize(this, AppManager.Instance.GetActivatedApp());
            
            _systemMessageView = new SystemMessageView();
            _systemMessageView.Initialize(container_);
            
            // Add menu to top of container
            var menuElement = _consoleViewSelectMenu.GetRootElement();
            if (menuElement != null)
            {
                container_.Insert(0, menuElement);
            }
            
            // Update UI view based on current view type
            UpdateUIView();
        }

        public void Terminate()
        {
            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                consoleView?.Terminate();
            }

            _consoleViewSelectMenu?.Terminate();
            _systemMessageView?.Terminate();
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
            
            // Update UI view if container is available
            UpdateUIView();
        }

        private void UpdateUIView()
        {
            if (_uiToolkitContainer == null) return;

            // Clear current UI view
            if (_currentUIToolkitView != null)
            {
                if( _currentUIToolkitView.parent == _uiToolkitContainer)
                {
                    _uiToolkitContainer.Remove(_currentUIToolkitView);
                }
                _currentUIToolkitView = null;
            }

            // Add new UI view
            var currentView = _consoleViews[(int)_currentConsoleViewType];
            VisualElement viewElement = null;

            switch (currentView)
            {
                case PreferencesView preferencesView:
                    viewElement = preferencesView.GetRootElement();
                    break;
                case ApplicationsView applicationsView:
                    viewElement = applicationsView.GetRootElement();
                    break;
                case CommandView commandView:
                    viewElement = commandView.GetRootElement();
                    break;
                case LogView logView:
                    viewElement = logView.GetRootElement();
                    break;
            }

            if (viewElement != null)
            {
                _currentUIToolkitView = viewElement;
                _uiToolkitContainer.Add(_currentUIToolkitView);
            }
        }

        public void UpdateCustom()
        {
            RemoteConsoleToLocalApplicationBridge.Instance.UpdateInEditor();
            AppManager.Instance.UpdateCustom();
            _consoleViews[(int)_currentConsoleViewType]?.UpdateCustom();
            
            // Update system message view
            if (_systemMessageView != null)
            {
                _systemMessageView.UpdateCustom();
            }
            
            // Update menu
            _consoleViewSelectMenu?.UpdateCustom();

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

        public void AddSystemLogData(LogItem logData_)
        {
            if (_systemMessageView != null)
            {
                _systemMessageView.AddSystemLogData(logData_);
            }
        }

        public void RequestLogViewRefresh()
        {
            // Request refresh on LogView to update background colors
            if (_consoleViews[(int)ConsoleViewType.LOG] is LogView logView)
            {
                logView.RequestRefresh();
            }
        }
    }
}