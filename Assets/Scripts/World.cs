using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class World : MonoBehaviour
{

    [System.Serializable]
    public struct DimensionInputs
    {
        public KeyCode positive;
        public KeyCode negative;
    }

    public Transform rootObject;

    public MArray<Cell> Cells;

    public int[] dimensions;

    public DimensionInputs[] dimensionInputs;

    public GameObject CellPrefab;

    public Color defaultCellColor = Color.white, currentCellColor = Color.green;

    public int[] currentPosition;

    public int sum;

    public List<Transform> dimensionHolders = new List<Transform>();

    public void RenderPositionChanges()
    {

        Queue<(Transform, int)> q = new Queue<(Transform, int)>();

        (Transform parent, int dim) wo = (dimensionHolders[0], dimensions.Length - 1);


        //optimizable?, instead of enabling some and then disabling all objects, disable current active ones and just enable required ones

        q.Enqueue(wo);

        while(q.Count != 0)
        {
            (Transform parent, int dim) = q.Dequeue();
            if(dim > 1)
            {
                for(int i = 0; i < parent.childCount; i++)
                {
                    q.Enqueue((parent.GetChild(i), dim - 1));
                    if(currentPosition[dim] == i)
                    {
                        parent.GetChild(i).gameObject.SetActive(true);
                    } else
                    {
                        parent.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }


    }

    bool MakeMove(int dim, int direction)
    {

        //print("Move: " + string.Join(", ", direction));

        currentPosition[dim] += direction;

        RenderPositionChanges();

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        //print(Directory.GetCurrentDirectory());
        print(LoadCells("celltest.txt"));
        
    }

    // Update is called once per frame
    void Update()
    {

        //check for moves between dimensions

        for(int i = 0; i < dimensionInputs.Length; i++)
        {
            if (Input.GetKeyDown(dimensionInputs[i].positive))
            {
                MakeMove(i, 1);
            }
            else if (Input.GetKeyDown(dimensionInputs[i].negative))
            {
                MakeMove(i, -1);
            }

        }

    }

    static readonly char[] separators = { ' ', '\t', '\n', '\r' };

    static List<int> parseStrings(string[] s)
    {
        List<int> r = new List<int>(s.Length + 1);
        for(int i = 0; i < s.Length; i++)
        {
            r.Add(int.Parse(s[i]));
        }
        return r;
    }

    public bool LoadCells(string file)
    {
        try
        {

            StreamReader r = new StreamReader(file);

            string[] infos = r.ReadLine().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            List<int> dims = parseStrings(infos);
            dimensions = dims.ToArray();

            //adding 1s to dimensions list doesn't change array in any way
            dims.Add(1);

            //check if dimensions are > 0
            for(int i = 0; i < dimensions.Length; i++)
            {
                if(dimensions[i] <= 0)  //0 means that size of an array is 0
                {
                    return true;    //since array has 0 elements, work here is done
                }
            }

            Cells = new MArray<Cell>(dimensions);

            int[] current = new int[dimensions.Length];

            currentPosition = new int[dimensions.Length];

            string[] cellInfos = r.ReadToEnd().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //dimension holders array and starting object
            dimensionHolders.Clear();
            dimensionHolders.Add(new GameObject("World Objects").transform);
            dimensionHolders[dimensionHolders.Count - 1].SetParent(rootObject);

            //create dimension holder for every dimension > 1, non recursive
            Queue<(int dimindex, Transform parent)> qq = new Queue<(int, Transform)>(64);

            //start with highest dimension
            qq.Enqueue((dimensions.Length - 1, dimensionHolders[0]));

            //naming index vector
            int[] dinx = new int[dimensions.Length];

            //index of current cell for reading cell info from file
            int ci = 0;

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
                } else      //a single cell is 0D object
                {

                    Cells.getCoordsNonAlloc(ci, ref current);
                    parent.transform.localPosition = -Vector3.up * current[1];

                    //create all cells for following 1st dimension
                    for (int j = 0; j < dimensions[0]; j++)
                    {

                        Cells.getCoordsNonAlloc(ci, ref current);
                        
                        GameObject cell = Instantiate(CellPrefab);
                        cell.name = "Cell" + (dinx[dimindex]++);

                        Transform cellTransform = cell.transform;

                        cellTransform.SetParent(parent);

                        //TODO make cell objects the better way
                        Cells.OneDimensional[ci] = cell.AddComponent<Cell>();
                        Cells.OneDimensional[ci].Parse(cellInfos[ci], cell);

                        if (Cells.OneDimensional[ci].Type == Cell.CellType.Start)
                            currentPosition = Cells.getCoords(ci);
                        Cells.OneDimensional[ci].TextColor = defaultCellColor;
                        Cells.OneDimensional[ci].Redraw();

                        //TODO better positioning for cells and create world with cell objects
                        Cells.OneDimensional[ci].transform.localPosition = new Vector3(current[0], 0);

                        ci++;

                    }
                }
            }

            //TODO world creation based on cell array?


        } catch (Exception e)
        {

            #if UNITY_EDITOR
            print(e.Message);
            #endif

            return false;   //fail in loading cells
        }

        return true;    //all job was done

    }

}
