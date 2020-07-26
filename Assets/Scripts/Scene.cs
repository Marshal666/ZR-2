using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{

    public enum GameStates
    {
        Playing,
        Paused,
        Menu,
        Editor,
        //more required?
    }

    public StateMachine<GameStates> GameState;

    Transform rootTransform;

    public GameObject root;

    public static Scene main;

    public static GameObject Root { get { return main.root; } }

    public static Transform RootTransform { get { return main.rootTransform; } }

    public GameObject player;

    Player playerC;

    public static GameObject PlayerObject { get { return main.player; } }

    public static Player Player { get { return main.playerC; } }

    EventSystem es;

    public static EventSystem EventSystem { get { return main.es; } }

    private void Awake()
    {

        main = this;

        //playing is temporary starting state
        GameState = new StateMachine<GameStates>(GameStates.Playing);

        //game state methods assignment
        GameState.Methods[GameStates.Playing] = Playing;
        GameState.Methods[GameStates.Paused] = Paused;
        GameState.Methods[GameStates.Menu] = Menu;
        GameState.Methods[GameStates.Editor] = Editor;

        GameState.Switches[GameStates.Playing][GameStates.Paused] = Pause;
        GameState.Switches[GameStates.Paused][GameStates.Playing] = Unpause;

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

    #region STATE_METHODS

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

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameState.Execute();
    }

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

    public static void PlayerWin()
    {
        print("player won");
        main.playerC.PlayerState.State = Player.PlayerStates.Won;
        main.GameState.State = GameStates.Paused;
    }

    public static void PlayerLose()
    {
        print("player lost");
        main.playerC.PlayerState.State = Player.PlayerStates.Lost;
        main.GameState.State = GameStates.Paused;
    }

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

    public static bool CheckPlayerLose()
    {
        if (main.GameState.State == GameStates.Playing)
        {
            //TODO: ??? as player cannot currently die because of EventSystem to drive it back
        }
        return false;
    }

}
