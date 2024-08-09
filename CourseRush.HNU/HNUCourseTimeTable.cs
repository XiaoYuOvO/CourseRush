using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseTimeTable : CourseTimeTable
{
    public HNUCourseTimeTable(IEnumerable<HNUCourseWeeklyTime> weeklyInformation) : base(weeklyInformation)
    {
    }
    
    public static Result<HNUCourseTimeTable, HdjwError> FromString(string data)
    {
        return data.Split("~")
            .Where(s => s.Length != 0)
            .Select(HNUCourseWeeklyTime.FromString)
            .CombineResults(HdjwError.Combine)
            .Map(HNUCourseWeeklyTime.TryMerge)
            .Map(list => new HNUCourseTimeTable(list));
    }

    public override string ToJsonString()
    {
        return string.Join("~", WeeklyInformation.Select(info => info.ToJsonString()));
    }
}