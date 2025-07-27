// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class CommandItemString : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.STRING;
        public override string ValueString => _value;
        private string _value;
        private string _displayValue;

        public string Value => _value;

        internal CommandItemString(string category_, string name_, string value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public void ChangeValue(string newValue_)
        {
            if (!string.Equals(newValue_, _value))
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

            var textField = UIResourceHelper.CreateTextField(_displayValue ?? string.Empty);
            textField.AddToClassList("command-input-field");
            UIResourceHelper.ApplyDefaultFont(textField);
            
            // Apply default font to input elements - CSS handles styling
            textField.schedule.Execute(() => {
                var textInput = textField.Q(className: "unity-text-field__input");
                if (textInput != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textInput);
                }
            });
            
            if (!string.IsNullOrEmpty(ToolTip))
            {
                textField.tooltip = ToolTip;
            }
            container.Add(textField);

            var applyButton = CreateApplyButton();
            container.Add(applyButton);

            return container;
        }

        public override void BindUIToolkitEvents(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var textField = container.Q<TextField>();
                var applyButton = container.Q<Button>();
                
                if (textField != null && applyButton != null)
                {
                    textField.RegisterValueChangedCallback(evt_ => {
                        _displayValue = evt_.newValue;
                        UpdateApplyButtonVisibility(container, textField, applyButton);
                    });

                    textField.RegisterCallback<KeyDownEvent>(evt_ => {
                        if (evt_.keyCode == KeyCode.Return || evt_.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeValue(textField.value);
                            UpdateApplyButtonVisibility(container, textField, applyButton);
                        }
                    });

                    // Bind Apply button click event
                    applyButton.RegisterCallback<ClickEvent>(_ => {
                        Debug.Log($"Apply button clicked for command: {CommandName}");
                        ChangeValue(textField.value);
                        UpdateApplyButtonVisibility(container, textField, applyButton);
                    });
                }
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var textField = container.Q<TextField>();
                var applyButton = container.Q<Button>();
                
                if (textField != null)
                {
                    textField.SetValueWithoutNotify(_displayValue ?? string.Empty);
                    if (applyButton != null)
                    {
                        UpdateApplyButtonVisibility(container, textField, applyButton);
                    }
                }
            }
        }

        private void UpdateApplyButtonVisibility(VisualElement container_, TextField textField_, Button applyButton_)
        {
            bool isDifferent = !string.Equals(textField_.value, _value);
            applyButton_.style.display = isDifferent ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}