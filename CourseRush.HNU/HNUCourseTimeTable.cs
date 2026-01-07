using System.Collections.Immutable;
using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Util;
using HtmlAgilityPack;
using Resultful;
using Resultful.LINQ;

namespace CourseRush.HNU;

public class HNUCourseTimeTable(IEnumerable<HNUCourseWeeklyTime> weeklyInformation) : CourseTimeTable(weeklyInformation)
{
    public static Result<HNUCourseTimeTable, HdjwError> FromJson(JsonArray jsonArray)
    {
        return jsonArray.RequireObjectArray().Bind(objects =>
            objects.Select(HNUCourseWeeklyTime.FromJson).CombineResults()
                .Bind<HNUCourseTimeTable>(infos => new HNUCourseTimeTable(infos)));
    }
    
    public static Result<HNUCourseTimeTable, HdjwError> FromHtmlNode(IEnumerable<HtmlNode> tableNode, string? teacher, string? campus)
    {
        return tableNode
            .Select(node => HNUCourseWeeklyTime.FromString(node.InnerText.Trim(), teacher ?? "")).ToList()
            .CombineResults()
            .Map(times => new HNUCourseTimeTable(times));
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

    public static readonly HNUCourseTimeTable Empty = new([]);
}