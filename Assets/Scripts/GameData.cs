using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{

    public static GameData main;

    public List<GameObject> buildingBlocks = new List<GameObject>();

    public static List<GameObject> BuildingBlocks { get { return main.buildingBlocks; } }

    public Material roadMaterial;

    public static Material RoadMaterial {  get { return main.roadMaterial; } }

    private void Awake()
    {
        main = this;
    }

}
