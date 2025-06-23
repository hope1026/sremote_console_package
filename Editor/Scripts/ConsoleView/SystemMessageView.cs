// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin
{
    internal class SystemMessageView
    {
        private readonly List<LogItem> _systemLogList = new List<LogItem>();
        private readonly List<LogItem> _drawLogList = new List<LogItem>();
        private bool _changedLogList = false;
        private bool _showAble = false;

        private VisualElement _rootElement;
        private VisualElement _overlayElement;
        private VisualElement _messageContainer;
        private Button _closeButton;

        public void Initialize(VisualElement rootContainer_)
        {
            CreateUIElements(rootContainer_);
            BindEvents();
        }

        private void CreateUIElements(VisualElement rootContainer_)
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/SystemMessageView");
            if (visualTree == null)
            {
                Debug.LogError("SystemMessageView.uxml not found in Resources/UI/");
                return;
            }

            _rootElement = visualTree.Instantiate();

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
            var systemMessageStyles = Resources.Load<StyleSheet>("UI/SystemMessageViewStyles");
            
            if (baseStyles != null)
            {
                _rootElement.styleSheets.Add(baseStyles);
            }
            if (systemMessageStyles != null)
            {
                _rootElement.styleSheets.Add(systemMessageStyles);
            }

            // Get references to UI elements
            _overlayElement = _rootElement.Q<VisualElement>("system-message-overlay");
            _messageContainer = _rootElement.Q<VisualElement>("message-container");
            _closeButton = _rootElement.Q<Button>("close-button");

            // Add to root container
            rootContainer_.Add(_rootElement);

            // Initially hidden
            Hide();
        }

        private void BindEvents()
        {
            // Close button click
            _closeButton?.RegisterCallback<ClickEvent>(_ =>
            {
                Hide();
                ClearMessages();
            });

            // Click anywhere on overlay to close
            _overlayElement?.RegisterCallback<ClickEvent>(evt_ =>
            {
                if (evt_.target == _overlayElement)
                {
                    Hide();
                    ClearMessages();
                }
            });
        }

        public void UpdateCustom()
        {
            if (_changedLogList)
            {
                lock (_systemLogList)
                {
                    _drawLogList.AddRange(_systemLogList);
                    _systemLogList.Clear();
                    _showAble = true;
                    _changedLogList = false;
                    
                    // Update UI
                    UpdateMessageDisplay();
                    Show();
                }
            }
        }

        private void UpdateMessageDisplay()
        {
            if (_messageContainer == null) return;

            // Clear existing messages
            _messageContainer.Clear();

            // Add new messages
            foreach (var logItem in _drawLogList)
            {
                var messageElement = CreateMessageElement(logItem);
                _messageContainer.Add(messageElement);
            }
        }

        private VisualElement CreateMessageElement(LogItem logItem_)
        {
            var messageElement = new Label();
            messageElement.AddToClassList("system-message-item");
            
            // Apply color based on log type
            string coloredText = logItem_.LogData;
            switch (logItem_.LogType)
            {
                case LogType.Log:
                    coloredText = ColorUtility.ReplaceColorString(coloredText, ConsoleEditorPrefs.LogTextColor);
                    break;
                case LogType.Warning:
                    coloredText = ColorUtility.ReplaceColorString(coloredText, ConsoleEditorPrefs.WarningTextColor);
                    break;
                case LogType.Error:
                    coloredText = ColorUtility.ReplaceColorString(coloredText, ConsoleEditorPrefs.ErrorTextColor);
                    break;
            }

            messageElement.text = coloredText;
            return messageElement;
        }

        private void Show()
        {
            if (_overlayElement != null)
            {
                _overlayElement.style.display = DisplayStyle.Flex;
                _showAble = true;
            }
        }

        private void Hide()
        {
            if (_overlayElement != null)
            {
                _overlayElement.style.display = DisplayStyle.None;
                _showAble = false;
            }
        }

        private void ClearMessages()
        {
            _drawLogList.Clear();
            _messageContainer?.Clear();
        }

        public void AddSystemLogData(LogItem logData_)
        {
            if (!ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_SYSTEM_MESSAGE))
                return;

            // Apply color formatting to log data
            switch (logData_.LogType)
            {
                case LogType.Log:
                    logData_.LogData = ColorUtility.ReplaceColorString(logData_.LogData, ConsoleEditorPrefs.LogTextColor);
                    break;
                case LogType.Warning:
                    logData_.LogData = ColorUtility.ReplaceColorString(logData_.LogData, ConsoleEditorPrefs.WarningTextColor);
                    break;
                case LogType.Error:
                    logData_.LogData = ColorUtility.ReplaceColorString(logData_.LogData, ConsoleEditorPrefs.ErrorTextColor);
                    break;
            }

            lock (_systemLogList)
            {
                _systemLogList.Add(logData_);
                _changedLogList = true;
            }
        }

        public bool IsVisible => _showAble;

        public VisualElement GetRootElement()
        {
            return _rootElement;
        }

        public void Terminate()
        {
            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
        }
    }
}