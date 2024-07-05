using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private CharacterController _characterController;
    private UnitMovement _unitMovement;
    private UnitAniEventController _aniEventController;

    public UnitMovement UnitMovement => _unitMovement;

    private void Awake()
    {
        _unitMovement = GetComponent<UnitMovement>();
        _aniEventController = GetComponent<UnitAniEventController>();
    }

    private void OnEnable()
    {
        
    }
}
