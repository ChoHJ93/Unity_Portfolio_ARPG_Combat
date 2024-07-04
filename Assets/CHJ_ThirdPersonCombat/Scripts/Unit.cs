using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;
    private UnitMovement _unitMovement;
    private UnitAniEventController _aniEventController;

    public UnitMovement UnitMovement => _unitMovement;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _unitMovement = GetComponent<UnitMovement>();
        _aniEventController = GetComponent<UnitAniEventController>();
    }

    #region Animation Control
    #endregion
}
