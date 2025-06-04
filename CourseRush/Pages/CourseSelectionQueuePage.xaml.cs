using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Models;
using Microsoft.Win32;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CourseRush.Pages;

public partial class CourseSelectionQueuePage
{
    private readonly ICourseSelectionQueuePageModel _model;
    private RichTextBox _loggerTextBlock;
    public CourseSelectionQueuePage(ICourseSelectionQueuePageModel model, Action<AutoFontSizeChanged> registerer)
    {
        _model = model;
        InitializeComponent();
        var courseDetailDrawer = _model.CreateCourseDetailDrawer();
        Grid.Children.Add(courseDetailDrawer);
        var taskTreeView = _model.CreateTaskTreeView(TaskDetailPanel, registerer);
        Grid.SetRow(taskTreeView, 1);
        Grid.SetColumn(taskTreeView, 0);
        Grid.Children.Add(taskTreeView);
        _loggerTextBlock = _model.GetLogTextBlock();
        LoggerViewer.Content = _loggerTextBlock;
        registerer(factor =>
        {
            _loggerTextBlock.FontSize = 14 * factor;
            taskTreeView.FontSize = 15 * factor;
            InfoHeader.FontSize = 16 * factor;
            TaskDetailPanel.SetValue(FontSizeProperty, 15 * factor);
            courseDetailDrawer.SetFontSize(16 * factor, 14 * factor);
            ToolBar.FontSize = 14 * factor;
            RemoveAllFinished.FontSize = 14 * factor;
            PauseAll.FontSize = 14 * factor;
            ResumeAll.FontSize = 14 * factor;
            RemoveAll.FontSize = 14 * factor;
            AutoStartCheck.FontSize = 14 * factor;
        });
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _loggerTextBlock.MinWidth = _loggerTextBlock.RenderSize.Width;
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

    private void RemoveAllFinished_OnClick(object sender, RoutedEventArgs e) => _model.RemoveAllFinished();

    private void PauseAll_OnClick(object sender, RoutedEventArgs e) => _model.PauseAll();

    private void ResumeAll_OnClick(object sender, RoutedEventArgs e) => _model.ResumeAll();

    private void RemoveAll_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(CourseRush.Language.ui_message_confirm_remove_all, button:MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
            _model.RemoveAll();
        }
    }

    private void AutoStart_OnClick(object sender, RoutedEventArgs e)
    {
        _model.SetAutoStart(AutoStartCheck.IsChecked ?? false);
    }
}