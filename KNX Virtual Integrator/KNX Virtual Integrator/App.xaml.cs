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
using KNX_Virtual_Integrator.Model.Implementations;
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
    public static WindowManager? WindowManager { get; private set; }

    /// <summary>
    /// Represents the main ViewModel of the application, handling the overall data-binding and command logic
    /// for the main window and core application functionality.
    /// </summary>
    public static MainViewModel? MainViewModel { get; private set; }

    /// <summary>
    /// Manages the application's core data models and business logic, providing a central point for 
    /// interacting with and managing the data and services required by the application.
    /// </summary>
    public static ModelManager? ModelManager { get; private set; }

        
        
        
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeApplicationComponents();
        OpenMainWindow();
        PerformStartupTasks();
    }

    private void InitializeApplicationComponents()
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        // Instancier les dépendances nécessaires
        var fileLoader = new FileLoader();
        var logger = new Logger();
        var projectFileManager = new ProjectFileManager(logger);
        var fileFinder = new FileFinder(logger, projectFileManager);
        var zipArchiveManager = new ZipArchiveManager(logger);
        var groupAddressManager = new GroupAddressManager(logger, projectFileManager);
        var systemSettingsDetector = new SystemSettingsDetector(logger);
        var debugArchiveGenerator = new DebugArchiveGenerator(logger, zipArchiveManager);
        var applicationFileManager = new ApplicationFileManager(logger, systemSettingsDetector);

        // Instancier ModelManager avec les dépendances
        ModelManager = new ModelManager(
            fileLoader,
            fileFinder,
            projectFileManager,
            logger,
            zipArchiveManager,
            groupAddressManager,
            systemSettingsDetector,
            debugArchiveGenerator,
            applicationFileManager);
        
        ModelManager.EnsureLogDirectoryExists();

        ModelManager.ConsoleAndLogWriteLine($"STARTING {AppName.ToUpper()} V{AppVersion.ToString("0.0", CultureInfo.InvariantCulture)} BUILD {AppBuild}...");

        MainViewModel = new MainViewModel();
        WindowManager = new WindowManager();
    }

    private void OpenMainWindow()
    {
        ModelManager?.ConsoleAndLogWriteLine("Opening main window");
        WindowManager?.MainWindow.UpdateWindowContents(true, true, true);
        WindowManager?.ShowMainWindow();
    }

    private void PerformStartupTasks()
    {
        ModelManager?.ConsoleAndLogWriteLine("Trying to archive log files");
        ModelManager?.ArchiveLogs();

        ModelManager?.ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
        ModelManager?.DeleteAllExceptLogsAndResources();

        ModelManager?.ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
        ModelManager?.ConsoleAndLogWriteLine("-----------------------------------------------------------");

        GC.Collect();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ModelManager?.ConsoleAndLogWriteLine("-----------------------------------------------------------");
        ModelManager?.ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP...");

        base.OnExit(e);

        ModelManager?.ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
        ModelManager?.CloseLogWriter();
    }

}