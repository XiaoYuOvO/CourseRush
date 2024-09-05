using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CourseRush.Core;
using CourseRush.Core.Task;
using CourseRush.Core.Util;
using HandyControl.Controls;
using HandyControl.Tools.Extension;

namespace CourseRush.Models;

public interface ISelectionSessionPageModel
{
    DataGrid GetDataGrid();
    Task SaveSessionsAsync(FileStream fileStream);
    Task LoadSessionsAsync(Stream openFile);
}

public class SelectionSessionPageModel<TCourseSelection, TError> : ISelectionSessionPageModel
    where TCourseSelection : class, ISelectionSession, IJsonSerializable<TCourseSelection, TError>
    where TError : BasicError, ICombinableError<TError>
{
    private readonly ObservableCollection<TCourseSelection> _selections;
    private readonly DataGrid _grid;
    public event Action<TCourseSelection>? OnSessionSelected;

    public SelectionSessionPageModel(List<PresentedData<TCourseSelection>> presentedData)
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
                OnSessionSelected?.Invoke(courseSelection);
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

    public async Task SaveSessionsAsync(FileStream fileStream)
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
        await Task.Run(()=>new JsonArray(_selections.Select(session => session.ToJson()).Cast<JsonNode>().ToArray()).WriteTo(writer, jsonSerializerOptions));
    }

    public async Task LoadSessionsAsync(Stream openFile)
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
                    select TCourseSelection.FromJson(node as JsonObject)).CombineResults())).Tee(
                selectionSessions =>
            {
                foreach (var session in selectionSessions)
                {
                    _selections.Add(session);
                }
                
                Growl.Info(string.Format(Language.ui_message_sessions_import_success, selectionSessions.Count));
            }).TeeError(e => Growl.Error(e.Message));
        }
        catch (JsonException e)
        {
            Growl.Error(e.Message);
        }
    }
}

public class PresentedSessionConverter<TCourseSelection>(PresentedData<TCourseSelection> data) : IValueConverter
    where TCourseSelection : ISelectionSession
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(string) && value is TCourseSelection selection)
        {
            return data.GetValue(selection);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}