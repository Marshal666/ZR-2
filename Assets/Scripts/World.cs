using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class World : MonoBehaviour
{

    public enum GameTypes
    {
        SumToZero,
        ReachPoints,
        //.... add more?
    }

    public MArray<Cell> Cells;

    public GameTypes GameType = GameTypes.SumToZero;

    public GameObject CellPrefab;

    public List<Transform> dimensionHolders = new List<Transform>();

    public float buildingDistance = 2.5f;

    public int Sum = 0;

    GameObject player;

    public static World main;

    public void RenderPositionChanges()
    {

        if (dimensionHolders.Count <= 0)
            return;

        Queue<(Transform, int)> q = new Queue<(Transform, int)>();

        (Transform parent, int dim) wo = (dimensionHolders[0], Cells.Dimensions.Length - 1);


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
                    if(Scene.Player.currentPosition[dim] == i)
                    {
                        parent.GetChild(i).gameObject.SetActive(true);
                    } else
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

    // Start is called before the first frame update
    void Start()
    {

        player = Scene.PlayerObject;

        //print(Directory.GetCurrentDirectory());
        print(LoadLevel("celltest.txt"));
        
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

    public bool LoadLevel(string file)
    {
        try
        {

            StreamReader r = new StreamReader(file);

            string[] infos = r.ReadLine().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            List<int> dims = parseStrings(infos);
            int[] dimensions = dims.ToArray();

#if UNITY_EDITOR
            print(string.Join(", ", dimensions));
#endif

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

            Scene.Player.currentPosition = new int[dimensions.Length];

            string cellInfosTxt = r.ReadToEnd();

            //get all cell infos using regex for "(...) (...)..."
            Regex regx = new Regex(@"\(.+?\)", RegexOptions.Multiline);
            Match mt = regx.Match(cellInfosTxt);

            //dimension holders array and starting object
            dimensionHolders.Clear();
            dimensionHolders.Add(new GameObject("World Objects").transform);
            dimensionHolders[dimensionHolders.Count - 1].SetParent(Scene.RootTransform);

            //create dimension holder for every dimension > 1, non recursive
            Queue<(int dimindex, Transform parent)> qq = new Queue<(int, Transform)>(64);

            //start with highest dimension
            qq.Enqueue((dimensions.Length - 1, dimensionHolders[0]));

            //naming index vector
            int[] dinx = new int[dimensions.Length];

            //index of current cell for reading cell info from file
            int ci = 0;

            Sum = 0;

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

                    if (current.Length > 1)
                        parent.transform.localPosition = Vector3.forward * current[1] * buildingDistance;

                    //create all cells for following 1st dimension
                    for (int j = 0; j < dimensions[0]; j++)
                    {

                        //parse cell data
                        CellData data = CellData.Parse(mt.Value);
                        mt = mt.NextMatch();

                        Cells.getCoordsNonAlloc(ci, ref current);
                        
                        GameObject cell = Instantiate(CellPrefab);
                        cell.name = "Cell" + (dinx[dimindex]++);
                        

                        Transform cellTransform = cell.transform;

                        cellTransform.SetParent(parent);
                        cell.transform.localPosition = Vector3.right * j * buildingDistance;

                        Cells.OneDimensional[ci] = cell.AddComponent<Cell>();
                        Cells.OneDimensional[ci].Data = data;
                        Cells.OneDimensional[ci].Draw();

                        Sum += Cells.OneDimensional[ci].Data.Number1;

                        if (Cells.OneDimensional[ci].Data.Type == CellData.CellType.Start)
                            Scene.Player.currentPosition = Cells.getCoords(ci);

                        ci++;

                    }
                }
            }

            //hide other worlds (if any)
            RenderPositionChanges();

            //create world around cells
            CreateWorld();


        } catch (Exception e)
        {

            #if UNITY_EDITOR
            print(e.Message);
            #endif

            return false;   //fail in loading cells
        }

        PositionPlayer();

        return true;    //all job was done

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

}
