// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SPlugin
{
    internal abstract class AppAbstract
    {
        public readonly LogCollection logCollection = new LogCollection();
        public readonly CommandCollection commandCollection = new CommandCollection();
        public readonly SystemInfoContext systemInfoContext = new SystemInfoContext();
        public readonly ProfileInfoContext profileInfoContext = new ProfileInfoContext();
        public Action<AppAbstract> onReceiveErrorLogDelegate = null;
        protected readonly List<PacketAbstract> receivedPacketList = new List<PacketAbstract>();
        protected string ip = string.Empty;
        protected int port = 0;
        protected string ipAddressString = string.Empty;
        public bool IsActivated { get; private set; } = false;
        public AppConnectionStateType AppConnectionStateType { get; private set; } = AppConnectionStateType.NONE;
        public string IpAddressString => ipAddressString;
        public abstract bool IsLocalEditor { get; }

        public void Activate()
        {
            IsActivated = true;
            OnActivate();
        }

        public void DeActivate()
        {
            IsActivated = false;
            OnDeActivate();
        }

        public void Connect(string ip_ = null)
        {
            if (HasConnected())
                return;

            if (string.IsNullOrEmpty(ip_) == false)
                ip = ip_;

            OnConnect();
        }

        public void Disconnect()
        {
            OnDisconnect();
        }

        public void UpdateCustom()
        {
            if (IsActivated)
            {
                foreach (List<CommandItemAbstract> commandList in commandCollection.commandsByCategory.Values)
                {
                    foreach (CommandItemAbstract command in commandList)
                    {
                        if (command.IsDirty == false)
                            continue;

                        CommandPacket commandPacket = new CommandPacket();
                        commandPacket.commandType = command.CommandType;
                        commandPacket.commandCategory = command.Category;
                        commandPacket.commandName = command.Name;
                        commandPacket.commandValue = command.ValueString;
                        SendPacket(commandPacket);
                        command.OnSendCompleted();
                    }
                }
            }
            OnUpdateCustom();
        }

        public abstract bool IsPlaying();

        public bool HasConnected()
        {
            return AppConnectionStateType == AppConnectionStateType.CONNECTED;
        }

        protected void ChangeState(AppConnectionStateType connectionStateType_)
        {
            if (connectionStateType_ != AppConnectionStateType)
            {
                AppConnectionStateType = connectionStateType_;
            }
        }

        protected void ProcessAllReceivedPackets()
        {
            lock (receivedPacketList)
            {
                try
                {
                    foreach (PacketAbstract packet in receivedPacketList)
                    {
                        ProcessPacket(packet);
                    }
                }
                catch (Exception exception)
                {
                    RemoteConsoleInternalLog.Exception(exception);
                }

                receivedPacketList.Clear();
            }
        }

        private void ProcessPacket(PacketAbstract packet_)
        {
            if (null == packet_)
                return;

            if (packet_ is LogPacket logPacket)
            {
                OnProcessLogPacket(logPacket);
            }
            else if (packet_ is CommandRegisterPacket commandRegisterPacket)
            {
                OnProcessCommandRegisterPacket(commandRegisterPacket);
            }
            else if (packet_ is SystemInfoPacket systemInfoPacket)
            {
                systemInfoContext.ResetFromOtherContext(systemInfoPacket.context);
            }
            else if (packet_ is ProfileInfoPacket profileInfoPacket)
            {
                profileInfoContext.ResetFromOtherContext(profileInfoPacket.context);
            }
        }

        private void OnProcessLogPacket(LogPacket logPacket_)
        {
            LogItem logItem = new LogItem();
            logItem.LogType = logPacket_.context.LogType;
            logItem.LogData = logPacket_.context.LogString;
            logItem.FrameCount = logPacket_.context.FrameCount;
            logItem.TimeSeconds = logPacket_.context.TimeSeconds;
            logItem.ObjectName = logPacket_.context.ObjectName;
            logItem.ObjectInstanceID = logPacket_.context.ObjectInstanceID;
            Object unityObject = EditorUtility.InstanceIDToObject(logItem.ObjectInstanceID);
            if (null != unityObject)
            {
                logItem.ObjectName = unityObject.name;
            }

            logItem.BuildStackInfo(logPacket_.context.LogStackTrace);
            logCollection.AddItem(logItem);

            if ((logItem.LogType == LogType.Error || logItem.LogType == LogType.Assert || logItem.LogType == LogType.Exception))
            {
                onReceiveErrorLogDelegate?.Invoke(this);
            }
        }

        public void SendPreferences()
        {
            PreferencesPacket packet = new PreferencesPacket();
            packet.context.ProfileRefreshIntervalTimeSeconds = ConsoleEditorPrefs.ProfileRefreshIntervalTimeS;
            packet.context.SkipStackFrameCount = ConsoleEditorPrefs.SkipStackFrameCount;
            packet.context.ShowUnityDebugLog = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG);
            SendPacket(packet);
        }

        public void SendPause(bool isPause_, bool canStep_, bool errorPause_)
        {
            PausePlayingPacket playingPacket = new PausePlayingPacket();
            playingPacket.IsPause = isPause_;
            playingPacket.CanStep = canStep_;
            playingPacket.PauseWhenError = errorPause_;
            SendPacket(playingPacket);
        }

        protected void SendSyncActive(bool isActiveSync_)
        {
            SyncActivePacket syncActivePacket = new SyncActivePacket();
            syncActivePacket.isActiveSync = isActiveSync_;
            SendPacket(syncActivePacket);
        }

        private void OnProcessCommandRegisterPacket(CommandRegisterPacket commandRegisterPacket_)
        {
            if (commandRegisterPacket_ == null)
                return;

            commandCollection.CreateAndInsertItemByPacket(commandRegisterPacket_);
        }

        protected virtual void OnActivate() { }
        protected virtual void OnDeActivate() { }
        protected virtual void OnConnect() { }
        protected virtual void OnDisconnect() { }
        protected abstract void SendPacket(PacketAbstract packet_);
        protected virtual void OnUpdateCustom() { }
    }
}