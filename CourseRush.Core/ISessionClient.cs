using Resultful;

namespace CourseRush.Core;

public interface ISessionClient<TError, TCourseSelection, TCourse, TCourseCategory> where TError : BasicError
    where TCourseSelection : ICourseSelection
    where TCourse : ICourse
    where TCourseCategory : ICourseCategory
{
    public Result<bool, TError> IsOnline();
    public ICourseSelectionClient<TError, TCourse, TCourseCategory> GetSelectionClient(TCourseSelection target);
    public Result<IReadOnlyList<TCourseSelection>, TError> GetOngoingCourseSelections();
    public Result<IUserInfo, TError> GetUserInfo();
}