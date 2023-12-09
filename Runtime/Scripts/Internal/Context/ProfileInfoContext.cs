// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
	internal class ProfileInfoContext
	{
		public float FramePerSecond { set; get; }
		public Int64 UsedHeapSize { set; get; }

		public void ResetFromOtherContext(ProfileInfoContext otherContext_)
		{
			if (otherContext_ != null)
			{
				FramePerSecond = otherContext_.FramePerSecond;
				UsedHeapSize = otherContext_.UsedHeapSize;
			}
		}
	}
}

