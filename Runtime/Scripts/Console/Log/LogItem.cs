// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class LogItem
    {
        private const string HYPER_LINK_KEY_FILE_PATH = "FilePath";
        private const string HYPER_LINK_KEY_LINE_NUMBER = "LineNumber";

        public class StackContext
        {
            public string FilePath { get; set; } = string.Empty;
            public int LineNumber { get; set; } = 0;
            public string DisplayStackString { get; set; } = string.Empty;
            public string OriginalStackString { get; set; } = string.Empty;
            public bool IsAbsolutePath { get; set; } = false;
        }

        public string LogData { get; set; } = string.Empty;
        public string StackString { get; private set; } = string.Empty;
        public LogType LogType { get; set; }
        public string FilePath { get; private set; } = string.Empty;
        public int LineNumber { get; private set; }
        public int FrameCount { get; set; }
        public float TimeSeconds { get; set; }
        public int CollapseCount { get; set; }
        public string ObjectName { get; set; } = string.Empty;
        public List<StackContext> StackList { get; } = new List<StackContext>();
        public List<string> ContainSearchStringList { get; } = new List<string>();

        public void BuildStackInfo(string stackString_)
        {
            if (string.IsNullOrEmpty(stackString_))
                return;

            const string DATA_DIRECTORY_NAME = "Assets";
            const string STACK_PATH_START_STRING = "(at ";

            StringBuilder stringBuilder = new StringBuilder();
            FilePath = string.Empty;
            StackList.Clear();
            string[] stackStringArray = stackString_.Split('\n', '\r');
            foreach (string stackString in stackStringArray)
            {
                if (true == string.IsNullOrEmpty(stackString))
                    continue;

                LogItem.StackContext stackContext = new LogItem.StackContext();
                stackContext.OriginalStackString = stackString;
                stringBuilder.AppendLine(stackString);

                if (true == stackString.Contains(STACK_PATH_START_STRING) && true == stackString.Contains(":"))
                {
                    stackContext.IsAbsolutePath = !stackContext.OriginalStackString.Contains(DATA_DIRECTORY_NAME);

                    string pathString = stackString.Substring(stackString.IndexOf(STACK_PATH_START_STRING, StringComparison.Ordinal) + STACK_PATH_START_STRING.Length);
                    pathString = pathString.Remove(pathString.LastIndexOf(":", StringComparison.Ordinal));

                    if (false == stackContext.IsAbsolutePath)
                    {
                        string fullPathRelativeToTheProject = pathString;
                        string fullPathRelativeToTheData = fullPathRelativeToTheProject.Substring(DATA_DIRECTORY_NAME.Length);
                        string fullPath = Application.dataPath + fullPathRelativeToTheData;
                        if (true == File.Exists(fullPath))
                        {
                            stackContext.FilePath = fullPath;
                        }
                    }
                    else
                    {
                        if (true == File.Exists(pathString))
                        {
                            stackContext.FilePath = pathString;
                        }
                    }

                    string lineNumberString = stackString.Substring(stackString.LastIndexOf(":", StringComparison.Ordinal));
                    lineNumberString = lineNumberString.Trim(')', ':');

                    if (true == int.TryParse(lineNumberString, out int lineNumber))
                    {
                        stackContext.LineNumber = lineNumber;
                    }

                    if (string.IsNullOrEmpty(pathString) == false)
                    {
                        string oldString = $"{pathString}:{lineNumberString}";
                        string hyperLinkString = $"<a {HYPER_LINK_KEY_FILE_PATH}=\"{stackContext.FilePath}\" {HYPER_LINK_KEY_LINE_NUMBER}=\"{stackContext.LineNumber}\">{pathString}:{lineNumberString}</a>";
                        stackContext.DisplayStackString = stackContext.OriginalStackString.Replace(oldString, hyperLinkString);
                    }
                }

                if (true == string.IsNullOrEmpty(FilePath) && false == string.IsNullOrEmpty(stackContext.FilePath))
                {
                    FilePath = stackContext.FilePath;
                    LineNumber = stackContext.LineNumber;
                }

                StackList.Add(stackContext);
            }

            StackString = stringBuilder.ToString();
        }
    }
}