using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View;

public class WindowManager (MainViewModel mainViewModel)
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Represents the main window instance of the application.
    /// </summary>
    public MainWindow MainWindow { get; } = new(mainViewModel);
        
    /// <summary>
    /// Represents the settings window instance where application settings are configured.
    /// </summary>
    public SettingsWindow? SettingsWindow { get; } = new(mainViewModel);
    
    // /// <summary>
    // /// Represents the connection window instance.
    // /// </summary>
    // public ConnectionWindow? ConnectionWindow { get; } = new(mainViewModel);

        
    /* ------------------------------------------------------------------------------------------------
   --------------------------------------------  METHODES  --------------------------------------------
   ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Shows the main window of the application.
    /// </summary>
    public void ShowMainWindow()
    {
        MainWindow.Show();
    }
        
        
    /// <summary>
    /// Shows the settings window of the application if it is available.
    /// </summary>
    public void ShowSettingsWindow()
    {
        SettingsWindow?.Show();
    }
    
    
    // /// <summary>
    // /// Shows the connection window of the application if it is available.
    // /// </summary>
    // public void ShowConnectionWindow()
    // {
    //     ConnectionWindow?.Show();
    // }
}