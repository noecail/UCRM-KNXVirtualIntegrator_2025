/***************************************************************************
 * Nom du Projet : KNX Virtual Integrator
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime, TETAZ Louison
 * Date          : 07/08/2024
 * Version       : 1.1
 *
 * Description :
 * Fichier principal contenant la structure de l'application et toutes les
 * fonctions necessaires a son utilisation.
 *
 * Remarques :
 * Repo GitHub --> https://github.com/Moliveiralo/UCRM-KNXVirtualIntegrator
 *
 * **************************************************************************/

// ReSharper disable GrammarMistakeInComment

using System.Diagnostics;
using System.Globalization;
using System.Windows;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.Model.Implementations;
using KNX_Virtual_Integrator.View;
using KNX_Virtual_Integrator.View.Windows;
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
    public const float AppVersion = 1.2f; // Version de l'application

    /// <summary>
    /// Represents the build of the application. Updated each time portions of code are merged on github.
    /// </summary>
    public const int AppBuild = 103;
    
        
    
    // --> Composants de l'application
    /// <summary>
    /// Manages the application's display elements, including windows, buttons, and other UI components.
    /// </summary>
    public static WindowManager? WindowManager { get; private set; }

    /// <summary>
    /// Represents the main ViewModel of the application, handling the overall data-binding and command logic
    /// for the main window and core application functionality.
    /// </summary>
    private static MainViewModel? MainViewModel { get; set; }

    /// <summary>
    /// Manages the application's core data models and business logic, providing a central point for 
    /// interacting with and managing the data and services required by the application.
    /// </summary>
    private static ModelManager? ModelManager { get; set; }

    private ReportCreationWindow w = new ReportCreationWindow();
        
        
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Entry point of the application that triggers initialization and opening of the main window.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeApplicationComponents(); // Initialiser les composants de l'application
        OpenMainWindow(); // Ouvrir la fenêtre principale
        PerformStartupTasks(); // Exécuter les tâches de démarrage
        w.Show();
    }

    
    /// <summary>
    /// Initializes various components and dependencies of the application.
    /// </summary>
    private void InitializeApplicationComponents()
    {
        // Définir la priorité du processus à un niveau inférieur pour réduire l'utilisation des ressources
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        // Instancier les dépendances nécessaires
        var logger = new Logger();
        var fileLoader = new FileLoader(logger);
        var applicationFileManager = new ApplicationFileManager(logger);
        var systemSettingsDetector = new SystemSettingsDetector(logger);
        var appSettings = new ApplicationSettings(applicationFileManager, systemSettingsDetector);
        var projectFileManager = new ProjectFileManager(logger, appSettings);
        var fileFinder = new FileFinder(logger, projectFileManager);
        var zipArchiveManager = new ZipArchiveManager(logger);
        var namespaceResolver = new NamespaceResolver(logger);
        var groupAddressProcessor = new GroupAddressProcessor(logger);
        var stringManagement = new StringManagement(groupAddressProcessor);
        var groupAddressMerger = new GroupAddressMerger(groupAddressProcessor, stringManagement, logger);
        var groupAddressManager = new GroupAddressManager(logger, projectFileManager, fileLoader, namespaceResolver, groupAddressProcessor, groupAddressMerger);
        var debugArchiveGenerator = new DebugArchiveGenerator(logger, zipArchiveManager, appSettings);
        var busConnection = new BusConnection();
        var groupCommunication = new GroupCommunication(busConnection);
        var parentFinder = new ParentFinder(logger);
        var sliderClickHandler = new SliderClickHandler(logger, parentFinder);
        var pdfDocumentCreator = new PdfDocumentCreator(projectFileManager);

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
            applicationFileManager,
            busConnection,
            groupCommunication,
            appSettings, 
            parentFinder,
            sliderClickHandler,
            pdfDocumentCreator);
        
        // Enregistrer un message de démarrage dans la console et le journal
        ModelManager.Logger.ConsoleAndLogWriteLine($"STARTING {AppName.ToUpper()} V{AppVersion.ToString("0.0", CultureInfo.InvariantCulture)} BUILD {AppBuild}...");

        // Initialiser le ViewModel principal et le gestionnaire de fenêtres
        MainViewModel = new MainViewModel(ModelManager);
        WindowManager = new WindowManager(MainViewModel);
    }

    
    /// <summary>
    /// Opens and displays the main window of the application.
    /// </summary>
    private void OpenMainWindow()
    {
        // Enregistrer un message de l'ouverture de la fenêtre principale
        ModelManager?.Logger.ConsoleAndLogWriteLine("Opening main window");
    
        // Mettre à jour le contenu de la fenêtre principale
        WindowManager?.MainWindow.UpdateWindowContents(true, true, true);
    
        // Afficher la fenêtre principale
        WindowManager?.ShowMainWindow();
    }

    
    /// <summary>
    /// Executes additional tasks required during the startup process.
    /// </summary>
    private void PerformStartupTasks()
    {
        // Enregistrer un message d'archivage des fichiers de logs
        ModelManager?.Logger.ConsoleAndLogWriteLine("Trying to archive log files");
    
        // Archiver les fichiers de logs
        ModelManager?.ApplicationFileManager.ArchiveLogs();

        // Enregistrer un message de suppression des dossiers extraits
        ModelManager?.Logger.ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
    
        // Supprimer tous les fichiers sauf les logs et les ressources
        ModelManager?.ApplicationFileManager.DeleteAllExceptLogsAndResources();

        // Enregistrer un message indiquant le démarrage de l'application
        ModelManager?.Logger.ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
        ModelManager?.Logger.ConsoleAndLogWriteLine("-----------------------------------------------------------");

        // Forcer la collecte des objets inutilisés
        GC.Collect();
    }

    
    /// <summary>
    /// Handles the cleanup process when the application exits.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        // Enregistrer un message de fermeture dans la console et le journal
        ModelManager?.Logger.ConsoleAndLogWriteLine("-----------------------------------------------------------");
        ModelManager?.Logger.ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP...");

        base.OnExit(e);

        // Enregistrer un message indiquant que l'application est fermée
        ModelManager?.Logger.ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
    
        // Fermer l'écrivain de journal
        ModelManager?.Logger.CloseLogWriter();
    }
    
}
