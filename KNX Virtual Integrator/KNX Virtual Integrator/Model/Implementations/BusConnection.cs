using CommunityToolkit.Mvvm.ComponentModel;
using Knx.Falcon;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;
using System.Security;
using System.Windows.Threading;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon.Configuration;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon.Sdk;
using KNX_Virtual_Integrator.Model.Wrappers;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Class handling the connection to and disconnection from the KNX Bus
/// </summary>
public sealed class BusConnection : ObservableObject, IBusConnection
{
    /// <summary>
    /// Name space used for KNX operations
    /// </summary>
    public static XNamespace GlobalKnxNamespace = "http://knx.org/xml/ga-export/01"; 
    
/*--------------------------------------------------------------------------------------------------------------------*/
/************************************** Properties and Instances ******************************************************/
/*--------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///     The instance of the KNX bus.
    ///     It is wrapped to reduce the code dependency on the Falcon library and allow testing.
    ///     The actual KNX Bus can be null and is accessed through
    ///     <see cref="IKnxBusWrapper.IsNull"/> and <see cref="IKnxBusWrapper.KnxBusSetter"/>
    ///     The wrapper can possess more implementations in case more methods have to be used.
    /// </summary>
    public readonly IKnxBusWrapper Bus;

    /// <summary>
    ///     The instance of the Logger. It is used to log and print to the console the application activity,
    ///     while reducing code dependency on Console library.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    ///     Allows cancelling of ongoing asynchronous tasks. Can be null when there is no ongoing task.
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource;

    /// <summary>
    ///     Observable collection of the discovered bus interfaces for the current connection type.
    ///     It contains instances of <see cref="ConnectionInterfaceViewModel"/> of the discovered interfaces
    /// </summary>
    public ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; private set; }

    /// <summary>
    ///     Private property that is handled with <see cref="SelectedInterface"/>.
    /// </summary>
    private ConnectionInterfaceViewModel? _selectedInterface;

    /// <summary>
    ///     Currently selected bus interface. It is automatically updated and linked with the UI.
    /// </summary>
    public ConnectionInterfaceViewModel? SelectedInterface
    {
        get => _selectedInterface;
        set => SetProperty(ref _selectedInterface, value); // Notifie l'interface utilisateur des changements
    }

    /// <summary>
    ///     Private property that is handled with <see cref="IsBusy"/>
    /// </summary>
    private bool _isBusy;

    /// <summary>
    ///     Indicate whether there is an ongoing activity (like a connection).
    ///     Used to deactivate the UI during the activity.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value); // Notifie l'interface utilisateur des changements
    }

    /// <summary>
    ///     Private property that is handled with <see cref="IsConnected"/>
    /// </summary>
    private bool _isConnected;

    /// <summary>
    ///     Indicates whether the bus is connected or not. It is linked with the UI.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value) return; // Pas de changement si la valeur est la même
            _isConnected = value;
            WhenPropertyChanged(nameof(IsConnected)); // Notifie le view model du changement
        }
    }

    /// <summary>
    ///     Private property that is handled with <see cref="ConnectionState"/>
    /// </summary>
    private string? _connectionState;

    /// <summary>
    ///     Current connection state (i.e. : "Connected" ou "Disconnected"). Automatically updated
    /// </summary>
    public string? ConnectionState
    {
        get => _connectionState;
        private set => SetProperty(ref _connectionState, value); // Met à jour et notifie l'interface utilisateur
    }

    /// <summary>
    ///     Private property that is handled with <see cref="CurrentInterface"/>
    /// </summary>
    private string _currentInterface = "Aucune interface connectée";

    /// <summary>
    ///     Current connection interface
    /// </summary>
    public string CurrentInterface
    {
        get => _currentInterface;
        set
        {
            if (_currentInterface == value) return; // Pas de changement si la valeur est la même 
            _currentInterface = value;
            WhenPropertyChanged(nameof(CurrentInterface)); // Notifie l'interface utilisateur du changement
        }
    }

    /// <summary>
    ///     Private property that is handled with <see cref="NatAddress"/>
    /// </summary>
    private string _natAddress = "";

    /// <summary>
    ///     IP address of the distant router to allow NAT connection
    /// </summary>
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
    ///     Property that indicates whether we use NAT to access the interface.
    /// </summary>
    public bool NatAccess { get; set; }

    /// <summary>
    ///     Hardcoded default port for connection with NAT (3671 is the default for KNX)
    /// </summary>
    public int NatPort = 3671;

    /// <summary>
    ///     Individual Address for the given IP Secure interface
    /// </summary>
    // TODO : Do not hardcode it, use the connectionParameters to have it
    public string InterfaceAddress { get; set; }= "1.1.255";

    /// <summary>
    ///     Private property that is handled by <see cref="KeysFilePassword"/>
    /// </summary>
    private string _keysFilePassword = "";

    /// <summary>
    ///     Password that allows access to the file that holds the knxkeys. See <see cref="KeysPath"/>
    /// </summary>
    public string KeysFilePassword
    {
        get => _keysFilePassword;
        set
        {
            if (_keysFilePassword == value) return; // Pas de changement si la valeur est la même 
            _keysFilePassword = value;
            _secureKeysFilePassword.Clear();
            foreach (var c in _keysFilePassword)
                _secureKeysFilePassword.AppendChar(c);
            WhenPropertyChanged(nameof(KeysFilePassword)); // Notifie l'interface utilisateur du changement
        }
    }

    /// <summary>
    ///     Secure password that is currently selected
    /// </summary>
    private SecureString _secureKeysFilePassword = new();

    /// <summary>
    ///     Private property that is handled by <see cref="KeysPath"/>
    /// </summary>
    private string _keysPath = "";

    /// <summary>
    ///     The path to the file that holds the keys for the IP secure connection
    /// </summary>
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
    ///     Private property that is handled by <see cref="SelectedConnectionType"/>
    /// </summary>
    private string? _selectedConnectionType;

    /// <summary>
    ///     Connection Type chosen by the user (IP, IP NAT, USB). Its changes are shared with the user interface
    /// </summary>
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
    ///     Private property that is handled with <see cref="ConnectionErrorMessage"/>.
    /// </summary>
    private string _connectionErrorMessage = "";

    /// <summary>
    ///     Property that possesses the error message to be printed to the user interface.
    ///     It is collected through <see cref="CheckError"/>.
    ///     Different cases : 
    ///  </summary>
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


/*--------------------------------------------------------------------------------------------------------------------*/
/**************************************** Events and Update Methods ***************************************************/
/*--------------------------------------------------------------------------------------------------------------------*/
    // Gestion de la Communication avec ViewModel

