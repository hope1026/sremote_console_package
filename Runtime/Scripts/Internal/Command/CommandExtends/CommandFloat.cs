// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;

namespace SPlugin
{
    internal class CommandFloat : CommandAbstract
    {
        public override CommandType CommandType => CommandType.FLOAT;
        public override string ValueString => _value.ToString(CultureInfo.InvariantCulture);
        
        private float _value = 0;
        private readonly Action<float> _onChangedValueDelegate = null;
        public float Value => _value;

        internal CommandFloat(string category_, string name_, float value_, Action<float> onChangedValueDelegate_) : base(category_, name_)
        {
            _onChangedValueDelegate = onChangedValueDelegate_;
            _value = value_;
        }
        
        public override void OnReceiveCommandValue(string commandValue_)
        {
            if (float.TryParse(commandValue_, out float newValue) && Math.Abs(_value - newValue) > float.Epsilon)
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