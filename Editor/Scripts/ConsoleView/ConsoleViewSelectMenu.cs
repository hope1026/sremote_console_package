// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Editor
{
    internal class ConsoleViewSelectMenu
    {
        private ConsoleViewMain _consoleViewMainRef = null;
        private AppAbstract _currentAppRef = null;

        private VisualElement _rootElement;
        private Button _logViewButton;
        private Button _commandViewButton;
        private Button _applicationsViewButton;
        private Button _preferencesViewButton;
        private Label _appInfoLabel;

        public void Initialize(ConsoleViewMain consoleViewMain_, AppAbstract currentApp_)
        {
            _consoleViewMainRef = consoleViewMain_;
            _currentAppRef = currentApp_;
            CreateUIElements();
            BindEvents();
            UpdateButtonStates();
            UpdateAppInfo();
        }

        private void CreateUIElements()
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/ConsoleViewSelectMenu");
            if (visualTree == null)
            {
                Debug.LogError("ConsoleMenu.uxml not found in Resources/UI/");
                return;
            }

            _rootElement = visualTree.Instantiate();

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
            var consoleMenuStyles = Resources.Load<StyleSheet>("UI/ConsoleViewSelectMenuStyles");
            
            if (baseStyles != null)
            {
                _rootElement.styleSheets.Add(baseStyles);
            }
            if (consoleMenuStyles != null)
            {
                _rootElement.styleSheets.Add(consoleMenuStyles);
            }

            // Get references to UI elements
            _logViewButton = _rootElement.Q<Button>("log-view-button");
            _commandViewButton = _rootElement.Q<Button>("command-view-button");
            _applicationsViewButton = _rootElement.Q<Button>("applications-view-button");
            _preferencesViewButton = _rootElement.Q<Button>("preferences-view-button");
            _appInfoLabel = _rootElement.Q<Label>("app-info-label");
        }

        private void BindEvents()
        {
            // View button events
            _logViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_consoleViewMainRef.CurrentConsoleViewType != ConsoleViewType.LOG)
                {
                    _consoleViewMainRef.ShowView(ConsoleViewType.LOG);
                    UpdateButtonStates();
                }
            });

            _commandViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_consoleViewMainRef.CurrentConsoleViewType != ConsoleViewType.COMMAND)
                {
                    _consoleViewMainRef.ShowView(ConsoleViewType.COMMAND);
                    UpdateButtonStates();
                }
            });

            _applicationsViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_consoleViewMainRef.CurrentConsoleViewType != ConsoleViewType.APPLICATIONS)
                {
                    _consoleViewMainRef.ShowView(ConsoleViewType.APPLICATIONS);
                    UpdateButtonStates();
                }
            });

            _preferencesViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_consoleViewMainRef.CurrentConsoleViewType != ConsoleViewType.PREFERENCES)
                {
                    _consoleViewMainRef.ShowView(ConsoleViewType.PREFERENCES);
                    UpdateButtonStates();
                }
            });
        }

        private void UpdateButtonStates()
        {
            if (_consoleViewMainRef == null) return;

            var currentViewType = _consoleViewMainRef.CurrentConsoleViewType;

            // Remove selected class from all buttons
            _logViewButton?.RemoveFromClassList("selected");
            _commandViewButton?.RemoveFromClassList("selected");
            _applicationsViewButton?.RemoveFromClassList("selected");
            _preferencesViewButton?.RemoveFromClassList("selected");

            // Add selected class to current button
            switch (currentViewType)
            {
                case ConsoleViewType.LOG:
                    _logViewButton?.AddToClassList("selected");
                    break;
                case ConsoleViewType.COMMAND:
                    _commandViewButton?.AddToClassList("selected");
                    break;
                case ConsoleViewType.APPLICATIONS:
                    _applicationsViewButton?.AddToClassList("selected");
                    break;
                case ConsoleViewType.PREFERENCES:
                    _preferencesViewButton?.AddToClassList("selected");
                    break;
            }
        }

        public void OnChangedCurrentApp(AppAbstract currentApp_)
        {
            _currentAppRef = currentApp_;
            UpdateAppInfo();
        }

        private void UpdateAppInfo()
        {
            if (_appInfoLabel == null || _currentAppRef == null)
            {
                if (_appInfoLabel != null)
                {
                    _appInfoLabel.text = "No connection";
                    _appInfoLabel.RemoveFromClassList("connected");
                    _appInfoLabel.AddToClassList("disconnected");
                }
                return;
            }

            // Build app info text
            string appInfoText = $"{_currentAppRef.systemInfoContext.DeviceName}({_currentAppRef.systemInfoContext.RuntimePlatform})-{_currentAppRef.IpAddressString}:{_currentAppRef.AppConnectionStateType}";

            // Apply styling based on connection state
            if (_currentAppRef.HasConnected())
            {
                _appInfoLabel.text = appInfoText;
                _appInfoLabel.RemoveFromClassList("disconnected");
                _appInfoLabel.AddToClassList("connected");
            }
            else
            {
                // Apply gray color for disconnected state
                string coloredText = ColorUtility.ReplaceColorString(appInfoText, Color.gray);
                _appInfoLabel.text = coloredText;
                _appInfoLabel.RemoveFromClassList("connected");
                _appInfoLabel.AddToClassList("disconnected");
            }
        }

        public void UpdateCustom()
        {
            // Update app info periodically
            UpdateAppInfo();
            
            // Update button states in case view changed programmatically
            UpdateButtonStates();
        }

        public VisualElement GetRootElement()
        {
            return _rootElement;
        }

        public void Terminate()
        {
            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
            _consoleViewMainRef = null;
        }
    }
}