// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal static class SGuiStyle
    {
        private static GUIStyle _labelStyle = null;
        private static GUIStyle _boxStyle = null;
        private static GUIStyle _lineStyle = null;
        private static GUIStyle _foldOutStyle = null;
        private static GUIStyle _buttonStyle = null;
        private static GUIStyle _menuTapNormalStyle = null;
        private static GUIStyle _menuTapSelectedStyle = null;
        private static GUIStyle _horizontalScrollbarThumbStyle = null;
        private static GUIStyle _textAreaStyle = null;
        private static GUIStyle _toggleStyle = null;
        private static GUIStyle _numberFieldStyle = null;
        private static GUIStyle _activeAppLabelStyle = null;
        private static GUIStyle _appListLabelStyle = null;
        private static GUIStyle _connectedAppAreaStyle = null;
        private static GUIStyle _disConnectedAppAreaStyle = null;

        public static GUIStyle LabelStyle { get { return InstanceStyle(ref _labelStyle, GUI.skin.label); } }
        public static GUIStyle ActiveAppLabelStyle { get { return InstanceStyle(ref _activeAppLabelStyle, GUI.skin.label); } }
        public static GUIStyle AppListLabelStyle { get { return InstanceStyle(ref _appListLabelStyle, GUI.skin.label); } }
        public static GUIStyle BoxStyle { get { return InstanceStyle(ref _boxStyle, GUI.skin.box); } }
        public static GUIStyle LineStyle { get { return InstanceStyle(ref _lineStyle, GUI.skin.box); } }
        public static GUIStyle FoldOutStyle { get { return InstanceStyle(ref _foldOutStyle, GUI.skin.GetStyle("FoldOut")); } }
        public static GUIStyle ButtonStyle { get { return InstanceStyle(ref _buttonStyle, GUI.skin.button); } }
        public static GUIStyle MenuTapNormalStyle { get { return InstanceStyle(ref _menuTapNormalStyle, GUI.skin.box); } }
        public static GUIStyle MenuTapSelectedStyle { get { return InstanceStyle(ref _menuTapSelectedStyle, GUI.skin.button); } }
        public static GUIStyle HorizontalScrollbarThumbStyle { get { return InstanceStyle(ref _horizontalScrollbarThumbStyle, GUI.skin.horizontalScrollbarThumb); } }
        public static GUIStyle TextAreaStyle { get { return InstanceStyle(ref _textAreaStyle, GUI.skin.textArea); } }
        public static GUIStyle ConnectedAppAreaStyle { get { return InstanceStyle(ref _connectedAppAreaStyle, GUI.skin.textArea); } }
        public static GUIStyle DisConnectedAppAreaStyle { get { return InstanceStyle(ref _disConnectedAppAreaStyle, GUI.skin.textArea); } }
        public static GUIStyle ToggleStyle { get { return InstanceStyle(ref _toggleStyle, GUI.skin.toggle); } }
        public static GUIStyle NumberFieldStyle { get { return InstanceStyle(ref _numberFieldStyle, EditorStyles.numberField); } }
        public static bool RequestUpdateColors { get; set; }

        public static void UpdateColor()
        {
            ChangeStyleTextColor(LabelStyle, ConsoleEditorPrefs.TextColor);
            ChangeStyleTextColor(BoxStyle, ConsoleEditorPrefs.TextColor);
            ChangeStyleTextColor(ButtonStyle, ConsoleEditorPrefs.TextColor);
            ChangeStyleTextColor(TextAreaStyle, ConsoleEditorPrefs.TextColor);
            ChangeStyleTextColor(ToggleStyle, ConsoleEditorPrefs.TextColor);
            ChangeStyleTextColor(FoldOutStyle, ConsoleEditorPrefs.TextColor);
        }

        private static GUIStyle InstanceStyle(ref GUIStyle instanceStyle_, GUIStyle destStyle_)
        {
            if (null != instanceStyle_) return instanceStyle_;

            instanceStyle_ = new GUIStyle(destStyle_);
            instanceStyle_.richText = true;
            return instanceStyle_;
        }

        private static void ChangeStyleTextColor(GUIStyle style_, Color textColor_)
        {
            if (null != style_)
            {
                style_.normal.textColor = textColor_;
            }
        }
    }
}