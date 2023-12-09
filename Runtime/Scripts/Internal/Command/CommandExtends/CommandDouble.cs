// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;

namespace SPlugin
{
    internal class CommandDouble : CommandAbstract
    {
        public override CommandType CommandType => CommandType.DOUBLE;
        public override string ValueString => _value.ToString(CultureInfo.InvariantCulture);

        private double _value = 0;
        private readonly Action<double> _onChangedValueDelegate = null;
        public double Value => _value;

        internal CommandDouble(string category_, string name_, double value_, Action<double> onChangedValueDelegate_) : base(category_, name_)
        {
            _onChangedValueDelegate = onChangedValueDelegate_;
            _value = value_;
        }

        public override void OnReceiveCommandValue(string commandValue_)
        {
            if (double.TryParse(commandValue_, out double newValue) && Math.Abs(_value - newValue) > double.Epsilon)
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