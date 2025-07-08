using Knx.Falcon.Sdk;  // Par exemple pour BusConnection, GroupAddress
using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de l�.
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon;
using Knx.Falcon.Configuration;
using Xunit.Abstractions;

namespace TestProject_KNXVirtualIntegrator_L
{
    public class KnxBusTests
    {
        private readonly ITestOutputHelper _output;
        private readonly BusConnection _busConnection;
        private readonly GroupCommunication _groupCommunication;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceUsb;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceIp;

        public KnxBusTests(ITestOutputHelper output)
        {
            _output = output;
            // Initialisation de BusConnection et GroupCommunication
            _busConnection = new BusConnection();
            _groupCommunication = new GroupCommunication(_busConnection);
            // Initialisation des interfaces de la maquette 
            // Pour modifier les interfaces de test (changement de maquette, rafraichissement,...), rajouter des lignes
            // Console.Write au niveau de la fonction DiscoverInterfaceAsync dans les blocs if
            _selectedInterfaceUsb = new ConnectionInterfaceViewModel(ConnectorType.Usb, 
                "SpaceLogic KNX USB Interface DIN Rail",
                "Type=Usb;DevicePath=\\\\?\\hid#vid_16de&pid_008e#7&2d02dbc0&1&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\"SpaceLogic KNX USB Interface DIN Rail\"");
            _selectedInterfaceIp = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling, 
                "IP-Interface Secure 192.168.10.132",
                "Type=IpTunneling;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"IP-Interface Secure\"");
        }

   [Fact]
        public async Task Discover_KnxInterfaces()
    {
        // Étape 1 : Récupération de toutes les interfaces détectées par Falcon
        var interfaces = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);
        var numberInterfaces = 0;

        // Étape 2 : Affichage console pour visualiser les interfaces disponibles
        await foreach (var knxInterface in interfaces)
        {
            foreach (var tunnelInterfaces in knxInterface.GetTunnelingConnections())
            {
                numberInterfaces++;
                var displayName = tunnelInterfaces.IndividualAddress.HasValue
                    ? $"{tunnelInterfaces.Name} {tunnelInterfaces.HostAddress} ({tunnelInterfaces.IndividualAddress.Value})"
                    : $"{tunnelInterfaces.Name} {tunnelInterfaces.HostAddress}";
                _output.WriteLine($"[DETECTED] {displayName} → {knxInterface.ToString()}");
            }
        }
        // Étape 3 : Vérifications basiques sur la liste d'interfaces
        Assert.True(numberInterfaces != 0,"No IP Tunneling Interface found");
    }

        [Fact]
        public async Task Test_KnxBus_IPConnect_Auto()
        {
            // Étape 1 : Récupération des interfaces disponibles
            var interfaces = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);
            
            await foreach (var knxInterface in interfaces)
            {
                foreach (var tunnelInterfaces in knxInterface.GetTunnelingConnections())
                {
                    // Étape 4 : Connexion au bus via l'interface détectée
                    _busConnection.SelectedInterface = new ConnectionInterfaceViewModel(tunnelInterfaces.Type, tunnelInterfaces.Name, tunnelInterfaces.ToConnectionString());
                }
            }
            await _busConnection.ConnectBusAsync();

            // Étape 5 : Vérification de la connexion
            Assert.True(_busConnection.IsConnected, "Connexion IP échouée.");

            // Étape 6 : Déconnexion propre du bus
            await _busConnection.DisconnectBusAsync();
        }

       [Fact]
        public async Task Test_KnxBus_USBConnect_Auto()
        {
            // Étape 1 : Récupère les périphériques USB connectés
            var usbDevices = KnxBus.GetAttachedUsbDevices();

            // Étape 2 : Transforme chaque périphérique en une interface de connexion utilisable
            var selectedInterface = usbDevices.Select(device =>
                new ConnectionInterfaceViewModel(
                    ConnectorType.Usb,
                    device.DisplayName,
                    device.ToConnectionString()
                )
            ).FirstOrDefault();

            // Étape 3 : Si aucun périphérique trouvé, échoue le test
            if (selectedInterface == null)
            {
                Assert.Fail("Aucune interface USB détectée.");
            }

            // Étape 4 : Affiche l'interface sélectionnée
            _output.WriteLine("Interface USB détectée : " + selectedInterface.DisplayName + " " + selectedInterface.ConnectionString);

            // Étape 5 : Connexion au bus
            _busConnection.SelectedInterface = selectedInterface;
            await _busConnection.ConnectBusAsync();

            // Étape 6 : Vérifie que la connexion a réussi
            Assert.True(_busConnection.IsConnected, "Connexion USB échouée.");

            // Étape 7 : Déconnexion
            await _busConnection.DisconnectBusAsync();
        }

