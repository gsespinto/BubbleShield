using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent[] events;

    public void CallEvent(int index)
    {
        if (index >= events.Length)
            return;

        events[index].Invoke();
    }

    public void AddEvent(UnityEvent e)
    {
        UnityEvent[] newEvents = new UnityEvent[events.Length + 1];
        for (int i = 0; i < events.Length; i++)
        {
            newEvents[i] = events[i];
        }

        newEvents[events.Length] = e;
        events = newEvents;
    }

    public void AddEvents(UnityEvent[] e)
    {
        UnityEvent[] newEvents = new UnityEvent[events.Length + e.Length];
        for (int i = 0; i < events.Length; i++)
        {
            newEvents[i] = events[i];
        }

        for (int i = 0; i < e.Length; i++)
        {
            newEvents[events.Length + i] = e[i];
        }
        
        events = newEvents;
    }
}
