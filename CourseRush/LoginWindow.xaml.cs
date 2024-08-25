using System;
using System.Threading;
using System.Windows;
using CourseRush.Auth;
using HandyControl.Controls;
using MahApps.Metro.Controls;
using Resultful;

namespace CourseRush;

public partial class LoginWindow
{
    public LoginWindow()
    {
        InitializeComponent();
        UniversitySelection.ItemsSource = Universities.GetAllUniversities();
        var password = Environment.GetEnvironmentVariable("COURSE_RUSH_PASSWORD");
        var username = Environment.GetEnvironmentVariable("COURSE_RUSH_USERNAME");
        if (password == null || username == null) return;
        PasswordBox.Password = password;
        UsernameBox.Text = username;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = false;
        var option = (UniversitySelection.SelectionBoxItem as UniversityInfo)?.ToOption<UniversityInfo>();
        var usernameBoxText = UsernameBox.Text;
        var passwordBoxPassword = PasswordBox.Password;
        if (usernameBoxText.Length == 0 || passwordBoxPassword.Length == 0 || !option.HasValue)
        {
            Growl.Error(CourseRush.Language.ui_error_empty_username_password);
            LoginButton.IsEnabled = true;
            return;
        }
        new Thread(() =>
        {
            option.Value.Tee(info =>
            {
                Universities
                    .LoginAndGetMainWindowModelFromId(info.Id, new UsernamePassword(usernameBoxText, passwordBoxPassword))
                    .Tee(model => this.Invoke(() =>
                    {
                        var mainWindow = new MainWindow(model.Invoke());
                        mainWindow.Top = Top + Height / 2 - mainWindow.Height / 2;
                        mainWindow.Left = Left + Width / 2 - mainWindow.Width / 2;
                        mainWindow.Show();
                        mainWindow.Title = $"COURSE RUSH >>> {info.Id}";
                        Close();
                    })).TeeError(error => this.Invoke(()=>
                    {
                        Growl.Error(error.Message);
                        LoginButton.IsEnabled = true;
                    }));
            });
        }).Start();
    }
}