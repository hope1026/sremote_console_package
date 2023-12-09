// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Object = System.Object;

namespace SPlugin.FrameWork.Network
{
    internal class AsyncSocketClient
    {
        private const string SYSTEM_MESSAGE_PREFIX = "[CLIENT]";

        public enum StateType
        {
            CONNECTING = 0,
            CONNECTED,
            SHOUT_DOWN,
        }

        private AsyncSocket _asyncSocket = null;
        private StateType _state = StateType.SHOUT_DOWN;
        private int _connectableEndPort;
        Action<LogType, string> _networkSystemMessageDelegate;

        public int Port { get; private set; }
        public string ServerIp { get; private set; }
        public Action<byte[]> OnReceiveDelegate { get; set; }
        public Action<SocketClientEventType> OnNetworkEventDelegate { get; set; }

        public bool ConnectToServer(string ipAddress_, int startPort_, int endPort_)
        {
            _state = StateType.CONNECTING;
            ServerIp = ipAddress_;
            Port = startPort_;
            _connectableEndPort = endPort_;
            ProcessNetworkEvent(SocketClientEventType.CONNECT_START);
            SocketBufferPool.Instance().Initialize(1, SocketBufferPool.Buffer.DEFAULT_CAPACITY);

            return TryConnect(startPort_);
        }

        private bool TryConnect(int startPort_)
        {
            Port = startPort_;
            SystemMessage(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}ConnectStart: IP:{ServerIp} Port:{Port}");

            if (null == _asyncSocket)
            {
                _asyncSocket = new AsyncSocket();
                Socket tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _asyncSocket.Initialize(tempSocket);
                _asyncSocket.NetworkLogDelegate = OnAsyncSocketNetworkLogHandler;
                _asyncSocket.SystemMessagePrefix = SYSTEM_MESSAGE_PREFIX;
                _asyncSocket.RequestShoutDownDelegate = OnAsyncSocketRequestShoutDownHandler;
            }

            try
            {
                _asyncSocket.Reset();
                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), Port);
                _asyncSocket.Socket.BeginConnect(remoteIPEndPoint, OnConnectHandler, _asyncSocket.Socket);
            }
            catch (SocketException socketException)
            {
                SystemMessage(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}Connect Failed SocketException:{socketException.SocketErrorCode}\n{socketException}");
                ShoutDown();
                return false;
            }
            catch (Exception exception)
            {
                SystemMessage(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}_ConnectServer Exception {exception}");
                ShoutDown();
                return false;
            }

            return true;
        }

        private void OnAsyncSocketRequestShoutDownHandler()
        {
            ShoutDown();
        }

        public void ShoutDown()
        {
            _state = StateType.SHOUT_DOWN;
            if (null != _asyncSocket)
            {
                _asyncSocket.Terminate();
                _asyncSocket = null;
            }

            ProcessNetworkEvent(SocketClientEventType.STOP_OR_DISCONNECTED);
            SystemMessage(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}CloseSocket");
        }

        private void OnConnectHandler(IAsyncResult asyncResult_)
        {
            try
            {
                _asyncSocket.Socket.EndConnect(asyncResult_);
                _asyncSocket.StartReceive(OnReceiveHandler, null);
                _state = StateType.CONNECTED;
            }
            catch (SocketException socketException)
            {
                if ((socketException.SocketErrorCode == SocketError.ConnectionRefused || socketException.SocketErrorCode == SocketError.TimedOut) &&
                    Port < _connectableEndPort)
                {
                    SystemMessage(LogType.Warning, $"{SYSTEM_MESSAGE_PREFIX}Connect Failed:Increase Port And Reconnect Server");
                    Port++;
                    ProcessNetworkEvent(SocketClientEventType.CONNECT_START_TO_NEXT_PORT);
                    TryConnect(Port);
                }
                else
                {
                    ShoutDown();
                    SystemMessage(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}Connect Failed SocketException:{socketException.SocketErrorCode}\n{socketException}");
                }

                return;
            }
            catch (Exception exception)
            {
                SystemMessage(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}OnConnectHandler Exception {exception}");
                ShoutDown();
                return;
            }

            if (_asyncSocket.Socket.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                SystemMessage(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Connect Succeed IPEndPoint:{ipEndPoint.Address}:{ipEndPoint.Port}");
            }
            else
            {
                SystemMessage(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Connect Succeed IP:{ServerIp} Port:{Port}");
            }

            SystemMessage(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Connect Connected :{_asyncSocket.Socket.Connected}");

            ProcessNetworkEvent(SocketClientEventType.CONNECT_COMPLETED);
        }

        private void OnReceiveHandler(byte[] byteArray_, Object stateObject_)
        {
            if (null != OnReceiveDelegate)
            {
                OnReceiveDelegate(byteArray_);
            }
        }

        public void SendToServer(byte[] byteArray_, int size_)
        {
            if (null != _asyncSocket)
            {
                _asyncSocket.Send(byteArray_, size_);
            }
        }

        public bool IsShoutDown()
        {
            if (StateType.SHOUT_DOWN == _state)
                return true;

            return false;
        }

        private void OnAsyncSocketNetworkLogHandler(LogType logType_, string log_)
        {
            SystemMessage(logType_, log_);
        }

        private void SystemMessage(LogType logType_, string log_)
        {
            if (null != _networkSystemMessageDelegate)
            {
                _networkSystemMessageDelegate(logType_, log_);
            }
        }

        public void RegisterNetworkSystemMessageHandler(Action<LogType, string> delegate_)
        {
            _networkSystemMessageDelegate = delegate_;
        }

        private void ProcessNetworkEvent(SocketClientEventType eventType_)
        {
            if (null != OnNetworkEventDelegate)
            {
                OnNetworkEventDelegate(eventType_);
            }
        }
    }
}