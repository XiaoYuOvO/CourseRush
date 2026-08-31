using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CourseRush.Auth;
using CourseRush.Core.Util;
using HandyControl.Tools.Extension;
using Resultful;

namespace CourseRush.Models;

public partial class LoginRequestDialogViewModelBase(string title, string? description, NamedAction? performAction) : ObservableObject
{
    [ObservableProperty]
    private string _title = title;
    [ObservableProperty]
    private string? _description = description;
    [ObservableProperty]
    private NamedAction? _performAction = performAction;
    public Action? CloseAction { get; set; }
}

public class LoginRequestDialogViewModel<T>(string title, string? description, NamedAction? performAction)
    : LoginRequestDialogViewModelBase(title, description, performAction), IDialogResultable<Result<T, AuthError>>
{
    public Result<T, AuthError> Result { get; set; }
}

public class LoginRequestDialogViewModel(string title, string? description, NamedAction? performAction)
    : LoginRequestDialogViewModelBase(title, description, performAction), IDialogResultable<VoidResult<AuthError>>
{
    public VoidResult<AuthError> Result { get; set; }
}