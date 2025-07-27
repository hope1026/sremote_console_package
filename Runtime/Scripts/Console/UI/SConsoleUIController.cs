// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    /// <summary>
    /// Runtime Console UI Controller - Manages UIToolkit-based console interface
    /// </summary>
    public class SConsoleUIController : MonoBehaviour
    {
        // Instance fields
        [Header("UI Settings")] [SerializeField] private bool _createUIAutomatically = true;
        [SerializeField] private string _toggleButtonLabel = "Console";

        private SConsoleRuntimeMain _ownerConsoleRuntimeMain;
        private bool _isInitialized = false;
        private UIDocument _uiDocument;
        private VisualElement _rootElement;
        private ConsoleMenu _consoleMenu;

        // View management
        private readonly ConsoleViewAbstract[] _consoleViews = new ConsoleViewAbstract[ConsoleViewTypeUtil.COUNT];
        private ConsoleViewType _currentConsoleViewType = ConsoleViewType.LOG;
        private VisualElement _uiToolkitContainer;
        private VisualElement _currentUIToolkitView;
        
        internal ConsoleViewType CurrentConsoleViewType => _currentConsoleViewType;
        internal SConsoleRuntimeMain OwnerConsoleRuntimeMain => _ownerConsoleRuntimeMain;

        // Methods

        public void OnOpenConsole(SConsoleRuntimeMain ownerConsoleRuntimeMain_, int sortingOrder_ = 1000)
        {
            _ownerConsoleRuntimeMain = ownerConsoleRuntimeMain_;

            if (_createUIAutomatically)
            {
                CreateConsoleUI(sortingOrder_);
            }

            CreateViews();

            if (_rootElement != null)
            {
                _rootElement.style.display = DisplayStyle.Flex;
            }
        }

        public void OnCloseConsole()
        {
            if (_consoleViews != null)
            {
                for (int i = 0; i < _consoleViews.Length; i++)
                {
                    _consoleViews[i]?.Terminate();
                    _consoleViews[i] = null;
                }
            }

            if (_consoleMenu != null)
            {
                _consoleMenu.Terminate();
                _consoleMenu = null;
            }

            _rootElement = null;

            if (_uiDocument != null)
            {
                GameObject.Destroy(_uiDocument.gameObject);
                _uiDocument = null;    
            }
            
        }

        private void CreateConsoleUI(int sortingOrder_)
        {
            CreateUIDocument(sortingOrder_);
            CreateSelectMenu();
        }

        private void CreateUIDocument(int sortingOrder_)
        {
            if (_uiDocument != null) return;
            
            GameObject uiDocumentObject = new GameObject("SConsoleUIDocument");
            uiDocumentObject.transform.SetParent(transform);

            _uiDocument = uiDocumentObject.AddComponent<UIDocument>();

            // Load pre-configured PanelSettings asset with theme already set
            PanelSettings panelSettings = Resources.Load<PanelSettings>("UI/SConsoleRuntimePanelSettings");
            
            if (panelSettings == null)
            {
                // Fallback: Create PanelSettings if asset not found
                Debug.LogWarning("SConsoleRuntimePanelSettings.asset not found, creating default settings");
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.targetTexture = null;
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.fallbackDpi = 96f;
                panelSettings.referenceDpi = 96f;
                panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;
            }

            // Override runtime-specific settings while preserving theme and other asset settings
            panelSettings.scale = 0.8f; // Scale down to 80% for better fit on various screen sizes
            
            // Use actual screen resolution as reference resolution
            Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
            panelSettings.referenceResolution = currentScreenSize;
            
            panelSettings.sortingOrder = sortingOrder_;

            _uiDocument.panelSettings = panelSettings;

            // Configure the rootVisualElement to use full screen
            _uiDocument.rootVisualElement.AddToClassList("console-ui-root");

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseRuntimeStyles");
            var controllerStyles = Resources.Load<StyleSheet>("UI/SConsoleUIControllerRuntimeStyles");

            if (baseStyles != null)
            {
                _uiDocument.rootVisualElement.styleSheets.Add(baseStyles);
            }
            if (controllerStyles != null)
            {
                _uiDocument.rootVisualElement.styleSheets.Add(controllerStyles);
            }

            // Create root element programmatically
            _rootElement = new VisualElement();
            _rootElement.name = "console-root";
            _rootElement.pickingMode = PickingMode.Position;

            _uiDocument.rootVisualElement.Add(_rootElement);
        }

        private void CreateSelectMenu()
        {
            if (_rootElement == null)
            {
                Debug.LogWarning("Console panel is not created.");
                return;
            }

            _consoleMenu = new ConsoleMenu();
            _consoleMenu.Initialize(this, _rootElement);
        }

        private void CreateViews()
        {
            // OnOpenConsole all views
            _consoleViews[(int)ConsoleViewType.LOG] = new LogView();
            _consoleViews[(int)ConsoleViewType.COMMAND] = new CommandView();
            _consoleViews[(int)ConsoleViewType.PREFERENCES] = new PreferencesView();

            foreach (ConsoleViewAbstract consoleView in _consoleViews)
            {
                consoleView.Initialize(_rootElement, _ownerConsoleRuntimeMain.ConsoleApp);
                if (consoleView.ConsoleViewType == _currentConsoleViewType)
                {
                    consoleView.Show();
                }
                else
                {
                    consoleView.Hide();
                }
            }

            ShowView(_currentConsoleViewType);
        }

        internal void ShowView(ConsoleViewType showingConsoleViewType_)
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
                else if (consoleView.ConsoleViewType == _currentConsoleViewType)
                {
                    consoleView.Hide();
                }
            }

            _currentConsoleViewType = showingConsoleViewType_;
        }

        public void UpdateCustom()
        {
            CheckScreenSize();
            if (_consoleMenu != null)
            {
                _consoleMenu.UpdateCustom();
            }

            // Update current view
            _consoleViews[(int)_currentConsoleViewType]?.UpdateCustom();
        }

        private void CheckScreenSize()
        {
            if (_uiDocument != null && _uiDocument.panelSettings != null &&
                (_uiDocument.panelSettings.referenceResolution.x != Screen.width ||
                 _uiDocument.panelSettings.referenceResolution.y != Screen.height))
            {
                _uiDocument.panelSettings.referenceResolution = new Vector2Int(Screen.width, Screen.height);
            }
        }
    }
}