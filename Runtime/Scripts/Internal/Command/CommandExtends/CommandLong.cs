// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    internal class CommandLong : CommandAbstract
    {
        public override CommandType CommandType => CommandType.LONG;
        public override string ValueString => _value.ToString();
        private long _value = 0;
        private readonly Action<long> _onChangedValueDelegate = null;
        public long Value => _value;

        internal CommandLong(string category_, string name_, long value_, Action<long> onChangedValueDelegate_) : base(category_, name_)
        {
            _onChangedValueDelegate = onChangedValueDelegate_;
            _value = value_;
        }

        public override void OnReceiveCommandValue(string commandValue_)
        {
            if (long.TryParse(commandValue_, out long newValue) && _value != newValue)
            {
                _value = newValue;
                base.requestChangeValueDelegateInvoking = true;
            }
        }
        
        public override void InvokeDelegateIfChangedValue()
        {
            if (base.requestChangeValueDelegateInvoking)
            {
                base.requestChangeValueDelegateInvoking = false;
                _onChangedValueDelegate?.Invoke(_value);
            }
        }
    }
}