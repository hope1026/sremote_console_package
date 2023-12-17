// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;

namespace SPlugin
{
    internal class LocalEditorApp : AppAbstract
    {
        public override bool IsLocalEditor => true;
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private bool _canStep = false;

        public void Initialize()
        {
            EditorApplication.playModeStateChanged -= OnChangePlayModeHandler;
            EditorApplication.playModeStateChanged += OnChangePlayModeHandler;

            EditorApplication.pauseStateChanged -= OnChangePauseStateHandler;
            EditorApplication.pauseStateChanged += OnChangePauseStateHandler;

            base.ip = SConsoleNetworkUtil.GetLocalIpAddress();
            base.ipAddressString = $"{base.ip}:LocalEditor";
            _isPlaying = false;
            ChangeState(AppConnectionStateType.CONNECTED);
        }

        protected override void OnActivate()
        {
            SendSyncActive(isActiveSync_: true);
            SendPreferences();
        }

        protected override void OnDeActivate()
        {
            SendSyncActive(isActiveSync_: false);
        }

        protected override void OnUpdateCustom()
        {
            if (0 < RemoteConsoleLocalEditorBridge.Instance.PacketsForEditor.Count)
            {
                receivedPacketList.InsertRange(0, RemoteConsoleLocalEditorBridge.Instance.PacketsForEditor);
                RemoteConsoleLocalEditorBridge.Instance.PacketsForEditor.Clear();
            }
            ProcessAllReceivedPackets();

            if (_isPaused != EditorApplication.isPaused)
            {
                EditorApplication.isPaused = _isPaused;
            }

            if (_canStep)
            {
                _canStep = false;
                EditorApplication.Step();
            }
        }

        protected override void SendPacket(PacketAbstract packet_)
        {
            if (null != packet_ && packet_.Write())
            {
                RemoteConsoleLocalEditorBridge.Instance.SendToApplication(packet_);
                if (packet_ is PausePlayingPacket pausePlayingPacket && IsActivated)
                {
                    _isPaused = pausePlayingPacket.IsPause;
                    _canStep = pausePlayingPacket.CanStep;
                }
            }
        }

        public override bool IsPlaying()
        {
            return _isPlaying && IsActivated;
        }

        private void OnChangePlayModeHandler(PlayModeStateChange playMode_)
        {
            if (playMode_ == PlayModeStateChange.EnteredPlayMode)
            {
                _isPlaying = true;
            }
            else
            {
                _isPlaying = false;
                commandCollection.RemoveAllItems();
                ChangeState(AppConnectionStateType.CONNECTED);
            }
        }

        private void OnChangePauseStateHandler(PauseState pauseState_)
        {
            if (IsActivated == false)
                return;

            bool isPause = pauseState_ == PauseState.Paused ? true : false;
            SendPause(isPause, canStep_: false, errorPause_: false);
        }
    }
}