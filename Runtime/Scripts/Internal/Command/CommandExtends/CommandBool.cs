// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    internal class CommandBool : CommandAbstract
    {
        public override CommandType CommandType => CommandType.BOOL;
        public override string ValueString => _value.ToString();

        private bool _value = false;
        private readonly Action<bool> _onChangedValueDelegate = null;
        public bool Value => _value;

        internal CommandBool(string category_, string name_, bool value_, Action<bool> onChangedValueDelegate_) : base(category_, name_)
        {
            _onChangedValueDelegate = onChangedValueDelegate_;
            _value = value_;
        }

        public override void OnReceiveCommandValue(string commandValue_)
        {
            if (bool.TryParse(commandValue_, out bool newValue) && _value != newValue)
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