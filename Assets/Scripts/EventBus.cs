using System;
using System.Collections.Generic;
using UnityEngine;

// This event bus works by associating an event name with possibly many actions that are triggered by that event,
// each of which can have different method signatures. In some designs, you would create an object type for each 
// event and that object would be created each time it's called. This design says that you can have many signatures
// instead of one centralized object type since different classes may want to provide different information.
//
// I need to do more research to know if this is good or bad though, so the system may be refined or rebuilt later on. 

public static class EventBus
{
    private static readonly Dictionary<string, List<Delegate>> _events = new();

    // If you want to represent multiple parameters, T just needs to be a tuple (or another simple collection)
    public static void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (!_events.ContainsKey(eventName))
        {
            // If no actions for eventName exist, create a list of generic actions (Delegates) and add the listener to it
            _events[eventName] = new List<Delegate> { listener };
        }
        else
        {
            List<Delegate> actions = _events[eventName]!;
            for (var i = 0; i < actions.Count; i++)
            {
                if (actions[i] is not Action<T> existingAction) continue;
                // Checks IF type cast is valid, creates a new variable with that type if so
                actions[i] = existingAction + listener;
                // If an existing delegate with type Action<T> exists, stack the listener with it
                return;
            }
            // If no existing delegate under Action<T> exists, add a new one to the list of actions for eventName
            _events[eventName].Add(listener);
        }
    }

    public static void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        // See Subscribe() for details on how it works
        if (!_events.ContainsKey(eventName)) return;
        
        List<Delegate> actions = _events[eventName]!;
        for (var i = 0; i < actions.Count; i++)
        {
            if (actions[i] is not Action<T> existingAction) continue;
            
            actions[i] = existingAction - listener;
            if (actions[i] == null) // If you removed the last action in that Action<T> actions stack
            {
                _events[eventName].Remove(actions[i]); // Otherwise actions[i] is a null reference (eek)
            }
        }
    }

    public static void Trigger<T>(string eventName, T param)
    {
        if (!_events.ContainsKey(eventName)) return;
        
        List<Delegate> actions = _events[eventName]!;
        foreach(Delegate action in actions) 
        {
            if (action is not Action<T> existingAction) continue;
            
            existingAction.Invoke(param);
            return;
        }
        
        // If you didn't return, you didn't find a matching action in the delegate list
        Debug.LogWarning($"No event {eventName} with parameter type {typeof(T).Name}");
    }
    
    
    // ===== ZERO PARAMETER OVERLOADS =====
    public static void Subscribe(string eventName, Action listener)
    {
        if (!_events.ContainsKey(eventName))
        {
            // If no actions for eventName exist, create a list of generic actions (Delegates) and add the listener to it
            _events[eventName] = new List<Delegate> { listener };
        }
        else
        {
            List<Delegate> actions = _events[eventName]!;
            for (var i = 0; i < actions.Count; i++)
            {
                if (actions[i] is not Action existingAction) continue;
                // Checks IF type cast is valid, creates a new variable with that type if so
                actions[i] = existingAction + listener;
                // If an existing delegate with type Action<T> exists, stack the listener with it
                return;
            }
            // If no existing delegate under Action<T> exists, add a new one to the list of actions for eventName
            _events[eventName].Add(listener);
        }
    }
    
    public static void Unsubscribe(string eventName, Action listener)
    {
        // See Subscribe() for details on how it works
        if (!_events.ContainsKey(eventName)) return;
        
        List<Delegate> actions = _events[eventName]!;
        for (var i = 0; i < actions.Count; i++)
        {
            if (actions[i] is not Action existingAction) continue;
            
            actions[i] = existingAction - listener;
            if (actions[i] == null) // If you removed the last action in that Action<T> actions stack
            {
                _events[eventName].Remove(actions[i]); // Otherwise actions[i] is a null reference (eek)
            }
        }
    }
    
    public static void Trigger(string eventName)
    {
        if (!_events.ContainsKey(eventName)) return;
        
        List<Delegate> actions = _events[eventName]!;
        foreach(Delegate action in actions) 
        {
            if (action is not Action existingAction) continue;
            
            existingAction.Invoke();
            return;
        }
        
        // If you didn't return, you didn't find a matching action in the delegate list
        Debug.LogWarning($"No event {eventName} with zero parameters");
    }
}
