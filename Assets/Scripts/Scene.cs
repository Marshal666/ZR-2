using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages game scene
/// </summary>
public class Scene : MonoBehaviour
{

    public GameObject StartMenuObject = null;
    public GameObject LevelSelectMenuObject = null;
    public GameObject OptionsMenuObject = null;


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
    EventSystem es;

    /// <summary>
    /// Games event system
    /// </summary>
    public static EventSystem EventSystem { get { return main.es; } }

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
        es = new EventSystem();

    }

    /// <summary>
    /// Calls state machine to change game state
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
    /// Exits game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

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
            es.Undo();
        }

        if (InputMapper.main.Redo)
        {
            es.Redo();
        }

    }

    void Paused()
    {

    }

    void Menu()
    {

    }

    void Editor()
    {

    }

    void Pause()
    {

    }

    void Unpause()
    {

    }

    void SwitchUIToState(MenuStates state)
    {
        foreach (MenuStates ms in MenuObjects.Keys)
        {
            if (ms != state)
            {
                GameObject o = MenuObjects[ms];
                if(o)
                {
                    o.SetActive(false);
                }
            }
        }
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
        print("player won");
        main.playerC.PlayerState.State = Player.PlayerStates.Won;
        main.GameState.State = GameStates.Paused;
    }

    /// <summary>
    /// Called when player loses which is currently never
    /// </summary>
    public static void PlayerLose()
    {
        print("player lost");
        main.playerC.PlayerState.State = Player.PlayerStates.Lost;
        main.GameState.State = GameStates.Paused;
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
                case World.GameTypes.SumToZero:
                    if(World.main.Sum == 0)
                    {
                        return true;
                    }
                    break;
                case World.GameTypes.ReachPoints:
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

}
