using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reposition()
    {
        World w = World.main;
        if (w.Cells != null && w.currentPosition.Length > 0)
        {
            Vector3 newPlayerPos = new Vector3(w.currentPosition[0] * w.buildingDistance, w.Cells[w.currentPosition].Data.Number1);
            if (w.currentPosition.Length > 1)
            {
                newPlayerPos.z = w.currentPosition[1] * w.buildingDistance;
            }
            transform.position = newPlayerPos;
        }
    }
}
