using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public enum PlayerStates
    {
        Playing,
        Won,
        Lost,
        NonPlaying
    }

    public StateMachine<PlayerStates> PlayerState;

    public int[] currentPosition;

    #region EVENTCLASSES

    public class PlayerMove : IGameEvent
    {

        Player player;

        int dimension, direction;

        GameEventExecutionResult res;

        public PlayerMove(Player player, int dimension, int direction)
        {

            res = GameEventExecutionResult.Failed;

            this.dimension = dimension;
            this.direction = direction;

            this.player = player;

            Execute();

        }

        public GameEventExecutionResult result { get { return res; } private set { res = value; } }

        public void Execute()
        {

            //crucial part is if player can make a move
            res = player.MakeMove(dimension, direction) ? GameEventExecutionResult.Success : GameEventExecutionResult.Failed;

            //if not then the event is failure
            if (res == GameEventExecutionResult.Failed)
                return;

            //reposition the player to new pos
            World w = World.main;
            if (w.Cells != null && player.currentPosition.Length > 0)
            {
                player.Reposition();
            }
        }

        public void Revert()
        {

            World w = World.main;

            //reverse cell visit effects
            w.Cells[player.currentPosition].Data.Number1++;
            w.Cells[player.currentPosition].Redraw();
            w.Sum++;
            w.RenderPositionChanges();

            //move player to old position
            player.currentPosition[dimension] -= direction;

            //reposition the player to old position
            if (w.Cells != null && player.currentPosition.Length > 0)
            {
                player.Reposition();
            }

        }

    }

    #endregion

    #region STATE_METHODS

    void Playing()
    {
        //check for moves between dimensions

        for (int i = 0; i < InputMapper.main.moveDimension.Length; i++)
        {
            if (InputMapper.main.moveDimension[i] != 0)
            {
                Scene.EventSystem.AddEvent(new PlayerMove(this, i, InputMapper.main.moveDimension[i]));
            }
        }
    }

    #endregion

    bool MakeMove(int dim, int direction)
    {

        //check if given dimensions is in level dim bounds
        if (dim >= World.main.Cells.Dimensions.Length || dim < 0)
            return false;

        int dt = currentPosition[dim] + direction;

        if (dt >= 0 && dt < World.main.Cells.Dimensions[dim])
        {

            //remember old position for case if cell cannot be visited
            int old = currentPosition[dim];

            //set new position to player
            currentPosition[dim] = dt;

            //print("Move: " + string.Join(", ", currentPosition) + " " + Cells[currentPosition].name + " i: " + Cells.getIndex(currentPosition));

            //empty cells cannot be visited
            if (World.main.Cells[currentPosition].Data.Number1 > 0)
            {

                World.main.Cells[currentPosition].Data.Number1--;
                World.main.Cells[currentPosition].Redraw();
                World.main.Sum--;

                World.main.RenderPositionChanges();

                return true;

            }
            else
            {

                //cell cannot be visited - rollback
                currentPosition[dim] = old;

            }

        }

        return false;
    }

    public void Reposition()
    {
        World w = World.main;
        Vector3 newPlayerPos = new Vector3(currentPosition[0] * w.buildingDistance, w.Cells[currentPosition].Data.Number1);
        if (currentPosition.Length > 1)
        {
            newPlayerPos.z = currentPosition[1] * w.buildingDistance;
        }
        transform.position = newPlayerPos;
    }


    private void Awake()
    {
        //playing at start for now
        PlayerState = new StateMachine<PlayerStates>(PlayerStates.Playing);

        //assign methods from sm
        PlayerState.Methods[PlayerStates.Playing] = Playing;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        PlayerState.Execute();

    }

}
