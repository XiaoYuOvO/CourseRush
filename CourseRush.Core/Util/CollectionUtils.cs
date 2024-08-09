using System.Collections.Immutable;

namespace CourseRush.Core.Util;

public static class CollectionUtils
{
    public static bool AllSame<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Distinct().Count() <= 1;
    }

    public static bool True(this bool? b)
    {
        return b != null && b.Value;
    }
    
    public static bool False(this bool? b)
    {
        return b != null && !b.Value;
    }

    public static Predicate<T> And<T>(this Predicate<T> predicate1, Predicate<T> predicate2)
    {
        return t => predicate1(t) && predicate2(t);
    }
    
    public static Predicate<T> AndCombine<T>(Predicate<T> predicate1, Predicate<T> predicate2)
    {
        return t => predicate1(t) && predicate2(t);
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
    
    public static IEnumerable<Range> FindRanges(ImmutableList<int> enumerable)
    {
        enumerable = enumerable.Sort();
        var ranges = new List<Range>();
        int? rangeStart = null;
        for (var i = 0; i < enumerable.Count; i++)
        {
            rangeStart ??= enumerable[i];
            if (i == enumerable.Count - 1)
            {
                ranges.Add(new Range((Index)rangeStart, enumerable[i]));
            }else if (enumerable[i + 1] - enumerable[i] > 1)
            {
                ranges.Add(new Range((Index)rangeStart, enumerable[i]));
                rangeStart = null;
            }
        }
        return ranges;
    }

    public static Dictionary<Range, List<TValue>> DistinctRanges<TValue>(IEnumerable<Tuple<Range, TValue>> enumerable)
    {
        var mergedItems = new Dictionary<Range, List<TValue>>();
        var rangeMax = enumerable.Select(tuple => tuple.Item1).Select(r => r.End.Value).Max();
        List<TValue> lastItems = new();
        var lastStart = 0;
        for (var i = 0; i <= rangeMax; i++)
        {
            var index = i;
            var currentItems = enumerable.Where(tuple => tuple.Item1.InRangeClosed(index)).Select(tuple => tuple.Item2).ToList();
            if (i != 0)
            {
                if (!currentItems.SequenceEqual(lastItems))
                {
                    mergedItems[new Range(lastStart, index-1)] = lastItems;
                    if (i == rangeMax)
                    {
                        mergedItems[new Range(index, index)] = currentItems;
                    }
                    lastStart = i;
                }else if (i == rangeMax)
                {
                    mergedItems[new Range(lastStart, index)] = lastItems;
                }
            }
            lastItems = currentItems;
        }

        return mergedItems;
    }


    public static bool InRangeClosed(this Range range, int index)
    {
        return range.Start.Value <= index && range.End.Value >= index;
    }
}