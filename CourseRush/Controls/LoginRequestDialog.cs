using System.Windows;
using CourseRush.Auth;
using CourseRush.Models;
using Resultful;

namespace CourseRush.Controls;

public class LoginRequestDialog(LoginRequestDialogViewModel viewModel) : LoginRequestDialogBase(viewModel)
{

    protected override void UpdateOkResult()
    {
        viewModel.Result = Result.Ok<AuthError>();
    }

    protected override void UpdateCancelResult()
    {
        viewModel.Result = new AuthError("User cancelled operation").Fail();
    }
}