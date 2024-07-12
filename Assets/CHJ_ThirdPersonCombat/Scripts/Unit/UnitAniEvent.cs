using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHJ.SimpleAniEventTool;

[RequireComponent(typeof(Unit))]
public class UnitAniEvent : SimpleAniEvent
{
    Unit _unit;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public void NextSkillFlag() 
    {
        _unit.UnitSkill.SetNextSkillFlag();
    }

    public void EnableMove() 
    {
    }
}
