using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CourseRush.Core;
using CourseRush.Core.Util;
using CourseRush.Models;
using HandyControl.Collections;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using static CourseRush.Core.Util.Utils;
using ScrollViewer = System.Windows.Controls.ScrollViewer;

namespace CourseRush.Controls;



public abstract class CourseDataPanel : UserControl
{
    internal static readonly PropertyInfo? ScrollViewerWheelScrolling = typeof(ScrollViewer).GetProperty("HandlesMouseWheelScrolling", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
    internal static readonly PropertyInfo? ScrollViewerScrollInfo = typeof(ScrollViewer).GetProperty("ScrollInfo", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
    public abstract void SubscribeAutoFontResize(Action<AutoFontSizeChanged> register);
    public abstract void UpdateRecordFilter(bool showFullCourses, bool showOnlyInClassCourses, bool showConflictCourses, string? className);
    public abstract void SetSearchPanelVisibility(bool visibility);
    public abstract bool IsSearchPanelVisible();
    public abstract void ClearCourses();
}

public class CourseDataGrid : DataGrid
{
    protected override DependencyObject GetContainerForItemOverride()
    {
        var courseDataGridRow = new CourseDataGridRow();
        courseDataGridRow.SetBinding(FontSizeProperty, new Binding
        {
            Source = this,
            Path = new PropertyPath("FontSize")
        });
        return courseDataGridRow;
    }

    private class CourseDataGridRow : DataGridRow
    {
        private readonly Brush? _primaryBrush;
        private readonly Brush? _darkDefaultBrush;
        private readonly Brush? _regionBrush;

        public CourseDataGridRow()
        {
            _primaryBrush = FindResource("PrimaryBrush") as Brush;
            _darkDefaultBrush = FindResource("DarkDefaultBrush") as Brush;
            _regionBrush = FindResource("RegionBrush") as Brush;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            if (e.Property != IsSelectedProperty && e.Property != IsMouseOverProperty && e.Property != IsSelectionActiveProperty && e.Property != DataContextProperty)
            {
                return;
            }
            if (IsSelected && !(GetValue(IsSelectionActiveProperty) as bool? ?? true))
            {
                Background =  _darkDefaultBrush;
                return;
            }

            if (this is not DataGridRow { DataContext: ICourse course } dataGridRow) return;
            var conflict = course.ConflictsCache.Count != 0;
            var full = course.SelectedStudentCount >= course.TotalStudentCount;
            Brush? brush;
            if (conflict && full)
            {
                if (dataGridRow.IsSelected) brush = Brushes.Red;
                else brush = dataGridRow.IsMouseOver ? Brushes.OrangeRed : Brushes.DarkRed;
            }else if (conflict)
            {
                if (dataGridRow.IsSelected) brush = Brushes.Purple;
                else brush = dataGridRow.IsMouseOver ? Brushes.MediumPurple : Brushes.DarkMagenta;
            }
            else if (full)
            {
                if (dataGridRow.IsSelected) brush = Brushes.Goldenrod;
                else brush = dataGridRow.IsMouseOver ? Brushes.Chocolate : Brushes.Peru;
            }
            else {
                if (dataGridRow.IsSelected) brush = _primaryBrush;
                else brush = dataGridRow.IsMouseOver ? _darkDefaultBrush : _regionBrush;
            }
            Background = brush;
        }
    }
}

public class CourseDataPanel<TCourse> : CourseDataPanel where TCourse : class, ICourse, IPresentedDataProvider<TCourse>
{
    public CourseDataGrid CourseGrid { get; }
    private readonly StackPanel _searchPropertyGrid = new(){Visibility = Visibility.Collapsed, Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch};
    private readonly ManualObservableCollection<TCourse> _courses = [];
    private readonly List<ISearchProperty<TCourse>> _searchProperties; 
    private readonly Button _searchApplyButton = new();

    public CourseDataPanel()
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                //Data Grid
                new ColumnDefinition
                {
                    Width = new GridLength(6, GridUnitType.Star)
                },
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Auto)
                },
                //Search Panel
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Auto)
                }
            }
        };
        Content = contentGrid;
        contentGrid.Children.Add(CourseGrid = new CourseDataGrid
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HeadersVisibility = DataGridHeadersVisibility.All,
            RowHeaderWidth = 60,
            SelectionMode = DataGridSelectionMode.Extended,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            CanUserReorderColumns = true,
            Style = FindResource("DataGridBaseStyle") as Style
        });
        contentGrid.Children.Add(_searchPropertyGrid);
        var gridSplitter = new GridSplitter
        {
            ResizeDirection = GridResizeDirection.Columns,
            Width = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = FindResource("BorderBrush") as Brush,
            ShowsPreview = true
        };
        gridSplitter.SetBinding(VisibilityProperty, new Binding
        {
            Source = _searchPropertyGrid,
            Path = new PropertyPath("Visibility")
        });
        contentGrid.Children.Add(gridSplitter);
        Grid.SetColumn(CourseGrid, 0);
        Grid.SetColumn(gridSplitter, 1);
        Grid.SetColumn(_searchPropertyGrid, 2);
        
        //Auto initialize the columns
        var presentedDataList = TCourse.GetPresentedData();
        presentedDataList.Select(data =>
        {
            var column = new DataGridTextColumn
            {
                IsReadOnly = true,
                Binding = new Binding
                {
                    Converter = new PresentedCourseConverter<TCourse>(data),
                    Path = new PropertyPath("")
                },
                CanUserReorder = true,
                CanUserSort = true,
                Header = data.DataTip
            };
            return column;
        }).Do(column => CourseGrid.Columns.Add(column));

        //Build search properties grid
        _searchProperties =
            (from searchProperty in from data in presentedDataList select ISearchProperty<TCourse>.CreateSearchProperty(data)
                where searchProperty != null
                select searchProperty).ToList();
        _searchProperties.ForEach(property =>
        {
            var searchPanel = property.GetSearchPanel();
            searchPanel.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    ApplyFilter();
                }
            };
            _searchPropertyGrid.Children.Add(searchPanel);
        });
        
        InitializeSearchApplyButton();

        //Add check box column
        AddGridCheckboxRowHeader();

        DataGridAttach.SetCanUnselectAllWithBlankArea(CourseGrid, true);
    }

    private void AddGridCheckboxRowHeader()
    {
        var headerBox = new FrameworkElementFactory(typeof(CheckBox));
        headerBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsSelected")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
        });
        CourseGrid.RowHeaderTemplate = new DataTemplate
        {
            VisualTree = headerBox
        };
    }

    private void InitializeSearchApplyButton()
    {
        _searchApplyButton.Content = CourseRush.Language.ui_button_apply;
        _searchApplyButton.Click += (_, _) => ApplyFilter();
        _searchApplyButton.Margin = new Thickness(0, 20, 0, 0);
        if (TryFindResource("ButtonPrimary") is Style style) _searchApplyButton.Style = style;
        _searchPropertyGrid.Children.Add(_searchApplyButton);
    }

    #region Custom Grid Wheel Handler
    private MouseWheelEventHandler? _mouseWheelEventHandler;
    private void UpdateScrollerListener()
    {
        var scrollViewer = GetScrollViewer(CourseGrid);
        if (scrollViewer == null) return;
        ScrollViewerWheelScrolling?.SetValue(scrollViewer, false);
        var scrollInfo = ScrollViewerScrollInfo?.GetValue(scrollViewer) as IScrollInfo;
        if (_mouseWheelEventHandler != null) CourseGrid.RemoveHandler(Mouse.MouseWheelEvent, _mouseWheelEventHandler);
        _mouseWheelEventHandler = (_, args) =>
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - args.Delta);
            }
            else
            {
                if (scrollInfo != null)
                {
                    if (args.Delta > 0)
                        scrollInfo.MouseWheelUp();
                    else
                        scrollInfo.MouseWheelDown();
                }
            }

            args.Handled = true;
        };
        CourseGrid.AddHandler(Mouse.MouseWheelEvent, _mouseWheelEventHandler, true);
        return;

        ScrollViewer? GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer obj) return obj;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
    #endregion
    public IReadOnlyList<TCourse> GetSelectedCourses()
    {
        return CourseGrid.SelectedItems.Cast<TCourse>().ToImmutableList();
    }

    public void AddCourse(IReadOnlyList<TCourse> courses)
    {
        foreach (var course in courses)
        {
            _courses.Add(course);
            _searchProperties.ForEach(property => property.UpdateData(course));
        }
        ApplyFilter();
        UpdateScrollerListener();
    }

    public void RefreshCourses(IReadOnlyList<TCourse> courses)
    {
        _courses.Clear();
        AddCourse(courses);
    }

    public IEnumerable<TCourse> GetCourses()
    {
        return _courses;
    }

    public void CheckCourseConflicts(Action<TCourse> conflictChecker)
    {
        foreach (var course in _courses)
        {
            conflictChecker(course);
        }
    }

    public override void SubscribeAutoFontResize(Action<AutoFontSizeChanged> register)
    {
        register(sizeFactor =>
        {
            CourseGrid.FontSize = 12 * sizeFactor;
            _searchApplyButton.FontSize = 16 * sizeFactor;
            _searchApplyButton.Height = 24 * sizeFactor;
            _searchPropertyGrid.SetValue(FontSizeProperty, 12 * sizeFactor);
        });
    }

    private bool _showFullCourses;
    private bool _showOnlyInClassCourses;
    private bool _showConflictCourses;
    private string? _className;
    public override void UpdateRecordFilter(bool showFullCourses, bool showOnlyInClassCourses, bool showConflictCourses, string? className)
    {
        var needUpdate = false;
        CompareAndUpdate(ref _showFullCourses, showFullCourses, ref needUpdate);
        CompareAndUpdate(ref _showOnlyInClassCourses, showOnlyInClassCourses, ref needUpdate);
        CompareAndUpdate(ref _showConflictCourses, showConflictCourses, ref needUpdate);
        CompareAndUpdate(ref _className, className, ref needUpdate);
        if (needUpdate) ApplyFilter();
    }

    private void ApplyFilter()
    {
        //Cache predicate in STA thread
        Predicate<TCourse> predicate = _ => true;
        if (!_showFullCourses)
        {
            predicate = predicate.And(course => course.SelectedStudentCount < course.TotalStudentCount);
        }

        if (_showOnlyInClassCourses && _className != null)
        {
            //TODO Ranged declaration support: 化工[2201-3]班
            predicate = predicate.And(course => course.ClassName.Contains(_className));
        }

        if (!_showConflictCourses)
        {
            predicate = predicate.And(course => course.ConflictsCache.Count == 0);
        }
        predicate = predicate.And(_searchProperties.Select(property => property.GetCurrentFilter()).Aggregate(CollectionUtils.AndCombine));
        //Run async filter in thread pool
        Task.Run(() =>
        {
            var courses = _courses.AsParallel().AsUnordered().WithMergeOptions(ParallelMergeOptions.AutoBuffered)
                .Where(course => predicate(course)).ToList();
            this.Invoke(() => CourseGrid.ItemsSource = new ObservableCollection<TCourse>(courses));
        });
    }

    public override void SetSearchPanelVisibility(bool visibility)
    {
        _searchPropertyGrid.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
        if (Content is Grid content)
        {
            content.ColumnDefinitions[2].Width = visibility ? new GridLength(1, GridUnitType.Star) : new GridLength(1, GridUnitType.Auto);    
        }
    }

    public override bool IsSearchPanelVisible()
    {
        return _searchPropertyGrid.Visibility == Visibility.Visible;
    }

    public override void ClearCourses()
    {
        _courses.Clear();
        CourseGrid.ItemsSource = new List<TCourse>();
    }
}

#region CourseAutoConverter
internal class PresentedCourseConverter<TCourse>(PresentedData<TCourse> data) : IValueConverter
    where TCourse : ICourse
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(string) && value is TCourse course)
        {
            return data.GetValue(course);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
#endregion