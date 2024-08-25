using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Controls;
using CourseRush.Core;
using CourseRush.Core.Util;
using HandyControl.Data;
using MahApps.Metro.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CourseRush.Models;

public interface ICourseSelectionPageModel
{
    void SubscribeCourseCategoryChanged(Action<List<CourseTabItem>> action);
    void RefreshCourse(ICourseCategory category);
    void LoadCoursesForCategory(Stream fileStream, ICourseCategory category);
}

public class CourseSelectionListPageModel<TCourse, TCourseCategory>(
    DelayedFunc<TCourseCategory, IReadOnlyList<TCourse>> courseTaskLoader,
    DelayedFunc<Stream, IReadOnlyList<TCourse>> courseReader,
    Action<IReadOnlyList<TCourse>> selectionCallback)
    : ICourseSelectionPageModel
    where TCourse : class, ICourse, IPresentedDataProvider<TCourse>
    where TCourseCategory : ICourseCategory
{
    private event Action<List<CourseTabItem>>? OnCourseCategoryAdded;

    private readonly Dictionary<TCourseCategory, CourseDataPanel<TCourse>> _courseDataByCategory = new();

    public void UpdateCategories(IReadOnlyList<TCourseCategory> courseCategories)
    {
        List<CourseTabItem> categories = [];
        
        foreach (var courseCategory in courseCategories)
        {
            var dataGrid = new CourseDataPanel<TCourse>();
            AddContextMenuToGrid(dataGrid);
            _courseDataByCategory[courseCategory] = dataGrid;
            categories.Add(new CourseTabItem(dataGrid, courseCategory));
        }

        OnCourseCategoryAdded?.Invoke(categories);
    }

    private void AddContextMenuToGrid(CourseDataPanel<TCourse> dataPanel)
    {
        var selectCourseItem = new MenuItem
        {
            Header = Language.ui_button_enqueue_selected_courses
        };
        
        selectCourseItem.Click += (_, _) =>
        {
            var selectedCourses = dataPanel.GetSelectedCourses();
            if (MessageBox.Show(new MessageBoxInfo
                {
                    Message = $"{Language.ui_message_confirm_select}\n{string.Join("\n", selectedCourses.Select(course => course.ToSimpleString()))}",
                    Caption = Language.ui_message_confirm_select_title,
                    Button = MessageBoxButton.OKCancel,
                    IconBrushKey = ResourceToken.InfoBrush,
                    IconKey = ResourceToken.AskGeometry,
                }) != MessageBoxResult.OK) return;
            selectionCallback(selectedCourses);
            dataPanel.CourseGrid.UnselectAll();
        };
            
        dataPanel.CourseGrid.ContextMenuOpening += (_, _) =>
        {
            selectCourseItem.IsEnabled = dataPanel.GetSelectedCourses().Count != 0;
        };
        dataPanel.CourseGrid.ContextMenu = new ContextMenu
        {
            Items =
            {
                selectCourseItem
            }
        };
    }

    private void AddCourse(TCourseCategory category, IReadOnlyList<TCourse> course)
    {
        _courseDataByCategory[category].AddCourse(course);
    }

    private void ClearCourses(TCourseCategory category)
    {
        _courseDataByCategory[category].ClearCourses();
    }

    public void SubscribeCourseCategoryChanged(Action<List<CourseTabItem>> action)
    {
        OnCourseCategoryAdded += action;
    }

    public void RefreshCourse(ICourseCategory category)
    {
        if (category is TCourseCategory tcategory)
        {
            courseTaskLoader(tcategory, list => _courseDataByCategory[tcategory].Invoke(()=>
            {
                _courseDataByCategory[tcategory].RefreshCourses(list);
            }));
        }
    }

    public void CheckAllCoursesConflicts(Action<TCourse> conflictChecker)
    {
        foreach (var (_, courseDataPanel) in _courseDataByCategory)
        {
            courseDataPanel.GetCourses().AsParallel().WithMergeOptions(ParallelMergeOptions.AutoBuffered).ForAll(conflictChecker);
        }
    }

    public void LoadCoursesForCategory(Stream fileStream, ICourseCategory category)
    {
        if (category is not TCourseCategory tcategory) return;
        var courseDataGrid = _courseDataByCategory[tcategory];
        courseReader(fileStream, list => courseDataGrid.Invoke(() => courseDataGrid.AddCourse(list)));
    }
}