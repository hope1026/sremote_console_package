// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace SPlugin
{
    internal static class ColorUtility
    {
        public static string ReplaceColorString(string srcString_, Color32 color_)
        {
            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FONT_EFFECT))
            {
                return $"<color=\"#{color_.r:X2}{color_.g:X2}{color_.b:X2}{color_.a:X2}\">{srcString_}</color>";
            }

            return srcString_;
        }
        
    }
}