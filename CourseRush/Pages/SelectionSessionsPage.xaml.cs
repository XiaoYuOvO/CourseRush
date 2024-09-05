using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Models;
using Microsoft.Win32;

namespace CourseRush.Pages;

public partial class SelectionSessionsPage
{
    private readonly Action _reloadSessionsFunc;
    private readonly ISelectionSessionPageModel _model;
    public SelectionSessionsPage(ISelectionSessionPageModel model, Action reloadSessionsFunc, Action<AutoFontSizeChanged> fontSizeSubscription)
    {
        InitializeComponent();
        _model = model;
        _reloadSessionsFunc = reloadSessionsFunc;
        var dataGrid = model.GetDataGrid();
        fontSizeSubscription(factor =>
        {
            FilesMenu.FontSize = 14 * factor;
            RefreshButton.FontSize = 16 * factor;
            dataGrid.FontSize = 14 * factor;
        });
        Grid.SetRow(dataGrid, 1);
        Grid.Children.Add(dataGrid);
    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
        _reloadSessionsFunc();
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
        await _model.SaveSessionsAsync(fileStream);
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
        await _model.LoadSessionsAsync(openFile);
    }
}