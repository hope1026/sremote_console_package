// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin
{
    internal class CommandView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.COMMAND;

        private VisualElement _rootElement;
        private VisualElement _noCommandsMessage;
        private VisualElement _commandsInterface;
        private VisualElement _categoryTabs;
        private VisualElement _commandsList;

        private string _selectedCategoryName = string.Empty;
        private readonly List<Button> _categoryButtons = new List<Button>();
        private int _lastCommandCollectionCount = -1;
        private int _lastCategoryCount = -1;

        protected override void OnInitialize()
        {
            CreateUIElements();
            UpdateCommandsDisplay();
        }

        protected override void OnShow()
        {
            UpdateCommandsDisplay();
        }

        private void CreateUIElements()
        {
            // Load UXML template
            var visualTree = Resources.Load<VisualTreeAsset>("UI/CommandView");
            if (visualTree == null)
            {
                Debug.LogError("CommandView.uxml not found in Resources/UI/");
                return;
            }

            _rootElement = visualTree.Instantiate();

            // Load USS styles
            var baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
            var commandViewStyles = Resources.Load<StyleSheet>("UI/CommandViewStyles");
            
            if (baseStyles != null)
            {
                _rootElement.styleSheets.Add(baseStyles);
            }
            if (commandViewStyles != null)
            {
                _rootElement.styleSheets.Add(commandViewStyles);
            }

            // Get references to UI elements
            _noCommandsMessage = _rootElement.Q<VisualElement>("no-commands-message");
            _commandsInterface = _rootElement.Q<VisualElement>("commands-interface");
            _categoryTabs = _rootElement.Q<VisualElement>("category-tabs");
            _commandsList = _rootElement.Q<VisualElement>("commands-list");
        }

        public override void UpdateCustom()
        {
            CheckAndUpdateCommandsDisplay();
        }

        private void CheckAndUpdateCommandsDisplay()
        {
            if (CurrentAppRef == null)
            {
                // Reset counters when no app
                _lastCommandCollectionCount = -1;
                _lastCategoryCount = -1;
                UpdateCommandsDisplay();
                return;
            }

            // Check if command collection has changed
            var commandsByCategory = CurrentAppRef.commandCollection.commandsByCategory;
            int totalCommandCount = 0;
            foreach (var categoryCommands in commandsByCategory.Values)
            {
                totalCommandCount += categoryCommands.Count;
            }

            bool commandsChanged = _lastCommandCollectionCount != totalCommandCount || _lastCategoryCount != commandsByCategory.Count;
            
            if (commandsChanged)
            {
                _lastCommandCollectionCount = totalCommandCount;
                _lastCategoryCount = commandsByCategory.Count;
                UpdateCommandsDisplay();
            }
        }

        private void UpdateCommandsDisplay()
        {
            if (CurrentAppRef == null || !CurrentAppRef.IsPlaying())
            {
                ShowNoCommandsMessage("Command feature is valid in play mode.");
                return;
            }

            if (CurrentAppRef.commandCollection.commandsByCategory.Count <= 0)
            {
                ShowNoCommandsMessage("You can add commands using SCommand.Register()");
                return;
            }

            ShowCommandsInterface();
            UpdateCategoryTabs();
            UpdateCommandsList();
        }

        private void ShowNoCommandsMessage(string message)
        {
            if (_noCommandsMessage != null)
            {
                _noCommandsMessage.style.display = DisplayStyle.Flex;
                var titleLabel = _noCommandsMessage.Q<Label>("no-commands-title");
                if (titleLabel != null)
                {
                    titleLabel.text = message;
                }
            }
            
            if (_commandsInterface != null)
            {
                _commandsInterface.style.display = DisplayStyle.None;
            }
        }

        private void ShowCommandsInterface()
        {
            if (_noCommandsMessage != null)
            {
                _noCommandsMessage.style.display = DisplayStyle.None;
            }
            
            if (_commandsInterface != null)
            {
                _commandsInterface.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateCategoryTabs()
        {
            if (_categoryTabs == null || CurrentAppRef == null) return;

            _categoryTabs.Clear();
            _categoryButtons.Clear();

            var commandsByCategory = CurrentAppRef.commandCollection.commandsByCategory;
            
            foreach (string categoryName in commandsByCategory.Keys)
            {
                if (string.IsNullOrEmpty(_selectedCategoryName))
                {
                    _selectedCategoryName = categoryName;
                }

                var categoryButton = new Button();
                categoryButton.text = categoryName;
                categoryButton.AddToClassList("category-tab-button");
                
                if (_selectedCategoryName.Equals(categoryName))
                {
                    categoryButton.AddToClassList("selected");
                }

                _categoryTabs.Add(categoryButton);
                _categoryButtons.Add(categoryButton);
                
                // Bind click event directly
                categoryButton.RegisterCallback<ClickEvent>(_ => {
                    SelectCategory(categoryName);
                });
            }
        }

        private void SelectCategory(string categoryName)
        {
            if (_selectedCategoryName == categoryName) return;

            _selectedCategoryName = categoryName;
            
            // Update button states
            foreach (var button in _categoryButtons)
            {
                button.RemoveFromClassList("selected");
                if (button.text == categoryName)
                {
                    button.AddToClassList("selected");
                }
            }

            UpdateCommandsList();
        }

        private void UpdateCommandsList()
        {
            if (_commandsList == null || CurrentAppRef == null) return;

            _commandsList.Clear();

            if (string.IsNullOrEmpty(_selectedCategoryName)) return;

            if (!CurrentAppRef.commandCollection.commandsByCategory.TryGetValue(_selectedCategoryName, out List<CommandItemAbstract> commandItemList) ||
                commandItemList == null ||
                commandItemList.Count <= 0)
            {
                return;
            }

            foreach (CommandItemAbstract commandItem in commandItemList)
            {
                var commandElement = CreateCommandElement(commandItem);
                _commandsList.Add(commandElement);
                
                // Schedule value update after the element is added to the hierarchy
                _commandsList.schedule.Execute(() => {
                    var valueContainer = commandElement.Q<VisualElement>("command-value-container");
                    if (valueContainer != null && valueContainer.childCount > 0)
                    {
                        commandItem.UpdateUIToolkitValue(valueContainer[0]);
                    }
                });
            }
        }

        private VisualElement CreateCommandElement(CommandItemAbstract commandItem)
        {
            var commandContainer = new VisualElement();
            commandContainer.AddToClassList("command-item");

            // Command name
            var nameLabel = new Label(commandItem.CommandName);
            nameLabel.AddToClassList("command-name-label");
            nameLabel.style.minWidth = 200;
            commandContainer.Add(nameLabel);

            // Command value container
            var valueContainer = new VisualElement();
            valueContainer.name = "command-value-container";
            valueContainer.AddToClassList("command-value-container");

            // Create control using the command item's own method
            VisualElement valueControl = commandItem.CreateUIToolkitControl();
            if (valueControl != null)
            {
                valueContainer.Add(valueControl);
                
                // Schedule event binding after the control is added to the hierarchy
                valueContainer.schedule.Execute(() => {
                    commandItem.BindUIToolkitEvents(valueControl);
                });
            }

            commandContainer.Add(valueContainer);

            // Tooltip
            if (!string.IsNullOrEmpty(commandItem.ToolTip))
            {
                commandContainer.tooltip = commandItem.ToolTip;
            }

            return commandContainer;
        }

        public VisualElement GetRootElement()
        {
            if (_rootElement == null)
            {
                CreateUIElements();
            }
            return _rootElement;
        }

        protected override void OnTerminate()
        {
            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
            _categoryButtons.Clear();
        }
    }
}