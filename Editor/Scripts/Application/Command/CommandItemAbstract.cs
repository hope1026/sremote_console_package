// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine.UIElements;

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

        public abstract CommandType CommandType { get; }
        public abstract string ValueString { get; }
        public string Category => _category;
        public string CommandName => _name;
        public string ToolTip => _toolTip;
        public int DisplayPriority => _displayPriority;
        public bool IsDirty => isDirty;

        internal CommandItemAbstract(string category_, string name_, int displayPriority_, string tooltip_)
        {
            _name = name_;
            _displayPriority = displayPriority_;
            _category = category_;
            _toolTip = tooltip_;
        }

        // UIToolkit support
        public abstract VisualElement CreateUIToolkitControl();
        public abstract void BindUIToolkitEvents(VisualElement control_);
        public abstract void UpdateUIToolkitValue(VisualElement control_);

        public void OnSendCompleted()
        {
            isDirty = false;
        }
    }
}