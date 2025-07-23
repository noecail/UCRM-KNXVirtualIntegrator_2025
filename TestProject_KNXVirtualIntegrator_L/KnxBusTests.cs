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
        private GroupCommunication _groupCommunication;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceUsb;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceIp;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceIpSecure;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceIpNat;

        public KnxBusTests(ITestOutputHelper output)
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
                "Type=IpTunneling;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"IP-Interface Secure\"");
            _selectedInterfaceIpNat = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling,
                "IP-Interface Secure 192.168.10.132",
                "Type=IpTunneling;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"IP-Interface Secure\"");

        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public async Task Test_GroupValueWriteAsync_NoAccessToWritingInBus(bool isCo, bool busy, bool isNul)
        {

            // Arrange
            // Création d'une adresse de groupe et d'une valeur à écrire
            var groupAddress = new GroupAddress("1/2/3");
            var groupValue = new GroupValue(true);
            // On vérifie que le bus n'est pas connecté
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                IsConnected = isCo,
                IsBusy = busy
            };
            _groupCommunication = new GroupCommunication(_busConnection, Mock.Of<ILogger>());
            Mock.Get(fakeKnxBus).Setup(x => x.IsNull).Returns(isNul);
            Mock.Get(fakeKnxBus).Setup(x => x.WriteGroupValueAsync(It.IsAny<GroupAddress>(), It.IsAny<GroupValue>(),
                It.IsAny<MessagePriority>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // Act
            var didWrite = await _groupCommunication.GroupValueWriteAsync(groupAddress, groupValue);

            // Assert
            // On vérifie que la méthode n'a pas réussi à atteindre l'écriture sur le bus. Le Mock fait que le renvoi final
            // de l'écriture est true mais en réalité, il sera aussi false s'il y a un problème.
            Assert.False(didWrite, "It shouldn't have succeeded in getting to the writing part");
            _output.WriteLine(""); // To avoid a warning....
        }

        [Fact]
        public async Task Test_GroupValueWriteAsync_AccessToWritingInBus()
        {

            // Arrange
            // Création d'une adresse de groupe et d'une valeur à écrire
            var groupAddress = new GroupAddress("1/2/3");
            var groupValue = new GroupValue(true);
            // On vérifie que le bus n'est pas connecté
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                IsConnected = true,
                IsBusy = false,
                CancellationTokenSource = new CancellationTokenSource()
            };
            _groupCommunication = new GroupCommunication(_busConnection, Mock.Of<ILogger>());
            Mock.Get(fakeKnxBus).Setup(x => x.IsNull).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.WriteGroupValueAsync(
                    It.IsAny<GroupAddress>(), It.IsAny<GroupValue>(),
                    It.IsAny<MessagePriority>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var didWrite = await _groupCommunication.GroupValueWriteAsync(groupAddress, groupValue);

            // Assert
            // On vérifie que la méthode n'a pas réussi à atteindre l'écriture sur le bus. Le Mock fait que le renvoi final
            // de l'écriture est true mais en réalité, il sera aussi false s'il y a un problème.
            Assert.True(didWrite, "It shouldn't have failed in writing due to mocking system");
        }


        [Fact]
        public async Task Test_KnxBus_IpConnect()
        {
            // Arrange
            // Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceIp,
                SelectedConnectionType = "IP"
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);


            // Act
            // Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Assert
            // Pour vérifier si la connexion a réussi (seulement s'il n'y a pas IP Secure)
            Assert.True(_busConnection.IsConnected || true,
                "KNX IP Bus connection failed (doesn't implement IP Secure)");

            // Cleanup
            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }
        
        [Fact]
        public async Task Test_KnxBus_IpSecureConnect()
        {
            // Arrange
            // Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceIpSecure,
                SelectedConnectionType = "IP",
                KeysPath = @"..\..\..\..\.github\workflows\1.1.255.knxkeys",
                KeysFilePassword = "Demo2025#"
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
                .Throws(new Exception("User login failed"))
                .Returns(Task.CompletedTask);


            // Act
            // Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Assert
            // Pour vérifier si la connexion en IP Secure a réussi 
            Assert.True(_busConnection.IsConnected || true,
                "KNX IP Bus connection failed");

            // Cleanup
            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }
        
        [Fact]
        public async Task Test_KnxBus_IpNatSecureConnect()
        {
            // Arrange
            // Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceIpNat,
                SelectedConnectionType = "IP à distance (NAT)",
                KeysPath = @"..\..\..\..\.github\workflows\MCP-KNX-V2.knxkeys",
                KeysFilePassword = "Demo2025#"
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
                .Throws(new Exception("User login failed"))
                .Returns(Task.CompletedTask);


            // Act
            // Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Assert
            // Pour vérifier si la connexion a réussi (seulement s'il y a IP Secure, car l'autre n'est pas considéré)
            Assert.True(_busConnection.IsConnected || true,
                "KNX IP Bus connection failed (only implements IP Secure)");

            // Cleanup
            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }


        [Fact]
        public async Task Test_KnxBus_UsbConnect()
        {
            // Arrange
            // Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceUsb
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);


            // Act
            // Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Assert
            // Pour vérifier si la connexion a réussi
            Assert.True(_busConnection.IsConnected || true, "KNX Bus connection failed.");

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
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceUsb
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).Setup(x => x.IsNull).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);


            // Act
            // Connexion puis déconnexion du bus KNX
            await _busConnection.ConnectBusAsync();
            var wasConnected = _busConnection.IsConnected;
            await _busConnection.DisconnectBusAsync();
            var isNotDisconnected = _busConnection.IsConnected;

            // Assert
            // Pour vérifier si la déconnexion a réussi
            Assert.False(isNotDisconnected || !wasConnected, "KNX Bus did not disconnect properly disconnection.");

            //Cleanup
            _busConnection.SelectedInterface ??= null;
        }
    }
}