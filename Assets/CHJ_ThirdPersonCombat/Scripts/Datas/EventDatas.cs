using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventBase { }

public class EventCommonInput : EventBase 
{
    public EInputKey inputKey { get; private set; }
    public EInputState inputState { get; private set; }
    public Vector2 value { get; set; }

    public EventCommonInput(EInputKey inputKey, EInputState inputState)
    {
        this.inputKey = inputKey;
        this.inputState = inputState;
    }
}
     