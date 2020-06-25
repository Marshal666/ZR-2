using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public enum CellType
    {
        Default,
        Start,
        TeleportIn,
        TeleportOut
    }

    public CellType Type = CellType.Default;

    public int Number1 = 0;

    public int Number2 = -1;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    public bool Parse(string s)
    {

        if (string.IsNullOrEmpty(s))
            return false;
        int n;
        if (int.TryParse(s, out n))
        {
            Type = CellType.Default;
            Number1 = n;
            return true;
        }
        else
        {
            switch (s[0])
            {
                case 'x':
                    Type = CellType.Default;
                    Number1 = 0;
                    break;
                case 's':
                    Type = CellType.Start;
                    break;
                default:
                    return false;
            }
            return true;
        }
    }

    public void Redraw()
    {
        
    }

    public override string ToString()
    {
        switch (Type)
        {
            case CellType.Default:
                return Number1.ToString();
            case CellType.Start:
                return "s";
            case CellType.TeleportIn:
                return "ti" + Number2 + "," + Number1;
            case CellType.TeleportOut:
                return "to" + Number2 + "," + Number1;
            default:
                return "err";
        }
    }

}
