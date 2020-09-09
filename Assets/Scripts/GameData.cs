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
    public List<CellLook> buildingBlocks = new List<CellLook>();
    public static List<CellLook> BuildingBlocks { get { return main.buildingBlocks; } }

    /// <summary>
    /// Material used for roads
    /// </summary>
    public Material roadMaterial;
    public static Material RoadMaterial {  get { return main.roadMaterial; } }

    /// <summary>
    /// UI object that stores levels access buttons
    /// </summary>
    public RectTransform levelsContentHolder;
    public static RectTransform LevelsContentHolder { get { return main.levelsContentHolder; } }

    /// <summary>
    /// UI object that stores levels access buttons for editing
    /// </summary>
    public RectTransform levels2EditContentHolder;
    public static RectTransform Levels2EditContentHolder { get { return main.levels2EditContentHolder; } }

    /// <summary>
    /// Folder with level files
    /// </summary>
    public string localLevelsDirectory;
    public static string LocalLevelsDirectory { get { return main.localLevelsDirectory; } }

    /// <summary>
    /// LoadLevel button prefab
    /// </summary>
    public GameObject loadLevelButton;
    public static GameObject LoadLevelButton { get { return main.loadLevelButton; } }

    /// <summary>
    /// Time scale when game is paused (0)
    /// </summary>
    public float pausedTimeScale = 0f;
    public static float PausedTimeScale { get { return main.pausedTimeScale; } }

    /// <summary>
    /// Normal game speed
    /// </summary>
    public float unPausedTimeScale = 1f;
    public static float UnPausedTimeScale { get { return main.unPausedTimeScale; } }

    /// <summary>
    /// Default cell transparency - not transparent
    /// </summary>
    public float defaultCellAlphaColor = 1f;
    public static float DefaultCellAlphaColor { get { return main.defaultCellAlphaColor; } }

    /// <summary>
    /// Cell transparency used in views - half transparent
    /// </summary>
    public float semiTransparentCellColor = 0.5f;
    public static float SemiTransparentCellColor { get { return main.semiTransparentCellColor; } }

    /// <summary>
    /// 3D Arrow for pointing where teleport teleports to
    /// </summary>
    public GameObject arrowObject;
    public static GameObject ArrowObject { get { return main.arrowObject; } }

    /// <summary>
    /// Script init
    /// </summary>
    private void Awake()
    {
        main = this;
    }

}
