// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class CommandCollection
    {
        public readonly Dictionary<string, List<CommandItemAbstract>> commandsByCategory = new Dictionary<string, List<CommandItemAbstract>>();

        private void InsertItem(CommandItemAbstract commandItem_)
        {
            if (commandItem_ == null)
                return;

            if (commandsByCategory.TryGetValue(commandItem_.Category, out List<CommandItemAbstract> commandList) == false)
            {
                commandList = new List<CommandItemAbstract>();
                commandsByCategory.Add(commandItem_.Category, commandList);
            }

            int foundIndex = commandList.FindIndex((item_) => item_.CommandName.Equals(commandItem_.CommandName));
            if (0 <= foundIndex && foundIndex < commandList.Count)
            {
                commandList[foundIndex] = commandItem_;
            }
            else
            {
                commandList.Add(commandItem_);
                commandList.Sort(ComparisonByDisplayPriority);
            }
        }

        public void RemoveAllItems()
        {
            foreach (List<CommandItemAbstract> commandList in commandsByCategory.Values)
            {
                commandList.Clear();
            }
            commandsByCategory.Clear();
        }

        private static int ComparisonByDisplayPriority(CommandItemAbstract left_, CommandItemAbstract right_)
        {
            if (left_ == null && right_ == null)
                return 0;

            if (left_ != null && right_ == null)
                return -1;

            if (left_ == null)
                return 1;

            return left_.DisplayPriority.CompareTo(right_.DisplayPriority);
        }

        public void CreateAndInsertItemByPacket(CommandRegisterPacket commandRegisterPacket_)
        {
            if (commandRegisterPacket_ == null)
                return;

            switch (commandRegisterPacket_.commandType)
            {
                case CommandType.BOOL:
                {
                    bool.TryParse(commandRegisterPacket_.commandValue, out bool boolValue);
                    CommandItemBool commandItem = new CommandItemBool(commandRegisterPacket_.commandCategory, commandRegisterPacket_.commandName, boolValue,
                                                                      commandRegisterPacket_.commandDisplayPriority, commandRegisterPacket_.commandTooltip);
                    InsertItem(commandItem);
                    break;
                }
                case CommandType.DOUBLE:
                {
                    double.TryParse(commandRegisterPacket_.commandValue, out double doubleValue);
                    CommandItemDouble commandItem = new CommandItemDouble(commandRegisterPacket_.commandCategory, commandRegisterPacket_.commandName, doubleValue,
                                                                          commandRegisterPacket_.commandDisplayPriority, commandRegisterPacket_.commandTooltip);
                    InsertItem(commandItem);
                    break;
                }
                case CommandType.FLOAT:
                {
                    float.TryParse(commandRegisterPacket_.commandValue, out float floatValue);
                    CommandItemFloat commandItem = new CommandItemFloat(commandRegisterPacket_.commandCategory, commandRegisterPacket_.commandName, floatValue,
                                                                        commandRegisterPacket_.commandDisplayPriority, commandRegisterPacket_.commandTooltip);
                    InsertItem(commandItem);
                    break;
                }
                case CommandType.INT:
                {
                    int.TryParse(commandRegisterPacket_.commandValue, out int intValue);
                    CommandItemInt commandItem = new CommandItemInt(commandRegisterPacket_.commandCategory, commandRegisterPacket_.commandName, intValue,
                                                                    commandRegisterPacket_.commandDisplayPriority, commandRegisterPacket_.commandTooltip);
                    InsertItem(commandItem);
                    break;
                }
                case CommandType.LONG:
                {
                    long.TryParse(commandRegisterPacket_.commandValue, out long longValue);
                    CommandItemLong commandItem = new CommandItemLong(commandRegisterPacket_.commandCategory, commandRegisterPacket_.commandName, longValue,
                                                                      commandRegisterPacket_.commandDisplayPriority, commandRegisterPacket_.commandTooltip);
                    InsertItem(commandItem);
                    break;
                }
                case CommandType.STRING:
                {
                    CommandItemString commandItem = new CommandItemString(commandRegisterPacket_.commandCategory, commandRegisterPacket_.commandName,
                                                                          commandRegisterPacket_.commandValue, commandRegisterPacket_.commandDisplayPriority,
                                                                          commandRegisterPacket_.commandTooltip);
                    InsertItem(commandItem);
                    break;
                }
            }
        }
    }
}