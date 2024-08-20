using KNX_Virtual_Integrator.Model;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace KNX_Virtual_Integrator.ViewModel;

public class MainViewModel (ModelManager modelManager) : INotifyPropertyChanged
{
    public string ProjectFolderPath = "";

    public ICommand ConsoleAndLogWriteLineCommand { get; } =
        new RelayCommand<string>(
            parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter)) modelManager.Logger.ConsoleAndLogWriteLine(parameter);
            }
        );

    
    public ICommand ExtractGroupAddressCommand { get; } =
        new RelayCommand<object>(_ => modelManager.GroupAddressManager.ExtractGroupAddress());

    
    public ICommand SaveApplicationSettingsCommand { get; } =
        new RelayCommand<object>(_ => modelManager.ApplicationFileManager.SaveApplicationSettings());

    
    public ICommand EnsureSettingsFileExistsCommand { get; } =
        new RelayCommand<string>(
            parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter)) modelManager.ApplicationFileManager.EnsureSettingsFileExists(parameter);
            }
        );
    
    
    public ICommand CreateDebugArchiveCommand { get; } =
        new RelayCommand<(bool IncludeOsInfo, bool IncludeHardwareInfo, bool IncludeImportedProjects)>(
            parameters =>
            {
                modelManager.DebugArchiveGenerator.CreateDebugArchive(parameters.IncludeOsInfo,
                                                                    parameters.IncludeHardwareInfo,
                                                                    parameters.IncludeImportedProjects);
            }
        );
    
    public bool ExtractGroupAddressFile(string fileName)
    {
        Console.WriteLine("Not implemented");
        return false;
    }

    public bool ExtractProjectFiles(string fileName)
    {
        Console.WriteLine("Not implemented");
        return false;
    }

    public object FindZeroXml(string viewModelProjectFolderPath)
    {
        Console.WriteLine("Not implemented");
        return null;
    }
    
    
    public ICommand ConnectBusCommand { get; } =
        new RelayCommand<object>(_ => modelManager.BusConnection.ConnectBusAsync());

    public ICommand DisconnectBusCommand { get; } =
        new RelayCommand<object>(_ => modelManager.BusConnection.DisconnectBusAsync());

    public ICommand RefreshInterfacesCommand { get; } =
        new RelayCommand<object>(_ => modelManager.BusConnection.DiscoverInterfacesAsync());

    public ICommand GroupValueWriteOnCommand { get; } =
        new RelayCommand<object>(_ => modelManager.GroupCommunication.GroupValueWriteOnAsync());

    public ICommand GroupValueWriteOffCommand { get; } =
        new RelayCommand<object>(_ => modelManager.GroupCommunication.GroupValueWriteOffAsync());

}