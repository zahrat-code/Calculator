namespace WindowsCalculatorMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Restore saved theme before creating the shell
        string saved = Preferences.Get("AppTheme", "System");
        Application.Current!.UserAppTheme = saved switch
        {
            "Light" => AppTheme.Light,
            "Dark"  => AppTheme.Dark,
            _       => AppTheme.Unspecified   // "System"
        };

        MainPage = new AppShell();
    }
}
