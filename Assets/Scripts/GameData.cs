using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for storing game constants/links to assets
/// There should be only one GameData class in the game
/// </summary>
public class GameData : MonoBehaviour
{

    /// <summary>
    /// Main and only GameData object
    /// </summary>
    public static GameData main;

    /// <summary>
    /// Indexes of blocks for cell drawing
    /// </summary>
    public List<GameObject> buildingBlocks = new List<GameObject>();

    /// <summary>
    /// Indexes of blocks for cell drawing
    /// </summary>
    public static List<GameObject> BuildingBlocks { get { return main.buildingBlocks; } }

    /// <summary>
    /// Material used for roads
    /// </summary>
    public Material roadMaterial;

    /// <summary>
    /// Material used for roads
    /// </summary>
    public static Material RoadMaterial {  get { return main.roadMaterial; } }

    /// <summary>
    /// Script init
    /// </summary>
    private void Awake()
    {
        main = this;
    }

}
