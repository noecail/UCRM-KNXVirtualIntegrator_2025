using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.Model.Interfaces;
using ICommand = KNX_Virtual_Integrator.ViewModel.Commands.ICommand;
using System.ComponentModel;


namespace KNX_Virtual_Integrator.ViewModel
{
  
    public partial class MainViewModel
    {
        /* ------------------------------------------------------------------------------------------------
            ------------------------------------------- ATTRIBUTS  --------------------------------------------
            ------------------------------------------------------------------------------------------------ */
        
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public new event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Property that stores the path to the Project folder.
        /// </summary>
        public string ProjectFolderPath { get; private set; } // Stocke le chemin du dossier projet
        
        /// <summary>
        /// The instance of the KNX Bus Connection to inform the connection window.
        /// </summary>
        /// <seealso cref="View.Windows.ConnectionWindow"/>
        public IBusConnection BusConnection { get; }

        /// <summary>
        /// Direct reference to the settings so that the windows may access them directly.
        /// </summary>
        public IApplicationSettings AppSettings => _modelManager.AppSettings;
        
        /// <summary>
        /// The reference to the ModelManager to communicate with the Back-end.
        /// </summary>
        private readonly ModelManager _modelManager;  // Référence à ModelManager

        /// <summary>
        /// Direct reference to the discovered interfaces of the Back-end so that the windows may access them directly
        /// </summary>
        /// <seealso cref="View.Windows.ConnectionWindow"/>
        public ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces => BusConnection.DiscoveredInterfaces;
        
