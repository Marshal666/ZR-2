using System.Collections;
using System.Collections.Generic;
using System;
public class StateMachine<T> where T : struct, IConvertible     //T is enum
{

    private Dictionary<T, Action> methods;

    private Dictionary<T, Dictionary<T, Action>> switches;

    T state;

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
                if (i != j)
                    switches[keys[i]].Add(keys[j], null);
            }
        }
    }

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
                if (i != j)
                    switches[keys[i]].Add(keys[j], null);
            }
        }
    }

    public void SwitchState(T newState)
    {
        switches[state][newState]?.Invoke();
        state = newState;
    }

    public void SwitchStateConditional(T newState)
    {
        var m = switches[state][newState];
        if(m != null)
        {
            m();
            state = newState;
        }
    }

    public void Execute()
    {
        methods[state]?.Invoke();
    }

    public T State { get { return state; } set { SwitchState(value); } }

    public Dictionary<T, Action> Methods { get { return methods; } }

    public Dictionary<T, Dictionary<T, Action>> Switches { get { return switches; } }
    

}
