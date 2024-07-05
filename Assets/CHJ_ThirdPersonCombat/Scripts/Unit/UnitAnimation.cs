using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UnitAnimation : MonoBehaviour
{
    private Unit _unit;
    private Animator _animator;

    private bool _lastMoveInput = false;
    private float _moveInputValue = 0f;
    private float _moveAniBlendVelocity = 0f;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        SetMoveAnimation(_unit.UnitMovement.MoveInput);
    }

    private void SetMoveAnimation(Vector2 moveInput)
    {
        float moveInputValue = moveInput.normalized.magnitude;
        bool isMoving = moveInputValue > 0.01f;

        if (_animator == null)
            return;

        if (_lastMoveInput != isMoving)
        {
            if (isMoving)
                _animator.SetTrigger("Move");
            else
                _animator.ResetTrigger("Move");

            _lastMoveInput = isMoving;
        }

        _moveInputValue = Mathf.SmoothDamp(_moveInputValue, moveInputValue, ref _moveAniBlendVelocity, 0.1f);
        _animator.SetFloat("MoveInputValue", _moveInputValue);


        _lastMoveInput = isMoving;
    }
}
