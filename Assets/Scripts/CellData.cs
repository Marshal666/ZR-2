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
        //Start = 2,
        TeleportIn = 2,
        ReachCell = 4,
        Increaser = 8
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
        if ((int)type == 0)
            type = CellType.Default;
        Type = type;
        if (num1 < 0)
            num1 = 0;
        Number1 = num1;
        Number2 = num2;
        if (num3 > 0)
            num3 = 0;
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
        sb.Append(Number3);
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

        char[] delims = { '(', ',', ' ', '\t', '\r', '\n', ')', '{', '}', ';' };

        string[] data = s.Split(delims, System.StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 6)
            throw new System.Exception("Not enough data for parsing given");

        /*cd.Type =;
        cd.Number1 = ;
        cd.Number2 = ;
        cd.Number3 = ;
        cd.BuildingType = );
        cd.AffectedCellGroup = ;*/

        CellData cd = new CellData((CellType)Convert.ToInt32(data[0], 2), int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]));
        return cd;
    }

    public override bool Equals(object obj)
    {
        return this == (CellData)obj;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(CellData a, CellData b) {
        if (a.Number1 != b.Number1)
            return false;
        if (a.Number2 != b.Number2)
            return false;
        if (a.Number3 != b.Number3)
            return false;
        if (a.Type != b.Type)
            return false;
        if (a.AffectedCellGroup != b.AffectedCellGroup)
            return false;
        if (a.BuildingType != b.BuildingType)
            return false;
        return true;
    }

    public static bool operator !=(CellData a, CellData b)
    {
        if (a.Number1 != b.Number1)
            return true;
        if (a.Number2 != b.Number2)
            return true;
        if (a.Number3 != b.Number3)
            return true;
        if (a.Type != b.Type)
            return true;
        if (a.AffectedCellGroup != b.AffectedCellGroup)
            return true;
        if (a.BuildingType != b.BuildingType)
            return true;
        return false;
    }

}
