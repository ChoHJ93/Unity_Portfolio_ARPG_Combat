using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private CharacterController _characterController;
    private UnitMovement _unitMovement;
    private UnitAniEventController _aniEventController;

    private Dictionary<EInputKey, SkillData> _skillDataDic;
    public UnitMovement UnitMovement => _unitMovement;

    private void Awake()
    {
        _unitMovement = GetComponent<UnitMovement>();
        _aniEventController = GetComponent<UnitAniEventController>();

        _skillDataDic = new Dictionary<EInputKey, SkillData>();

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
