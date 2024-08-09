using System.Collections.Immutable;

namespace CourseRush.Core;

public class WeekTimeTable
{
    public WeekTimeTable(ImmutableList<LessonTime> lessons)
    {
        Lessons = lessons;
    }

    public ImmutableList<LessonTime> Lessons { get; }
}