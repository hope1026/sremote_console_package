// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;

namespace SPlugin
{
    internal class SystemLogPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.SYSTEM_LOG; } }
        public string log;
        public LogType logType;
        
        protected override void OnWrite()
        {
            base.WriteInt((int)logType);
            base.WriteString(log);
        }

        protected override void OnRead()
        {
            log = base.ReadString();
            logType = (LogType)base.ReadInt();
        }
    }
}