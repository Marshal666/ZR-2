using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for managing the player
/// </summary>
public class Player : MonoBehaviour
{

    /// <summary>
    /// Players states
    /// </summary>
    public enum PlayerStates
    {
        Playing,
        Won,
        Lost,
        NonPlaying
    }

    /// <summary>
    /// info about players moves
    /// </summary>
    public class PlayerMoveInfo
    {

        public List<int> ExCells = new List<int>();

    }

    /// <summary>
    /// Players state machine
    /// </summary>
    public StateMachine<PlayerStates> PlayerState;

    /// <summary>
    /// Current position of player in World
    /// </summary>
    public int[] CurrentPosition;

    /// <summary>
    /// World (cells) player is currently in
    /// </summary>
    public MArray<Cell> WorldIn;

    /// <summary>
    /// Object that draws world changes (World or Editor)
    /// </summary>
    public IWorldRenderer WorldRenderer;

    #region EVENTCLASSES

    /// <summary>
    /// Used for moving the player as GameEvent
    /// </summary>
    public class PlayerMove : IGameEvent
    {

        /// <summary>
        /// Player moved by this event
        /// </summary>
        Player player;

        /// <summary>
        /// Dimension and direction of players move
        /// </summary>
        int dimension, direction;

        /// <summary>
        /// Event execution result
        /// </summary>
        GameEventExecutionResult res;

        /// <summary>
        /// Info about the move
        /// </summary>
        PlayerMoveInfo info;

        /// <summary>
        /// Creates a new PlayerMove object
        /// </summary>
        /// <param name="player">Player to move</param>
        /// <param name="dimension">Dimension argument</param>
        /// <param name="direction">Direction argument</param>
        public PlayerMove(Player player, int dimension, int direction)
        {

            //by default, the event fails
            res = GameEventExecutionResult.Failed;

            this.dimension = dimension;
            this.direction = direction;

            this.player = player;

            info = new PlayerMoveInfo();

            //run the event straight after its creation
            Execute();

        }

        /// <summary>
        /// Result of events execution
        /// </summary>
        public GameEventExecutionResult Result { get { return res; } private set { res = value; } }

        /// <summary>
        /// Tries to move the player to given location
        /// </summary>
        public void Execute()
        {

            //crucial part is if player can make a move
            res = player.MakeMove(dimension, direction, ref info) ? GameEventExecutionResult.Success : GameEventExecutionResult.Failed;

            //if not then the event is failure
            if (res == GameEventExecutionResult.Failed)
                return;

            //reposition the player to new pos
            if (player.WorldIn != null && player.CurrentPosition.Length > 0)
            {
                player.Reposition();
            }
        }

        /// <summary>
        /// Reverts Execute
        /// </summary>
        public void Revert()
        {

            //revert all visited cells
            for (int i = info.ExCells.Count - 1; i >= 1; i--) {

                //simply unvisit the cell(s)
                player.WorldIn.OneDimensional[info.ExCells[i]].UnVisit.Invoke(player, ref info);
            }


            player.WorldRenderer.RenderPositionChanges();
            

            //reposition the player to old position
            if (player.WorldIn != null && player.CurrentPosition.Length > 0)
            {
                player.WorldIn.getCoordsNonAlloc(info.ExCells[0], ref player.CurrentPosition);
                player.Reposition();
            }

        }

    }

    #endregion

    #region STATE_METHODS

    /// <summary>
    /// Playing state method, called every frame
    /// </summary>
    void Playing()
    {

        //moving for 1st and 2nd dimension is special case because of camera rotation
        Vector2Int movement = new Vector2Int();

        if (InputMapper.main.moveDimension.Length > 0)
            movement.x = InputMapper.main.moveDimension[0];

        if (InputMapper.main.moveDimension.Length > 1)
            movement.y = InputMapper.main.moveDimension[1];

        int t;
        //rotate movement vector according to camera
        switch(PlayerCamera.main.lookDirectionIndex)
        {
            case 0: //forward
                //all fine
                break;
            case 1: //left
                t = movement.x;
                movement.x = -movement.y;
                movement.y = t;

                break;
            case 2: //down
                movement *= -1;

                break;
            case 3: //right
                t = movement.x;
                movement.x = movement.y;
                movement.y = -t;

                break;
            default: break;
        }

        //move first 2 dims if needed
        if(movement.x != 0)
        {
            Scene.EventSystem.AddEvent(new PlayerMove(this, 0, movement.x));
        }

        if (movement.y != 0)
        {
            Scene.EventSystem.AddEvent(new PlayerMove(this, 1, movement.y));
        }

        //move other dims
        for (int i = 2; i < InputMapper.main.moveDimension.Length; i++)
        {
            if (InputMapper.main.moveDimension[i] != 0)
            {
                Scene.EventSystem.AddEvent(new PlayerMove(this, i, InputMapper.main.moveDimension[i]));
            }
        }

    }

    #endregion

    /// <summary>
    /// Moves player in World
    /// </summary>
    /// <param name="dim">Dimension to visit</param>
    /// <param name="direction">Direction of visit (positive or negative)</param>
    /// <param name="info">Info of the event</param>
    /// <returns></returns>
    bool MakeMove(int dim, int direction, ref PlayerMoveInfo info)
    {

        //check if given dimensions is in level dim bounds
        if (dim >= WorldIn.Dimensions.Length || dim < 0)
            return false;

        //init & clear info if required
        if (info == null)
            info = new PlayerMoveInfo();

        info.ExCells.Clear();

        //delta
        int dt = CurrentPosition[dim] + direction;

        //if position is not outside of world bounds
        if (dt >= 0 && dt < WorldIn.Dimensions[dim])
        {

            //remember old position for case if cell cannot be visited
            int old = CurrentPosition[dim];
            info.ExCells.Add(WorldIn.getIndex(CurrentPosition));

            //set new position to player
            CurrentPosition[dim] = dt;

            //print("Move: " + string.Join(", ", currentPosition) + " " + Cells[currentPosition].name + " i: " + Cells.getIndex(currentPosition));

            //empty cells cannot be visited
            if (WorldIn[CurrentPosition].Data.Number1 != 0)
            {

                //visit cell
                WorldIn[CurrentPosition].Visit.Invoke(this, ref info);

                WorldRenderer.RenderPositionChanges();

                return true;

            }
            else
            {

                //cell cannot be visited - rollback
                CurrentPosition[dim] = old;

                info.ExCells.Clear();

            }

        }

        return false;
    }

    /// <summary>
    /// Repositions player in scene world according to its CurrentPos
    /// </summary>
    public void Reposition()
    {
        Vector3 newPlayerPos = new Vector3(CurrentPosition[0] * WorldRenderer.BuildingDistance, Mathf.Clamp(WorldIn[CurrentPosition].Data.Number1, 0f, float.MaxValue));
        if (CurrentPosition.Length > 1)
        {
            newPlayerPos.z = CurrentPosition[1] * WorldRenderer.BuildingDistance;
        }
        transform.position = newPlayerPos;
    }

    /// <summary>
    /// Script init
    /// </summary>
    private void Awake()
    {
        //menu is at start
        PlayerState = new StateMachine<PlayerStates>(PlayerStates.NonPlaying);

        //assign methods from sm
        PlayerState.Methods[PlayerStates.Playing] = Playing;

    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    void Update()
    {

        //Execute state method
        PlayerState.Execute();

    }


    public void ResetToDefault()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        if (PlayerState.State != PlayerStates.NonPlaying)
            PlayerState.SwitchState(PlayerStates.NonPlaying);
        WorldRenderer = null;
        WorldIn = null;
    }

}
