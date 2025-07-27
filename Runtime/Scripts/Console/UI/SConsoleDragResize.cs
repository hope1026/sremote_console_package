// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SPlugin.RemoteConsole.Runtime
{
    /// <summary>
    /// Handles drag-to-resize functionality for console panel
    /// </summary>
    public class SConsoleDragResize : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        // Instance fields
        [Header("Resize Settings")]
        [SerializeField] private RectTransform _targetPanel;
        [SerializeField] private Vector2 _minSize = new Vector2(300, 200);
        [SerializeField] private Vector2 _maxSize = new Vector2(1600, 1200);
        [SerializeField] private float _resizeHandleSize = 20f;
        
        private bool _isDragging = false;
        private Vector2 _lastPointerPosition;
        private RectTransform _rectTransform;
        private Canvas _parentCanvas;
        
        // Properties
        public RectTransform TargetPanel
        {
            get => _targetPanel;
            set => _targetPanel = value;
        }
        
        // Methods
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            
            // Setup resize handle appearance
            SetupResizeHandle();
        }
        
        private void SetupResizeHandle()
        {
            // Add visual indicator for resize handle
            Image handleImage = GetComponent<Image>();
            if (handleImage == null)
            {
                handleImage = gameObject.AddComponent<Image>();
            }
            
            // Semi-transparent corner indicator
            handleImage.color = new Color(1f, 1f, 1f, 0.3f);
            
            // Position at bottom-right corner
            _rectTransform.anchorMin = new Vector2(1f, 0f);
            _rectTransform.anchorMax = new Vector2(1f, 0f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.sizeDelta = new Vector2(_resizeHandleSize, _resizeHandleSize);
        }
        
        public void OnPointerDown(PointerEventData eventData_)
        {
            _isDragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                eventData_.position,
                _parentCanvas.worldCamera,
                out _lastPointerPosition
            );
        }
        
        public void OnDrag(PointerEventData eventData_)
        {
            if (!_isDragging || _targetPanel == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentCanvas.transform as RectTransform,
                    eventData_.position,
                    _parentCanvas.worldCamera,
                    out Vector2 currentPointerPosition))
            {
                Vector2 deltaPosition = currentPointerPosition - _lastPointerPosition;
                ResizePanel(deltaPosition);
                _lastPointerPosition = currentPointerPosition;
            }
        }
        
        public void OnPointerUp(PointerEventData eventData_)
        {
            _isDragging = false;
        }
        
        private void ResizePanel(Vector2 deltaPosition_)
        {
            Vector2 currentSize = _targetPanel.sizeDelta;
            Vector2 newSize = currentSize + deltaPosition_;
            
            // Clamp to min/max size
            newSize.x = Mathf.Clamp(newSize.x, _minSize.x, _maxSize.x);
            newSize.y = Mathf.Clamp(newSize.y, _minSize.y, _maxSize.y);
            
            _targetPanel.sizeDelta = newSize;
        }
        
        /// <summary>
        /// Sets the target panel to resize
        /// </summary>
        /// <param name="targetPanel_">Panel to resize</param>
        public void SetTargetPanel(RectTransform targetPanel_)
        {
            _targetPanel = targetPanel_;
        }
        
        /// <summary>
        /// Sets the size constraints for resizing
        /// </summary>
        /// <param name="minSize_">Minimum size</param>
        /// <param name="maxSize_">Maximum size</param>
        public void SetSizeConstraints(Vector2 minSize_, Vector2 maxSize_)
        {
            _minSize = minSize_;
            _maxSize = maxSize_;
        }
    }
}