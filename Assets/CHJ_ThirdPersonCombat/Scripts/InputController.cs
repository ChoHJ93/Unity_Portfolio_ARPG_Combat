using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class InputController : MonoBehaviour
{
    //variables
    private PlayerInput _playerInput;   
    private InputAction[] _actions;
    private bool[] _isHoldActions;
    private string _controlScheme;
    private int _devieIndex;
    private Dictionary<string, EInputKey> _dicInputKey = new Dictionary<string, EInputKey>();

    //for Invoke on performed event
    private EventCommonInput _eventMoveInput; 
    private EventCommonInput _eventLookInput; 

    private void Awake()
    {
        Initialize();
    }

    public void Initialize() 
    {
        if(_playerInput == null && !TryGetComponent(out _playerInput))
        {
            Debug.LogError("PlayerInput component not found");
            return;
        }

        _controlScheme = _playerInput.currentControlScheme;
        _playerInput.controlsChangedEvent.AddListener(OnInputDeviceChanged);

        EnableInputAction();
    }

    private void OnInputDeviceChanged(PlayerInput playerInput) 
    {
        if(_controlScheme == playerInput.currentControlScheme)
            return;

        _controlScheme = playerInput.currentControlScheme;

        EnableInputAction();
    }

    private void EnableInputAction() 
    {
        if(_playerInput == null)
            return;

        _devieIndex = _playerInput.actions.controlSchemes.IndexOf(scheme => scheme.name.Equals(_playerInput.currentControlScheme));

        InputActionMap actionMap = _playerInput.currentActionMap;
        _actions = actionMap.actions.ToArray();

        _isHoldActions = _actions.Select(action => action.bindings.Count > _devieIndex && action.bindings[_devieIndex].interactions.Contains("Hold")).ToArray();

        _dicInputKey.Clear();

        for(int i = 0; i < _actions.Length; i++)
        {
            InputAction action = _actions[i];

            //for loop EInputKey enum values and compare with action name
            foreach(EInputKey key in Enum.GetValues(typeof(EInputKey)))
            {
                if(action.name.Equals(key.ToString(), StringComparison.OrdinalIgnoreCase)
                    && _dicInputKey.ContainsKey(action.name) == false)
                {
                    _dicInputKey.Add(action.name, key);
                    BindActionToEvent(i, action, key);
                    break;
                }
            }
        }
    }

    private void BindActionToEvent(int index, InputAction action, EInputKey eInputKey) 
    {
        if (eInputKey == EInputKey.Move || eInputKey == EInputKey.Look)
        {
            action.performed += OnInputEventCalled;
            action.canceled += OnInputEventCalled;
        }
        else
        {
            bool hasHoldAction = _isHoldActions[index];

            if (hasHoldAction)
                action.performed += OnInputEventCalled;

            action.started += OnInputEventCalled;
            action.canceled += OnInputEventCalled;
        }

    }

    private void OnInputEventCalled(InputAction.CallbackContext context) 
    {
        string actionName = context.action.name;

        if(_dicInputKey.TryGetValue(actionName, out EInputKey key) == false)
        {
            Debug.LogError("지정되지 않은 키 입력!");
            return;
        }

        EInputState inputState = GetInputState(context);
        if(inputState == EInputState.None)
            return;

        EventCommonInput eventCommonInput = new EventCommonInput(key, inputState);
        eventCommonInput.value = context.action.expectedControlType == "Vector2" ? context.ReadValue<Vector2>() : Vector2.zero;

        EventManager.Instance.ExecuteEvent(eventCommonInput);
    }

    EInputState GetInputState(InputAction.CallbackContext _Context)
    {
        EInputState eState = EInputState.None;

        if (_Context.started)
            eState = EInputState.Down;
        else if (_Context.performed)
            eState = EInputState.Hold;
        else if (_Context.canceled)
            eState = EInputState.Up;

        return eState;
    }
}
