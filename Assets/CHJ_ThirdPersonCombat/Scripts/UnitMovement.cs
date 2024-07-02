using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Unit))]
public class UnitMovement : MonoBehaviour
{
    private Unit _unit;
    private CharacterController _characterController;

    private Vector2 _moveInput;

    [SerializeField] protected float _moveSpeed = 5f;
    public Vector2 MoveInput => _moveInput;


    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<EventMoveInput>(SetMoveInput);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<EventMoveInput>(SetMoveInput);
    }
    private void Update()
    {
        
    }


    private void SetMoveInput(EventMoveInput eventMoveInput) 
    {
        _moveInput = eventMoveInput.value.normalized;
    }

    private void UpdateMove() 
    {

    }
}
