using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model
{
    /// <summary>
    ///     Manager which holds every instance of the model classes that need to only be accessed by the ViewModel. The instances can be accessed but not set.
    /// </summary>
    /// <param name="fileFinder"><see cref="Implementations.FileFinder"/>, to import files</param>
    /// <param name="projectFileManager"><see cref="Implementations.ProjectFileManager"/>, to manage the project</param>
    /// <param name="logger"><see cref="Logger"/>, to log and print information</param>
    /// <param name="groupAddressManager"><see cref="Implementations.GroupAddressManager"/>, to manage group addresses</param>
    /// <param name="debugArchiveGenerator"><see cref="Implementations.DebugArchiveGenerator"/>, to create bug reports</param>
    /// <param name="applicationFileManager"><see cref="Implementations.ApplicationFileManager"/>, to handle files</param>
    /// <param name="busConnection"><see cref="Implementations.BusConnection"/>, to handle the KNX Bus</param>
    /// <param name="groupCommunication"><see cref="Implementations.GroupCommunication"/>, to communicate with the bus</param>
    /// <param name="appSettings"><see cref="Implementations.ApplicationSettings"/>, to manage the settings</param>
    /// <param name="parentFinder"><see cref="Implementations.ParentFinder"/>, to organize group addresses</param>
    /// <param name="settingsSliderClickHandler"><see cref="Implementations.SliderClickHandler"/>, to handle sliders</param>
    /// <param name="pdfDocumentCreator"><see cref="Implementations.PdfDocumentCreator"/>, to create Test report PDF</param>
    public class ModelManager(
        IFileFinder fileFinder,
        IProjectFileManager projectFileManager,
        ILogger logger,
        IGroupAddressManager groupAddressManager,
        IDebugArchiveGenerator debugArchiveGenerator,
        IApplicationFileManager applicationFileManager,
        IBusConnection busConnection,
        IGroupCommunication groupCommunication,
        IApplicationSettings appSettings,
        IParentFinder parentFinder,
        ISliderClickHandler settingsSliderClickHandler,
        IPdfDocumentCreator pdfDocumentCreator)
    {
        
        // Instances de toutes les classes
        /// <summary> Class used to handle importing files  </summary>
        public IFileFinder FileFinder { get; } = fileFinder;
        /// <summary> Class used to extract information from the imperted files </summary>
        public IProjectFileManager ProjectFileManager { get; } = projectFileManager;
        /// <summary> Class used to print out information and to log it in a file </summary>
        public ILogger Logger { get; } = logger;
        /// <summary> Class handling any processing of group addresses </summary>
        public IGroupAddressManager GroupAddressManager { get; } = groupAddressManager;
        /// <summary> Class handling the generation of the debug file </summary>
        public IDebugArchiveGenerator DebugArchiveGenerator { get; } = debugArchiveGenerator;
        /// <summary> Class handling the logs and archives of the app </summary>
        public IApplicationFileManager ApplicationFileManager { get; } = applicationFileManager;
        /// <summary> Class handling the connection to the bus </summary>
        public IBusConnection BusConnection { get; } = busConnection;
        /// <summary> Class handling any connection to the bus after connection </summary>
        public IGroupCommunication GroupCommunication { get; } = groupCommunication;
        /// <summary> Class handling the logs and archives of the app </summary>
        public IApplicationSettings AppSettings { get; } = appSettings;
        /// <summary> Class handling the search of related elements of the UI </summary>
        public IParentFinder ParentFinder { get; } = parentFinder;
        /// <summary> Class handling the slider of the <see cref="View.Windows.SettingsWindow"/></summary>
        public ISliderClickHandler SettingsSliderClickHandler { get; } = settingsSliderClickHandler;
        /// <summary> Class handling the creation of the PDF analysis report </summary>
        public IPdfDocumentCreator PdfDocumentCreator { get; } = pdfDocumentCreator;
    }
}