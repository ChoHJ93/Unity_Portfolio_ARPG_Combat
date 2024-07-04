using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private const int s_livePriority = 10;
    private const int s_standbyPriority = 0;
    private const float s_rotateThreshold = 0.1f;

    private Camera _camera;
    private CinemachineVirtualCameraBase _virtualCamera;

    [Header("RotateSetting")]
    [SerializeField] private float _rotateSpeed = 1.0f;
    [SerializeField] private float _rotateDamping = 0.1f;
    [SerializeField] private float _rotateTopClamp = 70.0f;
    [SerializeField] private float _rotateBottomClamp = -30.0f;

    private Vector2 _rotateInput = Vector2.zero;
    private Vector2 _rotateValue = Vector2.zero;
    private bool _ignoreInput = false;

    private Transform FollowTarget => _virtualCamera.Follow;
    public Transform CameraTr => _camera.transform;

    private void Awake()
    {
        _camera = _camera ?? Camera.main;
        _virtualCamera = _virtualCamera ?? GetComponentInChildren<CinemachineVirtualCameraBase>();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<EventLookInput>(SetLookInput);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<EventLookInput>(SetLookInput);
    }

    private void SetLookInput(EventLookInput eventLookInput) 
    {
        if (_ignoreInput)
        {
            _rotateInput = Vector2.zero;
            return;
        }

        _rotateInput = eventLookInput.value;
    }

    private void LateUpdate()
    {
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            UpdateRotate();
        }   
    }

    private void UpdateRotate() 
    {
        if(FollowTarget == null)
            return;

        if (_rotateInput.magnitude > s_rotateThreshold)
        {
            _rotateValue += _rotateInput;
        }

        float rotatePitch = ClampAngle(_rotateValue.y, _rotateBottomClamp, _rotateTopClamp);
        float rotateYaw = _rotateValue.x;
        FollowTarget.rotation = Quaternion.Euler(rotatePitch, rotateYaw, 0);
    }
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
