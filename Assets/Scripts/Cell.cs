using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Class for managing cells
/// </summary>
public class Cell : MonoBehaviour
{

    /// <summary>
    /// Cells data
    /// </summary>
    public CellData Data;

    /// <summary>
    /// Script init
    /// </summary>
    private void Awake()
    {
        Visit += BaseVisit;
        UnVisit += BaseUnVisit;
    }

    /// <summary>
    /// Initializes cell delegate methods
    /// </summary>
    public void Init()
    {
        if((Data.Type & CellData.CellType.Default) == CellData.CellType.Default)
        {
            Visit += DefaultVisit;
            UnVisit += DefaultUnVisit;
        }
        if ((Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn)
        {
            Visit += TeleportInVisit;
            UnVisit += TeleportInUnVisit;
        }
        if((Data.Type & CellData.CellType.ReachCell) == CellData.CellType.ReachCell)
        {
            Visit += ReachCellVisit;
            UnVisit += ReachCellUnVisit;
        }
        if ((Data.Type & CellData.CellType.Increaser) == CellData.CellType.Increaser)
        {
            Visit += IncreaserVisit;
            UnVisit += IncreaserUnVisit;
        }
    }

    /// <summary>
    /// Draws cell objects
    /// </summary>
    public void Draw()
    {

        for (int i = 0; i < Data.Number1; i++)
        {
            GameObject g = Instantiate(GameData.BuildingBlocks[Data.BuildingType]);
            g.transform.SetParent(transform);
            g.transform.localPosition = Vector3.up * i;
        }

    }

    /// <summary>
    /// Expects that cell was already drawn, redraws it according to current state
    /// </summary>
    public void Redraw()
    {

        for(int i = 0; i < Mathf.Max(Data.Number1, transform.childCount); i++)
        {
            GameObject o;
            if (i < transform.childCount) {
                o = transform.GetChild(i).gameObject;
            }
            else {
                o = Instantiate(GameData.BuildingBlocks[Data.BuildingType]);
                o.transform.SetParent(transform);
                o.transform.localPosition = Vector3.up * i;
            }
            o.SetActive(i < Data.Number1);
        }

    }

    /// <summary>
    /// For Visit methods
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    public delegate void VisitDelegate(Player player, ref Player.PlayerMoveInfo moveInfo);

    /// <summary>
    /// Visit methods
    /// </summary>
    public VisitDelegate Visit;

    /// <summary>
    /// For UnVisit methods
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    public delegate void UnVisitDelegate(Player player, ref Player.PlayerMoveInfo moveInfo);

    /// <summary>
    /// UnVisit methods
    /// </summary>
    public UnVisitDelegate UnVisit;

    #region VISIT_UNVISITMETHODS

    /// <summary>
    /// Preforms basic cell visit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void BaseVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {

        moveInfo.ExCells.Add(World.main.Cells.getIndex(player.CurrentPosition));

    }

