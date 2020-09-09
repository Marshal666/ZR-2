﻿using System.Collections;
using System.Collections.Generic;
using System;
public class StateMachine<T> where T : struct, IConvertible     //T is enum
{

    private Dictionary<T, Action> methods;

    private Dictionary<T, Dictionary<T, Action>> switches;

    T state;

    /// <summary>
    /// Creates a new state machine object
    /// </summary>
    /// <param name="startState">Starting state of this machine</param>
    public StateMachine(T startState)
    {
        state = startState;

        T[] keys = (T[])Enum.GetValues(typeof(T));

        methods = new Dictionary<T, Action>(keys.Length);
        for(int i = 0; i < keys.Length; i++)
        {
            methods.Add(keys[i], null);
        }

        switches = new Dictionary<T, Dictionary<T, Action>>(keys.Length);
        for(int i = 0; i < keys.Length; i++)
        {
            switches.Add(keys[i], new Dictionary<T, Action>(keys.Length - 1));
            for(int j = 0; j < keys.Length; j++)
            {
                switches[keys[i]].Add(keys[j], null);
            }
        }
    }
    
    /// <summary>
    /// Creates a new state machine object with predifined state methods
    /// </summary>
    /// <param name="startState">Starting state of this machine</param>
    /// <param name="baseActs">Methods for states</param>
    public StateMachine(T startState, Action[] baseActs)
    {
        state = startState;

        T[] keys = (T[])Enum.GetValues(typeof(T));

        methods = new Dictionary<T, Action>(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {
            methods.Add(keys[i], baseActs[i]);
        }

        switches = new Dictionary<T, Dictionary<T, Action>>(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {
            switches.Add(keys[i], new Dictionary<T, Action>(keys.Length - 1));
            for (int j = 0; j < keys.Length; j++)
            {
                switches[keys[i]].Add(keys[j], null);
            }
        }
    }

    /// <summary>
    /// Switches state of this machine
    /// </summary>
    /// <param name="newState">New state for this machine</param>
    public void SwitchState(T newState)
    {
        switches[state][newState]?.Invoke();
        state = newState;
    }

    /// <summary>
    /// Switches to a new state only if transition method to new state from current one is defined
    /// </summary>
    /// <param name="newState">Potential new state for this machine</param>
    public void SwitchStateConditional(T newState)
    {
        var m = switches[state][newState];
        if(m != null)
        {
            m();
            state = newState;
        }
    }

    /// <summary>
    /// Calls current states method, if method is not assigned then it does nothing
    /// </summary>
    public void Execute()
    {
        methods[state]?.Invoke();
    }

    /// <summary>
    /// Gets or switches a state machine state
    /// </summary>
    public T State { get { return state; } set { SwitchState(value); } }

    /// <summary>
    /// State methods dictionary
    /// </summary>
    public Dictionary<T, Action> Methods { get { return methods; } }

    /// <summary>
    /// State switching methods dictionary
    /// </summary>
    public Dictionary<T, Dictionary<T, Action>> Switches { get { return switches; } }
    

}
