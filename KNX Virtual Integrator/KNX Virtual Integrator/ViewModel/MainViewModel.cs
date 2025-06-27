using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.ViewModel.Commands;
using ICommand = KNX_Virtual_Integrator.ViewModel.Commands.ICommand;
using System.ComponentModel;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon;
using KNXIntegrator.Models;

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
    public event PropertyChangedEventHandler? PropertyChanged;


    /* ------------------------------------------------------------------------------------------------
    ----------------------------------------- CONSTRUCTEUR  -------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainViewModel(ModelManager modelManager)
    {
        // Initialisation des attributs
        _modelManager = modelManager;
        
        _busConnection = _modelManager.BusConnection;
        _busConnection.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != nameof(_busConnection.IsConnected)) return;
            OnPropertyChanged(nameof(IsConnected)); // Mise à jour de la vue
            ConnectBusCommand?.RaiseCanExecuteChanged();
            DisconnectBusCommand?.RaiseCanExecuteChanged();
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

        ConnectBusCommand = new RelayCommand(ConnectBusTask);

        DisconnectBusCommand = new RelayCommand(DisconnectBusTask);

        RefreshInterfacesCommand = new RelayCommand(RefreshInterfacesTask);

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
                var groupValue = await modelManager.GroupCommunication.MaGroupValueReadAsync(groupAddress);
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

        // Ajout de 3 modèles par défaut
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Modèle par défaut 1"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Modèle par défaut 2"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(3, "Modèle 3"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Modèle 4"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(2, "Boby Lapointe"));
        _functionalModelDictionary.Add_FunctionalModel(new FunctionalModel(3, "Modèle 5"));

        // Chargement des modèles dans la collection observable
        Models = new ObservableCollection<FunctionalModel>(_functionalModelDictionary.GetAllModels());

        //Sauvegarde des modèles --------------------------------------------------------------------

        GenerateReportCommand =
            new Commands.RelayCommand<(string fileName, string authorName)>(args =>
                _modelManager.PdfDocumentCreator.CreatePdf(args.fileName, args.authorName));
        
        return;

        async void ConnectBusTask() => await modelManager.BusConnection.ConnectBusAsync();
        async void DisconnectBusTask() => await modelManager.BusConnection.DisconnectBusAsync();
        async void RefreshInterfacesTask() => await modelManager.BusConnection.DiscoverInterfacesAsync();

    }


}
