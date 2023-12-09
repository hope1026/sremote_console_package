// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class ApplicationsView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.APPLICATIONS;
        private readonly SGuiDragLine _deviceNameDragLine = new SGuiDragLine();
        private Rect _deviceNameDragLineRect = new Rect(150f, 0f, 1f, 20f);
        private readonly SGuiDragLine _appNameDragLine = new SGuiDragLine();
        private Rect _productNameDragLineRect = new Rect(300f, 0f, 1f, 20f);
        private string _directConnectionIp = string.Empty;

        public override void OnGuiCustom()
        {
            GUILayoutOption lineHeight = GUILayout.Height(2f);
            GUILayoutOption lineWidth = GUILayout.ExpandWidth(true);

            GUILayout.BeginVertical();

            const float WIDTH_MIN = 50f;
            const float WIDTH_MAX = 200f;
            OnGuiVerticalDragLine(_deviceNameDragLine, ref _deviceNameDragLineRect, WIDTH_MIN, WIDTH_MAX);
            OnGuiVerticalDragLine(_appNameDragLine, ref _productNameDragLineRect, _deviceNameDragLineRect.x + WIDTH_MIN, _deviceNameDragLineRect.x + WIDTH_MAX);

            float deviceNameWidth = _deviceNameDragLineRect.xMin;
            float productNameWidth = _productNameDragLineRect.xMin - _deviceNameDragLineRect.xMin;
            const float IP_ADDRESS_WIDTH = 150f;
            GUILayout.BeginHorizontal();
            GUILayout.Box("DeviceName", SGuiStyle.LabelStyle, GUILayout.Width(deviceNameWidth));
            GUILayout.Box("ProductName", SGuiStyle.LabelStyle, GUILayout.Width(productNameWidth));
            GUILayout.Box("IpAddress", SGuiStyle.LabelStyle, GUILayout.Width(IP_ADDRESS_WIDTH));
            GUILayout.Box("State", SGuiStyle.LabelStyle, GUILayout.Width(200f));
            GUILayout.EndHorizontal();
            SGuiUtility.OnGuiLine(lineWidth, lineHeight);

            OnGuiCurrentAppInfo(deviceNameWidth, productNameWidth, IP_ADDRESS_WIDTH);
            SGuiUtility.OnGuiLine(lineWidth, lineHeight);
            
            GUILayout.Space(10);
            GUILayout.Box("LocalApps", SGuiStyle.LabelStyle, GUILayout.Width(300f));
            OnGuiAppSummaryInfo(AppManager.Instance.GetLocalApp(), deviceNameWidth, productNameWidth, IP_ADDRESS_WIDTH);
            
            SGuiUtility.OnGuiLine(lineWidth, lineHeight);
            GUILayout.BeginHorizontal();
            GUILayout.Box("RemoteApps", SGuiStyle.LabelStyle, GUILayout.Width(200f));
            OnGuiScanningInfo();
            GUILayout.EndHorizontal();
            OnGuiRemoteAppsInfo(deviceNameWidth, productNameWidth, IP_ADDRESS_WIDTH);

            GUILayout.EndVertical();
        }

        private void OnGuiCurrentAppInfo(float deviceNameWidth_, float productNameWidth_, float ipAddressWidth_)
        {
            SGuiStyle.ActiveAppLabelStyle.fontStyle = FontStyle.Bold;
            SGuiStyle.ActiveAppLabelStyle.fontSize = 13;
            GUILayout.Box("CurrentApp", SGuiStyle.LabelStyle, GUILayout.Width(300f));
            OnGuiAppSummaryInfo(AppManager.Instance.GetActivatedApp(), deviceNameWidth_, productNameWidth_, ipAddressWidth_);
        }

        private void OnGuiScanningInfo()
        {
            if (AppManager.Instance.IsScanning())
            {
                GUILayout.BeginHorizontal();

                AppManager.Instance.CalculateScanningProgressValue(out float curValue, out float maxValue);
                GUILayout.Label($"Looking for remote apps...({curValue:N0}/{maxValue:N0})");

                if (GUILayout.Button("Cancel", SGuiStyle.ButtonStyle))
                    AppManager.Instance.CancelAllScanningApps();

                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginVertical();

                if (GUILayout.Button($"ScanRemoteAppsInLocalNetwork[{SConsoleNetworkUtil.GetLocalIpStartAddress()} - {SConsoleNetworkUtil.GetLocalIpEndAddress()}]", SGuiStyle.ButtonStyle))
                {
                    AppManager.Instance.ScanRemoteAppsInPrivateNetwork();
                }

                GUILayout.BeginHorizontal();

                if (string.IsNullOrEmpty(_directConnectionIp))
                    _directConnectionIp = SConsoleNetworkUtil.GetLocalIpAddress();

                _directConnectionIp = EditorGUILayout.TextField("Direct Connection", _directConnectionIp);
                if (GUILayout.Button("ScanRemoteApp", SGuiStyle.ButtonStyle))
                {
                    AppManager.Instance.ScanRemoteAppByIp(_directConnectionIp);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
        }

        private void OnGuiRemoteAppsInfo(float deviceNameWidth_, float productNameWidth_, float ipAddressWidth_)
        {
            GUILayout.BeginVertical();

            foreach (RemoteApp remoteApp in AppManager.Instance.remoteAppsByAddress.Values)
            {
                OnGuiAppSummaryInfo(remoteApp, deviceNameWidth_, productNameWidth_, ipAddressWidth_);
            }

            GUILayout.EndVertical();
        }

        private void OnGuiAppSummaryInfo(AppAbstract appAbstract_, float deviceNameWidth_, float productNameWidth_, float ipAddressWidth_)
        {
            if (appAbstract_ == null)
                return;

            GUILayout.BeginHorizontal();

            if (appAbstract_.IsActivated)
            {
                GUILayout.Label(appAbstract_.systemInfoContext.DeviceName, SGuiStyle.ActiveAppLabelStyle, GUILayout.Width(deviceNameWidth_));
                GUILayout.Label(appAbstract_.systemInfoContext.ProductName, SGuiStyle.ActiveAppLabelStyle, GUILayout.Width(productNameWidth_));
                GUILayout.Label(appAbstract_.IpAddressString, SGuiStyle.ActiveAppLabelStyle, GUILayout.Width(ipAddressWidth_));
                GUILayout.Label(appAbstract_.AppConnectionStateType.ToString(), SGuiStyle.ActiveAppLabelStyle);
            }
            else
            {
                Color color = Color.gray;
                string deviceName = SGuiUtility.ReplaceColorString(appAbstract_.systemInfoContext.DeviceName, color);
                string productName = SGuiUtility.ReplaceColorString(appAbstract_.systemInfoContext.ProductName, color);
                string ipAddressString = SGuiUtility.ReplaceColorString(appAbstract_.IpAddressString, color);
                string appStateType = SGuiUtility.ReplaceColorString(appAbstract_.AppConnectionStateType.ToString(), color);

                GUILayout.Label(deviceName, SGuiStyle.AppListLabelStyle, GUILayout.Width(deviceNameWidth_));
                GUILayout.Label(productName, SGuiStyle.AppListLabelStyle, GUILayout.Width(productNameWidth_));
                GUILayout.Label(ipAddressString, SGuiStyle.AppListLabelStyle, GUILayout.Width(ipAddressWidth_));
                GUILayout.Label(appStateType, SGuiStyle.AppListLabelStyle);
                if (appAbstract_.AppConnectionStateType == AppConnectionStateType.CONNECTING)
                {
                    if (GUILayout.Button("DisConnect", SGuiStyle.ButtonStyle) == true)
                    {
                        appAbstract_.Disconnect();
                    }
                }
                else if (appAbstract_.HasConnected() == false)
                {
                    if (GUILayout.Button("Connect", SGuiStyle.ButtonStyle) == true)
                    {
                        appAbstract_.Connect();
                    }
                }
                else if (appAbstract_.IsActivated == false)
                {
                    if (GUILayout.Button("Select", SGuiStyle.ButtonStyle) == true)
                    {
                        AppManager.Instance.ActivateApp(appAbstract_);
                    }
                }
            }

            if (appAbstract_.HasConnected() || appAbstract_.IsLocalEditor)
            {
                if (true == GUILayout.Button("ShowInfo", SGuiStyle.ButtonStyle))
                {
                    ApplicationSystemInfoEditorWindow window = EditorWindow.GetWindowWithRect<ApplicationSystemInfoEditorWindow>(ConsoleViewLayoutDefines.ApplicationView.systemInfoWindowAreaRect);
                    window.SetSelectedApp(appAbstract_);
                    window.Show();
                }
            }
            else
            {
                if (true == GUILayout.Button("Delete", SGuiStyle.ButtonStyle))
                {
                    AppManager.Instance.RemoveAppIfDisConnected(appAbstract_ as RemoteApp);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void OnGuiVerticalDragLine(SGuiDragLine sGuiDragLine_, ref Rect lineRect_, float posMinX_, float posMax_)
        {
            SGuiStyle.LineStyle.normal.background = EditorGUIUtility.whiteTexture;
            Color lineColor = new Color(0.15f, 0.15f, 0.15f);

            float posX = Math.Clamp(lineRect_.x, posMinX_, posMax_);
            lineRect_.Set(posX, 0f, 1f, 20f);

            Vector2 mousePosition = Vector2.zero;
            Rect collisionOffset = new Rect(-5f, 0f, 10f, 0f);
            if (sGuiDragLine_.OnGuiCustom(ref mousePosition, lineRect_, collisionOffset, lineColor, SGuiStyle.LineStyle, true))
            {
                posX = Math.Clamp(mousePosition.x, posMinX_, posMax_);
                lineRect_.Set(posX, 0f, 1f, 20f);
            }

            SGuiStyle.LineStyle.normal.background = GUI.skin.box.normal.background;
        }
    }
}