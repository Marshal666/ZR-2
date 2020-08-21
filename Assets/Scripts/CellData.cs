using System.Text;
using System.Runtime.CompilerServices;
using System;

/// <summary>
/// Contains info about cell data
/// </summary>
[System.Serializable]
public struct CellData
{
    /// <summary>
    /// Types of cells used in type bitmask
    /// </summary>
    public enum CellType
    {
        Default = 1,
        Start = 2,
        TeleportIn = 4,
        ReachCell = 8,
        Increaser = 16
    }

    /// <summary>
    /// Bitmask of cell type(s)
    /// </summary>
    public CellType Type;

    /// <summary>
    /// Decides if cell can be visited
    /// If > 0 cell can be visited
    /// If == 0 cell cannot be visited
    /// Else cell can be visited all the time
    /// </summary>
    public int Number1;

    /// <summary>
    /// One dimensial cell target index used for teleporting
    /// </summary>
    public int Number2;

    /// <summary>
    /// The amount increaser cell increase/decrease cell groups
    /// </summary>
    public int Number3;

    /// <summary>
    /// Types of blocks used in cell drawing
    /// </summary>
    public int BuildingType;

    /// <summary>
    /// Cell group index used by increaser, defined in LevelData
    /// </summary>
    public int AffectedCellGroup;

    /// <summary>
    /// Creates a new CellData struct
    /// </summary>
    /// <param name="type"></param>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <param name="num3"></param>
    /// <param name="bdt"></param>
    /// <param name="affected"></param>
    public CellData(CellType type, int num1, int num2, int num3, int bdt, int affected)
    {
        Type = type;
        Number1 = num1;
        Number2 = num2;
        Number3 = num3;
        BuildingType = bdt;
        AffectedCellGroup = affected;
    }

    /// <summary>
    /// for ValueTuple -> CellData conversion
    /// </summary>
    /// <param name="d"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CellData((int, int, int, int, int, int) d)
    {
        return new CellData((CellType)d.Item1, d.Item2, d.Item3, d.Item4, d.Item5, d.Item6);
    }

    /// <summary>
    /// for ValueTuple -> CellData conversion
    /// </summary>
    /// <param name="d"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CellData((CellType, int, int, int, int, int) d)
    {
        return new CellData(d.Item1, d.Item2, d.Item3, d.Item4, d.Item5, d.Item6);
    }

    /// <summary>
    /// ToString() override
    /// </summary>
    /// <returns>CellData in string format</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(32);
        sb.Append('(');
        sb.Append(Convert.ToString((int)Type, 2));
        sb.Append(", ");
        sb.Append(Number1);
        sb.Append(", ");
        sb.Append(Number2);
        sb.Append(", ");
        sb.Append(BuildingType);
        sb.Append(", ");
        sb.Append(AffectedCellGroup);
        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>
    /// Parses string to CellData
    /// </summary>
    /// <param name="s">String for parsing</param>
    /// <returns>CellData based on string</returns>
    public static CellData Parse(string s)
    {
        CellData cd = new CellData();

        char[] delims = { '(', ',', ' ', '\t', '\r', '\n', ')', '{', '}', ';' };

        string[] data = s.Split(delims, System.StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 6)
            throw new System.Exception("Not enough data for parsing given");

        cd.Type = (CellType)System.Convert.ToInt32(data[0], 2);
        cd.Number1 = int.Parse(data[1]);
        cd.Number2 = int.Parse(data[2]);
        cd.Number3 = int.Parse(data[3]);
        cd.BuildingType = int.Parse(data[4]);
        cd.AffectedCellGroup = int.Parse(data[5]);

        return cd;
    }

}
