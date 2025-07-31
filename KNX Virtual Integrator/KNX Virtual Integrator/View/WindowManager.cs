using KNX_Virtual_Integrator.View.Windows;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View;

public class WindowManager
{
    public MainWindow MainWindow { get; }
    public SettingsWindow SettingsWindow { get; }
    public ConnectionWindow ConnectionWindow { get; }
    public TestConfigWindow TestConfigWindow { get; }
    public ReportCreationWindow ReportCreationWindow { get; }
    public ModelEditWindow ModelEditWindow {get;}
    
    
    public WindowManager(MainViewModel mainViewModel)
    {
        // Initialisation des fenêtres
        SettingsWindow = new SettingsWindow(mainViewModel);
        ConnectionWindow = new ConnectionWindow(mainViewModel);
        MainWindow = new MainWindow(mainViewModel, this);
        TestConfigWindow = new TestConfigWindow(mainViewModel);
        ReportCreationWindow = new ReportCreationWindow(mainViewModel);
        ModelEditWindow = new ModelEditWindow(mainViewModel);
    }

    public void ShowMainWindow() {
        MainWindow.Show(); // Pour ouvrir la fenêtre
        MainWindow.Focus();
        
    }

    public void ShowSettingsWindow() {
        SettingsWindow.ShowDialog(); //Pour empêcher l'intéraction avec le reste tant que la fenêtre est ouverte
        SettingsWindow.Focus(); // Pour s'assurer que la fenêtre se ré-affiche si nouvel appui sur bouton sans fermeture
    }

    public void ShowConnectionWindow() {
        ConnectionWindow.Show();
        ConnectionWindow.Focus();
    }

    public void ShowTestConfigWindow() {
        TestConfigWindow.Show();
        TestConfigWindow.Focus();
    }

    public void ShowReportCreationWindow() {
        ReportCreationWindow.Show();
        ReportCreationWindow.Focus();
    }

    public void ShowModelEditWindow() {
        ModelEditWindow.ShowDialog();
        ModelEditWindow.Focus();
    }
}