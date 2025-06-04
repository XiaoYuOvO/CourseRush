using System;
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
using System.Windows.Documents;
using System.Windows.Media;
using CourseRush.Controls;
using CourseRush.Core;
using CourseRush.Core.Task;
using HandyControl.Controls;
using MahApps.Metro.Controls;
using Resultful;
using JsonException = Newtonsoft.Json.JsonException;
using TaskStatus = CourseRush.Core.Task.TaskStatus;

namespace CourseRush.Models;

public interface ICourseSelectionQueuePageModel
{
    public TaskTreeView CreateTaskTreeView(Border taskDetailPanel, Action<AutoFontSizeChanged> fontRegister);
    public RichTextBox GetLogTextBlock();
    public CourseDetailDrawer CreateCourseDetailDrawer();
    Task LoadTasksAsync(Stream openFile);
    Task SaveTasksAsync(Stream writeFile);
    void RemoveAllFinished();
    void RemoveAll();
    void PauseAll(); 
    void ResumeAll();
    void SetAutoStart(bool isChecked);
}

public class CourseSelectionQueuePageModel<TError, TCourse> : ICourseSelectionQueuePageModel, ITaskLogger
    where TCourse : ICourse, IPresentedDataProvider<TCourse>, IJsonSerializable<TCourse, TError>
    where TError : BasicError, ISelectionError, ICombinableError<TError>
{
    private readonly ObservableCollection<SelectionTask<TError, TCourse>> _tasks = [];
    private readonly CourseDetailDrawer<TCourse> _courseDetailDrawer = new();
    private readonly Paragraph _logParagraph = new();
    private readonly RichTextBox _loggerTextBlock;
    private readonly Action<AutoFontSizeChanged> _registerer;
    private readonly Action<SelectionTask<TError, TCourse>> _taskSubmit;
    private TaskDetailPanel<TError, TCourse>? _taskDetailPanel;
    
    private Option<ICourseSelector<TError, TCourse>> _selector;
    private bool _startAfterAdd = true;

    public CourseSelectionQueuePageModel(Action<AutoFontSizeChanged> registerer,
        Action<SelectionTask<TError, TCourse>> taskSubmit)
    {
        _registerer = registerer;
        _taskSubmit = taskSubmit;
        _loggerTextBlock = new RichTextBox
        {
            Document = new FlowDocument { Blocks = { _logParagraph } },
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            MinWidth = 300,
            IsReadOnly = true
        };
        _loggerTextBlock.MouseDown += (_, _) => _loggerTextBlock.MinWidth = _loggerTextBlock.RenderSize.Width;
        _loggerTextBlock.TextChanged += (_, _) => _loggerTextBlock.MinWidth = _loggerTextBlock.RenderSize.Width;
    }

    public VoidResult<UiError> SubmitTask(SelectionTask<TError, TCourse> task)
    {
        
        return _selector.Tee(client =>
        {
            _tasks.Insert(0, task);
            if (_startAfterAdd) _ = task.InitializeAndStartTask(client);
        }).Match(_ => Result.Ok<UiError>(),
            _ => new UiError("Failed to submit task because the client is not present").Fail());
    }

    public void SetClient(ICourseSelector<TError, TCourse> client)
    {
        _selector = client.ToOption();
        if (_taskDetailPanel != null) _taskDetailPanel.Selector = _selector;
    }

    public void ApplyCourseConflict(TCourse course)
    {
        foreach (var selectionTask in _tasks)
        {
            if (selectionTask.Status != TaskStatus.Cancelled &&
                selectionTask.Status != TaskStatus.Completed &&
                selectionTask.Status != TaskStatus.Failed)
            {
                selectionTask.ApplyCourseConflict(course);
            }
        }
    }

    public TaskTreeView CreateTaskTreeView(Border taskDetailPanel, Action<AutoFontSizeChanged> fontRegister)
    {
        var treeView = new TaskTreeView(_registerer);
        var frameworkElementFactory = new FrameworkElementFactory(typeof(TextBlock));
        frameworkElementFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(SelectionTask<TError, TCourse>.Name)));
        var aggregatedTemplate = new HierarchicalDataTemplate
        {
            DataType = typeof(AggregatedSelectionTask<TError, TCourse>),
            VisualTree = frameworkElementFactory,
            ItemsSource = new Binding(nameof(AggregatedSelectionTask<TError, TCourse>.SubTasks)),
        };
        frameworkElementFactory = new FrameworkElementFactory(typeof(TextBlock));
        frameworkElementFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(SelectionTask<TError, TCourse>.Name)));
        frameworkElementFactory.SetBinding(TextBlock.FontSizeProperty, new Binding
        {
            Source = treeView,
            Path = new PropertyPath("FontSize"),
        });
        var taskTemplate = new DataTemplate
        {
            DataType = typeof(SelectionTask<TError, TCourse>),
            VisualTree = frameworkElementFactory
        };
        var taskTemplateSelector = new TaskTemplateSelector(aggregatedTemplate, taskTemplate);
        treeView.ItemTemplateSelector = taskTemplateSelector;
        treeView.SelectedItemChanged += (_, args) => taskDetailPanel.Child.SetValue(FrameworkElement.DataContextProperty, args.NewValue);
        taskDetailPanel.Child = _taskDetailPanel = CreateTaskDetailPanel(fontRegister);
        treeView.ItemsSource = _tasks;
        return treeView;
    }

    public RichTextBox GetLogTextBlock()
    {
        return _loggerTextBlock;
    }

    public CourseDetailDrawer CreateCourseDetailDrawer()
    {
        return _courseDetailDrawer;
    }

    public async Task LoadTasksAsync(Stream openFile)
    {
        try
        {
            if (await JsonNode.ParseAsync(openFile) is not JsonArray taskArray)
            {
                Growl.Error(Language.ui_message_tasks_file_not_array);
                return;
            }

            (await Task.Run(()=>TaskTypes<TError, TCourse>.DeserializeAll(taskArray))).Tee(tasks =>
            {
                foreach (var selectionTask in tasks)
                {
                    _taskSubmit(selectionTask);
                }

                Growl.Info(string.Format(Language.ui_message_tasks_import_success, tasks.Count));
            }).TeeError(e => Growl.Error(e.Message));
        }
        catch (JsonException e)
        {
            Growl.Error(e.Message);
        }
    }


    public async Task SaveTasksAsync(Stream openFile)
    {
        var javaScriptEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        await using var writer = new Utf8JsonWriter(openFile, new JsonWriterOptions
        {
            Encoder = javaScriptEncoder,
            Indented = true
        });
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = javaScriptEncoder,
            WriteIndented = true
        };
        await Task.Run(()=>TaskTypes<TError, TCourse>.SerializeAll(_tasks).WriteTo(writer, jsonSerializerOptions));
    }

    public void RemoveAllFinished()
    {
        foreach (var selectionTask in _tasks.Where(task => task.Status.IsFinished()).ToList())
        {
            _tasks.Remove(selectionTask);
            if (!selectionTask.Status.IsFinished()) selectionTask.Cancel();
        }
    }

    public void RemoveAll()
    {
        foreach (var selectionTask in _tasks)
        {
            selectionTask.Pause();
            selectionTask.Cancel();
        }
        _tasks.Clear();
    }

    public void PauseAll()
    {
        foreach (var selectionTask in _tasks)
        {
            selectionTask.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (var selectionTask in _tasks)
        {
            selectionTask.Resume();
        }
    }

    public void SetAutoStart(bool isChecked)
    {
        _startAfterAdd = isChecked;
    }

    private TaskDetailPanel<TError, TCourse> CreateTaskDetailPanel(Action<AutoFontSizeChanged> fontRegister)
    {
        var taskDetailPanel = new TaskDetailPanel<TError, TCourse>(fontRegister);
        taskDetailPanel.OnTaskCanceled += task => _tasks.Remove(task);
        taskDetailPanel.OnRequestShowCourse += _courseDetailDrawer.ShowCourse;
        return taskDetailPanel;
    }

    private class TaskTemplateSelector(HierarchicalDataTemplate aggregatedTemplate, DataTemplate taskTemplate) : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object? item, DependencyObject container)
        {
            return item is AggregatedSelectionTask<TError, TCourse> ? aggregatedTemplate : taskTemplate;
        }
    }

    
    public void LogInfo(ISelectionTask task, string message)
    {
        _loggerTextBlock.Invoke(() =>
        {
            _logParagraph.Inlines.Add(new Run(FormatMessage(task, message)));
            _logParagraph.Inlines.Add(new LineBreak());
        });
    }

    public void LogWarn(ISelectionTask task, string message)
    {
        _loggerTextBlock.Invoke(() =>
        {
            _logParagraph.Inlines.Add(new Run(FormatMessage(task, message)) { Foreground = Brushes.Orange });
            _logParagraph.Inlines.Add(new LineBreak());
        });
    }

    public void LogError(ISelectionTask task, string message)
    {
        _loggerTextBlock.Invoke(() =>
        {
            _logParagraph.Inlines.Add(new Run(FormatMessage(task, message)) { Foreground = Brushes.Red });
            _logParagraph.Inlines.Add(new LineBreak());
        });
    }

    public void LogError(ISelectionTask task, string message, BasicError error)
    {
        _loggerTextBlock.Invoke(() =>
        {
            _logParagraph.Inlines.Add(new Run(FormatMessage(task, $"{message}:\n    {error.Message}")) { Foreground = Brushes.Red });
            _logParagraph.Inlines.Add(new LineBreak());
        });
    }

    private static string FormatMessage(ISelectionTask task, string message)
    {
        return $"[{DateTime.Now:T}] [{task.Name}]: {message}";
    }
}