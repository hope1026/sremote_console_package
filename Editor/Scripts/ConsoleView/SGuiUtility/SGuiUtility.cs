// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace SPlugin
{
    internal static class SGuiUtility
    {
        private static bool _beginBackgroundColor = false;
        private static Color _oldBackgroundColor = new Color();

        public static void OnGuiLine(params GUILayoutOption[] layoutOption_)
        {
            Texture2D oldTexture = SGuiStyle.LineStyle.normal.background;
            SGuiStyle.LineStyle.normal.background = Texture2D.whiteTexture;

            SGuiUtility.BeginBackgroundColor(Color.black);
            GUILayout.Box("", SGuiStyle.LineStyle, layoutOption_);
            SGuiUtility.EndBackgroundColor();
            SGuiStyle.LineStyle.normal.background = oldTexture;
        }

        public static void BeginBackgroundColor(Color color_)
        {
            if (false == _beginBackgroundColor)
            {
                _oldBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = color_;
                _beginBackgroundColor = true;
            }
            else
            {
                Debug.LogError("You have to call EndBackgroundColor before BeginBackgroundColor.");
            }
        }

        public static void EndBackgroundColor()
        {
            if (true == _beginBackgroundColor)
            {
                GUI.backgroundColor = _oldBackgroundColor;
                _beginBackgroundColor = false;
            }
            else
            {
                Debug.LogError("You have to call BeginBackgroundColor before EndBackgroundColor.");
            }
        }

        public static void SetTextEditorCursorPos(int pos_)
        {
            TextEditor editor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
            if (null == editor) return;

            editor.cursorIndex = editor.selectIndex = pos_;
        }

        public static void OnGuiCheckTextFieldCopyAndPaste(ref string refString_)
        {
            if (Event.current.type == EventType.KeyUp && (true == Event.current.command || true == Event.current.control))
            {
                TextEditor editor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
                if (null == editor) return;

                switch (Event.current.keyCode)
                {
                    case KeyCode.V:
                    {
                        if (true == editor.hasSelection)
                        {
                            int startPos = Mathf.Min(editor.cursorIndex, editor.selectIndex);
                            refString_ = refString_.Remove(startPos, editor.SelectedText.Length);
                            refString_ = refString_.Insert(startPos, EditorGUIUtility.systemCopyBuffer);
                        }
                        else
                        {
                            refString_ = refString_.Insert(editor.selectIndex, EditorGUIUtility.systemCopyBuffer);
                        }

                        editor.DeleteSelection();
                    }
                        break;
                    case KeyCode.C:
                    {
                        EditorGUIUtility.systemCopyBuffer = editor.SelectedText;
                    }
                        break;
                    case KeyCode.A:
                    {
                        editor.SelectAll();
                    }
                        break;
                }
            }
        }

        public static string ReplaceColorString(string srcString_, string colorString_)
        {
            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FONT_EFFECT))
            {
                return $"<color=\"#{colorString_}\">{srcString_}</color>";
            }

            return srcString_;
        }

        public static string ReplaceColorString(string srcString_, Color32 color_)
        {
            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FONT_EFFECT))
            {
                return $"<color=\"#{color_.r:X2}{color_.g:X2}{color_.b:X2}{color_.a:X2}\">{srcString_}</color>";
            }

            return srcString_;
        }

        public static string ReplaceBoldString(string srcString_)
        {
            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FONT_EFFECT))
            {
                return $"<b>{srcString_}</b>";
            }

            return srcString_;
        }

        public static GUIContent GetWinTitleContent(EditorWindow editor_)
        {
            const BindingFlags BIND_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
            PropertyInfo propertyInfo = typeof(EditorWindow).GetProperty("cachedTitleContent", BIND_FLAGS);
            if (propertyInfo == null) return null;
            return propertyInfo.GetValue(editor_, null) as GUIContent;
        }

        public static bool GetStatusBarWindowRect(ref Rect statusBarWindowRect_)
        {
            try
            {
                Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                if (null != unityEditorAssembly)
                {
                    Type appStatusBarType = unityEditorAssembly.GetType("UnityEditor.AppStatusBar");
                    Debug.Log(appStatusBarType);
                    if (null != appStatusBarType)
                    {
                        FieldInfo staticFieldInfo = appStatusBarType.GetField("s_AppStatusBar", BindingFlags.Static | BindingFlags.NonPublic);
                        if (null != staticFieldInfo)
                        {
                            Debug.Log("staticFieldInfo:" + staticFieldInfo);
                            Object staticInstance = staticFieldInfo.GetValue(null);
                            if (null != staticInstance)
                            {
                                Debug.Log("staticInstance:" + staticFieldInfo);

                                PropertyInfo windowPositionMemberInfo = appStatusBarType.GetProperty("windowPosition");
                                Debug.Log("windowPosition:" + windowPositionMemberInfo);
                                if (windowPositionMemberInfo != null)
                                {
                                    statusBarWindowRect_ = (Rect)windowPositionMemberInfo.GetValue(staticInstance, null);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return false;
        }
    }
}