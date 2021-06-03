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

    public delegate void VisitDelegate(ref int playerPos, ref TreeNode node);

    public delegate void UnVisitDelegate(ref int playerPos, ref TreeNode node);

    public VisitDelegate VisitMethods;

    public UnVisitDelegate UnVisitMethods;

    public CellTest(CellData data, WorldTester tester)
    {
        Data = data;
        World = tester;

        if((data.Type & CellData.CellType.Default) == CellData.CellType.Default)
        {
            VisitMethods += DefaultVisit;
            UnVisitMethods += DefaultUnVisit;
        }

        if ((data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn)
        {
            VisitMethods += TeleportVisit;
            UnVisitMethods += TeleportUnVisit;
        }

        if((data.Type & CellData.CellType.Increaser) == CellData.CellType.Increaser)
        {
            VisitMethods += DecreaserVisit;
            UnVisitMethods += DecreaserUnVisit;
        }
        
    }

    public int PeekVisit(ref int playerPos, ref TreeNode node)
    {
        int s = 0;
        int po = playerPos;
        TreeNode no = node;

        playerPos = node.Position;

        VisitMethods.Invoke(ref playerPos, ref node);
        s = World.Sum;

        TreeNode tn = node;

        while(tn != no)
        {
            tn.Cell.UnVisit(ref playerPos, ref node);
            tn.ClearChildren();
            tn = tn.Parent;
        }

        node = no;
        
        UnVisitMethods.Invoke(ref playerPos, ref node);

        playerPos = po;
        node = no;

        return s;
    }

    public void Visit(ref int playerPos, ref TreeNode node)
    {
        VisitMethods.Invoke(ref playerPos, ref node);
    }

    public void UnVisit(ref int playerPos, ref TreeNode node)
    {
        UnVisitMethods.Invoke(ref playerPos, ref node);
    }

    public void DefaultVisit(ref int playerPos, ref TreeNode node)
    {
        Data.Number1--;

        if (Data.Number1 >= 0)
            World.Sum--;
    }

    public void DefaultUnVisit(ref int playerPos, ref TreeNode node)
    {
        Data.Number1++;

        if (Data.Number1 > 0)
            World.Sum++;
    }

    public void TeleportVisit(ref int playerPos, ref TreeNode node)
    {
        if(Data.Number1 >= 0)
        {
            int pos = Data.Number2;

            CellTest cell = World.Cells.OneDimensional[pos];

            //visit pointed cell but not the teleport method(if any)
            (cell.VisitMethods - cell.TeleportVisit).Invoke(ref playerPos, ref node);

            node.Left = new TreeNode(cell, node, pos);
            node = node.Left;

            playerPos = pos;

            while ((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn && cell.Data.Number1 >= 0)
            {

                //visit cell pointed by other teleport
                pos = cell.Data.Number2;

                cell = World.Cells.OneDimensional[pos];

                (cell.VisitMethods - cell.TeleportVisit).Invoke(ref playerPos, ref node);

                node.Left = new TreeNode(cell, node, pos);

                node = node.Left;

                playerPos = pos;

            }

            playerPos = pos;

        }

    }

    public void TeleportUnVisit(ref int playerPos, ref TreeNode node)
    {
        //all nodes are in a tree
       /*if(Data.Number1 >= -1)
        {

            int pos = Data.Number2;

            CellTest cell = World.Cells.OneDimensional[pos];

            //visit pointed cell but not the teleport method(if any)
            (cell.VisitMethods - cell.TeleportVisit).Invoke(ref playerPos, ref node);

            //playerPos = pos;

            Stack<int> visiteds = new Stack<int>();

            while ((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn && cell.Data.Number1 >= 0)
            {

                //visit cell pointed by other teleport
                pos = cell.Data.Number2;

                visiteds.Push(pos);

                cell = World.Cells.OneDimensional[pos];

            }

            while(visiteds.Count > 0)
            {
                int p = visiteds.Pop();
                CellTest c = World.Cells.OneDimensional[pos];
                (c.UnVisitMethods - c.TeleportUnVisit).Invoke(ref playerPos, ref node);
            }

        }*/
    }

    public void DecreaserVisit(ref int playerPos, ref TreeNode node)
    {

        if(World.CellGroups != null)
        {

            WorldTester w = World;

            int[] AffectedCellGroup = w.CellGroups[Data.AffectedCellGroup];

            //for tracking how sum of group changes
            (int, int) sumsOld = (0, 0), sumsNew = (0, 0);

            //for every element of group, save old sum(s), change Number1 value, new sum(s) and redraw the cell
            for (int i = 0; i < AffectedCellGroup.Length; i++)
            {

                CellTest cell = w.Cells.OneDimensional[AffectedCellGroup[i]];

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

            }

            w.Sum += (sumsNew.Item1 - sumsOld.Item1);

            //w.ReachCellSum += (sumsNew.Item2 - sumsOld.Item2);

        }

    }

    public void DecreaserUnVisit(ref int playerPos, ref TreeNode node)
    {

        WorldTester w = World;

        if (w.CellGroups != null)
        {

            //increaser can only increase existing cell groups
            if (Data.AffectedCellGroup < 0 || Data.AffectedCellGroup >= w.CellGroups.Length)
                return;

            //get belonging cell group
            int[] AffectedCellGroup = w.CellGroups[Data.AffectedCellGroup];

            //for tracking how sum of group changes
            (int, int) sumsOld = (0, 0), sumsNew = (0, 0);

            //for every element of group, save old sum(s), change (revert) Number1 value, new sum(s) and redraw the cell
            for (int i = 0; i < AffectedCellGroup.Length; i++)
            {

                CellTest cell = w.Cells.OneDimensional[AffectedCellGroup[i]];

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

            }

            //revert sum changes

            w.Sum += (sumsNew.Item1 - sumsOld.Item1);

            //w.ReachCellSum += (sumsNew.Item2 - sumsOld.Item2);

        }

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

    //TODO: add reach sum - not needed for final project
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
        int positionInt;
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
            positionInt = Cells.getIndex(position);

            //Debug.Log("Pos: " + string.Join(", ", position) + " Cells state: " + Cells);

            //visit all the way back if this state (cell) belongs to other branch (clear tree for memory)
            while (previous != current.Parent)
            {
                position = Cells.getCoords(previous.Position);

                UpdateView();
                Debug.Log("Going back");
                yield return null;

                previous.Cell.UnVisit(ref positionInt, ref current);
                previous.ClearChildren();
                previous = previous.Parent;

                UpdateView();
                Debug.Log("Going back");
                yield return null;
            }

            //can't visit the root node state
            if (current.Parent != null)
                current.Cell.Visit(ref positionInt, ref current);

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

    public (List<int>, int) BuildTree(out double elapsed)
    {
        int startPosition = Data.CellDatas.getIndex(Data.PlayerStartPosition);
        int[] position = new int[2];
        int positionInt = startPosition;
        CalculateSums();
        int startSum = Sum;
        List<int> ret = new List<int>();
        System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        s.Start();

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
            positionInt = Cells.getIndex(position);

            //Debug.Log("Pos: " + string.Join(", ", position) + " Cells state: " + Cells);

            //visit all the way back if this state (cell) belongs to other branch (clear tree for memory)
            while (previous != current.Parent)
            {
                previous.Cell.UnVisit(ref positionInt, ref current);
                previous.ClearChildren();
                previous = previous.Parent;
            }

            //can't visit the root node state
            if (current.Parent != null)
                current.Cell.Visit(ref positionInt, ref current);

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

            Cells.getCoordsNonAlloc(positionInt, ref position);
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

        s.Stop();
        elapsed = s.Elapsed.TotalSeconds;
        ret = bestPath.path;
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
            Debug.Log("Elapsed: " + s.Elapsed.TotalSeconds);
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
            Debug.Log("Elapsed: " + s.Elapsed.TotalSeconds);
        }
        return (ret, startSum - bestPath.sum);
        bool CanVisit(int dim, int move)
        {
            return move >= 0 && move < Cells.Dimensions[dim];
        }
    }

    public (List<int>, int) BuildTreeSelective(out double elapsed)
    {
        int startPosition = Data.CellDatas.getIndex(Data.PlayerStartPosition);
        int[] position = new int[2];
        int positionInt = startPosition;
        CalculateSums();
        int startSum = Sum;
        List<int> ret = new List<int>();
        System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        s.Start();

        Tree = new TreeNode(Cells.OneDimensional[startPosition], null, startPosition);

        Stack<TreeNode> open = new Stack<TreeNode>();
        open.Push(Tree);

        bool done = false;
        TreeNode doneNode = Tree;

        TreeNode previous = null;

        (int sum, List<int> path) bestPath = (int.MaxValue, null);

        List<(int, TreeNode)> nds = new List<(int, TreeNode)>();

        while (open.Count > 0)
        {
            TreeNode current = open.Pop();
            Cells.getCoordsNonAlloc(current.Position, ref position);
            positionInt = Cells.getIndex(position);

            //Debug.Log("Pos: " + string.Join(", ", position) + " Cells state: " + Cells);

            //visit all the way back if this state (cell) belongs to other branch (clear tree for memory)
            while (previous != current.Parent)
            {
                previous.Cell.UnVisit(ref positionInt, ref current);
                previous.ClearChildren();
                previous = previous.Parent;
            }

            //can't visit the root node state
            if (current.Parent != null)
                current.Cell.Visit(ref positionInt, ref current);

            if (Sum < bestPath.sum)
            {
                bestPath.sum = Sum;
                bestPath.path = RetrackPath(current);
            }

            if (Sum == 0)
            {
                done = true;
                doneNode = current;
                break;
            }

            Cells.getCoordsNonAlloc(positionInt, ref position);

            nds.Clear();

            position[0]++;
            if (CanVisit(0, position[0]) && Cells[position].Data.Number1 > 0)
            {
                current.Right = new TreeNode(Cells[position], current, Cells.getIndex(position));
                int sd = current.Right.Cell.PeekVisit(ref positionInt, ref current.Right);
                nds.Add((sd, current.Right));
                //open.Push(current.Right);
            }
            position[0] -= 2;
            if (CanVisit(0, position[0]) && Cells[position].Data.Number1 > 0)
            {
                current.Left = new TreeNode(Cells[position], current, Cells.getIndex(position));
                int sd = current.Left.Cell.PeekVisit(ref positionInt, ref current.Left);
                nds.Add((sd, current.Left));
                //open.Push(current.Left);
            }
            position[0]++;

            position[1]++;
            if (CanVisit(1, position[1]) && Cells[position].Data.Number1 > 0)
            {
                current.Up = new TreeNode(Cells[position], current, Cells.getIndex(position));
                int sd = current.Up.Cell.PeekVisit(ref positionInt, ref current.Up);
                nds.Add((sd, current.Up));
                //open.Push(current.Up);
            }
            position[1] -= 2;
            if (CanVisit(1, position[1]) && Cells[position].Data.Number1 > 0)
            {
                current.Down = new TreeNode(Cells[position], current, Cells.getIndex(position));
                int sd = current.Down.Cell.PeekVisit(ref positionInt, ref current.Down);
                nds.Add((sd, current.Down));
                //open.Push(current.Down);
            }
            position[1]++;

            nds.Sort(((int, TreeNode) a, (int, TreeNode) b) => { return -(a.Item1 - b.Item1); });
            foreach (var n in nds) open.Push(n.Item2);

            previous = current;

        }

        s.Stop();
        elapsed = s.Elapsed.TotalSeconds;
        ret = bestPath.path;
        if (done)
        {
            Debug.Log("Zero sum found! Score: " + (bestPath.path.Count - 1));
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
            Debug.Log("Elapsed: " + s.Elapsed.TotalSeconds);
        }
        else
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
            Debug.Log("Elapsed: " + s.Elapsed.TotalSeconds);
        }
        return (ret, startSum - bestPath.sum);
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
