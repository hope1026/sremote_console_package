// 
// Copyright 2023 hope1026.

using UnityEngine;

namespace Sample.Scripts
{
    public class SampleMoveComponent : MonoBehaviour
    {
        [SerializeField] private Vector3 _startPos;
        [SerializeField] private Vector3 _endPos;
        private float _accumulateTime;
        private bool _isReverseMovement = false;
        public bool MoveEnabled { get; set; } = true;

        private void Update()
        {
            if (MoveEnabled == false)
                return;

            _accumulateTime += Time.deltaTime;
            if (_isReverseMovement)
            {
                base.transform.position = Vector3.Lerp(_startPos, _endPos, _accumulateTime);
            }
            else
            {
                base.transform.position = Vector3.Lerp(_endPos, _startPos, _accumulateTime);
            }

            if (1f < _accumulateTime)
            {
                _accumulateTime = 0f;
                _isReverseMovement = !_isReverseMovement;
            }
        }
    }
}