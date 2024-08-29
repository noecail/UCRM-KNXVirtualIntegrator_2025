using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Knx.Falcon;
using Knx.Falcon.KnxnetIp;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Knx.Falcon.Configuration;
using Knx.Falcon.Sdk;

namespace KNX_PROJET_2
{
    public class MainViewModel : ViewModelBase
    {
        private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

        public KnxBus _bus;
        public CancellationTokenSource _cancellationTokenSource;

        // Propriétés liées à l'interface utilisateur //test
        public ObservableCollection<InterfaceViewModel> GroupAddresses { get; private set; }
        public ObservableCollection<InterfaceViewModel> DiscoveredInterfaces { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand ConnectCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand RefreshInterfacesCommand { get; private set; }
        public ICommand TypeConnectionCommand { get; set; }


        public GroupCommunicationViewModel GroupCommunicationVM { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => Set(ref _isBusy, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set => Set(ref _isConnected, value);
        }

        private string _connectionState;
        public string ConnectionState
        {
            get => _connectionState;
            private set => Set(ref _connectionState, value);
        }

        private string _selectedConnectionType;
        public string SelectedConnectionType
        {
            get => _selectedConnectionType;
            set
            {
                if (_selectedConnectionType != value)
                {
                    _selectedConnectionType = value;
                    
                    // Exécuter la commande lorsque la sélection change
                    OnSelectedConnectionTypeChanged();
                }
            }
        }
        
        private async void OnSelectedConnectionTypeChanged()
        {
            try
            {
                await DiscoverInterfacesAsync();
            }
            catch (Exception ex)
            {
                // Gestion d'erreur
                Console.WriteLine($"Erreur lors de la découverte des interfaces: {ex.Message}");
            }
        }

        
        public MainViewModel()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            GroupAddresses = new ObservableCollection<InterfaceViewModel>();
            DiscoveredInterfaces = new ObservableCollection<InterfaceViewModel>();

            ImportCommand = new RelayCommand(async () => await ImportListGroupAddress());
            ConnectCommand = new RelayCommand(async () => await ConnectBusAsync());
            DisconnectCommand = new RelayCommand(async () => await DisconnectBusAsync());
            RefreshInterfacesCommand = new RelayCommand(async () => await DiscoverInterfacesAsync());
            
            GroupCommunicationVM = new GroupCommunicationViewModel(this);

            
        }

        //GESTIONNAIRE EVENEMENT POUR GROUPCOMMUNICATIONVIEWMODEL
        public event EventHandler<KnxBus> BusConnectedReady;
        protected virtual void OnBusConnectedReady(KnxBus bus)
        {
            BusConnectedReady?.Invoke(this, bus);
        }
        //------------//

        private async Task ImportListGroupAddress()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Sélectionner un fichier XML",
                Filter = "Fichiers XML|*.xml|Tous les fichiers|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    XDocument doc = XDocument.Load(filePath);
                    var allGroupAddresses = doc.Descendants(_globalKnxNamespace + "GroupAddress").ToList();

                    GroupAddresses.Clear();
                    // J'AI DU CHANGER ICI, A VOIR SI LE CONNECTORTYPE EST BON
                    foreach (var groupAddress in allGroupAddresses)
                    {
                        var name = groupAddress.Attribute("Name")?.Value;
                        var address = groupAddress.Attribute("Address")?.Value;

                        var interfaceViewModel = new InterfaceViewModel(
                            ConnectorType.Usb, 
                            $"{name} - {address}",
                            address 
                        );

                        // Ajouter l'instance à GroupAddresses
                        GroupAddresses.Add(interfaceViewModel);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement du fichier XML : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ConnectBusAsync()
        {
            if (IsBusy)
                return;

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Le bus est occupé le temps de la connexion
                IsBusy = true;
                
                // Dans connectionString, on a toutes les informations nécessaire pour bien gérer la connexion :
                // Type=IpTunneling;HostAddress=127.0.0.1;SerialNumber=00FA:00000001;MacAddress=060606030E47;ProtocolType=Udp;Name="KNX Virtual "
                var connectionString = SelectedInterface?.ConnectionString;

                // Si on n'a pas choisi d'interface, demande d'en choisir une
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    MessageBox.Show("Le type de connexion et la chaîne de connexion doivent être fournis.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Si le bus est déjà connecté, on le déconnecte
                if (_bus != null)
                {
                    _bus.ConnectionStateChanged -= BusConnectionStateChanged;
                    await _bus.DisposeAsync();
                    _bus = null;
                    UpdateConnectionState();
                }

                // A partir de connectionString, on obtient les paramètres de connexion
                var connectorParameters = ConnectorParameters.FromConnectionString(connectionString);

                // Création du bus et connexion
                _bus = new KnxBus(connectorParameters);
                await _bus.ConnectAsync(_cancellationTokenSource.Token);

                // Si le bus est bien connecté, on met à jour son état et les variables qui vont avec
                if (_bus.ConnectionState == BusConnectionState.Connected)
                {
                    _bus.ConnectionStateChanged += BusConnectionStateChanged;
                    IsConnected = true;
                    UpdateConnectionState();
                    OnBusConnectedReady(_bus); //AVERTIR GROUPCOMMUNICATIONVIEWMODEL = cest bon tu peux commencer à ecouter les messages
                    MessageBox.Show("Connexion réussie au bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // Sinon, message d'erreur
                else
                {
                    throw new InvalidOperationException("La connexion au bus a échoué.");
                }
            }
            // Exception si y a besoin
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion au bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // En fin de connexion, on n'est plus occupé
            finally
            {
                IsBusy = false;
                ResetCancellationTokenSource();
                //_cancellationTokenSource.Dispose();
                //_cancellationTokenSource = null;
            }
        }

        private async Task DisconnectBusAsync()
        {
            if (IsBusy || !IsConnected)
            {
                MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;

            try
            {
                if (_bus != null)
                {
                    _bus.ConnectionStateChanged -= BusConnectionStateChanged;
                    await _bus.DisposeAsync();
                    _bus = null;
                    IsConnected = false;
                    UpdateConnectionState();
                    MessageBox.Show("Déconnexion réussie du bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la déconnexion du bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateConnectionState()
        {
            ConnectionState = IsConnected ? "Connected" : "Disconnected";
        }

        private void BusConnectionStateChanged(object sender, EventArgs e)
        {
            UpdateConnectionState();
        }

        public async Task DiscoverInterfacesAsync()
        {
            try
            {
                // Créer un objet ObservableCollection pour stocker les interfaces découvertes
                var discoveredInterfaces = new ObservableCollection<InterfaceViewModel>();

                // Découverte des interfaces IP
                if (SelectedConnectionType == "System.Windows.Controls.ComboBoxItem : Type=IP")
                {
                    var ipDiscoveryTask = Task.Run(async () =>
                {

                        using (var cts = new CancellationTokenSource())
                        {

                            var results = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);


                            await foreach (var result in results)
                            {
                                // Traitement des connexions tunneling
                                foreach (var tunnelingServer in result.GetTunnelingConnections())
                                {
                                    // Mettre à jour les connexions existantes ou ajouter de nouvelles connexions
                                    if (result.IsExtensionOf != null)
                                    {
                                        var existing = discoveredInterfaces.FirstOrDefault(i => i.ConnectionString == result.IsExtensionOf.ToConnectionString());
                                        if (existing != null)
                                        {
                                            existing.ConnectionString = tunnelingServer.ToConnectionString();
                                            continue;
                                        }
                                    }

                                    var displayName = tunnelingServer.IndividualAddress.HasValue
                                        ? $"{tunnelingServer.Name} {tunnelingServer.HostAddress} ({tunnelingServer.IndividualAddress.Value})"
                                        : $"{tunnelingServer.Name} {tunnelingServer.HostAddress}";
                                    var tunneling = new InterfaceViewModel(ConnectorType.IpTunneling, displayName, tunnelingServer.ToConnectionString());
                                    discoveredInterfaces.Add(tunneling);
                                }

                                // Traitement des connexions IP routing
                                if (result.Supports(ServiceFamily.Routing, 1))
                                {
                                    var routingParameters = IpRoutingConnectorParameters.FromDiscovery(result);
                                    var routing = new InterfaceViewModel(ConnectorType.IpRouting, $"{result.MulticastAddress} on {result.LocalIPAddress}", routingParameters.ToConnectionString());
                                    discoveredInterfaces.Add(routing);
                                }
                            }
                        }
                    });
                    // Attendre que toutes les découvertes soient terminées
                    await Task.WhenAll(ipDiscoveryTask);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Mettre à jour les interfaces pour l'utilisateur
                        DiscoveredInterfaces.Clear();
                        foreach (var discoveredInterface in discoveredInterfaces)
                        {
                            DiscoveredInterfaces.Add(discoveredInterface);
                        }
                    });
                }

                // Découverte des périphériques USB
                if (SelectedConnectionType == "System.Windows.Controls.ComboBoxItem : Type=USB")
                {
                    var usbDiscoveryTask = Task.Run(() =>
                    {
                        foreach (var usbDevice in KnxBus.GetAttachedUsbDevices())
                        {
                            var interfaceViewModel = new InterfaceViewModel(ConnectorType.Usb, usbDevice.DisplayName, usbDevice.ToConnectionString());
                           discoveredInterfaces.Add(interfaceViewModel);
                        }
                    });
                    // Attendre que toutes les découvertes soient terminées
                    await Task.WhenAll(usbDiscoveryTask);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Mettre à jour les interfaces pour l'utilisateur
                        DiscoveredInterfaces.Clear();
                        foreach (var discoveredInterface in discoveredInterfaces)
                        {
                            DiscoveredInterfaces.Add(discoveredInterface);
                        }
                    });
                }
                              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la découverte des interfaces : {ex.Message}");
            }
        }
        
        private void ResetCancellationTokenSource()
        {
            // Si l'ancien TokenSource existe, le supprimer pour éviter les fuites de mémoire
            _cancellationTokenSource?.Dispose();

            // Réinitialiser le TokenSource
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private InterfaceViewModel _selectedInterface;
        public InterfaceViewModel SelectedInterface
        {
            get => _selectedInterface;
            set => Set(ref _selectedInterface, value);
        }
    }
}
