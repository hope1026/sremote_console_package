// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal class PreferencesPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.PREFERENCES; } }
        public readonly PreferencesContext context = new PreferencesContext();

        protected override void OnWrite()
        {
            base.WriteFloat(context.ProfileRefreshIntervalTimeSeconds);
            base.WriteUint(context.SkipStackFrameCount);
            base.WriteBool(context.ShowUnityDebugLog);
        }

        protected override void OnRead()
        {
            context.ProfileRefreshIntervalTimeSeconds = base.ReadFloat();
            context.SkipStackFrameCount = base.ReadUint();
            context.ShowUnityDebugLog = base.ReadBool();
        }
    }
}