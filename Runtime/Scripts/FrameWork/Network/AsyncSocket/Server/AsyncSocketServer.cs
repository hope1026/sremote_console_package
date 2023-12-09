// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Object = System.Object;

namespace SPlugin.FrameWork.Network
{
    internal class AsyncSocketServer
    {
        private const string SYSTEM_MESSAGE_PREFIX = "[SERVER]";

        enum StateType
        {
            STARTING = 0,
            STARTING_COMPLETED,
            SHOUT_DOWN,
        }

        private AsyncSocket _asyncSocket = null;
        private StateType _stateType = StateType.SHOUT_DOWN;
        private readonly List<ClientContextOfServer> _clientContextList = new List<ClientContextOfServer>();
        private int _maxConnectCount = 100;
        public Action<LogType, string> NetworkLogDelegate { get; set; }
        public Action<byte[]> DelegateOnReceive { get; set; }
        public Action<SocketServerEventType> OnServerEventDelegate { get; set; }
        public int Port { get; private set; }

        public bool StartServer(int startPort_, int endPort_, int maxConnection_)
        {
            _stateType = StateType.STARTING;
            bool success = false;
            int port = startPort_;
            do
            {
                try
                {
                    success = OpenSocket(port, maxConnection_);
                    if (true == success)
                        break;
                }
                catch (SocketException socketException)
                {
                    if (socketException.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        WriteNetworkLog(LogType.Warning, SYSTEM_MESSAGE_PREFIX + "Start Failed:Increase Port And Restart Server");
                        port++;
                    }
                    else
                    {
                        WriteNetworkLog(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}Start Failed:SocketException:{socketException.SocketErrorCode}\n{socketException}");
                        CloseSocket();
                        break;
                    }
                }
                catch (Exception exception)
                {
                    WriteNetworkLog(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}OpenSocket Exception {exception}");
                    CloseSocket();
                    break;
                }
            } while (success == false && port <= endPort_);

            return success;
        }

        private bool OpenSocket(int serverPort_, int maxConnection_)
        {
            Port = serverPort_;
            _maxConnectCount = maxConnection_;
            WriteNetworkLog(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Start Port:{serverPort_} MaxConnection:{maxConnection_}");
            if (null == _asyncSocket)
            {
                _asyncSocket = new AsyncSocket();
                _asyncSocket.Initialize(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                _asyncSocket.SystemMessagePrefix = SYSTEM_MESSAGE_PREFIX;
                _asyncSocket.NetworkLogDelegate = WriteNetworkLog;
            }

            _asyncSocket.Reset();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
            _asyncSocket.Socket.Bind(ipEndPoint);
            _asyncSocket.Socket.Listen(_maxConnectCount);
            _asyncSocket.Socket.BeginAccept(OnAcceptHandler, _asyncSocket);

            ProcessNetworkEvent(SocketServerEventType.SERVER_OPENED);
            WriteNetworkLog(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Start Succeed");
            _stateType = StateType.STARTING_COMPLETED;
            return true;
        }

        public void CloseSocket()
        {
            foreach (ClientContextOfServer clientContext in _clientContextList)
            {
                if (null != clientContext)
                {
                    clientContext.Terminate();
                }
            }

            _clientContextList.Clear();
            if (null != _asyncSocket)
            {
                _asyncSocket.Terminate();
                _asyncSocket = null;
            }

            ProcessNetworkEvent(SocketServerEventType.SERVER_CLOSED);
            WriteNetworkLog(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}CloseSocket");
            _stateType = StateType.SHOUT_DOWN;
        }

        public bool IsShoutDown()
        {
            if (null == _asyncSocket)
            {
                return true;
            }

            return false;
        }

        private void OnAcceptHandler(IAsyncResult asyncResult_)
        {
            if (null == _asyncSocket || null == _asyncSocket.Socket)
            {
                if (StateType.SHOUT_DOWN != _stateType)
                {
                    WriteNetworkLog(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}AcceptCallback Socket == NULL");
                    CloseSocket();
                }

                return;
            }

            Socket tempClientSocket = null;
            try
            {
                tempClientSocket = _asyncSocket.Socket.EndAccept(asyncResult_);
            }
            catch (SocketException socketException)
            {
                WriteNetworkLog(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}AcceptCallback :SocketException:{socketException.SocketErrorCode}\n{socketException}");
                tempClientSocket = null;
            }
            catch (Exception exception)
            {
                WriteNetworkLog(LogType.Error, $"{SYSTEM_MESSAGE_PREFIX}AcceptCallback Exception {exception}");
                tempClientSocket = null;
            }

            if (null != tempClientSocket)
            {
                ClientContextOfServer tempClientContextOfServer = new ClientContextOfServer();
                tempClientContextOfServer.Initialize(tempClientSocket);
                tempClientContextOfServer.OnRequestShoutDownDelegate = RemoveClient;
                _clientContextList.Add(tempClientContextOfServer);

                tempClientContextOfServer.AsyncSocket.StartReceive(OnReceiveHandler, tempClientContextOfServer);

                IPEndPoint ipEndPoint = tempClientContextOfServer.AsyncSocket.Socket.RemoteEndPoint as IPEndPoint;
                string remoteIP = string.Empty;
                if (null != ipEndPoint)
                {
                    remoteIP = $"{ipEndPoint.Address}:{ipEndPoint.Port}";
                }

                WriteNetworkLog(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Accept Client:{remoteIP}");
                ProcessNetworkEvent(SocketServerEventType.CLIENT_CONNECTED);
            }

            _asyncSocket.Socket.BeginAccept(OnAcceptHandler, _asyncSocket.Socket);
        }

        private void OnReceiveHandler(byte[] byteArray_, Object stateObject_)
        {
            if (stateObject_ is ClientContextOfServer)
            {
                if (null != DelegateOnReceive)
                {
                    DelegateOnReceive(byteArray_);
                }
            }
        }

        public void SendToAllClients(byte[] byteArray_, int size_)
        {
            foreach (ClientContextOfServer clientData in _clientContextList)
            {
                SendToClient(clientData, byteArray_, size_);
            }
        }

        private void SendToClient(ClientContextOfServer clientContextOfServer_, byte[] byteArray_, int size_)
        {
            if (null != clientContextOfServer_)
            {
                clientContextOfServer_.AsyncSocket.Send(byteArray_, size_);
            }
        }

        private void RemoveClient(ClientContextOfServer clientContextOfServer_)
        {
            if (null != clientContextOfServer_)
            {
                string remoteIP = string.Empty;
                if (clientContextOfServer_.AsyncSocket.Socket.RemoteEndPoint is IPEndPoint ipEndPoint)
                {
                    remoteIP = $"{ipEndPoint.Address}:{ipEndPoint.Port}";
                }

                WriteNetworkLog(LogType.Log, $"{SYSTEM_MESSAGE_PREFIX}Remove Client:{remoteIP}");

                clientContextOfServer_.Terminate();
                _clientContextList.Remove(clientContextOfServer_);
                ProcessNetworkEvent(SocketServerEventType.CLIENT_DISCONNECTED);
            }
        }

        private void ProcessNetworkEvent(SocketServerEventType eventType_)
        {
            if (null != OnServerEventDelegate)
            {
                OnServerEventDelegate(eventType_);
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
            if (_asyncSocket != null)
            {
                return _asyncSocket.TryGetLocalIpAndPort(out outIp_, out outPort_);
            }
            outIp_ = string.Empty;
            outPort_ = 0;
            return false;
        }
    }
}