using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TreeNode
{

    public CellTest Cell;

    public TreeNode Up = null, Left = null, Right = null, Down = null;
    public TreeNode Parent = null;

    public int Position;

    public TreeNode this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return Up;
                case 1:
                    return Left;
                case 2:
                    return Down;
                case 3:
                    return Right;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }
        set
        {
            switch (i)
            {
                case 0:
                    Up = value;
                    return;
                case 1:
                    Left = value;
                    return;
                case 2:
                    Down = value;
                    return;
                case 3:
                    Right = value;
                    return;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }
    }

    public TreeNode(CellTest cell, TreeNode parent = null)
    {
        Cell = cell;
        Parent = parent;
    }

    public TreeNode(CellTest cell, TreeNode parent, int position)
    {
        Cell = cell;
        Parent = parent;
        Position = position;
    }

    public void ClearChildren()
    {
        Up = Left = Right = Down = null;
    }

    public override string ToString()
    {
        return "Node Position: " + Position;
    }
}

public class CellTest
{

    public CellData Data;

    public WorldTester World;

    public delegate void VisitDelegate();

    public delegate void UnVisitDelegate();

    public VisitDelegate VisitMethods;

    public UnVisitDelegate UnVisitMethods;

    public CellTest(CellData data, WorldTester tester)
    {
        Data = data;
        World = tester;

        if(data.Type == CellData.CellType.Default)
        {
            VisitMethods += DefaultVisit;
            UnVisitMethods += DefaultUnVisit;
        }
        //TODO: add for teleporters and other things
    }

    public void Visit()
    {
        VisitMethods.Invoke();
    }

    public void UnVisit()
    {
        UnVisitMethods.Invoke();
    }

    public void DefaultVisit()
    {
        Data.Number1--;

        if (Data.Number1 >= 0)
            World.Sum--;
    }

    public void DefaultUnVisit()
    {
        Data.Number1++;

        if (Data.Number1 > 0)
            World.Sum++;
    }

    //TODO: Finish methods, generate tree nodes
    public void TeleportVisit()
    {
        if(Data.Number1 >= 0)
        {
            int pos = Data.Number2;

            CellTest cell = World.Cells.OneDimensional[pos];

            //visit pointed cell but not the teleport method(if any)
            (cell.VisitMethods - cell.TeleportVisit).Invoke();

            while ((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn && cell.Data.Number1 >= 0)
            {

                //visit cell pointed by other teleport
                pos = cell.Data.Number2;

                cell = World.Cells.OneDimensional[pos];

                (cell.VisitMethods - cell.TeleportVisit).Invoke();

            }

        }



    }

    public void TeleportUnVisit()
    {

    }

    public override string ToString()
    {
        return "Data: " + Data.ToString();
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

    public WorldTester(WorldData data)
    {
        if (data == null)
            throw new ArgumentNullException("Given data is null.");

        Data = data.Duplicate();
        Cells = new MArray<CellTest>(Data.CellDatas.Dimensions);

        for(int i = 0; i < Cells.OneDimensional.Length; i++)
        {
            Cells.OneDimensional[i] = new CellTest(Data.CellDatas.OneDimensional[i], this);
        }

        CalculateSums();
    }
    
    public void Visit(TreeNode position, int direction)
    {

    }

    public void UnVisit(TreeNode position)
    {

    }

    //TODO: add reach sum
    void CalculateSums()
    {
        Sum = 0;
        for(int i = 0; i < Cells.OneDimensional.Length; i++)
        {
            if (Cells.OneDimensional[i].Data.Number1 > 0)
                Sum += Cells.OneDimensional[i].Data.Number1;
        }
    }

    public void ChangeDataCells()
    {
        Data.CellDatas = new MArray<CellData>(Cells.Dimensions);
        for(int i = 0; i < Cells.OneDimensional.Length; i++)
        {
            Data.CellDatas.OneDimensional[i] = Cells.OneDimensional[i].Data;
        }
    }

    public IEnumerator BuildTreeSteps()
    {
        Debug.Log("Building Tree");

        WorldData old = GameEditor.main.LevelData;

        int startPosition = Data.CellDatas.getIndex(Data.PlayerStartPosition);
        int[] position = new int[2];
        CalculateSums();

        Tree = new TreeNode(Cells.OneDimensional[startPosition], null, startPosition);

        Stack<TreeNode> open = new Stack<TreeNode>();
        open.Push(Tree);

        bool done = false;
        TreeNode doneNode = Tree;

        TreeNode previous = null;

        while (open.Count > 0)
        {
            //Debug.Log("Open: " + string.Join(", ", open));
            TreeNode current = open.Pop();
            Cells.getCoordsNonAlloc(current.Position, ref position);

            //Debug.Log("Pos: " + string.Join(", ", position) + " Cells state: " + Cells);

            //visit all the way back if this state (cell) belongs to other branch (clear tree for memory)
            while (previous != current.Parent)
            {
                position = Cells.getCoords(previous.Position);

                UpdateView();
                Debug.Log("Going back");
                yield return null;

                previous.Cell.UnVisit();
                previous.ClearChildren();
                previous = previous.Parent;

                UpdateView();
                Debug.Log("Going back");
                yield return null;
            }

            //can't visit the root node state
            if (current.Parent != null)
                current.Cell.Visit();

            UpdateView();

            if (Sum == 0)
            {
                done = true;
                doneNode = current;
                break;
            } else
            {
                yield return null;
            }

            position[0]++;
            if (CanVisit(0, position[0]) && Cells[position].Data.Number1 > 0)
            {
                current.Right = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Right);
            }
            position[0] -= 2;
            if (CanVisit(0, position[0]) && Cells[position].Data.Number1 > 0)
            {
                current.Left = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Left);
            }
            position[0]++;

            position[1]++;
            if (CanVisit(1, position[1]) && Cells[position].Data.Number1 > 0)
            {
                current.Up = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Up);
            }
            position[1] -= 2;
            if (CanVisit(1, position[1]) && Cells[position].Data.Number1 > 0)
            {
                current.Down = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Down);
            }
            position[1]++;

