// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using SPlugin.Network;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace SPlugin
{
    internal class SConsoleMain
    {
        private GameObject _consoleGameObject = null;
        private readonly Dictionary<string, CommandAbstract> _commandsByKey = new Dictionary<string, CommandAbstract>();
        private readonly SConsoleNetwork _consoleNetwork = new SConsoleNetwork();
        private readonly PreferencesContext _preferencesContext = new PreferencesContext();
        private readonly ProfileInfoContext _profileInfoContext = new ProfileInfoContext();
        private readonly SystemInfoContext _systemInfoContext = new SystemInfoContext();
        private readonly PausePlayingContext _pausePlayingContext = new PausePlayingContext();
        private int _frameCount = 0;
        private int _lastFrameCount = 0;
        private float _accumulateTimeSecondsForFrame = 0f;
        private float _lastSentProfileTimeSeconds;
        private readonly int _mainThreadId;
        private float _lastRealTimeSinceStartup = 0f;

        private bool _isSyncActivated = false;

        public SConsoleMain()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _consoleNetwork.OnReceivePacketDelegate = OnReceivePacketHandler;
            _consoleNetwork.OnNetworkEventDelegate = OnNetworkEventHandler;

            _systemInfoContext.ProductName = Application.productName;
            _systemInfoContext.RuntimePlatform = Application.platform;
            _systemInfoContext.DeviceUniqueIdentifier = UnityEngine.SystemInfo.deviceUniqueIdentifier;
            _systemInfoContext.OperatingSystem = UnityEngine.SystemInfo.operatingSystem;
            _systemInfoContext.DeviceName = UnityEngine.SystemInfo.deviceName;
            _systemInfoContext.DeviceModel = UnityEngine.SystemInfo.deviceModel;
            _systemInfoContext.SystemMemorySize = UnityEngine.SystemInfo.systemMemorySize;
            _systemInfoContext.ProcessorCount = UnityEngine.SystemInfo.processorCount;
            _systemInfoContext.GraphicsDeviceName = UnityEngine.SystemInfo.graphicsDeviceName;
            _systemInfoContext.GraphicsDeviceType = UnityEngine.SystemInfo.graphicsDeviceType;
            _systemInfoContext.GraphicsMemorySize = UnityEngine.SystemInfo.graphicsMemorySize;
            _systemInfoContext.MaxTextureSize = UnityEngine.SystemInfo.maxTextureSize;
            _systemInfoContext.IsDevelopmentBuild = Debug.isDebugBuild;

            _pausePlayingContext.IsPause = false;
            _pausePlayingContext.PauseWhenError = false;
            _pausePlayingContext.CanStep = false;
        }

        public void StartIfNotStarted()
        {
            CreateConsoleGameObjectIfNotExist();
            _consoleNetwork.StartHost();
        }

        public void OnStartIfLocalEditor()
        {
            SendSystemInfo();
        }

        private void Stop()
        {
            _consoleNetwork.End();
            _consoleGameObject = null;
        }

        public void UpdateCustom()
        {
            try
            {
                if (null != _consoleNetwork)
                {
                    _accumulateTimeSecondsForFrame += Time.unscaledDeltaTime;
                    _frameCount++;
                    _profileInfoContext.FramePerSecond = _frameCount / _accumulateTimeSecondsForFrame;
                    if (1f <= _accumulateTimeSecondsForFrame)
                    {
                        _accumulateTimeSecondsForFrame = 0f;
                        _frameCount = 0;
                    }

                    float refreshIntervalTimeS = _preferencesContext.ProfileRefreshIntervalTimeSeconds;
                    if (_lastSentProfileTimeSeconds + refreshIntervalTimeS < Time.realtimeSinceStartup)
                    {
                        _lastSentProfileTimeSeconds = Time.realtimeSinceStartup;
                        _profileInfoContext.UsedHeapSize = Profiler.usedHeapSizeLong;
                        SendProfileInfo();
                    }

                    _consoleNetwork.Update();
                }

                if (true == _pausePlayingContext.IsPause && false == Application.isEditor)
                {
                    while (true == _pausePlayingContext.IsPause)
                    {
                        if (true == _pausePlayingContext.CanStep)
                        {
                            _pausePlayingContext.CanStep = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RemoteConsoleInternalLog.Exception(exception);
            }

            InvokeAllChangedCommand();
        }

        private void InvokeAllChangedCommand()
        {
            foreach (CommandAbstract command in _commandsByKey.Values)
            {
                command.InvokeDelegateIfChangedValue();
            }
        }

        private void OnNetworkEventHandler(SocketServerEventType eventType_)
        {
            if (eventType_ == SocketServerEventType.CLIENT_DISCONNECTED || eventType_ == SocketServerEventType.SERVER_CLOSED)
            {
                _pausePlayingContext.IsPause = false;
            }
            else if (eventType_ == SocketServerEventType.CLIENT_CONNECTED)
            {
                SendSystemInfo();
            }
        }

        private void OnReceivePacketHandler(PacketAbstract packet_)
        {
            if (packet_ == null)
                return;

            if (packet_ is SyncActivePacket activeSyncPacket)
            {
                _isSyncActivated = activeSyncPacket.isActiveSync;
                if (_isSyncActivated)
                {
                    SendAllCommandRegister();
                }
                else
                {
                    _pausePlayingContext.IsPause = false;
                }
            }
            else if (packet_ is PreferencesPacket preferencesPacket)
            {
                OnReceivedPreferencesPacket(preferencesPacket);
            }
            else if (packet_ is CommandPacket commandPacket)
            {
                OnReceivedCommandPacket(commandPacket);
            }
            else if (packet_ is PausePlayingPacket pausePlayingPacket)
            {
                _pausePlayingContext.IsPause = pausePlayingPacket.IsPause;
                _pausePlayingContext.CanStep = pausePlayingPacket.CanStep;
                _pausePlayingContext.PauseWhenError = pausePlayingPacket.PauseWhenError;
            }
        }

        private void OnReceivedCommandPacket(CommandPacket commandPacket_)
        {
            if (commandPacket_ == null)
                return;

            string key = CommandAbstract.GenerateCommandKey(commandPacket_.commandCategory, commandPacket_.commandName);
            if (_commandsByKey.TryGetValue(key, out CommandAbstract command))
            {
                command?.OnReceiveCommandValue(commandPacket_.commandValue);
            }
        }

        private void OnReceivedPreferencesPacket(PreferencesPacket preferencesPacket_)
        {
            if (preferencesPacket_ == null || preferencesPacket_.context == null)
                return;

            _preferencesContext.ShowUnityDebugLog = preferencesPacket_.context.ShowUnityDebugLog;
            _preferencesContext.SkipStackFrameCount = preferencesPacket_.context.SkipStackFrameCount;
            _preferencesContext.ProfileRefreshIntervalTimeSeconds = preferencesPacket_.context.ProfileRefreshIntervalTimeSeconds;

            if (_preferencesContext.ShowUnityDebugLog)
            {
                Application.logMessageReceivedThreaded -= OnUnityDebugLogHandler;
                Application.logMessageReceivedThreaded += OnUnityDebugLogHandler;
            }
            else
            {
                Application.logMessageReceivedThreaded -= OnUnityDebugLogHandler;
            }
        }

        private void SendSystemInfo()
        {
            if (null != _consoleNetwork)
            {
                SystemInfoPacket systemInfoPacket = new SystemInfoPacket(_systemInfoContext);
                _consoleNetwork.SendPacket(systemInfoPacket);
            }
        }

        private void SendProfileInfo()
        {
            if (null != _consoleNetwork && _isSyncActivated)
            {
                ProfileInfoPacket packet = new ProfileInfoPacket();
                packet.context = _profileInfoContext;
                _consoleNetwork.SendPacket(packet);
            }
        }

        private void SendAllCommandRegister()
        {
            if (_isSyncActivated == false)
                return;

            foreach (CommandAbstract command in _commandsByKey.Values)
            {
                SendCommandRegister(command);
            }
        }

        private void SendCommandRegister(CommandAbstract command_)
        {
            if (null != _consoleNetwork && _isSyncActivated)
            {
                CommandRegisterPacket commandRegisterPacket = new CommandRegisterPacket();
                commandRegisterPacket.commandType = command_.CommandType;
                commandRegisterPacket.commandCategory = command_.Category;
                commandRegisterPacket.commandName = command_.Name;
                commandRegisterPacket.commandValue = command_.ValueString;
                commandRegisterPacket.commandDisplayPriority = command_.DisplayPriority;
                commandRegisterPacket.commandTooltip = command_.Tooltip;
                _consoleNetwork.SendPacket(commandRegisterPacket);
            }
        }

        public bool TryAddCommand(CommandAbstract commandAbstract_)
        {
            if (commandAbstract_ == null)
                return false;

            if (_commandsByKey.TryAdd(CommandAbstract.GenerateCommandKey(commandAbstract_.Category, commandAbstract_.Name), commandAbstract_))
            {
                SendCommandRegister(commandAbstract_);
                return true;
            }

            return false;
        }

        private void OnUnityDebugLogHandler(string condition_, string stackTrace_, LogType logType_)
        {
            if (_consoleNetwork == null || _isSyncActivated == false)
                return;

            StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
            SendLogPacket(logType_, condition_, unityObject_: null, stackTrace);
        }

        public void SendLogException(Exception exception_, Object unityObject_)
        {
            if (_consoleNetwork == null || _isSyncActivated == false)
                return;

            StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
            SendLogPacket(LogType.Exception, exception_.Message, unityObject_, stackTrace);
        }

        public void SendLog(LogType logType_, string logMessage_, Object unityObject_)
        {
            if (_consoleNetwork == null || _isSyncActivated == false)
                return;

            StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
            SendLogPacket(logType_, logMessage_, unityObject_, stackTrace);
        }

        private void SendLogPacket(LogType logType_, string logMessage_, Object unityObject_, StackTrace stackTrace_)
        {
            if (_consoleNetwork == null || _isSyncActivated == false)
                return;

            LogPacket packet = new LogPacket();
            packet.context.LogType = logType_;
            packet.context.LogString = logMessage_;
            packet.context.TimeSeconds = GetRealTimeSinceStartup();
            packet.context.FrameCount = GetFrameCount();

            if (null != unityObject_)
            {
                packet.context.ObjectName = unityObject_.name;
                packet.context.ObjectInstanceID = unityObject_.GetInstanceID();
            }

            BuildStackTrace(stackTrace_, ref packet.context);

            _consoleNetwork.SendPacket(packet);

            if (false == Application.isEditor && true == _pausePlayingContext.PauseWhenError)
            {
                if (LogType.Error == logType_ || LogType.Exception == logType_)
                {
                    _pausePlayingContext.IsPause = true;
                }
            }
        }

        private void BuildStackTrace(StackTrace stackTrack_, ref LogContext logContext_)
        {
            StringBuilder builder = new StringBuilder();
            int remainSkipCount = (int)_preferencesContext.SkipStackFrameCount;
            for (int frameIndex = 0; frameIndex < stackTrack_.FrameCount; frameIndex++)
            {
                StackFrame stackFrame = stackTrack_.GetFrame(frameIndex);

                MethodBase methodBase = stackFrame.GetMethod();
                if (null == methodBase)
                    continue;

                Type reflectedType = methodBase.ReflectedType;
                if (null == reflectedType || reflectedType.Module.Assembly.FullName.Contains("UnityEngine.CoreModule") ||
                    reflectedType.Module.Assembly.FullName.Contains("SPlugin.RemoteConsole"))
                    continue;

                if (0 < remainSkipCount)
                {
                    remainSkipCount--;
                    continue;
                }

                builder.Append(reflectedType.Name);
                builder.Append(':');
                builder.Append(methodBase.Name);
                builder.Append('(');
                ParameterInfo[] parameterInfoArray = methodBase.GetParameters();
                for (int index = 0; index < parameterInfoArray.Length; index++)
                {
                    if (0 != index)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(parameterInfoArray[index].ParameterType.Name);
                }

                builder.Append(") (at ");
                string fileAbsolutePath = stackFrame.GetFileName();
                if (false == string.IsNullOrEmpty(fileAbsolutePath))
                {
                    if (true == fileAbsolutePath.Contains("Assets"))
                    {
                        builder.Append(fileAbsolutePath.Substring(fileAbsolutePath.IndexOf("Assets", StringComparison.Ordinal)));
                    }
                    else if (true == fileAbsolutePath.Contains("Packages"))
                    {
                        builder.Append(fileAbsolutePath.Substring(fileAbsolutePath.IndexOf("Packages", StringComparison.Ordinal)));
                    }
                    else
                    {
                        builder.Append(fileAbsolutePath);
                    }
                }

                builder.Append(':');
                builder.Append(stackFrame.GetFileLineNumber());
                builder.Append(')');
                builder.Append('\n');
            }

            logContext_.LogStackTrace = builder.ToString();
        }

        private float GetRealTimeSinceStartup()
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                _lastRealTimeSinceStartup = Time.realtimeSinceStartup;
                return _lastRealTimeSinceStartup;
            }
            else
            {
                return _lastRealTimeSinceStartup;
            }
        }

        private int GetFrameCount()
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                _lastFrameCount = Time.frameCount;
                return _lastFrameCount;
            }
            else
            {
                return _lastFrameCount;
            }
        }

        private void CreateConsoleGameObjectIfNotExist()
        {
            if (null == _consoleGameObject)
            {
                SConsoleComponent component = Object.FindObjectOfType(typeof(SConsoleComponent)) as SConsoleComponent;
                if (null != component)
                {
                    _consoleGameObject = component.gameObject;
                }

                if (null == _consoleGameObject)
                {
                    _consoleGameObject = new GameObject("SPluginRemoteConsole");
                    _consoleGameObject.hideFlags = HideFlags.HideAndDontSave;
                    component = _consoleGameObject.AddComponent<SConsoleComponent>();
                }

                if (null != component)
                {
                    component.DelegateOnDestroy = Stop;
                    component.DelegateOnUpdate = UpdateCustom;
                    RemoteConsoleLocalEditorBridge.Instance.UnRegisterUpdateInEditorDelegate();
                }

                Object.DontDestroyOnLoad(_consoleGameObject);
            }
        }
    }
}