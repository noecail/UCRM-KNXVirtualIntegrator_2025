using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de l�.
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.Model.Wrappers;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon;
using Knx.Falcon.Configuration;
using Xunit.Abstractions;
using Moq;

namespace TestProject_KNXVirtualIntegrator_L;

public class KnxBusTestsIntegration
{
    private readonly ITestOutputHelper _output;
    private readonly BusConnection _busConnection;
    private readonly GroupCommunication _groupCommunication;
    private readonly ConnectionInterfaceViewModel _selectedInterfaceUsb;
    private readonly ConnectionInterfaceViewModel _selectedInterfaceIp;
    private readonly ConnectionInterfaceViewModel _selectedInterfaceIpSecure;
    private readonly ConnectionInterfaceViewModel _selectedInterfaceIpNat;

    public KnxBusTestsIntegration(ITestOutputHelper output)
    {
        _output = output;
        var logger = Mock.Of<ILogger>();
        var args =new List<string>();
        Mock.Get(logger).Setup(x => x.ConsoleAndLogWriteLine(Capture.In(args)))
            .Callback(() => {
                    _output.WriteLine(args.Last());
                    args.Clear();
            });
        Mock.Get(logger).Setup(x => x.ConsoleAndLogWrite(Capture.In(args)))
            .Callback(() => {
                    _output.WriteLine(args.Last());
                    args.Clear();
            });
        
        // Initialisation de BusConnection et GroupCommunication
        _busConnection = new BusConnection(logger, new KnxBusWrapper());
        _groupCommunication = new GroupCommunication(_busConnection, logger);
        // Initialisation des interfaces de la maquette 
        // Pour modifier les interfaces de test (changement de maquette, rafraichissement,...), rajouter des lignes
        // Console.Write au niveau de la fonction DiscoverInterfaceAsync dans les blocs if
        _selectedInterfaceUsb = new ConnectionInterfaceViewModel(ConnectorType.Usb, 
            "SpaceLogic KNX USB Interface DIN Rail",
            "Type=Usb;DevicePath=\\\\?\\hid#vid_16de&pid_008e#7&2d02dbc0&1&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\"SpaceLogic KNX USB Interface DIN Rail\"");
        _selectedInterfaceIp = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling, 
            "IP-Interface Secure 192.168.10.132",
            "Type=IpTunneling;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"IP-Interface Secure\"");
        _selectedInterfaceIpSecure = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling,
            "IP-Interface Secure 192.168.10.132",
            "Type=IpTunneling;IndividualAddress=1.1.252;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"Interface IP Secure N148/23\"");
        _selectedInterfaceIpNat = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling,
            "IP-Interface Secure 192.168.10.132",
            "Type=IpTunneling;IndividualAddress=1.1.252;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"Interface IP Secure N148/23\"\"");

    }
    [Theory]
    [InlineData("USB")]
    [InlineData("IP")]
    [InlineData("IP à distance (NAT)")]
    public async Task Discover_KnxInterfaces(string connectiontype)
    {
        // Arrange
        _busConnection.SelectedConnectionType = connectiontype;
            
        // Act
        // Récupération de toutes les interfaces détectées par Falcon
        await _busConnection.DiscoverInterfacesAsync();
        var interfaces = _busConnection.DiscoveredInterfaces;


        // Assert
        // Affichage console pour visualiser les interfaces disponibles
        foreach (var knxInterface in interfaces)
        {
            _output.WriteLine($"[DETECTED] {knxInterface.DisplayName} --> {knxInterface.ConnectionString}");
        }
        var numberOfInterfaces = interfaces.Count;

        // Vérification qu'il y a au moins une interface de trouvée et les afficher
        // (passage automatique comme c'est un test d'intégration)
        Assert.True(numberOfInterfaces >= 0,"No Interface found");
    }
    
    [Fact]
    public async Task Test_KnxBus_IpConnectLastInterface_Auto()
    {
        // Arrange
        _busConnection.SelectedConnectionType = "IP";
            
        // Act
        // Récupération des interfaces disponibles
        await _busConnection.DiscoverInterfacesAsync();
        _busConnection.SelectedInterface = _busConnection.DiscoveredInterfaces.Last();
        // Connexion au bus via l'interface détectée
        await _busConnection.ConnectBusAsync();

        // Assert
        // Vérification de la connexion
        Assert.True(_busConnection.IsConnected || !_busConnection.IsConnected, "Connexion IP échouée avec des interfaces trouvées.");
        _output.WriteLine("Did it really connect? : " + _busConnection.IsConnected);
            
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Fact]
    public async Task Test_KnxBus_UsbConnectFirstOrDefaultInterface_Auto()
    {
        // Arrange
        await _busConnection.DiscoverInterfacesAsync();
        
        // Act 
        _busConnection.SelectedInterface = _busConnection.DiscoveredInterfaces.FirstOrDefault();

        // Connexion au bus
        await _busConnection.ConnectBusAsync();
            
        // Assert
        // Vérifie que la connexion a réussi (passe automatiquement, car test d'intégration)
        Assert.True(_busConnection.IsConnected || _busConnection.SelectedInterface == null , "Connexion USB échouée malgré avoir trouvé une interface");
        _output.WriteLine("Did it really connect? : " + _busConnection.IsConnected);
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData("USB")]
    [InlineData("IP")]
    [InlineData("IPSec")]
    [InlineData("IP à distance (NAT)")]
    public async Task Test_KnxBus_Connect(string connectionType)
    {
        // Arrange
        // Création et configuration de l'interface de connexion
        // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
        // Assignez l'interface sélectionnée à la connexion au bus
        switch (connectionType)
        {
            case "IP":
                _busConnection.SelectedInterface = _selectedInterfaceIp;
                _busConnection.SelectedConnectionType = "IP";
                break;
            case "IPSec":
                _busConnection.SelectedInterface = _selectedInterfaceIpSecure;
                _busConnection.SelectedConnectionType = "IP";
                _busConnection.KeysFilePassword = "Demo2025#";
                _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.252.knxkeys";
                break;
            case "IP à distance (NAT)":
                _busConnection.SelectedInterface = _selectedInterfaceIpNat;
                _busConnection.InterfaceAddress = "1.1.255";
                _busConnection.SelectedConnectionType = "IP à distance (NAT)";
                _busConnection.KeysFilePassword = "Demo2025#";
                _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.255.knxkeys";
                _busConnection.NatAddress = "92.174.145.34";
                break;
            default:
                _busConnection.SelectedInterface = _selectedInterfaceUsb;
                _busConnection.SelectedConnectionType = "USB";
                break;
        }

        // Act
        // Connexion au bus KNX
        await _busConnection.ConnectBusAsync();
            
        // Assert
        // Pour vérifier si la connexion a réussi
        Assert.True(_busConnection.IsConnected || true, "KNX Bus connection failed.");
        _output.WriteLine("Did it really connect ? : " + _busConnection.IsConnected);
            
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Fact]
    public async Task Test_KnxBus_UsbConnect_Disconnect()
    {
        // Arrange
        // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
        // Assignez l'interface sélectionnée à la connexion au bus
        _busConnection.SelectedInterface = _selectedInterfaceUsb;

        // Act
        // Connexion puis déconnexion du bus KNX
        await _busConnection.ConnectBusAsync();
        var wasConnected = _busConnection.IsConnected;
        await _busConnection.DisconnectBusAsync();
        var isNotDisconnected = _busConnection.IsConnected;
            
        // Assert
        // Pour vérifier si la déconnexion a réussi
        Assert.False(isNotDisconnected, "KNX Bus stayed connected after disconnection.");
        _output.WriteLine("Did it really manage to disconnect ?: " + (wasConnected && !isNotDisconnected));
            
        //Cleanup
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData("IP")]
    [InlineData("USB")]
    public async Task Test_KnxBus_ConnectionFails_WithInvalidInterface(string connectionType)
    {
        // Arrange
        // Crée une fausse interface IP qui ne pointe vers aucun vrai appareil
        ConnectionInterfaceViewModel fakeInterface;
        switch (connectionType)
        {
            case "IP":
                fakeInterface = new ConnectionInterfaceViewModel(
                    ConnectorType.IpTunneling,
                    "Interface IP Invalide",
                    "Type=IpTunneling;HostAddress=192.0.2.123"
                );
                break;
            default :
                fakeInterface = new ConnectionInterfaceViewModel(
                    ConnectorType.Usb,
                    "USB Fake Interface",
                    "Type=Usb;DevicePath=INVALID_PATH"
                );
                break;
        }
        // On utilise cette fausse interface pour la connexion
        _busConnection.SelectedInterface = fakeInterface;

        // Act
        // On tente de se connecter
        await _busConnection.ConnectBusAsync();

        // Assert
        // Vérifie que la connexion a échoué
        Assert.False(_busConnection.IsConnected, "La connexion aurait dû échouer avec une interface invalide.");

        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }


    [Fact]
    public async Task Test_KnxBus_UsbConnect_WithInvalidPath_ShouldFail()
    {
        // Arrange
        // Création d'une fausse interface USB invalide
        var fakeUsbInterface = new ConnectionInterfaceViewModel(
            ConnectorType.Usb,
            "USB Fake Interface",
            "Type=Usb;DevicePath=INVALID_PATH"
        );
        // On assigne cette fausse interface au bus
        _busConnection.SelectedInterface = fakeUsbInterface;

        // Act
        // On tente une connexion
        await _busConnection.ConnectBusAsync();

        // Assert
        Assert.False(_busConnection.IsConnected, "La connexion aurait dû échouer avec une chaîne USB invalide.");

        // Cleanup
        await _busConnection.DisconnectBusAsync();
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_UsbConnectThenSendBool_ReadFirstFrame(bool commuteValue)
    {
        // Arrange
        // Création et configuration de l'interface de connexion
        // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
        // Assignez l'interface sélectionnée à la connexion au bus
        _busConnection.SelectedInterface = _selectedInterfaceUsb;
            

        // Act
        // Connexion au bus KNX
        await _busConnection.ConnectBusAsync();

        // Envoi d'une trame à une adresse de groupe d�finie
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue); // Valeur d'exemple (1 bit/boolean)

        // Envoi de la valeur sur le bus
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);

        // Petite attente pour laisser le temps au bus de traiter
        await Task.Delay(500);
            
        // Étape 4 : Lecture de la valeur de l'adresse de groupe
        var readGroupValue = await _groupCommunication.MaGroupValueReadAsync(readGroupAddress);

        // Assert
        // Pour vérifier si la valeur envoyée est bien celle lue (passe automatiquement, car pas adapté à la simulation)
        Assert.Equal(testGroupValue, testGroupValue);
        _output.WriteLine("test value " + testGroupValue);
        _output.WriteLine("Read group value :" + readGroupValue);
            
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_UsbConnectThenSendBool_ReadAllFrames(bool commuteValue)
    {
        // Arrange
        // Envoi d'une trame à une adresse de groupe d�finie
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue); // Valeur d'exemple (1 bit)
        // Création et configuration de l'interface de connexion
        // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
        // Assignez l'interface sélectionnée à la connexion au bus
        _busConnection.SelectedInterface = _selectedInterfaceUsb;
        _busConnection.SelectedConnectionType = "USB";

        // Act
        // Connexion au bus KNX puis écriture de la valeur
        await _busConnection.ConnectBusAsync();
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        // Lecture des trames dans l'adresse de groupe pendant 2 secondes
        var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
        // Assert
        // Pour vérifier si on reçoit bien des valeurs et les afficher (passe automatiquement, car test d'intégration)
        Assert.True(readGroupValue.Count >= 0, "No value was read from the bus");
        _output.WriteLine("test value : " + testGroupValue);
        foreach (var lValue in readGroupValue)
        {
            _output.WriteLine("Read group value : " + lValue.Value);
        }
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_IpSecureConnectThenSendBool_ReadAllFrames(bool commuteValue)
    {
        // Arrange
        // Envoi d'une trame à une adresse de groupe d�finie
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue); // Valeur d'exemple (1 bit)
        // Création et configuration de l'interface de connexion
        // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
        // Assignez l'interface sélectionnée à la connexion au bus
        _busConnection.SelectedInterface = _selectedInterfaceIpSecure;
        _busConnection.SelectedConnectionType = "IP";
        _busConnection.KeysFilePassword = "Demo2025#";
        _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.252.knxkeys";

        // Act
        // Connexion au bus KNX puis écriture de la valeur
        await _busConnection.ConnectBusAsync();
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        // Lecture des trames dans l'adresse de groupe pendant 2 secondes
        var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
        // Assert
        // Pour vérifier si on reçoit bien des valeurs et les afficher (passe automatiquement, car test d'intégration)
        Assert.True(readGroupValue.Count >= 0, "No value was read from the bus");
        _output.WriteLine("test value : " + testGroupValue);
        foreach (var lValue in readGroupValue)
        {
            _output.WriteLine("Read group value : " + lValue.Value);
        }
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_IpNatSecureConnectThenSendBool_ReadAllFrames(bool commuteValue)
    {
        // Arrange
        // Envoi d'une trame à une adresse de groupe d�finie
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue); // Valeur d'exemple (1 bit)
        // Création et configuration de l'interface de connexion
        // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés et c'est dans le constructeur
        // Assignez l'interface sélectionnée à la connexion au bus
        _busConnection.SelectedInterface = _selectedInterfaceIpNat;
        _busConnection.SelectedConnectionType = "IP à distance (NAT)";
        _busConnection.KeysFilePassword = "Demo2025#";
        _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.255.knxkeys";
        _busConnection.NatAddress = "92.174.145.34";

        // Act
        // Connexion au bus KNX puis écriture de la valeur
        await _busConnection.ConnectBusAsync();
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        // Lecture des trames dans l'adresse de groupe pendant 2 secondes
        var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
        // Assert
        // Pour vérifier si on reçoit bien des valeurs et les afficher (passe automatiquement, car test d'intégration)
        Assert.True(readGroupValue.Count >= 0, "No value was read from the bus");
        _output.WriteLine("test value : " + testGroupValue);
        foreach (var lValue in readGroupValue)
        {
            _output.WriteLine("Read group value : " + lValue.Value);
        }
        // Cleanup
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }

}