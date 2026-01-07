using System.Collections.Immutable;
using System.Globalization;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Core;

public abstract class CourseWeeklyTime(
    string teachingLocation,
    IEnumerable<int> teachingWeek,
    IDictionary<DayOfWeek, ImmutableList<int>> weeklySchedule)
{
    /// <summary>
    /// The day of week and the lesson index on that day.
    /// </summary>
    public ImmutableDictionary<DayOfWeek, ImmutableList<int>> WeeklySchedule { get; } = weeklySchedule.ToImmutableDictionary();

    public ImmutableList<int> TeachingWeek { get; } = teachingWeek.ToImmutableList();
    public string TeachingLocation { get; } = teachingLocation;

    private string? _toStringCache;
    public Option<ICourse> BindingCourse = Option<ICourse>.None;


    //1-16周 星期一1-2节, 星期二3-4节@
    public override string ToString()
    {
        if (_toStringCache != null) return _toStringCache;
        var scheduleString = string.Join("#",WeeklySchedule
            .Select(pair => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(pair.Key)}{GetSimplifiedRangeString(pair.Value)}节"));
        return _toStringCache = $"{GetSimplifiedRangeString(TeachingWeek)}周 {scheduleString}@{TeachingLocation}";
        
        string GetSimplifiedRangeString(ImmutableList<int> list)
        {
            return string.Join(",", CollectionUtils.FindRanges(list).Select(Utils.ToSimpleString).ToList());
        }
    }
    
    
    public ConflictResult? ResolveConflictWith(CourseWeeklyTime other)
    {
        var intersectWeeks = TeachingWeek.Intersect(other.TeachingWeek).ToImmutableList();
        if (intersectWeeks.IsEmpty) return null;
        var conflictMap = new Dictionary<DayOfWeek, List<int>>();
        foreach (var dayOfWeek in WeeklySchedule.Keys)
        {
            if (!other.WeeklySchedule.TryGetValue(dayOfWeek, out var value)) continue;
            var conflictLessonIds = value.Intersect(WeeklySchedule[dayOfWeek]).ToList();
            if (conflictLessonIds.Count != 0)
            {
                conflictMap[dayOfWeek] = conflictLessonIds;
            }
        }
        return conflictMap.Count != 0 ? new ConflictResult(conflictMap.ToImmutableDictionary(), intersectWeeks) : null;
    }
    
    public class ConflictResult(ImmutableDictionary<DayOfWeek, List<int>> conflictMap, ImmutableList<int> conflictWeeks)
    {
        public ImmutableDictionary<DayOfWeek, List<int>> ConflictMap { get; } = conflictMap;
        public ImmutableList<int> ConflictWeeks { get; } = conflictWeeks;
    }

    public abstract string ToJsonString();
}