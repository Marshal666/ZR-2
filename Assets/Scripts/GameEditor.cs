using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameEditor : MonoBehaviour, IWorldRenderer
{

    public enum GameEditorState
    {
        Idle,
        ConfiguringCellGroups,
        EditingCell
    }

    public StateMachine<GameEditorState> EditorState = new StateMachine<GameEditorState>(GameEditorState.Idle);

    /// <summary>
    /// Hosts all GameObjects that editor works with
    /// </summary>
    GameObject Host;

    /// <summary>
    /// Data of level that's currently being edited
    /// </summary>
    public WorldData LevelData;

    /// <summary>
    /// Main and (should be) only game editor
    /// </summary>
    public static GameEditor main;

    public float buildingDistance = 2.5f;

    public float BuildingDistance { get { return buildingDistance; } set { buildingDistance = value; } }

    public InputField levelNameField;

    public InputField[] levelDimensionFields;

    public InputField levelNameEditorField;

    public Text levelDimensionsText;

    public Button buttonTemplate;

    public RectTransform cellGroupButtonsHolder;

    public LayerMask CellEditorLayer;
    public int CellEditorLayerIndex = 9;

    public int currentCellGroup = -1;

    public GameObject arrowPointer;

    public Rect levelViewScreenSize;

    public List<List<int>> CellGroupsLists;

    public Color buttonSelectedColor = Color.grey, buttonUnselectedColor = Color.white;

    ObjectSpawner ArrowSpawner;

    List<List<T>> JaggedArrayToLists<T>(T[][] arr)
    {
        if (arr == null)
            return null;
        List<List<T>> ret = new List<List<T>>(arr.Length);
        for(int i = 0; i < arr.Length; i++)
        {
            ret.Add(new List<T>(arr[i].Length));
            for(int j = 0; j < arr[i].Length; j++)
            {
                ret[i].Add(arr[i][j]);
            }
        }
        return ret;
    }

    T[][] ListsToJaggedArray<T>(List<List<T>> lists)
    {
        if (lists == null)
            return null;
        T[][] ret = new T[lists.Count][];
        for(int i = 0; i < ret.Length; i++)
        {
            ret[i] = new T[lists[i].Count];
            for(int j = 0; j < ret[i].Length; j++)
            {
                ret[i][j] = lists[i][j];
            }
        }
        return ret;
    }

    /// <summary>
    /// Cell that is under a raycast
    /// </summary>
    EditorCell currentCell = null;

    void CheckRaycast()
    {
        const float raycastDistance = 1000f;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        if (Physics.Raycast(ray, out hit, raycastDistance, CellEditorLayer))
        {
            //search for parent cell
            //limit levels of going up to 4
            int cutoff = 4;
            Transform t = hit.transform;
            while (cutoff > 0 && currentCell == null)
            {
                if (!t)
                    break;
                EditorCell c = t.GetComponent<EditorCell>();
                if (c)
                    currentCell = c;
                t = t.parent;
                cutoff--;
            }

        }
        else
        {

            EditorState.State = GameEditorState.Idle;

        }

    }

    private void Awake()
    {
        main = this;

        EditorState.Methods[GameEditorState.Idle] = Idle;
        EditorState.Methods[GameEditorState.ConfiguringCellGroups] = ConfiguringCellGroups;
        EditorState.Methods[GameEditorState.EditingCell] = EditingCell;

        EditorState.Switches[GameEditorState.Idle][GameEditorState.ConfiguringCellGroups] = Idle2ConfiguringCellGroups;
        EditorState.Switches[GameEditorState.ConfiguringCellGroups][GameEditorState.Idle] = ConfiguringCellGroups2Idle;
        //EditorState.Switches[GameEditorState.ConfiguringCellGroups][GameEditorState.ConfiguringCellGroups] = ConfiguringCellGroups2Itself;
    }

    /// <summary>
    /// Loads an existing level data form file
    /// </summary>
    /// <param name="file">File which contains level data</param>
    /// <returns>True if level loading succeeded, false otherwize</returns>
    public bool LoadLevel(string file)
    {
        bool r = true;

        WorldData d = new WorldData();

        r &= d.Load(file);

        if (!r)
            return r;

        LevelData = d;

        Scene.main.GameState.SwitchState(Scene.GameStates.Editor);

        return true;
    }

    /// <summary>
    /// For button onClick events
    /// </summary>
    public void StartNewEditor()
    {

        if(string.IsNullOrEmpty(levelNameField.text) || string.IsNullOrWhiteSpace(levelNameField.text))
        {
            Scene.main.ShowMessageBox("New level must have a name!");
            return;
        }

        int[] dims = new int[levelDimensionFields.Length]; 

        for(int i = 0; i < levelDimensionFields.Length; i++)
        {
            if (string.IsNullOrEmpty(levelDimensionFields[i].text) || string.IsNullOrWhiteSpace(levelDimensionFields[i].text))
            {
                Scene.main.ShowMessageBox("Dimension" + i + " isn't specified!");
                return;
            }

            int rp;
            if(int.TryParse(levelDimensionFields[i].text, out rp))
            {
                dims[i] = rp;
            } else
            {
                Scene.main.ShowMessageBox("Dimension" + i + " is not a whole number!");
                return;
            }
        }

        CreateLevel(levelNameField.text, dims);

    }

    /// <summary>
    /// Creates an empty level data
    /// </summary>
    /// <param name="name">Name of the level</param>
    /// <param name="dimensions">Level dimensions</param>
    public void CreateLevel(string name, int[] dimensions)
    {

        WorldData data = new WorldData();

        data.LevelName = name;
        data.GameType = WorldData.GameTypes.SumToZero;

        MArray<CellData> cells = new MArray<CellData>(dimensions);
        for (int i = 0; i < cells.OneDimensional.Length; i++)
        {
            cells.OneDimensional[i].Type = CellData.CellType.Default;
            cells.OneDimensional[i].Number1 = 1;
        }

        data.CellDatas = cells;

        LevelData = data;

        //Calls Init()
        Scene.main.GameState.SwitchState(Scene.GameStates.Editor);

    }

    //onClick like events
    #region UI_EVENTS

    public void ChangeLevelName()
    {

        Scene.EventSystem.AddEvent(new GameEditorRename(this, levelNameEditorField.text));

    }

    public void AddCellGroup()
    {
        Scene.EventSystem.AddEvent(new GameEditorAddCellGroup(this));
    }

    public void RemoveCellGroup()
    {
        if (currentCellGroup >= 0)
            Scene.EventSystem.AddEvent(new GameEditorRemoveCellGroup(this, currentCellGroup));
    }

    #endregion

    //methods for Editor state
    #region STATE_METHODS

    void Idle()
    {

    }

    void ConfiguringCellGroups()
    {
        currentCell = null;
        //print(PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize));
        if((InputMapper.main.CellSelect || InputMapper.main.CellDeselect) && PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize))
        {
            CheckRaycast();
            if(currentCell)
            {
                if (InputMapper.main.CellSelect)
                {
                    Scene.EventSystem.AddEvent(new GameEditorAddCellToGroup(this, currentCellGroup, currentCell.index));
                } else
                {
                    Scene.EventSystem.AddEvent(new GameEditorRemoveCellFromGroup(this, currentCellGroup, currentCell.index));
                }
            }
        }
    }

    void EditingCell()
    {

    }

    #endregion

    #region STATE_SWITCHES

    void Idle2ConfiguringCellGroups()
    {
        if(currentCellGroup >= 0)
        {
            RemoveArrows();
            DrawArrows(currentCellGroup);
        }
    }

    void ConfiguringCellGroups2Idle()
    {
        currentCellGroup = -1;
        RemoveArrows();
    }

    /*void ConfiguringCellGroups2Itself()
    {

    }*/

    #endregion

    public void DrawArrows(int cellGroup)
    {
        for (int c = 0; c < CellGroupsLists[cellGroup].Count; c++)
        {
            World.main.Cells.OneDimensional[CellGroupsLists[cellGroup][c]].DrawAbove(ArrowSpawner.GetObject());
        }
    }

    public void RemoveArrows()
    {
        ArrowSpawner.ReturnAll();
    }

    public List<(Button button, int index)> CellGroupButtons;

    void NameCellGroupButton(Button b, int i)
    {

        Text t = b.GetComponentInChildren<Text>();
        if (t)
        {
            t.text = "Cell Group " + i;
        }

    }

    void AddCellGroupButton(int i)
    {
        Button button = Instantiate(buttonTemplate);
        CellGroupButtons.Add((button, i));
        button.Select();

        NameCellGroupButton(button, i);

        int cellGroup = i;
        button.onClick.AddListener(
            delegate
            {

                RemoveArrows();

                if (EditorState.State != GameEditorState.ConfiguringCellGroups)
                {
                    EditorState.SwitchState(GameEditorState.ConfiguringCellGroups);
                    currentCellGroup = cellGroup;
                    DrawArrows(cellGroup);
                }
                else
                {
                    if (currentCellGroup != cellGroup)
                    {
                        ArrowSpawner.ReturnAll();
                        currentCellGroup = cellGroup;
                        DrawArrows(cellGroup);
                    }
                    else
                    {
                        EditorState.SwitchState(GameEditorState.Idle);
                        Scene.UIEventSystem.SetSelectedGameObject(null);
                        currentCellGroup = -1;
                        RemoveArrows();
                    }
                }
            }
        );

        button.transform.SetParent(cellGroupButtonsHolder, false);
    }

    void SwapButtons(List<(Button, int)> l, int i, int j)
    {
        if(i > l.Count || j > l.Count)
        {
            throw new IndexOutOfRangeException();
        }

        if (i == j)
            return;

        l[i] = (l[j].Item1, l[i].Item2);
        l[j] = (l[i].Item1, l[j].Item2);

        (Button, int) t = l[i];
        l[i] = t;
        l[j] = l[i];

        NameCellGroupButton(l[i].Item1, i);
        NameCellGroupButton(l[j].Item1, j);

    }

    /// <summary>
    /// Inits cell group buttons UI and list
    /// </summary>
    void InitCellGroupButtons()
    {
        CellGroupButtons = new List<(Button group, int index)>();
        if (LevelData.CellGroups != null)
        {
            CellGroupButtons.Capacity = LevelData.CellGroups.Length;

            for (int i = 0; i < LevelData.CellGroups.Length; i++)
            {
                AddCellGroupButton(i);
            }
        }

    }

    /// <summary>
    /// Requires that CellGroupButtons was already inited, supports only one cell group change for call
    /// </summary>
    public void ReInitCellGroupButtons()
    {
        int buttonCount = CellGroupButtons.Count;
        int cellGroupCount = CellGroupsLists.Count;

        //adding is just adding a new button on the bottom
        if(cellGroupCount > buttonCount)
        {
            int delta = cellGroupCount - buttonCount;
            while (delta > 0)
            {
                AddCellGroupButton(cellGroupCount - 1);
                delta--;
            }
        } else if(cellGroupCount < buttonCount) //deleting
        {
            int deleted = -1;
            for(int i = 0; i < buttonCount; i++)
            {
                if(i < cellGroupCount)
                {
                    if(CellGroupButtons[i].index == i)
                    {
                        continue;
                    } else
                    {
                        if (deleted == -1)
                        {
                            deleted = i - 1;

                            CellGroupButtons[CellGroupButtons.Count - 1] = (CellGroupButtons[CellGroupButtons.Count - 1].button, deleted);
                            NameCellGroupButton(CellGroupButtons[CellGroupButtons.Count - 1].button, deleted);
                            break;
                        }
                        break;
                    }

                } else
                {
                    deleted = CellGroupButtons.Count - 1;
                }
            }
            Destroy(CellGroupButtons[deleted].button.gameObject);
            CellGroupButtons.RemoveAt(CellGroupButtons.Count - 1);
        }
    }

    /// <summary>
    /// Inits editor for work
    /// </summary>
    public void Init()
    {

        Host = new GameObject("EditorHost");
        Host.transform.SetParent(Scene.RootTransform);

        levelNameEditorField.text = LevelData.LevelName;

        levelDimensionsText.text = "Dimensions: " + string.Join(" x ", LevelData.CellDatas.Dimensions);

        World.main.AssembleLevel(LevelData, Host);

        for(int i = 0; i < World.main.Cells.OneDimensional.Length; i++)
        {
            EditorCell c = World.main.Cells.OneDimensional[i].gameObject.AddComponent<EditorCell>();
            c.index = i;
        }

        currentCellGroup = -1;

        ArrowSpawner = new ObjectSpawner(arrowPointer, Host.transform, World.main.Cells.OneDimensional.Length, ObjectSpawner.SpawnerType.Expandable);

        CellGroupsLists = JaggedArrayToLists(LevelData.CellGroups);

        if (CellGroupsLists == null)
            CellGroupsLists = new List<List<int>>();

        InitCellGroupButtons();
        
    }

    /// <summary>
    /// Clears everything Editor has done
    /// </summary>
    public void Clear()
    {
        Scene.ClearChildren(Host.transform);
        Scene.ClearChildren(cellGroupButtonsHolder);
        Destroy(Host);
        LevelData = null;
        ArrowSpawner = null;
        CellGroupsLists = null;
        CellGroupButtons = null;
    }

    private void Update()
    {
        EditorState.Execute();
    }

    public string GroupStatesString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("CellGroupLists: {");
        for(int i = 0; i < CellGroupsLists.Count; i++)
        {
            sb.Append(" ");
            sb.Append(i);
            sb.Append(" { ");
            for(int j = 0; j < CellGroupsLists[i].Count; j++)
            {
                sb.Append(CellGroupsLists[i][j]);
                if (j + 1 < CellGroupsLists[i].Count)
                    sb.Append(", ");
            }
            sb.Append(" }");
            if (i + 1 < CellGroupsLists.Count)
                sb.Append(", ");
        }
        sb.Append(" } ");
        return sb.ToString();
    }

    public void ReDrawCellGroupControls(bool reinitButtons, int buttonSelectIndex = -1, bool drawArrows = false)
    {
        if (reinitButtons)
        {
            ReInitCellGroupButtons();
        }

        if(buttonSelectIndex >= 0 && buttonSelectIndex < CellGroupButtons.Count)
        {
            CellGroupButtons[buttonSelectIndex].button.Select();
        }

        RemoveArrows();
        if(drawArrows)
        {
            DrawArrows(buttonSelectIndex);
        }
    }

    public void RenderPositionChanges()
    {
        //TODO
    }
}
