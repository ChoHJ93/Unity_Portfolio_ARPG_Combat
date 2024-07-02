using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.InputSystem;

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

    private bool _isCursorLocked = false;

    //for Invoke on performed event
    private EventMoveInput _eventMoveInput; 
    private EventLookInput _eventLookInput; 

    private void Awake()
    {
        Initialize();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (_isCursorLocked == false)
        { 
            Cursor.lockState = CursorLockMode.Locked;
            _isCursorLocked = true;
        }
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
        switch (eInputKey)
        {
            case EInputKey.Menu:
                {
                    action.performed += OnMenuInputPerformed;
                }
                break;
            case EInputKey.Move:
                {
                    _eventMoveInput = new EventMoveInput(Vector2.zero, EInputState.None);
                    action.performed += OnMoveInputCalled;
                    action.canceled += OnMoveInputCalled;
                }
                break;

            case EInputKey.Look:
                {
                    _eventLookInput = new EventLookInput(Vector2.zero, EInputState.None);
                    action.performed += OnLookInputCalled;
                    action.canceled += OnLookInputCalled;
                }
                break;

            default:
                {
                    bool hasHoldAction = _isHoldActions[index];

                    if (hasHoldAction)
                        action.performed += OnInputEventCalled;

                    action.started += OnInputEventCalled;
                    action.canceled += OnInputEventCalled;
                }
                break;
        }
    }
    private void OnMoveInputCalled(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        EInputState inputState = GetInputState(context);

        _eventMoveInput.SetMoveInput(value, inputState);
        EventManager.Instance.ExecuteEvent(_eventMoveInput);
    }

    private void OnLookInputCalled(InputAction.CallbackContext context) 
    {
        Vector2 value = context.ReadValue<Vector2>();
        EInputState inputState = GetInputState(context);

        _eventLookInput.SetLookInput(value, inputState);
        EventManager.Instance.ExecuteEvent(_eventLookInput);
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
        //eventCommonInput.value = context.action.expectedControlType == "Vector2" ? context.ReadValue<Vector2>() : Vector2.zero;

        EventManager.Instance.ExecuteEvent(eventCommonInput);
    }

    private void OnMenuInputPerformed(InputAction.CallbackContext context) 
    {
        if (_isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            _isCursorLocked = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            _isCursorLocked = true;
        }
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
