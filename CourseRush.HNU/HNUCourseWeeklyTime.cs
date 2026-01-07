using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseWeeklyTime : CourseWeeklyTime
{
    private HNUCourseWeeklyTime(string teachingLocation, IEnumerable<int> teachingWeek,
        IDictionary<DayOfWeek, ImmutableList<int>> weeklySchedule) : base(teachingLocation,
        teachingWeek, weeklySchedule)
    {
    }

    public HNUCourseWeeklyTime Clone()
    {
        return new HNUCourseWeeklyTime(TeachingLocation, new List<int>(TeachingWeek),
            new Dictionary<DayOfWeek, ImmutableList<int>>(WeeklySchedule));
    }

    public static Option<HNUCourseWeeklyTime> Compressed(ImmutableList<HNUCourseWeeklyTime> times)
    {
        if(!times.Any()) return Option<HNUCourseWeeklyTime>.None;
        var first = times.First();
        return new HNUCourseWeeklyTime(first.TeachingLocation, first.TeachingWeek,
            times.Select(t => t.WeeklySchedule).Aggregate(CollectionUtils.MergeDictionaries))
        {
            BindingCourse = first.BindingCourse
        };
    }

    public override string ToJsonString()
    {
        return $"{IntsToRangeString(TeachingWeek)}周 {string.Join(";", WeeklySchedule.Select(pair => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(pair.Key)} {IntsToRangeString(pair.Value)}节"))}";
}

    private static string IntsToRangeString(ImmutableList<int> ints)
    {
        return string.Join(",", CollectionUtils.FindRanges(ints).Select(range => $"{range.Start}-{range.End}"));
    }

    //1-16@
    //1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16@
    //40708@
    //407,408@
    //研A208@
    //1610@
    //1740@3@
    //南校区(天马)@W134b3640000WH~
    public static Result<HNUCourseWeeklyTime, HdjwError> FromString(string dateTime, string teachingLocation)
    {
        try
        {
            var strings = dateTime.Split(" ");
            var teachingWeeks = strings[0].TrimEnd('周').Split(",").SelectMany(StringRangeToNumbers);
            Dictionary<DayOfWeek, IEnumerable<int>> weeklySchedule = new()
            {
                //Parse Weekly Schedule
                [NameToDayOfWeek(strings[1])] = StringRangeToNumbers(strings[2])
            };
            //Immutable copy
            Dictionary<DayOfWeek, ImmutableList<int>> immutableWeeklySchedule = new();
            foreach (var (key, value) in weeklySchedule)
            {
                immutableWeeklySchedule[key] = value.ToImmutableList();
            }
            return new HNUCourseWeeklyTime(teachingLocation, teachingWeeks, immutableWeeklySchedule);
        }
        catch (Exception e)
        {
            return HdjwError.Wrap(e);
        }
    }
    
    public static Result<HNUCourseWeeklyTime, HdjwError> FromJson(JsonObject json)
    {
        return json.RequireString("jgxm")
            .Bind(teacherName => json
                .RequireString("jsmc")
                .Bind(teachingLocation => json
                    .RequireArray("skzcList")
                    .Bind(array => array.Where(node => node is not null).Select(node => node!.ParseInt())
                        .CombineResults()
                        .Bind(teachingWeeks => json
                            .RequireString("xq")
                            .Bind(dayOfWeek => json
                                .RequireString("skjcmc")
                                .Map(MultipleStringRangeToNumbers)
                                .Bind<HNUCourseWeeklyTime>(lessonIndexes => new HNUCourseWeeklyTime(teachingLocation,
                                    teachingWeeks, new Dictionary<DayOfWeek, ImmutableList<int>>
                                    {
                                        [IdToDayOfWeek(dayOfWeek[0])] = lessonIndexes.ToImmutableList()
                                    })))))));
    }

    private static IEnumerable<int> MultipleStringRangeToNumbers(string s)
    {
        return s.Split(",").SelectMany(StringRangeToNumbers);
    }
    
    private static IEnumerable<int> StringRangeToNumbers(string s)
    {
        if (!s.Contains('-'))
            return [int.Parse(s)];
        var startEnd = s.Split("-");
        var start = int.Parse(startEnd[0]);
        return startEnd.Length == 1
            ? [start]
            : Enumerable.Range(start, int.Parse(startEnd[1]) - start + 1);
    }

    public static IReadOnlyList<HNUCourseWeeklyTime> TryMerge(IReadOnlyList<HNUCourseWeeklyTime> times)
    {
        if (times.Count == 0) return times;
        if (!times.Select(time => time.BindingCourse).AllSame()) return times;
        if (!times.Select(time => time.TeachingLocation).AllSame()) return times;
        if (!times.Select(time => time.TeachingWeek).AllSubsequencesEqual()) return times;

        var first = times[0];
        return new List<HNUCourseWeeklyTime>
        {
            new(first.TeachingLocation, first.TeachingWeek,
                times.Select(time => time.WeeklySchedule).Aggregate(CollectionUtils.MergeDictionaries))
            {
                BindingCourse = first.BindingCourse
            }
        };
    }
    
    private static DayOfWeek IdToDayOfWeek(char id)
    {
        return id switch
        {
            '1' => DayOfWeek.Monday,
            '2' => DayOfWeek.Tuesday,
            '3' => DayOfWeek.Wednesday,
            '4' => DayOfWeek.Thursday,
            '5' => DayOfWeek.Friday,
            '6' => DayOfWeek.Saturday,
            '7' => DayOfWeek.Sunday,
            _   => DayOfWeek.Monday
        };
    }
    
    private static DayOfWeek NameToDayOfWeek(string name)
    {
        return name switch
        {
            "星期一" => DayOfWeek.Monday,
            "星期二" => DayOfWeek.Tuesday,
            "星期三" => DayOfWeek.Wednesday,
            "星期四" => DayOfWeek.Thursday,
            "星期五" => DayOfWeek.Friday,
            "星期六" => DayOfWeek.Saturday,
            "星期日" => DayOfWeek.Sunday,
            _   => DayOfWeek.Monday
        };
    }
    
    private static string DayOfWeekToId(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? "7" : ((int)dayOfWeek).ToString();
    }

}