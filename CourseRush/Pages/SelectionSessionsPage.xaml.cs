using System;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Models;

namespace CourseRush.Pages;

public partial class SelectionSessionsPage
{
    private readonly Action _reloadSessionsFunc;
    public SelectionSessionsPage(ISelectionSessionPageModel model, Action reloadSessionsFunc, Action<AutoFontSizeChanged> fontSizeSubscription)
    {
        InitializeComponent();
        _reloadSessionsFunc = reloadSessionsFunc;
        var dataGrid = model.GetDataGrid();
        fontSizeSubscription(factor =>
        {
            FilesMenu.FontSize = 14 * factor;
            RefreshButton.FontSize = 14 * factor;
            dataGrid.FontSize = 14 * factor;
        });
        Grid.SetRow(dataGrid, 1);
        Grid.Children.Add(dataGrid);
    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
        _reloadSessionsFunc();
    }

    private void Export_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private void Import_OnClick(object sender, RoutedEventArgs e)
    {
        
    }
}