using Sample.Scripts;
using SPlugin;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SRemoteConsoleSampleMain : MonoBehaviour
{
    [SerializeField] private Text _moneyTest;
    [SerializeField] private SampleMoveComponent _moveComponent;
    
    private UIDocument _uiDocument;
    private VisualElement _root;

    private void Start()
    {
        ChangeMoney(0);
        SCommand.Register("CheatCategory", "MoveEnable", defaultValue_: true, OnChangedMoveEnableValueHandler, displayPriority_: 10, tooltip_: "You can change movement state.");
        SCommand.Register("CheatCategory", "ChangeMoney", defaultValue_: 10, OnChangedMoneyValueHandler, displayPriority_: 10, tooltip_: "You can change money.");
        SCommand.Register("DefaultCategory", "LongName", (long)100, OnChangedLongValueHandler, displayPriority_: 1, tooltip_: "The long");
        SCommand.Register("DefaultCategory", "FloatName", defaultValue_: 10.5f, OnChangedFloatValueHandler, displayPriority_: 1000, tooltip_: "The float");
        SCommand.Register("DefaultCategory", "DoubleName", defaultValue_: 100.5d, OnChangedDoubleValueHandler, displayPriority_: 100, tooltip_: "The double");
        SCommand.Register("DefaultCategory", "StringName", defaultValue_: "string value", OnChangedStringValueHandler, displayPriority_: 3, tooltip_: "The string");
        
        SetupUIToolkit();
    }

    void SetupUIToolkit()
    {
        _uiDocument = gameObject.AddComponent<UIDocument>();
        
        // Create runtime panel settings
        PanelSettings panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.scale = 1.0f;
        panelSettings.referenceResolution = new Vector2Int(1200, 800);
        panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        panelSettings.match = 0.5f;
        panelSettings.sortingOrder = 10;
        _uiDocument.panelSettings = panelSettings;
        
        _root = new VisualElement();
        _uiDocument.rootVisualElement.Add(_root);
        
        // Configure root container with vertical layout
        _root.style.flexDirection = FlexDirection.Column;
        _root.style.alignItems = Align.Center;
        _root.style.justifyContent = Justify.Center;
        _root.style.width = Length.Percent(100);
        _root.style.height = Length.Percent(100);
        _root.style.paddingTop = 100;
        
        // Create buttons with styling
        UnityEngine.UIElements.Button logButton = CreateStyledButton("Write Log");
        logButton.clicked += () => SDebug.Log("SPlugin.SDebug.Log function was called.", this);
        
        UnityEngine.UIElements.Button warningButton = CreateStyledButton("Write Warning");
        warningButton.clicked += () => SDebug.LogWarning("SPlugin.SDebug.LogWarning function was called.", this);
        
        UnityEngine.UIElements.Button errorButton = CreateStyledButton("Write Error");
        errorButton.clicked += () => SDebug.LogError("SPlugin.SDebug.LogError function was called.", this);
        
        UnityEngine.UIElements.Button unityLogButton = CreateStyledButton("Write UnityLog");
        unityLogButton.clicked += () => Debug.Log("UnityEngine.Debug.Log function was called.");
        
        UnityEngine.UIElements.Button openConsoleButton = CreateStyledButton("Open Console");
        openConsoleButton.clicked += () => SDebug.OpenConsole(11);
        
        // Add buttons to root with spacing
        _root.Add(logButton);
        _root.Add(CreateSpacer(20));
        _root.Add(warningButton);
        _root.Add(CreateSpacer(20));
        _root.Add(errorButton);
        _root.Add(CreateSpacer(20));
        _root.Add(unityLogButton);
        _root.Add(CreateSpacer(20));
        _root.Add(openConsoleButton);
    }
    
    UnityEngine.UIElements.Button CreateStyledButton(string text_)
    {
        UnityEngine.UIElements.Button button = new UnityEngine.UIElements.Button(() => {});
        button.text = text_;
        
        // Apply font from Resources - same pattern as SRemoteConsole UI
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont != null)
        {
            button.style.unityFont = new StyleFont(defaultFont);
        }
        
        // Apply styles matching OnGUI version
        button.style.fontSize = 35;
        button.style.width = 350;
        button.style.height = 150;
        button.style.backgroundColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 0.5f));
        button.style.color = new StyleColor(Color.black);
        button.style.borderTopWidth = 1;
        button.style.borderBottomWidth = 1;
        button.style.borderLeftWidth = 1;
        button.style.borderRightWidth = 1;
        button.style.borderTopColor = new StyleColor(Color.gray);
        button.style.borderBottomColor = new StyleColor(Color.gray);
        button.style.borderLeftColor = new StyleColor(Color.gray);
        button.style.borderRightColor = new StyleColor(Color.gray);
        button.style.borderTopLeftRadius = 4;
        button.style.borderTopRightRadius = 4;
        button.style.borderBottomLeftRadius = 4;
        button.style.borderBottomRightRadius = 4;
        button.style.unityTextAlign = TextAnchor.MiddleCenter;
        button.style.whiteSpace = WhiteSpace.Normal;
        
        // Ensure text is visible with proper contrast
        button.style.unityFontStyleAndWeight = FontStyle.Normal;
        
        return button;
    }
    
    VisualElement CreateSpacer(float height_)
    {
        VisualElement spacer = new VisualElement();
        spacer.style.height = height_;
        return spacer;
    }

    void ChangeMoney(int money_)
    {
        if (_moneyTest != null)
        {
            _moneyTest.text = money_.ToString();
        }
    }

    void OnChangedMoneyValueHandler(int value_)
    {
        ChangeMoney(value_);
    }

    void OnChangedMoveEnableValueHandler(bool value_)
    {
        if (_moveComponent != null)
        {
            _moveComponent.MoveEnabled = value_;
        }
    }

    void OnChangedLongValueHandler(long value_) { }
    void OnChangedFloatValueHandler(float value_) { }
    void OnChangedDoubleValueHandler(double value_) { }
    void OnChangedStringValueHandler(string value_) { }
}