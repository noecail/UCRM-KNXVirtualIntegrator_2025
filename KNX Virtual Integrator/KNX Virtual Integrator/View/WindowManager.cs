using KNX_Virtual_Integrator.View.Windows;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View;

public class WindowManager
{
    private readonly MainViewModel _mainViewModel;
    public MainWindow MainWindow { get; }
    public SettingsWindow? SettingsWindow { get; }
    public ConnectionWindow? ConnectionWindow { get; private set; }

    public WindowManager(MainViewModel mainViewModel)
    {
        // Initialisation du viewmodel
        _mainViewModel = mainViewModel;
        
        
        // Initialisation des fenêtres
        SettingsWindow = new SettingsWindow(_mainViewModel);
        ConnectionWindow = new ConnectionWindow(_mainViewModel);
        MainWindow = new MainWindow(_mainViewModel, ConnectionWindow);
    }

    public void ShowMainWindow() => MainWindow.Show();
    public void ShowSettingsWindow() => SettingsWindow?.Show();
    public void ShowConnectionWindow() => ConnectionWindow?.Show();
}