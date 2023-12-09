// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;

namespace SPlugin
{
    internal class LogPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.LOG; } }
        public LogContext context = new LogContext();

        public LogPacket()
        {
            context.LogType = LogType.Log;
            context.LogString = string.Empty;
            context.LogStackTrace = string.Empty;
            context.FrameCount = 0;
            context.TimeSeconds = 0;
            context.ObjectInstanceID = 0;
        }

        protected override void OnWrite()
        {
            base.WriteByte((byte)context.LogType);
            base.WriteString(context.LogString);
            base.WriteString(context.LogStackTrace);
            base.WriteInt(context.FrameCount);
            base.WriteFloat(context.TimeSeconds);
            base.WriteInt(context.ObjectInstanceID);
            base.WriteString(context.ObjectName);
        }

        protected override void OnRead()
        {
            context.LogType = (LogType)base.ReadByte();
            context.LogString = base.ReadString();
            context.LogStackTrace = base.ReadString();
            context.FrameCount = base.ReadInt();
            context.TimeSeconds = base.ReadFloat();
            context.ObjectInstanceID = base.ReadInt();
            context.ObjectName = base.ReadString();
        }
    }
}