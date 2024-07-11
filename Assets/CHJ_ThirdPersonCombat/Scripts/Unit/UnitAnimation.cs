using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimation
{
    private Unit _unit;
    private Animator _animator;

    private bool _lastMoveInput = false;
    private float _moveInputValue = 0f;
    private float _moveAniBlendVelocity = 0f;

    public UnitAnimation(Unit unit, Animator animator)
    {
        _unit = unit;
        _animator = animator;
    }

    public void SetMoveAnimation(Vector2 moveInput, bool forcePlay = false)
    {
        float moveInputValue = moveInput.normalized.magnitude;
        bool isMoving = moveInputValue > 0.01f;

        if (_animator == null)
            return;

        if (_lastMoveInput != isMoving)
        {
            if (isMoving && forcePlay)
                _animator.SetTrigger("Move");
            else
                _animator.ResetTrigger("Move");

            _lastMoveInput = isMoving;
        }

        _moveInputValue = Mathf.SmoothDamp(_moveInputValue, moveInputValue, ref _moveAniBlendVelocity, 0.1f);
        _animator.SetFloat("MoveInputValue", _moveInputValue);


        _lastMoveInput = isMoving;
    }

    public void PlayAni(string aniStateName, bool crossFade = false, float transitionDuration = 0.1f) 
    {
        if (crossFade)
            _animator.CrossFade(aniStateName, transitionDuration);
        else
            _animator.Play(aniStateName);
    }
}
