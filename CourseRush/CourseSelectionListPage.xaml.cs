using System;
using System.Windows;
using CourseRush.Core;
using Microsoft.Win32;

namespace CourseRush;

public partial class CourseSelectionListPage
{
    private readonly ICourseSelectionPageModel _model;
    public IUserInfo? UserInfo { get; set; }

    public CourseSelectionListPage(ICourseSelectionPageModel model, Action<AutoFontSizeChanged> subscription)
    {
        _model = model;
        InitializeComponent();

        subscription(fontSizeFactor =>
        {
            ShowSearchButton.FontSize = 16 * fontSizeFactor;
            ImportButton.FontSize = 16 * fontSizeFactor;
            ExportButton.FontSize = 16 * fontSizeFactor;
            RefreshButton.FontSize = 16 * fontSizeFactor;
            FilesMenu.FontSize = 16 * fontSizeFactor;
            ShowConflictCourse.FontSize = 16 * fontSizeFactor;
            ShowSearchButton.FontSize = 16 * fontSizeFactor;
            ShowFullCourse.FontSize = 16 * fontSizeFactor;
            ShowOnlyInClassCourse.FontSize = 16 * fontSizeFactor;
        });
        _model.SubscribeCourseCategoryChanged(list =>
        {
            DataTableTab.Items.Clear();
            list.ForEach(tabItem =>
            {
                tabItem.SubscribeAutoFontResize(subscription);
                DataTableTab.Items.Add(tabItem);
            });
        });
        DataTableTab.SelectionChanged += (_, _) =>
        {
            ShowSearchButton.IsChecked = (DataTableTab.SelectedContent as CourseDataGrid)?.IsSearchPanelVisible() ?? false;
        };

    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataTableTab.Items[DataTableTab.SelectedIndex] is CourseTabItem courseTabItem)
        {
            _model.RefreshCourse(courseTabItem.Category);
        }
    }

    private void Import_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataTableTab.Items[DataTableTab.SelectedIndex] is not CourseTabItem courseTabItem) return;
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = false
        };
        if (openFileDialog.ShowDialog() ?? false)
        {
            _model.LoadCoursesForCategory(openFileDialog.OpenFile(),courseTabItem.Category);
        }
    }

    private void Export_OnClick(object sender, RoutedEventArgs e)
    {

    }

    private void ToggleButton_OnClick(object sender, RoutedEventArgs e)
    {
        (DataTableTab.SelectedContent as CourseDataGrid)?.SetSearchPanelVisibility(ShowSearchButton.IsChecked ?? false);
    }

    private void OnFilterButtonClicked(object sender, RoutedEventArgs e)
    {
        UpdateFilter();
    }

    private void UpdateFilter()
    {
        (DataTableTab.SelectedContent as CourseDataGrid)?.UpdateRecordFilter(ShowFullCourse.IsChecked ?? false, ShowOnlyInClassCourse.IsChecked ?? false, ShowConflictCourse.IsChecked ?? false, UserInfo?.ClassName);
    }
}