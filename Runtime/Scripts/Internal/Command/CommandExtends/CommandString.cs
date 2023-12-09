// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    internal class CommandString : CommandAbstract
    {
        public override CommandType CommandType => CommandType.STRING;
        public override string ValueString => _value;

        string _value;
        private readonly Action<string> _onChangedValueDelegate = null;
        public string Value => _value;

        internal CommandString(string category_, string name_, string value_, Action<string> onChangedValueDelegate_) : base(category_, name_)
        {
            _onChangedValueDelegate = onChangedValueDelegate_;
            _value = value_;
        }

        public override void OnReceiveCommandValue(string commandValue_)
        {
            if (_value != commandValue_)
            {
                _value = commandValue_;
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