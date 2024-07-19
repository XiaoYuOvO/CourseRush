using System.Collections.Immutable;

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


    public CourseWeeklyTime(string teachingLocation, string teachingCampus, IEnumerable<int> teachingWeek, Dictionary<DayOfWeek, ImmutableList<int>> weeklySchedule)
    {
        TeachingLocation = teachingLocation;
        TeachingCampus = teachingCampus;
        TeachingWeek = teachingWeek.ToImmutableList();
        WeeklySchedule = weeklySchedule.ToImmutableDictionary();
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