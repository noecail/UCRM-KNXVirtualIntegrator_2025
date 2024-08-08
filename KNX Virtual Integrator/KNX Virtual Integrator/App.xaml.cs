using System.Diagnostics;
using System.Windows;

namespace KNX_Virtual_Integrator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    
    /// <summary>
    /// Manages project files, providing functionality to handle project-related file operations.
    /// </summary>
    public static ProjectFileManager? Fm { get; private set; } // Gestionnaire de fichiers du projet
    
    /// <summary>
    /// Manages the application's display elements, including windows, buttons, and other UI components.
    /// </summary>
    public static DisplayElements? DisplayElements { get; private set; } // Gestionnaire de l'affichage (contient les fenetres, boutons, ...)
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Fonction s'executant e l'ouverture de l'application
    /// <summary>
    /// Executes when the application starts up.
    /// <para>
    /// This method performs the following tasks:
    /// <list type="bullet">
    ///     <item>
    ///         Creates a directory for log files if it does not already exist.
    ///     </item>
    ///     <item>
    ///         Initializes the log file path and sets up the <see cref="_writer"/> for logging.
    ///     </item>
    ///     <item>
    ///         Logs the start-up process of the application.
    ///     </item>
    ///     <item>
    ///         Initializes and displays the main window and updates related UI components.
    ///     </item>
    ///     <item>
    ///         Opens the project file manager.
    ///     </item>
    ///     <item>
    ///         Attempts to archive old log files and cleans up folders from the last session.
    ///     </item>
    ///     <item>
    ///         Logs a message indicating that the application has started successfully and performs garbage collection.
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="e">An instance of <see cref="StartupEventArgs"/> that contains the event data.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // J'AI ENLEVE TOUTE LA GESTION DES LOGS + VERSIONNAGE? A RAJOUTER
        
        base.OnStartup(e);
            
        var currentProcess = Process.GetCurrentProcess();
        currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal; // JSP SI C'EST PERTINENT ICI
      
        // Ouverture la fenetre principale
        ConsoleAndLogWriteLine("Opening main window");
        DisplayElements = new DisplayElements();
  
        // Affichage de la fenêtre principale
        DisplayElements.ShowMainWindow();

        // Ouverture du gestionnaire de fichiers de projet
        ConsoleAndLogWriteLine("Opening project file manager");
        Fm = new ProjectFileManager();
<<<<<<< HEAD
     
=======


        // Tentative d'archivage des fichiers de log
        ConsoleAndLogWriteLine("Trying to archive log files");
        ArchiveLogs();


        // Nettoyage des dossiers restants de la derniere session
        ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
        DeleteAllExceptLogsAndResources();

        // CheckForUpdatesAsync();

        ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
        ConsoleAndLogWriteLine("-----------------------------------------------------------");
        
>>>>>>> 909de65f463c9a83a512371a7f9ecefc3c1dfa7f
        // Appel au garbage collector pour nettoyer les variables issues 
        GC.Collect();
    }

    // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'ecrivant dans les
    // logs. Ajoute la date et l'heure avant affichage. Saut d'une ligne en fin de message.
    /// <summary>
    /// Writes a message to the application console and log file, including the current date and time, and appends a newline after the message.
    /// <para>
    /// This method performs the following tasks:
    /// <list type="bullet">
    ///     <item>
    ///         Writes the provided message to the console with a timestamp (date and time) at the beginning, followed by a newline.
    ///     </item>
    ///     <item>
    ///         If the console window is visible, scrolls to the end of the console text to ensure that the latest message is displayed.
    ///     </item>
    ///     <item>
    ///         Writes the same message to the log file with a timestamp (date and time) at the beginning, followed by a newline.
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="msg">The message to be written to the console and log file.</param>
    public static void ConsoleAndLogWriteLine(string msg)
    {
        Console.WriteLine($@"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
        
        // A RAJOUTER UNE FOIS LES LOGS GERES
        //_writer?.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
    }
}