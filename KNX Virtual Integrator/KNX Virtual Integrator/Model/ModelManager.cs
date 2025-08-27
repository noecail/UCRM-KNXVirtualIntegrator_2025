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
        public IFileFinder FileFinder { get; } = fileFinder;
        public IProjectFileManager ProjectFileManager { get; } = projectFileManager;
        public ILogger Logger { get; } = logger;
        public IGroupAddressManager GroupAddressManager { get; } = groupAddressManager;
        public IDebugArchiveGenerator DebugArchiveGenerator { get; } = debugArchiveGenerator;
        public IApplicationFileManager ApplicationFileManager { get; } = applicationFileManager;
        public IBusConnection BusConnection { get; } = busConnection;
        public IGroupCommunication GroupCommunication { get; } = groupCommunication;
        public IApplicationSettings AppSettings { get; } = appSettings;
        public IParentFinder ParentFinder { get; } = parentFinder;
        public ISliderClickHandler SettingsSliderClickHandler { get; } = settingsSliderClickHandler;
        public IPdfDocumentCreator PdfDocumentCreator { get; } = pdfDocumentCreator;
    }
}