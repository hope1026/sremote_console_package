// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal class PausePlayingPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.PAUSE_PLAYING; } }
        public bool IsPause { get; set; } = false;
        public bool CanStep { get; set; } = false;
        public bool PauseWhenError { get; set; } = true;

        protected override void OnWrite()
        {
            base.WriteBool(IsPause);
            base.WriteBool(CanStep);
            base.WriteBool(PauseWhenError);
        }

        protected override void OnRead()
        {
            IsPause = base.ReadBool();
            CanStep = base.ReadBool();
            PauseWhenError = base.ReadBool();
        }
    }
}