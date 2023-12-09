// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;

namespace SPlugin.FrameWork.Network
{
    internal class SendQueue
    {
        public void Enqueue(byte[] byteArray_, int size_)
        {
            SendPacket packet = new SendPacket();
            packet.SetBody(byteArray_, (UInt16)size_);
            packet.SendOffset = 0;
            _sendPacketQueue.Enqueue(packet);
        }

        public int Dequeue(byte[] destBuffer_)
        {
            if (_sendPacketQueue.Count <= 0)
                return 0;

            int writeSize = 0;
            int destBufferSize = destBuffer_.Length;
            SendPacket tempPacket = _sendPacketQueue.Peek();
            while (null != tempPacket)
            {
                int remainDestBufferSize = destBufferSize - writeSize;
                int packetSize = tempPacket.PacketHeader.BodySize + AbstractPacket.Header.SIZE;

                //패킷 해더 센드 버퍼에 복사
                if (tempPacket.SendOffset == 0)
                {
                    if (AbstractPacket.Header.SIZE > remainDestBufferSize)
                        break;

                    Buffer.BlockCopy(tempPacket.PacketHeader.ByteArray, 0, destBuffer_, writeSize, AbstractPacket.Header.SIZE);
                    tempPacket.SendOffset += AbstractPacket.Header.SIZE;
                    writeSize += AbstractPacket.Header.SIZE;
                    remainDestBufferSize = destBufferSize - writeSize;
                }

                int remainWritePacketSize = packetSize - tempPacket.SendOffset;
                int writeableCount = Math.Min(remainDestBufferSize, remainWritePacketSize);
                if (writeableCount > 0)
                {
                    Buffer.BlockCopy(tempPacket.BodyByteArray, tempPacket.SendOffset - AbstractPacket.Header.SIZE, destBuffer_, writeSize, writeableCount);
                    tempPacket.SendOffset += writeableCount;
                    writeSize += writeableCount;
                    remainDestBufferSize = destBufferSize - writeSize;
                }

                if (true == tempPacket.IsCompletedWriteSendBuffer())
                {
                    _sendPacketQueue.Dequeue();
                }

                if (remainDestBufferSize <= 0 || _sendPacketQueue.Count <= 0)
                    break;

                tempPacket = _sendPacketQueue.Peek();
            }

            return writeSize;
        }

        private Queue<SendPacket> _sendPacketQueue = new Queue<SendPacket>();
    }
}