using CourseRush.Auth;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using Resultful;

namespace CourseRush.HNU.Debug;

public class HdjwDebugClient(HdjwAuthResult auth) : AuthClient<HdjwAuthResult>(auth),
    ISessionClient<HdjwError, HNUSelectionSession, HNUCourse, HNUSelectedCourse, HNUCourseCategory>,
    IResultConvertible<HdjwAuthResult, HdjwDebugClient>
{
    public Result<bool, HdjwError> IsOnline()
    {
        Thread.Sleep(1000);
        return true;
    }

    public ICourseSelectionClient<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseCategory> GetSelectionClient(HNUSelectionSession target)
    {
        return new HNUDebugSelectionClient(Auth);
    }

    public Result<IReadOnlyList<HNUSelectionSession>, HdjwError> GetOngoingCourseSelections()
    {
        // Thread.Sleep(1000);
        return new List<HNUSelectionSession>
        {
            new("WZTESTID", "2023-2024-3", "初修选课", DateTime.Today, DateTime.Today, "一选", TimeOnly.FromDateTime(DateTime.Now),TimeOnly.FromDateTime(DateTime.Now), "W134d9bb0000WH"),
            new("WZTESTID2", "2023-2024-4", "重修选课", DateTime.Today, DateTime.Today, "二选", TimeOnly.FromDateTime(DateTime.Now),TimeOnly.FromDateTime(DateTime.Now), "W134d9bb0000WH")
        };
    }

    public Result<IUserInfo, HdjwError> GetUserInfo()
    {
        // Thread.Sleep(1000);
        return new HNUUserInfo("Test", new AvatarGetter(() => new HdjwError("No avatar for test")),"TestingClass");
    }

    public static HdjwDebugClient CreateFromResult(HdjwAuthResult result)
    {
        return new HdjwDebugClient(result);
    }
}