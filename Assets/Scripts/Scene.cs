using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages game scene
/// </summary>
public class Scene : MonoBehaviour
{

    public GameObject StartMenuObject = null;
    public GameObject LevelSelectMenuObject = null;
    public GameObject OptionsMenuObject = null;

    public GameObject WonLostObject = null;
    public GameObject WonObjects = null;
    public GameObject LostObjects = null;

    public GameObject MessageBox = null;
    public Text MessageBoxTitle = null;
    public Text MessageBoxMessage = null;


    public GameObject PauseMenuObject = null;


    public Dictionary<MenuStates, GameObject> MenuObjects;

    /// <summary>
    /// Game states
    /// </summary>
    public enum GameStates
    {
        Playing,
        Paused,
        Menu,
        Editor,
        WonLost,
        //more required?
    }

    public enum MenuStates
    {
        Start,
        LevelSelect,
        Options
    }

    /// <summary>
    /// State machine for controlling game states
    /// </summary>
    public StateMachine<GameStates> GameState;

    public StateMachine<MenuStates> MenuState;

    /// <summary>
    /// Transform of root object
    /// </summary>
    Transform rootTransform;

    /// <summary>
    /// Root object refrence
    /// </summary>
    public GameObject root;

    /// <summary>
    /// There should be only one scene in game
    /// </summary>
    public static Scene main;

    /// <summary>
    /// Root object refrence
    /// </summary>
    public static GameObject Root { get { return main.root; } }

    /// <summary>
    /// Transform of root object
    /// </summary>
    public static Transform RootTransform { get { return main.rootTransform; } }

    /// <summary>
    /// Player object refrence
    /// </summary>
    public GameObject player;


    /// <summary>
    /// Player component reference
    /// </summary>
    Player playerC;

    /// <summary>
    /// Player camera component reference
    /// </summary>
    public PlayerCamera playerCameraC;

    /// <summary>
    /// Player object refrence
    /// </summary>
    public static GameObject PlayerObject { get { return main.player; } }

    /// <summary>
    /// Player component reference
    /// </summary>
    public static Player Player { get { return main.playerC; } }

    /// <summary>
    /// Games event system object
    /// </summary>
    EventSystem Events;

    /// <summary>
    /// Games event system
    /// </summary>
    public static EventSystem EventSystem { get { return main.Events; } }

    /// <summary>
    /// Script init
    /// </summary>
    private void Awake()
    {

        main = this;

        //playing is temporary starting state
        GameState = new StateMachine<GameStates>(GameStates.Menu);

        //game state methods assignment
        GameState.Methods[GameStates.Playing] = Playing;
        GameState.Methods[GameStates.Paused] = Paused;
        GameState.Methods[GameStates.Menu] = Menu;
        GameState.Methods[GameStates.Editor] = Editor;

        GameState.Switches[GameStates.Playing][GameStates.Paused] = Pause;
        GameState.Switches[GameStates.Paused][GameStates.Playing] = Unpause;

        GameState.Switches[GameStates.Menu][GameStates.Playing] = Menu2Playing;
        GameState.Switches[GameStates.Paused][GameStates.Menu] = Paused2Menu;

        GameState.Switches[GameStates.Playing][GameStates.WonLost] = Playing2WonLost;
        GameState.Switches[GameStates.WonLost][GameStates.Menu] = WonLost2Menu;

        //init menu sm
        MenuState = new StateMachine<MenuStates>(MenuStates.Start);

        MenuState.Switches[MenuStates.Start][MenuStates.LevelSelect] = ToLevelSelect;
        MenuState.Switches[MenuStates.Start][MenuStates.Options] = ToOptions;

        MenuState.Switches[MenuStates.Options][MenuStates.Start] = ToStart;

        MenuState.Switches[MenuStates.LevelSelect][MenuStates.Start] = ToStart;

        //init MenuObjects Dictionary
        MenuObjects = new Dictionary<MenuStates, GameObject>() { 
            { MenuStates.Start, StartMenuObject },
            { MenuStates.LevelSelect, LevelSelectMenuObject },
            { MenuStates.Options, OptionsMenuObject }
        };

        //load Start UI (might not be neccesary)
        SwitchUIToState(MenuStates.Start);


        //find or create root if needed
        if (!root)
        {
            root = GameObject.Find("Root");
            if(!root)
            {
                root = new GameObject("Root");
            }
        }

        //assign root transform
        rootTransform = root.transform;

        //search for player if it hasn't been assigned
        if(!player)
        {
            player = GameObject.Find("Player");
            if(!player)
            {
                #if UNITY_EDITOR
                Debug.LogError("Player does not exist");
                #endif
            }
        }

        //find players component
        playerC = player.GetComponent<Player>();

        //create event system object
        Events = new EventSystem();

    }

