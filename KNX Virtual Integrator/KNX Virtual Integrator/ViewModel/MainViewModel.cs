using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.ViewModel.Commands;
using System.ComponentModel;
using System.Windows;
using Knx.Falcon;
using KNX_Virtual_Integrator.Model.Implementations;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.View;


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
    public ObservableCollection<string> ConnectionTypes { get; } = [
        "IP",
        "IP à distance (NAT)",
        "USB"
    ];


    /* ------------------------------------------------------------------------------------------------
    ----------------------------------------- CONSTRUCTEUR  -------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainViewModel(ModelManager modelManager)
    {
        // Initialisation des attributs
        _modelManager = modelManager;
        _busConnection = _modelManager.BusConnection;
        _busConnection.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_busConnection.IsConnected))
            {
                WhenPropertyChanged(nameof(IsConnected)); // Notification de la view
                ConnectBusCommand?.NotifyCanExecuteChanged(); //ATTENTION, PEUT CAUSER PROBLÈMES APRÈS L'UPGRADE
                DisconnectBusCommand?.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(_busConnection.CurrentInterface))
            {
                WhenPropertyChanged(nameof(CurrentInterface)); // Mise à jour de la vue
            }
            // Pas besoin de WhenPropertyChanged(nameof(NatAddress)) car la View est directement bind à la propriété du Model
            // Contrairement à CurrentInterface, la View est bind à une propriété du ViewModel et donc WhenPropertyChanged(nameof(CurrentInterface))
            else if (e.PropertyName == nameof(_busConnection.SelectedConnectionType))
            {
                if (_busConnection.SelectedConnectionType is "IP")
                {
                    DiscoveredInterfacesVisibility = Visibility.Visible;
                    RemoteConnexionVisibility = Visibility.Collapsed;
                    SecureConnectionVisibility = Visibility.Visible;
                }
                else if (_busConnection.SelectedConnectionType is "IP à distance (NAT)")
                {
                    DiscoveredInterfacesVisibility = Visibility.Collapsed;
                    RemoteConnexionVisibility = Visibility.Visible;
                    SecureConnectionVisibility = Visibility.Visible;
                }
                else if (_busConnection.SelectedConnectionType is "USB")
                {
                    DiscoveredInterfacesVisibility = Visibility.Visible;
                    RemoteConnexionVisibility = Visibility.Collapsed;
                    SecureConnectionVisibility = Visibility.Collapsed;
                }
            }
            else if (e.PropertyName == nameof(_busConnection.ConnectionErrorMessage))
            {
                if (_busConnection.ConnectionErrorMessage == "") ErrorMessageVisibility = Visibility.Collapsed;
                else ErrorMessageVisibility = Visibility.Visible;
            }
        };
        
        //Gestion des modèles -----------------------------------------------------------------------
        _functionalModelList = new FunctionalModelList();
        _functionalModelList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_functionalModelList.FunctionalModels))
            {
                // Updating the Models list, using Clear and Add commands triggers the Observable to send a notification to the UI
                Models?.Clear();
                var newModels = new ObservableCollection<FunctionalModel>(_functionalModelList.FunctionalModels);
                foreach (var newModel in newModels)
                {
                    //Console.WriteLine("New model from fml  : " + newModel.Name);
                    Models?.Add(newModel);
                    //Console.WriteLine("New model in Models : " + Models?.Last());
                }

                // Also, selecting the newly created model and scrolling down to it
                //SelectedModel = Models?.Last();
                //Console.WriteLine("SelectedModel : " + SelectedModel);
                WhenPropertyChanged(nameof(ScrollToEnd));
            }
        };

        _busConnection.SelectedConnectionType = "USB";

        ProjectFolderPath = "";





        // Initialisation des commandes
        ConsoleAndLogWriteLineCommand = new Commands.RelayCommand<string>(parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter))
                    modelManager.Logger.ConsoleAndLogWriteLine(parameter);
            }
        );

        ExtractGroupAddressCommand =
            new Commands.RelayCommand<object>(_ => modelManager.GroupAddressManager.ExtractGroupAddress()
            );

        EnsureSettingsFileExistsCommand = new Commands.RelayCommand<string>(parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter))
                    modelManager.ApplicationFileManager.EnsureSettingsFileExists(parameter);
            }
        );

        CreateDebugArchiveCommand =
            new Commands.RelayCommand<(bool IncludeOsInfo, bool IncludeHardwareInfo, bool IncludeImportedProjects
                )>(parameters =>
                {
                    modelManager.DebugArchiveGenerator.CreateDebugArchive(
                        parameters.IncludeOsInfo,
                        parameters.IncludeHardwareInfo,
                        parameters.IncludeImportedProjects
                    );
                }
            );

        FindZeroXmlCommand = new Commands.RelayCommand<string>(fileName =>
            {
                if (fileName != null) modelManager.FileFinder.FindZeroXml(fileName);
            }
        );

        CreateFunctionalModelDictionaryCommand = new Commands.RelayCommand<object>(_ =>
            {
                _functionalModelList?.AddToDictionary(new FunctionalModel("New Model"));
                //ConsoleAndLogWriteLineCommand.Execute(Models?.Count);
            }
        );

        DeleteFunctionalModelDictionaryCommand = new Commands.RelayCommand<int>(parameter =>
            {
                _functionalModelList?.DeleteFromDictionary(parameter);
            }
        );

        AddTestedElementToModel = new Commands.RelayCommand<FunctionalModel>(model =>
            {
                model?.AddElement(new TestedElement());
            }
        );

        RemoveTestedElementFromModel = new Commands.RelayCommand<(FunctionalModel model, int index)>(parameters =>
            {
                parameters.model.RemoveElement(parameters.index);
            }
        );
        
        AddTestToElement = new Commands.RelayCommand<TestedElement>(parameter =>
            {
                parameter?.AddTest();
            }
        );
        
        RemoveTestFromElement = new Commands.RelayCommand<(TestedElement element, int index)>(parameters =>
            {
                parameters.element.RemoveTest(parameters.index);
            }
        );
        
        AddDptToElement = new Commands.RelayCommand<TestedElement>(parameter =>
            {
                parameter?.AddDpt(1,"",[]);
            }
        );
        
        RemoveDptFromElement = new Commands.RelayCommand<(TestedElement element, int index)>(parameters =>
            {
                parameters.element.RemoveDpt(parameters.index);
            }
        );

        ConnectBusCommand = new AsyncRelayCommand(
            async _ =>
            {
                _busConnection.NatAccess = false;
                await modelManager.BusConnection.ConnectBusAsync();
            });
        DisconnectBusCommand = new AsyncRelayCommand(modelManager.BusConnection.DisconnectBusAsync);

        RefreshInterfacesCommand = new AsyncRelayCommand(modelManager.BusConnection.DiscoverInterfacesAsync);

        ConnectBusRemotelyCommand = new AsyncRelayCommand(
           async _ =>
        {
            _busConnection.NatAccess = true;
            await modelManager.BusConnection.ConnectBusAsync();
        });

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
        
        //Crée une liste avec tous les messages reçus pendant 2 secondes
        MaGroupValueReadCommandWithinTimer = new Commands.RelayCommand<GroupAddress>(
            async groupAddress =>
            {
                var msglist = new List<GroupCommunication.GroupMessage>(await modelManager.GroupCommunication.GroupValuesWithinTimerAsync(groupAddress,2000));
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


        

        // Chargement des modèles par défaut dans la collection observable
        Models = new ObservableCollection<FunctionalModel>(_functionalModelList.FunctionalModels);

        //Sauvegarde des modèles --------------------------------------------------------------------

        GenerateReportCommand =
            new Commands.RelayCommand<(string fileName, string authorName)>(args =>
                _modelManager.PdfDocumentCreator.CreatePdf(args.fileName, args.authorName));

    }


}
