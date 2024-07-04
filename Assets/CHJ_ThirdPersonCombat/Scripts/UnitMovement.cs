using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Unit))]
public class UnitMovement : MonoBehaviour
{
    private Unit _unit;
    private CharacterController _characterController;

    private Vector2 _moveInput;
    private bool _ignoreInput = false;
    private Vector3 _moveDirection = Vector3.zero;
    private float _lookRotationY = 0f;
    private float _rotationVelocity = 0f;

    [SerializeField] 
    protected float _moveSpeed = 5f;
    [Range(0.0f, 0.3f)]
    [SerializeField] 
    protected float _rotationSmoothTime = 0.1f;
    private Transform CameraTr => GameManager.Instance.CameraController.CameraTr;

    public Vector2 MoveInput => _moveInput;


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
        if(GameManager.Instance.IsGameStart == false)
            return;

        if (_ignoreInput)
        {
            _moveInput = Vector2.zero;
            return;
        }

        UpdateMove();
    }


    private void SetMoveInput(EventMoveInput eventMoveInput)
    {
        _moveInput = eventMoveInput.value.normalized;
    }

    private void UpdateMove()
    {
        float lookRotationY = Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg + CameraTr.eulerAngles.y;
        _moveDirection = Quaternion.Euler(0, lookRotationY, 0) * Vector3.forward;

        if (_moveInput.magnitude < 0.01f)
        {
            StopMove();
            return;
        }

        SetRotationY(lookRotationY);

        _characterController.Move(_moveDirection * _moveSpeed * Time.deltaTime);


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
}
