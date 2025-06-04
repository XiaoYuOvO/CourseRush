using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CourseRush.Core;
using CourseRush.Models;
using HandyControl.Controls;
using MahApps.Metro.Controls;

namespace CourseRush;

public partial class MainWindow
{
    private readonly IMainWindowModel _mainWindowModel;
    private Page _currentPage;
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public MainWindow(IMainWindowModel mainWindowModel)
    {
        _mainWindowModel = mainWindowModel;
        InitializeComponent();
        
        GotoPage(_currentPage = _mainWindowModel.GetSelectionSessionsPage());
        Dialog.Register("MainWindow", this);
        NavFrame.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        NavFrame.VerticalContentAlignment = VerticalAlignment.Stretch;
        NavFrame.SizeChanged += (_, _) =>
        {
            _currentPage.Width = NavFrame.RenderSize.Width;
            _currentPage.Height = NavFrame.RenderSize.Height;
            _currentPage.RenderSize = NavFrame.RenderSize;

            var fontSizeFactor = Math.Min(ActualWidth / 1920, ActualHeight / 1080);
            _mainWindowModel.OnAutoFontSizeChanged(fontSizeFactor);
            StudentGravatar.Height = 80 * fontSizeFactor;
            StudentGravatar.Width = 80 * fontSizeFactor;
            StudentNameLabel.FontSize = 15 * fontSizeFactor;
            ClassLabel.FontSize = 14 * fontSizeFactor;
            AccountLabel.FontSize = 14 * fontSizeFactor;
            CurrentSelectionLabel.FontSize = 14 * fontSizeFactor;
            SessionListBtn.FontSize = 20 * fontSizeFactor;
            SessionListBtn.Height = double.NaN;
            CourseSelectionBtn.FontSize = 20 * fontSizeFactor;
            CourseSelectionBtn.Height = double.NaN;
            CoursesSelectionQueueBtn.FontSize = 20 * fontSizeFactor;
            CoursesSelectionQueueBtn.Height = double.NaN;
            CurrentCourseTableBtn.FontSize = 20 * fontSizeFactor;
            CurrentCourseTableBtn.Height = double.NaN;
            SideMenu.Height = double.NaN;
        };
        _mainWindowModel.RegisterUserInfoListener(userInfo =>
        {
            StudentNameLabel.Content = userInfo.Name;
            ClassLabel.Content = userInfo.ClassName;
            Task.Run(() => UpdateUserAvatar(userInfo));
        });
        _mainWindowModel.RegisterSessionSelectedListener(selection =>
            //Callback is on task thread, so we need to call it on main thread
            this.Invoke(()=>
        {
            CurrentSelectionLabel.Text = string.Format(CourseRush.Language.ui_label_selection_session, selection.SelectionTypeName, selection.SelectionTimeId);
            CourseSelectionBtn.IsEnabled = true;
            CoursesSelectionQueueBtn.IsEnabled = true;
            CurrentCourseTableBtn.IsEnabled = true;
        }));
        _mainWindowModel.ReloadUserInfo();
        AccountLabel.Content = _mainWindowModel.GetUserAccount();
    }

    private void UpdateUserAvatar(IUserInfo userInfo)
    {
        userInfo.AvatarGetter.Get().Tee(task =>
        {
            task.Wait();
            var stream = new MemoryStream();
            task.Result.CopyTo(stream);
            stream.Seek(0,SeekOrigin.Begin);
            this.Invoke(()=>
            {
                var bitmapDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                StudentGravatar.Source = bitmapDecoder.Frames.First();
                stream.Dispose();
            });
        }).TeeError(error => Growl.Error(error.Message));
    }

    private void CurrentCourseTable_OnSelected(object sender, RoutedEventArgs e)
    {
        GotoPage(_mainWindowModel.GetCurrentCourseTablePage());
    }

    private void CourseSelectionQueue_OnSelected(object sender, RoutedEventArgs e)
    {
        GotoPage(_mainWindowModel.GetSelectionQueuePage());
    }

    private void SelectionSessionList_OnSelected(object sender, RoutedEventArgs e)
    {
        GotoPage(_mainWindowModel.GetSelectionSessionsPage());
    }

    // private void Gravatar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    // {
    //     _mainWindowModel.ReloadUserInfo();
    // }

    private void CourseSelectionList_OnSelected(object sender, RoutedEventArgs e)
    {
        GotoPage(_mainWindowModel.GetCourseSelectionListPage());
    }

    private void GotoPage(Page page)
    {
        _currentPage = page;
        NavFrame.Navigate(page);
            _currentPage.Width = NavFrame.RenderSize.Width;
            _currentPage.Height = NavFrame.RenderSize.Height;
        _currentPage.RenderSize = NavFrame.RenderSize;
    }
}