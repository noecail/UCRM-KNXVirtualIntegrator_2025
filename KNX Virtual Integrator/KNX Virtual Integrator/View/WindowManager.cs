using KNX_Virtual_Integrator.View.Windows;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View;

public class WindowManager
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- Propriétés  -------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// The MainWindow instance 
    /// </summary>
    public MainWindow MainWindow { get; }
    /// <summary>
    /// The SettingsWindow instance 
    /// </summary>
    public SettingsWindow SettingsWindow { get; }
    /// <summary>
    /// The ConnectionWindow instance 
    /// </summary>
    public ConnectionWindow ConnectionWindow { get; }
    /// <summary>
    /// The TestConfigWindow instance 
    /// </summary>
    public TestConfigWindow TestConfigWindow { get; }
    /// <summary>
    /// The ReportCreationWindow instance 
    /// </summary>
    public ReportCreationWindow ReportCreationWindow { get; }
    /// <summary>
    /// The StructureEditWindow instance 
    /// </summary>
    public StructureEditWindow StructureEditWindow {get;}
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- MÉTHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowManager"/> class.
    /// It instantiates every window once since they are never fully closed (only hidden).
    /// They all have the same MainViewModel to communicate effectively.
    /// </summary>
    public WindowManager(MainViewModel mainViewModel)
    {
        // Initialisation des fenêtres
        SettingsWindow = new SettingsWindow(mainViewModel);
        ConnectionWindow = new ConnectionWindow(mainViewModel);
        MainWindow = new MainWindow(mainViewModel, this);
        TestConfigWindow = new TestConfigWindow(mainViewModel);
        ReportCreationWindow = new ReportCreationWindow(mainViewModel);
        StructureEditWindow = new StructureEditWindow(mainViewModel);
    }
    
    /// <summary>
    /// Shows the Main Window, even when it is already open, with Focus().
    /// </summary>
    public void ShowMainWindow() {
        MainWindow.Show(); // Pour ouvrir la fenêtre
        MainWindow.Focus(); // Un peu inutile ici
    }

    /// <summary>
    /// Shows the Settings Window, even when it is already open, with Focus().
    /// </summary>
    public void ShowSettingsWindow() {
        SettingsWindow.Show();
        SettingsWindow.Focus(); // Pour s'assurer que la fenêtre se ré-affiche si nouvel appui sur bouton sans fermeture
    }
    
    /// <summary>
    /// Shows the Connection Window, even when it is already open, with Focus().
    /// </summary>
    public void ShowConnectionWindow() {
        ConnectionWindow.Show();
        ConnectionWindow.Focus();
    }

    /// <summary>
    /// Shows the Analysis/Test Configuration Window, even when it is already open, with Focus().
    /// </summary>
    public void ShowTestConfigWindow() {
        TestConfigWindow.Show();
        TestConfigWindow.Focus();
    }

    /// <summary>
    /// Shows the Analysis Report Creation Window, even when it is already open, with Focus().
    /// </summary>
    public void ShowReportCreationWindow() {
        ReportCreationWindow.Show();
        ReportCreationWindow.Focus();
    }

    /// <summary>
    /// Shows the Structure Edition Window, even when it is already open, with Focus().
    /// Stops the user from interacting with other windows as long as it is open with ShowDialog()
    /// </summary>
    public void ShowStructureEditWindow() {
        StructureEditWindow.ShowDialog(); //Pour empêcher l'intéraction avec le reste tant que la fenêtre est ouverte
        StructureEditWindow.Focus();
    }
}