[Fact]
        public async Task Test_KnxBus_ConnectionFails_WithInvalidInterface()
        {
            // Crée une fausse interface IP qui ne pointe vers aucun vrai appareil
            var fakeInterface = new ConnectionInterfaceViewModel(
                ConnectorType.IpTunneling,
                "Interface IP Invalide",
                "Type=IpTunneling;HostAddress=192.0.2.123"
            );

            // On utilise cette fausse interface pour la connexion
            _busConnection.SelectedInterface = fakeInterface;

            // On tente de se connecter
            await _busConnection.ConnectBusAsync();

            // Vérifie que la connexion a échoué
            Assert.False(_busConnection.IsConnected, "La connexion aurait dû échouer avec une interface invalide.");

            // Nettoyage
            await _busConnection.DisconnectBusAsync();
        }
        
        [Fact]
        public async Task Test_WriteWithoutConnection()
        {
            var groupAddress = new GroupAddress("1/2/3");
            var groupValue = new GroupValue(true);

            // On vérifie que la méthode échoue quand on n'est pas connecté au bus
            var exception = await Assert.ThrowsAsync<NullReferenceException>(() =>
                _busConnection.Bus.WriteGroupValueAsync(
                    groupAddress, groupValue, MessagePriority.Low, CancellationToken.None)
            );

            _output.WriteLine("Erreur attendue : " + exception.Message);
        }
        
        [Fact]
        public async Task Test_KnxBus_IPConnect()
        {
            // Étape 1 : Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceIp;

            // Étape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            bool isConnected = _busConnection.IsConnected;

            // Étape 3 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();

            // Assertion pour vérifier si la connexion a réussi
            Assert.True(isConnected, "KNX IP Bus connection failed.");
        }
        
        
        
        [Fact]
        public async Task Test_KnxBus_USBConnect()
        {
            // Étape 1 : Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée � la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // Étape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            bool isConnected = _busConnection.IsConnected;
            
            // Étape 3 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            
            // Assertion pour vérifier si la connexion a réussi
            Assert.True(isConnected, "KNX Bus connection failed.");
        }
        
        [Fact]
        public async Task Test_KnxBus_USBConnect_Disconnect()
        {
            // Étape 1 : Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // Étape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            var wasConnected = _busConnection.IsConnected;
            
            // Étape 3 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            var isDisconnected = _busConnection.IsConnected;
            
            // Assertion pour vérifier si la connexion a réussi
            Assert.False((isDisconnected || !wasConnected), "KNX Bus disconnection failed.");
        }
        
      
        //Corriger l'envoi et réception : Pour le moment, on reçoit l'état passé et pas le nouvel état OK !
        [Fact]
        public async Task Test_KnxBus_USBConnectThenSendFrame_ReadValue()
        {
            // Étape 1 : Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // Étape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Étape 3 : Envoi d'une trame à une adresse de groupe d�finie
            var testGroupAddress = new GroupAddress("0/1/1");
            //var readGroupAddress = new GroupAddress("0/2/1");
            var testGroupValue = new GroupValue(true); // Valeur d'exemple (1 bit)

            // Envoi de la valeur sur le bus
            await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);

            // Petite attente pour laisser le temps au bus de traiter
            await Task.Delay(500);
            
            // Étape 4 : Lecture de la valeur de l'adresse de groupe
            var readGroupValue = await _groupCommunication.MaGroupValueReadAsync(testGroupAddress);

            //Assertion pour vérifier si la valeur envoyée est bien celle lue
            Assert.NotNull(readGroupValue);
            Assert.Equal(testGroupValue.Value, readGroupValue.Value);
            _output.WriteLine("Valeur lue depuis le bus : " + testGroupValue.ToString());
            
            // Étape 5 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
        }
    }
}
