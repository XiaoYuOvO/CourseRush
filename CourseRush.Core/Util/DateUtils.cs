namespace CourseRush.Core.Util;

public static class DateUtils
{
    public static int GetAsIndex(this DayOfWeek dayOfWeek)
    {
        if (dayOfWeek == DayOfWeek.Sunday)
        {
            return 6;
        }

        return (int)(dayOfWeek - 1);
    }
}