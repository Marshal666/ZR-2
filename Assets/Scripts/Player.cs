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
        public GameEventExecutionResult result { get { return res; } private set { res = value; } }

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
            World w = World.main;
            if (w.Cells != null && player.CurrentPosition.Length > 0)
            {
                player.Reposition();
            }
        }

        /// <summary>
        /// Reverts Execute
        /// </summary>
        public void Revert()
        {

            World w = World.main;

            //revert all visited cells
            for (int i = info.ExCells.Count - 1; i >= 1; i--) {

                //simply unvisit the cell(s)
                w.Cells.OneDimensional[info.ExCells[i]].UnVisit.Invoke(player, ref info);

                #region OLD_UNVISIT

                /*Cell cell = w.Cells.OneDimensional[info.ExCells[i]];
                cell.Data.Number1++;
                if (cell.Data.Type == CellData.CellType.Increaser)
                    cell.IncreaserReverseIncrease();
                cell.Redraw();
                

                if (cell.Data.Number1 > 0)
                {
                    switch (w.GameType)
                    {
                        case World.GameTypes.SumToZero:
                            if (cell.Data.Type != CellData.CellType.ReachCell)
                                w.Sum++;
                            break;
                        case World.GameTypes.ReachPoints:
                            if (cell.Data.Type == CellData.CellType.ReachCell)
                                w.ReachCellSum++;
                            break;
                        default:
                            break;
                    }
                }*/

                #endregion

            }


            w.RenderPositionChanges();
            

            //reposition the player to old position
            if (w.Cells != null && player.CurrentPosition.Length > 0)
            {
                w.Cells.getCoordsNonAlloc(info.ExCells[0], ref player.CurrentPosition);
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
        if (dim >= World.main.Cells.Dimensions.Length || dim < 0)
            return false;

        //init & clear info if required
        if (info == null)
            info = new PlayerMoveInfo();

        info.ExCells.Clear();

        //delta
        int dt = CurrentPosition[dim] + direction;

        //if position is not outside of world bounds
        if (dt >= 0 && dt < World.main.Cells.Dimensions[dim])
        {

            //remember old position for case if cell cannot be visited
            int old = CurrentPosition[dim];
            info.ExCells.Add(World.main.Cells.getIndex(CurrentPosition));

            //set new position to player
            CurrentPosition[dim] = dt;

            //print("Move: " + string.Join(", ", currentPosition) + " " + Cells[currentPosition].name + " i: " + Cells.getIndex(currentPosition));

            //empty cells cannot be visited
            if (World.main.Cells[CurrentPosition].Data.Number1 != 0)
            {

                //visit cell
                World.main.Cells[CurrentPosition].Visit.Invoke(this, ref info);

                #region OLD_VISIT
                /*switch (World.main.Cells[CurrentPosition].Data.Type)
                {
                    case CellData.CellType.Default:
                    case CellData.CellType.Start:

                        Cell cellDS = World.main.Cells[CurrentPosition];

                        cellDS.Data.Number1--;
                        cellDS.Redraw();
                        if (cellDS.Data.Number1 >= 0)
                            World.main.Sum--;

                        info.ExCells.Add(World.main.Cells.getIndex(CurrentPosition));

                        break;
                    case CellData.CellType.TeleportIn:

                        Cell cellTI = World.main.Cells[CurrentPosition];

                        cellTI.Data.Number1--;
                        cellTI.Redraw();
                        if (cellTI.Data.Number1 >= 0)
                            World.main.Sum--;

                        info.ExCells.Add(World.main.Cells.getIndex(CurrentPosition));

                        int pos = World.main.Cells[CurrentPosition].Data.Number2;

                        while(World.main.Cells.OneDimensional[pos].Data.Type == CellData.CellType.TeleportIn && World.main.Cells.OneDimensional[pos].Data.Number1 > 0)
                        {

                            info.ExCells.Add(pos);

                            Cell cell = World.main.Cells.OneDimensional[pos];
                            cell.Data.Number1--;
                            cell.Redraw();
                            if (cell.Data.Number1 >= 0)
                                World.main.Sum--;

                            pos = cell.Data.Number2;

                        }

                        info.ExCells.Add(pos);

                        cellTI = World.main.Cells.OneDimensional[pos];

                        cellTI.Data.Number1--;
                        cellTI.Redraw();
                        if (cellTI.Data.Number1 >= 0)
                            World.main.Sum--;

                        //move player to pos
                        World.main.Cells.getCoordsNonAlloc(pos, ref CurrentPosition);


                        break;

                    case CellData.CellType.ReachCell:

                        Cell cellRC = World.main.Cells[CurrentPosition];

                        cellRC.Data.Number1--;
                        cellRC.Redraw();
                        if (cellRC.Data.Number1 >= 0)
                            World.main.ReachCellSum--;

                        info.ExCells.Add(World.main.Cells.getIndex(CurrentPosition));

                        break;

                    case CellData.CellType.Increaser:

                        Cell cellI = World.main.Cells[CurrentPosition];

                        cellI.Data.Number1--;
                        cellI.Redraw();
                        if (cellI.Data.Number1 >= 0)
                        {
                            World.main.Sum--;
                            //conditional increase?
                            //cellI.IncraserIncrease();
                        }

                        cellI.IncraserIncrease();

                        info.ExCells.Add(World.main.Cells.getIndex(CurrentPosition));

                        break;

                    default:
                        //error?
                        break;
                }*/

                #endregion

                World.main.RenderPositionChanges();

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
        World w = World.main;
        Vector3 newPlayerPos = new Vector3(CurrentPosition[0] * w.buildingDistance, Mathf.Clamp(w.Cells[CurrentPosition].Data.Number1, 0f, float.MaxValue));
        if (CurrentPosition.Length > 1)
        {
            newPlayerPos.z = CurrentPosition[1] * w.buildingDistance;
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

}
