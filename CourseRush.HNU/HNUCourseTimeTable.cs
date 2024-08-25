using System.Collections.Immutable;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseTimeTable(IEnumerable<HNUCourseWeeklyTime> weeklyInformation) : CourseTimeTable(weeklyInformation)
{
    public static Result<HNUCourseTimeTable, HdjwError> FromString(string data)
    {
        return data.Split("~")
            .Where(s => s.Length != 0)
            .Select(HNUCourseWeeklyTime.FromString)
            .CombineResults()
            .Map(HNUCourseWeeklyTime.TryMerge)
            .Map(list => new HNUCourseTimeTable(list));
    }

    public HNUCourseTimeTable Clone()
    {
        return new HNUCourseTimeTable(WeeklyInformation.Cast<HNUCourseWeeklyTime>().Select(info => info.Clone()));
    }

    public override Option<CourseWeeklyTime> GetCompressedTime()
    {
        return HNUCourseWeeklyTime.Compressed(WeeklyInformation.Cast<HNUCourseWeeklyTime>().ToImmutableList()).CastError<CourseWeeklyTime>();
    }

    public override string ToJsonString()
    {
        return string.Join("~", WeeklyInformation.Select(info => info.ToJsonString()));
    }
}