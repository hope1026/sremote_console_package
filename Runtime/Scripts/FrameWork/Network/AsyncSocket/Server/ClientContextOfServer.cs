// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Net.Sockets;

namespace SPlugin.FrameWork.Network
{
    internal class ClientContextOfServer
    {
        private static int _instanceCount = 0;
        public int ClientId { get; private set; }
        internal AsyncSocket AsyncSocket { get; private set; }
        internal Action<ClientContextOfServer> OnRequestShoutDownDelegate { get; set; }
        public ClientContextOfServer()
        {
            ClientId = _instanceCount++;
            AsyncSocket = new AsyncSocket();
            if (_instanceCount >= int.MaxValue)
            {
                _instanceCount = 0;
            }
        }

        public void Initialize(Socket socket_)
        {
            AsyncSocket.Initialize(socket_);
            AsyncSocket.RequestShoutDownDelegate = RequestShoutDown;
        }

        public void Terminate()
        {
            if (null != AsyncSocket)
            {
                AsyncSocket.Terminate();
                AsyncSocket = null;
            }
        }

        private void RequestShoutDown()
        {
            if (null != OnRequestShoutDownDelegate)
            {
                OnRequestShoutDownDelegate(this);
            }
        }
    }
}