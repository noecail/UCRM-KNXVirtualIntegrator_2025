using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.ViewModel.Commands;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
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
    /* ------------------------------------------------------------------------------------------------
    ----------------------------------------- CONSTRUCTEUR  -------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Constructor of the ViewModel, subscribes everything concerning the viewModel and initializes every other attribute.
    /// </summary>
    /// <param name="modelManager"></param>
    public MainViewModel(ModelManager modelManager)
    {
        // Initialisation des attributs
        _modelManager = modelManager;
        BusConnection = _modelManager.BusConnection;
        BusConnection.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BusConnection.IsConnected))
            {
                WhenPropertyChanged(nameof(IsConnected)); // Notification de la view
                ConnectBusCommand?.NotifyCanExecuteChanged(); //ATTENTION, PEUT CAUSER PROBLÈMES APRÈS L'UPGRADE
                DisconnectBusCommand?.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(BusConnection.CurrentInterface))
            {
                WhenPropertyChanged(nameof(CurrentInterface)); // Mise à jour de la vue
            }
            // Pas besoin de WhenPropertyChanged(nameof(NatAddress)) car la View est directement bind à la propriété du Model
            // Contrairement à CurrentInterface, la View est bind à une propriété du ViewModel et donc WhenPropertyChanged(nameof(CurrentInterface))
            else if (e.PropertyName == nameof(BusConnection.SelectedConnectionType))
            {
                if (BusConnection.SelectedConnectionType is "IP")
                {
                    DiscoveredInterfacesVisibility = Visibility.Visible;
                    RemoteConnexionVisibility = Visibility.Collapsed;
                    SecureConnectionVisibility = Visibility.Visible;
                }
                else if (BusConnection.SelectedConnectionType is "Remote IP (NAT)")
                {
                    DiscoveredInterfacesVisibility = Visibility.Collapsed;
                    RemoteConnexionVisibility = Visibility.Visible;
                    SecureConnectionVisibility = Visibility.Visible;
                }
                else if (BusConnection.SelectedConnectionType is "USB")
                {
                    DiscoveredInterfacesVisibility = Visibility.Visible;
                    RemoteConnexionVisibility = Visibility.Collapsed;
                    SecureConnectionVisibility = Visibility.Collapsed;
                }
            }
            else if (e.PropertyName == nameof(BusConnection.ConnectionErrorMessage))
            {
                ErrorMessageVisibility = BusConnection.ConnectionErrorMessage==""? Visibility.Collapsed : Visibility.Visible;
            }
        };
        
        //Gestion des modèles -----------------------------------------------------------------------
        _functionalModelList = new FunctionalModelList();
        Structures = [];
        

        _functionalModelList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_functionalModelList.FunctionalModelDictionary))
            {
                // Updating the Models list, using Clear and Add commands triggers the Observable to send a notification to the UI
                Structures.Clear();
                var newStructures =
                    new ObservableCollection<FunctionalModelStructure>(_functionalModelList.GetAllModels());
                foreach (var newstructure in newStructures)
                    Structures.Add(newstructure);
            }
            else if (e.PropertyName == nameof(_functionalModelList.FunctionalModelDictionary.FunctionalModels))
            {
                // Updating the Models list, using Clear and Add commands triggers the Observable to send a notification to the UI
                Structures.Clear();
                var newStructures =
                    new ObservableCollection<FunctionalModelStructure>(_functionalModelList.GetAllModels());
                foreach (var newstructure in newStructures)
                    Structures.Add(newstructure);
            }
        };

        BusConnection.SelectedConnectionType = "USB";

        ProjectFolderPath = "";

        AllModelsConsoleWriteCommand = new Commands.RelayCommand<object>(_ => 
            {
                foreach (var modelList in _functionalModelList.FunctionalModels)
                {
                    foreach (var model in modelList)
                    {
                        //model.UpdateValue();
                        ModelConsoleWriteCommand?.Execute(model);
                    }
                }
                
            }
        );

        HideApplyChangesErrorMessageCommand = new Commands.RelayCommand<object>(_ => 
            ApplyChangesErrorMessageVisibility = Visibility.Hidden
        );

        ShowReportGenerationSuccessMessageCommand = new Commands.RelayCommand<object>(_ => 
            ReportGenerationSuccessMessageVisibility = Visibility.Visible
        );

        HideReportGenerationSuccessMessageCommand = new Commands.RelayCommand<object>(_ => 
            ReportGenerationSuccessMessageVisibility = Visibility.Hidden
        );

        SelectedModelConsoleWriteCommand = new Commands.RelayCommand<object>(_ =>
            ModelConsoleWriteCommand?.Execute(SelectedModel)
        );

        ModelConsoleWriteCommand = new Commands.RelayCommand<FunctionalModel>(model =>
            {
                if (model is null)
                { 
                    Console.WriteLine("Model is null"); 
                    return;
                }
                Console.WriteLine("Accessing the model : " + model.Name);
                var i = 1;
                    
                if (model.ElementList.Count == 0)
                {
                    Console.WriteLine(model.Name + " is empty");
                    return;
                }
                
                
                foreach (var testedelement in model.ElementList)
                {
                    Console.WriteLine("--- Tested Element : " + i + "---");

                    Console.Write("DPT(s) to send         : ");
                    foreach (var dpttosend in testedelement.TestsCmd)
                        Console.Write(dpttosend.Type + "|");
                    Console.WriteLine();
                    Console.Write("Address(es) to send    : ");
                    foreach (var dpttosend in testedelement.TestsCmd)
                        Console.Write(dpttosend.Address + "|");
                    Console.WriteLine();
                    Console.Write("Value(s) to send       : ");
                    foreach (var dpttosend in testedelement.TestsCmd)
                    {
                        foreach (var value in dpttosend.IntValue)
                            Console.Write(value.BigIntegerValue + ",");
                        Console.Write("|");
                    }

                    Console.WriteLine();

                    Console.Write("DPT(s) to receive      : ");
                    foreach (var dpttoreceive in testedelement.TestsIe)
                        Console.Write(dpttoreceive.Type + "|");
                    Console.WriteLine();
                    Console.Write("Address(es) to receive : ");
                    foreach (var dpttoreceive in testedelement.TestsIe)
                        Console.Write(dpttoreceive.Address + "|");
                    Console.WriteLine();
                    Console.Write("Value(s) to receive    : ");
                    foreach (var dpttoreceive in testedelement.TestsIe)
                    {
                        foreach (var value in dpttoreceive.IntValue)
                            Console.Write(value.BigIntegerValue + ",");
                        Console.Write("|");
                    }

                    Console.WriteLine();
                    Console.WriteLine("----------------------------");
                    Console.WriteLine();
                    i++;
                }
            }
        );


        // Initialisation des commandes
        ConsoleAndLogWriteLineCommand = new Commands.RelayCommand<string>(parameter =>
            {
                if (!string.IsNullOrWhiteSpace(parameter))
                    modelManager.Logger.ConsoleAndLogWriteLine(parameter);
            }
        );

        ExtractGroupAddressCommand =
            new Commands.RelayCommand<object>(_ => 
                GroupAddressFile = modelManager.GroupAddressManager.ExtractGroupAddress(_functionalModelList)
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
        
        PrintStructureDictionaryCommand = new Commands.RelayCommand<object>(_ =>
        {
            Console.WriteLine("############-- STRUCTURE DICTIONARY --############");
            Console.WriteLine("A total of " + _functionalModelList.FunctionalModelDictionary.FunctionalModels.Count + " structures");
            foreach(var structure in _functionalModelList.FunctionalModelDictionary.FunctionalModels)
                Console.WriteLine("structure " + structure.FullName);
        });

        CreateStructureDictionaryCommand = new Commands.RelayCommand<object>(_ =>
            {
                _functionalModelList.AddToDictionary(new FunctionalModelStructure("New_Model"),false);
            }
        );

        DuplicateStructureDictionaryCommand = new Commands.RelayCommand<object>(_ =>
            {
                if (SelectedStructure!=null && Structures.Count != 0)
                    _functionalModelList.AddToDictionary(new FunctionalModelStructure(new FunctionalModel(SelectedStructure.Model,Structures.Count+1,false),_functionalModelList.FunctionalModelDictionary.FunctionalModels.Count+1),false);
            }
        );

        DeleteStructureDictionaryCommand = new Commands.RelayCommand<int>(index =>
            {
                // clear selected models if the selected structure is deleted 
                if (SelectedStructure?.Model.Key-1 == index)
                    SelectedModels?.Clear();
                
                // save the previously selected structure and model
                var previouslySelectedStructure = SelectedStructure;
                var previouslySelectedModel = SelectedModel;
                // unselect the structure and model
                SelectedStructure = null;
                SelectedModel = null;
                _functionalModelList.ReinitializeNbModels(index);

                
                // delete the structure
                _functionalModelList.DeleteFromDictionary(index);
                
                // restore (or not) the previously selected structure and model
                if (previouslySelectedStructure is null) return;
                if (!_functionalModelList.FunctionalModelDictionary.FunctionalModels.Contains(
                        previouslySelectedStructure) ||
                    _functionalModelList.FunctionalModelDictionary.FunctionalModels[
                        _functionalModelList.FunctionalModelDictionary.FunctionalModels.IndexOf(
                            previouslySelectedStructure)].Model.Name != previouslySelectedStructure.Model.Name) return;
                SelectedStructure =  previouslySelectedStructure;
                SelectedModel = previouslySelectedModel;
            }
        );

        AddTestedElementToStructureCommand = new Commands.RelayCommand<FunctionalModel>(model =>
            {
                model?.AddElement(new TestedElement([1], [""], [[new GroupValue(true)]], [1], [""], [[new GroupValue(true)]]));
                WhenPropertyChanged(nameof(Structures));
            }
        );

        RemoveTestedElementFromStructureCommand = new Commands.RelayCommand<(FunctionalModel model, int index)>(parameters =>
            {
                parameters.model.RemoveElement(parameters.index);
            }
        );

        AddTestedElementToModelStructureCommand = new Commands.RelayCommand<ObservableCollection<ElementStructure>>(modelStructure =>
            {
                modelStructure?.Add(new ElementStructure([0],[]));
            }
        );
        
        RemoveTestedElementFromModelStructureCommand = new Commands.RelayCommand<(ObservableCollection<ElementStructure> modelStructure, int index)>(parameters =>
            {
                parameters.modelStructure.RemoveAt(parameters.index);
            }
        );
        
        AddTestToElementCommand = new Commands.RelayCommand<TestedElement>(parameter =>
            {
                parameter?.AddTest();
            }
        );
        
        RemoveTestFromElementCommand = new Commands.RelayCommand<(TestedElement element, int index)>(parameters =>
            {
                parameters.element.RemoveTest(parameters.index);
            }
        );
        
        AddTestToElementStructureCommand = new Commands.RelayCommand<ElementStructure>(parameter =>
            {
                parameter?.AddTest();
            }
        );
        
        RemoveTestFromElementStructureCommand = new Commands.RelayCommand<(ElementStructure elementStructure, int index)>(parameters =>
            {
                parameters.elementStructure.RemoveTestAt(parameters.index);
            }
        );
        
        AddDptCmdToElementStructureCommand = new Commands.RelayCommand<ElementStructure>(parameter =>
            {
                parameter?.AddToCmd(0);
            }
        );
        
        AddDptIeToElementStructureCommand = new Commands.RelayCommand<ElementStructure>(parameter =>
            {
                parameter?.AddToIe(0);
            }
        );
        
        // For some reason that I do not understand, commands that are directly bound to buttons (and not called via the code-behind, using th Click event) need to be Async in order to execute
        RemoveCmdDptFromElementStructureCommand = new AsyncRelayCommand<Tuple<int,int>>(tuple  =>
            {
                if (tuple == null) return Task.CompletedTask;
                var (elementIndex, cmdIndex) = tuple;

                ElementStructure? elementStructure = null;
                if (SelectedStructure != null)
                    elementStructure = SelectedStructure.ModelStructure[elementIndex];
                if (elementStructure?.Cmd.Count > 1)
                {
                    elementStructure.RemoveCmdAt(cmdIndex);
                }

                return Task.CompletedTask;
            }
        );
        
        RemoveIeDptFromElementStructureCommand = new AsyncRelayCommand<Tuple<int,int>>(tuple  =>
            {
                if (tuple == null) return Task.CompletedTask;
                var (elementIndex, ieIndex) = tuple;

                ElementStructure? elementStructure = null;
                if (SelectedStructure != null)
                    elementStructure = SelectedStructure.ModelStructure[elementIndex];
                elementStructure?.RemoveIeAt(ieIndex);
                return Task.CompletedTask;
            }
        );

        AddFunctionalModelToListCommand = new Commands.RelayCommand<object>(model =>
            {
                if (SelectedStructure != null)
                {
                    if (model is FunctionalModel myModel)
                        _functionalModelList.AddToList(SelectedStructure.Model.Key - 1, myModel, false);
                    else
                        _functionalModelList.AddToList(SelectedStructure.Model.Key - 1);
                    SelectedModels = SelectedStructure != null ? _functionalModelList.FunctionalModels[SelectedStructure.Model.Key - 1] : null;
                    SelectedModel = SelectedModels?.Last();
                    /*
                     * Je le garde dans un coin pour le moment
                     * SelectedModels?.Clear();
                     * foreach (var newmodel in _functionalModelList.FunctionalModels[SelectedStructure.Key - 1])
                     * SelectedModels?.Add(newmodel);
                     * */
                }
            }
        );
        
        DeleteFunctionalModelFromListCommand = new Commands.RelayCommand<int>(indexModel =>
            {
                // save the previously selected model 
                var previouslySelectedModel = SelectedModel;
                // unselect the structure and model
                SelectedModel = null;
                SelectedModels = null;
                
                if (SelectedStructure != null)
                {
                    var indexStructure = SelectedStructure.Model.Key-1;
                    _functionalModelList.DeleteFromList(indexStructure, indexModel);
                }
                
                SelectedModels = SelectedStructure != null ? _functionalModelList.FunctionalModels[SelectedStructure.Model.Key-1] : null;
               
                // restore (or not) the previously selected model
                if (SelectedModels is null || previouslySelectedModel is null || SelectedStructure is null) return;
                if (SelectedModels.Contains(previouslySelectedModel) && SelectedModels[_functionalModelList.FunctionalModels[SelectedStructure.Model.Key-1].IndexOf(previouslySelectedModel)].Name == previouslySelectedModel.Name)
                    SelectedModel = previouslySelectedModel;
            }
        );

        AddDptToDictionaryCommand = new Commands.RelayCommand<FunctionalModelStructure>(structure =>
        {
            structure?.CreateDpt();
        });
        
        RemoveDptFromDictionaryCommand = new Commands.RelayCommand<(int key, FunctionalModelStructure structure)>(parameters =>
        {
            // delete types in the element structures if they have selected the soon-to-be-removed dpt
            var keyToRemove = parameters.key;
            foreach (var elementStructure in parameters.structure.ModelStructure)
            {
                foreach (var cmd in elementStructure.Cmd)
                    if (cmd.Value == keyToRemove)
                        cmd.Value = 0;

                foreach (var ie in elementStructure.Ie)
                    if (ie.Value == keyToRemove)
                        ie.Value = 0;
            }
            
            // remove the dpt
            parameters.structure.RemoveDpt(parameters.key);
        });

        UndoChangesSelectedStructureCommand = new Commands.RelayCommand<object>(_ =>
        {
            if (EditedStructureSave == null || SelectedStructure == null) return;

            var index = SelectedStructure.Model.Key-1;
            
            // save the previously selected structure and model
            //var previouslySelectedStructure = SelectedStructure;
            // unselect the structure and model
            SelectedStructure = null;
            
            // delete the structure
            _functionalModelList.ResetInDictionary(index, EditedStructureSave);
            OnPropertyChanged(nameof(Structures));
            OnPropertyChanged(nameof(SelectedStructure));
                
            // restore the previously selected structure and model
            SelectedStructure = Structures[index];
        });
        
        ApplyChangesSelectedStructureCommand = new Commands.RelayCommand<object>(_ =>
        {
            if (SelectedStructure == null) return;

            if (!SelectedStructure.IsValid())
            {
                ApplyChangesErrorMessageVisibility = Visibility.Visible;
                return;
            }
            ApplyChangesErrorMessageVisibility = Visibility.Hidden;
            
            int structureKey = SelectedStructure.Model.Key - 1;
            
            // nettoyer liste MFs associés à la structure
            _functionalModelList.FunctionalModels[structureKey].Clear();
            
            // la structure
            var structure = _functionalModelList.FunctionalModelDictionary.FunctionalModels[structureKey];
            // construire le nouveau modèle selon la structure
            structure.Model = structure.BuildFunctionalModel(structure.Model.Name, structureKey+1);
            
            // reconstruire la liste de MFs
            _functionalModelList.ReinitializeNbModels(structureKey);
            _functionalModelList.AddToList(structureKey);
            
            // update UI
            var source = _functionalModelList.FunctionalModels[SelectedStructure.Model.Key-1];
            SelectedModels = new ObservableCollection<FunctionalModel>(source);
            SelectedModel = SelectedModels.First();
        });
            
        ExportDictionaryCommand = new Commands.RelayCommand<string>(path =>
            {
                if (path != null)
                    _functionalModelList.ExportDictionary(path);
            }
        );
        
        ImportDictionaryCommand = new Commands.RelayCommand<string>(path =>
            {
                SelectedTestModels.Clear();
                ChosenModelsAndState.Clear();
                LastTestResults.Clear();
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
                SelectedTestModels.Clear();
                ChosenModelsAndState.Clear();
                LastTestResults.Clear();
                if (path != null)
                    _functionalModelList.ImportList(path);
            }
        );
        
        ExportListAndDictionaryCommand = new Commands.RelayCommand<string>(path =>
            {
                if (path != null)
                {
                    if (GroupAddressFile == null)
                    {
                        _functionalModelList.ExportListAndDictionary(path,
                            _modelManager.ProjectFileManager.ProjectName);
                    }
                    else
                    {
                        _functionalModelList.ExportListAndDictionary(path,
                            _modelManager.ProjectFileManager.ProjectName,GroupAddressFile);
                    }
                }
            }
        );
                
        ImportListAndDictionaryCommand = new Commands.RelayCommand<string>(path =>
            {
                SelectedTestModels.Clear();
                ChosenModelsAndState.Clear();
                LastTestResults.Clear();
                if (path != null)
                {
                    var res = _functionalModelList.ImportListAndDictionaryWithDoc(path);
                    _modelManager.ProjectFileManager.ProjectName = res.Item1;
                    GroupAddressFile = res.Item2;
                    _modelManager.GroupAddressManager.GroupAddressStructure =
                        _modelManager.GroupAddressManager.DetermineGroupAddressStructureGroupAddressFile(
                            GroupAddressFile.Root?.Elements());
                    _modelManager.ProjectFileManager.UpdateTitle();
                    
                }
            }
        );


        ConnectBusCommand = new AsyncRelayCommand(
            async _ =>
            {
                BusConnection.NatAccess = false;
                await modelManager.BusConnection.ConnectBusAsync();
            });
        
        DisconnectBusCommand = new AsyncRelayCommand(modelManager.BusConnection.DisconnectBusAsync);

        RefreshInterfacesCommand = new AsyncRelayCommand(modelManager.BusConnection.DiscoverInterfacesAsync);
        
        SaveSettingsCommand = new Commands.RelayCommand<object>(
            _ => modelManager.AppSettings.Save()
        );

        ExtractGroupAddressFileCommand = new RelayCommandWithResult<string, bool>(
        fileName=>
            {
                SelectedTestModels.Clear();
                ChosenModelsAndState.Clear();
                LastTestResults.Clear();
                var success = modelManager.ProjectFileManager.ExtractGroupAddressFile(fileName);
                return success;
            }
        );

        ExtractProjectFilesCommand = new RelayCommandWithResult<string, bool>(
            fileName =>
            {
                SelectedTestModels.Clear();
                ChosenModelsAndState.Clear();
                LastTestResults.Clear();
                var success = _modelManager.ProjectFileManager.ExtractProjectFiles(fileName);
                ProjectFolderPath = _modelManager.ProjectFileManager.ProjectFolderPath;
                return success;
            }
        );
        

        // Chargement des modèles par défaut dans la collection observable
        Structures = new ObservableCollection<FunctionalModelStructure>(_functionalModelList.FunctionalModelDictionary.FunctionalModels);
        SelectedModels = [];
        
        
        //Sauvegarde des modèles --------------------------------------------------------------------

        GenerateReportCommand =
            new Commands.RelayCommand<(string fileName, string authorName, ObservableCollection<FunctionalModel>
                testedList, List<List<List<List<ResultType>>>> testResults)>(args =>
                _modelManager.PdfDocumentCreator.CreatePdf(args.fileName, args.authorName, args.testedList, args.testResults));

        LaunchAnalysisCommand = new RelayCommandWithResult<ObservableCollection<FunctionalModel>, Task<List<List<List<List<ResultType>>>>>>(
            async testModels =>
            {
                Analyze analysis = new Analyze(_modelManager.GroupCommunication);
                AnalysisState = new List<string>(testModels.Count);
                for (int i = 0; i < testModels.Count; i++)
                {
                    AnalysisState.Add("Waiting");
                    ChosenModelsAndState[i].State = "Waiting";
                    WhenPropertyChanged(nameof(AnalysisState));
                }
                analysis.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == "Running")
                    {
                        for (int i = 0; i < AnalysisState.Count; i++)
                        {
                            if (AnalysisState[i] != "Waiting") continue;
                            AnalysisState[i] = "Running";
                            ChosenModelsAndState[i].State = "Running";
                            WhenPropertyChanged(nameof(AnalysisState));
                            return;
                        }
                    } else if (e.PropertyName == "Finished")
                    {
                        for (int i = 0; i < AnalysisState.Count; i++)
                        {
                            if (AnalysisState[i] != "Running") continue;
                            AnalysisState[i] = "Finished";
                            ChosenModelsAndState[i].State = "Finished";
                            WhenPropertyChanged(nameof(AnalysisState));
                            return;
                        }   
                    }
                };
                ConsoleAndLogWriteLineCommand.Execute("Analysis Started");
                await analysis.TestAll(testModels, CommandTimeout, ElementLatency);
                ConsoleAndLogWriteLineCommand.Execute("Analysis Finished");
                LastTestResults = analysis.Results;
                return analysis.Results;
            }
        );
        
    }
}
