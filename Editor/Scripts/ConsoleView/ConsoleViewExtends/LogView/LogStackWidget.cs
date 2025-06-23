// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin
{
    internal class LogStackWidget
    {
        private ScrollView _stackTraceScroll;
        private LogItem _selectedLogItem;

        public void Initialize(VisualElement rootElement_)
        {
            _stackTraceScroll = rootElement_.Q<ScrollView>("stack-trace-scroll");
            if (_stackTraceScroll == null)
            {
                Debug.LogError("Stack trace scroll element not found!");
            }
        }

        public void UpdateStackTrace(LogItem selectedLogItem_)
        {
            _selectedLogItem = selectedLogItem_;
            
            if (_stackTraceScroll == null)
            {
                Debug.LogError("Stack trace scroll is null!");
                return;
            }

            // Clear existing content
            _stackTraceScroll.Clear();

            if (_selectedLogItem == null)
            {
                var noSelectionLabel = new Label("No log selected");
                noSelectionLabel.AddToClassList("stack-trace-content");
                _stackTraceScroll.Add(noSelectionLabel);
                return;
            }

            // Use the same logic as IMGUI version - display formatted stack trace
            if (_selectedLogItem.StackList != null && _selectedLogItem.StackList.Count > 0)
            {
                foreach (var stackContext in _selectedLogItem.StackList)
                {
                    string displayText = "";
                    
                    // Use OriginalStackString instead of DisplayStackString to avoid HTML parsing issues
                    if (!string.IsNullOrEmpty(stackContext.OriginalStackString))
                    {
                        displayText = stackContext.OriginalStackString;
                    }
                    else if (!string.IsNullOrEmpty(stackContext.DisplayStackString))
                    {
                        // Clean HTML tags from DisplayStackString if we must use it
                        displayText = CleanHtmlTags(stackContext.DisplayStackString);
                    }

                    if (!string.IsNullOrEmpty(displayText))
                    {
                        var stackLabel = new Label(displayText);
                        stackLabel.AddToClassList("stack-trace-line");
                        
                        // Make it clickable if there's a file path
                        if (!string.IsNullOrEmpty(stackContext.FilePath))
                        {
                            stackLabel.AddToClassList("stack-trace-clickable");
                            
                            // Add double-click handler to open source file
                            stackLabel.RegisterCallback<MouseDownEvent>(evt_ => {
                                if (evt_.clickCount == 2 && evt_.button == 0)
                                {
                                    LogItem.OpenStackTraceFile(stackContext.FilePath, stackContext.LineNumber);
                                    evt_.StopPropagation();
                                }
                            });
                            
                            // Add right-click context menu
                            stackLabel.RegisterCallback<MouseUpEvent>(evt_ => {
                                if (evt_.button == 1) // Right click
                                {
                                    ShowStackTraceContextMenu(stackContext);
                                    evt_.StopPropagation();
                                }
                            });
                        }
                        
                        _stackTraceScroll.Add(stackLabel);
                    }
                }
                
                if (_selectedLogItem.StackList.Count == 0)
                {
                    var noStackLabel = new Label("No stack trace available");
                    noStackLabel.AddToClassList("stack-trace-content");
                    _stackTraceScroll.Add(noStackLabel);
                }
            }
            else
            {
                // Fallback to original StackString if StackList is not available
                string stackTrace = _selectedLogItem.StackString ?? "";
                if (string.IsNullOrEmpty(stackTrace))
                {
                    stackTrace = "No stack trace available";
                }
                
                var stackLabel = new Label(stackTrace);
                stackLabel.AddToClassList("stack-trace-content");
                _stackTraceScroll.Add(stackLabel);
            }
        }

        private string CleanHtmlTags(string input_)
        {
            if (string.IsNullOrEmpty(input_)) return input_;
            
            // Remove HTML tags using simple string replacement
            // This is safer than regex for this specific case
            string result = input_;
            
            // Remove <a> tags but keep the content
            int startIndex = 0;
            while ((startIndex = result.IndexOf("<a ", startIndex)) != -1)
            {
                int endIndex = result.IndexOf(">", startIndex);
                if (endIndex != -1)
                {
                    result = result.Remove(startIndex, endIndex - startIndex + 1);
                }
                else
                {
                    break; // Malformed tag, stop processing
                }
            }
            
            // Remove closing </a> tags
            result = result.Replace("</a>", "");
            
            return result;
        }

        private void ShowStackTraceContextMenu(LogItem.StackContext stackContext_)
        {
            var menu = new UnityEditor.GenericMenu();
            
            if (System.IO.File.Exists(stackContext_.FilePath))
            {
                menu.AddItem(new UnityEngine.GUIContent("Open Source File"), false, () => {
                    LogItem.OpenStackTraceFile(stackContext_.FilePath, stackContext_.LineNumber);
                });
            }
            else
            {
                menu.AddDisabledItem(new UnityEngine.GUIContent("Open Source File"));
            }
            
            menu.AddItem(new UnityEngine.GUIContent("Copy Selected"), false, () => {
                UnityEditor.EditorGUIUtility.systemCopyBuffer = stackContext_.OriginalStackString;
            });
            
            menu.AddItem(new UnityEngine.GUIContent("Copy All"), false, () => {
                if (_selectedLogItem != null)
                {
                    UnityEditor.EditorGUIUtility.systemCopyBuffer = _selectedLogItem.StackString;
                }
            });
            
            menu.ShowAsContext();
        }
    }
}