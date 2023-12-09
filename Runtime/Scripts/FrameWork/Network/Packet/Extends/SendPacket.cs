// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin.FrameWork.Network
{
    internal class SendPacket : AbstractPacket
    {
        public int SendOffset { get; set; }

        public bool IsCompletedWriteSendBuffer()
        {
            if (SendOffset >= base.PacketHeader.BodySize + Header.SIZE)
                return true;

            return false;
        }
    }
}