using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Core;
using HandyControl.Controls;
using HandyControl.Data;
using ComboBox = System.Windows.Controls.ComboBox;

namespace CourseRush.Controls;

public interface ISearchProperty<in TCourse> where TCourse : ICourse
{
    
    public static ISearchProperty<TCourse>? CreateSearchProperty(PresentedData<TCourse> data)
    {
        return data.SearchType switch
        {
            EnumSearchType<TCourse> enumType => new EnumSearchProperty<TCourse>(enumType),
            SearchType<TCourse, string> stringType => new StringSearchProperty<TCourse>(stringType),
            SearchType<TCourse, int> intType => new IntSearchProperty<TCourse>(intType),
            SearchType<TCourse, float> floatType => new FloatSearchProperty<TCourse>(floatType),
            _ => null
        };
    }
    
    public UIElement GetSearchPanel();
    public Predicate<TCourse> GetCurrentFilter();
    public void UpdateData(TCourse course);
}

public abstract class SearchProperty<TCourse, TElement> : ISearchProperty<TCourse> where TCourse : ICourse where TElement : UIElement
{
    private readonly DockPanel _searchPanel = new(){VerticalAlignment = VerticalAlignment.Stretch};
    private readonly CheckBox _enabled = new(){VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0,10,0,0)};
    protected readonly TElement InputElement;
    protected abstract Predicate<TCourse> GetFilter();

    public Predicate<TCourse> GetCurrentFilter()
    {
        return _enabled.IsChecked ?? false ? GetFilter() : _ => true;
    }

    public abstract void UpdateData(TCourse course);
    
    protected SearchProperty(TElement inputElement, string tip)
    {
        InputElement = inputElement;
        InputElement.IsEnabled = false;
        var textBlock = new TextBlock { Text = tip, TextAlignment = TextAlignment.Justify, VerticalAlignment = VerticalAlignment.Center,  HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(5,10,0,0)};
        var dockPanel = new DockPanel();
        _searchPanel.Children.Add(_enabled);
        _searchPanel.Children.Add(dockPanel);
        dockPanel.Children.Add(textBlock);
        dockPanel.Children.Add(inputElement);
        
        DockPanel.SetDock(_enabled, Dock.Left);
        DockPanel.SetDock(dockPanel, Dock.Right);
        DockPanel.SetDock(textBlock, Dock.Left);
        DockPanel.SetDock(inputElement, Dock.Right);
        AddEnableListener();
    }

    private void AddEnableListener()
    {
        _searchPanel.MouseLeftButtonDown += (_, _) =>
        {
            InputElement.IsEnabled = !InputElement.IsEnabled;
            _enabled.IsChecked = !_enabled.IsChecked;
        };
        
        _searchPanel.MouseRightButtonDown += (_, _) =>
        {
            InputElement.IsEnabled = false;
            _enabled.IsChecked = false;
        };
        _enabled.Click += (_, _) => InputElement.IsEnabled = _enabled.IsChecked ?? false;
    }

    public UIElement GetSearchPanel()
    {
        return _searchPanel;
    }
}

public class EnumSearchProperty<TCourse> : SearchProperty<TCourse, ComboBox> where TCourse : ICourse
{
    private readonly ObservableCollection<string> _enumStrings = new();
    private readonly SearchType<TCourse, string> _enumType;

    public EnumSearchProperty(EnumSearchType<TCourse> enumType) : base(new(){HorizontalAlignment = HorizontalAlignment.Stretch,Margin = new Thickness(5,10,5,0)}, enumType.DataTip)
    {
        _enumType = enumType;
        // var resource = InputElement.TryFindResource("ComboBoxExtend");
        // if (resource is Style style)
        // {
        //     InputElement.Style = style;    
        // }
        //  
        // TitleElement.SetTitle(InputElement, enumType.DataTip);
        // TitleElement.SetTitlePlacement(InputElement, TitlePlacementType.Left);
        InputElement.ItemsSource = _enumStrings;
    }

    protected override Predicate<TCourse> GetFilter()
    {
        var enumBoxSelectionBoxItem = InputElement.SelectedItem as string;
        //This predicate may invoke in other non-STA threads, so we need to cache the values
        return course => _enumType.Extract(course).Trim().Equals(enumBoxSelectionBoxItem, StringComparison.Ordinal);
    }

    public override void UpdateData(TCourse course)
    {
        var trim = _enumType.Extract(course).Trim();
        if (!_enumStrings.Contains(trim))
        {
            _enumStrings.Add(trim);
        }
    }
}

