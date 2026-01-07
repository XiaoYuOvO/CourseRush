using Resultful;

namespace CourseRush.Core;

public interface ICourseSelector<TError, in TCourse> where TError : BasicError where TCourse : ICourse
{
    public VoidResult<TError> SelectCourse(TCourse course);
    public VoidResult<TError> RemoveSelectedCourse(TCourse course);
}

public interface ICourseSelectionClient<TError, TCourse, TSelectedCourse, TCourseCategory> : ICourseSelector<TError, TCourse>
    where TError : BasicError where TCourse : ICourse where TCourseCategory : ICourseCategory where TSelectedCourse : ISelectedCourse
{
    public Task<VoidResult<TError>> InitializeSelectionAsync();
    public IAsyncEnumerable<IEnumerable<Result<TCourse, TError>>>  GetCoursesByCategory(TCourseCategory category);
    public Task<Result<IReadOnlyList<TCourseCategory>, TError>> GetCategoriesInRoundAsync();
    public Result<IReadOnlyList<TSelectedCourse>, TError> GetCurrentCourseTable();
    public new VoidResult<TError> SelectCourse(TCourse course);
    public new VoidResult<TError> RemoveSelectedCourse(TCourse course);
    public Result<WeekTimeTable, TError> GetWeekTimeTable();
}