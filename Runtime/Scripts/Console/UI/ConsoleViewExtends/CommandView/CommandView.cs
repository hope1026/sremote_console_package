// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class CommandView : ConsoleViewAbstract
    {
        private VisualElement _viewRootElement;
        private VisualElement _noCommandsMessageRoot;
        private VisualElement _commandsInterfaceRoot;
        private VisualElement _categoryTabsScroll;
        private VisualElement _commandsList;

        private string _selectedCategoryName = string.Empty;
        private readonly List<Button> _categoryButtons = new List<Button>();
        private int _lastCommandCollectionCount = -1;
        private int _lastCategoryCount = -1;

        public override ConsoleViewType ConsoleViewType => ConsoleViewType.COMMAND;

        protected override void OnInitialize()
        {
            LoadUIFromUXML();
            LoadStyleSheets();
            UpdateCommandsDisplay();
        }

        protected override void OnTerminate()
        {
            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
            _categoryButtons.Clear();
        }

        protected override void OnShow()
        {
            if (_viewRootElement != null)
            {
                _viewRootElement.style.display = DisplayStyle.Flex;
                UpdateCommandsDisplay();
            }
        }

        protected override void OnHide()
        {
            if (_viewRootElement != null)
            {
                _viewRootElement.style.display = DisplayStyle.None;
            }
        }

        private void LoadUIFromUXML()
        {
            // Load UXML template
            var visualTreeAsset = UIResourceHelper.LoadUXML("CommandViewRuntime");
            if (visualTreeAsset != null)
            {
                _viewRootElement = visualTreeAsset.Instantiate();
                _viewRootElement.name = "command-view";
                _viewRootElement.style.flexGrow = 1;
                
                // Get references to UI elements
                _noCommandsMessageRoot = _viewRootElement.Q<VisualElement>("no-commands-message-root");
                _commandsInterfaceRoot = _viewRootElement.Q<VisualElement>("commands-interface-root");
                _categoryTabsScroll = _viewRootElement.Q<VisualElement>("category-tabs");
                _commandsList = _viewRootElement.Q<ScrollView>("commands-list");
            }
            else
            {
                Debug.LogError("CommandView.uxml not found in Resources/UI/");
                // Fallback to programmatic creation
                CreateUIElementsFallback();
            }

            if (_rootElement != null && _viewRootElement != null)
            {
                _rootElement.Add(_viewRootElement);
            }
        }

        private void CreateUIElementsFallback()
        {
            // Create root element programmatically as fallback
            _viewRootElement = new VisualElement();
            _viewRootElement.name = "command-view";
            _viewRootElement.AddToClassList("command-view");

            // Create no commands message
            _noCommandsMessageRoot = new VisualElement();
            _noCommandsMessageRoot.name = "no-commands-message-root";
            _noCommandsMessageRoot.AddToClassList("no-commands-message-root");

            var noCommandsTitle = UIResourceHelper.CreateLabel("No commands available");
            noCommandsTitle.name = "no-commands-title";
            noCommandsTitle.AddToClassList("no-commands-title");
            _noCommandsMessageRoot.Add(noCommandsTitle);

            _viewRootElement.Add(_noCommandsMessageRoot);

            // Create commands interface
            _commandsInterfaceRoot = new VisualElement();
            _commandsInterfaceRoot.name = "commands-interface-root";
            _commandsInterfaceRoot.AddToClassList("commands-interface-root");

            // Create category tabs container
            VisualElement categoryTabsContainer = new VisualElement();
            categoryTabsContainer.name = "category-tabs-container";
            categoryTabsContainer.AddToClassList("category-tabs-container");
            _commandsInterfaceRoot.Add(categoryTabsContainer);
            
            _categoryTabsScroll = new VisualElement();
            _categoryTabsScroll.name = "category-tabs";
            _categoryTabsScroll.AddToClassList("category-tabs");
            categoryTabsContainer.Add(_categoryTabsScroll);

            // Create commands list container
            VisualElement commandsListContainer = new VisualElement();
            commandsListContainer.name = "commands-list-container";
            commandsListContainer.AddToClassList("commands-list-container");
            _commandsInterfaceRoot.Add(commandsListContainer);

            // Create commands header
            VisualElement commandsHeader = new VisualElement();
            commandsHeader.name = "commands-header";
            commandsHeader.AddToClassList("commands-header");
            commandsListContainer.Add(commandsHeader);

            Label headerName = UIResourceHelper.CreateLabel("Command Name");
            headerName.AddToClassList("command-header-name");
            commandsHeader.Add(headerName);

            Label headerValue = UIResourceHelper.CreateLabel("Command Value");
            headerValue.AddToClassList("command-header-value");
            commandsHeader.Add(headerValue);

            // Create commands list
            _commandsList = new ScrollView();
            _commandsList.name = "commands-list";
            _commandsList.AddToClassList("commands-list");
            commandsListContainer.Add(_commandsList);

            _viewRootElement.Add(_commandsInterfaceRoot);
        }

        private void LoadStyleSheets()
        {
            if (_viewRootElement == null) return;

            // Load USS styles
            var baseStyles = UIResourceHelper.LoadStyleSheet("BaseRuntimeStyles");
            var commandViewStyles = UIResourceHelper.LoadStyleSheet("CommandViewRuntimeStyles");
            
            if (baseStyles != null)
            {
                _viewRootElement.styleSheets.Add(baseStyles);
            }
            if (commandViewStyles != null)
            {
                _viewRootElement.styleSheets.Add(commandViewStyles);
            }

            // Apply fonts programmatically since CSS font references don't work at runtime
            UIResourceHelper.ApplyDefaultFontRecursive(_viewRootElement);
        }
        
        public override void UpdateCustom()
        {
            CheckAndUpdateCommandsDisplay();
        }

        private void CheckAndUpdateCommandsDisplay()
        {
            if (currentAppRef == null)
            {
                // Reset counters when no app
                _lastCommandCollectionCount = -1;
                _lastCategoryCount = -1;
                UpdateCommandsDisplay();
                return;
            }

            // Check if command collection has changed
            var commandsByCategory = currentAppRef.commandCollection.commandsByCategory;
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
            if (currentAppRef == null)
            {
                ShowNoCommandsMessage("Command feature is valid in play mode.");
                return;
            }

            if (currentAppRef.commandCollection.commandsByCategory.Count <= 0)
            {
                ShowNoCommandsMessage("You can add commands using SCommand.Register()");
                return;
            }

            ShowCommandsInterface();
            UpdateCategoryTabs();
            UpdateCommandsList();
        }

        private void ShowNoCommandsMessage(string message_)
        {
            if (_noCommandsMessageRoot != null)
            {
                _noCommandsMessageRoot.style.display = DisplayStyle.Flex;
                var titleLabel = _noCommandsMessageRoot.Q<Label>("no-commands-title");
                if (titleLabel != null)
                {
                    titleLabel.text = message_;
                }
            }

            if (_commandsInterfaceRoot != null)
            {
                _commandsInterfaceRoot.style.display = DisplayStyle.None;
            }
        }

        private void ShowCommandsInterface()
        {
            if (_noCommandsMessageRoot != null)
            {
                _noCommandsMessageRoot.style.display = DisplayStyle.None;
            }

            if (_commandsInterfaceRoot != null)
            {
                _commandsInterfaceRoot.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateCategoryTabs()
        {
            if (_categoryTabsScroll == null || currentAppRef == null) return;

            _categoryTabsScroll.Clear();
            _categoryButtons.Clear();

            var commandsByCategory = currentAppRef.commandCollection.commandsByCategory;

            foreach (string categoryName in commandsByCategory.Keys)
            {
                if (string.IsNullOrEmpty(_selectedCategoryName))
                {
                    _selectedCategoryName = categoryName;
                }

                var categoryButton = UIResourceHelper.CreateButton(categoryName);
                categoryButton.AddToClassList("category-tab-button");

                if (_selectedCategoryName.Equals(categoryName))
                {
                    categoryButton.AddToClassList("selected");
                }

                _categoryTabsScroll.Add(categoryButton);
                _categoryButtons.Add(categoryButton);

                // Bind click event
                categoryButton.RegisterCallback<ClickEvent>(_ =>
                {
                    SelectCategory(categoryName);
                });
            }
        }

        private void SelectCategory(string categoryName_)
        {
            if (_selectedCategoryName == categoryName_) return;

            _selectedCategoryName = categoryName_;

            // Update button states
            foreach (var button in _categoryButtons)
            {
                button.RemoveFromClassList("selected");

                if (button.text == categoryName_)
                {
                    button.AddToClassList("selected");
                }
            }

            UpdateCommandsList();
        }

        private void UpdateCommandsList()
        {
            if (_commandsList == null || currentAppRef == null) return;

            _commandsList.Clear();

            if (string.IsNullOrEmpty(_selectedCategoryName)) return;

            if (!currentAppRef.commandCollection.commandsByCategory.TryGetValue(_selectedCategoryName, out List<CommandItemAbstract> commandItemList) ||
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
                _commandsList.schedule.Execute(() =>
                {
                    var valueContainer = commandElement.Q<VisualElement>("command-value-container");
                    if (valueContainer != null && valueContainer.childCount > 0)
                    {
                        commandItem.UpdateUIToolkitValue(valueContainer[0]);
                    }
                });
            }
        }

        private VisualElement CreateCommandElement(CommandItemAbstract commandItem_)
        {
            var commandContainer = new VisualElement();
            commandContainer.AddToClassList("command-item");

            // Command name
            var nameLabel = UIResourceHelper.CreateLabel(commandItem_.CommandName);
            nameLabel.AddToClassList("command-name-label");
            commandContainer.Add(nameLabel);

            // Command value container
            var valueContainer = new VisualElement();
            valueContainer.name = "command-value-container";
            valueContainer.AddToClassList("command-value-container");

            // Create control using the command item's own method
            VisualElement valueControl = commandItem_.CreateUIToolkitControl();
            if (valueControl != null)
            {
                valueContainer.Add(valueControl);

                // Schedule event binding after the control is added to the hierarchy
                valueContainer.schedule.Execute(() =>
                {
                    commandItem_.BindUIToolkitEvents(valueControl);
                });
            }

            commandContainer.Add(valueContainer);

            // Tooltip
            if (!string.IsNullOrEmpty(commandItem_.ToolTip))
            {
                commandContainer.tooltip = commandItem_.ToolTip;
            }

            return commandContainer;
        }
    }
}