using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventExecutionResult
{
    Failed,
    Success,
}

public interface IGameEvent
{

    void Execute();

    void Revert();

    GameEventExecutionResult Result { get; }

}
