using System;
using System.Windows.Controls;
using CourseRush.Core;

namespace CourseRush;

public interface IMainWindowModel
{
    internal Page GetCurrentCourseTablePage();
    internal Page GetCourseSelectionListPage();
    internal Page GetSelectionSessionsPage();
    internal void OnAutoFontSizeChanged(double fontSizeFactor);
    internal void RegisterUserInfoListener(Action<IUserInfo> userInfoAction);
    internal void RegisterSessionSelectedListener(Action<ICourseSelection> action);
    internal void ReloadUserInfo();
}