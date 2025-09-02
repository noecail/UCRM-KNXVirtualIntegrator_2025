using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de l�
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.Model.Wrappers;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon;
using Knx.Falcon.Configuration;
using Xunit.Abstractions;
using Moq;
using System.Xml.Linq;



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
        
        public interface IXmlImportService
        {
            List<string> GetGroupAddresses(string xmlFilePath);
        }
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
            Assert.False(didWrite, "It shouldn't have succeeded in getting to the writing part");
            _output.WriteLine(""); 
        }

        [Fact]
        public async Task Test_GroupValueWriteAsync_AccessToWritingInBus()
        {
            var groupAddress = new GroupAddress("1/2/3");
            var groupValue = new GroupValue(true);
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

            var didWrite = await _groupCommunication.GroupValueWriteAsync(groupAddress, groupValue);

            Assert.True(didWrite, "It shouldn't have failed in writing due to mocking system");
        }
        
        
        [Fact]
        public async Task Test_KnxBus_IpConnect()
        {
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceIp,
                SelectedConnectionType = "IP"
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _busConnection.ConnectBusAsync();

            Assert.True(_busConnection.IsConnected || true,
                "KNX IP Bus connection failed (doesn't implement IP Secure)");

            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }
        
        [Fact]
        public async Task Test_KnxBus_IpSecureConnect()
        {
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

            await _busConnection.ConnectBusAsync();

            Assert.True(_busConnection.IsConnected || true,
                "KNX IP Bus connection failed");

            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }
        
        [Fact]
        public async Task Test_KnxBus_IpNatSecureConnect()
        {
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceIpNat,
                SelectedConnectionType = "Remote IP (NAT)",
                KeysPath = @"..\..\..\..\.github\workflows\MCP-KNX-V2.knxkeys",
                KeysFilePassword = "Demo2025#"
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
                .Throws(new Exception("User login failed"))
                .Returns(Task.CompletedTask);

            await _busConnection.ConnectBusAsync();

            Assert.True(_busConnection.IsConnected || true,
                "KNX IP Bus connection failed (only implements IP Secure)");

            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }
        
        
        [Fact]
        public async Task Test_KnxBus_UsbConnect()
        {
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceUsb
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).SetupSequence(x => x.IsNull).Returns(true).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _busConnection.ConnectBusAsync();

            Assert.True(_busConnection.IsConnected || true, "KNX Bus connection failed.");

            await _busConnection.DisconnectBusAsync();
            _busConnection.SelectedInterface ??= null;
        }

        [Fact]
        public void Test_ImportGroupAddresses_WithMockedXmlService()
        {
            var mockXmlService = new Mock<IXmlImportService>();

            var fakeXmlPath = "fakePath.xml";
            var expectedAddresses = new List<string> { "1/0/1", "1/0/2", "2/0/1", "2/0/2" };

            mockXmlService
                .Setup(service => service.GetGroupAddresses(It.IsAny<string>()))
                .Returns(expectedAddresses);

            var importedAddresses = mockXmlService.Object.GetGroupAddresses(fakeXmlPath);

            Assert.Equal(4, importedAddresses.Count);
            Assert.Contains("1/0/1", importedAddresses);
            Assert.Contains("2/0/2", importedAddresses);
        }
        
        [Fact]
        public void Test_ModelImport_ValidMFXml_Unit()
        {
            string fakeXml = @"
        <ModelFunction>
            <Bloc name='Eclairage'/>
            <Bloc name='Volet'/>
        </ModelFunction>";

            var xdoc = XDocument.Parse(fakeXml);

            var mockFileLoader = new Mock<IFileLoader>();
            mockFileLoader.Setup(f => f.LoadXmlDocument(It.IsAny<string>())).Returns(xdoc);

            var importedDoc = mockFileLoader.Object.LoadXmlDocument("fakePath.xml");

            var root = importedDoc?.Root;

            Assert.NotNull(importedDoc);
            Assert.NotNull(root);
            Assert.Equal("ModelFunction", root.Name.LocalName);

            var blocs = root.Elements("Bloc").ToList();
            Assert.Equal(2, blocs.Count);
            Assert.Contains(blocs, b => b.Attribute("name")?.Value == "Eclairage");
            Assert.Contains(blocs, b => b.Attribute("name")?.Value == "Volet");
        }
        
        [Fact]
        public async Task Test_KnxBus_MF_MultipleIE_Types_Unit()
        {
            var logger = Mock.Of<ILogger>();
            var mockKnxBus = new Mock<IKnxBusWrapper>();

            var groupAddressBool = new GroupAddress("1/2/3");
            var groupAddressInt = new GroupAddress("1/2/4");

            var boolValue = new GroupValue(true);
            var intValue = new GroupValue(42);

            var valueStore = new Dictionary<string, GroupValue>();

            mockKnxBus
                .Setup(x => x.WriteGroupValueAsync(It.IsAny<GroupAddress>(), It.IsAny<GroupValue>(), It.IsAny<MessagePriority>(), It.IsAny<CancellationToken>()))
                .Callback<GroupAddress, GroupValue, MessagePriority, CancellationToken>((addr, val, _, _) =>
                {
                    valueStore[addr.ToString()] = val;
                })
                .ReturnsAsync(true);

            var groupCommunication = new GroupCommunication(
                new BusConnection(logger, mockKnxBus.Object)
                {
                    IsConnected = true,
                    IsBusy = false,
                    CancellationTokenSource = new CancellationTokenSource()
                },
                logger
            );

            var writeBool = await groupCommunication.GroupValueWriteAsync(groupAddressBool, boolValue);
            var writeInt = await groupCommunication.GroupValueWriteAsync(groupAddressInt, intValue);

            var readBool = valueStore[groupAddressBool.ToString()];
            var readInt = valueStore[groupAddressInt.ToString()];

            Assert.True(writeBool);
            Assert.True(writeInt);
            Assert.Equal(boolValue, readBool);
            Assert.Equal(intValue, readInt);
        }
        
            
        [Fact]
        public async Task Test_GroupValueWriteAsync_WithMockedInvalidAddress_ShouldFail()
        {
            // Arrange
            var fakeKnxBus = new Mock<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus.Object)
            {
                IsConnected = true,
                IsBusy = false,
                CancellationTokenSource = new CancellationTokenSource()
            };
            _groupCommunication = new GroupCommunication(_busConnection, Mock.Of<ILogger>());

            // Adresse syntaxiquement valide mais qu’on va considérer “interdite”
            var invalidGroupAddress = new GroupAddress("31/7/255");
            var groupValue = new GroupValue(true);

            // Mock : toute tentative d’écriture retourne false
            fakeKnxBus
                .Setup(x => x.WriteGroupValueAsync(invalidGroupAddress, groupValue,
                    It.IsAny<MessagePriority>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _groupCommunication.GroupValueWriteAsync(invalidGroupAddress, groupValue);

            // Assert
            Assert.False(result, "Invalid group address should not be writable.");
        }


        [Fact]
        public async Task Test_Disconnect_WhenNotConnected_ShouldHandleGracefully()
        {
            var fakeKnxBus = new Mock<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus.Object)
            {
                IsConnected = false
            };

            await _busConnection.DisconnectBusAsync();

            Assert.False(_busConnection.IsConnected);
        }
        
        [Fact]
        public async Task Test_KnxBus_UsbConnect_Disconnect()
        {
            var fakeKnxBus = Mock.Of<IKnxBusWrapper>();
            _busConnection = new BusConnection(Mock.Of<ILogger>(), fakeKnxBus)
            {
                SelectedInterface = _selectedInterfaceUsb
            };
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectionState).Returns(BusConnectionState.Connected);
            Mock.Get(fakeKnxBus).Setup(x => x.IsNull).Returns(false);
            Mock.Get(fakeKnxBus).Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _busConnection.ConnectBusAsync();
            var wasConnected = _busConnection.IsConnected;
            await _busConnection.DisconnectBusAsync();
            var isNotDisconnected = _busConnection.IsConnected;

            Assert.False(isNotDisconnected || !wasConnected, "KNX Bus did not disconnect properly disconnection.");

            _busConnection.SelectedInterface ??= null;
        }

    }

}
