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
        
        public IBusConnection _busConnection { get; }

        public IApplicationSettings AppSettings => _modelManager.AppSettings;
        private readonly ModelManager _modelManager;  // Référence à ModelManager

        public ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces => _busConnection.DiscoveredInterfaces;
        public string? SelectedConnectionType
        {
            get => _busConnection.SelectedConnectionType;
            set
            {
                if (_busConnection.SelectedConnectionType == value) return;
                _busConnection.SelectedConnectionType = value;
                _busConnection.OnSelectedConnectionTypeChanged();
            }
        }
        public ConnectionInterfaceViewModel? SelectedInterface
        {
            get => _busConnection.SelectedInterface;
            set
            {
                if (_busConnection.SelectedInterface == value) return;
                _busConnection.SelectedInterface = value;
            }
        }
        public bool IsConnected => _busConnection.IsConnected;
        public string? CurrentInterface => _busConnection.CurrentInterface;

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
        /// Command that ensures the settings file exists. Creates the file if it does not exist, using the provided file path.
        /// </summary>
        public ICommand EnsureSettingsFileExistsCommand { get; private set; }

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
        
        public ICommand ModelConsoleWriteCommand { get; private set; }
        
        public ICommand CreateStructureDictionaryCommand { get; private set; }

        public ICommand DuplicateStructureDictionaryCommand { get;private set; }
        
        public ICommand DeleteStructureDictionaryCommand { get; private set; }
        
        public ICommand AddTestedElementToStructureCommand  { get; private set; }
        
        public ICommand RemoveTestedElementFromStructureCommand  { get; private set; }
        
        public ICommand AddTestToElementCommand { get; private set; }
        
        public ICommand RemoveTestFromElementCommand{ get; private set; }
        
        public ICommand AddDptCmdToElementCommand {get; private set; }

        public ICommand AddDptIeToElementCommand {get; private set; }
        
        public ICommand RemoveCmdDptFromElementCommand  { get; private set; }

        public ICommand RemoveIeDptFromElementCommand  { get; private set; }
        
        public ICommand AddFunctionalModelToListCommand { get; private set; }
            
        public ICommand DeleteFunctionalModelFromListCommand { get; private set; }
        
        public ICommand ImportDictionaryCommand  { get; private set; }

        public ICommand ExportDictionaryCommand  { get; private set; }

        public ICommand ImportListCommand  { get; private set; }

        public ICommand ExportListCommand  { get; private set; }

        //public AsyncRelayCommand OpenConnectionWindowCommand { get; }
        public AsyncRelayCommand ConnectBusCommand { get; }
        public AsyncRelayCommand DisconnectBusCommand { get; }
        public AsyncRelayCommand RefreshInterfacesCommand { get; }
        // à implémenter dans le VM
        public AsyncRelayCommand ConnectBusRemotelyCommand { get; private set; }

        /// <summary>
        /// Command that sends a group value write "on" command asynchronously.
        /// </summary>
        public ICommand GroupValueWriteOnCommand { get; private set; }

        /// <summary>
        /// Command that sends a group value write "off" command asynchronously.
        /// </summary>
        public ICommand GroupValueWriteOffCommand { get; private set; }

        public ICommand MaGroupValueReadCommand { get; private set; }
        
        public ICommand MaGroupValueReadCommandWithinTimer { get; private set; }
        
        public ICommand GroupValueWriteCommand { get; private set; }


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

