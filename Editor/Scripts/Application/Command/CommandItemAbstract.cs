// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;

namespace SPlugin
{
    internal abstract class CommandItemAbstract
    {
        protected const int DEFAULT_DISPLAY_PRIORITY = int.MaxValue;

        private string _id;
        private readonly string _category;
        private readonly string _name;
        private readonly string _toolTip;
        private readonly int _displayPriority = int.MaxValue;
        protected bool isDirty = false;
        protected readonly GUIContent guiContent = new GUIContent();
        protected readonly string guiControlName;

        public abstract CommandType CommandType { get; }
        public abstract string ValueString { get; }
        public string Category => _category;
        public string Name => _name;
        public string ToolTip => _toolTip;
        public int DisplayPriority => _displayPriority;
        public bool IsDirty => isDirty;

        internal CommandItemAbstract(string category_, string name_, int displayPriority_, string tooltip_)
        {
            _name = name_;
            _displayPriority = displayPriority_;
            _category = category_;
            _toolTip = tooltip_;
            guiContent.text = _name;
            guiContent.tooltip = _toolTip;
            guiControlName = category_ + name_;
        }

        public abstract void OnGui(float commandNameWidth_);

        public void OnSendCompleted()
        {
            isDirty = false;
        }
    }
}