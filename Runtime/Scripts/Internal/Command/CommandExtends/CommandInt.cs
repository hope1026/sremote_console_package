// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    internal class CommandInt : CommandAbstract
    {
        public override CommandType CommandType => CommandType.INT;
        public override string ValueString => _value.ToString();
        private int _value = 0;
        private readonly Action<int> _onChangedValueDelegate = null;
        public int Value => _value;

        internal CommandInt(string category_, string name_, int value_, Action<int> onChangedValueDelegate_) : base(category_, name_)
        {
            _onChangedValueDelegate = onChangedValueDelegate_;
            _value = value_;
        }
        
        public override void OnReceiveCommandValue(string commandValue_)
        {
            if (int.TryParse(commandValue_, out int newValue) && _value != newValue)
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