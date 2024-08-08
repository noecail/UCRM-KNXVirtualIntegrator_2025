namespace KNX_Virtual_Integrator;

public class DisplayElements
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Represents the main window instance of the application.
    /// </summary>
    public MainWindow MainWindow { get; } = new();
        
    /// <summary>
    /// Represents the settings window instance where application settings are configured.
    /// </summary>
    public SettingsWindow? SettingsWindow { get; } = new();
    
        
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
}