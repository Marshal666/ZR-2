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

    //info for events about player moves
    public class PlayerMoveInfo
    {

        public List<int> ExCells = new List<int>();

    }

    public StateMachine<PlayerStates> PlayerState;

    public int[] currentPosition;

    #region EVENTCLASSES

    public class PlayerMove : IGameEvent
    {

        Player player;

        int dimension, direction;

        GameEventExecutionResult res;

        PlayerMoveInfo info;

        public PlayerMove(Player player, int dimension, int direction)
        {

            res = GameEventExecutionResult.Failed;

            this.dimension = dimension;
            this.direction = direction;

            this.player = player;

            info = new PlayerMoveInfo();

            Execute();

        }

        public GameEventExecutionResult result { get { return res; } private set { res = value; } }

        public void Execute()
        {

            //crucial part is if player can make a move
            res = player.MakeMove(dimension, direction, ref info) ? GameEventExecutionResult.Success : GameEventExecutionResult.Failed;

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

            for (int i = info.ExCells.Count - 1; i >= 1; i--) {

                Cell cell = w.Cells.OneDimensional[info.ExCells[i]];
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
                }

            }


            w.RenderPositionChanges();
            

            //reposition the player to old position
            if (w.Cells != null && player.currentPosition.Length > 0)
            {
                w.Cells.getCoordsNonAlloc(info.ExCells[0], ref player.currentPosition);
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

    bool MakeMove(int dim, int direction, ref PlayerMoveInfo info)
    {

        //check if given dimensions is in level dim bounds
        if (dim >= World.main.Cells.Dimensions.Length || dim < 0)
            return false;

        if (info == null)
            info = new PlayerMoveInfo();

        info.ExCells.Clear();

        int dt = currentPosition[dim] + direction;

        if (dt >= 0 && dt < World.main.Cells.Dimensions[dim])
        {

            //remember old position for case if cell cannot be visited
            int old = currentPosition[dim];
            info.ExCells.Add(World.main.Cells.getIndex(currentPosition));

            //set new position to player
            currentPosition[dim] = dt;

            //print("Move: " + string.Join(", ", currentPosition) + " " + Cells[currentPosition].name + " i: " + Cells.getIndex(currentPosition));

            //empty cells cannot be visited
            if (World.main.Cells[currentPosition].Data.Number1 != 0)
            {

                switch (World.main.Cells[currentPosition].Data.Type)
                {
                    case CellData.CellType.Default:
                    case CellData.CellType.Start:

                        Cell cellDS = World.main.Cells[currentPosition];

                        cellDS.Data.Number1--;
                        cellDS.Redraw();
                        if (cellDS.Data.Number1 >= 0)
                            World.main.Sum--;

                        info.ExCells.Add(World.main.Cells.getIndex(currentPosition));

                        break;
                    case CellData.CellType.TeleportIn:

                        Cell cellTI = World.main.Cells[currentPosition];

                        cellTI.Data.Number1--;
                        cellTI.Redraw();
                        if (cellTI.Data.Number1 >= 0)
                            World.main.Sum--;

                        info.ExCells.Add(World.main.Cells.getIndex(currentPosition));

                        //TODO: visit other teleports/cell straght away

                        int pos = World.main.Cells[currentPosition].Data.Number2;

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
                        World.main.Cells.getCoordsNonAlloc(pos, ref currentPosition);


                        break;

                    case CellData.CellType.ReachCell:

                        Cell cellRC = World.main.Cells[currentPosition];

                        cellRC.Data.Number1--;
                        cellRC.Redraw();
                        if (cellRC.Data.Number1 >= 0)
                            World.main.ReachCellSum--;

                        info.ExCells.Add(World.main.Cells.getIndex(currentPosition));

                        break;

                    case CellData.CellType.Increaser:

                        Cell cellI = World.main.Cells[currentPosition];

                        cellI.Data.Number1--;
                        cellI.Redraw();
                        if (cellI.Data.Number1 >= 0)
                        {
                            World.main.Sum--;
                            //conditional increase?
                            //cellI.IncraserIncrease();
                        }

                        cellI.IncraserIncrease();

                        info.ExCells.Add(World.main.Cells.getIndex(currentPosition));

                        break;

                    default:
                        //error?
                        break;
                }

                

                World.main.RenderPositionChanges();

                return true;

            }
            else
            {

                //cell cannot be visited - rollback
                currentPosition[dim] = old;

                info.ExCells.Clear();

            }

        }

        return false;
    }

    public void Reposition()
    {
        World w = World.main;
        Vector3 newPlayerPos = new Vector3(currentPosition[0] * w.buildingDistance, Mathf.Clamp(w.Cells[currentPosition].Data.Number1, 0f, float.MaxValue));
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
