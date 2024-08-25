using System.Collections.Immutable;
using Resultful;

namespace CourseRush.Core;

public abstract class CourseTimeTable(IEnumerable<CourseWeeklyTime> weeklyInformation)
{
    public ImmutableList<CourseWeeklyTime> WeeklyInformation { get; } = weeklyInformation.ToImmutableList();

    public Option<List<CourseWeeklyTime.ConflictResult>> ResolveConflictWith(CourseTimeTable other)
    {
        var conflictResultList = new List<CourseWeeklyTime.ConflictResult>();
        foreach (var thisWeeklyTime in WeeklyInformation)
        {
            conflictResultList.AddRange(other.WeeklyInformation
                .Select(otherWeeklyTime => thisWeeklyTime.ResolveConflictWith(otherWeeklyTime))
                .Where(weeklyConflict => weeklyConflict != null)!);
        }

        return conflictResultList.Count != 0 ? conflictResultList : Option<List<CourseWeeklyTime.ConflictResult>>.None;
    }

    public ImmutableList<CourseWeeklyTime> GetTimeTableAtWeek(int weekIndex)
    {
        return WeeklyInformation.FindAll(time => time.TeachingWeek.Contains(weekIndex));
    }

    public abstract Option<CourseWeeklyTime> GetCompressedTime();

    public override string ToString()
    {
        return $"{nameof(WeeklyInformation)}: {string.Join("|", WeeklyInformation)}";
    }

    public abstract string ToJsonString();
}