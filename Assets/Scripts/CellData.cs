using System.Text;
using System.Runtime.CompilerServices;

[System.Serializable]
public struct CellData
{
    public enum CellType
    {
        Default,
        Start,
        TeleportIn,
        TeleportOut
    }

    public CellType Type;

    public int Number1;

    public int Number2;

    public int BuildingType;

    public CellData(CellType type, int num1, int num2, int bdt)
    {
        Type = type;
        Number1 = num1;
        Number2 = num2;
        BuildingType = bdt;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CellData((int, int, int, int) d)
    {
        return new CellData((CellType)d.Item1, d.Item2, d.Item3, d.Item4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CellData((CellType, int, int, int) d)
    {
        return new CellData(d.Item1, d.Item2, d.Item3, d.Item4);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(32);
        sb.Append('(');
        sb.Append(Type);
        sb.Append(", ");
        sb.Append(Number1);
        sb.Append(", ");
        sb.Append(Number2);
        sb.Append(", ");
        sb.Append(BuildingType);
        sb.Append(')');
        return sb.ToString();
    }

    public static CellData Parse(string s)
    {
        CellData cd = new CellData();

        char[] delims = { '(', ',', ' ', '\t', '\r', '\n', ')' };

        string[] data = s.Split(delims, System.StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 4)
            throw new System.Exception("Not enough data for parsing given");

        cd.Type = (CellType)System.Enum.Parse(typeof(CellType), data[0]);
        cd.Number1 = int.Parse(data[1]);
        cd.Number2 = int.Parse(data[2]);
        cd.BuildingType = int.Parse(data[3]);

        return cd;
    }

}
