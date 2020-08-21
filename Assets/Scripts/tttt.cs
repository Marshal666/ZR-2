using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using JetBrains.Annotations;

public class tttt : MonoBehaviour
{


    #region MARRAYNAV
    /*
    MArray<int> arr;
    MArray<GameObject> arro;
    int[] pos;
    */
    #endregion

    #region ROADGEN
    /*
    public int width = 2, height = 2;
    public float distance = 0.5f, depth = 0.1f;
    public int roadCount = 2;
    */
    #endregion

    #region EVENTSYSTEST
    /*
    class asd : IGameEvent
    {


        static int count = 0;

        int a;

        bool e = true;

        public asd()
        {
            a = count++;
        }

        public void Execute()
        {
            e = true;
        }

        public void Revert()
        {
            e = false;
        }

        public override string ToString()
        {
            return "e" + a.ToString() + (e ? "e" : "f");
        }
    }
    */
    #endregion

    #region SCENEVENTSYSTT
    /*
    public static string str = "";

    public class addA : IGameEvent
    {

        GameEventExecutionResult res = GameEventExecutionResult.Success;

        public addA()
        {
            Execute();
        }

        public GameEventExecutionResult result { get { return res; } }

        public void Execute()
        {
            str += "A";
        }

        public void Revert()
        {
            str = str.Remove(str.Length - 1, 1);
        }
    }

    public class addB : IGameEvent
    {

        GameEventExecutionResult res = GameEventExecutionResult.Success;

        public addB()
        {
            Execute();
        }

        public GameEventExecutionResult result { get { return res; } }

        public void Execute()
        {
            str += "B";
        }

        public void Revert()
        {
            str = str.Remove(str.Length - 1, 1);
        }
    }
    */
    #endregion

    #region EVENTTESTMTH
    /*
    void A ()
    {
        print("A");
    }

    void B()
    {
        print("B");
    }

    delegate void Del();

    Del meth;
    */
    #endregion

    private void Awake()
    {
        /*
        #region AWAKETEST
        print("aw");
        #endregion
        */
    }

