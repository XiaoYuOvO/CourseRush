using Resultful;

namespace CourseRush.Core;

public interface ISessionClient<TError, TCourseSelection, TCourse, TSelectedCourse, TCourseCategory> where TError : BasicError
    where TCourseSelection : ISelectionSession
    where TCourse : ICourse
    where TCourseCategory : ICourseCategory
    where TSelectedCourse : ISelectedCourse
{
    public Result<bool, TError> IsOnline();
    public ICourseSelectionClient<TError, TCourse, TSelectedCourse, TCourseCategory> GetSelectionClient(TCourseSelection target);
    public Result<IReadOnlyList<TCourseSelection>, TError> GetOngoingCourseSelections();
    public Result<IUserInfo, TError> GetUserInfo();
}