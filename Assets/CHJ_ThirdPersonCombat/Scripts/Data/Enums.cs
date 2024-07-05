using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ThirdPerson_InputAction 파일의 InputAction 이름을 Enum으로 정의
/// Enum 이름은 ThirdPerson_InputAction 파일에 정의된 이름과 동일해야 함
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