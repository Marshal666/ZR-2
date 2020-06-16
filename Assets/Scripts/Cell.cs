using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
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

    public int Number = 0;

    public int TeleportID = -1;

    TextMesh tm;

    private void Awake()
    {
        tm = GetComponent<TextMesh>();
    }

    private void Start()
    {
        tm = GetComponent<TextMesh>();
    }

    public string Text { get { return tm.text; } set { tm.text = value; } }

    public Color TextColor { get { return tm.color; } set { tm.color = value; } }

    public bool Parse(string s, GameObject o = null)
    {

        if (string.IsNullOrEmpty(s))
            return false;
        int n;
        if (int.TryParse(s, out n))
        {
            Type = CellType.Default;
            Number = n;
            return true;
        }
        else
        {
            switch (s[0])
            {
                case 'x':
                    //return new Cell(CellType.Default, 0);
                    Type = CellType.Default;
                    Number = 0;
                    break;
                case 's':
                    //return new Cell(CellType.Start);
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
        Text = ToString();
    }

    public override string ToString()
    {
        switch (Type)
        {
            case CellType.Default:
                return Number.ToString();
            case CellType.Start:
                return "s";
            case CellType.TeleportIn:
                return "ti" + TeleportID + "," + Number;
            case CellType.TeleportOut:
                return "to" + TeleportID + "," + Number;
            default:
                return "err";
        }
    }

}
