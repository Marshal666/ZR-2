using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GameEditor : MonoBehaviour, IWorldRenderer
{

    public enum GameEditorState
    {
        Idle,
        ConfiguringCellGroups,
        EditingCell,
        EditingPlayer, //position
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

    public LayerMask PlayerLayer;

    public LayerMask CellEditorLayer;
    public int CellEditorLayerIndex = 9;

    public int currentCellGroup = -1;

    public EditorObject currentEditorObject = null;

    public PlayerEditor playerEditorObject;

    /// <summary>
    /// Cell that is under a raycast
    /// </summary>
    public EditorCell currentCell { get { return currentEditorObject as EditorCell; } }

    public GameObject arrowPointer;
    public GameObject blueArrowPointer;

    public Rect levelViewScreenSize;

    public List<List<int>> CellGroupsLists;

    public Color buttonSelectedColor = Color.grey, buttonUnselectedColor = Color.white;

    public Toggle[] CellTypeToggles;

    public CellTypeUIElement[] CellPropertyObjects;

    public GameObject CellPropertiesInspectorHodler;

    public Text CellNameText;

    public UIChildPlacer CellPropertiesHolder;

    public InputField[] CellNumbersInputFields;

    [Serializable]
    public class SaveLevelWindow
    {
        public GameObject Window;
        public InputField LevelFilenameField;

        public void Show()
        {
            Window.SetActive(true);
        }

        public void Hide()
        {
            Window.SetActive(false);
        }

    }

    public SaveLevelWindow SaveLevelWindowObject;

    
    

    ObjectSpawner ArrowSpawner;

    ObjectSpawner BlueArrowSpawner;

    [Serializable]
    public class CellTypeUIElement
    {
        public int ToggleIndex;
        public GameObject UIElement;
    }

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
    /// Checks if mouse it pointing at an editor cell
    /// </summary>
    EditorObject CheckRaycast(int layerMask)
    {
        const float raycastDistance = 1000f;
        EditorObject o = null;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        if (Physics.Raycast(ray, out hit, raycastDistance, layerMask))
        {
            //search for parent cell
            //limit levels of going up to 4
            int cutoff = 4;
            Transform t = hit.transform;
            currentEditorObject = null;
            while (cutoff > 0 && o == null)
            {
                if (!t)
                    break;
                o = t.GetComponent<EditorObject>();
                t = t.parent;
                cutoff--;
            }

        }
        else
        {

            EditorState.State = GameEditorState.Idle;

        }

        currentEditorObject = o;
        return o;
    }

    private void Awake()
    {
        main = this;

        EditorState.Methods[GameEditorState.Idle] = Idle;
        EditorState.Methods[GameEditorState.ConfiguringCellGroups] = ConfiguringCellGroups;
        EditorState.Methods[GameEditorState.EditingCell] = EditingCell;
        EditorState.Methods[GameEditorState.EditingPlayer] = EditingPlayer;

        EditorState.StateEnterMethods[GameEditorState.EditingPlayer] = EditingPlayerEnter;

        EditorState.StateExitMethods[GameEditorState.EditingPlayer] = EditingPlayerExit;
        EditorState.StateExitMethods[GameEditorState.EditingCell] = EditingCellExit;

        EditorState.Switches[GameEditorState.Idle][GameEditorState.ConfiguringCellGroups] = Idle2ConfiguringCellGroups;
        EditorState.Switches[GameEditorState.ConfiguringCellGroups][GameEditorState.Idle] = ConfiguringCellGroups2Idle;
        //EditorState.Switches[GameEditorState.ConfiguringCellGroups][GameEditorState.ConfiguringCellGroups] = ConfiguringCellGroups2Itself;

        EditorState.Switches[GameEditorState.Idle][GameEditorState.EditingCell] = Idle2EditingCell;
        EditorState.Switches[GameEditorState.EditingCell][GameEditorState.Idle] = EditingCell2Idle;

        EditorState.Switches[GameEditorState.ConfiguringCellGroups][GameEditorState.EditingCell] = ConfiguringCellGroups2EditingCell;
        EditorState.Switches[GameEditorState.EditingCell][GameEditorState.ConfiguringCellGroups] = EditingCell2ConfiguringCellGroups;

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

    public bool LoadLevel(WorldData data)
    {
        if (data == null)
            return false;

        LevelData = data;

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
        data.PlayerStartPosition = new int[dimensions.Length];

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

    public void ApplyCellChanges()
    {
        //print("h");

        if (currentCell)
        {
            CellData data = new CellData();

            for (int i = 0; i < CellTypeToggles.Length; i++)
            {
                data.Type |= (CellData.CellType)((CellTypeToggles[i].isOn ? 1 : 0) * (int)Mathf.Pow(2, i));
            }

            if (CellNumbersInputFields.Length >= 4)
            {
                data.Number1 = int.Parse(CellNumbersInputFields[0].text);
                data.Number2 = int.Parse(CellNumbersInputFields[1].text);
                data.Number3 = -Mathf.Abs(int.Parse(CellNumbersInputFields[2].text));
                data.AffectedCellGroup = int.Parse(CellNumbersInputFields[3].text);
            }

            //bools are always valid, only numbers need checking

            if (data.Number1 > Limits.MaxCellNumber1)
                data.Number1 = Limits.MaxCellNumber1;

            if (data.Number2 < 0)
                data.Number2 = 0;

            if (data.Number2 >= World.main.Cells.OneDimensional.Length)
                data.Number2 = World.main.Cells.OneDimensional.Length - 1;

            if (data.Number3 > Limits.MaxCellNumber3)
                data.Number3 = Limits.MaxCellNumber3;

            //cell groups use no checking

            //add event
            if (data != LevelData.CellDatas.OneDimensional[currentCell.index])
                Scene.EventSystem.AddEvent(new GameEditorEditCellData(this, currentCell, data));

            /*
            //revert back to old data
            CellData oldData = World.main.Cells.OneDimensional[currentCell.index].Data;

            for (int i = 0; i < Mathf.Min(CellTypeToggles.Length, sizeof(int) * 8); i++)
            {
                CellTypeToggles[i].SetIsOnWithoutNotify(((int)oldData.Type & (i << 1)) == 1);
            }

            CellNumbersInputFields[0].text = oldData.Number1.ToString();
            CellNumbersInputFields[1].text = oldData.Number2.ToString();
            CellNumbersInputFields[2].text = oldData.Number3.ToString();
            CellNumbersInputFields[3].text = oldData.AffectedCellGroup.ToString();
            */

        }
    }

    public void SaveLevel()
    {
        LevelData.CellGroups = ListsToJaggedArray(CellGroupsLists);

        SaveLevelWindowObject.Show();
    }

    public void CloseSaveLevelWindow()
    {
        SaveLevelWindowObject.Hide();
    }

    public void SaveLevelToFile()
    {
        string name = SaveLevelWindowObject.LevelFilenameField.text;
        FileInfo f = null;
        try
        {
            f = new FileInfo(name);
            if (string.IsNullOrEmpty(f.Extension) || string.IsNullOrWhiteSpace(f.Extension))
                name += GameData.DefaultSaveFileExtension;
            //print(f.Extension);
        } catch (Exception) { f = null; }
        if(f is object)
        {
            if(File.Exists(name))
            {
                //TODO: show yes/no message box for overwrite
            } else
            {
                SaveLevelDataToFile(name, LevelData);
                SaveLevelWindowObject.Hide();
                
            }
        } else
        {
            Scene.main.ShowMessageBox("Invalid file name");
        }
    }

    #endregion

    //methods for Editor state
    #region STATE_METHODS

    void Idle()
    {
        currentEditorObject = null;
        if (InputMapper.main.CellSelect && PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize))
        {
            //TODO: add player click support & switch to edit player state
            EditorObject o = CheckRaycast(CellEditorLayer | PlayerLayer);
            if (currentCell)
            {
                EditorState.SwitchState(GameEditorState.EditingCell);
            }
            if(o is PlayerEditor)
            {
                EditorState.SwitchState(GameEditorState.EditingPlayer);
            }
        }
    }

    void ConfiguringCellGroups()
    {
        currentEditorObject = null;
        //print(PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize));
        if((InputMapper.main.CellSelect || InputMapper.main.CellDeselect) && PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize))
        {
            CheckRaycast(CellEditorLayer);
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
        if ((InputMapper.main.CellSelect || InputMapper.main.CellDeselect) && PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize))
        {
            EditorCell current = currentEditorObject as EditorCell;
            CheckRaycast(CellEditorLayer);

            if(currentEditorObject as EditorCell == current)
            {
                int delta = 0;
                if (InputMapper.main.CellIncrease)
                {
                    delta++;
                }
                if (InputMapper.main.CellDecrease)
                {
                    delta--;
                }
                if (CellNumbersInputFields != null && CellNumbersInputFields.Length > 0 && delta != 0)
                {
                    int cn1 = int.Parse(CellNumbersInputFields[0].text);
                    cn1 += delta;
                    CellNumbersInputFields[0].text = cn1.ToString();
                    ApplyCellChanges();
                }
                if (InputMapper.main.CellIncrease)
                {
                    
                }
                
            }

            ReDrawCellEditingObjects();
            RedrawCellInspectorUI();
        }
    }

    void EditingPlayer()
    {

        if (InputMapper.main.CellSelect && PlayerCamera.main.MouseOnScreenPart(levelViewScreenSize))
        {

            CheckRaycast(CellEditorLayer | PlayerLayer);

            if(currentCell)
            {
                if(Scene.Player.CurrentPosition != null)
                {
                    World.main.Cells.getCoordsNonAlloc(currentCell.index, ref Scene.Player.CurrentPosition);
                } else
                {
                    Scene.Player.CurrentPosition = World.main.Cells.getCoords(currentCell.index);
                }
                LevelData.PlayerStartPosition = Scene.Player.CurrentPosition;
                Scene.Player.Reposition();
            }
        }

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

    void Idle2EditingCell()
    {
        if(currentCell)
        {
            ReDrawCellEditingObjects();
            RedrawCellInspectorUI();
        }
    }

    void EditingCell2Idle()
    {
        currentEditorObject = null;
        ReDrawCellEditingObjects();
    }

    void ConfiguringCellGroups2EditingCell()
    {

    }

    void EditingCell2ConfiguringCellGroups()
    {
        currentEditorObject = null;
        ReDrawCellEditingObjects();
    }

    void EditingPlayerExit()
    {
        if(playerEditorObject)
        {
            playerEditorObject.DisableOutline();
        }
    }

    void EditingPlayerEnter()
    {
        if (playerEditorObject)
        {
            playerEditorObject.EnableOutline();
        }
    }

    void EditingCellEnter()
    {

    }

    void EditingCellExit()
    {
        currentEditorObject = null;
        ReDrawCellEditingObjects();
        RedrawCellInspectorUI();
    }

    #endregion

    void SaveLevelDataToFile(string filename, WorldData data)
    {
        try
        {

            //print(Directory.GetCurrentDirectory() + " " + GameData.DefaultSaveLevelDirectory + " " + filename);

            filename = Path.Combine(Directory.GetCurrentDirectory(), GameData.DefaultSaveLevelDirectory, filename);

            //print(filename);

            StreamWriter sw = new StreamWriter(filename);

            sw.Write(data.ToString());

            sw.Close();
            Scene.main.ShowMessageBox("Level saved");

        }
        catch (OutOfMemoryException)
        {
            Scene.main.ShowMessageBox("Out of memory", "Cannot save level");
        }
        catch (UnauthorizedAccessException)
        {
            Scene.main.ShowMessageBox("Cannot write to file", "Cannot save level");
        }
        catch (DirectoryNotFoundException)
        {
            Scene.main.ShowMessageBox("Given directory does not exist", "Cannot save level");
        }
        catch (PathTooLongException)
        {
            Scene.main.ShowMessageBox("Given path to file is too long", "Cannot save level");
        }
        catch (SecurityException)
        {
            Scene.main.ShowMessageBox("Cannot write to file", "Cannot save level");
        }
        catch (Exception e)
        {
            Scene.main.ShowMessageBox("Error: " + e.Message);
        }
    }

    public void ReDrawCellEditingObjects()
    {
        RemoveArrows();

        if(currentCell)
        {

            Cell cell = World.main.Cells.OneDimensional[currentCell.index];

            cell.Redraw();
            cell.DrawAbove(ArrowSpawner.GetObject());

            if((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn)
            {
                int targetIndex = cell.Data.Number2;
                if(targetIndex >= 0 && targetIndex < World.main.Cells.OneDimensional.Length)
                {
                    Cell targetCell = World.main.Cells.OneDimensional[targetIndex];
                    targetCell.DrawAbove(BlueArrowSpawner.GetObject());
                }
            }
        }

        Scene.Player.Reposition();
    }

    public void RedrawCellInspectorUI()
    {

        if (currentCell)
        {

            CellPropertiesInspectorHodler.SetActive(true);

            CellNameText.text = "Cell " + currentCell.index;

            CellData data = LevelData.CellDatas.OneDimensional[currentCell.index];

            for(int i = 0; i < CellTypeToggles.Length; i++)
            {
                CellTypeToggles[i].SetIsOnWithoutNotify(((int)data.Type & ((int)Mathf.Pow(2, i))) != 0);
            }

            for (int j = 0; j < CellPropertyObjects.Length; j++)
            {
                CellPropertyObjects[j].UIElement.SetActive(false);
            }


            for(int i = 0; i < CellPropertyObjects.Length; i++)
            {
                CellPropertyObjects[i].UIElement.SetActive(false);
            }

            for (int i = 0; i < CellTypeToggles.Length; i++)
            {
                if (CellTypeToggles[i].isOn)
                {
                    for (int j = 0; j < CellPropertyObjects.Length; j++)
                    {
                        if (CellPropertyObjects[j].ToggleIndex == i)
                            CellPropertyObjects[j].UIElement.SetActive(true);
                    }
                }
            }

            int[] dint = { data.Number1, data.Number2, data.Number3, data.AffectedCellGroup };
            int mm = Mathf.Min(dint.Length, CellNumbersInputFields.Length);

            for(int i = 0; i < mm; i++)
            {
                CellNumbersInputFields[i].SetTextWithoutNotify(dint[i].ToString());
            }

            CellPropertiesHolder.Redraw();

        } else
        {

            CellPropertiesInspectorHodler.SetActive(false);

        }
    }

    public void DrawArrows(int cellGroup)
    {
        for (int c = 0; c < CellGroupsLists[cellGroup].Count; c++)
        {
            World.main.Cells.OneDimensional[CellGroupsLists[cellGroup][c]].DrawAbove(ArrowSpawner.GetObject());
        }
    }

    public void DrawBlueArrows(int cellTargetIndex)
    {
        World.main.Cells.OneDimensional[cellTargetIndex].DrawAbove(BlueArrowSpawner.GetObject());
    }

    public void RemoveArrows()
    {
        ArrowSpawner.ReturnAll();
        BlueArrowSpawner.ReturnAll();
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

    public void RedrawCells(WorldData data)
    {
        LevelData = data;
        Scene.ClearChildren(Host.transform);
        World.main.AssembleLevel(LevelData, Host);
        ArrowSpawner = new ObjectSpawner(arrowPointer, Host.transform, World.main.Cells.OneDimensional.Length, ObjectSpawner.SpawnerType.Expandable);
        BlueArrowSpawner = new ObjectSpawner(blueArrowPointer, Host.transform, 1);

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

        BlueArrowSpawner = new ObjectSpawner(blueArrowPointer, Host.transform, 1);

        CellGroupsLists = JaggedArrayToLists(LevelData.CellGroups);

        if (CellGroupsLists == null)
            CellGroupsLists = new List<List<int>>();

        InitCellGroupButtons();

        CellPropertiesInspectorHodler.SetActive(false);

        SaveLevelWindowObject.Hide();

        Scene.Player.WorldRenderer = this;
        Scene.Player.WorldIn = World.main.Cells;

        Scene.Player.CurrentPosition = new int[LevelData.PlayerStartPosition.Length];
        Array.Copy(LevelData.PlayerStartPosition, Scene.Player.CurrentPosition, LevelData.PlayerStartPosition.Length);
        Scene.Player.Reposition();

        EditorState.State = GameEditorState.Idle;
        
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
        BlueArrowSpawner = null;
        CellGroupsLists = null;
        CellGroupButtons = null;
        currentEditorObject = null;
        CellPropertiesInspectorHodler.SetActive(false);
        testerE = null;
        StopAllCoroutines();
        if (testThread != null && testThread.IsAlive)
            testThread.Abort();
    }

    private void Update()
    {
        //print(EditorState.State + " " + currentCell);
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

    IEnumerator testerE = null;

    public void TestLevel()
    {
        if (testerE == null)
        {
            WorldTester tester = new WorldTester(LevelData);
            testerE = tester.BuildTreeSteps();
        } 
        if(!testerE.MoveNext())
        {
            testerE = null;
        }
        
    }

    Thread testThread = null;

    public void TestLevelFast()
    {
        if(testerE != null)
        {
            print("Cannot do both tests at the same time");
            return;
        }


        if (testThread != null && !testThread.IsAlive)
            testThread = null;
        if (testThread == null)
        {
            testThread = new Thread(new ThreadStart(StartThread));
            testThread.Start();

            void StartThread()
            {
                print("Thread started");
                WorldTester tester = new WorldTester(LevelData);
                tester.BuildTree();
            }
        }
    }

    public void RenderPositionChanges()
    {
        //TODO
    }
}
