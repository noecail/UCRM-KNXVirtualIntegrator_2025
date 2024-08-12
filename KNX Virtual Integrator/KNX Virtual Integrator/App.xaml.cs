/***************************************************************************
 * Nom du Projet : KNX Virtual Integrator
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime
 * Date          : 07/08/2024
 * Version       : 1.0
 *
 * Description :
 * Fichier principal contenant la structure de l'application et toutes les
 * fonctions necessaires a son utilisation.
 *
 * Remarques :
 * Repo GitHub --> https://github.com/Daichi9764/UCRM
 *
 * **************************************************************************/

// ReSharper disable GrammarMistakeInComment

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace KNX_Virtual_Integrator;

public partial class App
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Donnees de l'application

    /// <summary>
    /// Represents the name of the application.
    /// </summary>
    public const string AppName = "KNX Virtual Integrator"; // Nom de l'application

    /// <summary>
    /// Represents the version of the application.
    /// </summary>
    public const float AppVersion = 1.0f; // Version de l'application

    /// <summary>
    /// Represents the build of the application. Updated each time portions of code are merged on github.
    /// </summary>
    public const int AppBuild = 1;


    // Gestion des logs
    /// <summary>
    /// Stores the file path for the log file. This path is used to determine where the log entries will be written.
    /// </summary>
    public static string? LogPath { get; private set; } // Chemin du fichier logs
        
    /// <summary>
    /// Provides a <see cref="StreamWriter"/> instance for writing log entries to the log file.
    /// </summary>
    /// <remarks>
    /// This writer is used for appending log messages to the file specified by <see cref="LogPath"/>.
    /// </remarks>
    private static StreamWriter? _writer; // Permet l'ecriture du fichier de logging
        
        
        
    // Composants de l'application
        
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
        if (!Directory.Exists("./logs"))
        {
            Directory.CreateDirectory("./logs");
        }

        LogPath = $"./logs/logs-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        _writer = new StreamWriter(LogPath);

        base.OnStartup(e);
            
        var currentProcess = Process.GetCurrentProcess();
        currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

        // Activation de l'auto-vidage du buffer du stream d'ecriture
        _writer.AutoFlush = true;


        ConsoleAndLogWriteLine(
            $"STARTING {AppName.ToUpper()} V{AppVersion.ToString(CultureInfo.InvariantCulture)} BUILD {AppBuild}...");


        // Ouverture la fenetre principale
        ConsoleAndLogWriteLine("Opening main window");
        DisplayElements = new DisplayElements();
        
        // Mise a jour de la fenetre principale (titre, langue, thème, ...)
        DisplayElements.MainWindow.UpdateWindowContents(true, true, true);

        // Affichage de la fenêtre principale
        DisplayElements.ShowMainWindow();


        // Ouverture du gestionnaire de fichiers de projet
        ConsoleAndLogWriteLine("Opening project file manager");
        Fm = new ProjectFileManager();


        // Tentative d'archivage des fichiers de log
        ConsoleAndLogWriteLine("Trying to archive log files");
        ArchiveLogs();


        // Nettoyage des dossiers restants de la derniere session
        ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
        DeleteAllExceptLogsAndResources();

        // CheckForUpdatesAsync();

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
        _writer?.Close(); // Fermeture du stream d'ecriture des logs
    }

        
        
    // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'ecrivant dans les
    // logs sans sauter de ligne apres le message.
    /// <summary>
    /// Writes a message to the application console and log file without appending a newline after the message.
    /// <para>
    /// This method performs the following tasks:
    /// <list type="bullet">
    ///     <item>
    ///         Writes the provided message to the console without adding a newline character.
    ///     </item>
    ///     <item>
    ///         If the console window is visible, scrolls to the end of the console text to ensure the latest message is visible.
    ///     </item>
    ///     <item>
    ///         Writes the same message to the log file without appending a newline character.
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="msg">The message to be written to the console and log file.</param>
    public static void ConsoleAndLogWrite(string msg)
    {
        Console.Write(msg); // Ecriture du message dans la console
        _writer?.Write(msg); // Ecriture du message dans le fichier logs
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
        Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
        _writer?.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
    }

        
        
    // Fonction d'archivage des logs
    // Fonctionnement : S'il y a plus de 50 fichiers logs.txt, ces fichiers sont rassembles et compresses dans une archive zip
    // S'il y a plus de 10 archives, ces dernieres sont supprimees avant la creation de la nouvelle archive
    // Conséquence : on ne stocke les logs que des 50 derniers lancements de l'application
    /// <summary>
    /// Archives the log files in the log directory by compressing them into a ZIP archive when the number of log files exceeds 50.
    /// <para>
    /// If there are more than 50 log files, the method will create a new ZIP archive containing all log files, excluding the current log file.
    /// If there are already 10 or more existing archives, it will delete the oldest ones before creating a new archive.
    /// This ensures that only the log files from the last 50 application runs are retained.
    /// </para>
    /// <para>
    /// If there are fewer than 50 log files, no archiving will be performed.
    /// </para>
    /// <para>
    /// If an error occurs during the process, it logs the error message to the console and log file.
    /// </para>
    /// </summary>
    private static void ArchiveLogs()
    {
        var logDirectory = @"./logs/"; // Chemin du dossier de logs
            
        try
        {
            // Verifier si le repertoire existe
            if (!Directory.Exists(logDirectory))
            {
                ConsoleAndLogWriteLine($"--> The specified directory does not exist : {logDirectory}");
                return;
            }

            // Obtenir tous les fichiers log dans le repertoire
            var logFiles = Directory.GetFiles(logDirectory, "*.txt");

            // Verifier s'il y a plus de 50 fichiers log
            if (logFiles.Length > 50)
            {
                // Obtenir tous les fichiers d'archive dans le repertoire
                var archiveFiles = Directory.GetFiles(logDirectory, "LogsArchive-*.zip");

                // Supprimer les archives existantes si elles sont plus de 10
                if (archiveFiles.Length >= 10)
                {
                    foreach (var archiveFile in archiveFiles)
                    {
                        File.Delete(archiveFile);
                    }
                    ConsoleAndLogWriteLine("--> Deleted all existing archive files as they exceeded the limit of 10.");
                }

                // Creer le nom du fichier zip avec la date actuelle
                var zipFileName = Path.Combine(logDirectory, $"LogsArchive-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");

                // Creer l'archive zip et y ajouter les fichiers log
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                {
                    foreach (var logFile in logFiles)
                    {
                        if (logFile != LogPath) // Si le fichier logs n'est pas celui que l'on vient de creer pour le lancement actuel
                        {
                            zip.CreateEntryFromFile(logFile, Path.GetFileName(logFile)); // On l'ajoute e l'archive
                            File.Delete(logFile); // Puis, on le supprime
                        }
                    }
                }

                ConsoleAndLogWriteLine($"--> Successfully archived log files to {zipFileName}");
            }
            else
            {
                ConsoleAndLogWriteLine("--> Not enough log files to archive.");
            }
        }
        catch (Exception ex)
        {
            ConsoleAndLogWriteLine($"--> An error occured while creating the log archive : {ex.Message}");
        }
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
            
    }
    
        
    // Destructeur de App
    /// <summary>
    /// Finalizer for the <see cref="App"/> class.
    /// Closes the log writer stream if it is still open when the application is being finalized.
    /// </summary>
    ~App()
    {
        // Si le stream d'écriture dans les logs est toujours ouvert, on le ferme
        _writer?.Close();
    }
}
















