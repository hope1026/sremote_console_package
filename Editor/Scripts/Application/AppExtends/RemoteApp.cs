// 
// Copyright 2015 https://github.com/hope1026

using System;
using SPlugin.FrameWork.Network;

namespace SPlugin
{
    internal class RemoteApp : AppAbstract
    {
        public override bool IsLocalEditor => false;
        private readonly AsyncSocketClient _asyncSocketClient = new AsyncSocketClient();
        private bool _requestActiveSync = false;
        public int TriedConnectingCount { get; private set; }
        public int MaxConnectingCountForDisplay { get; private set; }
        private int _maxConnectingCount;
        private bool _requestDisconnectedNotification = false;

        public void Initialize()
        {
            _asyncSocketClient.OnReceiveDelegate = OnReceiveHandler;
            _asyncSocketClient.OnNetworkEventDelegate = OnNetworkEventHandler;
            _requestActiveSync = false;
        }

        protected override void OnActivate()
        {
            if (HasConnected())
                _requestActiveSync = true;

            string message = $"Activate:{systemInfoContext.DeviceName}-{IpAddressString}";
            ConsoleViewMain.Instance.ShowNotification(message);
        }

        protected override void OnDeActivate()
        {
            SendSyncActive(isActiveSync_: false);
        }

        protected override void OnConnect()
        {
            ChangeState(AppConnectionStateType.CONNECTING);
            base.ipAddressString = $"{base.ip}";
            TriedConnectingCount = 0;
            _maxConnectingCount = SConsoleNetworkUtil.PORT_MAX - SConsoleNetworkUtil.PORT_MIN + 1;
            MaxConnectingCountForDisplay = 5;
            _asyncSocketClient.ConnectToServer(base.ip, SConsoleNetworkUtil.PORT_MIN, SConsoleNetworkUtil.PORT_MAX);
        }

        protected override void OnDisconnect()
        {
            commandCollection.RemoveAllItems();
            ChangeState(AppConnectionStateType.DISCONNECTED);
            _asyncSocketClient.ShoutDown();
            TriedConnectingCount = 0;
            MaxConnectingCountForDisplay = 0;
            _maxConnectingCount = 0;
        }

        protected override void SendPacket(PacketAbstract packet_)
        {
            if (null != packet_ && _asyncSocketClient != null && packet_.Write())
            {
                _asyncSocketClient.SendToServer(packet_.ByteArrayBuffer, packet_.PacketSize);
            }
        }

        protected override void OnUpdateCustom()
        {
            if (_requestActiveSync)
            {
                SendSyncActive(isActiveSync_: true);
                SendPreferences();
                _requestActiveSync = false;
            }
            ProcessAllReceivedPackets();

            if (_requestDisconnectedNotification)
            {
                _requestDisconnectedNotification = false;
                string message = $"Disconnected:{systemInfoContext.DeviceName}-{IpAddressString}";
                ConsoleViewMain.Instance.ShowNotification(message);
            }
        }

        public override bool IsPlaying()
        {
            return IsActivated && base.AppConnectionStateType == AppConnectionStateType.CONNECTED;
        }

        private void OnReceiveHandler(byte[] receivedBytes_)
        {
            if (null != receivedBytes_)
            {
                PacketAbstract.Type type = (PacketAbstract.Type)receivedBytes_[0];
                PacketAbstract packet = PacketFactory.Instance(type);
                if (null != packet)
                {
                    packet.SetData(receivedBytes_);
                    if (packet.Read())
                    {
                        lock (receivedPacketList)
                        {
                            receivedPacketList.Add(packet);
                        }
                    }
                }
            }
        }

        private void OnNetworkEventHandler(SocketClientEventType socketClientEventType_)
        {
            if (socketClientEventType_ == SocketClientEventType.CONNECT_COMPLETED)
            {
                ChangeState(AppConnectionStateType.CONNECTED);
                base.port = _asyncSocketClient.Port;
                TriedConnectingCount = MaxConnectingCountForDisplay;
                base.ipAddressString = $"{base.ip}:{base.port}";
                if (base.IsActivated)
                    _requestActiveSync = true;
            }
            else if (socketClientEventType_ == SocketClientEventType.STOP_OR_DISCONNECTED)
            {
                if (HasConnected())
                    _requestDisconnectedNotification = true;

                ChangeState(AppConnectionStateType.DISCONNECTED);
                TriedConnectingCount = MaxConnectingCountForDisplay;
            }
            else if (socketClientEventType_ == SocketClientEventType.CONNECT_START ||
                     socketClientEventType_ == SocketClientEventType.CONNECT_START_TO_NEXT_PORT)
            {
                ChangeState(AppConnectionStateType.CONNECTING);
                TriedConnectingCount = _asyncSocketClient.Port - SConsoleNetworkUtil.PORT_MIN + 1;
                if (MaxConnectingCountForDisplay - 2 <= TriedConnectingCount)
                {
                    MaxConnectingCountForDisplay = Math.Min(_maxConnectingCount, MaxConnectingCountForDisplay + 10);
                }
            }
        }
    }
}