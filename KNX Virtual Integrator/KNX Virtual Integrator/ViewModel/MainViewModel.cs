using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.ViewModel.Commands;
using System.ComponentModel;
using Knx.Falcon;
using KNXIntegrator.Models;
using KNX_Virtual_Integrator.Model.Entities;


// ReSharper disable InvalidXmlDocComment
// ReSharper disable NullableWarningSuppressionIsUsed


namespace KNX_Virtual_Integrator.ViewModel;

/// <summary>
/// MainViewModel class that serves as a central relay for all partial classes in the ViewModel.
///
/// This file ONLY contains:
/// - The constructor for `MainViewModel`, which initializes RelayCommands and other necessary components.
///
/// This setup ensures that the `MainViewModel` class effectively manages and links the functionality defined across multiple partial classes,
/// providing a unified interface and behavior for the application's main view.
/// 
/// The ViewModels are created for each window and for special tasks, such as model management across windows.
/// </summary>
/// 
public partial class MainViewModel : ObservableObject, INotifyPropertyChanged
{

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public new event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<string> ConnectionTypes { get; } = new()
    {
        "Type=IP",
        "Type=USB"
    };


    /* ------------------------------------------------------------------------------------------------
    ----------------------------------------- CONSTRUCTEUR  -------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainViewModel(ModelManager modelManager)
    {
        // Initialisation des attributs
        _modelManager = modelManager;
        _selectedModel = new FunctionalModel("NaN");
        _busConnection = _modelManager.BusConnection;
        _busConnection.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_busConnection.IsConnected)) {
                WhenPropertyChanged(nameof(IsConnected)); // Mise à jour de la vue
                ConnectBusCommand?.NotifyCanExecuteChanged(); //ATTENTION, PEUT CAUSER PROBLÈMES APRÈS L'UPGRADE
                DisconnectBusCommand?.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(_busConnection.CurrentInterface))
            {
                WhenPropertyChanged(nameof(CurrentInterface)); // Mise à jour de la vue
            }
            // Pas besoin de WhenPropertyChanged(nameof(NatAddress)) car la View est directement bind à la propriété du Model
            // Contrairement à CurrentInterface, la View est bind à une propriété du ViewModel et donc WhenPropertyChanged(nameof(CurrentInterface))
        };
        
        _busConnection.SelectedConnectionType = "Type=USB";
        
        ProjectFolderPath = "";

        
    
        
        
        // Initialisation des commandes
        ConsoleAndLogWriteLineCommand = new Commands.RelayCommand<string>(
            parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter))
                    modelManager.Logger.ConsoleAndLogWriteLine(parameter);
            }
        );

        ExtractGroupAddressCommand = new Commands.RelayCommand<object>(
            _ => modelManager.GroupAddressManager.ExtractGroupAddress()
        );

        EnsureSettingsFileExistsCommand = new Commands.RelayCommand<string>(
            parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter))
                    modelManager.ApplicationFileManager.EnsureSettingsFileExists(parameter);
            }
        );

        CreateDebugArchiveCommand = new Commands.RelayCommand<(bool IncludeOsInfo, bool IncludeHardwareInfo, bool IncludeImportedProjects)>(
            parameters =>
            {
                modelManager.DebugArchiveGenerator.CreateDebugArchive(
                    parameters.IncludeOsInfo,
                    parameters.IncludeHardwareInfo,
                    parameters.IncludeImportedProjects
                );
            }
        );

        FindZeroXmlCommand = new Commands.RelayCommand<string>(
            fileName =>
            {
                if (fileName != null) modelManager.FileFinder.FindZeroXml(fileName);
            }
        );

        ConnectBusCommand = new AsyncRelayCommand(modelManager.BusConnection.ConnectBusAsync);

        DisconnectBusCommand = new AsyncRelayCommand(modelManager.BusConnection.DisconnectBusAsync);

        RefreshInterfacesCommand = new AsyncRelayCommand(modelManager.BusConnection.DiscoverInterfacesAsync);

        // A implémenter, sera liée au bouton Connexion NAT
        ConnectRemotelyCommand = new Commands.RelayCommand<object>(
            _ => modelManager.GroupCommunication.GroupValueWriteOnAsync()
        );

        // A supprimer plus tard, utilisée pour tester
        TestRechercherCommand = new AsyncRelayCommand(modelManager.BusConnection.ClearField);
        

        GroupValueWriteOnCommand = new Commands.RelayCommand<object>(
            _ => modelManager.GroupCommunication.GroupValueWriteOnAsync()
        );

        GroupValueWriteOffCommand = new Commands.RelayCommand<object>(
            _ => modelManager.GroupCommunication.GroupValueWriteOffAsync()
        );


        // Initialisation des commandes
        GroupValueWriteCommand = new Commands.RelayCommand<(GroupAddress, GroupValue)>(
            async parameters =>
            {
                await modelManager.GroupCommunication.GroupValueWriteAsync(parameters.Item1, parameters.Item2);
            }
        );

        MaGroupValueReadCommand = new Commands.RelayCommand<GroupAddress>(
            async groupAddress =>
            {
                await modelManager.GroupCommunication.MaGroupValueReadAsync(groupAddress);
                // Vous pouvez faire quelque chose avec la valeur lue ici si nécessaire
            }
        );


        SaveSettingsCommand = new Commands.RelayCommand<object>(
            _ => modelManager.AppSettings.Save()
        );

        ExtractGroupAddressFileCommand = new RelayCommandWithResult<string, bool>(
            fileName => modelManager.ProjectFileManager.ExtractGroupAddressFile(fileName)
        );

        ExtractProjectFilesCommand = new RelayCommandWithResult<string, bool>(
            fileName =>
            {
                var success = _modelManager.ProjectFileManager.ExtractProjectFiles(fileName);
                ProjectFolderPath = _modelManager.ProjectFileManager.ProjectFolderPath;
                return success;
            }
        );

        // Gestion des colonnes 
        HideModelColumnCommand = new RelayCommand(HideModelColumn);
        HideAdressColumnCommand = new RelayCommand(HideAdressColumn);
        ShowModelColumnCommand = new RelayCommand(ShowModelColumn);
        ShowAdressColumnCommand = new RelayCommand(ShowAdressColumn);


        //Gestion des modèles -----------------------------------------------------------------------
        _functionalModelDictionary = new FunctionalModelDictionary();

        /*// Ajout de 3 modèles par défaut
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Modèle par défaut 1"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Modèle par défaut 2"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(3, "Modèle 3"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Modèle 4"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Boby Lapointe"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(3, "Modèle 5"));*/

        // Chargement des modèles dans la collection observable
        Models = new ObservableCollection<FunctionalModel>(_functionalModelDictionary.GetAllModels());

        //Sauvegarde des modèles --------------------------------------------------------------------

        GenerateReportCommand =
            new Commands.RelayCommand<(string fileName, string authorName)>(args =>
                _modelManager.PdfDocumentCreator.CreatePdf(args.fileName, args.authorName));

    }


}
