// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class CommandItemLong : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.LONG;
        public override string ValueString => _value.ToString();
        private long _value = 0;
        private long _displayValue;

        internal CommandItemLong(string category_, string name_, long value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public override void OnGui(float commandNameWidth_)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(commandNameWidth_));
            GUI.SetNextControlName(base.guiControlName);
            _displayValue = EditorGUILayout.LongField(_displayValue, GUILayout.ExpandWidth(expand: true));
            if (_displayValue != _value)
            {
                bool canApply = GUILayout.Button("Apply");
                if (canApply || (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl().Equals(base.guiControlName)))
                {
                    _value = _displayValue;
                    isDirty = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}