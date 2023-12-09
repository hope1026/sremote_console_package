// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal abstract class CommandAbstract
    {
        public const int DEFAULT_DISPLAY_PRIORITY = int.MaxValue;

        private string _id;
        private readonly string _category;
        private readonly string _name;
        private string _tooltip;
        private int _displayPriority = DEFAULT_DISPLAY_PRIORITY;
        protected bool requestChangeValueDelegateInvoking = false;

        public string Category => _category;
        public string Name => _name;
        public string Tooltip => _tooltip;
        public int DisplayPriority => _displayPriority;
        public abstract CommandType CommandType { get; }
        public abstract string ValueString { get; }

        protected CommandAbstract(string category_, string name_)
        {
            _name = name_;
            _category = category_;
            requestChangeValueDelegateInvoking = true;
        }

        public void SetToolTip(string tooltip_)
        {
            if (tooltip_ == null)
                tooltip_ = string.Empty;

            _tooltip = tooltip_;
        }

        public void SetDisplayPriority(int displayPriority_)
        {
            _displayPriority = displayPriority_;
        }

        public abstract void OnReceiveCommandValue(string commandValue_);

        public static string GenerateCommandKey(string category_, string name_)
        {
            return category_ + name_;
        }

        public abstract void InvokeDelegateIfChangedValue();
    }
}