// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class PreferencesView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.PREFERENCES;

        private VisualElement _preferencesRootElement;
        private bool _requestedSendingPreferencesPacket = false;

        // UI Elements
        private Toggle _showTimeToggle;
        private Toggle _showFrameCountToggle;
        private Toggle _showUnityDebugLogToggle;
        private FloatField _profilerRefreshTimeField;
        private IntegerField _skipStackFrameField;
        
        protected override void OnInitialize()
        {
            CreateUIElements();
            LoadStyleSheets();
            BindUIEvents();
            LoadPreferences();
        }

        private void CreateUIElements()
        {
            // Create preferences content element
            _preferencesRootElement = new VisualElement();
            _preferencesRootElement.name = "preferences-view";

            // Create scroll view container (like Editor)
            ScrollView scrollView = new ScrollView();
            scrollView.AddToClassList("preferences-container");

            // Create show options section
            CreateShowOptionsSection(scrollView);

            // Add separator
            CreateSeparator(scrollView);

            // Create preferences section (profiler settings)
            CreatePreferencesSection(scrollView);

            // Add separator
            CreateSeparator(scrollView);

            // Create debug settings section
            CreateDebugSettingsSection(scrollView);

            _preferencesRootElement.Add(scrollView);
            
            // Add to parent root element
            if (_rootElement != null)
            {
                _rootElement.Add(_preferencesRootElement);
            }
        }

        private void LoadStyleSheets()
        {
            if (_preferencesRootElement == null) return;

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseRuntimeStyles");
            var preferencesStyles = Resources.Load<StyleSheet>("UI/PreferencesViewRuntimeStyles");
            
            if (baseStyles != null)
            {
                _preferencesRootElement.styleSheets.Add(baseStyles);
            }
            if (preferencesStyles != null)
            {
                _preferencesRootElement.styleSheets.Add(preferencesStyles);
            }

            // Apply fonts programmatically since CSS font references don't work at runtime
            UIResourceHelper.ApplyDefaultFontRecursive(_preferencesRootElement);
        }

        private void CreateShowOptionsSection(ScrollView parent_)
        {
            // Show options section (matching Editor structure)
            VisualElement section = new VisualElement();
            section.AddToClassList("preferences-section");

            // Section title
            Label sectionTitle = UIResourceHelper.CreateLabel("Show Options");
            sectionTitle.AddToClassList("preferences-section-title");
            sectionTitle.AddToClassList("bold-text");
            sectionTitle.style.fontSize = 14;
            sectionTitle.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(sectionTitle);
            });
            section.Add(sectionTitle);

            // Show time toggle row
            VisualElement timeRow = new VisualElement();
            timeRow.AddToClassList("preferences-row");
            _showTimeToggle = new Toggle("Show Time");
            _showTimeToggle.name = "show-time-toggle";
            _showTimeToggle.AddToClassList("console-toggle");
            _showTimeToggle.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(_showTimeToggle);
                var labelElement = _showTimeToggle.Q<Label>();
                if (labelElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(labelElement);
                }
                
                // Add checkmark text for runtime visibility
                var checkmark = _showTimeToggle.Q(className: "unity-toggle__checkmark");
                if (checkmark != null)
                {
                    var checkmarkLabel = new Label("✓");
                    checkmarkLabel.AddToClassList("toggle-checkmark-label");
                    checkmarkLabel.style.display = _showTimeToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                    checkmark.Add(checkmarkLabel);
                    checkmark.userData = checkmarkLabel;
                }
            });
            timeRow.Add(_showTimeToggle);
            section.Add(timeRow);

            // Show frame count toggle row
            VisualElement frameCountRow = new VisualElement();
            frameCountRow.AddToClassList("preferences-row");
            _showFrameCountToggle = new Toggle("Show FrameCount");
            _showFrameCountToggle.name = "show-frame-count-toggle";
            _showFrameCountToggle.AddToClassList("console-toggle");
            _showFrameCountToggle.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(_showFrameCountToggle);
                var labelElement = _showFrameCountToggle.Q<Label>();
                if (labelElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(labelElement);
                }
                
                // Add checkmark text for runtime visibility
                var checkmark = _showFrameCountToggle.Q(className: "unity-toggle__checkmark");
                if (checkmark != null)
                {
                    var checkmarkLabel = new Label("✓");
                    checkmarkLabel.AddToClassList("toggle-checkmark-label");
                    checkmarkLabel.style.display = _showFrameCountToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                    checkmark.Add(checkmarkLabel);
                    checkmark.userData = checkmarkLabel;
                }
            });
            frameCountRow.Add(_showFrameCountToggle);
            section.Add(frameCountRow);

            // Show unity debug log toggle row
            VisualElement debugLogRow = new VisualElement();
            debugLogRow.AddToClassList("preferences-row");
            _showUnityDebugLogToggle = new Toggle("Show UnityDebugLog");
            _showUnityDebugLogToggle.name = "show-unity-debug-log-toggle";
            _showUnityDebugLogToggle.AddToClassList("console-toggle");
            _showUnityDebugLogToggle.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(_showUnityDebugLogToggle);
                var labelElement = _showUnityDebugLogToggle.Q<Label>();
                if (labelElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(labelElement);
                }
                
                // Add checkmark text for runtime visibility
                var checkmark = _showUnityDebugLogToggle.Q(className: "unity-toggle__checkmark");
                if (checkmark != null)
                {
                    var checkmarkLabel = new Label("✓");
                    checkmarkLabel.AddToClassList("toggle-checkmark-label");
                    checkmarkLabel.style.display = _showUnityDebugLogToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                    checkmark.Add(checkmarkLabel);
                    checkmark.userData = checkmarkLabel;
                }
            });
            debugLogRow.Add(_showUnityDebugLogToggle);
            section.Add(debugLogRow);

            parent_.Add(section);
        }

        private void CreateSeparator(ScrollView parent_)
        {
            VisualElement separator = new VisualElement();
            separator.AddToClassList("separator-line");
            parent_.Add(separator);
        }

        private void CreatePreferencesSection(ScrollView parent_)
        {
            // Preferences section (matching Editor structure)
            VisualElement section = new VisualElement();
            section.name = "preferences-section";
            section.AddToClassList("preferences-section");

            // Section title
            Label sectionTitle = UIResourceHelper.CreateLabel("Preferences");
            sectionTitle.AddToClassList("preferences-section-title");
            sectionTitle.AddToClassList("bold-text");
            sectionTitle.style.fontSize = 14;
            sectionTitle.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(sectionTitle);
            });
            section.Add(sectionTitle);

            // Profiler refresh time row
            VisualElement profilerRow = new VisualElement();
            profilerRow.AddToClassList("preferences-row");
            
            Label profilerLabel = UIResourceHelper.CreateLabel("ProfilerRefreshTime(Seconds)");
            profilerLabel.AddToClassList("preferences-label");
            profilerLabel.AddToClassList("bold-text");
            profilerLabel.style.fontSize = 12;
            profilerLabel.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(profilerLabel);
            });
            profilerRow.Add(profilerLabel);

            _profilerRefreshTimeField = new FloatField();
            _profilerRefreshTimeField.name = "profiler-refresh-time-field";
            _profilerRefreshTimeField.AddToClassList("preferences-control");
            _profilerRefreshTimeField.AddToClassList("console-text-field");
            _profilerRefreshTimeField.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(_profilerRefreshTimeField);
                var textElement = _profilerRefreshTimeField.Q<Label>();
                if (textElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textElement);
                }
            });
            profilerRow.Add(_profilerRefreshTimeField);
            
            section.Add(profilerRow);
            parent_.Add(section);
        }


        private void CreateDebugSettingsSection(ScrollView parent_)
        {
            // Debug settings section (matching Editor structure)
            VisualElement section = new VisualElement();
            section.name = "debug-settings-section";
            section.AddToClassList("preferences-section");

            // Section title
            Label sectionTitle = UIResourceHelper.CreateLabel("Debug Settings");
            sectionTitle.AddToClassList("preferences-section-title");
            sectionTitle.AddToClassList("bold-text");
            sectionTitle.style.fontSize = 14;
            sectionTitle.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(sectionTitle);
            });
            section.Add(sectionTitle);

            // Skip stack frame count row
            VisualElement skipFrameRow = new VisualElement();
            skipFrameRow.AddToClassList("preferences-row");
            
            Label skipFrameLabel = UIResourceHelper.CreateLabel("SkipStackFrameCount");
            skipFrameLabel.AddToClassList("preferences-label");
            skipFrameLabel.AddToClassList("bold-text");
            skipFrameLabel.style.fontSize = 12;
            skipFrameLabel.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(skipFrameLabel);
            });
            skipFrameRow.Add(skipFrameLabel);

            _skipStackFrameField = new IntegerField();
            _skipStackFrameField.name = "skip-stack-frame-field";
            _skipStackFrameField.AddToClassList("preferences-control");
            _skipStackFrameField.AddToClassList("console-text-field");
            _skipStackFrameField.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(_skipStackFrameField);
                var textElement = _skipStackFrameField.Q<Label>();
                if (textElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textElement);
                }
            });
            skipFrameRow.Add(_skipStackFrameField);
            
            section.Add(skipFrameRow);
            parent_.Add(section);
        }

        private void BindUIEvents()
        {
            // Show options toggles
            _showTimeToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsolePrefsFlags flags = evt_.newValue ? ConsolePrefsFlags.SHOW_TIME : ConsolePrefsFlags.NONE;
                ConsolePrefs.SetFlags(flags, ConsolePrefsFlags.SHOW_TIME);
                
                // Update checkmark visibility
                var checkmark = _showTimeToggle.Q(className: "unity-toggle__checkmark");
                if (checkmark?.userData is Label checkmarkLabel)
                {
                    checkmarkLabel.style.display = evt_.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            _showFrameCountToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsolePrefsFlags flags = evt_.newValue ? ConsolePrefsFlags.SHOW_FRAME_COUNT : ConsolePrefsFlags.NONE;
                ConsolePrefs.SetFlags(flags, ConsolePrefsFlags.SHOW_FRAME_COUNT);
                
                // Update checkmark visibility
                var checkmark = _showFrameCountToggle.Q(className: "unity-toggle__checkmark");
                if (checkmark?.userData is Label checkmarkLabel)
                {
                    checkmarkLabel.style.display = evt_.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            _showUnityDebugLogToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsolePrefsFlags flags = evt_.newValue ? ConsolePrefsFlags.SHOW_UNITY_DEBUG_LOG : ConsolePrefsFlags.NONE;
                ConsolePrefs.SetFlags(flags, ConsolePrefsFlags.SHOW_UNITY_DEBUG_LOG);
                _requestedSendingPreferencesPacket = true;
                
                // Update checkmark visibility
                var checkmark = _showUnityDebugLogToggle.Q(className: "unity-toggle__checkmark");
                if (checkmark?.userData is Label checkmarkLabel)
                {
                    checkmarkLabel.style.display = evt_.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            // Preferences fields
            _profilerRefreshTimeField?.RegisterValueChangedCallback(evt_ =>
            {
                if (Math.Abs(evt_.newValue - ConsolePrefs.ProfileRefreshIntervalTimeS) > float.Epsilon)
                {
                    ConsolePrefs.SetProfileRefreshTimeS(evt_.newValue);
                    _requestedSendingPreferencesPacket = true;
                }
            });

            _skipStackFrameField?.RegisterValueChangedCallback(evt_ =>
            {
                int skipStackFrameCount = Mathf.Max(evt_.newValue, 0);
                if (skipStackFrameCount != ConsolePrefs.SkipStackFrameCount)
                {
                    ConsolePrefs.SetSkipStackFrameCount((uint)skipStackFrameCount);
                    _requestedSendingPreferencesPacket = true;
                }
                // Update field value to ensure it's not negative
                if (evt_.newValue != skipStackFrameCount)
                {
                    _skipStackFrameField.SetValueWithoutNotify(skipStackFrameCount);
                }
            });
        }

        private void LoadPreferences()
        {
            // Load show options and update checkmarks
            if (_showTimeToggle != null)
            {
                bool value = ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_TIME);
                _showTimeToggle.value = value;
                UpdateToggleCheckmark(_showTimeToggle, value);
            }
            
            if (_showFrameCountToggle != null)
            {
                bool value = ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_FRAME_COUNT);
                _showFrameCountToggle.value = value;
                UpdateToggleCheckmark(_showFrameCountToggle, value);
            }
            
            if (_showUnityDebugLogToggle != null)
            {
                bool value = ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_UNITY_DEBUG_LOG);
                _showUnityDebugLogToggle.value = value;
                UpdateToggleCheckmark(_showUnityDebugLogToggle, value);
            }

            // Load preferences
            if (_profilerRefreshTimeField != null)
                _profilerRefreshTimeField.value = ConsolePrefs.ProfileRefreshIntervalTimeS;
            
            if (_skipStackFrameField != null)
                _skipStackFrameField.value = (int)ConsolePrefs.SkipStackFrameCount;
        }

        private void UpdateToggleCheckmark(Toggle toggle_, bool isChecked_)
        {
            var checkmark = toggle_.Q(className: "unity-toggle__checkmark");
            if (checkmark?.userData is Label checkmarkLabel)
            {
                checkmarkLabel.style.display = isChecked_ ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        protected override void OnShow()
        {
            _requestedSendingPreferencesPacket = false;
            LoadPreferences(); // Refresh UI when shown
            
            if (_preferencesRootElement != null)
            {
                _preferencesRootElement.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnHide()
        {
            if (_requestedSendingPreferencesPacket)
            {
                base.currentAppRef.SendPreferences();
                _requestedSendingPreferencesPacket = false;
            }
            
            if (_preferencesRootElement != null)
            {
                _preferencesRootElement.style.display = DisplayStyle.None;
            }
        }

        protected override void OnTerminate()
        {
            // Clean up if needed
            _preferencesRootElement?.Clear();
            _preferencesRootElement = null;
        }
    }
}