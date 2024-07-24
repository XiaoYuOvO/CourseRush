using System.Collections.Immutable;
using System.Globalization;
using CourseRush.Core.Util;

namespace CourseRush.Core;

public class CourseWeeklyTime
{
    /// <summary>
    /// The day of week and the lesson index on that day.
    /// </summary>
    public ImmutableDictionary<DayOfWeek, ImmutableList<int>> WeeklySchedule { get; }

    public ImmutableList<int> TeachingWeek { get; }
    public string TeachingLocation { get; }
    public string TeachingCampus { get; }

    private string? _toStringCache;


    protected CourseWeeklyTime(string teachingLocation, string teachingCampus, IEnumerable<int> teachingWeek, IDictionary<DayOfWeek, ImmutableList<int>> weeklySchedule)
    {
        TeachingLocation = teachingLocation;
        TeachingCampus = teachingCampus;
        TeachingWeek = teachingWeek.ToImmutableList();
        WeeklySchedule = weeklySchedule.ToImmutableDictionary();
    }

    //1-16周 星期一1-2节, 星期二3-4节@
    public override string ToString()
    {
        if (_toStringCache != null) return _toStringCache;
        var scheduleString = string.Join("#",WeeklySchedule
            .Select(pair => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(pair.Key)}{GetSimplifiedRangeString(pair.Value)}节"));
        return _toStringCache = $"{GetSimplifiedRangeString(TeachingWeek)}周 {scheduleString}@{TeachingLocation}({TeachingCampus})";
        
        string GetSimplifiedRangeString(ImmutableList<int> list)
        {
            return string.Join(",",
                FindRanges(list).Select(range =>
                    range.Start.Equals(range.End) ? range.Start.Value.ToString() : $"{range.Start}-{range.End}").ToList());
        }
    }

    private static IEnumerable<Range> FindRanges(ImmutableList<int> enumerable)
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

    public static List<CourseWeeklyTime> TryMerge(List<CourseWeeklyTime> times)
    {
        if (times.Count == 0) return times;
        if (!times.Select(time => time.TeachingCampus).AllSame()) return times;
        if (!times.Select(time => time.TeachingLocation).AllSame()) return times;
        if (!times.Select(time => time.TeachingWeek).AllSubsequencesEqual()) return times;

        var first = times.First();
        return new List<CourseWeeklyTime>
        {
            new(first.TeachingLocation, first.TeachingCampus, first.TeachingWeek,
                times.Select(time => time.WeeklySchedule).Aggregate(CollectionUtils.MergeDictionaries))
        };
    }
    
    public ConflictResult? ResolveConflictWith(CourseWeeklyTime other)
    {
        var intersectWeeks = TeachingWeek.Intersect(other.TeachingWeek).ToImmutableList();
        if (!intersectWeeks.Any()) return null;
        var conflictMap = new Dictionary<DayOfWeek, List<int>>();
        foreach (var dayOfWeek in WeeklySchedule.Keys)
        {
            var conflictLessonIds = other.WeeklySchedule[dayOfWeek].Intersect(WeeklySchedule[dayOfWeek]).ToList();
            if (conflictLessonIds.Any())
            {
                conflictMap[dayOfWeek] = conflictLessonIds;
            }
        }
        return conflictMap.Any() ? new ConflictResult(conflictMap.ToImmutableDictionary(), intersectWeeks) : null;
    }
    
    public class ConflictResult
    {
        public ConflictResult(ImmutableDictionary<DayOfWeek, List<int>> conflictMap, ImmutableList<int> conflictWeeks)
        {
            ConflictMap = conflictMap;
            ConflictWeeks = conflictWeeks;
        }

        public ImmutableDictionary<DayOfWeek, List<int>> ConflictMap { get; }
        public ImmutableList<int> ConflictWeeks { get; }
    }
}