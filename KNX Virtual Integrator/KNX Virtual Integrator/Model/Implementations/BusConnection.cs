using GalaSoft.MvvmLight;
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

public sealed class BusConnection : ObservableObject ,IBusConnection
{
    public static XNamespace GlobalKnxNamespace = "http://knx.org/xml/ga-export/01"; // Namespace utilisé pour les opérations KNX

    /// <summary>
    /// Représente l'objet de connexion au bus KNX. Peut être nul si aucune connexion n'est établie.
    /// </summary>
    public KnxBus? Bus;

    /// <summary>
    /// Permet l'annulation d'opérations asynchrones en cours. Peut être nul si aucune opération n'est en cours.
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource;
    
    /// <summary>
    /// Collection observable des interfaces de connexion découvertes.
    /// </summary>
    public ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; private set; }

    /// <summary>
    /// Indique si le bus est actuellement connecté. Utilisé pour lier l'état de connexion à l'interface utilisateur.
    /// </summary>
    public bool IsBusConnected => IsConnected;

    /// <summary>
    /// Propriété privée qui stocke l'interface actuellement sélectionnée.
    /// </summary>
    private ConnectionInterfaceViewModel? _selectedInterface;

    /// <summary>
    /// Interface actuellement sélectionnée. La liaison avec l'interface utilisateur est mise à jour automatiquement.
    /// </summary>
    public ConnectionInterfaceViewModel? SelectedInterface
    {
        get => _selectedInterface;
        set => Set(ref _selectedInterface, value); // Notifie l'interface utilisateur des changements
    }

    /// <summary>
    /// Indique si une tâche ou une opération est en cours (par exemple, une connexion). Utilisé pour désactiver l'interface utilisateur pendant les opérations.
    /// </summary>
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => Set(ref _isBusy, value); // Notifie l'interface utilisateur des changements
    }

    /// <summary>
    /// Indique si le bus est connecté ou non. Liaison à l'interface utilisateur pour afficher l'état de la connexion.
    /// </summary>
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value) return; // Pas de changement si la valeur est la même
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected)); // Notifie l'interface utilisateur du changement
        }
    }

    /// <summary>
    /// État actuel de la connexion (par exemple, "Connecté" ou "Déconnecté"). Mis à jour automatiquement.
    /// </summary>
    private string? _connectionState;
    public string? ConnectionState
    {
        get => _connectionState;
        private set => Set(ref _connectionState, value); // Met à jour et notifie l'interface utilisateur
    }

    /// <summary>
    /// Type de connexion sélectionné par l'utilisateur (par exemple, Ethernet, USB, WiFi). Les changements de cette propriété sont propagés à l'interface utilisateur.
    /// </summary>
    private string _selectedConnectionType;
    public string SelectedConnectionType
    {
        get => _selectedConnectionType;
        set
        {
            if (_selectedConnectionType != value)
            {
                _selectedConnectionType = value;
                OnSelectedConnectionTypeChanged(); // Traite les changements de type de connexion
            }
        }
    }


    //GESTIONNAIRE EVENEMENT POUR GROUPCOMMUNICATIONVIEWMODEL
    
    /// <summary>
    /// Événement déclenché lorsque la connexion au bus KNX est prête.
    /// </summary>
    public event EventHandler<KnxBus> BusConnectedReady;

    /// <summary>
    /// Méthode protégée qui déclenche l'événement <see cref="BusConnectedReady"/>.
    /// </summary>
    /// <param name="bus">L'objet <see cref="KnxBus"/> qui représente la connexion au bus KNX.</param>
    private void OnBusConnectedReady(KnxBus bus)
    {
        BusConnectedReady.Invoke(this, bus); // Déclenche l'événement si des abonnés existent
    }


    /// <summary>
    /// Méthode asynchrone appelée lorsque le type de connexion sélectionné change.
    /// Cette méthode tente de découvrir les interfaces disponibles en appelant <see cref="DiscoverInterfacesAsync"/>.
    /// En cas d'erreur, elle capture l'exception et affiche un message dans la console.
    /// </summary>
    public async void OnSelectedConnectionTypeChanged()
    {
        try
        {
            await DiscoverInterfacesAsync(); // Appel asynchrone pour découvrir les interfaces
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la console
            Console.WriteLine($"Erreur lors de la découverte des interfaces: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Établit une connexion au bus KNX de manière asynchrone.
    /// Cette méthode vérifie d'abord si une connexion est déjà en cours. Si ce n'est pas le cas, elle procède à la connexion en utilisant les informations de connexion fournies.
    /// En cas de succès, elle met à jour l'état de connexion et notifie les abonnés de la connexion établie. En cas d'échec, elle affiche un message d'erreur.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de connexion asynchrone.</returns>
    public async Task ConnectBusAsync()
    {
        if (IsBusy)
            return; // Retourne immédiatement si une opération est déjà en cours

        CancellationTokenSource = new CancellationTokenSource(); // Initialise le token d'annulation

        try
        {
            IsBusy = true; // Indique que le bus est occupé pendant la connexion

            // Obtient la chaîne de connexion à partir de l'interface sélectionnée
            var connectionString = SelectedInterface?.ConnectionString;

            // Vérifie si la chaîne de connexion est fournie
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Le type de connexion et la chaîne de connexion doivent être fournis.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Interrompt la méthode si les informations de connexion sont manquantes
            }

            // Déconnecte le bus existant si nécessaire
            if (Bus != null)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged!;
                await Bus.DisposeAsync();
                Bus = null;
                UpdateConnectionState(); // Met à jour l'état de connexion
            }

            // Crée un nouvel objet de bus avec les paramètres extraits de la chaîne de connexion
            var connectorParameters = ConnectorParameters.FromConnectionString(connectionString);
            Bus = new KnxBus(connectorParameters);

            // Établit la connexion au bus
            await Bus.ConnectAsync(CancellationTokenSource.Token);

            // Vérifie si la connexion est établie avec succès
            if (Bus.ConnectionState == BusConnectionState.Connected)
            {
                Bus.ConnectionStateChanged += BusConnectionStateChanged!;
                IsConnected = true;
                UpdateConnectionState(); // Met à jour l'état de connexion
                OnBusConnectedReady(Bus); // Notifie les abonnés que la connexion est prête
                MessageBox.Show("Connexion réussie au bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                throw new InvalidOperationException("La connexion au bus a échoué."); // Lance une exception si la connexion échoue
            }
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la fenêtre
            MessageBox.Show($"Erreur lors de la connexion au bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false; // Réinitialise l'état d'occupation
            ResetCancellationTokenSource(); // Réinitialise le token d'annulation
        }
    }

    /// <summary>
    /// Déconnecte de manière asynchrone du bus KNX.
    /// Cette méthode vérifie d'abord si le bus est connecté et s'il y a une opération en cours. Si le bus est connecté, elle procède à la déconnexion.
    /// En cas de succès, elle met à jour l'état de connexion et affiche un message de succès. En cas d'erreur, elle affiche un message d'erreur.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de déconnexion asynchrone.</returns>
    public async Task DisconnectBusAsync()
    {
        if (IsBusy || !IsConnected)
        {
            MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return; // Retourne immédiatement si le bus est déjà déconnecté ou si une opération est en cours
        }

        await CancellationTokenSource?.CancelAsync()!; // Annule les opérations en cours
        CancellationTokenSource = null; // Réinitialise le token d'annulation

        try
        {
            if (Bus != null)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged!;
                await Bus.DisposeAsync(); // Déconnecte et libère les ressources du bus
                Bus = null;
                IsConnected = false;
                UpdateConnectionState(); // Met à jour l'état de connexion
                MessageBox.Show("Déconnexion réussie du bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la fenêtre
            MessageBox.Show($"Erreur lors de la déconnexion du bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Découvre les interfaces de connexion disponibles de manière asynchrone en fonction du type de connexion sélectionné.
    /// Cette méthode identifie les interfaces IP et USB disponibles et les ajoute à la collection <see cref="DiscoveredInterfaces"/>.
    /// Les résultats de la découverte sont mis à jour sur l'interface utilisateur une fois la découverte terminée.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de découverte asynchrone.</returns>
    public async Task DiscoverInterfacesAsync()
    {
        try
        {
            // Crée une collection observable pour stocker les interfaces découvertes
            var discoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();

            // Vérifie si le type de connexion sélectionné est "IP"
            if (SelectedConnectionType is "System.Windows.Controls.ComboBoxItem : Type=IP" or "Type=IP")
            {
                // Démarre une tâche pour découvrir les interfaces IP
                var ipDiscoveryTask = Task.Run(async () =>
                {
                    using var cts = new CancellationTokenSource();
                    // Découvre les dispositifs IP de manière asynchrone
                    var results = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);

                    // Traite chaque résultat de découverte
                    await foreach (var result in results)
                    {
                        // Traite les connexions tunneling
                        foreach (var tunnelingServer in result.GetTunnelingConnections())
                        {
                            // Met à jour les connexions existantes ou ajoute de nouvelles connexions
                            if (result.IsExtensionOf != null)
                            {
                                var existing = discoveredInterfaces.FirstOrDefault(i => i.ConnectionString == result.IsExtensionOf.ToConnectionString());
                                if (existing != null)
                                {
                                    existing.ConnectionString = tunnelingServer.ToConnectionString();
                                    continue;
                                }
                            }

                            // Crée une vue pour chaque serveur tunneling découvert
                            var displayName = tunnelingServer.IndividualAddress.HasValue
                                ? $"{tunnelingServer.Name} {tunnelingServer.HostAddress} ({tunnelingServer.IndividualAddress.Value})"
                                : $"{tunnelingServer.Name} {tunnelingServer.HostAddress}";
                            var tunneling = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling, displayName, tunnelingServer.ToConnectionString());
                            discoveredInterfaces.Add(tunneling);
                        }

                        // Traite les connexions IP routing si supportées
                        if (result.Supports(ServiceFamily.Routing, 1))
                        {
                            var routingParameters = IpRoutingConnectorParameters.FromDiscovery(result);
                            var routing = new ConnectionInterfaceViewModel(ConnectorType.IpRouting, $"{result.MulticastAddress} on {result.LocalIPAddress}", routingParameters.ToConnectionString());
                            discoveredInterfaces.Add(routing);
                        }
                    }
                });

                // Attends que la tâche de découverte des interfaces IP soit terminée
                await Task.WhenAll(ipDiscoveryTask);

                // Met à jour la collection des interfaces découvertes sur l'interface utilisateur
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DiscoveredInterfaces.Clear();
                    foreach (var discoveredInterface in discoveredInterfaces)
                    {
                        DiscoveredInterfaces.Add(discoveredInterface);
                    }
                });
            }

            // Vérifie si le type de connexion sélectionné est "USB"
            if (SelectedConnectionType is "System.Windows.Controls.ComboBoxItem : Type=USB" or "Type=USB")
            {
                // Démarre une tâche pour découvrir les périphériques USB
                var usbDiscoveryTask = Task.Run(() =>
                {
                    foreach (var usbDevice in KnxBus.GetAttachedUsbDevices())
                    {
                        var interfaceViewModel = new ConnectionInterfaceViewModel(ConnectorType.Usb, usbDevice.DisplayName, usbDevice.ToConnectionString());
                        discoveredInterfaces.Add(interfaceViewModel);
                    }
                });

                // Attends que la tâche de découverte des périphériques USB soit terminée
                await Task.WhenAll(usbDiscoveryTask);

                // Met à jour la collection des interfaces découvertes sur l'interface utilisateur
                Application.Current.Dispatcher.Invoke(() =>
                {
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
            // Affiche un message d'erreur en cas d'exception
            MessageBox.Show($"Erreur lors de la découverte des interfaces : {ex.Message}");
        }
    }
    
    /// <summary>
    /// Met à jour l'état de connexion en fonction de la valeur de la propriété <see cref="IsConnected"/>.
    /// Définit la propriété <see cref="ConnectionState"/> sur "Connected" si <see cref="IsConnected"/> est vrai,
    /// sinon sur "Disconnected".
    /// </summary>
    private void UpdateConnectionState()
    {
        ConnectionState = IsConnected ? "Connected" : "Disconnected";
    }

    /// <summary>
    /// Méthode de gestionnaire d'événements pour les changements d'état de connexion du bus.
    /// Cette méthode est appelée lorsque l'état de connexion du bus change et met à jour l'état de connexion
    /// en appelant <see cref="UpdateConnectionState"/>.
    /// </summary>
    /// <param name="sender">L'objet qui a déclenché l'événement.</param>
    /// <param name="e">Les arguments de l'événement.</param>
    private void BusConnectionStateChanged(object sender, EventArgs e)
    {
        UpdateConnectionState();
    }

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="BusConnection"/>.
    /// Crée une nouvelle instance d'ObservableCollection pour stocker les interfaces découvertes.
    /// </summary>
    public BusConnection()
    {
        DiscoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();
    }

    /// <summary>
    /// Réinitialise le token d'annulation en le disposant s'il existe, puis en créant un nouveau.
    /// Cette méthode est utilisée pour préparer un nouveau token d'annulation pour les opérations en cours.
    /// </summary>
    private void ResetCancellationTokenSource()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}