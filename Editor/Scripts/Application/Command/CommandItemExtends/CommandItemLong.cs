// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin
{
    internal class CommandItemLong : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.LONG;
        public override string ValueString => _value.ToString();
        private long _value = 0;
        private long _displayValue;

        public long Value => _value;

        internal CommandItemLong(string category_, string name_, long value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public void ChangeValue(long newValue_)
        {
            if (newValue_ != _value)
            {
                _value = newValue_;
                _displayValue = newValue_;
                isDirty = true;
            }
        }

        public override VisualElement CreateUIToolkitControl()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            var longField = new LongField();
            longField.value = _displayValue;
            longField.style.flexGrow = 1;
            if (!string.IsNullOrEmpty(ToolTip))
            {
                longField.tooltip = ToolTip;
            }
            container.Add(longField);

            var applyButton = new Button() { text = "Apply" };
            applyButton.AddToClassList("console-button");
            applyButton.AddToClassList("command-apply-button");
            applyButton.style.display = DisplayStyle.None;
            container.Add(applyButton);

            return container;
        }

        public override void BindUIToolkitEvents(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var longField = container.Q<LongField>();
                var applyButton = container.Q<Button>();
                
                if (longField != null && applyButton != null)
                {
                    longField.RegisterValueChangedCallback(evt_ => {
                        _displayValue = evt_.newValue;
                        UpdateApplyButtonVisibility(container, longField, applyButton);
                    });

                    longField.RegisterCallback<KeyDownEvent>(evt_ => {
                        if (evt_.keyCode == KeyCode.Return || evt_.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeValue(longField.value);
                            UpdateApplyButtonVisibility(container, longField, applyButton);
                        }
                    });

                    // Bind Apply button click event
                    applyButton.RegisterCallback<ClickEvent>(_ => {
                        Debug.Log($"Apply button clicked for command: {CommandName}");
                        ChangeValue(longField.value);
                        UpdateApplyButtonVisibility(container, longField, applyButton);
                    });
                }
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var longField = container.Q<LongField>();
                var applyButton = container.Q<Button>();
                
                if (longField != null)
                {
                    longField.SetValueWithoutNotify(_displayValue);
                    if (applyButton != null)
                    {
                        UpdateApplyButtonVisibility(container, longField, applyButton);
                    }
                }
            }
        }

        private void UpdateApplyButtonVisibility(VisualElement container_, LongField longField_, Button applyButton_)
        {
            bool isDifferent = longField_.value != _value;
            applyButton_.style.display = isDifferent ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}