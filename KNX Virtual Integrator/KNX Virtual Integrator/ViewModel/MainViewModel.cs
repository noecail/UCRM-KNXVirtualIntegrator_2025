using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.Model.Interfaces;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace KNX_Virtual_Integrator.ViewModel;

public class MainViewModel (ModelManager modelManager) : INotifyPropertyChanged
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public string ProjectFolderPath = "";
    
    public IApplicationSettings AppSettings => modelManager.AppSettings;

    
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------- COMMANDES SANS VALEUR DE RETOUR  ---------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Patterne d'utilisation :
    // MaCommande.Execute(Args)
    //
    // Si la fonction n'a pas d'arguments, la déclarer en tant que commande dont les paramètres sont de type "object"
    // et lors de l'utilisation, on écrira macommande.Execute(null);
    // Pour un exemple, voir : ExtractGroupAddressCommand
    
    
    
    
    /// <summary>
    /// Command that writes a line of text to the console and log if the provided parameter is not null or whitespace.
    /// </summary>
    public ICommand ConsoleAndLogWriteLineCommand { get; } =
        new RelayCommand<string>(
            parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter)) modelManager.Logger.ConsoleAndLogWriteLine(parameter);
            }
        );

    
    /// <summary>
    /// Command that extracts a group address using the GroupAddressManager.
    /// </summary>
    public ICommand ExtractGroupAddressCommand { get; } =
        new RelayCommand<object>(_ => modelManager.GroupAddressManager.ExtractGroupAddress());

    
    /// <summary>
    /// Command that ensures the settings file exists. Creates the file if it does not exist, using the provided file path.
    /// </summary>
    public ICommand EnsureSettingsFileExistsCommand { get; } =
        new RelayCommand<string>(
            parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter)) modelManager.ApplicationFileManager.EnsureSettingsFileExists(parameter);
            }
        );

    
    /// <summary>
    /// Command that creates a debug archive with optional OS info, hardware info, and imported projects.
    /// </summary>
    /// <param name="IncludeOsInfo">Specifies whether to include OS information in the debug archive.</param>
    /// <param name="IncludeHardwareInfo">Specifies whether to include hardware information in the debug archive.</param>
    /// <param name="IncludeImportedProjects">Specifies whether to include imported projects in the debug archive.</param>
    public ICommand CreateDebugArchiveCommand { get; } =
        new RelayCommand<(bool IncludeOsInfo, bool IncludeHardwareInfo, bool IncludeImportedProjects)>(
            parameters =>
            {
                modelManager.DebugArchiveGenerator.CreateDebugArchive(parameters.IncludeOsInfo,
                    parameters.IncludeHardwareInfo,
                    parameters.IncludeImportedProjects);
            }
        );

    
    /// <summary>
    /// Command that finds a zero XML file based on the provided file name.
    /// </summary>
    /// <param name="fileName">The name of the file to find.</param>
    public ICommand FindZeroXmlCommand { get; } = new RelayCommand<string>(fileName => modelManager.FileFinder.FindZeroXml(fileName));

    
    /// <summary>
    /// Command that connects to the bus asynchronously.
    /// </summary>
    public ICommand ConnectBusCommand { get; } =
        new RelayCommand<object>(_ => modelManager.BusConnection.ConnectBusAsync());

    
    /// <summary>
    /// Command that disconnects from the bus asynchronously.
    /// </summary>
    public ICommand DisconnectBusCommand { get; } =
        new RelayCommand<object>(_ => modelManager.BusConnection.DisconnectBusAsync());

    
    /// <summary>
    /// Command that refreshes the list of bus interfaces asynchronously.
    /// </summary>
    public ICommand RefreshInterfacesCommand { get; } =
        new RelayCommand<object>(_ => modelManager.BusConnection.DiscoverInterfacesAsync());

    
    /// <summary>
    /// Command that sends a group value write "on" command asynchronously.
    /// </summary>
    public ICommand GroupValueWriteOnCommand { get; } =
        new RelayCommand<object>(_ => modelManager.GroupCommunication.GroupValueWriteOnAsync());

    
    /// <summary>
    /// Command that sends a group value write "off" command asynchronously.
    /// </summary>
    public ICommand GroupValueWriteOffCommand { get; } =
        new RelayCommand<object>(_ => modelManager.GroupCommunication.GroupValueWriteOffAsync());

    
    /// <summary>
    /// Command that saves the current application settings.
    /// </summary>
    public ICommand SaveSettingsCommand { get; } =
        new RelayCommand<object>(_ => modelManager.AppSettings.Save());

    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------- COMMANDES AVEC VALEUR DE RETOUR  ---------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Patterne d'utilisation :
    // if (_viewModel.LENOMDELACOMMANDE is RelayCommandWithResult<typeDuParametre, typeDeRetour> command)
    // {
    //      [UTILISATION DE LA COMMANDE]
    // }
    // else
    // {
    //      [GESTION DE L'ERREUR SI LA COMMANDE N'EST PAS DU TYPE ESPERE]
    // }
    //
    //
    // Ou sinon :
    // var maCommande = _viewModel.NOMDELACOMMANDE as RelayCommandWithResult<typeDuParametre, typeDeRetour>;
    //
    // if (maCommande != null) [UTILISATION DE maCommande DIRECTEMENT]
    
    
    
    
    /// <summary>
    /// Command that extracts a group address file based on the provided file name and returns a boolean indicating success.
    /// </summary>
    /// <param name="fileName">The name of the file to extract.</param>
    /// <returns>True if the extraction was successful; otherwise, false.</returns>
    public ICommand ExtractGroupAddressFileCommand { get; } = new RelayCommandWithResult<string, bool>(fileName => 
        modelManager.ProjectFileManager.ExtractGroupAddressFile(fileName));

    
    /// <summary>
    /// Command that extracts project files based on the provided file name and returns a boolean indicating success.
    /// </summary>
    /// <param name="fileName">The name of the file to extract.</param>
    /// <returns>True if the extraction was successful; otherwise, false.</returns>
    public ICommand ExtractProjectFilesCommand { get; } = new RelayCommandWithResult<string, bool>(fileName =>
        modelManager.ProjectFileManager.ExtractProjectFiles(fileName));
}