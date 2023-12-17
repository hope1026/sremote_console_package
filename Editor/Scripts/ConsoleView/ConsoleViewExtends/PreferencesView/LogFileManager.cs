// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;

namespace SPlugin
{
    internal static class LogFileManager
    {
        public static readonly string DEFAULT_DIRECTORY_ABSOLUTE_PATH = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets", StringComparison.Ordinal) - 1) + "/SConsoleLog";

        public enum FileType
        {
            NONE = 0,
            TEXT,
            CSV,
            XML,
        }

        public static string FilePathAbsolute { get; private set; }
        public static string FileName { get; private set; }
        private static StreamWriter _streamWrite = null;
        private static FileStream _fileStream = null;
        private static XmlDocument _xmlDocument = null;
        private static bool _isFirstWrite = false;

        private static void OpenFile()
        {
            CloseFile();

            GenerateFilePath();
            _fileStream = new FileStream(FilePathAbsolute, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            if (null != _fileStream)
            {
                _streamWrite = new StreamWriter(_fileStream);
            }

            _isFirstWrite = true;
        }

        public static void GenerateFilePath()
        {
            if (ConsoleEditorPrefs.LogFileType == FileType.NONE)
                return;

            string footer = DateTime.Now.ToString("MMddHHmmss");
            if (AppManager.Instance.IsPlayingCurrentApp())
            {
                footer += "_Play";
            }

            if (FileName == string.Empty)
            {
                FileName = "Log_" + footer;
            }

            string fileNameE = FileName + "." + FileTypeToExtension(ConsoleEditorPrefs.LogFileType);
            if (FilePathAbsolute == string.Empty)
            {
                FilePathAbsolute = ConsoleEditorPrefs.LogDirectoryAbsolutePath + "/" + fileNameE;
            }
        }

        public static void SaveAs(string destFilePathAbsolute_)
        {
            if (null != _streamWrite)
            {
                _streamWrite.Flush();
            }

            if (true == File.Exists(FilePathAbsolute))
            {
                File.Copy(FilePathAbsolute, destFilePathAbsolute_, true);
            }
        }

        public static string FileTypeToExtension(FileType fileType_)
        {
            switch (fileType_)
            {
                case FileType.TEXT: { return "txt"; }
                case FileType.CSV:  { return "csv"; }
                case FileType.XML:  { return "xml"; }
            }

            return string.Empty;
        }

        public static void CloseFile()
        {
            if (null != _xmlDocument)
            {
                _xmlDocument.Save(_streamWrite);
                _xmlDocument = null;
            }

            if (null != _streamWrite)
            {
                _streamWrite.Flush();
                _streamWrite.Close();
                _streamWrite = null;
            }

            if (null != _fileStream)
            {
                _fileStream.Close();
                _fileStream = null;
            }

            FilePathAbsolute = string.Empty;
            FileName = string.Empty;
        }
        

        public static void Write(LogItem logLogItem_)
        {
            if (FileType.NONE == ConsoleEditorPrefs.LogFileType)
                return;

            if (null == _streamWrite)
            {
                OpenFile();
            }

            switch (ConsoleEditorPrefs.LogFileType)
            {
                case FileType.TEXT:
                {
                    WriteToTxt(logLogItem_);
                }
                    break;
                case FileType.CSV:
                {
                    WriteToCsv(logLogItem_);
                }
                    break;
                case FileType.XML:
                {
                    WriteToXml(logLogItem_);
                }
                    break;
            }

            _isFirstWrite = false;
        }

        private static void WriteToTxt(LogItem logLogItem_)
        {
            if (null != _streamWrite)
            {
                _streamWrite.WriteLine();
                string tempString = $"[TYPE]{logLogItem_.LogType} [TIME]{logLogItem_.TimeSeconds} [FRAME_COUNT]{logLogItem_.FrameCount} [OBJECT_NAME]{logLogItem_.ObjectName}";
                _streamWrite.WriteLine(tempString);

                foreach (LogItem.StackContext stack in logLogItem_.StackList)
                {
                    if (null != stack && false == string.IsNullOrEmpty(stack.DisplayStackString))
                    {
                        _streamWrite.WriteLine("[S]" + stack.DisplayStackString);
                    }
                }

                if (false == string.IsNullOrEmpty(logLogItem_.LogData))
                {
                    _streamWrite.WriteLine("[L]" + logLogItem_.LogData);
                }
            }
        }

        private static void WriteToCsv(LogItem logLogItem_)
        {
            if (null != _streamWrite)
            {
                if (true == _isFirstWrite)
                {
                    _streamWrite.WriteLine("[TYPE],[TIME],[FRAME_COUNT],[OBJECT_NAME],[S],[L]");
                }

                string tempString = string.Format("{0},{1},{2},{3},\"{4}\",\"{5}\"",
                                                  logLogItem_.LogType,
                                                  logLogItem_.TimeSeconds,
                                                  logLogItem_.FrameCount,
                                                  logLogItem_.ObjectName,
                                                  logLogItem_.StackString,
                                                  logLogItem_.LogData);
                _streamWrite.WriteLine(tempString);
            }
        }

        private static void WriteToXml(LogItem logLogItem_)
        {
            if (null == _xmlDocument)
            {
                _xmlDocument = new XmlDocument();
                XmlElement rootElement = _xmlDocument.CreateElement("SPluginRemoteConsoleLog");
                _xmlDocument.AppendChild(rootElement);
            }

            if (null != _xmlDocument.DocumentElement)
            {
                XmlElement xmlElement = _xmlDocument.CreateElement("Data");
                XmlElement childXMLElement = _xmlDocument.CreateElement("TYPE");
                childXMLElement.InnerText = logLogItem_.LogType.ToString();
                xmlElement.AppendChild(childXMLElement);

                childXMLElement = _xmlDocument.CreateElement("TIME");
                childXMLElement.InnerText = logLogItem_.TimeSeconds.ToString(CultureInfo.InvariantCulture);
                xmlElement.AppendChild(childXMLElement);

                childXMLElement = _xmlDocument.CreateElement("FRAME_COUNT");
                childXMLElement.InnerText = logLogItem_.FrameCount.ToString();
                xmlElement.AppendChild(childXMLElement);

                childXMLElement = _xmlDocument.CreateElement("OBJECT_NAME");
                childXMLElement.InnerText = logLogItem_.ObjectName;
                xmlElement.AppendChild(childXMLElement);

                childXMLElement = _xmlDocument.CreateElement("S");
                childXMLElement.InnerText = logLogItem_.StackString.TrimEnd('\n', '\r');
                xmlElement.AppendChild(childXMLElement);

                childXMLElement = _xmlDocument.CreateElement("L");
                childXMLElement.InnerText = logLogItem_.LogData;
                xmlElement.AppendChild(childXMLElement);

                _xmlDocument.DocumentElement.AppendChild(xmlElement);
            }
        }
    }
}