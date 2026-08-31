using System.Windows;
using CourseRush.Models;

namespace CourseRush.Controls;

public abstract partial class LoginRequestDialogBase
{
    private readonly LoginRequestDialogViewModelBase _dialogResult;

    protected LoginRequestDialogBase(LoginRequestDialogViewModelBase dialogResult)
    {
        InitializeComponent();
        _dialogResult = dialogResult;
        DataContext = _dialogResult;
        TitleLabel.Text = dialogResult.Title;
    }
    
    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        UpdateCancelResult();
        _dialogResult.CloseAction?.Invoke();        
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        UpdateOkResult();
        _dialogResult.CloseAction?.Invoke();  
    }

    protected abstract void UpdateOkResult();
    protected abstract void UpdateCancelResult();

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        _dialogResult.PerformAction?.Action.Invoke();
    }
}