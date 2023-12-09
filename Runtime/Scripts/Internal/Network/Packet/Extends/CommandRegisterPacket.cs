// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;

namespace SPlugin
{
    internal class CommandRegisterPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.COMMAND_REGISTER; } }
        public CommandType commandType;
        public string commandCategory;
        public string commandName;
        public string commandValue;
        public string commandTooltip;
        public int commandDisplayPriority;

        protected override void OnWrite()
        {
            base.WriteByte((byte)commandType);
            base.WriteString(commandCategory);
            base.WriteString(commandName);

            switch (commandType)
            {
                case CommandType.BOOL:
                {
                    base.WriteBool(Convert.ToBoolean(commandValue));
                    break;
                }
                case CommandType.DOUBLE:
                {
                    base.WriteDouble(Convert.ToDouble(commandValue));
                    break;
                }
                case CommandType.FLOAT:
                {
                    base.WriteFloat(Convert.ToSingle(commandValue));
                    break;
                }
                case CommandType.INT:
                {
                    base.WriteInt(Convert.ToInt32(commandValue));
                    break;
                }
                case CommandType.LONG:
                {
                    base.WriteLong(Convert.ToInt64(commandValue));
                    break;
                }
                case CommandType.STRING:
                {
                    base.WriteString(commandValue);
                    break;
                }
            }

            base.WriteInt(commandDisplayPriority);

            if (commandTooltip == null)
                commandTooltip = string.Empty;

            base.WriteString(commandTooltip);
        }

        protected override void OnRead()
        {
            commandType = (CommandType)base.ReadByte();
            commandCategory = base.ReadString();
            commandName = base.ReadString();

            switch (commandType)
            {
                case CommandType.BOOL:
                {
                    commandValue = base.ReadBool().ToString();
                    break;
                }
                case CommandType.DOUBLE:
                {
                    commandValue = base.ReadDouble().ToString(CultureInfo.InvariantCulture);
                    break;
                }
                case CommandType.FLOAT:
                {
                    commandValue = base.ReadFloat().ToString(CultureInfo.InvariantCulture);
                    break;
                }
                case CommandType.INT:
                {
                    commandValue = base.ReadInt().ToString();
                    break;
                }
                case CommandType.LONG:
                {
                    commandValue = base.ReadLong().ToString();
                    break;
                }
                case CommandType.STRING:
                {
                    commandValue = base.ReadString();
                    break;
                }
            }

            commandDisplayPriority = base.ReadInt();
            commandTooltip = base.ReadString();
        }
    }
}