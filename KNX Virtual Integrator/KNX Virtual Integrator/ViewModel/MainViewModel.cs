﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.ViewModel.Commands;
using System.ComponentModel;
using System.Windows;
using Knx.Falcon;
using KNX_Virtual_Integrator.Model.Implementations;
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
        //_functionalModelList.ImportDictionary(@"C:\Users\manui\Documents\Stage 4A\Test\Pray.xml");

        _functionalModelList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_functionalModelList.FunctionalModelDictionary))
            {
                // Updating the Models list, using Clear and Add commands triggers the Observable to send a notification to the UI
                Structures?.Clear();
                var newStructures = new ObservableCollection<FunctionalModel>(_functionalModelList.GetAllModels());
                foreach (var newstructure in newStructures)
                {
                    Structures?.Add(newstructure);
                }
                
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
        

        CreateStructureDictionaryCommand = new Commands.RelayCommand<object>(_ =>
            {
                _functionalModelList?.AddToDictionary(new FunctionalModel("New Model " + (Structures?.Count+1)));
            }
        );

        DuplicateStructureDictionaryCommand = new Commands.RelayCommand<object>(_ =>
            {
                if (SelectedStructure!=null && Structures!=null)
                    _functionalModelList?.AddToDictionary(new FunctionalModel(SelectedStructure,Structures.Count+1,false));
            }
        );

        DeleteStructureDictionaryCommand = new Commands.RelayCommand<int>(parameter =>
            {
                _functionalModelList.DeleteFromDictionary(parameter);
                HideModelColumnCommand?.Execute(null);
            }
        );

        AddTestedElementToStructure = new Commands.RelayCommand<FunctionalModel>(model =>
            {
                model?.AddElement(new TestedElement([1], [""], [[new GroupValue(true)]], [1], [""], [[new GroupValue(true)]]));
                //_functionalModelList.FunctionalModels[0].AddElement(new TestedElement());
                WhenPropertyChanged(nameof(Structures));
            }
        );

        RemoveTestedElementFromStructure = new Commands.RelayCommand<(FunctionalModel model, int index)>(parameters =>
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
        
        AddDptCmdToElement = new Commands.RelayCommand<TestedElement>(parameter =>
            {
                parameter?.AddDptToCmd(1,"",[]);
            }
        );
        
        AddDptIeToElement = new Commands.RelayCommand<TestedElement>(parameter =>
            {
                parameter?.AddDptToIe(1,"",[]);
            }
        );
        
        RemoveCmdDptFromElement = new Commands.RelayCommand<(TestedElement element, int index)>(parameters =>
            {
                parameters.element.RemoveDptFromCmd(parameters.index);
            }
        );
        
        RemoveIeDptFromElement = new Commands.RelayCommand<(TestedElement element, int index)>(parameters =>
            {
                parameters.element.RemoveDptFromIe(parameters.index);
            }
        );

        AddFunctionalModelToList = new Commands.RelayCommand<object>(model =>
            {
                if (SelectedStructure!=null)
                    if (model is FunctionalModel myModel)
                        _functionalModelList.AddToList(SelectedStructure.Key-1,myModel);
                    else
                        _functionalModelList.AddToList(SelectedStructure.Key-1);
            }
        );
        
        
        DeleteFunctionalModelFromList = new Commands.RelayCommand<FunctionalModel>(model =>
            {
                if (model != null && SelectedStructure != null)
                {
                    var indexStructure = SelectedStructure.Key - 1;
                    _functionalModelList.DeleteFromList(indexStructure, _functionalModelList.FunctionalModels[indexStructure].IndexOf(model));
                }
            }
        );

        ExportDictionaryCommand = new Commands.RelayCommand<string>(path =>
            {
                if (path != null)
                    _functionalModelList.ExportDictionary(path);
            }
        );
        
        ImportDictionaryCommand = new Commands.RelayCommand<string>(path =>
            {
                if (path != null)
                    _functionalModelList.ImportDictionary(path);
            }
        );
        
        ExportListCommand = new Commands.RelayCommand<string>(path =>
            {
                if (path != null)
                    _functionalModelList.ExportList(path);
            }
        );
        
        ImportListCommand = new Commands.RelayCommand<string>(path =>
            {
                if (path != null)
                    _functionalModelList.ImportList(path);
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
        Structures = new ObservableCollection<FunctionalModel>(_functionalModelList.FunctionalModelDictionary.FunctionalModels);
        SelectedModels = [];
        /*foreach (var model in Models)
            SelectedModels?.Add(model);
        Console.WriteLine("SelectedModels.Count " + SelectedModels?.Count);*/
        
        //Sauvegarde des modèles --------------------------------------------------------------------

        GenerateReportCommand =
            new Commands.RelayCommand<(string fileName, string authorName)>(args =>
                _modelManager.PdfDocumentCreator.CreatePdf(args.fileName, args.authorName));

    }


}
