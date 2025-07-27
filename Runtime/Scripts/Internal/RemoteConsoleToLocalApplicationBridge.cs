// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;

namespace SPlugin
{
    internal class RemoteConsoleToLocalApplicationBridge
    {
        #region SINGLETON

        private static RemoteConsoleToLocalApplicationBridge _instance = null;

        public static RemoteConsoleToLocalApplicationBridge Instance
        {
            get
            {
                if (null != _instance) { return _instance; }
                else { return (_instance = new RemoteConsoleToLocalApplicationBridge()); }
            }
        }

        #endregion

        public Queue<PacketAbstract> PacketsForRuntimeConsoleApp { get; } = new Queue<PacketAbstract>();
        public Queue<PacketAbstract> PacketsForEditorConsoleApp { get; } = new Queue<PacketAbstract>();
        public Queue<PacketAbstract> PacketsForConsoleMain { get; } = new Queue<PacketAbstract>();
        private Action _updateInEditorApplicationDelegate;

        public void SendToRuntimeConsoleApp(PacketAbstract packet_)
        {
            PacketsForRuntimeConsoleApp.Enqueue(packet_);
        }
        
        public void SendToEditorConsoleApp(PacketAbstract packet_)
        {
            PacketsForEditorConsoleApp.Enqueue(packet_);
        }

        public void SendToConsoleMain(PacketAbstract packet_)
        {
            PacketsForConsoleMain.Enqueue(packet_);
        }

        public void UpdateInEditor()
        {
            _updateInEditorApplicationDelegate?.Invoke();
        }

        public void RegisterUpdateInEditorApplicationDelegate()
        {
#if !DISABLE_SREMOTE_CONSOLE
            _updateInEditorApplicationDelegate = SDebug.ConsoleMain.UpdateCustom;
#endif
        }

        public void UnRegisterUpdateInEditorApplicationDelegate()
        {
            _updateInEditorApplicationDelegate = null;
        }

        public void OnStartInEditorApplication()
        {
#if !DISABLE_SREMOTE_CONSOLE
            SDebug.ConsoleMain.OnStartIfLocalApplication();
#endif
        }
    }
}