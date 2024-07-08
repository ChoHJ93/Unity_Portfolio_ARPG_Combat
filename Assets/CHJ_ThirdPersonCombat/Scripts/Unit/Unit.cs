using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private readonly EInputKey[] CombatInputKeys = new EInputKey[] { EInputKey.Attack, EInputKey.Skill_01, EInputKey.Skill_02, EInputKey.Dash };

    private CharacterController _characterController;
    private UnitMovement _unitMovement;
    private UnitAniEventController _aniEventController;

    private Dictionary<EInputKey, SkillData> _skillDataDic;

    [SerializeField] private UnitSkillTable _unitSkillTable;

    public UnitMovement UnitMovement => _unitMovement;

    private void Awake()
    {
        _unitMovement = GetComponent<UnitMovement>();
        _aniEventController = GetComponent<UnitAniEventController>();

        _skillDataDic = new Dictionary<EInputKey, SkillData>();

        if (_unitSkillTable != null)
        {

        }
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void OnSkillInputCalled(EventCommonInput eventCommonInput)
    {
        switch (eventCommonInput.inputKey)
        {
            case EInputKey.Attack:
                break;
            case EInputKey.Skill_01:
                break;
            case EInputKey.Skill_02:
                break;
            case EInputKey.Dash:
                break;
        }
    }
}