public class StringSearchProperty<TCourse> : SearchProperty<TCourse, AutoCompleteTextBox> where TCourse : ICourse
{
    private readonly SearchType<TCourse, string> _stringType;
    private readonly ObservableCollection<string> _strings = new();
    public StringSearchProperty(SearchType<TCourse, string> stringType) : base(new(){Margin = new Thickness(5,10,5,0), HorizontalAlignment = HorizontalAlignment.Stretch},stringType.DataTip)
    {
        _stringType = stringType;
        InputElement.ItemsSource = _strings;
    }

    protected override Predicate<TCourse> GetFilter()
    {
        var text = InputElement.Text;
        //This predicate may invoke in other non-STA threads, so we need to cache the values
        return course => _stringType.Extract(course).Trim().Contains(text);
    }

    public override void UpdateData(TCourse course)
    {
        if (_strings.Count > 300) return;
        var trim = _stringType.Extract(course).Trim();
        if (!_strings.Contains(trim))
        {
            _strings.Add(trim);
        }
    }
}

public class IntSearchProperty<TCourse> : SearchProperty<TCourse, RangeSlider> where TCourse : ICourse
{
    private readonly SearchType<TCourse, int> _searchType;
    private int _max;
    private int _min;

    public IntSearchProperty(SearchType<TCourse, int> searchType) : base(new(){IsSnapToTickEnabled = true, TickFrequency = 1, Margin = new Thickness(5,10,5,0), HorizontalAlignment = HorizontalAlignment.Stretch}, searchType.DataTip)
    {
        _searchType = searchType;
        // TipElement.SetPlacement(InputElement, PlacementType.Top);
        // TipElement.SetVisibility(InputElement, Visibility.Visible);
        // TitleElement.SetTitle(InputElement, searchType.DataTip);
        // TitleElement.SetTitlePlacement(InputElement, TitlePlacementType.Left);
    }

    protected override Predicate<TCourse> GetFilter()
    {
        var startCache = InputElement.ValueStart;
        var endCache = InputElement.ValueEnd;
        //This predicate may invoked in other non-STA threads so we need to cache the values
        return course =>
        {
            var extract = _searchType.Extract(course);
            return startCache <= extract && extract <= endCache;
        };
    }

    public override void UpdateData(TCourse course)
    {
        var lastMax = _max;
        var lastMin = _min;
        _max = Math.Max(_max, _searchType.Extract(course));
        _min = Math.Min(_min, _searchType.Extract(course));
        if (lastMax != _max)
        {
            InputElement.Maximum = _max;
        }

        if (lastMin != _min)
        {
            InputElement.Minimum = _min;
        }
    }
}

public class FloatSearchProperty<TCourse> : SearchProperty<TCourse, RangeSlider> where TCourse : ICourse
{
    private readonly SearchType<TCourse, float> _searchType;
    private float _max;
    private float _min;

    public FloatSearchProperty(SearchType<TCourse, float> searchType) : base(new(){IsSnapToTickEnabled = false, TickFrequency = 10, Margin = new Thickness(5,10,5,0), HorizontalAlignment = HorizontalAlignment.Stretch}, searchType.DataTip)
    {
        _searchType = searchType;
        TipElement.SetPlacement(InputElement, PlacementType.Top);
        TipElement.SetVisibility(InputElement, Visibility.Visible);
        TipElement.SetStringFormat(InputElement, "#0.00");
        TitleElement.SetTitle(InputElement, searchType.DataTip);
        TitleElement.SetTitlePlacement(InputElement, TitlePlacementType.Left);
    }

    protected override Predicate<TCourse> GetFilter()
    {
        var startCache = InputElement.ValueStart;
        var endCache = InputElement.ValueEnd;
        //This predicate may invoked in other non-STA threads so we need to cache the values
        return course =>
        {
            var extract = _searchType.Extract(course);
            return startCache <= extract && extract <= endCache;
        };
    }

    public override void UpdateData(TCourse course)
    {
        var lastMax = _max;
        var lastMin = _min;
        _max = Math.Max(_max, _searchType.Extract(course));
        _min = Math.Min(_min, _searchType.Extract(course));
        if (Math.Abs(lastMax - _max) > 0.001)
        {
            InputElement.Maximum = _max;
        }

        if (Math.Abs(lastMin - _min) > 0.001)
        {
            InputElement.Minimum = _min;
        }
    }
}