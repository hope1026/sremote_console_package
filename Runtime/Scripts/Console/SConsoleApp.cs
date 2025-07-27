// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class SConsoleApp
    {
        public readonly LogCollection logCollection = new LogCollection();
        public readonly CommandCollection commandCollection = new CommandCollection();
        private readonly SystemInfoContext _systemInfoContext = new SystemInfoContext();
        private readonly ProfileInfoContext _profileInfoContext = new ProfileInfoContext();
        private readonly Action<SConsoleApp> _onReceiveErrorLogDelegate = null;
        private readonly List<PacketAbstract> _receivedPacketList = new List<PacketAbstract>();

        public void UpdateCustom()
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
                    commandPacket.commandName = command.CommandName;
                    commandPacket.commandValue = command.ValueString;
                    SendPacket(commandPacket);
                    command.OnSendCompleted();
                }
            }
            
            ProcessAllReceivedPackets();
        }

        private void ProcessAllReceivedPackets()
        {
            lock (_receivedPacketList)
            {
                try
                {
                    if (0 < RemoteConsoleToLocalApplicationBridge.Instance.PacketsForRuntimeConsoleApp.Count)
                    {
                        _receivedPacketList.InsertRange(0, RemoteConsoleToLocalApplicationBridge.Instance.PacketsForRuntimeConsoleApp);
                        RemoteConsoleToLocalApplicationBridge.Instance.PacketsForRuntimeConsoleApp.Clear();
                    }
                    
                    foreach (PacketAbstract packet in _receivedPacketList)
                    {
                        ProcessPacket(packet);
                    }
                }
                catch (Exception exception)
                {
                    RemoteConsoleInternalLog.Exception(exception);
                }

                _receivedPacketList.Clear();
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
                _systemInfoContext.ResetFromOtherContext(systemInfoPacket.context);
            }
            else if (packet_ is ProfileInfoPacket profileInfoPacket)
            {
                _profileInfoContext.ResetFromOtherContext(profileInfoPacket.context);
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

            logItem.BuildStackInfo(logPacket_.context.LogStackTrace);
            logCollection.AddItem(logItem);

            if ((logItem.LogType == LogType.Error || logItem.LogType == LogType.Assert || logItem.LogType == LogType.Exception))
            {
                _onReceiveErrorLogDelegate?.Invoke(this);
            }
        }

        public void SendPreferences()
        {
            PreferencesPacket packet = new PreferencesPacket();
            packet.context.ProfileRefreshIntervalTimeSeconds = ConsolePrefs.ProfileRefreshIntervalTimeS;
            packet.context.SkipStackFrameCount = ConsolePrefs.SkipStackFrameCount;
            packet.context.ShowUnityDebugLog = ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_UNITY_DEBUG_LOG);
            SendPacket(packet);
        }

        private void OnProcessCommandRegisterPacket(CommandRegisterPacket commandRegisterPacket_)
        {
            if (commandRegisterPacket_ == null)
                return;

            commandCollection.CreateAndInsertItemByPacket(commandRegisterPacket_);
        }

        private void SendPacket(PacketAbstract packet_)
        {
            if (null != packet_ && packet_.Write())
            {
                RemoteConsoleToLocalApplicationBridge.Instance.SendToConsoleMain(packet_);
            }
        }
    }
}