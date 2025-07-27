// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Editor
{
    internal class ApplicationsView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.APPLICATIONS;

        private VisualElement _rootElement;
        private VisualElement _currentAppRow;
        private VisualElement _localAppsContainer;
        private VisualElement _remoteAppsContainer;
        
        // System info panel elements
        private VisualElement _systemInfoPanel;
        private VisualElement _systemInfoContent;
        private Button _closeSystemInfoButton;
        private AppAbstract _currentDisplayedApp;
        
        // Header elements for column resizing
        private Label _deviceNameHeader;
        private Label _productNameHeader;
        private Label _ipAddressHeader;
        private Label _stateHeader;
        private VisualElement _deviceNameSeparator;
        private VisualElement _productNameSeparator;
        private VisualElement _ipAddressSeparator;
        
        // Column widths
        private float _deviceNameWidth = 120f;
        private float _productNameWidth = 120f;
        private float _ipAddressWidth = 100f;
        private float _stateWidth = 80f;
        
        // Scanning controls
        private Button _scanNetworkButton;
        private TextField _directIpField;
        private Button _connectDirectButton;
        private VisualElement _scanningControls;
        private VisualElement _scanningProgress;
        private Label _scanningLabel;
        private Button _cancelScanButton;

        private string _directConnectionIp = string.Empty;
        
        // State tracking for performance optimization
        private AppAbstract _lastCurrentApp = null;
        private AppAbstract _lastLocalApp = null;
        private int _lastRemoteAppCount = -1;
        private bool _lastIsScanning = false;
        private float _lastScanProgress = -1f;
        private string _lastRemoteAppsStateHash = "";

        protected override void OnInitialize()
        {
            CreateUIElements();
            BindEvents();
            LoadInitialData();
        }

        protected override void OnShow()
        {
            // Reset state tracking to force initial update
            _lastCurrentApp = null;
            _lastLocalApp = null;
            _lastRemoteAppCount = -1;
            _lastIsScanning = false;
            _lastScanProgress = -1f;
            _lastRemoteAppsStateHash = "";
            
            // Hide system info panel when switching views
            HideSystemInfoPanel();
        }

        private void CreateUIElements()
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/ApplicationsView");
            if (visualTree == null)
            {
                Debug.LogError("ApplicationsView.uxml not found in Resources/UI/");
                return;
            }

            _rootElement = visualTree.Instantiate();

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
            var applicationsViewStyles = Resources.Load<StyleSheet>("UI/ApplicationsViewStyles");
            
            if (baseStyles != null)
            {
                _rootElement.styleSheets.Add(baseStyles);
            }
            if (applicationsViewStyles != null)
            {
                _rootElement.styleSheets.Add(applicationsViewStyles);
            }

            // Get references to UI elements
            _currentAppRow = _rootElement.Q<VisualElement>("current-app-row");
            _localAppsContainer = _rootElement.Q<VisualElement>("local-apps-container");
            _remoteAppsContainer = _rootElement.Q<VisualElement>("remote-apps-container");
            
            // Get system info panel elements
            _systemInfoPanel = _rootElement.Q<VisualElement>("system-info-panel");
            _systemInfoContent = _rootElement.Q<VisualElement>("system-info-content");
            _closeSystemInfoButton = _rootElement.Q<Button>("close-system-info-button");
            
            // Get header elements
            _deviceNameHeader = _rootElement.Q<Label>("device-name-header");
            _productNameHeader = _rootElement.Q<Label>("product-name-header");
            _ipAddressHeader = _rootElement.Q<Label>("ip-address-header");
            _stateHeader = _rootElement.Q<Label>("state-header");
            _deviceNameSeparator = _rootElement.Q<VisualElement>("device-name-separator");
            _productNameSeparator = _rootElement.Q<VisualElement>("product-name-separator");
            _ipAddressSeparator = _rootElement.Q<VisualElement>("ip-address-separator");
            
            _scanNetworkButton = _rootElement.Q<Button>("scan-network-button");
            _directIpField = _rootElement.Q<TextField>("direct-ip-field");
            _connectDirectButton = _rootElement.Q<Button>("connect-direct-button");
            _scanningControls = _rootElement.Q<VisualElement>("scanning-controls");
            _scanningProgress = _rootElement.Q<VisualElement>("scanning-progress");
            _scanningLabel = _rootElement.Q<Label>("scanning-label");
            _cancelScanButton = _rootElement.Q<Button>("cancel-scan-button");
            
            // Setup column resizing
            SetupColumnResizing();
        }

        private void BindEvents()
        {
            _scanNetworkButton?.RegisterCallback<ClickEvent>(_ =>
            {
                AppManager.Instance.ScanRemoteAppsInPrivateNetwork();
            });

            _connectDirectButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (!string.IsNullOrEmpty(_directIpField.value))
                {
                    AppManager.Instance.ScanRemoteAppByIp(_directIpField.value);
                }
            });

            _cancelScanButton?.RegisterCallback<ClickEvent>(_ =>
            {
                AppManager.Instance.CancelAllScanningApps();
            });

            _directIpField?.RegisterValueChangedCallback(evt_ =>
            {
                _directConnectionIp = evt_.newValue;
            });

            _closeSystemInfoButton?.RegisterCallback<ClickEvent>(_ =>
            {
                HideSystemInfoPanel();
            });
        }

        private void SetupColumnResizing()
        {
            // Load saved column widths from preferences
            LoadColumnWidths();
            
            // Apply initial column widths
            ApplyColumnWidths();
            
            // Setup resize manipulators
            if (_deviceNameSeparator != null && _deviceNameHeader != null && _productNameHeader != null)
            {
                var deviceNameManipulator = new ColumnResizeManipulator(_deviceNameHeader, _productNameHeader, "device-name", 60f);
                deviceNameManipulator.OnColumnResized += OnColumnResized;
                _deviceNameSeparator.AddManipulator(deviceNameManipulator);
            }
            
            if (_productNameSeparator != null && _productNameHeader != null && _ipAddressHeader != null)
            {
                var productNameManipulator = new ColumnResizeManipulator(_productNameHeader, _ipAddressHeader, "product-name", 60f);
                productNameManipulator.OnColumnResized += OnColumnResized;
                _productNameSeparator.AddManipulator(productNameManipulator);
            }
            
            if (_ipAddressSeparator != null && _ipAddressHeader != null && _stateHeader != null)
            {
                var ipAddressManipulator = new ColumnResizeManipulator(_ipAddressHeader, _stateHeader, "ip-address", 60f);
                ipAddressManipulator.OnColumnResized += OnColumnResized;
                _ipAddressSeparator.AddManipulator(ipAddressManipulator);
            }
        }
        
        private void LoadColumnWidths()
        {
            const string PREF_PREFIX = "SRemoteConsole.ApplicationsView.Column.";
            
            if (UnityEditor.EditorPrefs.HasKey(PREF_PREFIX + "DeviceName"))
                _deviceNameWidth = UnityEditor.EditorPrefs.GetFloat(PREF_PREFIX + "DeviceName", 120f);
            if (UnityEditor.EditorPrefs.HasKey(PREF_PREFIX + "ProductName"))
                _productNameWidth = UnityEditor.EditorPrefs.GetFloat(PREF_PREFIX + "ProductName", 120f);
            if (UnityEditor.EditorPrefs.HasKey(PREF_PREFIX + "IpAddress"))
                _ipAddressWidth = UnityEditor.EditorPrefs.GetFloat(PREF_PREFIX + "IpAddress", 100f);
            if (UnityEditor.EditorPrefs.HasKey(PREF_PREFIX + "State"))
                _stateWidth = UnityEditor.EditorPrefs.GetFloat(PREF_PREFIX + "State", 80f);
        }
        
        private void ApplyColumnWidths()
        {
            if (_deviceNameHeader != null)
                _deviceNameHeader.style.width = _deviceNameWidth;
            if (_productNameHeader != null)
                _productNameHeader.style.width = _productNameWidth;
            if (_ipAddressHeader != null)
                _ipAddressHeader.style.width = _ipAddressWidth;
            if (_stateHeader != null)
                _stateHeader.style.width = _stateWidth;
        }
        
        private void OnColumnResized(string columnType_, float newWidth_)
        {
            const string PREF_PREFIX = "SRemoteConsole.ApplicationsView.Column.";
            
            switch (columnType_)
            {
                case "device-name":
                    _deviceNameWidth = newWidth_;
                    UnityEditor.EditorPrefs.SetFloat(PREF_PREFIX + "DeviceName", newWidth_);
                    break;
                case "product-name":
                    _productNameWidth = newWidth_;
                    UnityEditor.EditorPrefs.SetFloat(PREF_PREFIX + "ProductName", newWidth_);
                    break;
                case "ip-address":
                    _ipAddressWidth = newWidth_;
                    UnityEditor.EditorPrefs.SetFloat(PREF_PREFIX + "IpAddress", newWidth_);
                    break;
                case "state":
                    _stateWidth = newWidth_;
                    UnityEditor.EditorPrefs.SetFloat(PREF_PREFIX + "State", newWidth_);
                    break;
            }
            
            // Update all app rows to match new column widths
            UpdateAllAppRowColumnWidths();
        }
        
        private void UpdateAllAppRowColumnWidths()
        {
            // Update current app row
            if (_currentAppRow != null)
            {
                UpdateAppRowColumnWidths(_currentAppRow);
            }
            
            // Update local apps
            if (_localAppsContainer != null)
            {
                foreach (VisualElement child in _localAppsContainer.Children())
                {
                    if (child.ClassListContains("app-row"))
                    {
                        UpdateAppRowColumnWidths(child);
                    }
                }
            }
            
            // Update remote apps
            if (_remoteAppsContainer != null)
            {
                foreach (VisualElement child in _remoteAppsContainer.Children())
                {
                    if (child.ClassListContains("app-row"))
                    {
                        UpdateAppRowColumnWidths(child);
                    }
                }
            }
        }
        
        private void UpdateAppRowColumnWidths(VisualElement appRow_)
        {
            var deviceNameCell = appRow_.Q(className: "device-name-cell");
            var productNameCell = appRow_.Q(className: "product-name-cell");
            var ipAddressCell = appRow_.Q(className: "ip-address-cell");
            var stateCell = appRow_.Q(className: "state-cell");
            
            if (deviceNameCell != null)
                deviceNameCell.style.width = _deviceNameWidth;
            if (productNameCell != null)
                productNameCell.style.width = _productNameWidth;
            if (ipAddressCell != null)
                ipAddressCell.style.width = _ipAddressWidth;
            if (stateCell != null)
                stateCell.style.width = _stateWidth;
        }

        private void LoadInitialData()
        {
            if (string.IsNullOrEmpty(_directConnectionIp))
            {
                _directConnectionIp = SConsoleNetworkUtil.GetLocalIpAddress();
                if (_directIpField != null)
                {
                    _directIpField.value = _directConnectionIp;
                }
            }
        }

        public override void UpdateCustom()
        {
            CheckAndUpdateCurrentApp();
            CheckAndUpdateLocalApps();
            CheckAndUpdateRemoteApps();
            CheckAndUpdateScanningState();
            
            // Update system info if panel is visible
            if (_currentDisplayedApp != null && _systemInfoPanel != null && !_systemInfoPanel.ClassListContains("hidden"))
            {
                UpdateSystemInfoContent();
            }
        }

        private void CheckAndUpdateCurrentApp()
        {
            var currentApp = AppManager.Instance.GetActivatedApp();
            if (_lastCurrentApp != currentApp)
            {
                _lastCurrentApp = currentApp;
                UpdateCurrentApp();
            }
        }

        private void CheckAndUpdateLocalApps()
        {
            var localApp = AppManager.Instance.GetLocalApp();
            if (_lastLocalApp != localApp)
            {
                _lastLocalApp = localApp;
                UpdateLocalApps();
            }
        }

        private void CheckAndUpdateRemoteApps()
        {
            var remoteApps = AppManager.Instance.remoteAppsByAddress;
            int currentRemoteAppCount = remoteApps?.Count ?? 0;
            
            // Create a simple hash of remote apps state for change detection
            string currentStateHash = "";
            if (remoteApps != null)
            {
                foreach (var app in remoteApps.Values)
                {
                    currentStateHash += $"{app.IpAddressString}_{app.AppConnectionStateType}_{app.IsActivated};";
                }
            }
            
            if (_lastRemoteAppCount != currentRemoteAppCount || _lastRemoteAppsStateHash != currentStateHash)
            {
                _lastRemoteAppCount = currentRemoteAppCount;
                _lastRemoteAppsStateHash = currentStateHash;
                UpdateRemoteApps();
            }
        }

        private void CheckAndUpdateScanningState()
        {
            bool isScanning = AppManager.Instance.IsScanning();
            float scanProgress = 0f;
            
            if (isScanning)
            {
                AppManager.Instance.CalculateScanningProgressValue(out float curValue, out float maxValue);
                scanProgress = maxValue > 0 ? curValue / maxValue : 0f;
            }
            
            if (_lastIsScanning != isScanning || (_lastScanProgress != scanProgress && isScanning))
            {
                _lastIsScanning = isScanning;
                _lastScanProgress = scanProgress;
                UpdateScanningState();
            }
        }

        private void UpdateCurrentApp()
        {
            if (_currentAppRow == null) return;

            _currentAppRow.Clear();
            var currentApp = AppManager.Instance.GetActivatedApp();
            if (currentApp != null)
            {
                CreateAppRow(_currentAppRow, currentApp, true);
            }
        }

        private void UpdateLocalApps()
        {
            if (_localAppsContainer == null) return;

            _localAppsContainer.Clear();
            var localApp = AppManager.Instance.GetLocalApp();
            if (localApp != null)
            {
                var localAppRow = new VisualElement();
                localAppRow.AddToClassList("app-row");
                CreateAppRow(localAppRow, localApp, false);
                _localAppsContainer.Add(localAppRow);
            }
        }

        private void UpdateRemoteApps()
        {
            if (_remoteAppsContainer == null) return;

            _remoteAppsContainer.Clear();
            foreach (var remoteApp in AppManager.Instance.remoteAppsByAddress.Values)
            {
                var remoteAppRow = new VisualElement();
                remoteAppRow.AddToClassList("app-row");
                CreateAppRow(remoteAppRow, remoteApp, false);
                _remoteAppsContainer.Add(remoteAppRow);
            }
        }

        private void CreateAppRow(VisualElement container_, AppAbstract app_, bool isCurrentApp_)
        {
            container_.Clear();

            // Device Name
            var deviceNameLabel = new Label(app_.systemInfoContext.DeviceName);
            deviceNameLabel.AddToClassList("device-name-cell");
            deviceNameLabel.style.width = _deviceNameWidth;
            if (isCurrentApp_)
                deviceNameLabel.AddToClassList("current-app");
            container_.Add(deviceNameLabel);

            // Product Name
            var productNameLabel = new Label(app_.systemInfoContext.ProductName);
            productNameLabel.AddToClassList("product-name-cell");
            productNameLabel.style.width = _productNameWidth;
            if (isCurrentApp_)
                productNameLabel.AddToClassList("current-app");
            container_.Add(productNameLabel);

            // IP Address
            var ipLabel = new Label(app_.IpAddressString);
            ipLabel.AddToClassList("ip-address-cell");
            ipLabel.style.width = _ipAddressWidth;
            if (isCurrentApp_)
                ipLabel.AddToClassList("current-app");
            container_.Add(ipLabel);

            // State
            var stateLabel = new Label(app_.AppConnectionStateType.ToString());
            stateLabel.AddToClassList("state-cell");
            stateLabel.style.width = _stateWidth;
            if (isCurrentApp_)
                stateLabel.AddToClassList("current-app");
            container_.Add(stateLabel);

            // Actions
            var actionsContainer = new VisualElement();
            actionsContainer.AddToClassList("actions-cell");
            actionsContainer.style.flexDirection = FlexDirection.Row;

            if (!isCurrentApp_)
            {
                // Connection controls
                if (app_.AppConnectionStateType == AppConnectionStateType.CONNECTING)
                {
                    var disconnectBtn = new Button(() => app_.Disconnect()) { text = "Disconnect" };
                    disconnectBtn.AddToClassList("console-button");
                    actionsContainer.Add(disconnectBtn);
                }
                else if (!app_.HasConnected())
                {
                    var connectBtn = new Button(() => app_.Connect()) { text = "Connect" };
                    connectBtn.AddToClassList("console-button");
                    actionsContainer.Add(connectBtn);
                }
                else if (!app_.IsActivated)
                {
                    var selectBtn = new Button(() => AppManager.Instance.ActivateApp(app_)) { text = "Select" };
                    selectBtn.AddToClassList("console-button");
                    actionsContainer.Add(selectBtn);
                }

                // Additional actions
                if (app_.HasConnected() || app_.IsLocalEditor)
                {
                    var showInfoBtn = new Button(() => ShowAppInfo(app_)) { text = "Info" };
                    showInfoBtn.AddToClassList("console-button");
                    actionsContainer.Add(showInfoBtn);
                }
                else
                {
                    var deleteBtn = new Button(() => AppManager.Instance.RemoveAppIfDisConnected(app_ as RemoteApp)) { text = "Delete" };
                    deleteBtn.AddToClassList("console-button");
                    actionsContainer.Add(deleteBtn);
                }
            }

            container_.Add(actionsContainer);
        }

        private void ShowAppInfo(AppAbstract app_)
        {
            _currentDisplayedApp = app_;
            ShowSystemInfoPanel();
            UpdateSystemInfoContent();
        }

        private void ShowSystemInfoPanel()
        {
            if (_systemInfoPanel != null)
            {
                _systemInfoPanel.RemoveFromClassList("hidden");
            }
        }

        private void HideSystemInfoPanel()
        {
            if (_systemInfoPanel != null)
            {
                _systemInfoPanel.AddToClassList("hidden");
                _currentDisplayedApp = null;
            }
        }

        private void UpdateSystemInfoContent()
        {
            if (_systemInfoContent == null || _currentDisplayedApp == null)
                return;

            _systemInfoContent.Clear();

            float memorySizeMb = (float)_currentDisplayedApp.profileInfoContext.UsedHeapSize / (1024f * 1024f);
            string fps = _currentDisplayedApp.profileInfoContext.FramePerSecond.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (!_currentDisplayedApp.IsPlaying()) { fps = "N/A"; }

            // Add system info items
            AddSystemInfoItem("Device Name", _currentDisplayedApp.systemInfoContext.DeviceName);
            AddSystemInfoItem("IP Address", _currentDisplayedApp.IpAddressString);
            AddSystemInfoItem("Runtime Platform", _currentDisplayedApp.systemInfoContext.RuntimePlatform.ToString());
            AddSystemInfoItem("Device Model", _currentDisplayedApp.systemInfoContext.DeviceModel);
            AddSystemInfoItem("Operating System", _currentDisplayedApp.systemInfoContext.OperatingSystem);
            AddSystemInfoItem("Processor Count", _currentDisplayedApp.systemInfoContext.ProcessorCount.ToString());
            AddSystemInfoItem("System Memory Size", _currentDisplayedApp.systemInfoContext.SystemMemorySize.ToString() + " MB");
            AddSystemInfoItem("Graphics Device Name", _currentDisplayedApp.systemInfoContext.GraphicsDeviceName);
            AddSystemInfoItem("Graphics Device Type", _currentDisplayedApp.systemInfoContext.GraphicsDeviceType.ToString());
            AddSystemInfoItem("Graphics Memory Size", _currentDisplayedApp.systemInfoContext.GraphicsMemorySize.ToString() + " MB");
            AddSystemInfoItem("Max Texture Size", _currentDisplayedApp.systemInfoContext.MaxTextureSize.ToString());
            AddSystemInfoItem("Is Development Build", _currentDisplayedApp.systemInfoContext.IsDevelopmentBuild.ToString());
            AddSystemInfoItem("Frame Per Second", fps);
            AddSystemInfoItem("Used Heap Size", memorySizeMb.ToString("F2") + " MB");
        }

        private void AddSystemInfoItem(string label, string value)
        {
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("system-info-item");

            var labelElement = new Label(label);
            labelElement.AddToClassList("system-info-label");
            itemContainer.Add(labelElement);

            var valueElement = new Label(FormatSystemInfoValue(value));
            valueElement.AddToClassList("system-info-value");
            
            if (string.IsNullOrEmpty(value) || value == "Unknown")
            {
                valueElement.AddToClassList("unknown");
                valueElement.text = "Unknown";
            }

            itemContainer.Add(valueElement);
            _systemInfoContent.Add(itemContainer);
        }

        private string FormatSystemInfoValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "Unknown";

            return value;
        }

        private void UpdateScanningState()
        {
            if (_scanningControls == null || _scanningProgress == null) return;

            bool isScanning = AppManager.Instance.IsScanning();
            
            _scanningControls.style.display = isScanning ? DisplayStyle.None : DisplayStyle.Flex;
            _scanningProgress.style.display = isScanning ? DisplayStyle.Flex : DisplayStyle.None;

            if (isScanning && _scanningLabel != null)
            {
                AppManager.Instance.CalculateScanningProgressValue(out float curValue, out float maxValue);
                _scanningLabel.text = $"Looking for remote apps...({curValue:N0}/{maxValue:N0})";
            }

            // Update scan button text
            if (_scanNetworkButton != null)
            {
                string startAddress = SConsoleNetworkUtil.GetLocalIpStartAddress();
                string endAddress = SConsoleNetworkUtil.GetLocalIpEndAddress();
                _scanNetworkButton.text = $"Scan Local Network [{startAddress} - {endAddress}]";
            }
        }

        public VisualElement GetRootElement()
        {
            return _rootElement;
        }

        protected override void OnTerminate()
        {
            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
        }
    }
}