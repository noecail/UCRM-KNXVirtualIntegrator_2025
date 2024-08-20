using KNX_Virtual_Integrator.Model;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace KNX_Virtual_Integrator.ViewModel;

public class MainViewModel (ModelManager modelManager) : INotifyPropertyChanged
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public string ProjectFolderPath = "";

    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------- COMMANDES SANS VALEUR DE RETOUR  ---------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Patterne d'utilisation :
    // MaCommande.Execute(Args)
    //
    // Si la fonction n'a pas d'arguments, la déclarer en tant que commande dont les paramètres sont de type "object"
    // et lors de l'utilisation, on écrira macommande.Execute(null);
    // Pour un exemple, voir : ExtractGroupAddressCommand
    
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
    
    public ICommand FindZeroXmlCommand { get; } = new RelayCommand<string>(fileName => modelManager.FileFinder.FindZeroXml(fileName));
    
    
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
    
    
    public ICommand ExtractGroupAddressFileCommand { get; } = new RelayCommandWithResult<string, bool>(fileName => 
        modelManager.ProjectFileManager.ExtractGroupAddressFile(fileName));

    public ICommand ExtractProjectFilesCommand { get; } = new RelayCommandWithResult<string, bool>(fileName =>
        modelManager.ProjectFileManager.ExtractProjectFiles(fileName));
    
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