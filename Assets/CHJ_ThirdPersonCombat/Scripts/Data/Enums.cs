using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ThirdPerson_InputAction ������ InputAction �̸��� Enum���� ����
/// Enum �̸��� ThirdPerson_InputAction ���Ͽ� ���ǵ� �̸��� �����ؾ� ��
/// </summary>
public enum EInputKey 
{
    Look,
    Zoom,
    Move,
    Attack,
    Skill_01,
    Skill_02,
    Dash,
    Interaction,
    Menu,
}

public enum EInputState
{
    None,
    Down,
    Hold,
    Up,
}