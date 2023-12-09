// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal class SyncActivePacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.SYNC_ACTIVE; } }
        public bool isActiveSync;

        protected override void OnWrite()
        {
            base.WriteBool(isActiveSync);
        }

        protected override void OnRead()
        {
            isActiveSync = base.ReadBool();
        }
    }
}