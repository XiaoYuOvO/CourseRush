using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Core;
using CourseRush.Core.Util;
using HandyControl.Controls;
using HandyControl.Data;
using MahApps.Metro.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CourseRush;

public interface ICourseSelectionPageModel
{
    void SubscribeCourseCategoryChanged(Action<List<CourseTabItem>> action);
    void RefreshCourse(ICourseCategory category);
    void LoadCoursesForCategory(Stream fileStream, ICourseCategory category);
}

public class CourseSelectionListPageModel<TCourse, TCourseCategory> : ICourseSelectionPageModel where TCourse : ICourse where TCourseCategory : ICourseCategory
{
    private event Action<List<CourseTabItem>>? OnCourseCategoryAdded; 
    
    private readonly List<PresentedData<TCourse>> _coursePresentedData;
    private readonly DelayedFunc<TCourseCategory, IReadOnlyList<TCourse>> _courseTaskLoader;
    private readonly DelayedFunc<Stream, IReadOnlyList<TCourse>> _courseReader;
    private readonly Dictionary<TCourseCategory, CourseDataGrid<TCourse>> _courseDataByCategory = new();

    public CourseSelectionListPageModel(List<PresentedData<TCourse>> coursePresentedData, DelayedFunc<TCourseCategory, IReadOnlyList<TCourse>> courseTaskLoader, DelayedFunc<Stream, IReadOnlyList<TCourse>> courseReader)
    {
        _coursePresentedData = coursePresentedData;
        _courseTaskLoader = courseTaskLoader;
        _courseReader = courseReader;
    }

    public void UpdateCategories(IReadOnlyList<TCourseCategory> courseCategories)
    {
        List<CourseTabItem> categories = new();
        
        foreach (var courseCategory in courseCategories)
        {
            var dataGrid = new CourseDataGrid<TCourse>(_coursePresentedData);
            AddContextMenuToGrid(dataGrid);
            _courseDataByCategory[courseCategory] = dataGrid;
            categories.Add(new CourseTabItem(dataGrid, courseCategory));
        }

        OnCourseCategoryAdded?.Invoke(categories);
    }

    private static void AddContextMenuToGrid(CourseDataGrid<TCourse> dataGrid)
    {
        var selectCourseItem = new MenuItem
        {
            Header = Language.ui_button_enqueue_selected_courses
        };
        
        selectCourseItem.Click += (_, _) =>
        {
            var selectedCourses = dataGrid.GetSelectedCourses();
            if (MessageBox.Show(new MessageBoxInfo
                {
                    Message =
                        $"{Language.ui_message_comfirm_select}\n{string.Join("\n", selectedCourses.Select(course => course.ToSimpleString()))}",
                    Caption = Language.ui_message_comfirm_select_title,
                    Button = MessageBoxButton.OKCancel,
                    IconBrushKey = ResourceToken.InfoBrush,
                    IconKey = ResourceToken.AskGeometry,
                }) != MessageBoxResult.OK) return;
            Growl.Info(string.Format(Language.ui_message_course_selected, selectedCourses.Count));
            dataGrid.CourseGrid.UnselectAll();
        };
            
        dataGrid.CourseGrid.ContextMenuOpening += (_, _) =>
        {
            selectCourseItem.IsEnabled = dataGrid.GetSelectedCourses().Count != 0;
        };
        dataGrid.CourseGrid.ContextMenu = new ContextMenu
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
            _courseTaskLoader(tcategory, list => _courseDataByCategory[tcategory].Invoke(()=>
            {
                ClearCourses(tcategory);
                AddCourse(tcategory, list);
            }));
        }
    }

    public void LoadCoursesForCategory(Stream fileStream, ICourseCategory category)
    {
        if (category is not TCourseCategory tcategory) return;
        var courseDataGrid = _courseDataByCategory[tcategory];
        _courseReader(fileStream, list => courseDataGrid.Invoke(() => courseDataGrid.AddCourse(list)));
    }
}