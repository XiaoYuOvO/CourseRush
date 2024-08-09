using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CourseRush.Core;
using HandyControl.Controls;
using HandyControl.Tools.Extension;

namespace CourseRush;

public interface ISelectionSessionPageModel
{
    DataGrid GetDataGrid();
}

public class SelectionSessionPageModel<TCourseSelection> : ISelectionSessionPageModel where TCourseSelection : class, ICourseSelection
{
    private readonly ObservableCollection<TCourseSelection> _selections;
    private readonly DataGrid _grid;

    public SelectionSessionPageModel(List<PresentedData<TCourseSelection>> presentedData,Action<TCourseSelection> selectCallback)
    {
        _selections = new ObservableCollection<TCourseSelection>();
        _grid = new DataGrid
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ItemsSource = _selections,
            HeadersVisibility = DataGridHeadersVisibility.All,
            RowHeaderWidth = 60,
            SelectionMode = DataGridSelectionMode.Extended,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            CanUserReorderColumns = true
        };

        presentedData.Select(data =>
        {
            var column = new DataGridTextColumn
            {
                Header = data.DataTip,
                Binding = new Binding
                {
                    Converter = new PresentedSessionConverter<TCourseSelection>(data),
                    Path = new PropertyPath("")
                },
                CanUserReorder = true
            };
            return column;
        }).Do(column => _grid.Columns.Add(column));


        var frameworkElementFactory = new FrameworkElementFactory(typeof(Button));
        frameworkElementFactory.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler((sender, _) =>
        {
            if (((sender as FrameworkElement)?.TemplatedParent as ContentPresenter)?.Content is TCourseSelection courseSelection)
            {
                selectCallback(courseSelection);
            }
        }));
        
        frameworkElementFactory.SetBinding(ContentControl.ContentProperty, new Binding
        {
            Path = new PropertyPath(""),
            Source = Language.ui_button_select
        });
        
        _grid.Columns.Add(new DataGridTemplateColumn
        {
            CellTemplate = new DataTemplate
            {
                VisualTree = frameworkElementFactory
            }
        });
        
        DataGridAttach.SetShowRowNumber(_grid, true);
    }

    public void AddSelection(TCourseSelection selection)
    {
        _selections.Add(selection);
    }

    public void Clear()
    {
        _selections.Clear();
    }

    public DataGrid GetDataGrid()
    {
        return _grid;
    }
}

public class PresentedSessionConverter<TCourseSelection> : IValueConverter where TCourseSelection : ICourseSelection
{
    private readonly PresentedData<TCourseSelection> _data;

    public PresentedSessionConverter(PresentedData<TCourseSelection> data)
    {
        _data = data;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(string) && value is TCourseSelection selection)
        {
            return _data.GetValue(selection);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}