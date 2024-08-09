using NodaTime;

namespace CourseRush.Core;

public class LessonTime
{
    public LessonTime(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }

    public TimeOnly Start { get; }
    public TimeOnly End { get; }
}