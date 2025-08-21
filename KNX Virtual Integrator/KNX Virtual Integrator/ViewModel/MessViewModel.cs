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
        public string ProjectFolderPath { get; private set; } // Stocke le chemin du dossier projet
        
        public IBusConnection BusConnection { get; }

        public IApplicationSettings AppSettings => _modelManager.AppSettings;
        private readonly ModelManager _modelManager;  // Référence à ModelManager

        public ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces => BusConnection.DiscoveredInterfaces;
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
        public ConnectionInterfaceViewModel? SelectedInterface
        {
            get => BusConnection.SelectedInterface;
            set
            {
                if (BusConnection.SelectedInterface == value) return;
                BusConnection.SelectedInterface = value;
            }
        }
        public bool IsConnected => BusConnection.IsConnected;
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
        
        private ICommand ModelConsoleWriteCommand { get; set; }
        public ICommand AllModelsConsoleWriteCommand { get; private set; }
        public ICommand SelectedModelConsoleWriteCommand { get; private set; }

        
        public ICommand CreateStructureDictionaryCommand { get; private set; }

        public ICommand DuplicateStructureDictionaryCommand { get;private set; }
        
        public ICommand DeleteStructureDictionaryCommand { get; private set; }
        
        public ICommand AddTestedElementToStructureCommand  { get; private set; }
        
        public ICommand RemoveTestedElementFromStructureCommand  { get; private set; }
        
        public ICommand AddTestedElementToModelStructureCommand  { get; private set; }
        
        public ICommand RemoveTestedElementFromModelStructureCommand  { get; private set; }
        
        public ICommand AddTestToElementCommand { get; private set; }
        
        public ICommand RemoveTestFromElementCommand{ get; private set; }
        
        public ICommand AddDptCmdToElementStructureCommand {get; private set; }

        public ICommand AddDptIeToElementStructureCommand {get; private set; }
        
        public AsyncRelayCommand<Tuple<int,int>> RemoveCmdDptFromElementStructureCommand  { get; private set; }

        public AsyncRelayCommand<Tuple<int,int>> RemoveIeDptFromElementStructureCommand  { get; private set; }
        
        public ICommand AddFunctionalModelToListCommand { get; private set; }
            
        public ICommand DeleteFunctionalModelFromListCommand { get; private set; }
        
        public ICommand AddDptToDictionaryCommand { get; private set; }
            
        public ICommand RemoveDptFromDictionaryCommand { get; private set; }
        
        public ICommand UpdateFunctionalModelListCommand { get; private set; }
        
        public ICommand ImportDictionaryCommand  { get; private set; }

        public ICommand ExportDictionaryCommand  { get; private set; }

        public ICommand ImportListCommand  { get; private set; }

        public ICommand ExportListCommand  { get; private set; }
        
        public AsyncRelayCommand ConnectBusCommand { get; }
        
        public AsyncRelayCommand DisconnectBusCommand { get; }
        
        public AsyncRelayCommand RefreshInterfacesCommand { get; }


        /// <summary>
        /// Command that saves the current application settings.
        /// </summary>
        public ICommand SaveSettingsCommand { get; private set; }

        /// <summary>
        /// Command that generates the report for the latest opened project
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
        /// Command that 
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

        private void WhenPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

}

