// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using SPlugin.FrameWork.Network;
using UnityEngine;

namespace SPlugin
{
    internal class SConsoleNetwork
    {
        private AsyncSocketServer _socketServer = null;
        private readonly List<PacketAbstract> _receivePacketList = new List<PacketAbstract>();
        internal Action<PacketAbstract> OnReceivePacketDelegate { get; set; }
        public Action<SocketServerEventType> OnNetworkEventDelegate { get; set; }
        public Action<LogType, string> OnNetworkMessageDelegate { get; set; }

        public void StartHost()
        {
            if (_socketServer == null)
            {
                _socketServer = new AsyncSocketServer();
                _socketServer.DelegateOnReceive = OnReceiveHandler;
                _socketServer.OnServerEventDelegate = OnNetworkSocketServerEventHandler;
                _socketServer.NetworkLogDelegate = OnNetworkMessageHandler;
            }

            _socketServer.StartServer(SConsoleNetworkUtil.PORT_MIN, SConsoleNetworkUtil.PORT_MAX, SConsoleNetworkUtil.DEFAULT_MAX_CONNECTION);
        }

        public void End()
        {
            if (null != _socketServer)
            {
                _socketServer.CloseSocket();
                _socketServer = null;
            }
        }

        private void OnReceiveHandler(byte[] receiveByteArray_)
        {
            if (null != receiveByteArray_)
            {
                PacketAbstract.Type type = (PacketAbstract.Type)receiveByteArray_[0];

                PacketAbstract packet = PacketFactory.Instance(type);
                if (null != packet)
                {
                    packet.SetData(receiveByteArray_);
                    packet.Read();
                    if (packet.PacketType == PacketAbstract.Type.PAUSE_PLAYING)
                    {
                        ProcessPacket(packet);
                    }
                    else
                    {
                        lock (_receivePacketList)
                        {
                            _receivePacketList.Add(packet);
                        }
                    }
                }
            }
        }

        private void ProcessPacket(PacketAbstract packet_)
        {
            if (null != packet_)
            {
                OnReceivePacketDelegate?.Invoke(packet_);
            }
        }

        public void SendPacket(PacketAbstract packet_)
        {
            if (null != packet_ && packet_.Write())
            {
                RemoteConsoleToLocalApplicationBridge.Instance.SendToRuntimeConsoleApp(packet_);
                if (Application.isEditor)
                {
                    RemoteConsoleToLocalApplicationBridge.Instance.SendToEditorConsoleApp(packet_);
                }
                else if (null != _socketServer)
                {
                    _socketServer.SendToAllClients(packet_.ByteArrayBuffer, packet_.PacketSize);
                }
            }
        }

        public void Update()
        {
            lock (_receivePacketList)
            {
                if (0 < RemoteConsoleToLocalApplicationBridge.Instance.PacketsForConsoleMain.Count)
                {
                    _receivePacketList.InsertRange(0, RemoteConsoleToLocalApplicationBridge.Instance.PacketsForConsoleMain);
                    RemoteConsoleToLocalApplicationBridge.Instance.PacketsForConsoleMain.Clear();
                }

                try
                {
                    foreach (PacketAbstract packet in _receivePacketList)
                    {
                        ProcessPacket(packet);
                    }
                }
                catch (Exception exception)
                {
                    RemoteConsoleInternalLog.Exception(exception);
                }

                _receivePacketList.Clear();
            }
        }

        private void OnNetworkMessageHandler(LogType logType_, string log_)
        {
            if (null != OnNetworkMessageDelegate)
            {
                OnNetworkMessageDelegate(logType_, log_);
            }
        }

        private void OnNetworkSocketServerEventHandler(SocketServerEventType eventType_)
        {
            if (null != OnNetworkEventDelegate)
            {
                OnNetworkEventDelegate(eventType_);
            }
        }
    }
}