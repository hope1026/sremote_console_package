// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;

namespace SPlugin.FrameWork.Network
{
    internal class ReceiveRingBuffer
    {
        public void Append(byte[] byteArray_, int count_)
        {
            int destinationSize = _offset + count_;
            CheckBufferSize(destinationSize);
            Buffer.BlockCopy(byteArray_, 0, _buffer, _offset, count_);
            _offset += count_;
        }

        public byte[] PopPacketBody()
        {
            if (0 == _bodySize && _offset >= AbstractPacket.Header.SIZE)
            {
                _bodySize = BitConverter.ToUInt16(_buffer, 0);
            }

            if (_bodySize > 0 && _offset >= _bodySize + AbstractPacket.Header.SIZE)
            {
                byte[] tempByteArray = new byte[_bodySize];

                Buffer.BlockCopy(_buffer, AbstractPacket.Header.SIZE, tempByteArray, 0, _bodySize);
                _offset = Mathf.Max(0, _offset - _bodySize - AbstractPacket.Header.SIZE);

                Buffer.BlockCopy(_buffer, _bodySize + AbstractPacket.Header.SIZE, _buffer, 0, _offset);
                _bodySize = 0;
                return tempByteArray;
            }

            return null;
        }

        private void CheckBufferSize(int destinationSize_)
        {
            if (destinationSize_ > _buffer.Length)
            {
                int magnification = (destinationSize_ / _buffer.Length) + 1;
                byte[] tempByteArray = _buffer;
                _buffer = new byte[_buffer.Length * magnification];
                Buffer.BlockCopy(tempByteArray, 0, _buffer, 0, tempByteArray.Length);
                tempByteArray = null;
            }
        }

        private byte[] _buffer = new byte[1024];
        private int _offset = 0;
        private int _bodySize = 0;
    }
}