// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal class PausePlayingContext
    {
        public bool IsPause { get; set; } = false;
        public bool CanStep { get; set; } = false;
        public bool PauseWhenError { get; set; } = true;
    }
}