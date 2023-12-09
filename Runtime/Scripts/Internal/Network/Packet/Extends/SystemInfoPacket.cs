// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.Rendering;

namespace SPlugin
{
    internal class SystemInfoPacket : PacketAbstract
    {
        public override Type PacketType { get { return Type.SYSTEM_INFO; } }
        public readonly SystemInfoContext context;

        public SystemInfoPacket(SystemInfoContext systemInfoContext_)
        {
            context = systemInfoContext_;
        }

        public SystemInfoPacket()
        {
            context = new SystemInfoContext();
        }

        protected override void OnWrite()
        {
            base.WriteString(context.DeviceUniqueIdentifier);
            base.WriteString(context.ProductName);
            base.WriteString(context.OperatingSystem);
            base.WriteInt((int)context.RuntimePlatform);
            base.WriteString(context.DeviceName);
            base.WriteString(context.DeviceModel);
            base.WriteInt(context.ProcessorCount);
            base.WriteInt(context.SystemMemorySize);
            base.WriteString(context.GraphicsDeviceName);
            base.WriteInt((int)context.GraphicsDeviceType);
            base.WriteInt(context.GraphicsMemorySize);
            base.WriteInt(context.MaxTextureSize);
            base.WriteBool(context.IsDevelopmentBuild);
        }

        protected override void OnRead()
        {
            context.DeviceUniqueIdentifier = base.ReadString();
            context.ProductName = base.ReadString();
            context.OperatingSystem = base.ReadString();
            context.RuntimePlatform = (RuntimePlatform)base.ReadInt();
            context.DeviceName = base.ReadString();
            context.DeviceModel = base.ReadString();
            context.ProcessorCount = base.ReadInt();
            context.SystemMemorySize = base.ReadInt();
            context.GraphicsDeviceName = base.ReadString();
            context.GraphicsDeviceType = (GraphicsDeviceType)base.ReadInt();
            context.GraphicsMemorySize = base.ReadInt();
            context.MaxTextureSize = base.ReadInt();
            context.IsDevelopmentBuild = base.ReadBool();
        }
    }
}