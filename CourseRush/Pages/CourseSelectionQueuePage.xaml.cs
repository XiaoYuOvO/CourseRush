using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Models;
using Microsoft.Win32;

namespace CourseRush.Pages;

public partial class CourseSelectionQueuePage
{
    private readonly ICourseSelectionQueuePageModel _model;
    public CourseSelectionQueuePage(ICourseSelectionQueuePageModel model, Action<AutoFontSizeChanged> registerer)
    {
        _model = model;
        InitializeComponent();
        var courseDetailDrawer = _model.CreateCourseDetailDrawer();
        // Grid.SetRow(courseDetailDrawer, 3);
        Grid.Children.Add(courseDetailDrawer);
        var taskTreeView = _model.CreateTaskTreeView(TaskDetailPanel, registerer);
        Grid.SetRow(taskTreeView, 1);
        Grid.SetColumn(taskTreeView, 0);
        Grid.Children.Add(taskTreeView);
        var logTextBlock = _model.GetLogTextBlock();
        LoggerViewer.Content = logTextBlock;
        registerer(factor =>
        {
            logTextBlock.FontSize = 14 * factor;
            taskTreeView.FontSize = 15 * factor;
            InfoHeader.FontSize = 16 * factor;
            TaskDetailPanel.SetValue(FontSizeProperty, 15 * factor);
            courseDetailDrawer.SetFontSize(16 * factor, 14 * factor);
            FilesMenu.FontSize = 14 * factor;
        });
    }

    private async void Export_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = false,
            CheckFileExists = false,
            DefaultExt = ".json"
        };
        if (!(openFileDialog.ShowDialog() ?? false)) return;
        await using var fileStream = File.Open(openFileDialog.FileName, FileMode.Create, FileAccess.Write);
        await _model.SaveTasksAsync(fileStream);
    }

    private async void Import_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = false,
            ShowReadOnly = true,
            CheckFileExists = true, 
            DefaultExt = ".json"
        };
        if (!(openFileDialog.ShowDialog() ?? false)) return;
        await using var openFile = openFileDialog.OpenFile();
        await _model.LoadTasksAsync(openFile);
    }
}