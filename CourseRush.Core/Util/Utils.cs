namespace CourseRush.Core.Util;

public static class Utils
{
    public static void CompareAndUpdate<T>(ref T field, T value, ref bool updateFlag) where T : struct
    {
        if (field.Equals(value)) return;
        field = value;
        updateFlag = true;
    }
    
    public static void CompareAndUpdate(ref string? field, string? value, ref bool updateFlag)
    {
        if (field == null && value == null) return;
        if (field != null && value == null) return;
        if (value!.Equals(field, StringComparison.Ordinal)) return;
        field = value;
        updateFlag = true;
    }

    public static string ToSimpleString(this Range range)
    {
        return range.Start.Equals(range.End) ? range.Start.Value.ToString() : $"{range.Start}-{range.End}";
    }
}