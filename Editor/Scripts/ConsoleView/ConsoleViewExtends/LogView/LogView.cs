// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Editor
{
    internal class LogView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.LOG;

        private VisualElement _rootElement;
        
        // Widgets
        private LogListWidget _logListWidget;
        private LogStackWidget _logStackWidget;
        
        // Toolbar elements
        private Button _clearButton;
        private Toggle _pauseButton;
        private Button _stepButton;
        private Toggle _showLogToggle;
        private Toggle _showWarningToggle;
        private Toggle _showErrorToggle;
        private Toggle _collapseToggle;
        private Toggle _clearOnPlayToggle;
        private Toggle _errorPauseToggle;
        private TextField _searchField;
        private TextField _excludeField;
        private Button _quickSearchButton;
        private VisualElement _quickSearchList;
        private VisualElement _quickSearchSection;
        private TwoPaneSplitView _mainSplitter;
        
        // Count labels
        private Label _logCountLabel;
        private Label _warningCountLabel;
        private Label _errorCountLabel;
        
        // State
        private bool _isPaused = false;
        
        private bool _needsRefresh = false;
        private float _lastRefreshTime = 0f;
        private const float REFRESH_INTERVAL = 0.1f;
        
        // Quick search state tracking
        private int _lastQuickSearchCount = -1;
        private bool _quickSearchNeedsUpdate = true;

        protected override void OnInitialize()
        {
            CreateUIElements();
            InitializeWidgets();
            BindEvents();
            InitializeSearchFields();
            
            // Subscribe to quick search updates
            QuickSearchEditorWindow.onQuickSearchChanged += RequestQuickSearchUpdate;
            
            // Force initial refresh after a short delay to ensure everything is set up
            UnityEditor.EditorApplication.delayCall += RequestRefresh;
        }

        private void InitializeWidgets()
        {
            // OnOpenConsole log list widget
            _logListWidget = new LogListWidget();
            _logListWidget.Initialize(_rootElement, this);
            _logListWidget.OnLogItemSelected += OnLogItemSelected;
            
            // OnOpenConsole stack widget
            _logStackWidget = new LogStackWidget();
            _logStackWidget.Initialize(_rootElement);
        }

        private void OnLogItemSelected(LogItem logItem_)
        {
            _logStackWidget.UpdateStackTrace(logItem_);
        }

        private void InitializeSearchFields()
        {
            // OnOpenConsole search fields with current preferences and placeholder text
            if (_searchField != null)
            {
                string searchString = ConsoleEditorPrefs.SearchString ?? "";
                SetupPlaceholderTextField(_searchField, searchString, "You can search for a specific string.");
            }
            
            if (_excludeField != null)
            {
                string excludeString = ConsoleEditorPrefs.ExcludeFilterString ?? "";
                SetupPlaceholderTextField(_excludeField, excludeString, "You can exclude for a specific string.");
            }
            
            // OnOpenConsole QuickSearch list
            _quickSearchNeedsUpdate = true; // Force initial update
            UpdateQuickSearchList();
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
                }
            });
        }

        private void CreateUIElements()
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/LogView");
            if (visualTree == null)
            {
                Debug.LogError("LogView.uxml not found in Resources/UI/");
                return;
            }

            _rootElement = visualTree.Instantiate();
            _rootElement.style.width = Length.Percent(100);
            _rootElement.style.height = Length.Percent(100);

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
            var logViewStyles = Resources.Load<StyleSheet>("UI/LogViewStyles");
            
            if (baseStyles != null)
            {
                _rootElement.styleSheets.Add(baseStyles);
            }
            if (logViewStyles != null)
            {
                _rootElement.styleSheets.Add(logViewStyles);
            }

            // Get references to UI elements
            GetAndInitToolbarElements();
        }

        private void GetAndInitToolbarElements()
        {
            _clearButton = _rootElement.Q<Button>("clear-button");
            _pauseButton = _rootElement.Q<Toggle>("pause-button");
            _stepButton = _rootElement.Q<Button>("step-button");
            _showLogToggle = _rootElement.Q<Toggle>("show-log-toggle");
            _showWarningToggle = _rootElement.Q<Toggle>("show-warning-toggle");
            _showErrorToggle = _rootElement.Q<Toggle>("show-error-toggle");
            _collapseToggle = _rootElement.Q<Toggle>("collapse-toggle");
            _clearOnPlayToggle = _rootElement.Q<Toggle>("clear-on-play-toggle");
            _errorPauseToggle = _rootElement.Q<Toggle>("error-pause-toggle");
            _searchField = _rootElement.Q<TextField>("search-field");
            _excludeField = _rootElement.Q<TextField>("exclude-field");
            _quickSearchButton = _rootElement.Q<Button>("quick-search-button");
            _quickSearchList = _rootElement.Q<VisualElement>("quick-search-list");
            _quickSearchSection = _rootElement.Q<VisualElement>("quick-search-section");
            _mainSplitter = _rootElement.Q<TwoPaneSplitView>("main-splitter");
            
            // Count labels
            _logCountLabel = _rootElement.Q<Label>("log-count-label");
            _warningCountLabel = _rootElement.Q<Label>("warning-count-label");
            _errorCountLabel = _rootElement.Q<Label>("error-count-label");
            
            // Update toggle states
            if (_showLogToggle != null)
                _showLogToggle.SetValueWithoutNotify(ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_LOG));
            if (_showWarningToggle != null)
                _showWarningToggle.SetValueWithoutNotify(ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_WARNING));
            if (_showErrorToggle != null)
                _showErrorToggle.SetValueWithoutNotify(ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_ERROR));
            if (_collapseToggle != null)
                _collapseToggle.SetValueWithoutNotify(ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.IS_COLLAPSE));
            if (_clearOnPlayToggle != null)
                _clearOnPlayToggle.SetValueWithoutNotify(ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.IS_CLEAR_ON_PLAY));
            if (_errorPauseToggle != null)
                _errorPauseToggle.SetValueWithoutNotify(ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.ERROR_PAUSE));
            
            // Configure the splitter
            ConfigureSplitter();
        }

        private void ConfigureSplitter()
        {
            if (_mainSplitter != null)
            {
                // Set stack-trace-panel as the fixed pane (index 1), with initial height of 120px
                _mainSplitter.fixedPaneIndex = 1;
                _mainSplitter.fixedPaneInitialDimension = 120;
                _mainSplitter.orientation = TwoPaneSplitViewOrientation.Vertical;
                
                // Ensure the splitter is visible and takes up space
                _mainSplitter.style.flexGrow = 1;
                _mainSplitter.style.minHeight = 200;
                
                // Load saved splitter position from preferences if available
                const string PREF_KEY = "SRemoteConsole.LogView.StackTracePanelHeight";
                if (UnityEditor.EditorPrefs.HasKey(PREF_KEY))
                {
                    float savedHeight = UnityEditor.EditorPrefs.GetFloat(PREF_KEY);
                    if (savedHeight >= 80 && savedHeight <= 400) // Sanity check for stack trace panel height
                    {
                        _mainSplitter.fixedPaneInitialDimension = savedHeight;
                    }
                }
                
                // Force an initial refresh to show content
                UnityEditor.EditorApplication.delayCall += RequestRefresh;
                
                // Save splitter position when changed
                _mainSplitter.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    if (_mainSplitter.fixedPane != null)
                    {
                        float height = _mainSplitter.fixedPane.resolvedStyle.height;
                        if (height >= 80 && height <= 400) // Only save reasonable stack trace panel heights
                        {
                            UnityEditor.EditorPrefs.SetFloat(PREF_KEY, height);
                        }
                    }
                });
            }
        }

        private void BindEvents()
        {
            _clearButton?.RegisterCallback<ClickEvent>(_ => ClearLogs());
            _pauseButton?.RegisterValueChangedCallback(evt_ => {
                _isPaused = evt_.newValue;
                UpdatePauseIcon();
                if (CurrentAppRef != null)
                {
                    CurrentAppRef.SendPause(_isPaused, false, ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.ERROR_PAUSE));
                }
            });
            _stepButton?.RegisterCallback<ClickEvent>(_ => Step());

            // Filter toggles
            _showLogToggle?.RegisterValueChangedCallback(evt_ => {
                ConsoleEditorPrefs.SetFlags(evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_LOG : ConsoleEditorPrefsFlags.NONE, 
                                          ConsoleEditorPrefsFlags.SHOW_LOG);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _showWarningToggle?.RegisterValueChangedCallback(evt_ => {
                ConsoleEditorPrefs.SetFlags(evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_WARNING : ConsoleEditorPrefsFlags.NONE, 
                                          ConsoleEditorPrefsFlags.SHOW_WARNING);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _showErrorToggle?.RegisterValueChangedCallback(evt_ => {
                ConsoleEditorPrefs.SetFlags(evt_.newValue ? ConsoleEditorPrefsFlags.SHOW_ERROR : ConsoleEditorPrefsFlags.NONE, 
                                          ConsoleEditorPrefsFlags.SHOW_ERROR);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _collapseToggle?.RegisterValueChangedCallback(evt_ => {
                ConsoleEditorPrefs.SetFlags(evt_.newValue ? ConsoleEditorPrefsFlags.IS_COLLAPSE : ConsoleEditorPrefsFlags.NONE, 
                                          ConsoleEditorPrefsFlags.IS_COLLAPSE);
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            });

            _clearOnPlayToggle?.RegisterValueChangedCallback(evt_ => {
                ConsoleEditorPrefs.SetFlags(evt_.newValue ? ConsoleEditorPrefsFlags.IS_CLEAR_ON_PLAY : ConsoleEditorPrefsFlags.NONE, 
                                          ConsoleEditorPrefsFlags.IS_CLEAR_ON_PLAY);
            });

            _errorPauseToggle?.RegisterValueChangedCallback(evt_ => {
                ConsoleEditorPrefs.SetFlags(evt_.newValue ? ConsoleEditorPrefsFlags.ERROR_PAUSE : ConsoleEditorPrefsFlags.NONE, 
                                          ConsoleEditorPrefsFlags.ERROR_PAUSE);
            });

            // Search fields - handle real-time filtering like IMGUI version
            _searchField?.RegisterValueChangedCallback(evt_ => {
                string newValue = evt_.newValue ?? "";
                
                // Skip if this is placeholder text
                if (_searchField.ClassListContains("placeholder-text"))
                {
                    return;
                }
                
                string oldValue = ConsoleEditorPrefs.SearchString ?? "";
                if (!oldValue.Equals(newValue))
                {
                    ConsoleEditorPrefs.SearchString = newValue;
                    if (CurrentAppRef?.logCollection != null)
                    {
                        CurrentAppRef.logCollection.FilteringBuffer();
                        RequestRefresh();
                    }
                }
            });

            _excludeField?.RegisterValueChangedCallback(evt_ => {
                string newValue = evt_.newValue ?? "";
                
                // Skip if this is placeholder text
                if (_excludeField.ClassListContains("placeholder-text"))
                {
                    return;
                }
                
                string oldValue = ConsoleEditorPrefs.ExcludeFilterString ?? "";
                if (!oldValue.Equals(newValue))
                {
                    ConsoleEditorPrefs.ExcludeFilterString = newValue;
                    if (CurrentAppRef?.logCollection != null)
                    {
                        CurrentAppRef.logCollection.FilteringBuffer();
                        RequestRefresh();
                    }
                }
            });

            _quickSearchButton?.RegisterCallback<ClickEvent>(_ => ShowQuickSearch());
        }

        private void ClearLogs()
        {
            ConsoleEditorPrefs.CanClearLog = true;
        }

        private void UpdatePauseIcon()
        {
            if (_pauseButton == null) return;
            
            var pauseIcon = _pauseButton.Q<VisualElement>("pause-icon");
            if (pauseIcon != null)
            {
                if (CurrentAppRef != null && CurrentAppRef.IsPlaying())
                {
                    // Use play-specific icons when app is playing
                    pauseIcon.RemoveFromClassList("pause-icon");
                    pauseIcon.AddToClassList(_isPaused ? "pause-play-icon" : "pause-icon");
                }
                else
                {
                    // Use regular pause icon when not playing
                    pauseIcon.RemoveFromClassList("pause-play-icon");
                    pauseIcon.AddToClassList("pause-icon");
                }
            }
        }

        private void Step()
        {
            // Step functionality - advance one frame if paused
            if (CurrentAppRef != null)
            {
                _isPaused = true;
                _pauseButton?.SetValueWithoutNotify(_isPaused);
                UpdatePauseIcon();
                CurrentAppRef.SendPause(_isPaused, true, ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.ERROR_PAUSE));
            }
        }

        private void ShowQuickSearch()
        {
            // Open quick search window
            var window = UnityEditor.EditorWindow.GetWindow<QuickSearchEditorWindow>();
            window.Show();
        }

        private void UpdateQuickSearchList()
        {
            if (_quickSearchList == null) return;
            
            int quickSearchCount = ConsoleEditorPrefs.GetQuickSearchListCount();
            
            // Only update if quick search count changed or forced update is needed
            if (!_quickSearchNeedsUpdate && _lastQuickSearchCount == quickSearchCount) return;
            
            // Clear existing items
            _quickSearchList.Clear();
            if (_quickSearchSection != null)
            {
                _quickSearchSection.style.height = StyleKeyword.Auto;
            }
            
            if (quickSearchCount <= 0)
            {
                // Show help text when no quick search items
                var helpLabel = new Label("<--- You can add a quick search string by clicking the [QuickSearch+] button.");
                helpLabel.AddToClassList("quick-search-help");
                _quickSearchList.Add(helpLabel);
            }
            else
            {
                // Create layout similar to IMGUI implementation
                CreateQuickSearchLayout();
            }
            
            // Update tracking variables
            _lastQuickSearchCount = quickSearchCount;
            _quickSearchNeedsUpdate = false;
        }

        // Removed AdjustQuickSearchHeight - using CSS auto height instead

        private void CreateQuickSearchLayout()
        {
            int quickSearchCount = ConsoleEditorPrefs.GetQuickSearchListCount();
            if (quickSearchCount <= 0) return;

            // Simply add all toggles and let UIToolkit flex system handle the layout
            for (int i = 0; i < quickSearchCount; i++)
            {
                var searchContext = ConsoleEditorPrefs.GetQuickSearchContext(i);
                if (searchContext == null) continue;

                string displayText = searchContext.SearchString;
                if (searchContext.SearchCount > 0)
                {
                    displayText = $"{searchContext.SearchString}({searchContext.SearchCount})";
                }

                // Calculate minimum width for text content
                const float WIDTH_PER_CHARACTER = 8f;
                const float MIN_PADDING = 20f;
                float minWidth = Mathf.Max(60f, displayText.Length * WIDTH_PER_CHARACTER + MIN_PADDING);

                var toggle = CreateQuickSearchToggle(searchContext, minWidth);
                _quickSearchList.Add(toggle);
            }
        }

        private Toggle CreateQuickSearchToggle(ConsoleEditorPrefsSearchContext searchContext_, float minWidth_)
        {
            string displayText = searchContext_.SearchString;
            if (searchContext_.SearchCount > 0)
            {
                displayText = $"{searchContext_.SearchString}({searchContext_.SearchCount})";
            }
            
            var toggle = new Toggle(displayText);
            toggle.value = searchContext_.DoSearching;
            toggle.AddToClassList("button-style-toggle");
            toggle.AddToClassList("quick-search-toggle");
            
            // Set minimum width (will be adjusted in FinalizeLine)
            toggle.style.minWidth = minWidth_;
            toggle.style.flexGrow = 0;
            toggle.style.flexShrink = 0;

            // Handle value change - matches IMGUI behavior
            toggle.RegisterValueChangedCallback(OnValueChanged);

            return toggle;

            void OnValueChanged(ChangeEvent<bool> event_)
            {
                searchContext_.DoSearching = event_.newValue;
                ConsoleEditorPrefs.WriteSearchStringListPrefs();
                if (CurrentAppRef?.logCollection != null)
                {
                    CurrentAppRef.logCollection.FilteringBuffer();
                    RequestRefresh();
                }
            }
        }

        public override void UpdateCustom()
        {
            if (CurrentAppRef == null) return;

            // Update search fields - only if they're different to avoid infinite loops
            if (_searchField != null)
            {
                string currentSearchValue = ConsoleEditorPrefs.SearchString ?? "";
                if (_searchField.value != currentSearchValue)
                {
                    _searchField.SetValueWithoutNotify(currentSearchValue);
                }
            }
            if (_excludeField != null)
            {
                string currentExcludeValue = ConsoleEditorPrefs.ExcludeFilterString ?? "";
                if (_excludeField.value != currentExcludeValue)
                {
                    _excludeField.SetValueWithoutNotify(currentExcludeValue);
                }
            }

            // Update count labels
            UpdateCountLabels();
            
            // Update pause button state
            _pauseButton?.SetValueWithoutNotify(_isPaused);
            UpdatePauseIcon();
            
            _logListWidget?.UpdateHeaderVisibility();
            
            // Throttle refresh to prevent too frequent updates
            float currentTime = UnityEngine.Time.realtimeSinceStartup;
            if (_needsRefresh || (currentTime - _lastRefreshTime) > REFRESH_INTERVAL)
            {
                _logListWidget?.RefreshLogList();
                _lastRefreshTime = currentTime;
                _needsRefresh = false;
            }
            
            UpdateQuickSearchList();

            // Handle clear log
            if (ConsoleEditorPrefs.CanClearLog)
            {
                CurrentAppRef.logCollection.ClearItems();
                ConsoleEditorPrefs.CanClearLog = false;
                _logListWidget?.RefreshLogList();
            }
        }

        public void RequestRefresh()
        {
            _needsRefresh = true;
        }
        
        public void RequestQuickSearchUpdate()
        {
            _quickSearchNeedsUpdate = true;
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
                _logCountLabel.style.color = new StyleColor(ConsoleEditorPrefs.TextColor);
                
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
                _warningCountLabel.style.color = new StyleColor(ConsoleEditorPrefs.WarningTextColor);
                
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
                _errorCountLabel.style.color = new StyleColor(ConsoleEditorPrefs.ErrorTextColor);
                
                // Adjust toggle width based on text length
                var errorToggle = _showErrorToggle;
                if (errorToggle != null)
                {
                    float width = BASE_WIDTH + (errorCount.ToString().Length * CHAR_WIDTH);
                    errorToggle.style.minWidth = width;
                }
            }
        }

        public void ForcePause(bool isPause_, bool canStep_)
        {
            _isPaused = isPause_;
            _pauseButton?.SetValueWithoutNotify(_isPaused);
            UpdatePauseIcon();
            
            if (_stepButton != null)
            {
                _stepButton.SetEnabled(canStep_);
            }
        }

        public VisualElement GetRootElement()
        {
            return _rootElement;
        }

        protected override void OnTerminate()
        {
            // Unsubscribe from quick search updates
            QuickSearchEditorWindow.onQuickSearchChanged -= RequestQuickSearchUpdate;
            
            // Cleanup widgets
            _logListWidget?.Cleanup();
            
            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
        }
    }
}