// 
// Copyright 2015 https://github.com/hope1026

using System.Globalization;
using System.Text;
using SPlugin;
using UnityEditor;
using UnityEngine;

internal class ApplicationSystemInfoEditorWindow : EditorWindow
{
    private AppAbstract _selectedAppRef = null;

    public void SetSelectedApp(AppAbstract app_)
    {
        _selectedAppRef = app_;
    }

    void OnGUI()
    {
        if (_selectedAppRef == null)
            return;

        GUILayout.BeginVertical();

        OnGuiSelectedAppInfo();

        GUILayout.EndVertical();
    }

    private void OnGuiSelectedAppInfo()
    {
        if (_selectedAppRef == null)
            return;

        GUILayoutOption layoutOptionWidth = GUILayout.ExpandWidth(expand: true);

        float memorySizeMb = (float)_selectedAppRef.profileInfoContext.UsedHeapSize / (1024f * 1024f);

        string fps = _selectedAppRef.profileInfoContext.FramePerSecond.ToString(CultureInfo.InvariantCulture);
        if (false == _selectedAppRef.IsPlaying()) { fps = string.Empty; }

        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("DeviceName"), _selectedAppRef.systemInfoContext.DeviceName, SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("IpAddress"), _selectedAppRef.IpAddressString, SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("RuntimePlatform"), _selectedAppRef.systemInfoContext.RuntimePlatform.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("DeviceModel"), _selectedAppRef.systemInfoContext.DeviceModel, SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("OperationSystem"), _selectedAppRef.systemInfoContext.OperatingSystem, SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("RuntimePlatform"), _selectedAppRef.systemInfoContext.RuntimePlatform.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("ProcessorCount"), _selectedAppRef.systemInfoContext.ProcessorCount.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("SystemMemorySize"), _selectedAppRef.systemInfoContext.SystemMemorySize.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("GraphicsDeviceName"), _selectedAppRef.systemInfoContext.GraphicsDeviceName, SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("GraphicsDeviceType"), _selectedAppRef.systemInfoContext.GraphicsDeviceType.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("GraphicsMemorySize"), _selectedAppRef.systemInfoContext.GraphicsMemorySize.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("MaxTextureSize"), _selectedAppRef.systemInfoContext.MaxTextureSize.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("IsDevelopmentBuild"), _selectedAppRef.systemInfoContext.IsDevelopmentBuild.ToString(), SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("FramePerSecond"), fps, SGuiStyle.BoxStyle, layoutOptionWidth);
        OnGuiSystemInfo(SGuiUtility.ReplaceBoldString("UsedHeapSize"), memorySizeMb + "MB", SGuiStyle.BoxStyle, layoutOptionWidth);
    }

    private void OnGuiSystemInfo(string typeName_, string value_, GUIStyle style_, GUILayoutOption layoutOption_)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(typeName_, style_, GUILayout.Width(200f));
        GUILayout.Label(AlignmentSystemInfoLabelString(value_), SGuiStyle.LabelStyle, layoutOption_);
        GUILayout.EndHorizontal();
    }

    private string AlignmentSystemInfoLabelString(string string_)
    {
        if (string.IsNullOrEmpty(string_))
            return "Unknown";

        int drawLineCharacterCount = (int)(ConsoleViewLayoutDefines.View.areaRect.width * 0.5f / 8);
        drawLineCharacterCount = Mathf.Max(drawLineCharacterCount, 10);

        if (string_.Length <= drawLineCharacterCount)
            return string_;

        StringBuilder builder = new StringBuilder();
        int clipStartIndex = 0;
        while (clipStartIndex < string_.Length)
        {
            drawLineCharacterCount = Mathf.Min(string_.Length - clipStartIndex, drawLineCharacterCount);
            builder.Append(string_.Substring(clipStartIndex, drawLineCharacterCount));
            builder.Append("\n");
            clipStartIndex += drawLineCharacterCount;
        }

        return builder.ToString();
    }
}