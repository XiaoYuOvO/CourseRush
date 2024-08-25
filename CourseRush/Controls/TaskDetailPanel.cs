using System;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Core;
using CourseRush.Core.Task;
using CourseRush.Models;
using HandyControl.Controls;
using HandyControl.Data;
using MahApps.Metro.Controls;
using ComboBox = System.Windows.Controls.ComboBox;
using NumericUpDown = HandyControl.Controls.NumericUpDown;
using TextBox = System.Windows.Controls.TextBox;

namespace CourseRush.Controls;

public class TaskDetailPanel<TError, TCourse> : DockPanel where TError : BasicError, ISelectionError, ICombinableError<TError> where TCourse : ICourse, IJsonSerializable<TCourse, TError>
{
    private SelectionTask<TError, TCourse>? _selectedTask;
    public event Action<SelectionTask<TError, TCourse>>? OnTaskCanceled; 
    public event Action<TCourse>? OnRequestShowCourse; 
    public TaskDetailPanel(Action<AutoFontSizeChanged> fontRegister)
    {
        VerticalAlignment = VerticalAlignment.Stretch;
        
        var taskInfoPanel = new UniformSpacingPanel
        {
            Margin = new Thickness(0,16,0,0),
            Spacing = 16,
            Orientation = Orientation.Vertical,
            ItemHorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var taskOperationPanel = new UniformSpacingPanel
        {
            Margin = new Thickness(0, 16, 0, 0),
            Spacing = 16,
            Orientation = Orientation.Vertical,
            ItemHorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        SetDock(taskInfoPanel, Dock.Top);
        SetDock(taskOperationPanel, Dock.Bottom);
        Children.Add(taskInfoPanel);
        Children.Add(taskOperationPanel);
        var statusLabel = CreateTextBoxWithTitle(taskInfoPanel, CourseRush.Language.ui_label_task_status);
        var courseInfoLabel = CreateTextBoxWithTitle(taskInfoPanel, CourseRush.Language.ui_label_target_course_info);
        var showCourseInfoButton = new Button
        {
            Content = CourseRush.Language.ui_button_show_course_detail,
            Style = taskInfoPanel.FindResource("ButtonPrimary") as Style,
            Height = double.NaN,
            Margin = new Thickness(10,10,10,10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Visibility = Visibility.Collapsed
        };
        taskOperationPanel.Children.Add(showCourseInfoButton);
        var taskControlPanel = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)}
            }
        };
        
