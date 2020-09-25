using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

/// <summary>
/// Represents all data in level file
/// </summary>
[System.Serializable]
public class WorldData
{

    public enum GameTypes
    {
        SumToZero,
        ReachPoints,
        //.... add more?
    }

    public string LevelName;

    public GameTypes GameType = GameTypes.SumToZero;

    public int[] PlayerStartPosition;

    public int[][] CellGroups;

    public MArray<CellData> CellDatas;

    static List<int> ParseStrings(string[] s)
    {
        List<int> r = new List<int>(s.Length + 1);
        for (int i = 0; i < s.Length; i++)
        {
            r.Add(int.Parse(s[i]));
        }
        return r;
    }

    static readonly char[] separators = { ' ', '\t', '\n', '\r' };

    public bool Load(string file)
    {
        try
        {

            StreamReader r = new StreamReader(file);

            LevelName = r.ReadLine();

            //load game type
            string gameTypeString = r.ReadLine();

            GameType = (WorldData.GameTypes)Enum.Parse(typeof(WorldData.GameTypes), gameTypeString);

            //load player position
            string playerPosStr = r.ReadLine();
            char[] delimsp = { '(', ')', ' ', ',', '\t', '\r' };
            PlayerStartPosition = ParseStrings(playerPosStr.Split(delimsp, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            //load cell groups
            string gameGroupCountString = r.ReadLine();

            int gameGroupCount = int.Parse(gameGroupCountString);

            CellGroups = new int[gameGroupCount][];

            //parse cell groups
            char[] delims = { ',', ' ', '\t', '\r', '{', '}' };

            for (int i = 0; i < gameGroupCount; i++)
            {
                string listLine = r.ReadLine();
                string[] cellsIn = listLine.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                CellGroups[i] = new int[cellsIn.Length];
                for (int j = 0; j < CellGroups[i].Length; j++)
                {
                    CellGroups[i][j] = int.Parse(cellsIn[j]);
                }
            }

            //load level dimensions
            string[] infos = r.ReadLine().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            List<int> dims = ParseStrings(infos);
            int[] dimensions = dims.ToArray();

            CellDatas = new MArray<CellData>(dimensions);

            //print(string.Join(", ", dimensions));

            //adding 1s to dimensions list doesn't change array in any way
            dims.Add(1);

            //check if dimensions are > 0
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] <= 0)  //0 means that size of an array is 0
                {
                    return true;    //since array has 0 elements, work here is done
                }
            }

            string cellInfosTxt = r.ReadToEnd();

            //cell parsing
            //get all cell infos using regex for "(...) (...)..."
            Regex regx = new Regex(@"\(.+?\)", RegexOptions.Multiline);
            Match mt = regx.Match(cellInfosTxt);

            //load cell data
            for(int i = 0; i < CellDatas.Length; i++)
            {

                CellDatas.OneDimensional[i] = CellData.Parse(mt.Value);
                mt = mt.NextMatch();

                //Debug.Log(CellDatas.OneDimensional[i]);

            }

            r.Close();

        }
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
        {
            return false;
        }
        
        return true;
    }


    /// <summary>
    /// Used for corrupted levels
    /// </summary>
    static readonly string ErrorReadName = "File corrupted";

    /// <summary>
    /// Returns level name of a level in given file
    /// </summary>
    /// <param name="file">File which contains level data</param>
    /// <returns>Level name of given file</returns>
    public static string PeekLevelName(string file)
    {
        string rt;
        try
        {
            StreamReader r = new StreamReader(file);
            rt = r.ReadLine();
            r.Close();
        } catch (Exception)
        {
            return ErrorReadName;
        }
        return rt;
    }

    public override string ToString()
    {

        const char nl = '\n';

        StringBuilder sb = new StringBuilder();
        sb.Append(LevelName);
        sb.Append(nl);
        sb.Append(GameType.ToString());
        sb.Append(nl);
        sb.Append('(');
        if (PlayerStartPosition != null)
        {
            for(int i = 0; i < PlayerStartPosition.Length; i++)
            {
                sb.Append(PlayerStartPosition[i]);
                if (i + 1 < PlayerStartPosition.Length)
                    sb.Append(", ");
            }
        } else 
        {
            throw new NullReferenceException("PlayerStartPosition is null");
        }
        sb.Append(')');
        sb.Append(nl);
        if (CellGroups != null)
        {
            sb.Append(CellGroups.Length);
            sb.Append(nl);
            for (int i = 0; i < CellGroups.Length; i++)
            {
                if (CellGroups[i] != null)
                {
                    sb.Append('{');
                    sb.Append(string.Join(", ", CellGroups[i]));
                    sb.Append('}');
                    sb.Append(nl);
                }
            }
        } else
        {
            sb.Append('0');
            sb.Append(nl);
        }
        sb.Append(string.Join(" ", CellDatas.Dimensions));
        sb.Append(nl);
        sb.Append(string.Join(" ", CellDatas.OneDimensional));
        return sb.ToString();
    }

}
