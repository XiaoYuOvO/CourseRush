using System.Text.Json.Nodes;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using Resultful;

namespace CourseRush.HNU.Debug;

public class HNUDebugSelectionClient : HdjwDebugClient, ICourseSelectionClient<HdjwError, HNUCourse, HNUCourseCategory>
{
    protected internal HNUDebugSelectionClient(HdjwAuthResult auth) : base(auth)
    {
    }

    public Result<IReadOnlyList<HNUCourse>, HdjwError> GetCoursesByCategory(HNUCourseCategory category)
    {
        if (category.CategoryNumber % 2 == 0)
        {
            return ReadCourses("H:\\CSharpeProjects\\CourseRush\\hnu_courses_type3.json");
        }
        return ReadCourses("H:\\CSharpeProjects\\CourseRush\\hnu_courses_type1.json");
    }

    private List<HNUCourse> ReadCourses(string path)
    {
        using var fileStream = File.Open(path, FileMode.Open);
        var array = JsonNode.Parse(fileStream)?["data"]?["showKclist"]?.AsArray();
        var hnuCourses = new List<HNUCourse>();
        if (array is null) return hnuCourses;
        foreach (var jsonNode in array)
        {
            if (jsonNode != null)
            {
                HNUCourse.FromJson(jsonNode.AsObject()).Tee(course => hnuCourses.Add(course));
            }
        }
        return hnuCourses;
    }

    public Result<IReadOnlyList<HNUCourseCategory>, HdjwError> GetCategoriesInRound()
    {
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

    public Result<IReadOnlyList<ISelectedCourse>, HdjwError> GetCurrentCourseTable()
    {
        using var fileStream = File.Open("H:\\CSharpeProjects\\CourseRush\\hnu_course_categories.json", FileMode.Open);
        var array = JsonNode.Parse(fileStream)?.AsArray();
        var selectedCourses = new List<HNUSelectedCourse>();
        if (array is null) return selectedCourses;
        foreach (var jsonNode in array)
        {
            if (jsonNode?["kcCahe"] is JsonObject o)
            {
                HNUSelectedCourse.FromJson(o).Tee(selectedCourse => selectedCourses.Add(selectedCourse));
            }
        }

        return selectedCourses;
    }

    public VoidResult<HdjwError> SelectCourse(HNUCourse course)
    {
        if (Random.Shared.Next(2) == 1)
        {
            return new HdjwError("Failed to select course!");
        }

        return Result.Ok<HdjwError>();
    }

    public VoidResult<HdjwError> RemoveSelectedCourse(List<HNUCourse> course)
    {
        if (Random.Shared.Next(2) == 1)
        {
            return new HdjwError("Failed to remove course!");
        }

        return Result.Ok<HdjwError>();
    }
}