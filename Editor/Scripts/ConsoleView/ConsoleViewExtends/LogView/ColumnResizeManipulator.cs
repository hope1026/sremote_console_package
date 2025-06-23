// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin
{
    internal class ColumnResizeManipulator : Manipulator
    {
        private readonly VisualElement _targetColumn;
        private readonly VisualElement _nextColumn;
        private readonly float _minWidth;
        private readonly string _columnType; // "time", "frame", "object"
        private bool _isDragging = false;
        private Vector2 _startPosition;
        private float _startTargetWidth;
        private float _startNextWidth;

        public event Action<string, float> OnColumnResized;

        public ColumnResizeManipulator(VisualElement targetColumn_, VisualElement nextColumn_, string columnType_, float minWidth_ = 40f)
        {
            _targetColumn = targetColumn_;
            _nextColumn = nextColumn_;
            _columnType = columnType_;
            _minWidth = minWidth_;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        private void OnMouseDown(MouseDownEvent evt_)
        {
            if (evt_.button != 0) return; // Only handle left mouse button

            _isDragging = true;
            _startPosition = evt_.mousePosition;
            _startTargetWidth = _targetColumn.resolvedStyle.width;
            _startNextWidth = _nextColumn?.resolvedStyle.width ?? 0;

            target.CaptureMouse();
            evt_.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt_)
        {
            if (!_isDragging) return;

            float deltaX = evt_.mousePosition.x - _startPosition.x;
            float newTargetWidth = Mathf.Max(_minWidth, _startTargetWidth + deltaX);
            
            // Update target column width
            _targetColumn.style.width = newTargetWidth;
            
            // If there's a next column, adjust its width to maintain total width
            if (_nextColumn != null)
            {
                float newNextWidth = Mathf.Max(_minWidth, _startNextWidth - deltaX);
                _nextColumn.style.width = newNextWidth;
            }

            // Notify about column resize
            OnColumnResized?.Invoke(_columnType, newTargetWidth);

            evt_.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt_)
        {
            if (!_isDragging) return;

            _isDragging = false;
            target.ReleaseMouse();
            evt_.StopPropagation();
        }

        private void OnMouseCaptureOut(MouseCaptureOutEvent evt_)
        {
            if (_isDragging)
            {
                _isDragging = false;
            }
        }
    }
}