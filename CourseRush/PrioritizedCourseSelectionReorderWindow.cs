using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CourseRush.Controls;
using CourseRush.Core;
using HandyControl.Tools;
using Resultful;

namespace CourseRush;

public static class PrioritizedCourseSelectionReorderWindow<TCourse> where TCourse : ICourse, IPresentedDataProvider<TCourse>
{
    public static Option<IReadOnlyList<TCourse>> ShowReorderWindow(IReadOnlyList<TCourse> courses)
    {
        PrioritizedCourseSelectionReorderWindow? window = null;
        var courseList = new ObservableCollection<TCourse>(courses);
        var activeWindow = WindowHelper.GetActiveWindow();
        Application.Current.Dispatcher.Invoke(() =>
        {
            window = new PrioritizedCourseSelectionReorderWindow
            {
                CourseList =
                {
                    ItemsSource = courseList 
                },
                Title = Language.ui_title_prioritized_coures_window,
                Owner = activeWindow
            };
            var gridView = new GridView();
            window.CourseList.View = gridView; 
            foreach (var presentedData in TCourse.GetPresentedData())
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = presentedData.DataTip,
                    DisplayMemberBinding = new Binding
                    {
                        Converter = new PresentedCourseConverter<TCourse>(presentedData),
                        Path = new PropertyPath("")
                    }
                });
            }

            window.ItemDropped += info =>
            {
                if (info.Data is TCourse course)
                {
                    courseList.Insert(info.InsertIndex, course);
                }
            };
            window.ShowDialog();
        });
        return window!.IsAccepted ? window!.CourseList.ItemsSource.Cast<TCourse>().ToImmutableList() : Option<IReadOnlyList<TCourse>>.None;
    }
}