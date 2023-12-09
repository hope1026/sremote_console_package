// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Text;
using UnityEngine;

namespace SPlugin
{
    internal abstract class PacketAbstract
    {
        public enum Type
        {
            SYNC_ACTIVE = 0,
            COMMAND,
            COMMAND_REGISTER,
            LOG,
            PAUSE_PLAYING,
            PREFERENCES,
            PROFILE_INFO,
            SYSTEM_INFO,
            SYSTEM_LOG,
        }

        const int DEFAULT_ARRAY_SIZE = 256;

        private int _offset = 0;
        public byte[] ByteArrayBuffer { get; private set; }
        public int PacketSize { get; private set; } = 0;
        public abstract Type PacketType { get; }

        protected PacketAbstract()
        {
            _offset = 0;
        }

        public void SetData(byte[] buffer_)
        {
            ByteArrayBuffer = buffer_;
        }

        public bool Write()
        {
            try
            {
                ByteArrayBuffer = new byte[DEFAULT_ARRAY_SIZE];
                _offset = 0;
                WriteByte((byte)PacketType);
                OnWrite();
                PacketSize = _offset;
                return true;
            }
            catch (Exception)
            {
                PacketSize = 0;
                return false;
            }
        }

        public bool Read()
        {
            try
            {
                _offset = 0;
                ReadByte();
                OnRead();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }

        protected abstract void OnWrite();
        protected abstract void OnRead();

        protected void WriteBool(bool value_)
        {
            byte[] tempByteArray = BitConverter.GetBytes(value_);
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteInt(int value_)
        {
            byte[] tempByteArray = BitConverter.GetBytes(value_);
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteByte(byte value_)
        {
            byte[] tempByteArray = { value_ };
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteLong(long value_)
        {
            byte[] tempByteArray = BitConverter.GetBytes(value_);
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteUint(uint value_)
        {
            byte[] tempByteArray = BitConverter.GetBytes(value_);
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteFloat(float value_)
        {
            byte[] tempByteArray = BitConverter.GetBytes(value_);
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteDouble(double value_)
        {
            byte[] tempByteArray = BitConverter.GetBytes(value_);
            AppendBufferAfter(tempByteArray);
        }

        protected void WriteString(string value_)
        {
            if (true == string.IsNullOrEmpty(value_))
            {
                WriteInt(0);
                return;
            }

            byte[] tempByteArray = Encoding.UTF8.GetBytes(value_);
            WriteInt(tempByteArray.Length);

            if (tempByteArray.Length <= 0)
                return;

            AppendBufferAfter(tempByteArray);
        }

        protected bool ReadBool()
        {
            bool result = BitConverter.ToBoolean(ByteArrayBuffer, _offset);
            _offset += sizeof(bool);
            return result;
        }

        protected byte ReadByte()
        {
            byte result = ByteArrayBuffer[_offset];
            _offset += sizeof(byte);
            return result;
        }

        protected int ReadInt()
        {
            int result = BitConverter.ToInt32(ByteArrayBuffer, _offset);
            _offset += sizeof(int);
            return result;
        }

        protected long ReadLong()
        {
            long result = BitConverter.ToInt64(ByteArrayBuffer, _offset);
            _offset += sizeof(long);
            return result;
        }

        protected uint ReadUint()
        {
            uint result = BitConverter.ToUInt32(ByteArrayBuffer, _offset);
            _offset += sizeof(uint);
            return result;
        }

        protected float ReadFloat()
        {
            float result = BitConverter.ToSingle(ByteArrayBuffer, _offset);
            _offset += sizeof(float);
            return result;
        }

        protected double ReadDouble()
        {
            double result = BitConverter.ToDouble(ByteArrayBuffer, _offset);
            _offset += sizeof(double);
            return result;
        }

        protected string ReadString()
        {
            int stringByteLength = ReadInt();
            if (stringByteLength <= 0)
                return string.Empty;

            string result = Encoding.UTF8.GetString(ByteArrayBuffer, _offset, stringByteLength);
            _offset += stringByteLength;
            return result;
        }

        private void AppendBufferAfter(byte[] appendByteArrayData_)
        {
            int destinationSize = _offset + appendByteArrayData_.Length;
            CheckBufferSize(destinationSize);
            Array.Copy(appendByteArrayData_, 0, ByteArrayBuffer, _offset, appendByteArrayData_.Length);
            _offset += appendByteArrayData_.Length;
        }

        private void AppendBufferBefore(byte[] appendByteArrayData_)
        {
            int destinationSize = _offset + appendByteArrayData_.Length;
            CheckBufferSize(destinationSize);

            byte[] tempByteArray = new byte[ByteArrayBuffer.Length];
            Array.Copy(appendByteArrayData_, 0, tempByteArray, 0, appendByteArrayData_.Length);
            Array.Copy(ByteArrayBuffer, 0, tempByteArray, appendByteArrayData_.Length, _offset);
            ByteArrayBuffer = tempByteArray;
            _offset += appendByteArrayData_.Length;
        }

        private void CheckBufferSize(int destinationSize_)
        {
            if (destinationSize_ > ByteArrayBuffer.Length)
            {
                int magnification = (destinationSize_ / ByteArrayBuffer.Length) + 1;
                byte[] tempByteArray = ByteArrayBuffer;
                ByteArrayBuffer = new byte[ByteArrayBuffer.Length * magnification];
                Array.Copy(tempByteArray, ByteArrayBuffer, tempByteArray.Length);
                tempByteArray = null;
            }
        }
    }
}