using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.Core;

public abstract class CourseTimeTable
{
    public ImmutableList<CourseWeeklyTime> WeeklyInformation { get; }

    protected CourseTimeTable(IEnumerable<CourseWeeklyTime> weeklyInformation)
    {
        WeeklyInformation = weeklyInformation.ToImmutableList();
    }

    public List<CourseWeeklyTime.ConflictResult>? ResolveConflictWith(CourseTimeTable other)
    {
        var conflictResultList = new List<CourseWeeklyTime.ConflictResult>();
        foreach (var thisWeeklyTime in WeeklyInformation)
        {
            conflictResultList.AddRange(other.WeeklyInformation
                .Select(otherWeeklyTime => thisWeeklyTime.ResolveConflictWith(otherWeeklyTime))
                .Where(weeklyConflict => weeklyConflict != null)!);
        }

        return conflictResultList.Any() ? conflictResultList : null;
    }

    public Option<CourseWeeklyTime> GetTimeTableAtWeek(int weekIndex)
    {
        return WeeklyInformation.FirstOrDefault(time => time.TeachingWeek.Contains(weekIndex))?.ToOption<CourseWeeklyTime>() ?? Option<CourseWeeklyTime>.None;
    }

    public override string ToString()
    {
        return $"{nameof(WeeklyInformation)}: {string.Join("|", WeeklyInformation)}";
    }

    public abstract string ToJsonString();
}