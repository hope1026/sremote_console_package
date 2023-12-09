// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    public static class SCommand
    {
        public static bool Register(string categoryName_, string commandName_, bool defaultValue_, Action<bool> onChangedValueDelegate_,
                                        int displayPriority_ = CommandAbstract.DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
        {
            if (string.IsNullOrEmpty(categoryName_) || string.IsNullOrEmpty(commandName_) || onChangedValueDelegate_ == null)
                return false;

            CommandBool command = new CommandBool(categoryName_, commandName_, defaultValue_, onChangedValueDelegate_);
            command.SetDisplayPriority(displayPriority_);
            command.SetToolTip(tooltip_);
            return SDebug.ConsoleMain.TryAddCommand(command);
        }

        public static bool Register(string categoryName_, string commandName_, int defaultValue_, Action<int> onChangedValueDelegate_,
                                       int displayPriority_ = CommandAbstract.DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
        {
            if (string.IsNullOrEmpty(categoryName_) || string.IsNullOrEmpty(commandName_) || onChangedValueDelegate_ == null)
                return false;

            CommandInt command = new CommandInt(categoryName_, commandName_, defaultValue_, onChangedValueDelegate_);
            command.SetDisplayPriority(displayPriority_);
            command.SetToolTip(tooltip_);
            return SDebug.ConsoleMain.TryAddCommand(command);
        }

        public static bool Register(string categoryName_, string commandName_, long defaultValue_, Action<long> onChangedValueDelegate_,
                                        int displayPriority_ = CommandAbstract.DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
        {
            if (string.IsNullOrEmpty(categoryName_) || string.IsNullOrEmpty(commandName_) || onChangedValueDelegate_ == null)
                return false;

            CommandLong command = new CommandLong(categoryName_, commandName_, defaultValue_, onChangedValueDelegate_);
            command.SetDisplayPriority(displayPriority_);
            command.SetToolTip(tooltip_);
            return SDebug.ConsoleMain.TryAddCommand(command);
        }

        public static bool Register(string categoryName_, string commandName_, float defaultValue_, Action<float> onChangedValueDelegate_,
                                         int displayPriority_ = CommandAbstract.DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
        {
            if (string.IsNullOrEmpty(categoryName_) || string.IsNullOrEmpty(commandName_) || onChangedValueDelegate_ == null)
                return false;

            CommandFloat command = new CommandFloat(categoryName_, commandName_, defaultValue_, onChangedValueDelegate_);
            command.SetDisplayPriority(displayPriority_);
            command.SetToolTip(tooltip_);
            return SDebug.ConsoleMain.TryAddCommand(command);
        }

        public static bool Register(string categoryName_, string commandName_, double defaultValue_, Action<double> onChangedValueDelegate_,
                                          int displayPriority_ = CommandAbstract.DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
        {
            if (string.IsNullOrEmpty(categoryName_) || string.IsNullOrEmpty(commandName_) || onChangedValueDelegate_ == null)
                return false;

            CommandDouble command = new CommandDouble(categoryName_, commandName_, defaultValue_, onChangedValueDelegate_);
            command.SetDisplayPriority(displayPriority_);
            command.SetToolTip(tooltip_);
            return SDebug.ConsoleMain.TryAddCommand(command);
        }

        public static bool Register(string categoryName_, string commandName_, string defaultValue_, Action<string> onChangedValueDelegate_,
                                          int displayPriority_ = CommandAbstract.DEFAULT_DISPLAY_PRIORITY, string tooltip_ = "")
        {
            if (string.IsNullOrEmpty(categoryName_) || string.IsNullOrEmpty(commandName_) || onChangedValueDelegate_ == null)
                return false;

            CommandString command = new CommandString(categoryName_, commandName_, defaultValue_, onChangedValueDelegate_);
            command.SetDisplayPriority(displayPriority_);
            command.SetToolTip(tooltip_);
            return SDebug.ConsoleMain.TryAddCommand(command);
        }
    }
}