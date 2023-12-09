// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SPlugin
{
    internal class LogItem
    {
        public class Stack
        {
            public string FilePath { get; set; } = string.Empty;
            public int LineNumber { get; set; } = 0;
            public string StackString { get; set; } = string.Empty;
            public bool IsAbsolutePath { get; set; } = false;
        }

        public string LogData { get; set; } = string.Empty;
        public string StackString { get; set; } = string.Empty;
        public LogType LogType { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int FrameCount { get; set; }
        public float TimeSeconds { get; set; }
        public int CollapseCount { get; set; }
        public string ObjectName { get; set; } = string.Empty;
        public int ObjectInstanceID { get; set; }
        public List<Stack> StackList { get; } = new List<Stack>();
        public List<string> ContainSearchStringList { get; } = new List<string>();

        public void BuildStackInfo(string stackString_)
        {
            if (string.IsNullOrEmpty(stackString_))
                return;

            const string DATA_DIRECTORY_NAME = "Assets";
            const string STACK_PATH_START_STRING = "(at ";

            FilePath = string.Empty;
            StackList.Clear();
            string[] stackStringArray = stackString_.Split('\n', '\r');
            foreach (string stackString in stackStringArray)
            {
                if (true == string.IsNullOrEmpty(stackString))
                    continue;

                LogItem.Stack stack = new LogItem.Stack();
                stack.StackString = stackString;

                if (true == stackString.Contains(STACK_PATH_START_STRING) && true == stackString.Contains(":"))
                {
                    stack.IsAbsolutePath = !stack.StackString.Contains(DATA_DIRECTORY_NAME);

                    string pathString = stackString.Substring(stackString.IndexOf(STACK_PATH_START_STRING, StringComparison.Ordinal) + STACK_PATH_START_STRING.Length);
                    pathString = pathString.Remove(pathString.LastIndexOf(":", StringComparison.Ordinal));

                    if (false == stack.IsAbsolutePath)
                    {
                        string fullPathRelativeToTheProject = pathString;
                        string fullPathRelativeToTheData = fullPathRelativeToTheProject.Substring(DATA_DIRECTORY_NAME.Length);
                        string fullPath = Application.dataPath + fullPathRelativeToTheData;
                        if (true == File.Exists(fullPath))
                        {
                            stack.FilePath = fullPath;
                        }
                    }
                    else
                    {
                        if (true == File.Exists(pathString))
                        {
                            stack.FilePath = pathString;
                        }
                    }

                    string lineNumberString = stackString.Substring(stackString.LastIndexOf(":", StringComparison.Ordinal));
                    lineNumberString = lineNumberString.Trim(')', ':');

                    if (true == int.TryParse(lineNumberString, out int lineNumber))
                    {
                        stack.LineNumber = lineNumber;
                    }
                }

                if (true == string.IsNullOrEmpty(FilePath) && false == string.IsNullOrEmpty(stack.FilePath))
                {
                    FilePath = stack.FilePath;
                    LineNumber = stack.LineNumber;
                }

                StackList.Add(stack);
            }
        }
    }
}