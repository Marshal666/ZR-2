using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SumLabel : MonoBehaviour
{

    Text label;

    void Awake()
    {
        label = GetComponent<Text>();
        World.main.OnSumChange += UpdateSum;
        World.main.Sum = World.main.Sum;
    }

    void UpdateSum(int s)
    {
        label.text = "Sum: " + s;
    }
}
