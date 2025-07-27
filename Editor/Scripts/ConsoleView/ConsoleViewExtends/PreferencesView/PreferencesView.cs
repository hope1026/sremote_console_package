// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace SPlugin.RemoteConsole.Editor
{
    internal class PreferencesView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.PREFERENCES;

        private VisualElement _rootElement;
        private bool _requestedSendingPreferencesPacket = false;

        // UI Elements
        private Toggle _showTimeToggle;
        private Toggle _showFrameCountToggle;
        private Toggle _showObjectNameToggle;
        private Toggle _showUnityDebugLogToggle;
        private FloatField _profilerRefreshTimeField;
        private IntegerField _skipStackFrameField;
        private ColorField _backgroundColorField;
        private ColorField _textColorField;
        private ColorField _logBackground1ColorField;
        private ColorField _logBackground2ColorField;
        private ColorField _logSelectedBackgroundColorField;
        private Button _resetColorsButton;

        protected override void OnInitialize()
        {
            CreateUIElements();
            BindUIEvents();
            LoadPreferences();
        }

        private void CreateUIElements()
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/PreferencesView");
            if (visualTree == null)
            {
                Debug.LogError("PreferencesView.uxml not found in Resources/UI/. Make sure the file exists in the correct path.");
                // Create a fallback root element
                _rootElement = new VisualElement();
                _rootElement.Add(new Label("PreferencesView UXML file not found"));
                return;
            }

            _rootElement = visualTree.Instantiate();

           

            // Get references to UI elements
            _showTimeToggle = _rootElement.Q<Toggle>("show-time-toggle");
            _showFrameCountToggle = _rootElement.Q<Toggle>("show-frame-count-toggle");
            _showObjectNameToggle = _rootElement.Q<Toggle>("show-object-name-toggle");
            _showUnityDebugLogToggle = _rootElement.Q<Toggle>("show-unity-debug-log-toggle");
            _profilerRefreshTimeField = _rootElement.Q<FloatField>("profiler-refresh-time-field");
            _skipStackFrameField = _rootElement.Q<IntegerField>("skip-stack-frame-field");
            _backgroundColorField = _rootElement.Q<ColorField>("background-color-field");
            _textColorField = _rootElement.Q<ColorField>("text-color-field");
            _logBackground1ColorField = _rootElement.Q<ColorField>("log-background1-color-field");
            _logBackground2ColorField = _rootElement.Q<ColorField>("log-background2-color-field");
            _logSelectedBackgroundColorField = _rootElement.Q<ColorField>("log-selected-background-color-field");
            _resetColorsButton = _rootElement.Q<Button>("reset-colors-button");
        }

        private void LoadStyleSheets(VisualElement rootElement_)
        {
            if(rootElement_ == null) 
                return;
    
            StyleSheet baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
            if (baseStyles != null)
            {
                _rootElement.styleSheets.Add(baseStyles);
            }
            else
            {
                Debug.LogWarning("BaseStyles.uss not found in Resources/UI/. Styles may not be applied correctly.");
            }
            
            StyleSheet preferencesStyles = Resources.Load<StyleSheet>("UI/PreferencesViewStyles");
            if (preferencesStyles != null)
            {
                _rootElement.styleSheets.Add(preferencesStyles);
            }
            else
            {
                Debug.LogWarning("PreferencesViewStyle.uss not found in Resources/UI/. Styles may not be applied correctly.");
            }
        }

        private void BindUIEvents()
        {
            // Show options toggles
            _showTimeToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefsFlags flags = evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_TIME : ConsoleEditorPrefsFlags.NONE;
                ConsoleEditorPrefs.SetFlags(flags, ConsoleEditorPrefsFlags.SHOW_TIME);
            });

            _showFrameCountToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefsFlags flags = evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT : ConsoleEditorPrefsFlags.NONE;
                ConsoleEditorPrefs.SetFlags(flags, ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT);
            });

            _showObjectNameToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefsFlags flags = evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME : ConsoleEditorPrefsFlags.NONE;
                ConsoleEditorPrefs.SetFlags(flags, ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME);
            });

            _showUnityDebugLogToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefsFlags flags = evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG : ConsoleEditorPrefsFlags.NONE;
                ConsoleEditorPrefs.SetFlags(flags, ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG);
                _requestedSendingPreferencesPacket = true;
            });

            // Preferences fields
            _profilerRefreshTimeField?.RegisterValueChangedCallback(evt_ =>
            {
                if (Math.Abs(evt_.newValue - ConsoleEditorPrefs.ProfileRefreshIntervalTimeS) > float.Epsilon)
                {
                    ConsoleEditorPrefs.SetProfileRefreshTimeS(evt_.newValue);
                    _requestedSendingPreferencesPacket = true;
                }
            });

            _skipStackFrameField?.RegisterValueChangedCallback(evt_ =>
            {
                int skipStackFrameCount = Mathf.Max(evt_.newValue, 0);
                if (skipStackFrameCount != ConsoleEditorPrefs.SkipStackFrameCount)
                {
                    ConsoleEditorPrefs.SetSkipStackFrameCount((uint)skipStackFrameCount);
                    _requestedSendingPreferencesPacket = true;
                }
                // Update field value to ensure it's not negative
                if (evt_.newValue != skipStackFrameCount)
                {
                    _skipStackFrameField.SetValueWithoutNotify(skipStackFrameCount);
                }
            });

            // Color fields - Convert between Color and Color32
            _backgroundColorField?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefs.BackgroundColor = (Color32)evt_.newValue;
            });

            _textColorField?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefs.TextColor = (Color32)evt_.newValue;
            });

            _logBackground1ColorField?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefs.LogViewBackground1Color = (Color32)evt_.newValue;
                // Trigger log list refresh to apply new background color
                RequestLogListRefresh();
            });

            _logBackground2ColorField?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefs.LogViewBackground2Color = (Color32)evt_.newValue;
                // Trigger log list refresh to apply new background color
                RequestLogListRefresh();
            });

            _logSelectedBackgroundColorField?.RegisterValueChangedCallback(evt_ =>
            {
                ConsoleEditorPrefs.LogViewSelectedBackgroundColor = (Color32)evt_.newValue;
                // Trigger log list refresh to apply new background color
                RequestLogListRefresh();
            });

            // Reset colors button
            _resetColorsButton?.RegisterCallback<ClickEvent>(_ =>
            {
                ConsoleEditorPrefs.ResetDefaultColors();
                ConsoleEditorPrefs.WriteColorPrefs();
                LoadPreferences(); // Refresh UI with new values
                RequestLogListRefresh(); // Refresh log list to apply new background colors
            });
        }

        private void LoadPreferences()
        {
            // Load show options
            if (_showTimeToggle != null)
                _showTimeToggle.value = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME);
            
            if (_showFrameCountToggle != null)
                _showFrameCountToggle.value = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT);
            
            if (_showObjectNameToggle != null)
                _showObjectNameToggle.value = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME);
            
            if (_showUnityDebugLogToggle != null)
                _showUnityDebugLogToggle.value = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG);

            // Load preferences
            if (_profilerRefreshTimeField != null)
                _profilerRefreshTimeField.value = ConsoleEditorPrefs.ProfileRefreshIntervalTimeS;
            
            if (_skipStackFrameField != null)
                _skipStackFrameField.value = (int)ConsoleEditorPrefs.SkipStackFrameCount;

            // Load colors - Convert Color32 to Color
            if (_backgroundColorField != null)
                _backgroundColorField.value = (Color)ConsoleEditorPrefs.BackgroundColor;
            
            if (_textColorField != null)
                _textColorField.value = (Color)ConsoleEditorPrefs.TextColor;
            
            if (_logBackground1ColorField != null)
                _logBackground1ColorField.value = (Color)ConsoleEditorPrefs.LogViewBackground1Color;
            
            if (_logBackground2ColorField != null)
                _logBackground2ColorField.value = (Color)ConsoleEditorPrefs.LogViewBackground2Color;
            
            if (_logSelectedBackgroundColorField != null)
                _logSelectedBackgroundColorField.value = (Color)ConsoleEditorPrefs.LogViewSelectedBackgroundColor;
        }

        public VisualElement GetRootElement()
        {
            return _rootElement;
        }

        protected override void OnShow()
        {
            _requestedSendingPreferencesPacket = false;
            LoadPreferences(); // Refresh UI when shown
        }

        protected override void OnHide()
        {
            if (_requestedSendingPreferencesPacket)
            {
                AppManager.Instance.GetActivatedApp()?.SendPreferences();
                _requestedSendingPreferencesPacket = false;
            }
        }

        public override void UpdateCustom()
        {
            // UIToolkit handles most updates automatically through data binding
            // This can be used for custom update logic if needed
        }

        private void RequestLogListRefresh()
        {
            // Request refresh on LogView through ConsoleViewMain
            ConsoleViewMainRef?.RequestLogViewRefresh();
        }

        protected override void OnTerminate()
        {
            // Clean up if needed
            _rootElement?.Clear();
            _rootElement = null;
        }
    }
}