        taskOperationPanel.Children.Add(taskControlPanel);
        var pauseResumeButton = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = double.NaN,
            Content = CourseRush.Language.ui_button_pause,
            IsEnabled = false,
            Margin = new Thickness(10,0,4,0)
        };
        Grid.SetColumn(pauseResumeButton, 0);
        taskControlPanel.Children.Add(pauseResumeButton);
        
        var cancelDeleteButton = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = double.NaN,
            Content = CourseRush.Language.ui_button_cancel,
            Style = taskOperationPanel.FindResource("ButtonDanger") as Style,
            IsEnabled = false,
            Margin = new Thickness(4,0,10,0)
        };
        Grid.SetColumn(cancelDeleteButton, 1);
        taskControlPanel.Children.Add(cancelDeleteButton);
        
        taskInfoPanel.Children.Add(statusLabel);
        taskInfoPanel.Children.Add(courseInfoLabel);
        var retryOptionBox = CreateComboBoxWithTitle(taskInfoPanel, CourseRush.Language.ui_label_retry_option);
        retryOptionBox.ItemsSource = RetryOption.Values;
        taskInfoPanel.Children.Add(retryOptionBox);
        var retryIntervalEditor = CreateNumbericWithTitle(taskInfoPanel, CourseRush.Language.ui_label_retry_interval);
        taskInfoPanel.Children.Add(retryIntervalEditor);
        
        fontRegister(factor =>
        {
            SetValue(Control.FontSizeProperty, 14 * factor);
            pauseResumeButton.FontSize =  16 * factor;
            cancelDeleteButton.FontSize =  16 * factor;
            showCourseInfoButton.FontSize =  16 * factor;
        });
        
        retryOptionBox.SelectionChanged += (_, _) =>
        {
            if (_selectedTask != null && retryOptionBox.SelectedItem is RetryOption retryOption)
            {
                _selectedTask.RetryOption = retryOption;
            }
        };
        retryIntervalEditor.ValueChanged += (_, _) =>
        {
            if (_selectedTask != null)
            {
                _selectedTask.RetryInterval = (int)retryIntervalEditor.Value;
            }
        };
        
        cancelDeleteButton.Click += (_, _) =>
        {
            if (_selectedTask == null) return;
            if (_selectedTask.Status.IsFinished())
            {
                OnTaskCanceled?.Invoke(_selectedTask);
            }
            else
            {
                _selectedTask.Cancel();
                cancelDeleteButton.Content = CourseRush.Language.ui_button_delete;
            }
        };
        
        pauseResumeButton.Click += (_, _) =>
        {
            if (_selectedTask == null) return;
            if (_selectedTask.IsPaused)
            {
                _selectedTask.Resume();
                pauseResumeButton.Content = CourseRush.Language.ui_button_pause;
            }
            else
            {
                _selectedTask.Pause();
                pauseResumeButton.Content = CourseRush.Language.ui_button_resume;
            }
        };
        
        showCourseInfoButton.Click += (_, _) =>
        {
            if (_selectedTask is SingleSelectionTask<TError, TCourse> task)
            {
                OnRequestShowCourse?.Invoke(task.TargetCourse);
            }
        };
        var statusChanged = void (TaskStatus status) =>
        {
            this.Invoke(() =>
            {
                if (_selectedTask == null || !statusLabel.IsEnabled) return;
                statusLabel.Text = _selectedTask.Status.LocalizedName;
                UpdateOperationButton(_selectedTask);
            });
        };

        
        
        taskInfoPanel.DataContextChanged += (_, args) =>
        {
            if (_selectedTask != null && _selectedTask != args.NewValue)
            {
                _selectedTask.StatusChanged -= statusChanged;
            }
            switch (_selectedTask = args.NewValue as SelectionTask<TError, TCourse>)
            {
                case SingleSelectionTask<TError, TCourse> singleSelectionTask:
                    UpdateDefaultElements(singleSelectionTask);
                    courseInfoLabel.Text = singleSelectionTask.TargetCourse.ToSimpleString();
                    courseInfoLabel.Visibility = Visibility.Visible;
                    showCourseInfoButton.Visibility = Visibility.Visible;
                    break;
                case { } task:
                    UpdateDefaultElements(task);
                    break;
                case null:
                    courseInfoLabel.Visibility = Visibility.Collapsed;
                    showCourseInfoButton.Visibility = Visibility.Collapsed;
                    statusLabel.IsEnabled = false;
                    retryIntervalEditor.IsEnabled = false;
                    retryOptionBox.IsEnabled = false;
                    pauseResumeButton.IsEnabled = false;
                    cancelDeleteButton.IsEnabled = false;
                    statusLabel.Text = "";
                    retryIntervalEditor.Value = 0;
                    retryOptionBox.SelectedItem = null;
                    break;
            }
        };
        return;

        void UpdateDefaultElements(SelectionTask<TError, TCourse> task)
        {
            _selectedTask = task;
            statusLabel.IsEnabled = true;
            courseInfoLabel.IsEnabled = true;
            retryIntervalEditor.IsEnabled = true;
            retryOptionBox.IsEnabled = true;
            
            UpdateOperationButton(task);
            
            pauseResumeButton.Content = _selectedTask.IsPaused ? CourseRush.Language.ui_button_resume : CourseRush.Language.ui_button_pause;
            cancelDeleteButton.IsEnabled = true;
            statusLabel.Text = task.Status.LocalizedName;
            retryOptionBox.SelectedItem = task.RetryOption;
            retryIntervalEditor.Value = task.RetryInterval;
            task.StatusChanged += statusChanged;
            courseInfoLabel.Visibility = Visibility.Collapsed;
            showCourseInfoButton.Visibility = Visibility.Collapsed;
        }

        void UpdateOperationButton(SelectionTask<TError, TCourse> task)
        {
            if (task.Status.IsFinished())
            {
                pauseResumeButton.IsEnabled = false;
                cancelDeleteButton.Content = CourseRush.Language.ui_button_delete;
            }
            else
            {
                pauseResumeButton.IsEnabled = true;
                cancelDeleteButton.Content = CourseRush.Language.ui_button_cancel;
            }
        }
    }
    
    private static TextBox CreateTextBoxWithTitle(FrameworkElement element, string title)
    {
        var statusLabel = new TextBox
        {
            TextAlignment = TextAlignment.Justify,
            TextWrapping = TextWrapping.Wrap,
            IsReadOnly = true,
            IsEnabled = false,
            Style = element.FindResource("TextBoxExtend") as Style,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 60
        };
        TitleElement.SetTitle(statusLabel, title);
        TitleElement.SetTitlePlacement(statusLabel, TitlePlacementType.Left);
        TitleElement.SetTitleWidth(statusLabel, new GridLength(1, GridUnitType.Star));
        return statusLabel;
    }
    
    private static NumericUpDown CreateNumbericWithTitle(FrameworkElement element, string title)
    {
        var numericUpDown = new NumericUpDown
        {
            Style = element.FindResource("NumericUpDownExtend") as Style,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 60,
            Increment = 1,
            DecimalPlaces = 0,
            IsEnabled = false,
            Minimum = 0,
        };
        TitleElement.SetTitle(numericUpDown, title);
        TitleElement.SetTitlePlacement(numericUpDown, TitlePlacementType.Left);
        TitleElement.SetTitleWidth(numericUpDown, new GridLength(1, GridUnitType.Star));
        return numericUpDown;
    }

    private static ComboBox CreateComboBoxWithTitle(FrameworkElement element, string title)
    {
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 60,
            IsEnabled = false,
            IsReadOnly = true,
            Style = element.FindResource("ComboBoxExtend") as Style
        };
        TitleElement.SetTitle(comboBox, title);
        TitleElement.SetTitlePlacement(comboBox, TitlePlacementType.Left);
        TitleElement.SetTitleWidth(comboBox, new GridLength(1, GridUnitType.Star));
        return comboBox;   
    }
}