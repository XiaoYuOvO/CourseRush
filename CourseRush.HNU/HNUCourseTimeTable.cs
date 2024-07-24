using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseTimeTable : CourseTimeTable
{
    public HNUCourseTimeTable(IEnumerable<CourseWeeklyTime> weeklyInformation) : base(weeklyInformation)
    {
    }
    
    public static Result<HNUCourseTimeTable, HdjwError> FromString(string data)
    {
        return data.Split("~")
            .Where(s => s.Length != 0)
            .Select(HNUCourseWeeklyTime.FromString)
            .CombineResults(HdjwError.Combine)
            .Map(CourseWeeklyTime.TryMerge)
            .Map(list => new HNUCourseTimeTable(list));
    }
}