            previous = current;

            //yield return null;

        }

        if (done)
        {
            Debug.Log("Zero sum found!");
            List<int> path = new List<int>();
            while (doneNode != null)
            {
                path.Add(doneNode.Position);
                doneNode = doneNode.Parent;
            }
            path.Reverse();
            StringBuilder pathS = new StringBuilder(1024);
            for (int i = 0; i < path.Count - 1; i++)
            {
                pathS.Append("{");
                pathS.Append(string.Join(", ", Cells.getCoords(path[i])));
                pathS.Append("}, "); ;
            }
            pathS.Append("{");
            pathS.Append(string.Join(", ", Cells.getCoords(path[path.Count - 1])));
            pathS.Append("}");
            Debug.Log("Path: " + pathS.ToString());
        }
        else
        {
            Debug.Log("No zero sum found!");
        }

        bool CanVisit(int dim, int move)
        {
            return move >= 0 && move < Cells.Dimensions[dim];
        }

        yield return null;

        GameEditor.main.LevelData = old;
        
        GameEditor.main.RedrawCells(old);
        Scene.Player.CurrentPosition = old.PlayerStartPosition;
        Scene.Player.Reposition();

        yield break;

        void UpdateView()
        {
            ChangeDataCells();
            GameEditor.main.RedrawCells(Data);
            Scene.Player.CurrentPosition = position;
            Scene.Player.Reposition();
        }

    }

    public void BuildTree()
    {
        int startPosition = Data.CellDatas.getIndex(Data.PlayerStartPosition);
        int[] position = new int[2];
        CalculateSums();

        Tree = new TreeNode(Cells.OneDimensional[startPosition], null, startPosition);

        Stack<TreeNode> open = new Stack<TreeNode>();
        open.Push(Tree);

        bool done = false;
        TreeNode doneNode = Tree;

        TreeNode previous = null;

        (int sum, List<int> path) bestPath = (int.MaxValue, null);

        while(open.Count > 0)
        {
            TreeNode current = open.Pop();
            Cells.getCoordsNonAlloc(current.Position, ref position);

            //Debug.Log("Pos: " + string.Join(", ", position) + " Cells state: " + Cells);

            //visit all the way back if this state (cell) belongs to other branch (clear tree for memory)
            while (previous != current.Parent)
            {
                previous.Cell.UnVisit();
                previous.ClearChildren();
                previous = previous.Parent;
            }

            //can't visit the root node state
            if (current.Parent != null)
                current.Cell.Visit();

            if(Sum < bestPath.sum)
            {
                bestPath.sum = Sum;
                bestPath.path = RetrackPath(current);
            }

            if(Sum == 0)
            {
                done = true;
                doneNode = current;
                break;
            }

            position[0]++;
            if (CanVisit(0, position[0]) && Cells[position].Data.Number1 > 0)
            {
                current.Right = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Right);
            }
            position[0] -= 2;
            if (CanVisit(0, position[0]) && Cells[position].Data.Number1 > 0)
            {
                current.Left = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Left);
            }
            position[0]++;

            position[1]++;
            if (CanVisit(1, position[1]) && Cells[position].Data.Number1 > 0)
            {
                current.Up = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Up);
            }
            position[1] -= 2;
            if (CanVisit(1, position[1]) && Cells[position].Data.Number1 > 0)
            {
                current.Down = new TreeNode(Cells[position], current, Cells.getIndex(position));
                open.Push(current.Down);
            }
            position[1]++;

            previous = current;

        }

        if(done)
        {
            Debug.Log("Zero sum found! Score: " + (bestPath.path.Count - 1));
            List<int> path = bestPath.path;
            path.Reverse();
            StringBuilder pathS = new StringBuilder(1024);
            for(int i = 0; i < path.Count - 1; i++)
            {
                pathS.Append("{");
                pathS.Append(string.Join(", ", Cells.getCoords(path[i])));
                pathS.Append("}, ");;
            }
            pathS.Append("{");
            pathS.Append(string.Join(", ", Cells.getCoords(path[path.Count - 1])));
            pathS.Append("}");
            Debug.Log("Path: " + pathS.ToString());
        } else
        {
            Debug.Log("No zero sum found! Best sum is: " + bestPath.sum + " Score: " + (bestPath.path.Count - 1));
            List<int> path = bestPath.path;
            path.Reverse();
            StringBuilder pathS = new StringBuilder(1024);
            for (int i = 0; i < path.Count - 1; i++)
            {
                pathS.Append("{");
                pathS.Append(string.Join(", ", Cells.getCoords(path[i])));
                pathS.Append("}, "); ;
            }
            pathS.Append("{");
            pathS.Append(string.Join(", ", Cells.getCoords(path[path.Count - 1])));
            pathS.Append("}");
            Debug.Log("Path: " + pathS.ToString());
        }

        bool CanVisit(int dim, int move)
        {
            return move >= 0 && move < Cells.Dimensions[dim];
        }
    }

    List<int> RetrackPath(TreeNode node)
    {
        List<int> ret = new List<int>();
        while(node != null)
        {
            ret.Add(node.Position);
            node = node.Parent;
        }
        return ret;
    }

}