    /// <summary>
    /// Reverts BaseVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void BaseUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {
        //nothing here actually
    }

    /// <summary>
    /// Preforms default visit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void DefaultVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {

        Data.Number1--;
        Redraw();

        if (Data.Number1 >= 0)
            World.main.Sum--;

        //print("dv");

    }

    /// <summary>
    /// Reverts DefaultVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void DefaultUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {

        Data.Number1++;
        Redraw();

        if (Data.Number1 > 0)
            World.main.Sum++;

        //print("duv");

    }

    /// <summary>
    /// Preforms a teleport visit & teleportation to other (teleport) cell(s)
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void TeleportInVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {

        #region RECURSIVE_WAY
        //recursive solution - I don't like it
        /*
        int pos = Data.Number2;

        World.main.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

        if (Data.Number1 > 0 || (Data.Number1 == 0 && World.main.Cells.OneDimensional[pos] != this))
        {

            World.main.Cells.OneDimensional[pos].Visit.Invoke(player, ref moveInfo);

        }*/
        #endregion

        //better iterative solution

        //visit cell this teleport points to, visit condition is that Number1 >= 0
        if (Data.Number1 >= 0)
        {

            int pos = Data.Number2;

            Cell cell = World.main.Cells.OneDimensional[pos];

            World.main.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

            //visit pointed cell but not the teleport method(if any)
            (cell.Visit - cell.TeleportInVisit).Invoke(player, ref moveInfo);

            //visit teleport chain, condition for teleporting is that Number1 must be >= 0 to avoid infinite loops
            while ((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn && cell.Data.Number1 >= 0)
            {

                //visit cell pointed by other teleport
                pos = cell.Data.Number2;

                cell = World.main.Cells.OneDimensional[pos];

                World.main.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

                (cell.Visit - cell.TeleportInVisit).Invoke(player, ref moveInfo);

            }

            //position player to last visited cll
            World.main.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

        }

    }

    /// <summary>
    /// Does nothing as PlayerMove.Revert unvisits all necessary cells
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void TeleportInUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {
        //anything required here? -seems not since ExCells in info has all required data
    }

    /// <summary>
    /// Reach cell visit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void ReachCellVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {
        Data.Number1--;
        Redraw();

        if (Data.Number1 >= 0)
            World.main.ReachCellSum--;
    }

    /// <summary>
    /// Reverts ReachCellVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void ReachCellUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {
        Data.Number1++;
        Redraw();

        if (Data.Number1 >= 0)
            World.main.ReachCellSum++;
    }

    /// <summary>
    /// Runs Increasers job
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void IncreaserVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {
        IncraserIncrease();
    }

    /// <summary>
    /// Reverts IncreaserVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void IncreaserUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo)
    {
        IncreaserReverseIncrease();
    }

    #endregion

    /// <summary>
    /// Makes increaser do its job and keeping world sums valid
    /// </summary>
    public void IncraserIncrease()
    {
        if(World.main.CellGroups != null)
        {
            //get belonging cell group
            int[] AffectedCellGroup = World.main.CellGroups[Data.AffectedCellGroup];

            //for tracking how sum of group changes
            (int , int) sumsOld = (0, 0), sumsNew = (0, 0);

            //for every element of group, save old sum(s), change Number1 value, new sum(s) and redraw the cell
            for(int i = 0; i < AffectedCellGroup.Length; i++)
            {

                Cell cell = World.main.Cells.OneDimensional[AffectedCellGroup[i]];

                if ((cell.Data.Type & CellData.CellType.Default) == CellData.CellType.Default)
                {
                    sumsOld.Item1 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                if ((cell.Data.Type & CellData.CellType.ReachCell) == CellData.CellType.ReachCell)
                {
                    sumsOld.Item2 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                cell.Data.Number1 += Data.Number3;

                if ((cell.Data.Type & CellData.CellType.Default) == CellData.CellType.Default)
                {
                    sumsNew.Item1 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                if ((cell.Data.Type & CellData.CellType.ReachCell) == CellData.CellType.ReachCell)
                {
                    sumsNew.Item2 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                cell.Redraw();

            }

            //apply sum differences to world

            World.main.Sum += (sumsNew.Item1 - sumsOld.Item1);

            World.main.ReachCellSum += (sumsNew.Item2 - sumsOld.Item2);

        }
    }

    /// <summary>
    /// Reverts increasers job while keeping world sums valid
    /// </summary>
    public void IncreaserReverseIncrease()
    {
        if (World.main.CellGroups != null)
        {

            //get belonging cell group
            int[] AffectedCellGroup = World.main.CellGroups[Data.AffectedCellGroup];

            //for tracking how sum of group changes
            (int, int) sumsOld = (0, 0), sumsNew = (0, 0);

            //for every element of group, save old sum(s), change (revert) Number1 value, new sum(s) and redraw the cell
            for (int i = 0; i < AffectedCellGroup.Length; i++)
            {

                Cell cell = World.main.Cells.OneDimensional[AffectedCellGroup[i]];

                if ((cell.Data.Type & CellData.CellType.Default) == CellData.CellType.Default)
                {
                    sumsOld.Item1 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                if ((cell.Data.Type & CellData.CellType.ReachCell) == CellData.CellType.ReachCell)
                {
                    sumsOld.Item2 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                cell.Data.Number1 -= Data.Number3;

                if ((cell.Data.Type & CellData.CellType.Default) == CellData.CellType.Default)
                {
                    sumsNew.Item1 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                if ((cell.Data.Type & CellData.CellType.ReachCell) == CellData.CellType.ReachCell)
                {
                    sumsNew.Item2 += cell.Data.Number1 > 0 ? cell.Data.Number1 : 0;
                }

                cell.Redraw();

            }

            //revert sum changes

            World.main.Sum += (sumsNew.Item1 - sumsOld.Item1);

            World.main.ReachCellSum += (sumsNew.Item2 - sumsOld.Item2);

        }
    }

}
