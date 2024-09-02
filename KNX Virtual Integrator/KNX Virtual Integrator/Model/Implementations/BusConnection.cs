using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Knx.Falcon;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon.Configuration;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon.Sdk;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class BusConnection : ObservableObject ,IBusConnection
{
    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

    public KnxBus? Bus;
    public CancellationTokenSource? CancellationTokenSource;

    // Propriétés liées à l'interface utilisateur //test
    public ObservableCollection<ConnectionInterfaceViewModel>? GroupAddresses { get; private set; }
    public ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; private set; }
    
    public bool IsBusConnected => IsConnected;

    private ConnectionInterfaceViewModel? _selectedInterface;
    public ConnectionInterfaceViewModel? SelectedInterface
    {
        get => _selectedInterface;
        set => Set(ref _selectedInterface, value);
    }
    
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => Set(ref _isBusy, value);
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value) return;
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
        }
    }

    private string? _connectionState;
    public string? ConnectionState
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
                OnSelectedConnectionTypeChanged();
            }
        }
    }


    //GESTIONNAIRE EVENEMENT POUR GROUPCOMMUNICATIONVIEWMODEL
    public event EventHandler<KnxBus> BusConnectedReady;
    protected virtual void OnBusConnectedReady(KnxBus bus)
    {
        BusConnectedReady?.Invoke(this, bus);
    }
    //------------//

    public async void OnSelectedConnectionTypeChanged()
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
    
    public async Task ConnectBusAsync()
    {
        if (IsBusy)
            return;

        CancellationTokenSource = new CancellationTokenSource();

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
            if (Bus != null)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged;
                await Bus.DisposeAsync();
                Bus = null;
                UpdateConnectionState();
            }

            // À partir de connectionString, on obtient les paramètres de connexion
            var connectorParameters = ConnectorParameters.FromConnectionString(connectionString);

            // Création du bus et connexion
            Bus = new KnxBus(connectorParameters);
            await Bus.ConnectAsync(CancellationTokenSource.Token);

            // Si le bus est bien connecté, on met à jour son état et les variables qui vont avec
            if (Bus.ConnectionState == BusConnectionState.Connected)
            {
                Bus.ConnectionStateChanged += BusConnectionStateChanged;
                IsConnected = true;
                UpdateConnectionState();
                OnBusConnectedReady(Bus); //AVERTIR GROUPCOMMUNICATIONVIEWMODEL = cest bon tu peux commencer à ecouter les messages
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
        }
    }
    
    public async Task DisconnectBusAsync()
    {
        if (IsBusy || !IsConnected)
        {
            MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        CancellationTokenSource?.Cancel();
        CancellationTokenSource = null;

        try
        {
            if (Bus != null)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged;
                await Bus.DisposeAsync();
                Bus = null;
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
    
    public async Task DiscoverInterfacesAsync()
    {
        try
        {
            // Créer un objet ObservableCollection pour stocker les interfaces découvertes
            var discoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();

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
                                var tunneling = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling, displayName, tunnelingServer.ToConnectionString());
                                discoveredInterfaces.Add(tunneling);
                            }

                            // Traitement des connexions IP routing
                            if (result.Supports(ServiceFamily.Routing, 1))
                            {
                                var routingParameters = IpRoutingConnectorParameters.FromDiscovery(result);
                                var routing = new ConnectionInterfaceViewModel(ConnectorType.IpRouting, $"{result.MulticastAddress} on {result.LocalIPAddress}", routingParameters.ToConnectionString());
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
                        var interfaceViewModel = new ConnectionInterfaceViewModel(ConnectorType.Usb, usbDevice.DisplayName, usbDevice.ToConnectionString());
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
    
    private void UpdateConnectionState()
    {
        ConnectionState = IsConnected ? "Connected" : "Disconnected";
    }

    private void BusConnectionStateChanged(object sender, EventArgs e)
    {
        UpdateConnectionState();
    }
    
    public BusConnection()
    {
        DiscoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();
    }
    
    private void ResetCancellationTokenSource()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();
    }
    
    private void BusConnection_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BusConnection.IsConnected))
        {
            OnPropertyChanged(nameof(IsConnected));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}