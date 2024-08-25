using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CourseRush.Core.Task;
using CourseRush.Models;
using MahApps.Metro.Controls;

namespace CourseRush.Controls;

public partial class TaskTreeView
{
    private readonly Action<AutoFontSizeChanged>? _registerer;
    private readonly ControlTemplate? _itemTemplate;
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TaskTreeViewItem(_itemTemplate, _registerer);
    }

    public TaskTreeView(Action<AutoFontSizeChanged>? registerer = null)
    {
        _registerer = registerer;
        InitializeComponent();
        _itemTemplate = FindResource("TaskTreeViewItemTemplate") as ControlTemplate;
        Style = FindResource("TreeViewBaseStyle") as Style;
        
    }
}

internal class TaskTreeViewItem : TreeViewItem
    {
        private readonly Action<AutoFontSizeChanged>? _registerer;
        private TaskStatus _status = TaskStatus.Waiting;
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty && e.NewValue is ISelectionTask task)
            {
                _status = task.Status;
                task.StatusChanged += status =>
                {
                    _status = status;
                    this.Invoke(UpdateBackground);
                };
            } else if (e.Property == IsMouseOverProperty || e.Property == IsSelectedProperty || e.Property == Selector.IsSelectionActiveProperty)
            {
                UpdateBackground();
            }
        }
        private readonly Brush? _primaryBrush;
        private readonly Brush? _darkDefaultBrush;
        private readonly Brush? _regionBrush;

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TaskTreeViewItem(Template, _registerer);
        }
        public TaskTreeViewItem(ControlTemplate? template, Action<AutoFontSizeChanged>? registerer)
        {
            _registerer = registerer;
            registerer?.Invoke(factor => FontSize = 14 * factor);
            _primaryBrush = FindResource("PrimaryBrush") as Brush;
            _darkDefaultBrush = FindResource("DarkDefaultBrush") as Brush;
            _regionBrush = FindResource("RegionBrush") as Brush;
            Style = FindResource("TreeViewItemBaseStyle") as Style;
            Template = template;
        }
        
        

        private void UpdateBackground()
        {
            Brush? defaultBackground;
            Brush? selectedBackground;
            Brush? mouseOverBackground;
            if (_status == TaskStatus.Failed)
            {
                defaultBackground = Brushes.Firebrick;
                mouseOverBackground = Brushes.Red;
                selectedBackground = Brushes.Crimson;
            }else if (_status == TaskStatus.Completed)
            {
                defaultBackground = Brushes.DarkGreen;
                mouseOverBackground = Brushes.Green;
                selectedBackground = Brushes.ForestGreen;
            }else if (_status == TaskStatus.Paused)
            {
                defaultBackground = Brushes.Gray;
                mouseOverBackground = Brushes.Silver;
                selectedBackground = Brushes.DarkGray;
            }else if (_status == TaskStatus.Running || _status == TaskStatus.Next) 
            {
                defaultBackground = Brushes.Blue;
                mouseOverBackground = Brushes.CornflowerBlue;
                selectedBackground = Brushes.RoyalBlue;
            }else
            {
                selectedBackground = _primaryBrush;
                mouseOverBackground = _darkDefaultBrush;
                defaultBackground = _regionBrush;
            }
            
            if (IsSelected && !(GetValue(IsSelectionActiveProperty) as bool? ?? true))
            {
                Background =  _darkDefaultBrush;
                return;
            }
            if (IsSelected)
            {
                Background = selectedBackground;
            }else if (IsMouseOver)
            {
                Background = mouseOverBackground;    
            }
            else
            {
                Background = defaultBackground;
            }
            
        }
    }