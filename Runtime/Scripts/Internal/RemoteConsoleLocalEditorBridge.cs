// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;

namespace SPlugin
{
    internal class RemoteConsoleLocalEditorBridge
    {
        #region SINGLETON

        private static RemoteConsoleLocalEditorBridge _instance = null;

        public static RemoteConsoleLocalEditorBridge Instance
        {
            get
            {
                if (null != _instance) { return _instance; }
                else { return (_instance = new RemoteConsoleLocalEditorBridge()); }
            }
        }

        #endregion

        public Queue<PacketAbstract> PacketsForEditor { get; } = new Queue<PacketAbstract>();
        public Queue<PacketAbstract> PacketsForApplication { get; } = new Queue<PacketAbstract>();
        private Action _updateInEditorDelegate;

        public void SendToEditor(PacketAbstract packet_)
        {
            PacketsForEditor.Enqueue(packet_);
        }

        public void SendToApplication(PacketAbstract packet_)
        {
            PacketsForApplication.Enqueue(packet_);
        }

        public void UpdateInEditor()
        {
            _updateInEditorDelegate?.Invoke();
        }

        public void RegisterUpdateInEditorDelegate()
        {
            _updateInEditorDelegate = SDebug.ConsoleMain.UpdateCustom;
        }

        public void UnRegisterUpdateInEditorDelegate()
        {
            _updateInEditorDelegate = null;
        }

        public void OnStartInEditor()
        {
            SDebug.ConsoleMain.OnStartIfLocalEditor();
        }
    }
}