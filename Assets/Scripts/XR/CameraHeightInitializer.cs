using System;
using UnityEngine;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(XROrigin))]
public class CameraHeightInitializer : MonoBehaviour
{

    public static event Action OnStabilized;

    private const int StableFramesRequired = 10;

    private XROrigin _xrOrigin;
    private Transform _cameraOffsetTransform;
    private float _targetY;
    private int _stableFrames;
    private bool _hasFired;

    private void Awake()
    {
        _xrOrigin = GetComponent<XROrigin>();
        if (_xrOrigin == null) { enabled = false; return; }

        var offsetObj = _xrOrigin.CameraFloorOffsetObject;
        if (offsetObj == null) { enabled = false; return; }

        _cameraOffsetTransform = offsetObj.transform;
        _targetY = _xrOrigin.CameraYOffset;
    }

    private void LateUpdate()
    {
        var pos = _cameraOffsetTransform.localPosition;

        if (Mathf.Abs(pos.y - _targetY) > 0.01f)
        {
            pos.y = _targetY;
            _cameraOffsetTransform.localPosition = pos;
            _stableFrames = 0;
        }
        else
        {
            _stableFrames++;
            if (_stableFrames >= StableFramesRequired && !_hasFired)
            {
                _hasFired = true;
                OnStabilized?.Invoke();
                enabled = false;
            }
        }
    }

    private void OnDisable()
    {
        if (!_hasFired)
        {
            _hasFired = true;
            OnStabilized?.Invoke();
        }
    }
}
