// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal static class PacketFactory
    {
        public static PacketAbstract Instance(PacketAbstract.Type type_)
        {
            switch (type_)
            {
                case PacketAbstract.Type.SYNC_ACTIVE:      { return new SyncActivePacket(); }
                case PacketAbstract.Type.COMMAND:          { return new CommandPacket(); }
                case PacketAbstract.Type.COMMAND_REGISTER: { return new CommandRegisterPacket(); }
                case PacketAbstract.Type.LOG:              { return new LogPacket(); }
                case PacketAbstract.Type.PAUSE_PLAYING:    { return new PausePlayingPacket(); }
                case PacketAbstract.Type.PREFERENCES:      { return new PreferencesPacket(); }
                case PacketAbstract.Type.PROFILE_INFO:     { return new ProfileInfoPacket(); }
                case PacketAbstract.Type.SYSTEM_INFO:      { return new SystemInfoPacket(); }
                case PacketAbstract.Type.SYSTEM_LOG:       { return new SystemLogPacket(); }
            }
            return null;
        }
    }
}