using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Unit))]
public class UnitMovement : MonoBehaviour
{
    private Unit _unit;
    private CharacterController _characterController;

    //move
    private Vector2 _moveInput;
    private Vector3 _moveDirection = Vector3.zero;
    private bool _ignoreInput = false;
    private bool _enableMove = true;
    private bool _canCancelOtherAnimation = false;

    //rotation
    private float _lookRotationY = 0f;
    private float _rotationVelocity = 0f;

    [SerializeField]
    protected float _moveSpeed = 5f;
    [Range(0.0f, 0.3f)]
    [SerializeField]
    protected float _rotationSmoothTime = 0.1f;
    private Transform CameraTr => GameManager.Instance.CameraController.CameraTr;

    public Vector2 MoveInput => _moveInput;
    public bool IgnoreInput
    {
        private get => _ignoreInput;
        set => _ignoreInput = value;
    }

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _characterController = GetComponent<CharacterController>();
    }
    private void OnEnable()
    {
        EventManager.Instance.AddListener<EventMoveInput>(SetMoveInput);
    }
    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<EventMoveInput>(SetMoveInput);
    }
    private void Update()
    {
        if (GameManager.Instance.IsGameStart == false)
            return;

        if (IgnoreInput)
        {
            _moveInput = Vector2.zero;
            return;
        }

        UpdateMove();
        UpdateAnimation();
    }


    private void SetMoveInput(EventMoveInput eventMoveInput)
    {
        _moveInput = eventMoveInput.value.normalized;
    }

    private void UpdateMove()
    {
        float lookRotationY = Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg + CameraTr.eulerAngles.y;
        _moveDirection = Quaternion.Euler(0, lookRotationY, 0) * Vector3.forward;

        if (_enableMove == false || _moveInput.magnitude < 0.01f)
        {
            StopMove();
            return;
        }

        SetRotationY(lookRotationY);

        _characterController.Move(_moveDirection * _moveSpeed * Time.deltaTime);


    }
    private void UpdateAnimation()
    {
        if (_enableMove)
            _unit.UnitAnimation.SetMoveAnimation(_moveInput, _canCancelOtherAnimation);
    }

    private void SetRotationY(float lookRotationY, bool immediately = false)
    {
        _lookRotationY = lookRotationY;
        if (immediately)
        {
            _unit.transform.rotation = Quaternion.Euler(0, lookRotationY, 0);
        }
        else
        {
            float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, _lookRotationY, ref _rotationVelocity, _rotationSmoothTime);
            _unit.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        }
    }

    private void StopMove()
    {
        _moveDirection = Vector3.zero;
    }

    public void SetEnableMove(bool enableMove, bool _cancelOtherAni = false)
    {
        _enableMove = enableMove;
        _canCancelOtherAnimation = _cancelOtherAni;
    }
}
