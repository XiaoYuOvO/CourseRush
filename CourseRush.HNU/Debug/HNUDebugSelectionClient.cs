using System.Collections.Immutable;
using System.Text.Json.Nodes;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using Resultful;

namespace CourseRush.HNU.Debug;

public class HNUDebugSelectionClient : HdjwDebugClient, ICourseSelectionClient<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseCategory>
{
    private readonly List<HNUSelectedCourse> _selectedCourses = [];
    protected internal HNUDebugSelectionClient(HdjwAuthResult auth) : base(auth)
    {
    }

    public async IAsyncEnumerable<IEnumerable<Result<HNUCourse, HdjwError>>> GetCoursesByCategory(
        HNUCourseCategory category)
    {
        // Thread.Sleep(1000);
        if (category.CategoryNumber % 2 == 0)
        {
            await foreach (var readCourse in ReadCourses("H:\\CSharpeProjects\\CourseRush\\hnu_courses_type3.json"))
            {
                yield return [readCourse];
            }
        }

        await foreach (var readCourse in ReadCourses("H:\\CSharpeProjects\\CourseRush\\hnu_courses_type1.json"))
        {
            yield return [readCourse];
        }
    }

    private static async IAsyncEnumerable<Result<HNUCourse, HdjwError>> ReadCourses(string path)
    {
        // Thread.Sleep(1000);
        await using var fileStream = File.Open(path, FileMode.Open);
        var array = (await JsonNode.ParseAsync(fileStream))?["data"]?["showKclist"]?.AsArray();
        if (array == null) yield break;
        foreach (var jsonNode in array)
        {
            if (jsonNode != null)
            {
                yield return HNUCourse.FromJson(jsonNode.AsObject());
            }
        }
    }

    public Result<IReadOnlyList<HNUCourseCategory>, HdjwError> GetCategoriesInRound()
    {
        // Thread.Sleep(1000);
        using var fileStream = File.Open("H:\\CSharpeProjects\\CourseRush\\hnu_course_categories.json", FileMode.Open);
        var array = JsonNode.Parse(fileStream)?.AsArray();
        var hnuCategories = new List<HNUCourseCategory>();
        if (array is null) return hnuCategories;
        foreach (var jsonNode in array)
        {
            if (jsonNode is JsonObject o)
            {
                HNUCourseCategory.FromJson(o).Tee(category => hnuCategories.Add(category));
            }
        }

        return hnuCategories;
    }

    public Result<IReadOnlyList<HNUSelectedCourse>, HdjwError> GetCurrentCourseTable()
    {
        // Thread.Sleep(1000);
        return _selectedCourses;
    }

    public VoidResult<HdjwError> SelectCourse(HNUCourse course)
    {
        Thread.Sleep(5000);
        if (Random.Shared.Next(2) == 1)
        {
            return new HdjwError("Failed to select course!");
        }
        _selectedCourses.Add(HNUSelectedCourse.SelectFromCourse(course, SelectionMethod.SelfSelect));

        return Result.Ok<HdjwError>();
    }

    public VoidResult<HdjwError> RemoveSelectedCourse(IReadOnlyList<HNUCourse> courses)
    {
        Thread.Sleep(5000);
        if (Random.Shared.Next(2) == 1)
        {
            return new HdjwError("Failed to remove course!");
        }
        
        foreach (var hnuCourse in courses)
        {
            _selectedCourses.RemoveAll(selectedCourse => selectedCourse.Id.Equals(hnuCourse.Id));
        }
        
        return Result.Ok<HdjwError>();
    }

    public Result<WeekTimeTable, HdjwError> GetWeekTimeTable()
    {
        Thread.Sleep(1000);
        return new WeekTimeTable(ImmutableList.CreateRange(new List<LessonTime>
        {
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50))
        }));
    }
}