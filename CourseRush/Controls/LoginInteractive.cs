using System.Threading.Tasks;
using System.Windows;
using CourseRush.Auth;
using CourseRush.Core.Util;
using CourseRush.Models;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Resultful;

namespace CourseRush.Controls;

public class LoginInteractive : IAuthInteractive
{
    public static readonly LoginInteractive Instance = new();
    private LoginInteractive()
    {
    }

    public async Task<Result<string, AuthError>> RequestActionWithPayload(NamedAction? action, string title,
        string description)
    {
        return await Dialog.Show(new LoginRequestDialogWithPayload(new LoginRequestDialogViewModel<string>(title, description, action))).GetResultAsync<Result<string, AuthError>>();
    }

    public async Task<VoidResult<AuthError>> RequestAction(NamedAction? action, string title, string description)
    {
        return await Dialog.Show(new LoginRequestDialog(new LoginRequestDialogViewModel(title, description, action))).GetResultAsync<VoidResult<AuthError>>();
    }

    public void ShowInfo(string message)
    {
        Application.Current.Invoke(() => Growl.Info(message));
    }
}