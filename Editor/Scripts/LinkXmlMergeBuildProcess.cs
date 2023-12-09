// 
// Copyright 2023 https://github.com/hope1026

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SPlugin.RemoteConsole.Editor
{
    internal class LinkXmlMergeBuildProcess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string LINK_XML_FILE_FULL_NAME = "SRemoteConsoleLink.xml";

        private readonly string _tempDirPath = $"{Application.dataPath}/BuildTemp/";
        private readonly string _tempDirMetaPath = $"{Application.dataPath}/BuildTemp.meta";
        private string LinkFilePath => $"{_tempDirPath}link.xml";
        
        int IOrderedCallback.callbackOrder => 10;

        public async void OnPreprocessBuild(BuildReport inReport_)
        {
            await CreateMergedLinkFromPackages();
        }

        public void OnPostprocessBuild(BuildReport inReport_)
        {
            if (File.Exists(LinkFilePath)) File.Delete(LinkFilePath);

            if (Directory.Exists(_tempDirPath)) Directory.Delete(_tempDirPath, true);

            if (File.Exists(_tempDirMetaPath)) File.Delete(_tempDirMetaPath);
        }

        private async Task CreateMergedLinkFromPackages()
        {
            ListRequest packetListRequest = Client.List();

            do
            {
                await Task.Yield();
            } while (!packetListRequest.IsCompleted);

            switch (packetListRequest.Status)
            {
                case StatusCode.Failure:
                {
                    Debug.LogError(packetListRequest.Error.message);
                    return;
                }
                case StatusCode.Success:
                {
                    List<string> xmlPathList = new List<string>();
                    foreach (PackageInfo package in packetListRequest.Result)
                    {
                        string packageResolvedPath = package.resolvedPath;
                        xmlPathList.AddRange(Directory.EnumerateFiles(packageResolvedPath, LINK_XML_FILE_FULL_NAME, SearchOption.AllDirectories).ToList());
                    }

                    if (xmlPathList.Count <= 0) return;

                    XDocument[] linkXmlArray = xmlPathList.Select(XDocument.Load).ToArray();

                    XDocument combinedXmlDocument = linkXmlArray.First();

                    foreach (XDocument xDocument in linkXmlArray.Where(inXml_ => inXml_ != combinedXmlDocument))
                    {
                        if (xDocument?.Root != null)
                        {
                            combinedXmlDocument?.Root?.Add(xDocument.Root.Elements());
                        }
                    }

                    if (Directory.Exists(_tempDirPath) == false)
                    {
                        Directory.CreateDirectory(_tempDirPath);
                    }
                    else if (File.Exists(LinkFilePath))
                    {
                        XDocument existLinkXmlDocument = XDocument.Load(LinkFilePath);
                        if (existLinkXmlDocument.Root != null)
                        {
                            combinedXmlDocument?.Root?.Add(existLinkXmlDocument.Root.Elements());
                        }
                    }

                    combinedXmlDocument?.Save(LinkFilePath);
                    break;
                }
                case StatusCode.InProgress:
                default:
                {
                    Debug.LogWarning($"LinkXmlMergeBuildProcess invalid PacketListStatus:{packetListRequest.Status}");
                    break;
                }
            }
        }
    }
}