        /// <summary>
        /// Gets or sets the <see cref="Model.Implementations.BusConnection.SelectedConnectionType"/>
        /// by allowing the windows to access to it.
        /// It calls <see cref="Model.Implementations.BusConnection.OnSelectedConnectionTypeChanged"/>.
        /// </summary>
        public string? SelectedConnectionType
        {
            get => BusConnection.SelectedConnectionType;
            set
            {
                if (BusConnection.SelectedConnectionType == value) return;
                BusConnection.SelectedConnectionType = value;
                BusConnection.OnSelectedConnectionTypeChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the <see cref="Model.Implementations.BusConnection.SelectedInterface"/>
        /// by allowing the windows to access to it.
        /// </summary>
        public ConnectionInterfaceViewModel? SelectedInterface
        {
            get => BusConnection.SelectedInterface;
            set
            {
                if (BusConnection.SelectedInterface == value) return;
                BusConnection.SelectedInterface = value;
            }
        }
        /// <summary>
        /// Direct reference to <see cref="Model.Implementations.BusConnection.IsConnected"/>  so that the windows may access them directly.
        /// </summary>
        public bool IsConnected => BusConnection.IsConnected;
        /// <summary>
        /// Direct reference to <see cref="Model.Implementations.BusConnection.CurrentInterface"/>  so that the windows may access them directly.
        /// </summary>
        public string CurrentInterface => BusConnection.CurrentInterface;

        /* ------------------------------------------------------------------------------------------------
         -------------------------------- COMMANDES SANS VALEUR DE RETOUR  ---------------------------------
         ------------------------------------------------------------------------------------------------ */
        // Pattern d'utilisation :
        // MaCommande.Execute(Args)
        //
        // Si la fonction n'a pas d'arguments, la déclarer en tant que commande dont les paramètres sont de type "object"
        // et lors de l'utilisation, on écrira MaCommande.Execute(null);
        // Pour un exemple, voir : ExtractGroupAddressCommand


        /// <summary>
        /// Command that writes a line of text to the console and log if the provided parameter is not null or whitespace.
        /// </summary>
        public ICommand ConsoleAndLogWriteLineCommand { get; private set; }

        /// <summary>
        /// Command that extracts a group address using the GroupAddressManager.
        /// </summary>
        public ICommand ExtractGroupAddressCommand { get; private set; }


        /// <summary>
        /// Command that creates a debug archive with optional OS info, hardware info, and imported projects.
        /// </summary>
        /// <param name="../SettingsWindow.xaml.IncludeOsInfo">Specifies whether to include OS information in the debug archive.</param>
        /// <param name="../SettingsWindow.xaml.IncludeHardwareInfo">Specifies whether to include hardware information in the debug archive.</param>
        /// <param name="../SettingsWindow.xaml.IncludeImportedProjects">Specifies whether to include imported projects in the debug archive.</param>
        public ICommand CreateDebugArchiveCommand { get; private set; }

        /// <summary>
        /// Command that finds a zero XML file based on the provided file name.
        /// </summary>
        /// <param name="../FileFinder.fileName">The name of the file to find.</param>
        public ICommand FindZeroXmlCommand { get; private set; }
        
        /// <summary>
        /// Command that prints the Model given in parameter.
        /// </summary>  
        private ICommand ModelConsoleWriteCommand { get; }
        
        /// <summary>
        /// Command that prints all the Models in <see cref="SelectedModels"/>.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand AllModelsConsoleWriteCommand { get; private set; }
        
        /// <summary>
        /// Command that prints the <see cref="SelectedModel"/>.
        /// </summary>  
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand SelectedModelConsoleWriteCommand { get; private set; }
        
        /// <summary>
        /// Command that prints a structure from the dictionary to the console.
        /// </summary>  
        public ICommand PrintStructureDictionaryCommand { get; private set; }
        
        /// <summary>
        /// Command that creates a structure in the dictionary.
        /// </summary>  
        public ICommand CreateStructureDictionaryCommand { get; private set; }

        /// <summary>
        /// Command that duplicates a structure in the dictionary.
        /// </summary>  
        public ICommand DuplicateStructureDictionaryCommand { get;private set; }
        
        /// <summary>
        /// Command that removes a structure from the dictionary.
        /// </summary>  
        public ICommand DeleteStructureDictionaryCommand { get; private set; }
        
        /// <summary>
        /// Command that adds a <see cref="Model.Entities.TestedElement"/> to the model.
        /// It is not implemented in this version of the app.
        /// </summary>  
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand AddTestedElementToStructureCommand  { get; private set; }
        
        /// <summary>
        /// Command that removes a <see cref="Model.Entities.TestedElement"/> from the model.
        /// It is not implemented in this version of the app.
        /// </summary>  
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand RemoveTestedElementFromStructureCommand  { get; private set; }
        
        /// <summary>
        /// Command that adds a <see cref="Model.Entities.TestedElement"/> to the models of this structure.
        /// </summary>  
        public ICommand AddTestedElementToModelStructureCommand  { get; private set; }
        
        /// <summary>
        /// Command that removes a <see cref="Model.Entities.TestedElement"/> from the models of this structure.
        /// </summary>  
        public ICommand RemoveTestedElementFromModelStructureCommand  { get; private set; }
        
        /// <summary>
        /// Command that adds a test (a row of values) to the <see cref="SelectedModel"/>.
        /// </summary>  
        public ICommand AddTestToElementCommand { get; private set; }
        
        /// <summary>
        /// Command that removes a test (a row of values) from the <see cref="SelectedModel"/>.
        /// </summary>  
        public ICommand RemoveTestFromElementCommand{ get; private set; }
        
        /// <summary>
        /// Command that adds an CMD <see cref="Model.Entities.DataPointType"/> to the <see cref="SelectedStructure"/>.
        /// </summary>  
        public ICommand AddDptCmdToElementStructureCommand {get; private set; }

        /// <summary>
        /// Command that adds an IE <see cref="Model.Entities.DataPointType"/> to the <see cref="SelectedStructure"/>.
        /// </summary>  
        public ICommand AddDptIeToElementStructureCommand {get; private set; }
        
        /// <summary>
        /// Command that deletes an CMD <see cref="Model.Entities.DataPointType"/> from the <see cref="SelectedStructure"/>.
        /// </summary>  
        public AsyncRelayCommand<Tuple<int,int>> RemoveCmdDptFromElementStructureCommand  { get; private set; }

        /// <summary>
        /// Command that deletes an IE <see cref="Model.Entities.DataPointType"/> from the <see cref="SelectedStructure"/>.
        /// </summary>  
        public AsyncRelayCommand<Tuple<int,int>> RemoveIeDptFromElementStructureCommand  { get; private set; }
        
        /// <summary>
        /// Command that adds a <see cref="Model.Entities.FunctionalModel"/> to the <see cref="SelectedStructure"/>.
        /// </summary>  
        public ICommand AddFunctionalModelToListCommand { get; private set; }
          
        /// <summary>
        /// Command that deletes a <see cref="Model.Entities.FunctionalModel"/> from the <see cref="SelectedStructure"/>.
        /// </summary>  
        public ICommand DeleteFunctionalModelFromListCommand { get; private set; }
        
        /// <summary>
        /// Command that adds a <see cref="Model.Entities.DataPointType"/> to the structure dictionary. 
        /// </summary>
        public ICommand AddDptToDictionaryCommand { get; private set; }
       
        /// <summary>
        /// Command that removes a <see cref="Model.Entities.DataPointType"/> from the structure dictionary. 
        /// </summary>
        public ICommand RemoveDptFromDictionaryCommand { get; private set; }
        
        /// <summary>
        /// Command that undoes the changes done in <see cref="View.Windows.StructureEditWindow"/>
        /// to the <see cref="SelectedStructure"/>.
        /// </summary>
        public ICommand UndoChangesSelectedStructureCommand {get; private set;}
        
        /// <summary>
        /// Command that applies the changes done in <see cref="View.Windows.StructureEditWindow"/>
        /// to the <see cref="SelectedStructure"/>.
        /// </summary>
        public ICommand ApplyChangesSelectedStructureCommand {get; private set;}
        
        /// <summary>
        /// Command that imports the structure dictionary.
        /// </summary>
        public ICommand ImportDictionaryCommand  { get; private set; }
        
        /// <summary>
        /// Command that exports/saves the structure dictionary.
        /// </summary>
        public ICommand ExportDictionaryCommand  { get; private set; }
        
        /// <summary>
        /// Command that imports list of models.
        /// </summary>
        public ICommand ImportListCommand  { get; private set; }
        
        /// <summary>
        /// Command that exports/saves the list of models.
        /// </summary>
        public ICommand ExportListCommand  { get; private set; }
        
        /// <summary>
        /// Command that connects the computer to the KNX Bus.
        /// </summary>
        public AsyncRelayCommand ConnectBusCommand { get; }
        
        /// <summary>
        /// Command that disconnects the computer from the KNX Bus.
        /// </summary>
        public AsyncRelayCommand DisconnectBusCommand { get; }
        
        /// <summary>
        /// Command that refreshes the discovered interfaces.
        /// </summary>
        public AsyncRelayCommand RefreshInterfacesCommand { get; }
        
        /// <summary>
        /// Command that saves the current application settings.
        /// </summary>
        public ICommand SaveSettingsCommand { get; private set; }

        /// <summary>
        /// Command that generates the report for the latest opened project.
        /// </summary>
        public ICommand GenerateReportCommand { get; private set; }

       

        /* ------------------------------------------------------------------------------------------------
        -------------------------------- COMMANDES AVEC VALEUR DE RETOUR  ---------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Pattern d'utilisation :
        // if (_viewModel.NomDeLaCommande is RelayCommandWithResult<typeDuParamètre, typeDeRetour> command)
        // {
        //      [UTILISATION DE LA COMMANDE]
        // }
        // else
        // {
        //      [GESTION DE L'ERREUR SI LA COMMANDE N'EST PAS DU TYPE ESPÉRÉ]
        // }
        //
        //
        // Ou sinon :
        // var maCommande = _viewModel.NomDeLaCommande as RelayCommandWithResult<typeDuParamètre, typeDeRetour>;
        //
        // if (maCommande != null) [UTILISATION DE maCommande DIRECTEMENT]


        /// <summary>
        /// Command that extracts a group address file based on the provided file name and returns a boolean indicating success.
        /// </summary>
        /// <param name="../FileFinder.fileName">The name of the file to extract.</param>
        /// <returns>True if the extraction was successful; otherwise, false.</returns>
        public ICommand ExtractGroupAddressFileCommand { get; private set; }

        /// <summary>
        /// Command that extracts project files based on the provided file name and returns a boolean indicating success.
        /// </summary>
        /// <param name="../FileFinder.fileName">The name of the file to extract.</param>
        /// <returns>True if the extraction was successful; otherwise, false.</returns>
        public ICommand ExtractProjectFilesCommand { get; private set; }
        
        /// <summary>
        /// Command that launches the analysis of the <see cref="SelectedTestModels"/>.
        /// </summary>
        public ICommand LaunchAnalysisCommand {get; private set; }




        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- HANDLERS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */


        /// <summary>
        /// Handles the event when the left mouse button is pressed down on the slider.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse button event.</param>
        public void SliderMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _modelManager.SettingsSliderClickHandler.SliderMouseLeftButtonDown(sender, e);

        /// <summary>
        /// Handles the event when the left mouse button is released on the slider.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse button event.</param>
        public void SliderMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _modelManager.SettingsSliderClickHandler.SliderMouseLeftButtonUp(sender, e);

        /// <summary>
        /// Handles the event when the mouse is moved over the slider while dragging.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse movement event.</param>
        public void SliderMouseMove(object sender, MouseEventArgs e) => _modelManager.SettingsSliderClickHandler.SliderMouseMove(sender, e);

        /// <summary>
        /// Handles the event when the slider is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the routed event.</param>
        public void OnSliderClick(object sender, RoutedEventArgs e) => _modelManager.SettingsSliderClickHandler.OnSliderClick(sender, e);

        /// <summary>
        /// Method used to fire the <see cref="PropertyChanged"/> event with
        /// the propertyName as its <see cref="PropertyChangedEventArgs"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        private void WhenPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

}

