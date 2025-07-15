using CommunityToolkit.Mvvm.ComponentModel;
using Knx.Falcon;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using System.Security;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon.Configuration;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon.Sdk;
using KNX_Virtual_Integrator.Model.Wrappers;

namespace KNX_Virtual_Integrator.Model.Implementations;

public sealed class BusConnection : ObservableObject ,IBusConnection
{
    public static XNamespace GlobalKnxNamespace = "http://knx.org/xml/ga-export/01"; // Namespace utilisé pour les opérations KNX

    /// <summary>
    /// Représente l'objet de connexion au bus KNX. Peut-être nul si aucune connexion n'est établie.
    /// </summary>
    public IKnxBusWrapper Bus;
    
    private readonly ILogger _logger;


    /// <summary>
    /// Permet l'annulation d'opérations asynchrones en cours. Peut-être nul si aucune opération n'est en cours.
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
        set => SetProperty(ref _selectedInterface, value); // Notifie l'interface utilisateur des changements
    }
    
    /// <summary>
    /// Indique si une tâche ou une opération est en cours (par exemple, une connexion). Utilisé pour désactiver l'interface utilisateur pendant les opérations.
    /// </summary>
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value); // Notifie l'interface utilisateur des changements
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
            WhenPropertyChanged(nameof(IsConnected)); // Notifie l'interface utilisateur du changement
        }
    }

    /// <summary>
    /// État actuel de la connexion (par exemple, "Connecté" ou "Déconnecté"). Mis à jour automatiquement.
    /// </summary>
    private string? _connectionState;
    public string? ConnectionState
    {
        get => _connectionState;
        private set => SetProperty(ref _connectionState, value); // Met à jour et notifie l'interface utilisateur
    }

    /// <summary>
    /// Interface de la connexion actuelle
    /// </summary>
    private string _currentInterface = "Aucune interface connectée";

    public string CurrentInterface
    {
        get => _currentInterface;
        set
        {
            if (_currentInterface == value) return ; // Pas de changement si la valeur est la même 
            _currentInterface = value;
            WhenPropertyChanged(nameof(CurrentInterface)); // Notifie l'interface utilisateur du changement
        }
    }
    
    /// <summary>
    /// Adresse IP du routeur distant permettant la connexion distante au bus KNX
    /// </summary>
    private string _natAddress;
    public string NatAddress
    {
        get => _natAddress;
        set
        {
            if (_natAddress == value) return; // Pas de changement si la valeur est la même 
            _natAddress = value;
            WhenPropertyChanged(nameof(NatAddress)); // Notifie l'interface utilisateur du changement
        }
    }
    
    
    /// <summary>
    /// Choix d'accès à distance via NAT.
    /// </summary>
    public bool NatAccess { get; set; }

    /// <summary>
    /// Port cible pour le NAT.
    /// </summary>b
    public int NatPort = 3671;

    /// <summary>
    /// Adresse de l'interface IP;
    /// </summary>
    public string InterfaceAddress="1.1.255";



    
    /// <summary>
    /// Mot de passe permettant l'accès au fichier de clé pour connexion IP Secure
    /// </summary>bu
    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            if (_password == value) return; // Pas de changement si la valeur est la même 
            _password = value;
            _securePassword.Clear();
            foreach (var c in _password)
                _securePassword.AppendChar(c);
            WhenPropertyChanged(nameof(Password)); // Notifie l'interface utilisateur du changement
        }
    }

    /// <summary>
    /// Mot de passe sécurisé actuellement sélectionné.
    /// </summary>
    private SecureString _securePassword = new SecureString();

    /// <summary>
    /// Chemin du fichier de clés pour connexion IP Secure
    /// /// </summary>
    private string _keysPath = "";
    public string KeysPath
    {
        get => _keysPath;
        set
        {
            if (_keysPath == value) return; // Pas de changement si la valeur est la même 
            _keysPath = value;
            WhenPropertyChanged(nameof(KeysPath)); // Notifie l'interface utilisateur du changement
        }
    }

    /// <summary>
    /// Type de connexion sélectionné par l'utilisateur (par exemple, Ethernet, USB, WiFi). Les changements de cette propriété sont propagés à l'interface utilisateur.
    /// </summary>
    private string? _selectedConnectionType;
    public string? SelectedConnectionType
    {
        get => _selectedConnectionType;
        set
        {
            if (_selectedConnectionType == value) return;
            _selectedConnectionType = value;
            OnSelectedConnectionTypeChanged(); // Traite les changements de type de connexion
        }
    }
    
    /// <summary>
    /// Contient le message d'erreur à afficher dans la fenêtre de connexion après une connexion ratée
    /// Différents cas :
    /// -
    /// - 
    /// - </summary>
    private string _connectionErrorMessage;
    public string ConnectionErrorMessage
    {
        get => _connectionErrorMessage;
        set
        {
            if (_connectionErrorMessage == value) return;
            _connectionErrorMessage = value;
            WhenPropertyChanged(nameof(ConnectionErrorMessage)); // Notifie l'interface utilisateur du changement
        }
    }


    //GESTIONNAIRE ÉVÈNEMENT POUR GROUPCOMMUNICATIONVIEWMODEL
    
    /// <summary>
    /// Événement déclenché lorsque la connexion au bus KNX est prête.
    /// </summary>
    public event EventHandler<IKnxBusWrapper>? BusConnectedReady;               //Le mettre en "nullable" est bien correct?

    /// <summary>
    /// Méthode protégée qui déclenche l'événement <see cref="BusConnectedReady"/>.
    /// </summary>
    /// <param name="bus">L'objet <see cref="KnxBus"/> qui représente la connexion au bus KNX.</param>
    private void OnBusConnectedReady(IKnxBusWrapper bus)
    {
        BusConnectedReady?.Invoke(this, bus); // Déclenche l'événement si des abonnés existent
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedConnectionType)));
            await DiscoverInterfacesAsync(); // Appel asynchrone pour découvrir les interfaces
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la console
            _logger.ConsoleAndLogWrite($"Erreur lors de la découverte des interfaces: {ex.Message}");
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
            // Efface le potentiel message d'erreur dû à la tentative de connexion précédente
            ConnectionErrorMessage = "";
            
            IsBusy = true; // Indique que le bus est occupé pendant la connexion

            // Obtient la chaîne de connexion à partir de l'interface sélectionnée
            var connectionString = SelectedInterface?.ConnectionString;
            
            // Déconnecte le bus existant si nécessaire
            if (!Bus.IsNull)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged!;
                CurrentInterface = "Aucune interface connectée";
                await Bus.DisposeAsync();
                Bus.SetNull = null;
                UpdateConnectionState(); // Met à jour l'état de connexion
            }
            
            if (SelectedConnectionType is "IP à distance (NAT)")
            {
                var parameters = new IpTunnelingConnectorParameters(NatAddress, NatPort, useNat: true) //Crée les paramètres de connexion
                {
                    IndividualAddress = IndividualAddress.Parse(InterfaceAddress),
                    RequiresSecurity = true
                };
                // Crée un nouvel objet de bus avec les paramètres du NAT
                Bus.NewKnxBusWrapper(parameters);
                try
                {
                    await Bus.ConnectAsync(CancellationToken.None);
                    CheckBusConnection();
                }
                catch (Exception ex)
                {
                    _logger.ConsoleAndLogWrite($"Erreur : {ex.GetType().Name} - {ex.Message}");
                    if (ex.Message.Contains("User login failed", StringComparison.OrdinalIgnoreCase)) //Si l'interface est sécurisée
                    {
                        //Efface le message d'erreur dû à la tentative de connexion non sécurisée
                        ConnectionErrorMessage = "";
                        try
                        {
                            await parameters.LoadSecurityDataAsync(KeysPath,_securePassword);
                            Bus.NewKnxBusWrapper(parameters);
                            await Bus.ConnectAsync(CancellationToken.None);
                            CheckBusConnection();
                            _logger.ConsoleAndLogWrite("Connecté en NAT");
                        }
                        catch (Exception e)
                        {
                            CheckError(e);
                        }          
                    }
                    else
                    {
                        CheckError(ex);
                    }
                }
            }
            else if (SelectedConnectionType is "IP") //Si l'interface est IP locale
            {
                // Vérifie si la chaîne de connexion est fournie
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    //MessageBox.Show("Le type de connexion et la chaîne de connexion doivent être fournis.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Interrompt la méthode si les informations de connexion sont manquantes
                }
                
                var parameters = IpTunnelingConnectorParameters.FromConnectionString(connectionString);
             
                // Crée un nouvel objet de bus avec les paramètres extraits de la chaîne de connexion
                Bus.NewKnxBusWrapper(parameters);
                try
                {
                  // Tentative de connexion au bus
                    await Bus.ConnectAsync(CancellationToken.None);
                    CheckBusConnection();
                }
                catch (Exception ex)
                {
                    _logger.ConsoleAndLogWrite($"Erreur : {ex.GetType().Name} - {ex.Message}");
                    // Si l'erreur est user login failed
                    if (ex.Message.Contains("User login failed", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // Ajouter le mot de passe
                            await parameters.LoadSecurityDataAsync(KeysPath,_securePassword);
                         
                            // Créer un bus avec les mots de passe
                            Bus.NewKnxBusWrapper(parameters);
                            await Bus.ConnectAsync(CancellationToken.None);
                            CheckBusConnection();           
                        }
                        catch (Exception e)
                        {
                            CheckError(e);
                        }          
                    }
                } 
            }

            else // Dans le cas d'une connexion USB
            {
                // Vérifie si la chaîne de connexion est fournie
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    //MessageBox.Show("Le type de connexion et la chaîne de connexion doivent être fournis.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Interrompt la méthode si les informations de connexion sont manquantes
                } 
                var parameters = ConnectorParameters.FromConnectionString(connectionString);
                Bus.NewKnxBusWrapper(parameters);
                // Établit la connexion au bus
                await Bus.ConnectAsync(CancellationTokenSource.Token);
                CheckBusConnection();
            }
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la fenêtre
            _logger.ConsoleAndLogWrite($"Erreur lors de la connexion au bus : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la connexion au bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false; // Réinitialise l'état d'occupation
            ResetCancellationTokenSource(); // Réinitialise le token d'annulation
        }
    }



    private void CheckError(Exception e)
    {
        string ErrorMessage = "";
        _logger.ConsoleAndLogWriteLine($"Erreur : {e.GetType().Name} - {e.Message}");
        if (SelectedConnectionType is "IP")
        {
            // Connexion Secure mais pas de knxkeys fourni
            if (e.Message.Contains("The value cannot be an empty string. (Parameter 'path')",
                    StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Cette connexion est sécurisée IP Secure. Veuillez fournir un fichier de clés .knxkeys";
            }
            // Connexion Secure avec knxkeys fourni mais pas de mdp fourni, ou mdp fourni ne correspond pas
            else if (e.Message.Contains("Not a valid keyring file (Invalid signature)",
                         StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Veuillez renseigner un mot de passe correspondant au fichier knxkeys fourni.";
            }
        }
        else if (SelectedConnectionType is "IP à distance (NAT)")
        {
            // Connexion NAT mais pas d'IP fourni
            if (NatAddress == "")
            {
                ErrorMessage =
                    "Pour établir une connexion NAT, veuillez renseigner une adresse IP au format IPv4. Exemple : 92.174.145.34";
            }
            // Connexion NAT mais IP fourni n'est pas une adresse IP
            else if (!ValidateIPv4(NatAddress))
            {
                ErrorMessage = "Veuillez renseigner une adresse IP au format IPv4. Exemple : 92.174.145.34";
            }
            // Connexion NAT avec IP fourni valide mais ne permettant pas une connexion KNX
            else if (e.Message.Contains("No reply from interface", StringComparison.OrdinalIgnoreCase) |
                     e.Message.Contains("Failed to read device description", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "L'adresse IP fournie n'a pas permis d'établir une connexion à un bus KNX.";
            }
            // Connexion Secure mais pas de knxkeys fourni
            else if (e.Message.Contains("The value cannot be an empty string. (Parameter 'path')",
                         StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Cette connexion est sécurisée IP Secure. Veuillez fournir un fichier de clés .knxkeys";
            }
            // Connexion Secure avec knxkeys fourni mais pas de mdp fourni, ou mdp fourni ne correspond pas
            else if (e.Message.Contains("Not a valid keyring file (Invalid signature)",
                         StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Veuillez renseigner un mot de passe correspondant au fichier knxkeys fourni.";
            }
            else
            {
                ErrorMessage = "Erreur non reconnue, contactez les développeurs.";
            }

            _logger.ConsoleAndLogWriteLine(ErrorMessage);
            ConnectionErrorMessage = ErrorMessage;
        }
    }

    /// <summary>
    /// Vérifie si la connexion a été faite, met à jour l'interface et l'état de connexion.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void CheckBusConnection()
    {
        try
        {
            // Vérifie si la connexion est établie avec succès
            if (Bus.IsNull || Bus.ConnectionState != BusConnectionState.Connected)
            {
                throw new InvalidOperationException(
                    "La connexion au bus a échoué."); // Lance une exception si la connexion échoue
            }
            else
            {
                Bus.ConnectionStateChanged += BusConnectionStateChanged!; 
                CurrentInterface = (SelectedConnectionType is "IP à distance (NAT)") ? "À distance via l'IP publique " + NatAddress : SelectedInterface?.DisplayName;
                IsConnected = true;
                UpdateConnectionState(); // Met à jour l'état de connexion
                OnBusConnectedReady(Bus); // Notifie les abonnés que la connexion est prête
                _logger.ConsoleAndLogWrite("Connexion réussie au bus.");
                //MessageBox.Show("Connexion réussie au bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la fenêtre
            _logger.ConsoleAndLogWrite($"Erreur lors de la connexion au bus : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la connexion au bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            NatAccess = false;
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
            //MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return; // Retourne immédiatement si le bus est déjà déconnecté ou si une opération est en cours
        }

        await CancellationTokenSource?.CancelAsync()!; // Annule les opérations en cours
        CancellationTokenSource = null; // Réinitialise le token d'annulation

        try
        {
            if (!Bus.IsNull)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged!;
                await Bus.DisposeAsync(); // Déconnecte et libère les ressources du bus
                Bus.SetNull = null;
                CurrentInterface = "Aucune interface connectée";
                IsConnected = false;
                UpdateConnectionState(); // Met à jour l'état de connexion
                //MessageBox.Show("Déconnexion réussie du bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la fenêtre
            _logger.ConsoleAndLogWrite($"Erreur lors de la déconnexion du bus : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la déconnexion du bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (SelectedConnectionType is "IP")
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
            else if (SelectedConnectionType is "USB")
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
            //Affiche un message d'erreur en cas d'exception
            _logger.ConsoleAndLogWrite($"Erreur lors de la découverte des interfaces : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la découverte des interfaces : {ex.Message}");
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
    public BusConnection(ILogger logger, IKnxBusWrapper  knxBusWrapper)
    {
        DiscoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();
        Bus = knxBusWrapper;
        _password = "";
        _logger = logger;
        _natAddress = "";
        _keysPath = "";
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
    
    private void WhenPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    // Used for error handling in the case of NAT
    // Check if given IP address is in fact a correctly written IPv4 address
    public bool ValidateIPv4(string ipString)
    {
        if (String.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        byte tempForParsing;

        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }
    
}