    /// <summary>
    /// Clears all child objects of given object
    /// </summary>
    /// <param name="t">Object whose children will be deleted</param>
    public static void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Destroy(t.GetChild(i).gameObject);
        }
    }

    #region ONCLICK_EVENTS

    /// <summary>
    /// Calls state machine to change game state, used for button onClick events
    /// </summary>
    /// <param name="newState">Name of new game state</param>
    public void ChangeMenuState(string newState)
    {
        MenuStates state = (MenuStates)System.Enum.Parse(typeof(MenuStates), newState);
        //switching from same state to state is not possible
        if (state != MenuState.State)
        {
            MenuState.SwitchState(state);
        }
    }

    /// <summary>
    /// Changes game state to given one, used for onClick button events
    /// </summary>
    /// <param name="newState">New game state</param>
    public void ChangeGameState(string newState)
    {
        GameStates state = (GameStates)System.Enum.Parse(typeof(GameStates), newState);
        if(state != GameState.State)
        {
            GameState.SwitchState(state);
        }
    }

    /// <summary>
    /// Exits game, for onClick button event
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Disables message box
    /// </summary>
    public void DisableMessageBox()
    {
        MessageBox.SetActive(false);
    }

    #endregion

    #region UI_METHODS

    void DisableUI()
    {

        //disable menu
        foreach (MenuStates ms in MenuObjects.Keys)
        {
            GameObject o = MenuObjects[ms];
            if (o)
            {
                o.SetActive(false);
            }
        }

        //disable pause UI
        PauseMenuObject.SetActive(false);

        //disable won/lost objects
        WonLostObject.SetActive(false);
        WonObjects.SetActive(false);
        LostObjects.SetActive(false);

        //disable message box
        MessageBox.SetActive(false);

    }

    public void ShowMessageBox(string message, string title = "Game message")
    {
        MessageBox.SetActive(true);
        MessageBoxMessage.text = message;
        MessageBoxTitle.text = title;
    }

    void SwitchUIToState(MenuStates state)
    {
        DisableUI();
        GameObject sh = MenuObjects[state];
        if (sh)
        {
            sh.SetActive(true);
        }
    }

    void ToLevelSelect()
    {
        //print("level select");
        SwitchUIToState(MenuStates.LevelSelect);
        UnLoadLevelButtons();
        LoadLevelButtons();
    }

    void ToStart()
    {
        //print("start");
        SwitchUIToState(MenuStates.Start);
    }

    void ToOptions()
    {
        //print("options");
        SwitchUIToState(MenuStates.Options);
    }

    /// <summary>
    /// Loads LoadLevel Buttons
    /// </summary>
    public void LoadLevelButtons()
    {
        //load level filenames
        string[] levels = Directory.GetFiles(GameData.LocalLevelsDirectory);

        for (int i = 0; i < levels.Length; i++)
        {

            GameObject button = Instantiate(GameData.LoadLevelButton);

            button.name = "LoadLevel" + levels[i];
            Text txt = button.GetComponentInChildren<Text>();
            if (txt)
                txt.text = levels[i];

            button.transform.SetParent(GameData.LevelsContentHolder, false);

            string lvl = levels[i];
            button.GetComponent<Button>()?.onClick.AddListener(
                delegate {
                    if (World.main.LoadLevel(lvl))
                    {
                        GameState.SwitchState(GameStates.Playing);
                    } else
                    {
                        //level loading failed
                        ShowMessageBox("Level file is corrupt!", "Level loading failed");
                    }
                });

        }
    }

    public void UnLoadLevelButtons()
    {
        ClearChildren(GameData.LevelsContentHolder);
    }

    #endregion

    #region STATE_METHODS

    //methods used for game states

    void Playing()
    {

        //for optimization sake, don't do this every frame? But when player makes a move!
        if (CheckPlayerWin())
        {
            PlayerWin();
            return;
        }

        if (CheckPlayerLose())
        {
            PlayerLose();
            return;
        }

        if (InputMapper.main.Undo)
        {
            Events.Undo();
        }

        if (InputMapper.main.Redo)
        {
            Events.Redo();
        }

        if(InputMapper.main.Pause)
        {
            GameState.SwitchState(GameStates.Paused);
        }

    }

    void Paused()
    {
        if(InputMapper.main.Pause)
        {
            GameState.SwitchState(GameStates.Playing);
        }
    }

    void Menu()
    {

    }

    void Editor()
    {

    }

    #endregion

    #region SWITCH_STATE_METHODS

    //for switches between states

    void Menu2Playing()
    {
        DisableUI();
        playerC.PlayerState.SwitchState(Player.PlayerStates.Playing);
        playerCameraC.CameraState.SwitchState(PlayerCamera.PlayerCameraStates.FollowPlayer);
    }

    void Paused2Menu()
    {
        Time.timeScale = GameData.UnPausedTimeScale;
        playerC.ResetToDefault();
        World.main.ResetToDefault();
        playerCameraC.ResetToDefault();
        DisableUI();
        MenuState.SwitchState(MenuStates.Start);
        ResetToDefault();

    }

    void Pause()
    {
        Time.timeScale = GameData.PausedTimeScale;
        playerC.PlayerState.SwitchState(Player.PlayerStates.NonPlaying);
        playerCameraC.CameraState.SwitchState(PlayerCamera.PlayerCameraStates.Idle);

        DisableUI();
        PauseMenuObject.SetActive(true);

    }

    void Unpause()
    {
        Time.timeScale = GameData.UnPausedTimeScale;
        playerC.PlayerState.SwitchState(Player.PlayerStates.Playing);
        playerCameraC.CameraState.SwitchState(PlayerCamera.PlayerCameraStates.FollowPlayer);

        DisableUI();
    }

    void Playing2WonLost()
    {
        DisableUI();
        WonLostObject.SetActive(true);
        if (playerCameraC.CameraState.State != PlayerCamera.PlayerCameraStates.Idle)
            playerCameraC.CameraState.SwitchState(PlayerCamera.PlayerCameraStates.Idle);
        switch (playerC.PlayerState.State)
        {
            case Player.PlayerStates.Won:
                WonObjects.SetActive(true);
                break;
            case Player.PlayerStates.Lost:
                LostObjects.SetActive(true);
                break;
            default:
                throw new System.Exception("Game ended but player has neither won or lost");
        }
    }

    void WonLost2Menu()
    {
        Time.timeScale = GameData.UnPausedTimeScale;
        playerC.ResetToDefault();
        World.main.ResetToDefault();
        playerCameraC.ResetToDefault();
        DisableUI();
        MenuState.SwitchState(MenuStates.Start);
        ResetToDefault();
    }

    #endregion

    /// <summary>
    /// Runs GameState every frame
    /// </summary>
    void Update()
    {
        GameState.Execute();
    }

    /// <summary>
    /// Clears root object off its children
    /// </summary>
    public static void ClearRoot()
    {
        if(RootTransform)
        {
            for(int i = RootTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(RootTransform.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// Called when player wins
    /// </summary>
    public static void PlayerWin()
    {
        //print("player won");
        main.playerC.PlayerState.State = Player.PlayerStates.Won;
        main.GameState.State = GameStates.WonLost;
    }

    /// <summary>
    /// Called when player loses which is currently never
    /// </summary>
    public static void PlayerLose()
    {
        print("player lost");
        main.playerC.PlayerState.State = Player.PlayerStates.Lost;
        main.GameState.State = GameStates.WonLost;
    }

    /// <summary>
    /// Checks if player won
    /// </summary>
    /// <returns>True if player won, false otherwize</returns>
    public static bool CheckPlayerWin()
    {
        
        if(main.GameState.State == GameStates.Playing)
        {
            switch (World.main.GameType)
            {
                case WorldData.GameTypes.SumToZero:
                    if(World.main.Sum == 0)
                    {
                        return true;
                    }
                    break;
                case WorldData.GameTypes.ReachPoints:
                    if(World.main.ReachCellSum == 0)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if player lost which is currently nothing
    /// </summary>
    /// <returns>True if player lost, false otherwize</returns>
    public static bool CheckPlayerLose()
    {
        if (main.GameState.State == GameStates.Playing)
        {
            //TODO: ??? as player cannot currently die because of EventSystem to drive it back
        }
        return false;
    }

    public void ResetToDefault()
    {
        Events.Clear();
    }

}
