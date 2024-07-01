using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate<T>(T e) where T : EventBase;
    private delegate void EventDelegate(EventBase e);

    private Dictionary<Type, EventDelegate> _delegates = new Dictionary<Type, EventDelegate>();
    private Dictionary<Delegate, EventDelegate> _delegateLookup = new Dictionary<Delegate, EventDelegate>();

    public override void Clear()
    {
        base.Clear();
        _delegates.Clear();
        _delegateLookup.Clear();
    }

    public void AddListener<T>(EventDelegate<T> del) where T : EventBase
    {
        if (_delegateLookup.ContainsKey(del))
        {
            return;
        }

        EventDelegate internalDelegate = (e) => del((T)e);
        _delegateLookup[del] = internalDelegate;

        EventDelegate tempDel;
        if (_delegates.TryGetValue(typeof(T), out tempDel))
        {
            _delegates[typeof(T)] = tempDel += internalDelegate;
        }
        else
        {
            _delegates[typeof(T)] = internalDelegate;
        }
    }

    public void RemoveListener<T>(EventDelegate<T> del) where T : EventBase
    {
        EventDelegate internalDelegate;
        if (_delegateLookup.TryGetValue(del, out internalDelegate))
        {
            EventDelegate tempDel;
            if (_delegates.TryGetValue(typeof(T), out tempDel))
            {
                tempDel -= internalDelegate;
                if (tempDel == null)
                {
                    _delegates.Remove(typeof(T));
                }
                else
                {
                    _delegates[typeof(T)] = tempDel;
                }
            }

            _delegateLookup.Remove(del);
        }
    }

    public void ExecuteEvent(EventBase eventBase) 
    {
        EventDelegate del;
        if (_delegates.TryGetValue(eventBase.GetType(), out del))
        {
            del.Invoke(eventBase);
        }
    }
}