using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model
{
    public class ModelManager(
        IFileLoader fileLoader,
        IFileFinder fileFinder,
        IProjectFileManager projectFileManager,
        ILogger logger,
        IZipArchiveManager zipArchiveManager,
        IGroupAddressManager groupAddressManager,
        ISystemSettingsDetector systemSettingsDetector,
        IDebugArchiveGenerator debugArchiveGenerator,
        IApplicationFileManager applicationFileManager,
        IBusConnection busConnection,
        IGroupCommunication groupCommunication,
        IApplicationSettings appSettings,
        IParentFinder parentFinder,
        ISliderClickHandler settingsSliderClickHandler)

    {
    public IFileLoader FileLoader { get; } = fileLoader;
    public IFileFinder FileFinder { get; } = fileFinder;
    public IProjectFileManager ProjectFileManager { get; } = projectFileManager;
    public ILogger Logger { get; } = logger;
    public IZipArchiveManager ZipArchiveManager { get; } = zipArchiveManager;
    public IGroupAddressManager GroupAddressManager { get; } = groupAddressManager;
    public ISystemSettingsDetector SystemSettingsDetector { get; } = systemSettingsDetector;
    public IDebugArchiveGenerator DebugArchiveGenerator { get; } = debugArchiveGenerator;
    public IApplicationFileManager ApplicationFileManager { get; } = applicationFileManager;
    public IBusConnection BusConnection { get; } = busConnection;
    public IGroupCommunication GroupCommunication { get; } = groupCommunication;
    public IApplicationSettings AppSettings { get; } = appSettings;
    public IParentFinder ParentFinder { get; } = parentFinder;
    public ISliderClickHandler SettingsSliderClickHandler { get; } = settingsSliderClickHandler;
    }
}