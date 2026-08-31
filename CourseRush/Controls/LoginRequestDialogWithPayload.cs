using System.Windows;
using System.Windows.Controls;
using CourseRush.Auth;
using CourseRush.Models;
using Resultful;
using TextBox = HandyControl.Controls.TextBox;

namespace CourseRush.Controls;

public class LoginRequestDialogWithPayload : LoginRequestDialogBase
{
    private readonly LoginRequestDialogViewModel<string> _viewModel;
    private readonly TextBox _smsCodeBox = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(5,5,5,5)
    };
    public LoginRequestDialogWithPayload(LoginRequestDialogViewModel<string> viewModel) : base(viewModel)
    {
        _viewModel = viewModel;
        ContentPanel.ColumnDefinitions.Insert(0, new ColumnDefinition(){Width = new GridLength(1, GridUnitType.Star)});
        ContentPanel.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
        ContentPanel.Children.Insert(0, _smsCodeBox);
    }

    protected override void UpdateOkResult()
    {
        _viewModel.Result = _smsCodeBox.Text.Ok<string, AuthError>();
    }

    protected override void UpdateCancelResult()
    {
        _viewModel.Result = new AuthError("User cancelled SMS code input");
    }
}