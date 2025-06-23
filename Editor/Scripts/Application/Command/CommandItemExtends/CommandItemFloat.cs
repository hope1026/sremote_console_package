// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin
{
    internal class CommandItemFloat : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.FLOAT;
        public override string ValueString => _value.ToString(CultureInfo.InvariantCulture);
        private float _value = 0;
        private float _displayValue;

        public float Value => _value;

        internal CommandItemFloat(string category_, string name_, float value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public void ChangeValue(float newValue_)
        {
            if (Math.Abs(newValue_ - _value) > float.Epsilon)
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

            var floatField = new FloatField();
            floatField.value = _displayValue;
            floatField.style.flexGrow = 1;
            if (!string.IsNullOrEmpty(ToolTip))
            {
                floatField.tooltip = ToolTip;
            }
            container.Add(floatField);

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
                var floatField = container.Q<FloatField>();
                var applyButton = container.Q<Button>();
                
                if (floatField != null && applyButton != null)
                {
                    floatField.RegisterValueChangedCallback(evt_ => {
                        _displayValue = evt_.newValue;
                        UpdateApplyButtonVisibility(container, floatField, applyButton);
                    });

                    floatField.RegisterCallback<KeyDownEvent>(evt_ => {
                        if (evt_.keyCode == KeyCode.Return || evt_.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeValue(floatField.value);
                            UpdateApplyButtonVisibility(container, floatField, applyButton);
                        }
                    });

                    // Bind Apply button click event
                    applyButton.RegisterCallback<ClickEvent>(_ => {
                        Debug.Log($"Apply button clicked for command: {CommandName}");
                        ChangeValue(floatField.value);
                        UpdateApplyButtonVisibility(container, floatField, applyButton);
                    });
                }
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var floatField = container.Q<FloatField>();
                var applyButton = container.Q<Button>();
                
                if (floatField != null)
                {
                    floatField.SetValueWithoutNotify(_displayValue);
                    if (applyButton != null)
                    {
                        UpdateApplyButtonVisibility(container, floatField, applyButton);
                    }
                }
            }
        }

        private void UpdateApplyButtonVisibility(VisualElement container_, FloatField floatField_, Button applyButton_)
        {
            bool isDifferent = Math.Abs(floatField_.value - _value) > float.Epsilon;
            applyButton_.style.display = isDifferent ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}