// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;

namespace SPlugin.FrameWork.Network
{
    internal class SocketBufferPool
    {
        public class Buffer
        {
            public const int DEFAULT_CAPACITY = 1024;

            public Buffer(int capacity_)
            {
                ByteArray = new byte[capacity_];
                ByteArrayLength = 0;
                SendOffset = 0;
                SendSize = 0;
            }

            public void Reset()
            {
                ByteArrayLength = 0;
                SendOffset = 0;
                SendSize = 0;
            }

            public byte[] ByteArray { get; private set; }
            public int ByteArrayLength { get; set; }
            public int SendOffset { get; set; }
            public int SendSize { get; set; }
        }

        private readonly Queue<Buffer> _bufferQueue = new Queue<Buffer>();
        private UInt16 _bufferCapacity = Buffer.DEFAULT_CAPACITY;

        #region SingleTon

        private static SocketBufferPool _instance = null;

        public static SocketBufferPool Instance()
        {
            if (null == _instance)
            {
                _instance = new SocketBufferPool();
            }

            return _instance;
        }

        #endregion /*SingleTon*/

        public void Initialize(UInt32 capacity_, UInt16 bufferSize_)
        {
            _bufferQueue.Clear();
            for (int index = 0; index < capacity_; index++)
            {
                _bufferQueue.Enqueue(new Buffer(bufferSize_));
            }

            _bufferCapacity = bufferSize_;
        }

        public Buffer Pop()
        {
            if (_bufferQueue.Count <= 0)
            {
                _bufferQueue.Enqueue(new Buffer(_bufferCapacity));
            }

            Buffer buffer = _bufferQueue.Dequeue();
            if (null != buffer)
            {
                buffer.Reset();
            }

            return buffer;
        }

        public void Trash(Buffer buffer_)
        {
            if (null != buffer_)
            {
                buffer_.Reset();
                _bufferQueue.Enqueue(buffer_);
            }
        }
    }
}