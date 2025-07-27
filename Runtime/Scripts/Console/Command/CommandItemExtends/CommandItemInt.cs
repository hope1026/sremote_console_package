// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class CommandItemInt : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.INT;
        public override string ValueString => _value.ToString();
        private int _value = 0;
        private int _displayValue;

        public int Value => _value;

        internal CommandItemInt(string category_, string name_, int value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public void ChangeValue(int newValue_)
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
            container.AddToClassList("command-control-container");

            var intField = new IntegerField();
            intField.value = _displayValue;
            intField.AddToClassList("command-input-field");
            UIResourceHelper.ApplyDefaultFont(intField);
            
            // Apply default font - CSS handles styling
            intField.schedule.Execute(() => {
                // Try multiple possible input element class names
                var textInput = intField.Q(className: "unity-text-input") ?? 
                               intField.Q(className: "unity-base-text-field__input") ?? 
                               intField.Q(className: "unity-text-field__input");
                if (textInput != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textInput);
                }
                
                // Also apply to text element if it exists
                var textElement = intField.Q(className: "unity-text-element");
                if (textElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textElement);
                }
            });
            
            if (!string.IsNullOrEmpty(ToolTip))
            {
                intField.tooltip = ToolTip;
            }
            container.Add(intField);

            var applyButton = CreateApplyButton();
            container.Add(applyButton);

            return container;
        }

        public override void BindUIToolkitEvents(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var intField = container.Q<IntegerField>();
                var applyButton = container.Q<Button>();
                
                if (intField != null && applyButton != null)
                {
                    intField.RegisterValueChangedCallback(evt_ => {
                        _displayValue = evt_.newValue;
                        UpdateApplyButtonVisibility(container, intField, applyButton);
                    });

                    intField.RegisterCallback<KeyDownEvent>(evt_ => {
                        if (evt_.keyCode == KeyCode.Return || evt_.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeValue(intField.value);
                            UpdateApplyButtonVisibility(container, intField, applyButton);
                        }
                    });

                    // Bind Apply button click event
                    applyButton.RegisterCallback<ClickEvent>(_ => {
                        Debug.Log($"Apply button clicked for command: {CommandName}");
                        ChangeValue(intField.value);
                        UpdateApplyButtonVisibility(container, intField, applyButton);
                    });
                }
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var intField = container.Q<IntegerField>();
                var applyButton = container.Q<Button>();
                
                if (intField != null)
                {
                    intField.SetValueWithoutNotify(_displayValue);
                    if (applyButton != null)
                    {
                        UpdateApplyButtonVisibility(container, intField, applyButton);
                    }
                }
            }
        }

        private void UpdateApplyButtonVisibility(VisualElement container_, IntegerField intField_, Button applyButton_)
        {
            bool isDifferent = intField_.value != _value;
            applyButton_.style.display = isDifferent ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}