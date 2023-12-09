// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal class ProfileInfoPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.PROFILE_INFO; } }
        public ProfileInfoContext context = new ProfileInfoContext();

        public ProfileInfoPacket() : base()
        {
            context.FramePerSecond = 0;
            context.UsedHeapSize = 0;
        }

        protected override void OnWrite()
        {
            base.WriteFloat(context.FramePerSecond);
            base.WriteLong(context.UsedHeapSize);
        }

        protected override void OnRead()
        {
            context.FramePerSecond = base.ReadFloat();
            context.UsedHeapSize = base.ReadLong();
        }
    }
}