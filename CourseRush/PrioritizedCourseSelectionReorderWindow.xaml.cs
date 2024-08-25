using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;

namespace CourseRush;

public partial class PrioritizedCourseSelectionReorderWindow : IDropTarget
{
    public bool IsAccepted { get; private set; }
    internal event Action<IDropInfo>? ItemDropped; 
    public PrioritizedCourseSelectionReorderWindow()
    {
        InitializeComponent();
        CourseList.View = new GridView();
    }

    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
        dropInfo.Effects = DragDropEffects.Move;
    }

    void IDropTarget.Drop(IDropInfo dropInfo)
    {
        ItemDropped?.Invoke(dropInfo);
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        IsAccepted = true;
        Close();
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        IsAccepted = false;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                break;
            case Key.Enter:
                IsAccepted = true;
                Close();
                break;
        }
    }
}