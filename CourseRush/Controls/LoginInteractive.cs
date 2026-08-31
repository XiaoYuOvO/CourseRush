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

    public async Task<Result<string, AuthError>> RequestActionWithPayload(NamedAction? action, TranslatableText title,
        TranslatableText description)
    {
        return await Dialog
            .Show(new LoginRequestDialogWithPayload(new LoginRequestDialogViewModel<string>(
                title.Translate(Language.ResourceManager), description.Translate(Language.ResourceManager), action)))
            .GetResultAsync<Result<string, AuthError>>();
    }

    public async Task<VoidResult<AuthError>> RequestAction(NamedAction? action, TranslatableText title,
        TranslatableText description)
    {
        return await Dialog
            .Show(new LoginRequestDialog(new LoginRequestDialogViewModel(title.Translate(Language.ResourceManager),
                description.Translate(Language.ResourceManager), action))).GetResultAsync<VoidResult<AuthError>>();
    }

    public void ShowInfo(TranslatableText message)
    {
        Application.Current.Invoke(() => Growl.Info(message.Translate(Language.ResourceManager)));
    }
}