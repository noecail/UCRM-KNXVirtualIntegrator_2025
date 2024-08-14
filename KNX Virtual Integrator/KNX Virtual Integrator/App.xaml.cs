/***************************************************************************
 * Nom du Projet : KNX Virtual Integrator
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime, TETAZ Louison
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
using System.Windows;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.View;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator;

public partial class App
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    
    // --> Données de l'application
    /// <summary>
    /// Represents the name of the application.
    /// </summary>
    public const string AppName = "KNX Virtual Integrator"; // Nom de l'application

    /// <summary>
    /// Represents the version of the application.
    /// </summary>
    public const float AppVersion = 1.1f; // Version de l'application

    /// <summary>
    /// Represents the build of the application. Updated each time portions of code are merged on github.
    /// </summary>
    public const int AppBuild = 85;
    
        
    
    // --> Composants de l'application
    /// <summary>
    /// Manages the application's display elements, including windows, buttons, and other UI components.
    /// </summary>
    public static WindowManager? WindowManager { get; private set; } // Gestionnaire de l'affichage (contient les fenetres, boutons, ...)
    
    
    public static MainViewModel? MainViewModel { get; private set; }
        
        
        
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Fonction s'executant à l'ouverture de l'application
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
        ApplicationFileManager.EnsureLogDirectoryExists();

        base.OnStartup(e);
        
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        
        Logger.ConsoleAndLogWriteLine(
            $"STARTING {AppName.ToUpper()} V{AppVersion.ToString("0.0", CultureInfo.InvariantCulture)} BUILD {AppBuild}...");

        // Création du Main View Model
        MainViewModel = new();

        // Ouverture la fenetre principale
        Logger.ConsoleAndLogWriteLine("Opening main window");
        WindowManager = new WindowManager();
        
        // Mise a jour de la fenetre principale (titre, langue, thème, ...)
        WindowManager.MainWindow.UpdateWindowContents(true, true, true);

        // Affichage de la fenêtre principale
        WindowManager.ShowMainWindow();


        // Tentative d'archivage des fichiers de log
        Logger.ConsoleAndLogWriteLine("Trying to archive log files");
        ApplicationFileManager.ArchiveLogs();


        // Nettoyage des dossiers restants de la derniere session
        Logger.ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
        ApplicationFileManager.DeleteAllExceptLogsAndResources();

        // CheckForUpdatesAsync();

        Logger.ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
        Logger.ConsoleAndLogWriteLine("-----------------------------------------------------------");
        
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
        Logger.ConsoleAndLogWriteLine("-----------------------------------------------------------");
        Logger.ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP...");
            
        base.OnExit(e);

        Logger.ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
        Logger.CloseLogWriter(); // Fermeture du stream d'ecriture des logs
    }
}


