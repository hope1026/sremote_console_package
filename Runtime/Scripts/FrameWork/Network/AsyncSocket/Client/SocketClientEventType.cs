// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin.FrameWork.Network
{
    internal enum SocketClientEventType
    {
        CONNECT_START = 0,
        CONNECT_START_TO_NEXT_PORT,
        CONNECT_COMPLETED,
        STOP_OR_DISCONNECTED,
    }
}