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

    public static string Between(this string value, string start, string end)
    {
        var startIndex = value.IndexOf(start, StringComparison.Ordinal) + start.Length;
        var endIndex = value.LastIndexOf(end, StringComparison.Ordinal);
        return value.Substring(startIndex, endIndex - startIndex);
    }

    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
    {
        foreach (var x1 in enumerable)
        {
            yield return x1;
        }
    }
    
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this T enumerable)
    {
        yield return enumerable;
    }
}