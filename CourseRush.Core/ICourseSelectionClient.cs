using Resultful;

namespace CourseRush.Core;

public interface ICourseSelectionClient<TError, TCourse, TCourseCategory>
    where TError : BasicError where TCourse : ICourse where TCourseCategory : ICourseCategory
{
    public Result<IReadOnlyList<TCourse>, TError> GetCoursesByCategory(TCourseCategory category);
    public Result<IReadOnlyList<TCourseCategory>, TError> GetCategoriesInRound();
    public Result<IReadOnlyList<ISelectedCourse>, TError> GetCurrentCourseTable();
    public VoidResult<TError> SelectCourse(TCourse course);
    public VoidResult<TError> RemoveSelectedCourse(List<TCourse> course);
}