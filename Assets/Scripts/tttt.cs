using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Diagnostics;

public class tttt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

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

    }

}
