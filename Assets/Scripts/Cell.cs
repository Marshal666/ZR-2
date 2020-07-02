using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public CellData data;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    /// <summary>
    /// Draws cell objects
    /// </summary>
    public void Draw()
    {
        switch (data.Type)
        {
            case CellData.CellType.Default:
                for (int i = 0; i < data.Number1; i++)
                {
                    GameObject g = Instantiate(GameData.BuildingBlocks[data.BuildingType]);
                    g.transform.SetParent(transform);
                    g.transform.localPosition = Vector3.up * i;
                }

                break;
            case CellData.CellType.Start:
                break;
            case CellData.CellType.TeleportIn:
                break;
            case CellData.CellType.TeleportOut:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Expects that cell was already drawn, redraws it according to current state
    /// </summary>
    public void Redraw()
    {
        if(transform.childCount > data.Number1)
        {
            int diff = transform.childCount - data.Number1;
            for(int j = transform.childCount - 1; j >= 0 && diff > 0; j--, diff--)
            {
                GameObject o = transform.GetChild(j).gameObject;
                o.SetActive(false);
            }
        }
    }

}
