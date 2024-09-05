using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;

namespace CourseRush;

public partial class ReloginDialog
{
    public string Message { get; } = "Relogging in";

    public ReloginDialog()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Dialog.Close("MainWindow");
    }
}