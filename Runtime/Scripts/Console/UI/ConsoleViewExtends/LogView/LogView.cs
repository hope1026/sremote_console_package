// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class LogView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.LOG;
        private VisualElement _logViewRootElement;
        
        // Widgets
        private LogListWidget _logListWidget;
        
        // Toolbar elements
        private Button _clearButton;
        private Toggle _showLogToggle;
        private Toggle _showWarningToggle;
        private Toggle _showErrorToggle;
        private Toggle _collapseToggle;
        private TextField _searchField;
        
        // Count labels
        private Label _logCountLabel;
        private Label _warningCountLabel;
        private Label _errorCountLabel;
        
        // State
        private bool _needsRefresh = false;
        private float _lastRefreshTime = 0f;
        private const float REFRESH_INTERVAL = 0.1f;

        protected override void OnInitialize()
        {
            CreateUIElements();
            InitializeWidgets();
            BindEvents();
            InitializeSearchFields();
        }

        private void InitializeWidgets()
        {
            // Initialize log list widget
            _logListWidget = new LogListWidget();
            _logListWidget.Initialize(_logViewRootElement, this);
        }

        private void InitializeSearchFields()
        {
            // Initialize search fields with current preferences and placeholder text
            if (_searchField != null)
            {
                string searchString = ConsolePrefs.SearchString ?? "";
                SetupPlaceholderTextField(_searchField, searchString, "You can search for a specific string.");
                
                // If placeholder is shown, ensure search string is empty
                if (_searchField.ClassListContains("placeholder-text"))
                {
                    ConsolePrefs.SearchString = "";
                }
            }
        }
        
        protected override void OnShow()
        {
            if (_logViewRootElement!= null)
            {
                _logViewRootElement.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnHide()
        {
            if (_logViewRootElement != null)
            {
                _logViewRootElement.style.display = DisplayStyle.None;
            }
        }


        private void SetupPlaceholderTextField(TextField textField_, string currentValue_, string placeholderText_)
        {
            bool isEmpty = string.IsNullOrEmpty(currentValue_);
            
            if (isEmpty)
            {
                // Show placeholder text with gray color
                textField_.value = placeholderText_;
                textField_.AddToClassList("placeholder-text");
            }
            else
            {
                // Show actual value
                textField_.value = currentValue_;
                textField_.RemoveFromClassList("placeholder-text");
            }
            
            // Handle focus events
            textField_.RegisterCallback<FocusInEvent>(_ => {
                if (textField_.ClassListContains("placeholder-text"))
                {
                    textField_.value = "";
                    textField_.RemoveFromClassList("placeholder-text");
                }
            });
            
            textField_.RegisterCallback<FocusOutEvent>(_ => {
                if (string.IsNullOrEmpty(textField_.value))
                {
                    textField_.value = placeholderText_;
                    textField_.AddToClassList("placeholder-text");
                    
                    // Clear search string when showing placeholder (only for search field)
                    if (textField_.name == "search-field")
                    {
                        ConsolePrefs.SearchString = "";
                        if (CurrentAppRef?.logCollection != null)
                        {
                            CurrentAppRef.logCollection.FilteringBuffer();
                            RequestRefresh();
                        }
                    }
                }
            });
        }

        private void CreateUIElements()
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/LogViewRuntime");
            if (visualTree == null)
            {
                Debug.LogError("LogView.uxml not found in Resources/UI/");
                return;
            }

            _logViewRootElement = visualTree.Instantiate();
            _rootElement.Add(_logViewRootElement);
            _logViewRootElement.name = "log-view";
            _logViewRootElement.AddToClassList("log-view-full-size");

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseRuntimeStyles");
            var logViewStyles = Resources.Load<StyleSheet>("UI/LogViewRuntimeStyles");
            
            if (baseStyles != null)
            {
                _logViewRootElement.styleSheets.Add(baseStyles);
            }
            if (logViewStyles != null)
            {
                _logViewRootElement.styleSheets.Add(logViewStyles);
            }

            // Apply fonts programmatically since CSS font references don't work at runtime
            UIResourceHelper.ApplyDefaultFontRecursive(_logViewRootElement);

            // Get references to UI elements
            GetAndInitToolbarElements();
        }

        private void GetAndInitToolbarElements()
        {
            _clearButton = _logViewRootElement.Q<Button>("clear-button");
            _showLogToggle = _logViewRootElement.Q<Toggle>("show-log-toggle");
            _showWarningToggle = _logViewRootElement.Q<Toggle>("show-warning-toggle");
            _showErrorToggle = _logViewRootElement.Q<Toggle>("show-error-toggle");
            _collapseToggle = _logViewRootElement.Q<Toggle>("collapse-toggle");
            _searchField = _logViewRootElement.Q<TextField>("search-field");
            
            // Count labels
            _logCountLabel = _logViewRootElement.Q<Label>("log-count-label");
            _warningCountLabel = _logViewRootElement.Q<Label>("warning-count-label");
            _errorCountLabel = _logViewRootElement.Q<Label>("error-count-label");
            
            // Apply fonts to all UI elements after they are queried
            ApplyFontsToUIElements();
            
            // Update toggle states
            if (_showLogToggle != null)
                _showLogToggle.SetValueWithoutNotify(ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_LOG));
            
            if (_showWarningToggle != null)
                _showWarningToggle.SetValueWithoutNotify(ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_WARNING));
            
            if (_showErrorToggle != null)
                _showErrorToggle.SetValueWithoutNotify(ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_ERROR));
            
            if (_collapseToggle != null)
                _collapseToggle.SetValueWithoutNotify(ConsolePrefs.GetFlagState(ConsolePrefsFlags.IS_COLLAPSE));
        }

        private void ApplyFontsToUIElements()
        {
            // Apply fonts to buttons
            if (_clearButton != null)
            {
                _clearButton.schedule.Execute(() => {
                    UIResourceHelper.ApplyDefaultFont(_clearButton);
                });
            }

            // Apply fonts to toggles and their labels
            ApplyFontToToggle(_showLogToggle);
            ApplyFontToToggle(_showWarningToggle);
            ApplyFontToToggle(_showErrorToggle);
            ApplyFontToToggle(_collapseToggle);

            // Apply fonts to search field
            if (_searchField != null)
            {
                _searchField.schedule.Execute(() => {
                    UIResourceHelper.ApplyDefaultFont(_searchField);
                    var textElement = _searchField.Q<Label>();
                    if (textElement != null)
                    {
                        UIResourceHelper.ApplyDefaultFont(textElement);
                    }
                });
            }

            // Apply fonts to count labels
            if (_logCountLabel != null)
            {
                _logCountLabel.schedule.Execute(() => {
                    UIResourceHelper.ApplyDefaultFont(_logCountLabel);
                });
            }
            if (_warningCountLabel != null)
            {
                _warningCountLabel.schedule.Execute(() => {
                    UIResourceHelper.ApplyDefaultFont(_warningCountLabel);
                });
            }
            if (_errorCountLabel != null)
            {
                _errorCountLabel.schedule.Execute(() => {
                    UIResourceHelper.ApplyDefaultFont(_errorCountLabel);
                });
            }
        }

        private void ApplyFontToToggle(Toggle toggle_)
        {
            if (toggle_ == null) return;

            toggle_.schedule.Execute(() => {
                UIResourceHelper.ApplyDefaultFont(toggle_);
                var labelElement = toggle_.Q<Label>();
                if (labelElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(labelElement);
                }
            });
        }

        private void BindEvents()
        {
            _clearButton?.RegisterCallback<ClickEvent>(_ => ClearLogs());

            // Filter toggles
            _showLogToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsolePrefs.SetFlags(evt_.newValue ? ConsolePrefsFlags.SHOW_LOG : ConsolePrefsFlags.NONE, ConsolePrefsFlags.SHOW_LOG);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _showWarningToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsolePrefs.SetFlags(evt_.newValue ? ConsolePrefsFlags.SHOW_WARNING : ConsolePrefsFlags.NONE, ConsolePrefsFlags.SHOW_WARNING);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _showErrorToggle?.RegisterValueChangedCallback(evt_ =>
            {
                ConsolePrefs.SetFlags(evt_.newValue ? ConsolePrefsFlags.SHOW_ERROR : ConsolePrefsFlags.NONE, ConsolePrefsFlags.SHOW_ERROR);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _collapseToggle?.RegisterValueChangedCallback(evt_ => {
                ConsolePrefs.SetFlags(evt_.newValue ? ConsolePrefsFlags.IS_COLLAPSE : ConsolePrefsFlags.NONE, ConsolePrefsFlags.IS_COLLAPSE);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            // Search field - handle real-time filtering
            _searchField?.RegisterValueChangedCallback(evt_ => {
                string newValue = evt_.newValue ?? "";
                
                // Skip if this is placeholder text
                if (_searchField.ClassListContains("placeholder-text"))
                {
                    return;
                }
                
                
                string oldValue = ConsolePrefs.SearchString ?? "";
                if (!oldValue.Equals(newValue))
                {
                    ConsolePrefs.SearchString = newValue;
                    if (CurrentAppRef?.logCollection != null)
                    {
                        CurrentAppRef.logCollection.FilteringBuffer();
                        RequestRefresh();
                    }
                }
            });
        }

        private void ClearLogs()
        {
            ConsolePrefs.CanClearLog = true;
        }


        public override void UpdateCustom()
        {
            if (CurrentAppRef == null) return;

            // Update search field - only if different to avoid infinite loops
            if (_searchField != null)
            {
                string currentSearchValue = ConsolePrefs.SearchString ?? "";
                
                // Don't update if field is showing placeholder text
                if (!_searchField.ClassListContains("placeholder-text") && _searchField.value != currentSearchValue)
                {
                    _searchField.SetValueWithoutNotify(currentSearchValue);
                }
                
            }

            // Update count labels
            UpdateCountLabels();
            
            _logListWidget?.UpdateHeaderVisibility();
            
            // Throttle refresh to prevent too frequent updates
            float currentTime = UnityEngine.Time.realtimeSinceStartup;
            if (_needsRefresh || (currentTime - _lastRefreshTime) > REFRESH_INTERVAL)
            {
                _logListWidget?.RefreshLogList();
                _lastRefreshTime = currentTime;
                _needsRefresh = false;
            }

            // Handle clear log
            if (ConsolePrefs.CanClearLog)
            {
                CurrentAppRef.logCollection.ClearItems();
                ConsolePrefs.CanClearLog = false;
                _logListWidget?.RefreshLogList();
            }
        }

        public void RequestRefresh()
        {
            _needsRefresh = true;
        }

        private void UpdateCountLabels()
        {
            if (CurrentAppRef?.logCollection == null) return;

            const float BASE_WIDTH = 40f;
            const float CHAR_WIDTH = 8f;

            if (_logCountLabel != null)
            {
                var logCount = CurrentAppRef.logCollection.LogCount;
                _logCountLabel.text = logCount.ToString();
                
                // Adjust toggle width based on text length
                var logToggle = _showLogToggle;
                if (logToggle != null)
                {
                    float width = BASE_WIDTH + (logCount.ToString().Length * CHAR_WIDTH);
                    logToggle.style.minWidth = width;
                }
            }
            
            if (_warningCountLabel != null)
            {
                var warningCount = CurrentAppRef.logCollection.WarningCount;
                _warningCountLabel.text = warningCount.ToString();
                
                // Adjust toggle width based on text length
                var warningToggle = _showWarningToggle;
                if (warningToggle != null)
                {
                    float width = BASE_WIDTH + (warningCount.ToString().Length * CHAR_WIDTH);
                    warningToggle.style.minWidth = width;
                }
            }
            
            if (_errorCountLabel != null)
            {
                var errorCount = CurrentAppRef.logCollection.ErrorCount;
                _errorCountLabel.text = errorCount.ToString();
                
                // Adjust toggle width based on text length
                var errorToggle = _showErrorToggle;
                if (errorToggle != null)
                {
                    float width = BASE_WIDTH + (errorCount.ToString().Length * CHAR_WIDTH);
                    errorToggle.style.minWidth = width;
                }
            }
        }

        protected override void OnTerminate()
        {
            // Cleanup widgets
            _logListWidget?.Cleanup();
            
            _logViewRootElement?.RemoveFromHierarchy();
            _logViewRootElement = null;
        }
    }
}