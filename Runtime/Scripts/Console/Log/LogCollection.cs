// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using UnityEngine;

namespace SPlugin.RemoteConsole.Runtime
{
    internal class LogCollection
    {
        const int MAX_LOG_ITEM = 1000;

        private readonly List<LogItem> _bufferList = new List<LogItem>(MAX_LOG_ITEM);
        private readonly List<LogItem> _filteredBufferList = new List<LogItem>(MAX_LOG_ITEM);

        public int LogCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ErrorCount { get; private set; }

        public void AddItem(LogItem newLogItem_)
        {
            if (null == newLogItem_)
                return;

            _bufferList.Add(newLogItem_);

            switch (newLogItem_.LogType)
            {
                case LogType.Log:
                {
                    LogCount++;
                }
                    break;
                case LogType.Warning:
                {
                    WarningCount++;
                }
                    break;
                case LogType.Error:
                {
                    ErrorCount++;
                }
                    break;
            }

            if (_bufferList.Count >= MAX_LOG_ITEM)
            {
                switch (_bufferList[0].LogType)
                {
                    case LogType.Log:
                    {
                        LogCount = Mathf.Max(0, --LogCount);
                    }
                        break;
                    case LogType.Warning:
                    {
                        WarningCount = Mathf.Max(0, --WarningCount);
                    }
                        break;
                    case LogType.Error:
                    {
                        ErrorCount = Mathf.Max(0, --ErrorCount);
                    }
                        break;
                }
                _bufferList.RemoveAt(0);
            }

            if (true == CanPassFilter(newLogItem_))
            {
                newLogItem_.CollapseCount = 0;
                if (false == ConsolePrefs.GetFlagState(ConsolePrefsFlags.IS_COLLAPSE))
                {
                    _filteredBufferList.Add(newLogItem_);
                }
                else
                {
                    LogItem tempLogItem = FindEqualItemFromFilteredBufferList(newLogItem_);
                    if (null == tempLogItem)
                    {
                        _filteredBufferList.Add(newLogItem_);
                    }
                    else
                    {
                        tempLogItem.CollapseCount++;
                        tempLogItem.TimeSeconds = newLogItem_.TimeSeconds;
                        tempLogItem.FrameCount = newLogItem_.FrameCount;
                    }
                }

                if (_filteredBufferList.Count >= MAX_LOG_ITEM)
                {
                    _filteredBufferList.RemoveAt(0);
                }
            }
        }

        public void FilteringBuffer()
        {
            _filteredBufferList.Clear();

            foreach (LogItem item in _bufferList)
            {
                if (true == CanPassFilter(item))
                {
                    item.CollapseCount = 0;
                    if (false == ConsolePrefs.GetFlagState(ConsolePrefsFlags.IS_COLLAPSE))
                    {
                        _filteredBufferList.Add(item);
                    }
                    else
                    {
                        LogItem tempLogItem = FindEqualItemFromFilteredBufferList(item);
                        if (null == tempLogItem)
                        {
                            _filteredBufferList.Add(item);
                        }
                        else
                        {
                            tempLogItem.CollapseCount++;
                            tempLogItem.TimeSeconds = item.TimeSeconds;
                            tempLogItem.FrameCount = item.FrameCount;
                        }
                    }
                }
            }
        }

        private bool CanPassFilter(LogItem logLogItem_)
        {
            logLogItem_.ContainSearchStringList.Clear();
            switch (logLogItem_.LogType)
            {
                case LogType.Log:
                {
                    if (false == ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_LOG)) { return false; }
                    break;
                }
                case LogType.Warning:
                {
                    if (false == ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_WARNING)) { return false; }
                    break;
                }
                case LogType.Error:
                {
                    if (false == ConsolePrefs.GetFlagState(ConsolePrefsFlags.SHOW_ERROR)) { return false; }
                    break;
                }
            }

            if (false == string.IsNullOrEmpty(ConsolePrefs.SearchString))
            {
                if (true == logLogItem_.LogData.Contains(ConsolePrefs.SearchString))
                {
                    logLogItem_.ContainSearchStringList.Add(ConsolePrefs.SearchString);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public LogItem GetFilteredItem(int itemIndex_)
        {
            if (_filteredBufferList.Count <= itemIndex_)
                return null;

            return _filteredBufferList[itemIndex_];
        }

        public void ClearItems()
        {
            _bufferList.Clear();
            _filteredBufferList.Clear();
            LogCount = 0;
            WarningCount = 0;
            ErrorCount = 0;
        }

        public int FilteredItemsCount()
        {
            return _filteredBufferList.Count;
        }

        private LogItem FindEqualItemFromFilteredBufferList(LogItem targetLogItem_)
        {
            if (targetLogItem_ == null) return null;

            foreach (LogItem item in _filteredBufferList)
            {
                if (item == null) continue;

                if (targetLogItem_.LineNumber != item.LineNumber)
                    continue;

                // Null-safe string comparisons
                string targetObjectName = targetLogItem_.ObjectName ?? string.Empty;
                string itemObjectName = item.ObjectName ?? string.Empty;
                if (targetObjectName.GetHashCode() != itemObjectName.GetHashCode())
                    continue;

                string targetFilePath = targetLogItem_.FilePath ?? string.Empty;
                string itemFilePath = item.FilePath ?? string.Empty;
                if (targetFilePath.GetHashCode() != itemFilePath.GetHashCode())
                    continue;

                string targetLogData = targetLogItem_.LogData ?? string.Empty;
                string itemLogData = item.LogData ?? string.Empty;
                if (targetLogData.GetHashCode() != itemLogData.GetHashCode())
                    continue;

                if (!targetObjectName.Equals(itemObjectName))
                    continue;

                if (!targetFilePath.Equals(itemFilePath))
                    continue;

                if (targetLogData.Equals(itemLogData))
                    return item;
            }
            return null;
        }
    }
}