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
    /// Is Cell currently under a preview?
    /// </summary>
    public bool BeingPreviewed = false;

    /// <summary>
    /// Reference to an instantiated negative object for this cell, used when Number1 < 0
    /// </summary>
    //public GameObject Negative;

    /// <summary>
    /// Reference to an instantiated ruin object for this cell, used when Number1 == 0
    /// </summary>
    public GameObject Zero;

    /// <summary>
    /// Refrence to an instantianted empty child that holds building blocks, used when Number1 > 0
    /// </summary>
    public GameObject Positives;

    /// <summary>
    /// 1D index of this cell in Cells MArray object
    /// </summary>
    public int Index;

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

            PreviewChanges += TeleportPreview;
            RemovePreviewChanges += TeleportRemovePreview;
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

            PreviewChanges += IncreaserPreview;
            RemovePreviewChanges += IncreaserRemovePreview;
        }

        PreviewChanges += BasePreview;
        RemovePreviewChanges += BaseRemovePreview;

        if (!Zero)
        {
            Zero = Instantiate(GameData.BuildingBlocks[Data.BuildingType].Zero);
            Zero.transform.position = transform.position;
            Zero.transform.SetParent(transform);
            Zero.SetActive(false);
        }
        if (!Positives)
        {
            Positives = new GameObject("Positives");
            Positives.transform.position = transform.position;
            Positives.transform.SetParent(transform);
            Positives.SetActive(false);
        }

    }

    /// <summary>
    /// Draws cell objects, should be called only once after Init()
    /// </summary>
    public void Draw()
    {
        if (Data.Number1 > 0)
        {
            Positives.SetActive(true);
            for (int i = 0; i < Data.Number1; i++)
            {
                GameObject g = Instantiate(GameData.BuildingBlocks[Data.BuildingType].Positive);
                g.transform.SetParent(Positives.transform);
                g.transform.localPosition = Vector3.up * i;
            }
        } else
        {
            Zero.SetActive(true);
        }
    }

    /// <summary>
    /// Expects that cell was already drawn, redraws it according to current state
    /// </summary>
    public void Redraw()
    {
        Zero.SetActive(false);
        Positives.SetActive(false);
        if (Data.Number1 > 0)
        {
            Positives.SetActive(true);
            for (int i = 0; i < Mathf.Max(Data.Number1, Positives.transform.childCount); i++)
            {
                GameObject o;
                if (i < Positives.transform.childCount)
                {
                    o = Positives.transform.GetChild(i).gameObject;
                }
                else
                {
                    o = Instantiate(GameData.BuildingBlocks[Data.BuildingType].Positive);
                    o.transform.SetParent(Positives.transform);
                    o.transform.localPosition = Vector3.up * i;
                }
                o.SetActive(i < Data.Number1);
            }
        } else
        {
            Zero.SetActive(true);
        }

    }

    public void DrawPreview(int newNumber1)
    {
        Positives.SetActive(false);
        Zero.SetActive(false);
        if (newNumber1 > 0)
        {
            Positives.SetActive(true);
            for (int i = 0; i < Mathf.Max(newNumber1, Data.Number1, Positives.transform.childCount); i++)
            {
                GameObject o;
                if (i < Positives.transform.childCount)
                {
                    o = Positives.transform.GetChild(i).gameObject;
                }
                else
                {
                    o = Instantiate(GameData.BuildingBlocks[Data.BuildingType].Positive);
                    o.transform.SetParent(Positives.transform);
                    o.transform.localPosition = Vector3.up * i;
                }
                o.SetActive(i < newNumber1 || i < Data.Number1);

                void SetSemiTransparentColor()
                {
                    MeshRenderer mr = o.GetComponent<MeshRenderer>();
                    if (mr.material)
                    {
                        Color c = mr.material.color;
                        c.a = GameData.SemiTransparentCellColor;
                        mr.material.color = c;
                    }
                }

                //print("nn1: " + newNumber1 + " n1: " + Data.Number1);

                if (newNumber1 < Data.Number1)
                {
                    if (i > newNumber1 - 1)
                    {
                        SetSemiTransparentColor();
                        //print("stc");
                    }
                }
                else if (newNumber1 > Data.Number1)
                {
                    if (i >= Data.Number1 - 1)
                    {
                        SetSemiTransparentColor();
                        //print("stc");
                    }
                }


            }
        } else
        {
            Zero.SetActive(true);
        }
    }

    public void UnDrawPreview()
    {

        Zero.SetActive(false);
        Positives.SetActive(false);
        if (Data.Number1 > 0)
        {
            Positives.SetActive(true);
            for (int i = 0; i < Mathf.Max(Data.Number1, Positives.transform.childCount); i++)
            {
                GameObject o;
                if (i < Positives.transform.childCount)
                {
                    o = Positives.transform.GetChild(i).gameObject;
                }
                else
                {
                    o = Instantiate(GameData.BuildingBlocks[Data.BuildingType].Positive);
                    o.transform.SetParent(Positives.transform);
                    o.transform.localPosition = Vector3.up * i;
                }
                o.SetActive(i < Data.Number1);

                

                SetNormalTransparency(o);

            }
        }
        else
        {
            Zero.SetActive(true);
        }

        for(int i = 0; i < Positives.transform.childCount; i++)
        {
            SetNormalTransparency(Positives.transform.GetChild(i).gameObject);
        }

        void SetNormalTransparency(GameObject o)
        {
            MeshRenderer mr = o.GetComponent<MeshRenderer>();
            if (mr.material)
            {
                Color c = mr.material.color;
                c.a = GameData.DefaultCellAlphaColor;
                mr.material.color = c;
            }
        }

        /*
        for (int i = 0; i < Mathf.Max(Data.Number1, transform.childCount); i++)
        {
            GameObject o;
            if (i < transform.childCount)
            {
                o = transform.GetChild(i).gameObject;
            }
            else
            {
                o = Instantiate(GameData.BuildingBlocks[Data.BuildingType]);
                o.transform.SetParent(transform);
                o.transform.localPosition = Vector3.up * i;
            }
            o.SetActive(i < Data.Number1);

            void SetNormalTransparency()
            {
                MeshRenderer mr = o.GetComponent<MeshRenderer>();
                if (mr.material)
                {
                    Color c = mr.material.color;
                    c.a = GameData.DefaultCellAlphaColor;
                    mr.material.color = c;
                }
            }

            SetNormalTransparency();
        }*/

    }

    /// <summary>
    /// Places given object above the cell
    /// </summary>
    /// <param name="o"></param>
    public void DrawAbove(GameObject o)
    {

        Vector3 additionalHeight = Vector3.zero;
        if (Scene.Player.CurrentPosition != null && Index == World.main.Cells.getIndex(Scene.Player.CurrentPosition))
            additionalHeight += Vector3.up;

        if (o)
        {
            if(Positives.transform.childCount == 0)
            {
                o.transform.position = Positives.transform.position + Vector3.up + additionalHeight;
            }
            for(int i = Positives.transform.childCount - 1; i >= 0; i--)
            {
                Transform ch = Positives.transform.GetChild(i);
                if(ch.gameObject.activeSelf)
                {
                    //Add height factor maybe?
                    o.transform.position = ch.position + Vector3.up + additionalHeight;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// For Visit methods
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    public delegate void VisitDelegate(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null);

    /// <summary>
    /// For preview methods
    /// </summary>
    public delegate void PreviewDelegate();

    /// <summary>
    /// Visit methods
    /// </summary>
    public VisitDelegate Visit;

    /// <summary>
    /// For UnVisit methods
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    public delegate void UnVisitDelegate(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null);

    /// <summary>
    /// UnVisit methods
    /// </summary>
    public UnVisitDelegate UnVisit;

    /// <summary>
    /// Called for previewing changes before visiting this cell
    /// </summary>
    public PreviewDelegate PreviewChanges;

    /// <summary>
    /// Called for removal of previewing changes before visiting this cell
    /// </summary>
    public PreviewDelegate RemovePreviewChanges;

    #region VISIT_UNVISITMETHODS

    /// <summary>
    /// Preforms basic cell visit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void BaseVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {
        if (w == null)
            w = World.main;

        moveInfo.ExCells.Add(w.Cells.getIndex(player.CurrentPosition));
    }

    /// <summary>
    /// Reverts BaseVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void BaseUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {
        //nothing here actually
    }

    /// <summary>
    /// Preforms default visit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void DefaultVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {

        if (w == null)
            w = World.main;

        Data.Number1--;
        Redraw();

        if (Data.Number1 >= 0)
            w.Sum--;

        //print("dv");

    }

    /// <summary>
    /// Reverts DefaultVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void DefaultUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {

        if (w == null)
            w = World.main;

        Data.Number1++;
        Redraw();

        if (Data.Number1 > 0)
            w.Sum++;

        //print("duv");

    }

    /// <summary>
    /// Preforms a teleport visit & teleportation to other (teleport) cell(s)
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void TeleportInVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {

        if (w == null)
            w = World.main;

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

            Cell cell = w.Cells.OneDimensional[pos];

            w.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

            //visit pointed cell but not the teleport method(if any)
            (cell.Visit - cell.TeleportInVisit).Invoke(player, ref moveInfo, w);

            //visit teleport chain, condition for teleporting is that Number1 must be >= 0 to avoid infinite loops
            while ((cell.Data.Type & CellData.CellType.TeleportIn) == CellData.CellType.TeleportIn && cell.Data.Number1 >= 0)
            {

                //visit cell pointed by other teleport
                pos = cell.Data.Number2;

                cell = w.Cells.OneDimensional[pos];

                w.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

                (cell.Visit - cell.TeleportInVisit).Invoke(player, ref moveInfo, w);

            }

            //position player to last visited cll
            w.Cells.getCoordsNonAlloc(pos, ref player.CurrentPosition);

        }

    }

    /// <summary>
    /// Does nothing as PlayerMove.Revert unvisits all necessary cells
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void TeleportInUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {
        //anything required here? -seems not since ExCells in info has all required data
    }

    /// <summary>
    /// Reach cell visit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void ReachCellVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {

        if (w == null)
            w = World.main;

        Data.Number1--;
        Redraw();

        if (Data.Number1 >= 0)
           w.ReachCellSum--;
    }

    /// <summary>
    /// Reverts ReachCellVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void ReachCellUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {

        if (w == null)
            w = World.main;

        Data.Number1++;
        Redraw();

        if (Data.Number1 >= 0)
            w.ReachCellSum++;
    }

    /// <summary>
    /// Runs Increasers job
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void IncreaserVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {
        IncraserIncrease(w);
    }

    /// <summary>
    /// Reverts IncreaserVisit
    /// </summary>
    /// <param name="player">Player visiting the cell</param>
    /// <param name="moveInfo">Events moveInfo</param>
    void IncreaserUnVisit(Player player, ref Player.PlayerMoveInfo moveInfo, World w = null)
    {
        IncreaserReverseIncrease(w);
    }

    #endregion

    /// <summary>
    /// Makes increaser do its job and keeping world sums valid
    /// </summary>
    public void IncraserIncrease(World w)
    {

        if (w == null)
            w = World.main;

        if (w.CellGroups != null)
        {

            //increaser can only increase existing cell groups
            if (Data.AffectedCellGroup < 0 || Data.AffectedCellGroup >= w.CellGroups.Length)
                return;

            //get belonging cell group
            int[] AffectedCellGroup = w.CellGroups[Data.AffectedCellGroup];

            //for tracking how sum of group changes
            (int , int) sumsOld = (0, 0), sumsNew = (0, 0);

            //for every element of group, save old sum(s), change Number1 value, new sum(s) and redraw the cell
            for(int i = 0; i < AffectedCellGroup.Length; i++)
            {

                Cell cell = w.Cells.OneDimensional[AffectedCellGroup[i]];

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

            w.Sum += (sumsNew.Item1 - sumsOld.Item1);

            w.ReachCellSum += (sumsNew.Item2 - sumsOld.Item2);

        }
    }

    /// <summary>
    /// Reverts increasers job while keeping world sums valid
    /// </summary>
    public void IncreaserReverseIncrease(World w)
    {

        if (w == null)
            w = World.main;

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

                Cell cell = w.Cells.OneDimensional[AffectedCellGroup[i]];

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

            w.Sum += (sumsNew.Item1 - sumsOld.Item1);

            w.ReachCellSum += (sumsNew.Item2 - sumsOld.Item2);

        }
    }

    void BasePreview()
    {
        if(!BeingPreviewed)
        {
            BeingPreviewed = true;
        }
    }

    void BaseRemovePreview()
    {
        if(BeingPreviewed)
        {
            BeingPreviewed = false;
        }
    }

    void IncreaserPreview()
    {
        if(!BeingPreviewed)
        {
            if (Data.AffectedCellGroup < 0 || Data.AffectedCellGroup >= World.main.CellGroups.Length)
                return;

            int[] cells = World.main.CellGroups[Data.AffectedCellGroup];
            for(int i = 0; i < cells.Length; i++)
            {
                World.main.Cells.OneDimensional[cells[i]].DrawPreview(World.main.Cells[cells[i]].Data.Number1 + Data.Number3);
            }
        }
    }

    void IncreaserRemovePreview()
    {
        if(BeingPreviewed)
        {
            if (Data.AffectedCellGroup < 0 || Data.AffectedCellGroup >= World.main.CellGroups.Length)
                return;

            int[] cells = World.main.CellGroups[Data.AffectedCellGroup];
            for (int i = 0; i < cells.Length; i++)
            {
                World.main.Cells.OneDimensional[cells[i]].UnDrawPreview();
            }
        }
    }

    GameObject arrowPointer = null;

    void TeleportPreview()
    {
        if (!BeingPreviewed)
        {
            if (!arrowPointer)
            {
                arrowPointer = Instantiate(GameData.ArrowObject);
                arrowPointer.transform.SetParent(Scene.RootTransform);
            }
            else
            {
                arrowPointer.SetActive(true);
            }
            Cell t = World.main.Cells.OneDimensional[Data.Number2];
            t.DrawAbove(arrowPointer);
            if (t != this)
            {
                //recursive way..
                t.PreviewChanges?.Invoke();
            }
        }
    }

    void TeleportRemovePreview()
    {
        if (BeingPreviewed)
        {
            if (arrowPointer)
            {
                arrowPointer.SetActive(false);
            }
            Cell t = World.main.Cells.OneDimensional[Data.Number2];
            t.DrawAbove(arrowPointer);
            if (t != this)
            {
                //recursive way..
                t.RemovePreviewChanges?.Invoke();
            }
        }
    }

}
