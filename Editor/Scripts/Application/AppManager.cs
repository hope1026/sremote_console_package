// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;

namespace SPlugin.RemoteConsole.Editor
{
    internal class AppManager
    {
        public readonly Dictionary<string, RemoteApp> remoteAppsByAddress = new Dictionary<string, RemoteApp>(16);
        private readonly List<RemoteApp> _scanningRemoteApps = new List<RemoteApp>(255);
        private readonly List<RemoteApp> _deleteRequestedApps = new List<RemoteApp>(4);
        private LocalEditorApp _localEditorApp = null;
        private AppAbstract _activatedApp = null;

        #region SINGLETON

        private static AppManager _instance = null;
        private static readonly object _lockObject = new object();

        public static AppManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppManager();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        public void Initialize() { }

        public void ActivateApp(AppAbstract triedApp_)
        {
            if (triedApp_ == null)
                return;

            if (triedApp_ == _activatedApp && _activatedApp.HasConnected())
                return;

            if (_activatedApp != null)
            {
                _activatedApp.onReceiveErrorLogDelegate = null;
                _activatedApp.DeActivate();
            }

            _activatedApp = triedApp_;
            _activatedApp.onReceiveErrorLogDelegate = OnReceiveLogErrorHandler;
            _activatedApp.Activate();

            ConsoleViewMain.Instance.OnChangedCurrentApp(_activatedApp);
        }

        public void ScanRemoteAppsInPrivateNetwork()
        {
            CancelAllScanningApps();

            string localIpSubnetMaskWithoutLastDot = SConsoleNetworkUtil.GetLocalIpSubnetMaskWidthOutLastDot();
            for (int hostNumber = 0; hostNumber < 255; hostNumber++)
            {
                ScanRemoteAppByIp($"{localIpSubnetMaskWithoutLastDot}.{hostNumber}");
            }
        }

        public void ScanRemoteAppByIp(string ip_)
        {
            RemoteApp remoteApp = new RemoteApp();
            remoteApp.Initialize();
            remoteApp.Connect(ip_);
            _scanningRemoteApps.Add(remoteApp);
        }

        public void CancelAllScanningApps()
        {
            foreach (RemoteApp findingRemoteApp in _scanningRemoteApps)
            {
                findingRemoteApp.Disconnect();
            }
            _scanningRemoteApps.Clear();
        }

        public void RemoveAppIfDisConnected(RemoteApp app_)
        {
            if (app_ != null && _deleteRequestedApps.Contains(app_) == false &&
                remoteAppsByAddress.TryGetValue(app_.IpAddressString, out RemoteApp remoteApp))
            {
                _deleteRequestedApps.Add(remoteApp);
            }
        }

        public bool IsScanning()
        {
            return 0 < _scanningRemoteApps.Count;
        }

        public void CalculateScanningProgressValue(out float outCurValue_, out float outMaxValue_)
        {
            outCurValue_ = 0;
            outMaxValue_ = 0;

            if (_scanningRemoteApps.Count <= 0)
                return;

            foreach (RemoteApp scanningRemoteApp in _scanningRemoteApps)
            {
                outMaxValue_ += scanningRemoteApp.MaxConnectingCountForDisplay;
                outCurValue_ += scanningRemoteApp.TriedConnectingCount;
            }
        }

        public bool IsPlayingCurrentApp()
        {
            if (_activatedApp != null)
                return _activatedApp.IsPlaying();

            return false;
        }

        public AppAbstract GetActivatedApp()
        {
            if (_activatedApp == null)
            {
                ActivateApp(GetLocalApp());
            }

            return _activatedApp;
        }

        public LocalEditorApp GetLocalApp()
        {
            if (_localEditorApp == null)
            {
                _localEditorApp = new LocalEditorApp();
                _localEditorApp.Initialize();
            }
            return _localEditorApp;
        }

        public void UpdateCustom()
        {
            for (int index = _scanningRemoteApps.Count - 1; 0 <= index; index--)
            {
                if (_scanningRemoteApps[index].AppConnectionStateType == AppConnectionStateType.CONNECTED)
                {
                    remoteAppsByAddress.TryAdd(_scanningRemoteApps[index].IpAddressString, _scanningRemoteApps[index]);
                    _scanningRemoteApps.RemoveAt(index);
                }
                else if (_scanningRemoteApps[index].AppConnectionStateType == AppConnectionStateType.DISCONNECTED)
                {
                    _scanningRemoteApps.RemoveAt(index);
                }
            }

            foreach (RemoteApp deleteRequestedApp in _deleteRequestedApps)
            {
                if (remoteAppsByAddress.ContainsKey(deleteRequestedApp.IpAddressString))
                {
                    remoteAppsByAddress.Remove(deleteRequestedApp.IpAddressString);
                }
            }
            _deleteRequestedApps.Clear();

            _localEditorApp?.UpdateCustom();

            foreach (RemoteApp scanningRemoteApp in remoteAppsByAddress.Values)
            {
                scanningRemoteApp.UpdateCustom();
            }

            foreach (RemoteApp scanningRemoteApp in _scanningRemoteApps)
            {
                scanningRemoteApp.UpdateCustom();
            }
        }

        private void OnReceiveLogErrorHandler(AppAbstract app_)
        {
            if (app_ == _activatedApp)
            {
                ConsoleViewMain.Instance.OnReceivedLogErrorInActivatedApp();
            }
        }
    }
}