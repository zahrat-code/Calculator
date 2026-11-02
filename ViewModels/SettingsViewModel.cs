namespace WindowsCalculatorMaui.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _selectedTheme;

    public SettingsViewModel()
    {
        _selectedTheme = Preferences.Get("AppTheme", "System");
    }

    public bool IsLightSelected
    {
        get => _selectedTheme == "Light";
        set { if (value) ApplyTheme("Light"); }
    }

    public bool IsDarkSelected
    {
        get => _selectedTheme == "Dark";
        set { if (value) ApplyTheme("Dark"); }
    }

    public bool IsSystemSelected
    {
        get => _selectedTheme == "System";
        set { if (value) ApplyTheme("System"); }
    }

    private void ApplyTheme(string theme)
    {
        _selectedTheme = theme;
        Preferences.Set("AppTheme", theme);

        // Apply instantly without restart
        Application.Current!.UserAppTheme = theme switch
        {
            "Light"  => AppTheme.Light,
            "Dark"   => AppTheme.Dark,
            _        => AppTheme.Unspecified
        };

        OnPropertyChanged(nameof(IsLightSelected));
        OnPropertyChanged(nameof(IsDarkSelected));
        OnPropertyChanged(nameof(IsSystemSelected));
    }
}

