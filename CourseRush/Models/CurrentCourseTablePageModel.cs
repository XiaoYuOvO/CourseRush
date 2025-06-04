using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CourseRush.Controls;
using CourseRush.Core;
using CourseRush.Core.Util;
using HandyControl.Controls;
using HandyControl.Data;
using MahApps.Metro.Controls;
using MessageBox = HandyControl.Controls.MessageBox;
using ScrollViewer = System.Windows.Controls.ScrollViewer;

namespace CourseRush.Models;

public interface ICurrentCourseTablePageModel
{
    public CourseTable CourseTable { get; }
    public bool DisplayNextWeek();
    public bool CanDisplayNextWeek();
    public bool DisplayPrevWeek();
    public bool CanDisplayPrevWeek();
    public void DisplayCoursesAtCurrentWeek();
    public int GetCurrentWeek();
    public void DisplayCompressedCourses();
    Task SaveCoursesAsync(FileStream fileStream);
    Task LoadCoursesAsync(Stream openFile);
    public void ClearCourses();
    public void UpdateTimeTable(WeekTimeTable timeTable);

    public CourseDetailDrawer CreateCourseDetailDrawer();
    public void ConfigureOffTableCoursesDrawer(Drawer drawer);
}

public class CurrentCourseTablePageModel<TCourse, TError>(Action<TCourse> removeSelection) : ICurrentCourseTablePageModel
    where TCourse : class, ICourse, IPresentedDataProvider<TCourse>, IJsonSerializable<TCourse, TError> where TError : BasicError, ICombinableError<TError>
{
    public CourseTable CourseTable { get; } = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private int _maxWeek;
    private int _currentWeek = 1;
    private bool _isDisplayingCompressed;
    private List<TCourse> Courses { get; } = [];
    private readonly List<CourseWeeklyTime> _compressedTimesCache = [];
    private readonly ObservableCollection<TCourse> _offTableCourses = [];

    public void UpdateTimeTable(WeekTimeTable timeTable)
    {
        CourseTable.UpdateTimeTable(timeTable);
    }

    private Action<ICourse>? _showCourseInfoDrawer;
    private Button? _removeSelectionButton;

    public CourseDetailDrawer CreateCourseDetailDrawer()
    {
        var courseDetailDrawer = new CourseDetailDrawer<TCourse>();
        _removeSelectionButton = new Button
        {
            Content = Language.ui_button_remove_selection,
            Style = courseDetailDrawer.FindResource("ButtonDanger") as Style,
            Height = double.NaN,
            Margin = new Thickness(10,10,10,10),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _removeSelectionButton.Click += (_, _) =>
        {
            if (courseDetailDrawer.ContentPanel.DataContext is TCourse course && MessageBox.Show(new MessageBoxInfo
                {
                    Caption = Language.ui_title_remove_selection_confirm,
                    Message = Language.ui_label_remove_selection_confirm,
                    Button = MessageBoxButton.OKCancel,
                    IconBrushKey = ResourceToken.InfoBrush,
                    IconKey = ResourceToken.AskGeometry,
                }) == MessageBoxResult.OK)
            {
                removeSelection(course);
            }
        };
        courseDetailDrawer.ContentPanel.Children.Add(_removeSelectionButton);

        _showCourseInfoDrawer = selectedCourse =>
        {
            if (selectedCourse is TCourse course)
            {
                courseDetailDrawer.ShowCourse(course);    
            }
        };
        CourseTable.OnCourseClick += (_, courseWeeklyTime) =>
        {
            courseWeeklyTime.BindingCourse.Tee(_showCourseInfoDrawer);
        };
        return courseDetailDrawer;
    }



    public void ConfigureOffTableCoursesDrawer(Drawer drawer)
    {
        var scrollViewer = (drawer.Content as UIElement).FindChild<ScrollViewer>("OffTableCoursesViewer");
        if (scrollViewer == null) return;
        var contentPanel = new ListView();
        var drawerHeader = (drawer.Content as UIElement).FindChild<TextBlock>("OffTableCoursesHeader");
        if (drawerHeader != null)
            drawerHeader.Text = Language.ui_label_off_table_courses;
        var gridView = new GridView();
        contentPanel.View = gridView;
        foreach (var presentedData in TCourse.GetSimplePresentedData())
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

        contentPanel.MouseDoubleClick += (_, args) =>
        {
            var clickedItem = GetClickedListViewItem(args.OriginalSource as DependencyObject);
            if (clickedItem is { Content: TCourse course})
            {
                _showCourseInfoDrawer?.Invoke(course);
            }
        };

        contentPanel.ItemsSource = _offTableCourses;
        scrollViewer.Content = contentPanel;
        CourseTable.OnOffTableCourseButtonClick += (_, _) => drawer.IsOpen = true;
        return;

        ListViewItem? GetClickedListViewItem(DependencyObject? obj)
        {
            while (obj != null && obj is not ListViewItem)
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as ListViewItem;
        }
    }

    public void RegisterAutoFontSize(Action<AutoFontSizeChanged> registerer)
    {
        registerer(factor =>
        {
            CourseTable.FontSize = 15 * factor;
            CourseTable.UpdateFontSize(CourseTable.FontSize);
            _removeSelectionButton?.SetValue(Control.FontSizeProperty, 16 * factor);
        });
    }

    public void AddCourses(IReadOnlyList<TCourse> courses)
    {
        Courses.AddRange(courses);
        foreach (var course in courses.Where(course => course.TimeTable.WeeklyInformation.Count == 0))
        {
            _offTableCourses.Add(course);
        }
        _compressedTimesCache.AddRange(courses.Select(course => course.TimeTable.GetCompressedTime()).Presented());
        _maxWeek = Math.Max(_maxWeek,
            courses.Select(course => course.TimeTable.WeeklyInformation.Count != 0 ? course.TimeTable.WeeklyInformation.Select(weeklyTime => weeklyTime.TeachingWeek.Count != 0 ? weeklyTime.TeachingWeek.Max() : 0).Max() : 0).Max());
        if (_isDisplayingCompressed)  DisplayCompressedCourses();
        else DisplayCoursesAtCurrentWeek();
    }

    public void ResolveCourseConflictWithCurrent(ICourse course)
    {
        foreach (var currentCourse in Courses)
        {
            course.TimeTable.ResolveConflictWith(currentCourse.TimeTable).Tee(results => course.ConflictsCache[currentCourse] = results);
        }
    }

    public void DisplayCoursesAtCurrentWeek()
    {
        CourseTable.RemoveAll();
        Courses.ForEach(course => CourseTable.AddCourse(course, RandomColor(), _currentWeek));
        CourseTable.UpdateDisplay();
        _isDisplayingCompressed = false;
    }

    public bool DisplayNextWeek()
    {
        _currentWeek++;
        DisplayCoursesAtCurrentWeek();
        return CanDisplayNextWeek();
    }
    
    
    public bool CanDisplayNextWeek()
    {
        return _currentWeek < _maxWeek;
    }

    
    public bool DisplayPrevWeek()
    {
        _currentWeek--;
        DisplayCoursesAtCurrentWeek();
        return CanDisplayPrevWeek();
    }

    public bool CanDisplayPrevWeek()
    {
        return _currentWeek > 1;
    }

    public int GetCurrentWeek()
    {
        return _currentWeek;
    }

    public void DisplayCompressedCourses()
    {
        CourseTable.RemoveAll();
        _compressedTimesCache.ForEach(time => CourseTable.AddCourse(time, RandomColor()));
        CourseTable.UpdateDisplay();
        _isDisplayingCompressed = true;
    }

    public async Task SaveCoursesAsync(FileStream fileStream)
    {
        var javaScriptEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        await using var writer = new Utf8JsonWriter(fileStream, new JsonWriterOptions
        {
            Encoder = javaScriptEncoder,
            Indented = true
        });
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = javaScriptEncoder,
            WriteIndented = true
        };
        await Task.Run(()=>new JsonArray(Courses.Select<TCourse, JsonNode>(TCourse.ToJson).ToArray()).WriteTo(writer, jsonSerializerOptions));
    }

    public async Task LoadCoursesAsync(Stream openFile)
    {
        try
        {
            if (await JsonNode.ParseAsync(openFile) is not JsonArray sessionArray)
            {
                Growl.Error(Language.ui_messsage_sessions_file_not_array);
                return;
            }

            (await Task.Run(() =>
                (from node in sessionArray
                    where node is JsonObject
                    select TCourse.FromJson(node as JsonObject)).CombineResults())).Tee(
                selectionSessions =>
                {
                    AddCourses(selectionSessions);
                    Growl.Info(string.Format(Language.ui_message_sessions_import_success, selectionSessions.Count));
                }).TeeError(e => Growl.Error(e.Message));
        }
        catch (JsonException e)
        {
            Growl.Error(e.Message);
        }
    }

    public void ClearCourses()
    {
        Courses.Clear();
        _offTableCourses.Clear();
        _compressedTimesCache.Clear();
        CourseTable.RemoveAll();
        CourseTable.UpdateDisplay();
    }

    private static Color RandomColor()
    {
        return Color.FromArgb(100, (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255),
            (byte)Random.Shared.Next(255));
    }
}