using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class World : MonoBehaviour, IWorldRenderer
{

    public WorldData Data;

    public MArray<Cell> Cells;

    public int[][] CellGroups { get { return Data.CellGroups; } set { Data.CellGroups = value; } }

    public WorldData.GameTypes GameType { get { return Data.GameType; } set { Data.GameType = value; } }

    public GameObject CellPrefab;

    public List<Transform> dimensionHolders = new List<Transform>();

    public float buildingDistance = 2.5f;

    public float BuildingDistance { get { return buildingDistance; } set { buildingDistance = value; } }

    int sum = 0;
    public int Sum {
        get => sum;
        set
        {
            sum = value;
            OnSumChange?.Invoke(sum);
        }
    }

    public Action<int> OnSumChange;

    public int ReachCellSum = 0;

    public static World main;

    public Vector3 WorldCenter { get { return new Vector3(Cells.Dimensions[0] - 1, 0f, (Cells.Dimensions.Length > 1 ? Cells.Dimensions[1] - 1 : 0f)) * buildingDistance / 2f; } }

    public Vector3 WorldMinPoint { get { if (Cells.Dimensions.Length > 1)
                return WorldCenter + Vector3.back * buildingDistance * Cells.Dimensions[0] + Vector3.left * buildingDistance * Cells.Dimensions[0];
            else if (Cells.Dimensions.Length == 1)
                return WorldCenter + Vector3.left * buildingDistance * Cells.Dimensions[0];
            else return Vector3.zero; } }

    public Vector3 WorldMaxPoint
    {
        get
        {
            if (Cells.Dimensions.Length > 1)
                return WorldCenter + Vector3.forward * buildingDistance / 2f * Cells.Dimensions[0] + Vector3.right * buildingDistance / 2f * Cells.Dimensions[0];
            else if (Cells.Dimensions.Length == 1)
                return WorldCenter + Vector3.right * buildingDistance / 2f * Cells.Dimensions[0];
            else return Vector3.zero;
        }
    }

    public void RenderPositionChanges()
    {

        if (dimensionHolders.Count <= 0)
            return;

        Queue<(Transform, int)> q = new Queue<(Transform, int)>();

        (Transform parent, int dim) wo = (dimensionHolders[0], Cells.Dimensions.Length - 1);


        //optimizable?, instead of enabling some and then disabling all objects, disable current active ones and just enable required ones

        q.Enqueue(wo);

        while (q.Count != 0)
        {
            (Transform parent, int dim) = q.Dequeue();
            if (dim > 1)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    q.Enqueue((parent.GetChild(i), dim - 1));
                    if (Scene.Player.CurrentPosition[dim] == i)
                    {
                        parent.GetChild(i).gameObject.SetActive(true);
                    }
                    else
                    {
                        parent.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }

        PositionPlayer();

    }



    private void Awake()
    {
        main = this;
    }

    public void AssembleLevel(WorldData data, GameObject holder)
    {
        int[] dimensions = data.CellDatas.Dimensions;

        Cells = new MArray<Cell>(dimensions);

        int[] current = new int[dimensions.Length];

        Scene.Player.CurrentPosition = new int[dimensions.Length];

        dimensionHolders.Clear();
        dimensionHolders.Add(new GameObject("World Objects").transform);
        dimensionHolders[dimensionHolders.Count - 1].SetParent(holder.transform);

        //create dimension holder for every dimension > 1, non recursive
        Queue<(int dimindex, Transform parent)> qq = new Queue<(int, Transform)>(64);

        //start with highest dimension
        qq.Enqueue((dimensions.Length - 1, dimensionHolders[0]));

        //naming index vector
        int[] dinx = new int[dimensions.Length];

        //index of current cell for reading cell info from file
        int ci = 0;

        Sum = data.TargetSum;
        ReachCellSum = 0;

        //place cells in their dimension objects
        bool sz = Sum == 0;
        while (qq.Count != 0)
        {
            (int dimindex, Transform parent) = qq.Dequeue();
            if (dimindex >= 1)
            {
                for (int i = 0; i < dimensions[dimindex]; i++)
                {
                    Transform t = new GameObject("Dimension" + (dimindex) + "_" + (dinx[dimindex]++)).transform;
                    dimensionHolders.Add(t);
                    t.SetParent(parent);
                    qq.Enqueue((dimindex - 1, t));
                }
            }
            else      //a single cell is 0D object
            {

                Cells.getCoordsNonAlloc(ci, ref current);

                if (current.Length > 1)
                    parent.transform.localPosition = Vector3.forward * current[1] * buildingDistance;

                //create all cells for following 1st dimension
                for (int j = 0; j < dimensions[0]; j++)
                {

                    Cells.getCoordsNonAlloc(ci, ref current);

                    GameObject cell = Instantiate(CellPrefab);
                    cell.name = "Cell" + (dinx[dimindex]++);


                    Transform cellTransform = cell.transform;

                    cellTransform.SetParent(parent);
                    cell.transform.localPosition = Vector3.right * j * buildingDistance;

                    Cells.OneDimensional[ci] = cell.AddComponent<Cell>();
                    Cells.OneDimensional[ci].Index = ci;
                    Cells.OneDimensional[ci].Data = data.CellDatas.OneDimensional[ci];
                    Cells.OneDimensional[ci].Init();
                    Cells.OneDimensional[ci].Draw();
                    

                    switch (data.GameType)
                    {
                        case WorldData.GameTypes.SumToZero:
                            if (Cells.OneDimensional[ci].Data.Number1 > 0 &&
                                (Cells.OneDimensional[ci].Data.Type & CellData.CellType.ReachCell) != CellData.CellType.ReachCell &&
                                (Cells.OneDimensional[ci].Data.Type & CellData.CellType.Default) == CellData.CellType.Default && sz)
                                Sum += Cells.OneDimensional[ci].Data.Number1;
                            break;
                        case WorldData.GameTypes.ReachPoints:
                            if (Cells.OneDimensional[ci].Data.Type == CellData.CellType.ReachCell)
                                ReachCellSum += Cells.OneDimensional[ci].Data.Number1;
                            break;
                        default:
                            break;
                    }

                    //if ((Cells.OneDimensional[ci].Data.Type & CellData.CellType.Start) == CellData.CellType.Start)
                    //    Scene.Player.CurrentPosition = Cells.getCoords(ci);

                    ci++;

                }
            }
        }

        //refresh sum label
        Sum = Sum;

        //position player
        Scene.Player.CurrentPosition = new int[data.PlayerStartPosition.Length];
        Array.Copy(data.PlayerStartPosition, Scene.Player.CurrentPosition, data.PlayerStartPosition.Length);

    }

    public bool LoadLevel(string file)
    {

        bool r = true;

        Data = new WorldData();

        r &= Data.Load(file);

        if (!r)
            return r;

        AssembleLevel(Data, Scene.Root);

        Scene.Player.WorldIn = Cells;
        Scene.Player.WorldRenderer = this;

        //hide other worlds (if any)
        RenderPositionChanges();

        //create world around cells
        CreateWorld();

        PositionPlayer();

        

        PlayerCamera.main.lookDirectionIndex = 0;

        return r;

    }

    public void PositionPlayer()
    {
        Scene.Player.Reposition();
    }

    public void CreateWorld()
    {
        if (dimensionHolders.Count <= 0)
            return;

        //TODO: generate road and terrain around buildings

        //RoadGenerator.MakeRoad(Cells.Dimensions[0], Cells.Dimensions.Length > 1 ? Cells.Dimensions[1] : 1, buildingDistance);
    }

    /// <summary>
    /// Clears everything related to this object
    /// </summary>
    public void ResetToDefault()
    {
        Data = null;
        Cells = null;
        dimensionHolders.Clear();
        Sum = ReachCellSum = 0;
        Scene.ClearRoot();
    }

}
