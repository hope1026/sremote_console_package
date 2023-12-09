// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Object = System.Object;

namespace SPlugin.FrameWork.Network
{
    internal class AsyncSocket
    {
        private SocketBufferPool.Buffer _sendBuffer = null;
        private SocketBufferPool.Buffer _receiveBuffer = null;
        private ReceiveRingBuffer _ringBuffer = null;
        private SendQueue _sendQueue = null;
        private DelegateOnReceivePacket _delegateOnReceive = null;
        private Object _receiveStateObject = null;

        public Socket Socket { get; private set; }
        public Action RequestShoutDownDelegate { get; set; }
        public Action<LogType, string> NetworkLogDelegate { get; set; }
        public string SystemMessagePrefix { get; set; }
        public delegate void DelegateOnReceivePacket(byte[] byteArray_, Object stateObject_);

        public void Initialize(Socket socket_)
        {
            Socket = socket_;
            _receiveBuffer = SocketBufferPool.Instance().Pop();
            _sendBuffer = SocketBufferPool.Instance().Pop();
            _ringBuffer = new ReceiveRingBuffer();
            _sendQueue = new SendQueue();
        }

        public void Terminate()
        {
            if (null != Socket)
            {
                try
                {
                    if (true == Socket.Connected)
                    {
                        Socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception exception)
                {
                    WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}Terminate Exception {exception}");
                }

                Socket.Close();
                Socket = null;
            }

            SocketBufferPool.Instance().Trash(_receiveBuffer);
            _receiveBuffer = null;
        }

        public void Reset()
        {
            if (null != _sendBuffer)
            {
                _sendBuffer.Reset();
            }

            if (null != _receiveBuffer)
            {
                _receiveBuffer.Reset();
            }
        }

        public void StartReceive(DelegateOnReceivePacket delegateOnReceive_, Object stateObject_)
        {
            _delegateOnReceive = delegateOnReceive_;
            _receiveStateObject = stateObject_;
            BeginReceive();
        }

        private void BeginReceive()
        {
            try
            {
                Socket.BeginReceive(_receiveBuffer.ByteArray, 0, _receiveBuffer.ByteArray.Length, 0, out SocketError socketError, OnReceiveDelegate, Socket);
                if (SocketError.Success != socketError)
                {
                    WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}BeginReceive SocketError {socketError.ToString()}");
                    RequestShoutDown();
                }
            }
            catch (SocketException socketException)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}BeginReceive SocketException {socketException.SocketErrorCode}\n{socketException}");
                RequestShoutDown();
            }
            catch (Exception exception)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}BeginReceive Exception {exception}");
                RequestShoutDown();
            }
        }

        private void OnReceiveDelegate(IAsyncResult asyncResult_)
        {
            int receiveSize = 0;
            try
            {
                receiveSize = Socket.EndReceive(asyncResult_);
            }
            catch (SocketException socketException)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}OnReceiveDelegate SocketException {socketException.SocketErrorCode}\n{socketException}");
                RequestShoutDown();
                return;
            }
            catch (Exception exception)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}OnReceiveDelegate Exception {exception}");
                RequestShoutDown();
                return;
            }

            if (receiveSize > 0)
            {
                _ringBuffer.Append(_receiveBuffer.ByteArray, receiveSize);
                byte[] tempBodyByteArray = _ringBuffer.PopPacketBody();
                while (null != tempBodyByteArray)
                {
                    if (null != _delegateOnReceive)
                    {
                        _delegateOnReceive(tempBodyByteArray, _receiveStateObject);
                    }

                    tempBodyByteArray = _ringBuffer.PopPacketBody();
                }

                BeginReceive();
            }
            else
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}OnReceiveDelegate Socket Closed");
                RequestShoutDown();
            }
        }

        public void Send(byte[] byteArray_, int size_)
        {
            _sendQueue.Enqueue(byteArray_, size_);
            if (_sendBuffer.SendSize == 0 && true == Socket.Connected)
            {
                _sendBuffer.ByteArrayLength = _sendQueue.Dequeue(_sendBuffer.ByteArray);
                _sendBuffer.SendSize = _sendBuffer.ByteArrayLength;
                _sendBuffer.SendOffset = 0;
                if (_sendBuffer.SendSize > 0)
                {
                    BeginSend();
                }
            }
        }

        private void BeginSend()
        {
            try
            {
                Socket.BeginSend(_sendBuffer.ByteArray, _sendBuffer.SendOffset, _sendBuffer.SendSize,
                                 SocketFlags.None, out SocketError socketError, OnSendDelegate, Socket);
                if (SocketError.Success != socketError)
                {
                    WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}BeginSend SocketError {socketError}");
                    RequestShoutDown();
                }
            }
            catch (SocketException socketException)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}BeginSend SocketException {socketException.SocketErrorCode}\n{socketException}");
                RequestShoutDown();
            }
            catch (Exception exception)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}BeginSend Exception {exception}");
                RequestShoutDown();
            }
        }

        private void OnSendDelegate(IAsyncResult asyncResult_)
        {
            try
            {
                int sentSize = Socket.EndSend(asyncResult_);
                if (sentSize != _sendBuffer.SendSize)
                {
                    WriteNetworkLog(LogType.Warning, $"AsyncSocket::OnSendDelegate Not Equal SendSize REQ:{_sendBuffer.SendSize} END:{sentSize}");
                    _sendBuffer.SendSize -= sentSize;
                    _sendBuffer.SendOffset += sentSize;
                    BeginSend();
                }
                else
                {
                    _sendBuffer.ByteArrayLength = _sendQueue.Dequeue(_sendBuffer.ByteArray);
                    _sendBuffer.SendSize = _sendBuffer.ByteArrayLength;
                    _sendBuffer.SendOffset = 0;
                    if (_sendBuffer.ByteArrayLength > 0)
                    {
                        BeginSend();
                    }
                }
            }
            catch (SocketException socketException)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}SendCallback SocketException {socketException.SocketErrorCode}\n{socketException}");
                RequestShoutDown();
            }
            catch (Exception exception)
            {
                WriteNetworkLog(LogType.Error, $"{SystemMessagePrefix}SendCallback Exception {exception}");
                RequestShoutDown();
            }
        }

        private void RequestShoutDown()
        {
            if (null != _sendBuffer)
            {
                SocketBufferPool.Instance().Trash(_sendBuffer);
                _sendBuffer.Reset();
                _sendBuffer = null;
            }

            if (null != _receiveBuffer)
            {
                SocketBufferPool.Instance().Trash(_receiveBuffer);
                _receiveBuffer.Reset();
                _receiveBuffer = null;
            }

            if (null != RequestShoutDownDelegate)
            {
                RequestShoutDownDelegate();
            }
        }

        private void WriteNetworkLog(LogType logType_, string log_)
        {
            if (null != NetworkLogDelegate)
            {
                NetworkLogDelegate(logType_, log_);
            }
        }

        public bool TryGetLocalIpAndPort(out string outIp_, out int outPort_)
        {
            if (Socket != null && Socket.LocalEndPoint is IPEndPoint ipEndPoint)
            {
                outIp_ = ipEndPoint.Address.ToString();
                outPort_ = ipEndPoint.Port;
                return true;
            }
            outIp_ = string.Empty;
            outPort_ = 0;
            return false;
        }
        
        public bool TryGetRemoteIpAndPort(out string outIp_, out int outPort_)
        {
            if (Socket != null && Socket.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                outIp_ = ipEndPoint.Address.ToString();
                outPort_ = ipEndPoint.Port;
                return true;
            }
            outIp_ = string.Empty;
            outPort_ = 0;
            return false;
        }
    }
}