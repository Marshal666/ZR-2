using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellTest
{

    public CellData Data;

    public WorldTester World;

    public delegate void VisitDelegate(ref int playerPosition);

    public delegate void UnVisitDelegate(ref int playerPosition);

    public VisitDelegate VisitMethods;

    public UnVisitDelegate UnVisitMethods;

    public void DefaultVisit(ref int playerPosition)
    {
        Data.Number1--;

        if (Data.Number1 >= 0)
            World.Sum--;
    }

    public void DefaultUnVisit(ref int playerPosition)
    {
        Data.Number1++;

        if (Data.Number1 > 0)
            World.Sum++;
    }

    public void TeleportVisit(ref int playerPosition)
    {
        if(Data.Number1 >= 0)
        {
            int pos = Data.Number2;

            CellTest cell = World.Cells.OneDimensional[pos];

            //visit pointed cell but not the teleport method(if any)
            (cell.VisitMethods - cell.TeleportVisit).Invoke(ref playerPosition);

            while ((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn && cell.Data.Number1 >= 0)
            {

                //visit cell pointed by other teleport
                pos = cell.Data.Number2;

                cell = World.Cells.OneDimensional[pos];

                (cell.VisitMethods - cell.TeleportVisit).Invoke(ref playerPosition);

            }

        }
    }

}

public class WorldTester
{

    public MArray<CellTest> Cells;

    public WorldData Data;

    public int[][] CellGroups { get { return Data.CellGroups; } set { Data.CellGroups = value; } }

    public int Sum;

    public int ReachSum;

    public Player.PlayerMoveInfo MoveInfo;

    public TreeNode Tree;

    public void Visit(TreeNode position, int direction)
    {

    }

    public void UnVisit()
    {

    }

}
