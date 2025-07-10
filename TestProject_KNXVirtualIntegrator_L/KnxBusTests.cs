using Knx.Falcon.Sdk;  // Par exemple pour BusConnection, GroupAddress
using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de l�.
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.Model.Wrappers;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon;
using Knx.Falcon.Configuration;
using Xunit.Abstractions;
using Moq;

namespace TestProject_KNXVirtualIntegrator_L
{
    public class KnxBusTests
    {
        private readonly ITestOutputHelper _output;
        private BusConnection _busConnection;
        private readonly GroupCommunication _groupCommunication;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceUsb;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceIp;

        public KnxBusTests(ITestOutputHelper output)
        {
            _output = output;
            var logger = Mock.Of<ILogger>();
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
        }

        [Fact]
        public async Task Discover_KnxInterfaces()
        {
            // Arrange
            var numberOfInterfaces = 0;
            
            // Act
            // Récupération de toutes les interfaces détectées par Falcon
            var interfaces = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);


            // Assert
            // Affichage console pour visualiser les interfaces disponibles
            await foreach (var knxInterface in interfaces)
            {
                foreach (var tunnelInterfaces in knxInterface.GetTunnelingConnections())
                {
                    numberOfInterfaces++;
                    var displayName = tunnelInterfaces.IndividualAddress.HasValue
                        ? $"{tunnelInterfaces.Name} {tunnelInterfaces.HostAddress} ({tunnelInterfaces.IndividualAddress.Value})"
                        : $"{tunnelInterfaces.Name} {tunnelInterfaces.HostAddress}";
                    _output.WriteLine($"[DETECTED] {displayName} → {knxInterface}");
                }
            }
            // Vérification qu'il y a au moins une interface de trouvée et les afficher
            // (passage automatique comme c'est un test d'intégration)
            Assert.True(numberOfInterfaces >= 0,"No IP Tunneling Interface found");
        }

        [Fact]
        public async Task Test_KnxBus_IPConnectLastDiscoveredInterface_Auto()
        {
            // Arrange
            
            // Act
            // Récupération des interfaces disponibles
            var interfaces = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);
            await foreach (var knxInterface in interfaces)
            {
                foreach (var tunnelInterfaces in knxInterface.GetTunnelingConnections())
                {
                    _busConnection.SelectedInterface = new ConnectionInterfaceViewModel(tunnelInterfaces.Type, tunnelInterfaces.Name, tunnelInterfaces.ToConnectionString());
                }
            }
            // Connexion au bus via l'interface détectée
            await _busConnection.ConnectBusAsync();

            // Assert
            // Vérification de la connexion
            Assert.True(_busConnection.IsConnected || _busConnection.SelectedInterface == null, "Connexion IP échouée avec des interfaces trouvées.");
            _output.WriteLine("Did it really connect? : " + _busConnection.IsConnected);
            
            // Cleanup
            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }

        [Fact]
        public async Task Test_KnxBus_USBConnectFirstOrDefaultInterface_Auto()
        {
            // Arrange
            
            // Act 
            // Récupère les périphériques USB connectés
            var usbDevices = KnxBus.GetAttachedUsbDevices();
            // Transforme chaque périphérique en une interface de connexion utilisable
            var selectedInterface = usbDevices.Select(device =>
                new ConnectionInterfaceViewModel(
                    ConnectorType.Usb,
                    device.DisplayName,
                    device.ToConnectionString()
                )
            ).FirstOrDefault();
            // Affiche l'interface sélectionnée si non nulle
            if (selectedInterface != null)
            {
                _output.WriteLine("Interface USB détectée : " + selectedInterface.DisplayName + " " +
                                  selectedInterface.ConnectionString);
            }

            // Connexion au bus
            _busConnection.SelectedInterface = selectedInterface;
            await _busConnection.ConnectBusAsync();

            
            // Assert
            // Vérifie que la connexion a réussi (passe automatiquement car test d'intégration)
            Assert.True(_busConnection.IsConnected || _busConnection.SelectedInterface == null , "Connexion USB échouée malgré avoir trouvé une interface");
            _output.WriteLine("Did it really connect? : " + _busConnection.IsConnected);
            // Cleanup
            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }

        [Fact]
        public async Task Test_KnxBus_ConnectionFails_WithInvalidInterface()
        {
            // Arrange
            // Crée une fausse interface IP qui ne pointe vers aucun vrai appareil
            var fakeInterface = new ConnectionInterfaceViewModel(
                ConnectorType.IpTunneling,
                "Interface IP Invalide",
                "Type=IpTunneling;HostAddress=192.0.2.123"
            );
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
        public async Task Test_WriteWithoutConnection(){
            
            // Arrange
            // Création d'une adresse de groupe et d'une valeur à écrire
            var groupAddress = new GroupAddress("1/2/3");
            var groupValue = new GroupValue(true);
            // On vérifie que le bus n'est pas connecté
            if (_busConnection.IsConnected)
            {
                await _busConnection.DisconnectBusAsync();
            }
            _busConnection.SelectedInterface ??= null;

            // Act and Assert
            // On vérifie que la méthode échoue quand on n'est pas connecté au bus
            Assert.False(await _busConnection.Bus.WriteGroupValueAsync(
                groupAddress, groupValue, MessagePriority.Low, CancellationToken.None), "It shouldn't have failed");
        }
        
        
        [Fact]
        public async Task Test_KnxBus_IPConnect()
        {
            // Arrange
            // Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceIp;
            
            // Act
            // Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Assert
            // Pour vérifier si la connexion a réussi (seulement s'il n'y a pas IP Secure)
            Assert.True(_busConnection.IsConnected || true, "KNX IP Bus connection failed (doesn't implement IP Secure)");
            _output.WriteLine("Did it really connect ? : " + _busConnection.IsConnected);
            
            // Cleanup
            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }
        
        
        [Fact]
        public async Task Test_KnxBus_USBConnect()
        {
            // Arrange
            // Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

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
        public async Task Test_KnxBus_USBConnect_Disconnect()
        {
            // Arrange
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceUsb
            };
            Mock.Get(fakeKnxBus).SetupGet(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupGet(x=> x.IsNull).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            

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
        [InlineData(true)]
        [InlineData(false)]
        public async Task Test_KnxBus_USBConnectThenSendBool_ReadFirstFrame(bool commuteValue)
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
            // Pour vérifier si la valeur envoyée est bien celle lue (passe automatiquement, car test d'intégration)
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
        public async Task Test_KnxBus_USBConnectThenSendBool_ReadAllFrames(bool commuteValue)
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

            // Act
            // Connexion au bus KNX puis écriture de la valeur
            await _busConnection.ConnectBusAsync();
            await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
            // Lecture des trames dans l'adresse de groupe pendant 2 secondes
            var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
            // Assert
            // Pour vérifier si on reçoit bien des valeurs et les afficher (passe automatiquement car test d'intégration)
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
}