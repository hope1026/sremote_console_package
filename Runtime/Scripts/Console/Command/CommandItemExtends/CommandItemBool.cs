// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class CommandItemBool : CommandItemAbstract
    {
        public override CommandType CommandType => CommandType.BOOL;
        public override string ValueString => _value.ToString();
        private bool _value = false;
        private bool _displayValue;

        public bool Value => _value;

        internal CommandItemBool(string category_, string name_, bool value_, int displayPriority_ = DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
            : base(category_, name_, displayPriority_, tooltip_)
        {
            _value = value_;
            _displayValue = value_;
        }

        public void ChangeValue(bool newValue_)
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

            var toggle = new Toggle();
            toggle.value = _displayValue;
            toggle.AddToClassList("command-bool-toggle"); // CSS handles styling
            UIResourceHelper.ApplyDefaultFont(toggle);
            
            // Fix toggle child elements sizing and add checkmark text for runtime visibility
            toggle.schedule.Execute(() => {
                var checkmark = toggle.Q(className: "unity-toggle__checkmark");
                if (checkmark != null)
                {
                    // Add checkmark text for runtime visibility
                    var checkmarkLabel = new Label("✓");
                    checkmarkLabel.style.color = Color.white;
                    checkmarkLabel.style.fontSize = 12;
                    checkmarkLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    checkmarkLabel.style.position = Position.Absolute;
                    checkmarkLabel.style.width = 16;
                    checkmarkLabel.style.height = 16;
                    checkmarkLabel.style.left = 0;
                    checkmarkLabel.style.top = 0;
                    checkmarkLabel.style.display = toggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                    checkmark.Add(checkmarkLabel);
                    
                    // Store reference for updating visibility
                    checkmark.userData = checkmarkLabel;
                }
                
                // Apply font to label if it exists
                var text = toggle.Q<Label>();
                if (text != null)
                {
                    UIResourceHelper.ApplyDefaultFont(text);
                }
            });
            
            if (!string.IsNullOrEmpty(ToolTip))
            {
                toggle.tooltip = ToolTip;
            }
            container.Add(toggle);

            var applyButton = CreateApplyButton();
            container.Add(applyButton);

            return container;
        }

        public override void BindUIToolkitEvents(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var toggle = container.Q<Toggle>();
                var applyButton = container.Q<Button>();
                
                if (toggle != null && applyButton != null)
                {
                    toggle.RegisterValueChangedCallback(evt_ => 
                    {
                        _displayValue = evt_.newValue;
                        UpdateApplyButtonVisibility(container, toggle, applyButton);
                        
                        // Update checkmark visibility
                        var checkmark = toggle.Q(className: "unity-toggle__checkmark");
                        if (checkmark?.userData is Label checkmarkLabel)
                        {
                            checkmarkLabel.style.display = evt_.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                    });

                    // Bind Apply button click event
                    applyButton.RegisterCallback<ClickEvent>(_ => {
                        ChangeValue(toggle.value);
                        UpdateApplyButtonVisibility(container, toggle, applyButton);
                    });
                }
            }
        }

        public override void UpdateUIToolkitValue(VisualElement control_)
        {
            if (control_ is VisualElement container)
            {
                var toggle = container.Q<Toggle>();
                var applyButton = container.Q<Button>();
                
                if (toggle != null)
                {
                    toggle.SetValueWithoutNotify(_displayValue);
                    if (applyButton != null)
                    {
                        UpdateApplyButtonVisibility(container, toggle, applyButton);
                    }
                    
                    // Update checkmark visibility
                    var checkmark = toggle.Q(className: "unity-toggle__checkmark");
                    if (checkmark?.userData is Label checkmarkLabel)
                    {
                        checkmarkLabel.style.display = _displayValue ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
            }
        }

        private void UpdateApplyButtonVisibility(VisualElement container_, Toggle toggle_, Button applyButton_)
        {
            bool isDifferent = toggle_.value != _value;
            applyButton_.style.display = isDifferent ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}