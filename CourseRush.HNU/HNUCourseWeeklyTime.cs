using System.Collections.Immutable;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseWeeklyTime : CourseWeeklyTime
{
    protected HNUCourseWeeklyTime(string teachingLocation, string teachingCampus, IEnumerable<int> teachingWeek, IDictionary<DayOfWeek, ImmutableList<int>> weeklySchedule) : base(teachingLocation, teachingCampus, teachingWeek, weeklySchedule)
    {
    }

    public HNUCourseWeeklyTime Clone()
    {
        return new HNUCourseWeeklyTime(TeachingLocation, TeachingCampus, new List<int>(TeachingWeek),
            new Dictionary<DayOfWeek, ImmutableList<int>>(WeeklySchedule));
    }

    public static Option<HNUCourseWeeklyTime> Compressed(ImmutableList<HNUCourseWeeklyTime> times)
    {
        if(!times.Any()) return Option<HNUCourseWeeklyTime>.None;
        var first = times.First();
        return new HNUCourseWeeklyTime(first.TeachingLocation, first.TeachingCampus, first.TeachingWeek,
            times.Select(t => t.WeeklySchedule).Aggregate(CollectionUtils.MergeDictionaries))
        {
            BindingCourse = first.BindingCourse
        };
    }

    public override string ToJsonString()
    {
        return $"@{string.Join(",",TeachingWeek.Select(i => i.ToString()))}@@{string.Join(",",WeeklySchedule.SelectMany(pair => pair.Value.Select(lesson => DayOfWeekToId(pair.Key) + lesson.ToString("00"))))}@{TeachingLocation}@@@@{TeachingCampus}@";
    }

    //1-16@
    //1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16@
    //40708@
    //407,408@
    //研A208@
    //1610@
    //1740@3@
    //南校区(天马)@W134b3640000WH~
    public static Result<HNUCourseWeeklyTime, HdjwError> FromString(string data)
    {
        try
        {
            var strings = data.Split("@");
        
            var teachingWeeks = strings[1].Split(",").Select(int.Parse).ToImmutableList();
            Dictionary<DayOfWeek, List<int>> weeklySchedule = new();
        
            //Parse Weekly Schedule
            foreach (var singleLesson in strings[3].Split(","))
            {
                var dayOfWeek = IdToDayOfWeek(singleLesson[0]);
                if (weeklySchedule.ContainsKey(dayOfWeek))
                {
                    weeklySchedule[dayOfWeek].Add(int.Parse(singleLesson[1..]));
                }
                else
                {
                    weeklySchedule[dayOfWeek] = new List<int>{int.Parse(singleLesson[1..])};
                }
            }
        
            //Immutable copy
            Dictionary<DayOfWeek, ImmutableList<int>> immutableWeeklySchedule = new();
            foreach (var (key, value) in weeklySchedule)
            {
                immutableWeeklySchedule[key] = value.ToImmutableList();
            }
            return new HNUCourseWeeklyTime(strings[4], strings[8], teachingWeeks, immutableWeeklySchedule);
        }
        catch (Exception e)
        {
            return HdjwError.Wrap(e);
        }
    }
    
    public static IReadOnlyList<HNUCourseWeeklyTime> TryMerge(IReadOnlyList<HNUCourseWeeklyTime> times)
    {
        if (times.Count == 0) return times;
        if (!times.Select(time => time.BindingCourse).AllSame()) return times;
        if (!times.Select(time => time.TeachingCampus).AllSame()) return times;
        if (!times.Select(time => time.TeachingLocation).AllSame()) return times;
        if (!times.Select(time => time.TeachingWeek).AllSubsequencesEqual()) return times;

        var first = times.First();
        return new List<HNUCourseWeeklyTime>
        {
            new(first.TeachingLocation, first.TeachingCampus, first.TeachingWeek,
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
    
    private static string DayOfWeekToId(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? "7" : ((int)dayOfWeek).ToString();
    }

}