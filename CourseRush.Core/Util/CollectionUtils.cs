using System.Collections.Immutable;

namespace CourseRush.Core.Util;

public static class CollectionUtils
{
    public static bool AllSame<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Distinct().Count() <= 1;
    }
        
    public static bool AllSubsequencesEqual<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        // 使用Zip来组合序列，如果序列长度不一致，Zip会自动调整到最短序列长度
        // 然后使用SequenceEqual来检查所有组合的元素是否相等
        // ReSharper disable PossibleMultipleEnumeration
        return sequences.Skip(1).All(innerSeq => innerSeq.SequenceEqual(sequences.First()));
    }

    public static Dictionary<TKey, List<TValue>> MergeDictionaries<TKey, TValue>(
        Dictionary<TKey, List<TValue>> dict1,
        Dictionary<TKey, List<TValue>> dict2) where TKey : notnull
    {
        foreach (var kvp in dict1)
        {
            if (dict2.ContainsKey(kvp.Key))
            {
                dict2[kvp.Key].AddRange(kvp.Value);
            }
            else
            {
                dict2.Add(kvp.Key, new List<TValue>(kvp.Value));
            }
        }

        return dict2;
    }

    public static ImmutableDictionary<TKey, ImmutableList<TValue>> MergeDictionaries<TKey, TValue>(
        ImmutableDictionary<TKey, ImmutableList<TValue>> dict1,
        ImmutableDictionary<TKey, ImmutableList<TValue>> dict2) where TKey : notnull

    {
        // Union the two dictionaries and then group by the key.
        var groupedByKeys = dict1
            .Union(dict2)
            .GroupBy(kvp => kvp.Key);
        // Convert the groups into an ImmutableDictionary<TKey, ImmutableList<TValue>>
        // where each value list is the concatenation of all lists with the same key.
        return groupedByKeys
            .ToImmutableDictionary(
                group => group.Key,
                group => group.SelectMany(kvp => kvp.Value).ToImmutableList());
    }
}