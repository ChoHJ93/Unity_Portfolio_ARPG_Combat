using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventBase { }

public class EventCommonInput : EventBase 
{
    public EInputKey inputKey { get; protected set; }
    public EInputState inputState { get; protected set; }

    public EventCommonInput(EInputKey inputKey, EInputState inputState)
    {
        this.inputKey = inputKey;
        this.inputState = inputState;
    }
}

public class EventMoveInput : EventCommonInput 
{
    public Vector2 value { get; private set; }

    public EventMoveInput(Vector2 value, EInputState inputState) : base(EInputKey.Move, inputState)
    {
        this.value = value;
    }

    public void SetMoveInput(Vector2 value, EInputState inputState)
    {
        this.value = value;
        this.inputState = inputState;
    }
}

public class EventLookInput : EventCommonInput 
{
    public Vector2 value { get; private set; }

    public EventLookInput(Vector2 value, EInputState inputState) : base(EInputKey.Look, inputState)
    {
        this.value = value;
    }

    public void SetLookInput(Vector2 value, EInputState inputState)
    {
        this.value = value;
        this.inputState = inputState;
    }
}
     