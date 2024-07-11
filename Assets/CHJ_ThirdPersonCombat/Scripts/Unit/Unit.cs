using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Unit : MonoBehaviour
{
    private readonly EInputKey[] CombatInputKeys = new EInputKey[] { EInputKey.Attack, EInputKey.Skill_01, EInputKey.Skill_02, EInputKey.Dash };

    private CharacterController _characterController;
    private UnitMovement _unitMovement;
    private UnitSkill _unitSkill;
    private UnitAniEventController _aniEventController;

    private UnitAnimation _unitAnimation;

    public UnitAnimation UnitAnimation => _unitAnimation;
    public UnitMovement UnitMovement => _unitMovement;

    private void Awake()
    {
        TryGetComponent(out _characterController);
        TryGetComponent(out _unitMovement);
        TryGetComponent(out _unitSkill);
        TryGetComponent(out _aniEventController);

        if (TryGetComponent(out Animator animator))
        {
            _unitAnimation = new UnitAnimation(this, animator);
        }
    }

    private void OnEnable()
    {
        if(_unitSkill != null)
        {
            _unitSkill.OnSkillStart += OnSkillStart;
            _unitSkill.OnSkillEnd += OnSkillEnd;
        }
    }

    private void OnDisable()
    {
        if(_unitSkill != null)
        {
            _unitSkill.OnSkillStart -= OnSkillStart;
            _unitSkill.OnSkillEnd -= OnSkillEnd;
        }
    }

    private void OnSkillStart() 
    {
        if(_unitMovement)
            _unitMovement.SetEnableMove(false);
    }

    private void OnSkillEnd() 
    {
        if(_unitMovement)
            _unitMovement.SetEnableMove(true);
    }

    
}
