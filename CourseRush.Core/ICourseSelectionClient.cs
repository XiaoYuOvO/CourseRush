using Resultful;

namespace CourseRush.Core;

public interface ICourseSelector<TError, in TCourse> where TError : BasicError where TCourse : ICourse
{
    public VoidResult<TError> SelectCourse(TCourse course);
    public VoidResult<TError> RemoveSelectedCourse(IReadOnlyList<TCourse> course);
}

public interface ICourseSelectionClient<TError, TCourse, TSelectedCourse, TCourseCategory> : ICourseSelector<TError, TCourse>
    where TError : BasicError where TCourse : ICourse where TCourseCategory : ICourseCategory where TSelectedCourse : ISelectedCourse
{
    public Result<IReadOnlyList<TCourse>, TError> GetCoursesByCategory(TCourseCategory category);
    public Result<IReadOnlyList<TCourseCategory>, TError> GetCategoriesInRound();
    public Result<IReadOnlyList<TSelectedCourse>, TError> GetCurrentCourseTable();
    public new VoidResult<TError> SelectCourse(TCourse course);
    public new VoidResult<TError> RemoveSelectedCourse(IReadOnlyList<TCourse> course);
    public Result<WeekTimeTable, TError> GetWeekTimeTable();
}