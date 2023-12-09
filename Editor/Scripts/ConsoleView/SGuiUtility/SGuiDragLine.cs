// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class SGuiDragLine
    {
        public bool OnGuiCustom(ref Vector2 mousePosition_, Rect lineRect_, Rect collisionOffset_, Color backgroundColor_, GUIStyle guiStyle_, bool isVertical_)
        {
            bool isDragged = false;
            SGuiUtility.BeginBackgroundColor(backgroundColor_);

            GUI.Box(lineRect_, "", guiStyle_);

            SGuiUtility.EndBackgroundColor();

            Rect collisionRect = new Rect(lineRect_.x + collisionOffset_.x,
                                          lineRect_.y + collisionOffset_.y,
                                          lineRect_.width + collisionOffset_.width,
                                          lineRect_.height + collisionOffset_.height);
            MouseCursor cursor = MouseCursor.Arrow;
            if (true == isVertical_)
            {
                cursor = MouseCursor.ResizeHorizontal;
            }
            else
            {
                cursor = MouseCursor.ResizeVertical;
            }

            EditorGUIUtility.AddCursorRect(collisionRect, cursor);

            if (true == collisionRect.Contains(Event.current.mousePosition))
            {
                if (EventType.MouseDown == Event.current.type)
                {
                    _isSelectedStackTraceList = true;
                }
            }

            if (EventType.MouseUp == Event.current.type)
            {
                _isSelectedStackTraceList = false;
            }
            else if (EventType.MouseDrag == Event.current.type && true == _isSelectedStackTraceList)
            {
                mousePosition_ = Event.current.mousePosition;
                isDragged = true;
            }

            return isDragged;
        }

        private bool _isSelectedStackTraceList = false;
    }
} /*SPlugin*/