    /// <summary>
    ///     The event that is fired when a property has changed.
    /// </summary>
    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Event raised when the bus is ready to be connected.
    /// </summary>
    public event EventHandler<IKnxBusWrapper>? BusConnectedReady; 

    /// <summary>
    ///     Handler that deals with changes in the bus connection state.
    ///     It is called when the <see cref="KnxBusWrapper.ConnectionState"/> changes and updates the interface
    ///     by calling <see cref="UpdateConnectionState"/>.
    /// </summary>
    /// <param name="sender">The object that fired the event</param>
    /// <param name="e">The argument of the event.</param>
    private void BusConnectionStateChanged(object sender, EventArgs e)
    {
        UpdateConnectionState();
    }

    /// <summary>
    ///     Private method which updates the bus readiness to be connected
    ///     through firing the event <see cref="BusConnectedReady"/>.
    /// </summary>
    /// <param name="bus">The object <see cref="IKnxBusWrapper"/> which represents the KNX Bus.</param>
    private void OnBusConnectedReady(IKnxBusWrapper bus)
    {
        BusConnectedReady?.Invoke(this, bus); // Déclenche l'événement si des abonnés existent
    }

    /// <summary>
    ///     Asynchronous method called when then <see cref="SelectedConnectionType"/> changes.
    ///     It tries to discover new interfaces with <see cref="DiscoverInterfacesAsync"/>.
    ///     If there is an error, it catches it and prints it.
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
            _logger.ConsoleAndLogWrite($"Error while discovering interfaces: {ex.Message}");
        }
    }

    /// <summary>
    ///     Updates the interface when a property has been modified
    /// </summary>
    /// <param name="propertyName">The property that was modified.</param>
    private void WhenPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


