using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{

    public CellTest Cell;

    public TreeNode Up = null, Left = null, Right = null, Down = null;
    public TreeNode Parent = null;

    public TreeNode this[int i] {
        get {
             switch(i)
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

}
