using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CourseRush.Core;
using CourseRush.Core.Util;
using HandyControl.Tools.Extension;
using Color = System.Windows.Media.Color;

namespace CourseRush;

public partial class CourseTable
{
    public delegate void CourseClickHandler(object sender, CourseWeeklyTime course);
    private readonly Dictionary<CourseWeeklyTime, Color> _displayingTimes = new();
    public event CourseClickHandler? OnCourseClick; 
    public CourseTable(WeekTimeTable timeTable)
    {
        //TODO Font resize by the table render size
        InitializeComponent();
        TableGrid.RowDefinitions.Add(new RowDefinition
        {
            Height = new GridLength(1, GridUnitType.Auto)
        });
        for (var index = 0; index < timeTable.Lessons.Count; index++)
        {
            var timeTableLesson = timeTable.Lessons[index];
            TableGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });
            var textBlock = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 15,
                Foreground = MondayHeader.Foreground
            };
            textBlock.Inlines.Add((index + 1).ToString());
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(timeTableLesson.Start.ToString());
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(timeTableLesson.End.ToString());
            Grid.SetColumn(textBlock, 0);
            Grid.SetRow(textBlock, index + 1);
            TableGrid.Children.Add(textBlock);
        }

        var currentCultureDateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        MondayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Monday);
        TuesdayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Tuesday);
        WednesdayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Wednesday);
        ThursdayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Thursday);
        FridayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Friday);
        SaturdayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Saturday);
        SundayHeader.Text = currentCultureDateTimeFormat.GetDayName(DayOfWeek.Sunday);
    }

    public void AddCourse<TCourse>(TCourse course, Color color, int weekNumber) where TCourse : ICourse
    {
        course.TimeTable.GetTimeTableAtWeek(weekNumber).Tee(time => _displayingTimes[time] = color);
    }

    public void RemoveAll()
    {
        _displayingTimes.Clear();
    }

    public void UpdateDisplay()
    {
        //Clear the former items
        TableGrid.Children.OfType<Grid>().Cast<UIElement>().Do(TableGrid.Children.Remove);
        
        //Map all lessons to grid cords
        var dayRanges = new Dictionary<DayOfWeek, List<Tuple<Range, CourseWeeklyTime>>>();
        foreach (var (time, _) in _displayingTimes)
        {
            foreach (var (day, lessons) in time.WeeklySchedule)
            {
                if (dayRanges.ContainsKey(day))
                {
                    dayRanges[day].AddRange(CollectionUtils.FindRanges(lessons).Select(range => new Tuple<Range, CourseWeeklyTime>(range, time)));
                }
                else
                {
                    dayRanges[day] = CollectionUtils.FindRanges(lessons).Select(range => new Tuple<Range, CourseWeeklyTime>(range, time)).ToList();
                }
            }
        }
        
        //Add to display
        foreach (var (day, tuples) in dayRanges)
        {
            foreach (var (range, times) in CollectionUtils.DistinctRanges(tuples))
            {
                var lessonPanel = BuildDisplayPanelForCourses(times.Select(time => new Tuple<CourseWeeklyTime, Color>(time, _displayingTimes[time])).ToList());
                Grid.SetRow(lessonPanel, range.Start.Value);
                Grid.SetColumn(lessonPanel, day.GetAsIndex() + 1);
                Grid.SetRowSpan(lessonPanel, range.End.Value - range.Start.Value + 1);
                TableGrid.Children.Add(lessonPanel);
            }
        }
    }

    private Grid BuildDisplayPanelForCourses(IReadOnlyCollection<Tuple<CourseWeeklyTime, Color>> weeklyTimes)
    {
        var grid = new Grid();
        var gridIndex = 0;
        weeklyTimes.Select(courseAndColor =>
            {
                var textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment= VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = MondayHeader.Foreground
                };
                courseAndColor.Item1.BindingCourse.Tee(course =>
                {
                    textBlock.Inlines.Add(course.CourseName);
                    textBlock.Inlines.Add(new LineBreak());
                    textBlock.Inlines.Add(course.TeacherName);
                    textBlock.Inlines.Add(new LineBreak());
                });
                textBlock.Inlines.Add(courseAndColor.Item1.TeachingLocation);
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(courseAndColor.Item1.TeachingCampus);
                
                var border = new Border
                {
                    Background = new SolidColorBrush(courseAndColor.Item2),
                    CornerRadius = new CornerRadius(10, 10, 10, 10),
                    Child = textBlock,
                    Margin = new Thickness(5,5,5,5),
                };
                border.MouseLeftButtonDown += (_, _) =>
                {
                    OnCourseClick?.Invoke(this, courseAndColor.Item1);
                };
                return border;
        }).SelectMany((item, index) => index == weeklyTimes.Count - 1 ? new[] { item } : new FrameworkElement[] { item, new Separator() })
            .Do(item =>
            {
                grid.ColumnDefinitions.Add(item is Separator
                    ? new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
                    : new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetColumn(item, gridIndex++);
                grid.Children.Add(item);
            });
        return grid;
    }
}