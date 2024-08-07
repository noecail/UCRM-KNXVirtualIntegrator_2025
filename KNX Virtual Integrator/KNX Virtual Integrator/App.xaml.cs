using System.Diagnostics;
using System.IO;
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
    /// Represents the name of the application.
    /// </summary>
    public const string AppName = "KNX Virtual Integrator"; // Nom de l'application
    
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
    ///         Logs the start-up process of the application.
    ///     </item>
    ///     <item>
    ///         Initializes and displays the main window and updates related UI components.
    ///     </item>
    ///     <item>
    ///         Opens the project file manager.
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
        
        ConsoleAndLogWriteLine(
            $"STARTING {AppName.ToUpper()} ...");
      
        // Ouverture la fenetre principale
        ConsoleAndLogWriteLine("Opening main window");
        DisplayElements = new DisplayElements();
  
        // Affichage de la fenêtre principale
        DisplayElements.ShowMainWindow();

        // Ouverture du gestionnaire de fichiers de projet
        ConsoleAndLogWriteLine("Opening project file manager");
        Fm = new ProjectFileManager();
        
        // Nettoyage des dossiers restants de la derniere session
        ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
        DeleteAllExceptLogsAndResources();
        
        ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
        ConsoleAndLogWriteLine("-----------------------------------------------------------");
     
        // Appel au garbage collector pour nettoyer les variables issues 
        GC.Collect();
    }

    // Fonction s'executant lorsque l'on ferme l'application
    /// <summary>
    /// Executes when the application is closing.
    /// <para>
    /// This method performs the following tasks:
    /// <list type="bullet">
    ///     <item>
    ///         Logs the start of the application closing process.
    ///     </item>
    ///     <item>
    ///         Calls the base class implementation of <see cref="OnExit"/> to ensure proper shutdown behavior.
    ///     </item>
    ///     <item>
    ///         Logs the successful closure of the application.
    ///     </item>
    ///     <item>
    ///         Closes the log file stream if it is open, to ensure all log entries are properly written.
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="e">An instance of <see cref="ExitEventArgs"/> that contains the event data.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        ConsoleAndLogWriteLine("-----------------------------------------------------------");
        ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP..."); 
            
        base.OnExit(e);
            
        ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
        //_writer?.Close(); // Fermeture du stream d'ecriture des logs
    }
    
    // Fonction permettant de supprimer tous les dossiers presents dans le dossier courant
    // Sauf le fichier logs. Cela permet de supprimer tous les projets exportes a la session precedente.
    // Fonction pour supprimer tous les dossiers sauf le dossier 'logs'
    /// <summary>
    /// Deletes all directories in the application directory except for those named 'logs' and 'resources'.
    /// <para>
    /// This method iterates through all subdirectories in the base directory and deletes them, excluding the directories 'logs' and 'resources'.
    /// This helps in cleaning up directories from previous sessions, retaining only the specified directories for future use.
    /// </para>
    /// <para>
    /// In case of an error during the deletion, such as unauthorized access or I/O errors, the method logs the error message to the console and continues processing other directories.
    /// </para>
    /// <para>
    /// The method logs the path of each successfully deleted directory to the application log for tracking purposes.
    /// </para>
    /// </summary>
    private static void DeleteAllExceptLogsAndResources()
    {
        if (Directory.GetDirectories("./").Length <= 3 && Directory.GetFiles("./", "*.zip").Length == 0)
        {
            ConsoleAndLogWriteLine("--> No folder or zip file to delete");
        }
            
        // Itération sur tous les répertoires dans le répertoire de base
        foreach (var directory in Directory.GetDirectories("./"))
        {
            // Exclure le dossier 'logs', 'de' et 'runtimes'
            if ((Path.GetFileName(directory).Equals("logs", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("runtimes", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("de", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            // Supprimer le dossier et son contenu
            try
            {
                Directory.Delete(directory, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {directory}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {directory}: {ex.Message}");
                continue;
            }
            ConsoleAndLogWriteLine($"--> Deleted directory: {directory}");
        }

        foreach (var zipFile in Directory.GetFiles("./", "*.zip"))
        {
            try
            {
                File.Delete(zipFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {zipFile}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {zipFile}: {ex.Message}");
                continue;
            }
            ConsoleAndLogWriteLine($"--> Deleted file: {zipFile}");
        }
        
        foreach (var xmlFile in Directory.GetFiles("./", "*.xml"))
        {
            try
            {
                File.Delete(xmlFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {xmlFile}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {xmlFile}: {ex.Message}");
                continue;
            }
            ConsoleAndLogWriteLine($"--> Deleted file: {xmlFile}");
        }

            
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