using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem
{

    IGameEvent[] Events;

    //capacity of stack
    int size = 16, 
    //current pos in a stack
    currentPos = 0,
    //number of non null elements on stack
    count = 0,
    //count of done elements on stack ready to be reversed
    doneCount = 0;

    public int Size { get { return size; } set { size = value; } }

    public EventSystem(int size = 16)
    {

        this.size = size;

        Events = new IGameEvent[size];

    }

    public void AddEvent(IGameEvent e)
    {

        if (e.Result == GameEventExecutionResult.Failed)
            return;

        Events[currentPos] = e;

        currentPos = (currentPos + 1) % size;

        count++;
        if (count > size)
            count = size;

        doneCount++;
        if (doneCount > count)
        {
            doneCount = count;
        }
        else
        {

            int del = 0;
            for(int i = currentPos, j = doneCount; j < count; i = (i + 1) % size, j++)
            {
                Events[i] = null;
                del++;
            }

            count -= del;

            if (doneCount > count)
                doneCount = count;

        }

    }

    public void Undo()
    {
        if (doneCount > 0)
        {

            if (currentPos != 0)
            {
                currentPos--;
            }
            else
            {
                currentPos = size - 1;
            }

            Events[currentPos].Revert();
            //Debug.Log("undo done!");

            doneCount--;
        }

    }

    public void Redo()
    {
        if(doneCount < count)
        {

            Events[currentPos].Execute();

            currentPos = (currentPos + 1) % size;

            doneCount++;

        }
    }

    public void Clear()
    {
        doneCount = count = currentPos = 0;
        for(int i = 0; i < size; i++)
        {
            Events[i] = null;
        }
    }

    /// <summary>
    /// Deletes all elements and sets new event stack size
    /// </summary>
    /// <param name="newSize">New size of event stack, must be greater or equal to 0</param>
    public void Resize(int newSize)
    {
        if(newSize < size)
        {
            Clear();
            size = newSize;
        } else if(newSize > size)
        {
            doneCount = count = currentPos = 0;
            size = newSize;
            Events = new IGameEvent[size];
        }
    }

    //used for debugging
    public override string ToString()
    {
        return  "size: " + size + " currentPos: " + currentPos +
                " count: " + count + " doneCount: " + doneCount + " Events (" + string.Join(", ", (IEnumerable)Events) + ")";
    }

}
