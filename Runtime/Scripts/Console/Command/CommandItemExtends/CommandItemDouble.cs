// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class CommandItemDouble : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.DOUBLE;
        public override string ValueString => _value.ToString(CultureInfo.InvariantCulture);
        private double _value = 0;
        private double _displayValue;

        public double Value => _value;

        internal CommandItemDouble(string category_, string name_, double value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public void ChangeValue(double newValue_)
        {
            if (Math.Abs(newValue_ - _value) > double.Epsilon)
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

            var doubleField = new DoubleField();
            doubleField.value = _displayValue;
            doubleField.AddToClassList("command-input-field");
            UIResourceHelper.ApplyDefaultFont(doubleField);
            
            // Apply default font - CSS handles styling
            doubleField.schedule.Execute(() => {
                // Try multiple possible input element class names
                var textInput = doubleField.Q(className: "unity-text-input") ?? 
                               doubleField.Q(className: "unity-base-text-field__input") ?? 
                               doubleField.Q(className: "unity-text-field__input");
                if (textInput != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textInput);
                }
                
                // Also apply to text element if it exists
                var textElement = doubleField.Q(className: "unity-text-element");
                if (textElement != null)
                {
                    UIResourceHelper.ApplyDefaultFont(textElement);
                }
            });
            
            if (!string.IsNullOrEmpty(ToolTip))
            {
                doubleField.tooltip = ToolTip;
            }
            container.Add(doubleField);

            var applyButton = CreateApplyButton();
            container.Add(applyButton);

            return container;
        }

        public override void BindUIToolkitEvents(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var doubleField = container.Q<DoubleField>();
                var applyButton = container.Q<Button>();
                
                if (doubleField != null && applyButton != null)
                {
                    doubleField.RegisterValueChangedCallback(evt_ => {
                        _displayValue = evt_.newValue;
                        UpdateApplyButtonVisibility(container, doubleField, applyButton);
                    });

                    doubleField.RegisterCallback<KeyDownEvent>(evt_ => {
                        if (evt_.keyCode == KeyCode.Return || evt_.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeValue(doubleField.value);
                            UpdateApplyButtonVisibility(container, doubleField, applyButton);
                        }
                    });

                    // Bind Apply button click event
                    applyButton.RegisterCallback<ClickEvent>(_ => {
                        Debug.Log($"Apply button clicked for command: {CommandName}");
                        ChangeValue(doubleField.value);
                        UpdateApplyButtonVisibility(container, doubleField, applyButton);
                    });
                }
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var doubleField = container.Q<DoubleField>();
                var applyButton = container.Q<Button>();
                
                if (doubleField != null)
                {
                    doubleField.SetValueWithoutNotify(_displayValue);
                    if (applyButton != null)
                    {
                        UpdateApplyButtonVisibility(container, doubleField, applyButton);
                    }
                }
            }
        }

        private void UpdateApplyButtonVisibility(VisualElement container_, DoubleField doubleField_, Button applyButton_)
        {
            bool isDifferent = Math.Abs(doubleField_.value - _value) > double.Epsilon;
            applyButton_.style.display = isDifferent ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}