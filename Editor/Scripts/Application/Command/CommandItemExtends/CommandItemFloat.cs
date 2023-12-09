// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class CommandItemFloat : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.FLOAT;
        public override string ValueString => _value.ToString(CultureInfo.InvariantCulture);
        private float _value = 0;
        private float _displayValue;

        internal CommandItemFloat(string category_, string name_, float value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
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
            _displayValue = EditorGUILayout.FloatField(_displayValue, GUILayout.ExpandWidth(expand: true));
            if (float.Epsilon < Math.Abs(_displayValue - _value))
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