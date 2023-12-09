// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin.FrameWork.Network
{
    internal class AbstractPacket
    {
        public class Header
        {
            public const int SIZE = sizeof(UInt16);

            public void SetBodySize(UInt16 bodySize_)
            {
                ByteArray = BitConverter.GetBytes(bodySize_);
                BodySize = bodySize_;
            }

            public UInt16 BodySize { get; private set; }
            public byte[] ByteArray { get; private set; }
        }

        public Header PacketHeader { get; private set; } = new Header();
        public byte[] BodyByteArray { get; private set; }

        public void SetBody(byte[] bodyData_, UInt16 bodySize_)
        {
            PacketHeader.SetBodySize(bodySize_);
            BodyByteArray = bodyData_;
        }
    }
}