/*--------------------------------------------------------------------------------------------------------------------*/
/********************************************** Various Methods *******************************************************/
/*--------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///     Establishes a connection to the Knx Bus asynchronously
    ///     It first verifies whether there is an ongoing operation. If not, it proceeds with the connection.
    ///     When it succeeds, it updates the connection state and its subscribers.
    ///     When it fails, it prints an error message.
    /// </summary>
    /// <returns>A task representing the completion to the connection.</returns>
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

            ConnectorParameters parameters;
            //ATTENTION, fonctionnement de cette variable pas testée. Elle peut ne pas aimer être init avec IPTunneling
            
            // Déconnecte le bus existant si nécessaire
            if (!Bus.IsNull)
            {
                Bus.ConnectionStateChanged -= BusConnectionStateChanged!;
                CurrentInterface = "Aucune interface connectée";
                await Bus.DisposeAsync();
                Bus.KnxBusSetter = null;
                UpdateConnectionState(); // Met à jour l'état de connexion
            }
            
            switch (SelectedConnectionType) //Prépare la connexion en fonction du type de connexion voulue
            {
                case "Remote IP (NAT)": 
                    //Crée les paramètres de connexion
                    parameters = new IpTunnelingConnectorParameters(NatAddress, NatPort, useNat: true) 
                            {
                                IndividualAddress = IndividualAddress.Parse(InterfaceAddress)
                            };
                    break;
                
                default: // Dans le cas d'une connexion USB ou IP Locale
                    // Vérifie si la chaîne de connexion est fournie
                    if (string.IsNullOrWhiteSpace(connectionString))
                        throw new Exception("Connection type and string have to be set before connecting.");
                    
                    parameters = ConnectorParameters.FromConnectionString(connectionString);
                    break;
            }
            
            try
            {
                // Crée un nouvel objet de bus avec les paramètres extraits de la chaîne de connexion
                Bus.NewKnxBusWrapper(parameters);
                // Tentative de connexion au bus
                await Bus.ConnectAsync(CancellationToken.None);
                CheckBusConnection();
            }
            catch (Exception ex)
            {
                //Si l'interface est sécurisée, l'erreur contiendra "User login failed'
                if (!ex.Message.Contains("User login failed", StringComparison.OrdinalIgnoreCase)) 
                    throw new Exception(ex.Message);
                
                //Tentative de connexion en sécurisé
                _logger.ConsoleAndLogWriteLine($"{ex.Message} : Tentative de connexion en sécurisé");
                //Efface le message d'erreur dû à la tentative de connexion non sécurisée
                ConnectionErrorMessage = "";
                // Ajouter le mot de passe
                await parameters.LoadSecurityDataAsync(KeysPath, _secureKeysFilePassword);
                Bus.NewKnxBusWrapper(parameters);
                await Bus.ConnectAsync(CancellationToken.None);
                CheckBusConnection();
            }
        }
        catch (Exception e)
        {
            CheckError(e);
        }
        finally
        {
            IsBusy = false; // Réinitialise l'état d'occupation
            ResetCancellationTokenSource(); // Réinitialise le token d'annulation
        }
    }


    /// <summary>
    ///     Handles every exception raised in the bus context that needs to be shown to the user
    /// </summary>
    /// <param name="e">The raised exception.</param>
    private void CheckError(Exception e)
    {
        string errorMessage;
        _logger.ConsoleAndLogWriteLine($"Erreur : {e.GetType().Name} - {e.Message}");
        switch (SelectedConnectionType)
        {
            case "IP" when e.Message.Contains("The value cannot be an empty string. (Parameter 'path')", StringComparison.OrdinalIgnoreCase):
            case "Remote IP (NAT)" when e.Message.Contains("The value cannot be an empty string. (Parameter 'path')", StringComparison.OrdinalIgnoreCase):
                // Connexion Secure mais pas de knxkeys fourni
                errorMessage = "Cette connexion est sécurisée IP Secure. Veuillez fournir un fichier de clés .knxkeys";
                break;

            case "IP" when e.Message.Contains("Not a valid keyring file (Invalid signature)", StringComparison.OrdinalIgnoreCase):
            case "Remote IP (NAT)" when e.Message.Contains("Not a valid keyring file (Invalid signature)", StringComparison.OrdinalIgnoreCase):
                // Connexion Secure avec knxkeys fourni mais pas de mdp fourni, ou mdp fourni ne correspond pas
                errorMessage = "Veuillez renseigner un mot de passe correspondant au fichier knxkeys fourni.";
                break;

            case "IP" when e.Message.Contains("Could not connect to IP interface (The requested address is not available)", StringComparison.OrdinalIgnoreCase):
            case "Remote IP (NAT)" when e.Message.Contains("Could not connect to IP interface (The requested address is not available)", StringComparison.OrdinalIgnoreCase):
                // Connexion réussie, mais le bus est déjà utilisé
                errorMessage = "Un autre appareil est déjà connecté au bus KNX.";
                break;

            case "Remote IP (NAT)" when NatAddress == "":
                // Connexion NAT mais pas d'IP fourni
                errorMessage = "Pour établir une connexion NAT, veuillez renseigner une adresse IP au format IPv4. Exemple : 203.0.113.2";
                break;

            case "Remote IP (NAT)" when !ValidateIPv4(NatAddress):
                // Connexion NAT mais IP fourni n'est pas une adresse IP
                errorMessage = "Veuillez renseigner une adresse IP au format IPv4. Exemple : 203.0.113.2";
                break;

            case "Remote IP (NAT)" when e.Message.Contains("No reply from interface", StringComparison.OrdinalIgnoreCase):
            case "Remote IP (NAT)" when e.Message.Contains("Failed to read device description", StringComparison.OrdinalIgnoreCase):
                // Connexion NAT avec IP fourni valide mais ne permettant pas une connexion KNX
                errorMessage = "L'adresse IP fournie n'a pas permis d'établir une connexion à un bus KNX.";
                break;
            
            case "IP" when e.Message.Contains("User login failed", StringComparison.OrdinalIgnoreCase):
            case "Remote IP (NAT)" when e.Message.Contains("User login failed", StringComparison.OrdinalIgnoreCase):
                // User Login Failed 2 fois de suite
                errorMessage = "Il y a une erreur dans le fichier ou le mot de passe";
                break;
            
            case "IP" when e.Message.Contains("Connection type and string have to be set before connecting.", StringComparison.OrdinalIgnoreCase):
            case "USB" when e.Message.Contains("Connection type and string have to be set before connecting.", StringComparison.OrdinalIgnoreCase):
                errorMessage = "Veuillez choisir une interface pour vous connecter.";
                break;
            
            case "USB" when e.Message.Contains("USB device", StringComparison.OrdinalIgnoreCase) ://&& e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase):
                errorMessage = "Interface non trouvée, veuillez vérifier la connexion";
                break;
            
            default:
                errorMessage = "Erreur non reconnue.";
                break;
        }

        _logger.ConsoleAndLogWriteLine(errorMessage);
        ConnectionErrorMessage = errorMessage;
    }

    /// <summary>
    ///     Verifies if the connection was successful and updates the user interface.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void CheckBusConnection()
    {
        try
        {
            // Vérifie si la connexion est établie avec succès et lance une exception si elle a échoué
            if (Bus.IsNull || Bus.ConnectionState != BusConnectionState.Connected)
                throw new InvalidOperationException("La connexion au bus a échoué.");

            Bus.ConnectionStateChanged += BusConnectionStateChanged!;
            CurrentInterface = SelectedConnectionType is "Remote IP (NAT)" ?
                "À distance via l'IP publique " + NatAddress : SelectedInterface?.DisplayName is not null ? 
                    SelectedInterface.DisplayName : "";
            
            IsConnected = true;
            UpdateConnectionState(); // Met à jour l'état de connexion
            OnBusConnectedReady(Bus); // Notifie les abonnés que la connexion est prête
            _logger.ConsoleAndLogWriteLine("Connexion réussie au bus en " + SelectedConnectionType switch
                {
                    "IP" => "IP",
                    "Remote IP (NAT)" => "IP(NAT)",
                    "USB" => "USB",
                    _ => "???"
                }
            
            );
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur dans la fenêtre
            _logger.ConsoleAndLogWriteLine($"Erreur lors de la connexion au bus : {ex.Message}");
        }
        finally
        {
            NatAccess = false;
            ResetCancellationTokenSource(); // Réinitialise le token d'annulation
            IsBusy = false; // Réinitialise l'état d'occupation
        }
    }

    /// <summary>
    ///     Asynchronously disconnects from the KNX Bus.
    ///     It first verifies whether the bus is connected or if there is an ongoing operation.
    ///     If not, it starts the disconnection. When it succeeds, it updates the interface and prints a log.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de déconnexion asynchrone.</returns>
    public async Task DisconnectBusAsync()
    {
        try
        {
            if (!IsConnected)
                throw new Exception("The bus is already disconnected.");

            if (Bus.IsNull)
                throw new Exception("There is no bus to disconnect.");

            await CancellationTokenSource?.CancelAsync()!; // Annule les opérations en cours
            CancellationTokenSource = null; // Réinitialise le token d'annulation

            Bus.ConnectionStateChanged -= BusConnectionStateChanged!;
            await Bus.DisposeAsync(); // Déconnecte et libère les ressources du bus
            _logger.ConsoleAndLogWriteLine("Bus disconnection succeeded");
        }
        catch (Exception ex)
        {
            // Gestion des exceptions : affiche le message d'erreur
            _logger.ConsoleAndLogWrite($"Bus disconnection error : {ex.Message}");
        }
        finally
        {
            Bus.KnxBusSetter = null;
            CurrentInterface = "Aucune interface connectée";
            IsConnected = false;
            UpdateConnectionState(); // Met à jour l'état de connexion
        }
    }

    /// <summary>
    ///     Discover asynchronously the available interfaces according to the <see cref="SelectedConnectionType"/>.
    ///     This method discovers USB and IP interfaces and adds them to <see cref="DiscoveredInterfaces"/>.
    ///     The results are updated to the user interface.
    /// </summary>
    /// <returns>A task representing the completion of the method</returns>
    public async Task DiscoverInterfacesAsync()
    {
        try
        {
            // Crée une collection observable pour stocker les interfaces découvertes
            var discoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();
            // Vérifie si le type de connexion sélectionné est "IP" ou "USB"
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
                                var existing = discoveredInterfaces.FirstOrDefault(i =>
                                    i.ConnectionString == result.IsExtensionOf.ToConnectionString());
                                if (existing != null)
                                {
                                    existing.ConnectionString = tunnelingServer.ToConnectionString(); //Pas utile??
                                    continue;
                                }
                            }

                            // Crée une vue pour chaque serveur tunneling découvert
                            var displayName = tunnelingServer.IndividualAddress.HasValue
                                ? $"{tunnelingServer.Name} {tunnelingServer.HostAddress} ({tunnelingServer.IndividualAddress.Value})"
                                : $"{tunnelingServer.Name} {tunnelingServer.HostAddress}";
                            var tunneling = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling, displayName,
                                tunnelingServer.ToConnectionString());
                            discoveredInterfaces.Add(tunneling);
                        }

                        // Ne traite pas les connexions IP routing si non supportées
                        if (!result.Supports(ServiceFamily.Routing, 1))
                            continue;
                        
                        var routingParameters = IpRoutingConnectorParameters.FromDiscovery(result);
                        var routing = new ConnectionInterfaceViewModel(ConnectorType.IpRouting,
                            $"{result.MulticastAddress} on {result.LocalIPAddress}",
                            routingParameters.ToConnectionString());
                        discoveredInterfaces.Add(routing);
                    }
                });

                // Attends que la tâche de découverte des interfaces IP soit terminée
                await Task.WhenAll(ipDiscoveryTask);
            }
            // Vérifie si le type de connexion sélectionné est "USB"
            else if (SelectedConnectionType is "USB")
            {
                // Démarre une tâche pour découvrir les périphériques USB
                var usbDiscoveryTask = Task.Run(() =>
                {
                    foreach (var usbDevice in KnxBus.GetAttachedUsbDevices())
                    {
                        var interfaceViewModel = new ConnectionInterfaceViewModel(ConnectorType.Usb,
                            usbDevice.DisplayName, usbDevice.ToConnectionString());
                        discoveredInterfaces.Add(interfaceViewModel);
                    }
                });
                
                // Attends que la tâche de découverte des périphériques USB soit terminée
                await Task.WhenAll(usbDiscoveryTask);
            }
            
            // Met à jour la collection des interfaces découvertes sur l'interface utilisateur
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                DiscoveredInterfaces.Clear();
                foreach (var discoveredInterface in discoveredInterfaces)
                {
                    DiscoveredInterfaces.Add(discoveredInterface);
                }
            });
        }
        catch (Exception ex)
        {
            //Affiche un message d'erreur en cas d'exception
            _logger.ConsoleAndLogWrite($"Erreur lors de la découverte des interfaces : {ex.Message}");
        }
    }

    /// <summary>
    ///     Updates the <see cref="ConnectionState"/> according to the value of <see cref="IsConnected"/>
    ///     It is set to "Connected" if true and "Disconnected" if not
    /// </summary>
    private void UpdateConnectionState()
    {
        ConnectionState = IsConnected ? "Connected" : "Disconnected";
    }

    /// <summary>
    ///     Initialises a new instance of <see cref="BusConnection"/>.
    ///     It also creates a new instance of <see cref="ObservableCollection{T}"/> to store the discovered interfaces.
    /// </summary>
    /// <param name="logger">The logger instance to log and print out messages</param>
    /// <param name="knxBusWrapper">The KnxBus instance with which the class will communicate</param>
    public BusConnection(ILogger logger, IKnxBusWrapper knxBusWrapper)
    {
        DiscoveredInterfaces = new ObservableCollection<ConnectionInterfaceViewModel>();
        Bus = knxBusWrapper;
        _logger = logger;
    }

    /// <summary>
    ///     Resets the CancellationTokenSource if it exists with <see cref="CancellationTokenSource.Dispose()"/>.
    ///     It then creates a new CancellationTokenSource. It is used to create a new one for ongoing operations.
    /// </summary>
    private void ResetCancellationTokenSource()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    ///     Used for error handling in the case of NAT.
    ///     Checks if given IP address is in fact a correctly written IPv4 address
    /// </summary>
    /// <param name="ipString"> The IP address to check.</param>
    /// <returns> Returns true if the address has the form of an IPv4 address</returns>
    public bool ValidateIPv4(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
            return false;
        
        string[] splitValues = ipString.Split('.');
        
        if (splitValues.Length != 4)
            return false;

        return splitValues.All(r => byte.TryParse(r, out _));
    }
}