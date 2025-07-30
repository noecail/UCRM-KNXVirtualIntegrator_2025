/***************************************************************************
 * Nom du Projet : KNX Virtual Integrator
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime, TETAZ Louison
 * Date          : 07/08/2024
 * Version       : 1.2
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
using KNX_Virtual_Integrator.Model.Wrappers;
using KNX_Virtual_Integrator.View;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator;

/* -----------------------------------------------------------------------------------
 * |                         Explications sur le logiciel                            |
 * -----------------------------------------------------------------------------------
 *
 * 
 * Ce logiciel a pour but de permettre à un technicien de tester le fonctionnement d'une
 * installation KNX. Pour cela, cet outil dispose d'une connexion au bus KNX via USB ou en
 * IP.
 * 
 * Le technicien importe le projet ETS en important un knxproj ou en important le fichier
 * d'adresses de groupe, et le logiciel pré-configure des modèles de tests prédéfinis pour
 * vérifier qu'à l'envoi de certaines valeur de CMD, on reçoit certaines valeurs sur les IE.
 * 
 * Si le technicien n'est pas satisfait par le test prédéfini, ou que le logiciel n'a pas été
 * capable d'associer un test à certaines adresses de groupe car elles ont un DPT trop particulier,
 * il peut créer lui-même des modèles de tests et les associer aux différentes adresses de groupe.
 *
 * Une fois tous les tests configurés, le technicien peut lancer le test. Le logiciel va alors
 * envoyer différentes trames aux adresses de groupes des Cmd et lire les IE correspondant pour
 * vérifier le fonctionnement de l'installation. Il génère par la suite un rapport de fonctionnement
 * au format PDF que le technicien peut exporter facilement et imprimer si besoin.
 *
 *
 * -----------------------------------------------------------------------------------
 *
 * 
 * L'architecture de ce projet est un peu particulière. Il s'agit d'une application développée
 * en C#, en utilisant la plateforme .NET Core 8.0 (qui doit désormais être dépassée car .NET 9.0
 * est en pre-release au moment ou nous rédigeons ce texte).
 * 
 * Cette application n'est pas une application "console", mais plutôt une application WPF. Il
 * s'agit d'un format d'application qui facilite le développement d'interfaces graphiques avec
 * des outils puissants.
 *
 * Dans une application WPF, tout commence dans le fichier .csproj, dans lequel on configure
 * les paramètres du projet, des fichiers de sortie, les librairies, etc...
 *
 * Le point d'entrée au démarrage du logiciel est App.xaml. A l'intérieur, vous pouvez définir
 * des ressources que vous voulez rendre utilisables par l'ensemble des classes du namespace de
 * l'application.
 *
 * Le fichier App.xaml.cs contient obligatoirement au moins deux fonctions: OnStartup qui gère
 * toutes les actions à effectuer au démarrage du logiciel, OnExit qui gère toutes les actions
 * à effectuer à la fermeture du logiciel (notamment par exemple lorsque l'on fait les commandes
 * Environment.Exit() ou Application.Current.Shutdown()). Nous avons également défini dans ce
 * fichier des variables caractérisant l'application.
 *
 *
 * -----------------------------------------------------------------------------------
 *
 * 
 * L'application respecte une structure MVVM (Model, View, ViewModel). Cette structure est pensée
 * de manière à faciliter la généricité du code, l'indépendance des classes, la séparation des
 * préoccupations et le test des différentes fonctionnalités.
 *
 * Tout le traitement de données et le vrai back-end se trouve dans le dossier Model. Dans ce
 * dossier, les différents services sont implémentés. Chaque service a sa propre utilité et ne
 * sert qu'à un but précis. Ces services peuvent intéragir entre eux, mais pour cela il faut avoir
 * recours à ce que l'on appelle l'injection de dépendance. Pour faire simple, le constructeur du
 * service contient en paramètre une instance du service dont on a besoin de manière à en conserver
 * une référence. Ainsi, on n'utilise pas de méthode statique, et on évite de rendre la classe
 * dépendante d'autres classes.
 *
 * Pour rassembler et coordonner ces services, la classe ModelManager conserve et maintient en vie
 * des instances de chaque service. Pour appeler une fonction spécifique d'un service, on
 * passera donc par le ModelManager. Si le besoin y est, vous pouvez créer plusieurs instances
 * d'un même service (ex: nous avons un service qui régit le comportement lorsque l'on clique sur
 * le slider de la fenêtre de paramétrage. Si je crée un nouveau slider, je peux facilement réimplémenter
 * le même comportement sans réécrire de code juste en créant une nouvelle instance de ce service
 * et en la liant au nouveau slider).
 *
 * Il est recommandé de créer, pour chaque service, une interface et une implémentation.
 *
 * Le dossier Resources contient les ressources de l'application : styles, couleurs, images,
 * traductions des textes pour chaque langue et pour chaque fenêtre, logos, polices d'écriture, ...
 *
 * Le dossier View contient les fenêtres de l'application. Chaque fenêtre est composée de deux fichiers:
 * le front-end est contenue dans le fichier .xaml. Ce fichier permet d'assez facilement créer des
 * interfaces complexes et détaillées à l'aide de balises. Les fichiers font un peu peur au premier abord
 * mais en réalité la logique derrière est plutôt simple. Le back-end est lui contenu dans le fichier .xaml.cs.
 * Il agit comme une classe normale qui régit les comportements de la fenêtre. Attention cependant,
 * il faut à tout prix éviter d'effectuer du traitement dans ces fichiers. Dans une architecture MVVM, il
 * est largement conseillé de créer les fonctions de traitement dans Model, et d'utiliser le ViewModel pour
 * appeler ces fonctions, mais nous reviendrons sur la notion de ViewModel juste après.
 *
 * Le WindowManager contient les instances de chaque fenêtre, ainsi que des fonctions permettant à App
 * d'intéragir avec ces dernières.
 *
 * Les ViewModel font le lien entre les services et les éléments d'interface.
 * Dans un ViewModel, on implémente des commandes qui appellent les fonctions contenues dans
 * les divers services. On n'appellera jamais les fonctions d'un service dans le back-end
 * d'une fenêtre. Les ViewModels peuvent aussi contenir des variables, en récupérer ou en
 * affecter dans les services. Il est possible de créer plusieurs ViewModels si nécessaire.
 *
 *
 * -----------------------------------------------------------------------------------
 *
 *
 * Nous avons défini une logique de versionnage de l'application. A chaque fois que l'on fait un
 * merge fonctionnel qui apporte une modification significative de l'application, on incrémente
 * AppVersion.
 * Pour différencier un peu plus les micro-versions, nous avons tous implémentés sur notre
 * clone du repository sur notre ordinateur un hook qui exécute le fichier IncrementationVersion.sh.
 * Ce script incrémente automatiquement AppBuild à chaque fois que l'on fait un merge.
 * C'est pas le top du top mais ça fonctionne correctement et c'est automatique.
 * Par contre, il faut re-commit App.xaml.cs après le merge pour que tout le monde soit à jour
 * sur le numéro de build, sinon plusieurs builds auront le même ID et ça sera moins facile de retrouver
 * quelle modification du logiciel a tout cassé.
 *
 * Le hook doit être place dans le fichier ".git/hooks" (C'est un dossier caché à la base du
 * répertoire clôné)
 *
 *
 * -----------------------------------------------------------------------------------
 *
 * Parlons de ce qui a été fait sur le logiciel et de ce qu'il reste à faire:
 * - La fenêtre paramètres est terminée
 * - La connexion au bus fonctionne et la fenêtre associée est terminée
 * - La génération du PDF a été commencée, mais il reste des parties du rapport à générer
 * - Les fonctions de regroupement des adresses de groupes en liant les commandes (Cmd) au indications
 * d'états (Ie) principalement en fonction de leur nom (et en fonction du fichier importé) sont terminées
 * - Reste à faire: dans la ReportCreationWindow, afficher un aperçu du rapport en cours de création ?
 * - GESTION DES EXCEPTIONS ! Très important (Sur Jetbrains Rider, si vous survolez une fonction, ça vous
 * affiche les exceptions qu'elle peut lever
 * [PARLER DE CE QU'IL RESTE A FAIRE SUR LE LOGICIEL ET DE CE QUI EST DEJA FAIT]
 *
 *
 * -----------------------------------------------------------------------------------
 *
 * Quelques petites astuces pour le déroulé du projet:
 *
 * Vous l'avez probablement déjà fait, mais formez vous correctement au C#, il existe une
 * formation gratuite et certifiante proposée par Microsoft qui est très bien:
 * https://www.freecodecamp.org/learn/foundational-c-sharp-with-microsoft/
 * 
 * Pour la praticité et pour faciliter le travail de groupe, nous vous recommandons de faire
 * comme nous et d'utiliser soit Github Desktop soit un IDE qui implémente solidement Github
 * tel que Jetbrains Rider [Gratuit pour les étudiants] et à vous familiariser avec les Merge
 * pour pouvoir travailler sur les mêmes fichiers sans écraser les modifications que vos
 * collègues ont effectué quand vous faites un push.
 *
 * Hésitez pas à jeter un oeil au premier projet que l'on a développé avant celui-ci,
 * il n'est pas aussi bien structuré mais il fonctionne: https://github.com/Daichi9764/UCRM
 *
 * Pour faciliter la reprise de code et la coopération, faire des <summary> et bien commenter
 * le code.
 *
 * Bon courage 😉
 */

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
    public const int AppBuild = 147;
    
        
    
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

    //private ReportCreationWindow? _creationWindow;
        
        
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- MÉTHODES  --------------------------------------------
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

        //if (MainViewModel != null) _creationWindow = new ReportCreationWindow(MainViewModel);
        //_creationWindow?.Show();
    }

    
    /// <summary>
    /// Initializes various components and dependencies of the application.
    /// </summary>
    private void InitializeApplicationComponents()
    {
        // Définir la priorité du processus à un niveau inférieur pour réduire l'utilisation des ressources
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        //var list = new FunctionalModelList();
        //list.ImportDictionary(@"C:\Users\manui\Documents\Stage 4A\Test\Pray2.xml");
        //list.ExportDictionary(@"C:\Users\caill\Desktop\INSA\4A\Stage");
        //list.ExportDictionary(@"C:\Users\manui\Documents\Stage 4A\Test\Pray");
        

        // Instancier les dépendances nécessaires
        var logger = new Logger();
        var fileLoader = new FileLoader(logger);
        var applicationFileManager = new ApplicationFileManager(logger);
        var systemSettingsDetector = new SystemSettingsDetector(logger);
        var appSettings = new ApplicationSettings(applicationFileManager, systemSettingsDetector, logger);
        var projectFileManager = new ProjectFileManager(logger, appSettings);
        var fileFinder = new FileFinder(logger, projectFileManager);
        var zipArchiveManager = new ZipArchiveManager(logger);
        var namespaceResolver = new NamespaceResolver(logger);
        var groupAddressProcessor = new GroupAddressProcessor(logger);
        var stringManagement = new StringManagement(groupAddressProcessor);
        var groupAddressMerger = new GroupAddressMerger(stringManagement, logger);
        var groupAddressManager = new GroupAddressManager(logger, projectFileManager, fileLoader, namespaceResolver, groupAddressProcessor, groupAddressMerger);
        var projectInfoManager = new ProjectInfoManager(namespaceResolver);
        var debugArchiveGenerator = new DebugArchiveGenerator(logger, zipArchiveManager, appSettings);
        var busConnection = new BusConnection(logger, new KnxBusWrapper());
        var groupCommunication = new GroupCommunication(busConnection, logger);
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
            pdfDocumentCreator,
            projectInfoManager);
        
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
    
        // Fermer le flux d'écriture du logger
        ModelManager?.Logger.CloseLogWriter();
    }
    
}

































