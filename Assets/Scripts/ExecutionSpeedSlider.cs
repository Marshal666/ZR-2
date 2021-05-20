using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExecutionSpeedSlider : MonoBehaviour
{

    static ExecutionSpeedSlider main = null;

    public static ExecutionSpeedSlider Instance => main;

    private ExecutionSpeedSlider()
    {
        if (main != null)
            throw new System.Exception("Cannor have two instances of ExecutionSpeedSlider");

        main = this;
    }

    Slider s;

    private void Awake()
    {
        s = GetComponent<Slider>();
        s.onValueChanged.AddListener(Scene.Player.SetExecutionSpeed);
    }

}
