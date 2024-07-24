using System.Collections.Immutable;
using CourseRush.Core;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseWeeklyTime : CourseWeeklyTime
{
    protected HNUCourseWeeklyTime(string teachingLocation, string teachingCampus, IEnumerable<int> teachingWeek, IDictionary<DayOfWeek, ImmutableList<int>> weeklySchedule) : base(teachingLocation, teachingCampus, teachingWeek, weeklySchedule)
    {
    }
    
    //1-16@
    //1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16@
    //40708@
    //407,408@
    //研A208@
    //1610@
    //1740@3@
    //南校区(天马)@W134b3640000WH~
    public static Result<CourseWeeklyTime, HdjwError> FromString(string data)
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

}