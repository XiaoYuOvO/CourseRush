using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CourseRush.Controls;
using CourseRush.Models;

namespace CourseRush.Pages;

public partial class CurrentCourseTablePage
{
    private readonly ICurrentCourseTablePageModel _model;
    private bool _isDisplayingCompressedCourses;
    private readonly CourseDetailDrawer _courseInfoDrawer;
    private readonly Action _courseTableReloader;
    public CurrentCourseTablePage(ICurrentCourseTablePageModel model, Action courseTableReloader)
    {
        _model = model;
        _courseTableReloader = courseTableReloader;
        InitializeComponent();
        ContentGrid.Children.Add(model.CourseTable);
        model.CourseTable.OnOffTableCourseButtonClick += (_, _) => OffTableCoursesDrawer.IsOpen = true;
        Grid.SetRow(model.CourseTable, 1);
        UpdateWeekLabel();
        _courseInfoDrawer = model.CreateCourseDetailDrawer();
        Grid.SetRow(_courseInfoDrawer, 1);
        ContentGrid.Children.Add(_courseInfoDrawer);
        model.ConfigureOffTableCoursesDrawer(OffTableCoursesDrawer);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _model.CourseTable.RenderSize = sizeInfo.NewSize;
    }

    public void RegisterAutoFontSize(Action<AutoFontSizeChanged> registerer)
    {
        registerer(factor =>
        {
            FilesMenu.FontSize = 14 * factor;
            RefreshButton.FontSize = 16 * factor;
            WeekIndexLabel.FontSize = 16 * factor;
            OffTableCoursesHeader.FontSize = 16 * factor;
            OffTableCoursesViewer.SetValue(FontSizeProperty, 13 * factor);
            _courseInfoDrawer.SetFontSize(16 * factor, 13 * factor);
        });
    }

    private void Export_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private void Import_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private void PrevWeekButton_OnClick(object sender, RoutedEventArgs e)
    {
        PrevWeekButton.IsEnabled = _model.DisplayPrevWeek();
        NextWeekButton.IsEnabled = true;
        UpdateWeekLabel();
    }

    private void UpdateWeekLabel()
    {
        WeekIndexLabel.Content = string.Format(CourseRush.Language.ui_label_current_week, _model.GetCurrentWeek());
    }

    private void NextWeekButton_OnClick(object sender, RoutedEventArgs e)
    {
        NextWeekButton.IsEnabled = _model.DisplayNextWeek();
        PrevWeekButton.IsEnabled = true;
        UpdateWeekLabel();
    }

    private void WeekIndexLabel_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!_isDisplayingCompressedCourses)
        {
            _model.DisplayCompressedCourses();
            _isDisplayingCompressedCourses = true;
            NextWeekButton.IsEnabled = false;
            PrevWeekButton.IsEnabled = false;
            WeekIndexLabel.Content = CourseRush.Language.ui_label_compressed_courses;
        }
        else
        {
            _model.DisplayCoursesAtCurrentWeek();
            UpdateWeekLabel();
            _isDisplayingCompressedCourses = false;
            NextWeekButton.IsEnabled = _model.CanDisplayNextWeek();
            PrevWeekButton.IsEnabled = _model.CanDisplayPrevWeek();
        }
    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
        _courseTableReloader();
    }
}