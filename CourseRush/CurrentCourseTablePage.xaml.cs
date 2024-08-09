using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CourseRush.Core;
using CourseRush.HNU;

namespace CourseRush;

public partial class CurrentCourseTablePage
{
    private readonly CourseTable _courseTable;

    public CurrentCourseTablePage()
    {
        InitializeComponent();
        _courseTable = new CourseTable(new WeekTimeTable(ImmutableList.CreateRange(new List<LessonTime>
        {
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50)),
            new(new TimeOnly(8,0), new TimeOnly(8,50))
        })));
        Content = _courseTable;
        _courseTable.VerticalAlignment = VerticalAlignment.Stretch;
        _courseTable.HorizontalAlignment = HorizontalAlignment.Stretch;
        using var fileStream = File.Open("H:\\CSharpeProjects\\CourseRush\\hnu_courses_type1.json", FileMode.Open);
        var array = JsonNode.Parse(fileStream)?["data"]?["showKclist"]?.AsArray();
        if (array is null) return;
        var courseList = new List<HNUCourse>();
        foreach (var jsonNode in array)
        {
            if (jsonNode == null) continue;
            HNUCourse.FromJson(jsonNode.AsObject()).Tee(course =>
            {
                courseList.Add(course);
            });
        }

        var index = 0;
        MouseDown += (_, _) =>
        {
            _courseTable.RemoveAll();
            var orangeRed = Color.FromArgb(100, (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255),(byte)Random.Shared.Next(255));
            orangeRed.A = 100;
            _courseTable.AddCourse(courseList[index], orangeRed, 4);
            _courseTable.UpdateDisplay();
            index++;
        };
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _courseTable.RenderSize = sizeInfo.NewSize;
    }
}