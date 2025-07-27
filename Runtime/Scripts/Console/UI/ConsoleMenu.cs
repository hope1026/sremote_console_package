// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class ConsoleMenu
    {
        private SConsoleUIController _ownerUIController = null;

        private VisualElement _rootElement;
        private Button _logViewButton;
        private Button _commandViewButton;
        private Button _preferencesViewButton;
        private Button _closeButton;

        public void Initialize(SConsoleUIController ownerUIController_, VisualElement rootElement_)
        {
            _ownerUIController = ownerUIController_;
            _rootElement = rootElement_;
            LoadStyleSheet();
            LoadUIXML();
            BindEvents();
            UpdateButtonStates();
        }

        public void Terminate()
        {
            _rootElement = null;
            _ownerUIController = null;
        }

        private void LoadStyleSheet()
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("UI/ConsoleMenuRuntimeStyles");
            if (styleSheet != null)
            {
                _rootElement.styleSheets.Add(styleSheet);
            }

            // Apply fonts programmatically since CSS font references don't work at runtime
            UIResourceHelper.ApplyDefaultFontRecursive(_rootElement);
        }

        private void LoadUIXML()
        {
            if (_rootElement == null)
            {
                Debug.LogError("[SRemoteConsole] ConsoleMenu: Root element is null");
                return;
            }

            VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/ConsoleMenuRuntime");
            if (uiAsset != null)
            {
                VisualElement uiRoot = uiAsset.Instantiate();
                _rootElement.Add(uiRoot);

                _logViewButton = _rootElement.Q<Button>("log-view-button");
                _commandViewButton = _rootElement.Q<Button>("command-view-button");
                _preferencesViewButton = _rootElement.Q<Button>("preferences-view-button");
                _closeButton = _rootElement.Q<Button>("close-button");
            }
            else
            {
                Debug.LogError("[SRemoteConsole] ConsoleMenu: Failed to load UXML asset");
            }
        }

        private void BindEvents()
        {
            // View button events
            _logViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_ownerUIController.CurrentConsoleViewType != ConsoleViewType.LOG)
                {
                    _ownerUIController.ShowView(ConsoleViewType.LOG);
                    UpdateButtonStates();
                }
            });

            _commandViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_ownerUIController.CurrentConsoleViewType != ConsoleViewType.COMMAND)
                {
                    _ownerUIController.ShowView(ConsoleViewType.COMMAND);
                    UpdateButtonStates();
                }
            });

            _preferencesViewButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_ownerUIController.CurrentConsoleViewType != ConsoleViewType.PREFERENCES)
                {
                    _ownerUIController.ShowView(ConsoleViewType.PREFERENCES);
                    UpdateButtonStates();
                }
            });

            _closeButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_ownerUIController != null && _ownerUIController.OwnerConsoleRuntimeMain != null)
                {
                    _ownerUIController.OwnerConsoleRuntimeMain.CloseConsole();
                }
            });
        }

        private void UpdateButtonStates()
        {
            if (_ownerUIController == null) return;

            var currentViewType = _ownerUIController.CurrentConsoleViewType;

            // Reset all buttons to default style
            SetButtonDefaultStyle(_logViewButton);
            SetButtonDefaultStyle(_commandViewButton);
            SetButtonDefaultStyle(_preferencesViewButton);

            // Apply selected style to current button
            switch (currentViewType)
            {
                case ConsoleViewType.LOG:
                    SetButtonSelectedStyle(_logViewButton);
                    break;
                case ConsoleViewType.COMMAND:
                    SetButtonSelectedStyle(_commandViewButton);
                    break;
                case ConsoleViewType.PREFERENCES:
                    SetButtonSelectedStyle(_preferencesViewButton);
                    break;
            }
        }

        private void SetButtonDefaultStyle(Button button_)
        {
            if (button_ == null) return;
            button_.RemoveFromClassList("menu-button--selected");
        }

        private void SetButtonSelectedStyle(Button button_)
        {
            if (button_ == null) return;
            button_.AddToClassList("menu-button--selected");
        }

        public void UpdateCustom()
        {
            UpdateButtonStates();
        }
    }
}