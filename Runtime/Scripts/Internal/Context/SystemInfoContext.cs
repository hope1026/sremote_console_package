// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.Rendering;

namespace SPlugin
{
    internal class SystemInfoContext
    {
        public string DeviceUniqueIdentifier { get; set; } = string.Empty;
        public string ProductName { set; get; }
        public string OperatingSystem { set; get; }
        public RuntimePlatform RuntimePlatform { set; get; }
        public string DeviceName { set; get; }
        public string DeviceModel { set; get; }
        public int ProcessorCount { set; get; }
        public int SystemMemorySize { set; get; }
        public string GraphicsDeviceName { set; get; }
        public GraphicsDeviceType GraphicsDeviceType { set; get; }
        public int GraphicsMemorySize { set; get; }
        public int MaxTextureSize { set; get; }
        public bool IsDevelopmentBuild { set; get; }

        public void ResetFromOtherContext(SystemInfoContext otherSystemInfoContext_)
        {
            if (otherSystemInfoContext_ == null)
                return;

            DeviceUniqueIdentifier = otherSystemInfoContext_.DeviceUniqueIdentifier;
            ProductName = otherSystemInfoContext_.ProductName;
            OperatingSystem = otherSystemInfoContext_.OperatingSystem;
            RuntimePlatform = otherSystemInfoContext_.RuntimePlatform;
            DeviceName = otherSystemInfoContext_.DeviceName;
            DeviceModel = otherSystemInfoContext_.DeviceModel;
            ProcessorCount = otherSystemInfoContext_.ProcessorCount;
            SystemMemorySize = otherSystemInfoContext_.SystemMemorySize;
            GraphicsDeviceName = otherSystemInfoContext_.GraphicsDeviceName;
            GraphicsDeviceType = otherSystemInfoContext_.GraphicsDeviceType;
            GraphicsMemorySize = otherSystemInfoContext_.GraphicsMemorySize;
            MaxTextureSize = otherSystemInfoContext_.MaxTextureSize;
            IsDevelopmentBuild = otherSystemInfoContext_.IsDevelopmentBuild;
        }
    }
}