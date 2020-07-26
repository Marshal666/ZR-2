using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public CellData Data;

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
        switch (Data.Type)
        {
            case CellData.CellType.Default:
            case CellData.CellType.Start:
            case CellData.CellType.ReachCell:
            case CellData.CellType.Increaser:

                for (int i = 0; i < Data.Number1; i++)
                {
                    GameObject g = Instantiate(GameData.BuildingBlocks[Data.BuildingType]);
                    g.transform.SetParent(transform);
                    g.transform.localPosition = Vector3.up * i;
                }

                break;


            //Diffrences for teleports needed?
            case CellData.CellType.TeleportIn:

                for (int i = 0; i < Data.Number1; i++)
                {
                    GameObject g = Instantiate(GameData.BuildingBlocks[Data.BuildingType]);
                    g.transform.SetParent(transform);
                    g.transform.localPosition = Vector3.up * i;
                }

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

    public void IncraserIncrease()
    {
        if(World.main.CellGroups != null)
        {
            int[] AffectedCellGroup = World.main.CellGroups[Data.AffectedCellGroup];
            for(int i = 0; i < AffectedCellGroup.Length; i++)
            {
                World.main.Cells.OneDimensional[AffectedCellGroup[i]].Data.Number1 += Data.Number2;
                World.main.Cells.OneDimensional[AffectedCellGroup[i]].Redraw();
                if(Data.Number2 > 0)
                {
                    World.main.Sum += Data.Number2;
                } else if(Data.Number2 < 0)
                {
                    World.main.Sum += Data.Number2 - World.main.Cells.OneDimensional[AffectedCellGroup[i]].Data.Number1;
                }
            }
        }
    }

    //needs testing to see if it works
    public void IncreaserReverseIncrease()
    {
        if (World.main.CellGroups != null)
        {
            int[] AffectedCellGroup = World.main.CellGroups[Data.AffectedCellGroup];
            for (int i = 0; i < AffectedCellGroup.Length; i++)
            {
                World.main.Cells.OneDimensional[AffectedCellGroup[i]].Data.Number1 -= Data.Number2;
                World.main.Cells.OneDimensional[AffectedCellGroup[i]].Redraw();
                if (Data.Number2 > 0)
                {
                    World.main.Sum -= Data.Number2;
                }
                else if (Data.Number2 < 0)
                {
                    World.main.Sum -= Data.Number2 - World.main.Cells.OneDimensional[AffectedCellGroup[i]].Data.Number1;
                }
            }
        }
    }

}
