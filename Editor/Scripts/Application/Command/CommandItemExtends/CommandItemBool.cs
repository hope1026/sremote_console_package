// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine.UIElements;

namespace SPlugin
{
    internal class CommandItemBool : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.BOOL;
        public override string ValueString => _value.ToString();
        private bool _value = false;

        public bool Value => _value;

        internal CommandItemBool(string category_, string name_, bool value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
        }

        public void ChangeValue(bool newValue_)
        {
            if (newValue_ != _value)
            {
                _value = newValue_;
                isDirty = true;
            }
        }

        public override VisualElement CreateUIToolkitControl()
        {
            var toggle = new Toggle();
            toggle.value = _value;
            if (!string.IsNullOrEmpty(ToolTip))
            {
                toggle.tooltip = ToolTip;
            }
            return toggle;
        }

        public override void BindUIToolkitEvents(VisualElement control_)
        {
            if (control_ is Toggle toggle)
            {
                toggle.RegisterValueChangedCallback(evt_ => ChangeValue(evt_.newValue));
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is Toggle toggle)
            {
                toggle.SetValueWithoutNotify(_value);
            }
        }
    }
}