// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class CommandItemBool : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.BOOL;
        public override string ValueString => _value.ToString();
        private bool _value = false;

        internal CommandItemBool(string category_, string name_, bool value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
        }

        public override void OnGui(float commandNameWidth_)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(commandNameWidth_));
            GUI.SetNextControlName(base.guiControlName);
            bool newValue = EditorGUILayout.Toggle(_value, GUILayout.ExpandWidth(expand: true));
            if (newValue != _value)
            {
                _value = newValue;
                isDirty = true;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}