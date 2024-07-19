using System.Collections.Immutable;

namespace CourseRush.Core;

public class CourseTimeTable
{
    public ImmutableList<CourseWeeklyTime> WeeklyInformation { get; }

    public CourseTimeTable(IEnumerable<CourseWeeklyTime> weeklyInformation)
    {
        WeeklyInformation = weeklyInformation.ToImmutableList();
    }

    public List<CourseWeeklyTime.ConflictResult>? ResolveConflictWith(CourseTimeTable other)
    {
        var conflictResultList = new List<CourseWeeklyTime.ConflictResult>();
        foreach (var thisWeeklyTime in WeeklyInformation)
        {
            conflictResultList.AddRange(other.WeeklyInformation.Select(otherWeeklyTime => thisWeeklyTime.ResolveConflictWith(otherWeeklyTime)).Where(weeklyConflict => weeklyConflict != null)!);
        }
        return conflictResultList.Any() ? conflictResultList : null;
    }


}