    // Start is called before the first frame update
    void Start()
    {


        #region MARRAYTEST
        /*
        MArray<int> arr = new MArray<int>(4, 5, 3);
        for(int i = 0; i < arr.OneDimensional.Length; i++)
        {
            arr.OneDimensional[i] = i;
        }

        for(int i = 0; i < arr.Dimensions[2]; i++)
        {
            for(int j = 0; j < arr.Dimensions[1]; j++)
            {
                for (int k = 0; k < arr.Dimensions[0]; k++) {
                    print(string.Format("({0}, {1}, {2}) ", k, j, i) + arr[k, j, i]);
                }
            }
        }

        bool res = true;
        int ci = 0;

        for (int i = 0; i < arr.Dimensions[2]; i++)
        {
            for (int j = 0; j < arr.Dimensions[1]; j++)
            {
                for (int k = 0; k < arr.Dimensions[0]; k++)
                {
                    res &= arr[k, j, i] == arr.OneDimensional[ci];
                    ci++;
                }
            }
        }
        print(res);
        */

        #endregion

        #region MARRAYNAV
        /*
        arr = new MArray<int>(5, 4);
        arro = new MArray<GameObject>(5, 4);
        pos = new int[arr.Dimensions.Length];
        for(int i = 0; i < arr.OneDimensional.Length; i++)
        {
            arr.OneDimensional[i] = i;
            arro[i] = new GameObject("obj" + i);
            int[] crds = arro.getCoords(i);
            arro[i].transform.position = new Vector3(crds[0], crds[1]) * 2;
            arro[i].AddComponent<TextMesh>().text = i.ToString();
        }
        */
        #endregion

        #region 4DTEST
        /*
        MArray<float> arr = new MArray<float>(3, 4, 1, 2);

        StringBuilder sb = new StringBuilder();

        int index = 0;

        for(int i = 0; i < arr.GetLength(0); i++)
        {
            for(int j = 0; j < arr.GetLength(1); j++)
            {
                for (int k = 0; k < arr.GetLength(2); k++) {
                    for(int l = 0; l < arr.GetLength(3); l++)
                    {
                        arr[i, j, k, l] = index++;
                    }
                }
            }
        }

        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                for (int k = 0; k < arr.GetLength(2); k++)
                {
                    for (int l = 0; l < arr.GetLength(3); l++)
                    {
                        sb.Append(arr[i, j, k, l] + " ");
                    }
                    sb.Append("\n");
                }
                sb.Append("\nN\n");
            }
            sb.Append("\nM\n");
        }

        print(sb.ToString());

        sb.Clear();


        foreach(var v in arr)
        {
            sb.Append(v + " ");
        }

        print(sb.ToString());*/

        #endregion

        #region COORDSTEST
        /*
        MArray<int> arr = new MArray<int>(5, 4, 2);
        for(int i = 0; i < arr.Length; i++)
        {
            arr.OneDimensional[i] = i;
        }

        string s = "";
        for (int i = 0; i < arr.Length; i++)
        {
            s += i + " -> " + string.Join(",", arr.getCoords(i)) + "\n";
        }

        print(s);*/
        #endregion

        #region FIZZBUZZ
        /*
        StringBuilder sb = new StringBuilder();

        int[] nums = { 3, 5, 7 };
        string[] words = { "fizz", "buzz", "duzz" };

        for(int i = 1; i <= 105; i++)
        {
            bool set = false;
            for(int j = 0; j < nums.Length; j++)
            {
                if(i % nums[j] == 0)
                {
                    set = true;
                    sb.Append(words[j]);
                }
            }
            if(!set)
            {
                sb.Append(i);
            }

            sb.Append("\n");
        }
        print(sb.ToString());
        */
        #endregion

        #region CELLDATAT
        /*CellData dt = (0, 0, 0, 0);
        print(dt.ToString());*/
        #endregion

        #region EVENTTEST
        /*
        Stack<IEvent> se = new Stack<IEvent>();

        se.Push(new Player.PlayerMove());
        se.Peek().Execute();
        se.Push(new Player.PlayerMove());
        se.Peek().Execute();

        while (se.Count > 0)
        {
            se.Pop().Revert();
        }
        */
        #endregion

        #region EVENTSYSTEST
        /*
        EventSystem es = new EventSystem(8);

        string t = "aarrraarrrraauuauuuuuaurrraauuuurauu" + "aaaaauururuauraurauaruaruaaarruurrrrurruruuraarurrrrrruuuuaaaaaarrrrururuauauruaruaauuauauuauuruuauaaaaruurrrruuuruurrruuuuuuuuaauuuuuuuuuuuuuuaaaauuuururrruaauauruauruarururuaa";

        for(int i = 0; i < t.Length; i++)
        {
            if(t[i] == 'a')
            {
                es.AddEvent(new asd());
            }

            if(t[i] == 'u')
            {
                es.Undo();
            }
            
            if(t[i] == 'r')
            {
                es.Redo();
            }

            print(es.ToString());
        }

       */
        #endregion

        #region EVENTTESTMTH
        /*
        meth += A;
        meth += B;
        meth += B;

        meth.Invoke();
        */
        #endregion

        #region CLCHT
        /*
        GameObject g = GameObject.Find("GGG");
        print(g);
        Scene.ClearChildren(g.transform);
        */
        #endregion

    }

    private void Update()
    {

        #region MARRAYNAV
        /*
        if(Input.GetKeyDown(KeyCode.A))
        {
            Visit(0, -1);
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            Visit(0, 1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Visit(1, 1);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Visit(1, -1);
        }

        void Visit(int dim, int d)
        {
            int dt = pos[dim] + d;
            if (dt >= 0 && dt < arr.Dimensions[dim])
            {
                pos[dim] += d;

                //FIX: MArray.getIndex doesn't work well
                arr[pos]--;
                arro[pos].GetComponent<TextMesh>().text = arr[pos].ToString();
                print("Move: " + string.Join(", ", pos) + " i: " + arr.getIndex(pos));
            }
        }*/
        #endregion

        #region ROADGEN
        /*
        if (Input.GetKeyDown(KeyCode.G))
        {

            Scene.ClearRoot();
            //road dims = world_dims / 2 + (1, 1)
            RoadGenerator.MakeRoad(width, height, distance, depth, roadCount);
        }
        */
        #endregion

        #region SCENEVENTSYSTT
        /*
        if(Input.GetKeyDown(KeyCode.A))
        {
            Scene.EventSystem.AddEvent(new addA());
            print(str);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Scene.EventSystem.AddEvent(new addB());
            print(str);
        }*/
        #endregion

        #region AWAKETEST
        /*
        if(Input.GetKeyDown(KeyCode.I))
        {
            Instantiate(this);
        }
        */
        #endregion

    }

}
