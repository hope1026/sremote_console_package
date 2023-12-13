using Sample.Scripts;
using SPlugin;
using UnityEngine;
using UnityEngine.UI;

public class SRemoteConsoleSampleMain : MonoBehaviour
{
    [SerializeField] private Text _moneyTest;
    [SerializeField] private SampleMoveComponent _moveComponent;

    private void Start()
    {
        ChangeMoney(0);
        SCommand.Register("CheatCategory", "MoveEnable", defaultValue_: true, OnChangedMoveEnableValueHandler, displayPriority_: 10, tooltip_: "You can change movement state.");
        SCommand.Register("CheatCategory", "ChangeMoney", defaultValue_: 10, OnChangedMoneyValueHandler, displayPriority_: 10, tooltip_: "You can change money.");
        SCommand.Register("DefaultCategory", "LongName", (long)100, OnChangedLongValueHandler, displayPriority_: 1, tooltip_: "The long");
        SCommand.Register("DefaultCategory", "FloatName", defaultValue_: 10.5f, OnChangedFloatValueHandler, displayPriority_: 1000, tooltip_: "The float");
        SCommand.Register("DefaultCategory", "DoubleName", defaultValue_: 100.5d, OnChangedDoubleValueHandler, displayPriority_: 100, tooltip_: "The double");
        SCommand.Register("DefaultCategory", "StringName", defaultValue_: "string value", OnChangedStringValueHandler, displayPriority_: 3, tooltip_: "The string");
    }

    void OnGUI()
    {
        GUI.skin.button.fontSize = 35;
        GUILayout.BeginVertical();
        GUILayout.Space(100f);
        GUILayoutOption width = GUILayout.Width(350f);
        GUILayoutOption height = GUILayout.Height(150f);

        if (true == GUILayout.Button("Write Log", GUI.skin.button, width, height))
            SDebug.Log("SPlugin.SDebug.Log function was called.", this);

        GUILayout.Space(20f);
        if (true == GUILayout.Button("Write Warning", GUI.skin.button, width, height))
            SDebug.LogWarning("SPlugin.SDebug.LogWarning function was called.", this);

        GUILayout.Space(20f);
        if (true == GUILayout.Button("Write Error", GUI.skin.button, width, height))
            SDebug.LogError("SPlugin.SDebug.LogError function was called.", this);

        GUILayout.Space(20f);
        if (true == GUILayout.Button("Write UnityLog", GUI.skin.button, width, height))
            Debug.Log("UnityEngine.Debug.Log function was called.");

        GUILayout.EndVertical();
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