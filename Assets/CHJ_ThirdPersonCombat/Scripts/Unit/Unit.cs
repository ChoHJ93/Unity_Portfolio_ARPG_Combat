using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private readonly EInputKey[] CombatInputKeys = new EInputKey[] { EInputKey.Attack, EInputKey.Skill_01, EInputKey.Skill_02, EInputKey.Dash };

    private CharacterController _characterController;
    private UnitMovement _unitMovement;
    private UnitSkill _unitSkill;

    private UnitAnimation _unitAnimation;

    public UnitAnimation UnitAnimation => _unitAnimation;
    public UnitMovement UnitMovement => _unitMovement;
    public UnitSkill UnitSkill => _unitSkill;

    private void Awake()
    {
        TryGetComponent(out _characterController);
        TryGetComponent(out _unitMovement);
        TryGetComponent(out _unitAnimation);
        TryGetComponent(out _unitSkill);
    }

    private void OnEnable()
    {
        if (_unitSkill != null)
        {
            _unitSkill.OnSkillStart += OnSkillStart;
            _unitSkill.OnSkillEnd += OnSkillEnd;

            if (_unitAnimation != null)
                _unitAnimation.OnAniStateEnd += _unitSkill.EndCurrentSkill;
        }
    }

    private void OnDisable()
    {
        if (_unitSkill != null)
        {
            _unitSkill.OnSkillStart -= OnSkillStart;
            _unitSkill.OnSkillEnd -= OnSkillEnd;

            if (_unitAnimation != null)
                _unitAnimation.OnAniStateEnd -= _unitSkill.EndCurrentSkill;
        }
    }

    private void OnSkillStart()
    {
        if (_unitMovement)
            _unitMovement.SetEnableMove(false);
    }

    private void OnSkillEnd()
    {
        if (_unitMovement)
            _unitMovement.SetEnableMove(true);
    }

}
