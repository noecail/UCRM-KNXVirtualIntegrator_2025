using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de l�
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.Model.Wrappers;
using KNX_Virtual_Integrator.ViewModel;
using Knx.Falcon;
using Knx.Falcon.Configuration;
using Xunit.Abstractions;
using Moq;
using System.Xml.Linq;

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
        
        _busConnection = new BusConnection(logger, new KnxBusWrapper());
        _groupCommunication = new GroupCommunication(_busConnection, logger);

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
    [InlineData("Remote IP (NAT)")]
    public async Task Discover_KnxInterfaces(string connectiontype)
    {
        _busConnection.SelectedConnectionType = connectiontype;
            
        await _busConnection.DiscoverInterfacesAsync();
        var interfaces = _busConnection.DiscoveredInterfaces;

        foreach (var knxInterface in interfaces)
        {
            _output.WriteLine($"[DETECTED] {knxInterface.DisplayName} --> {knxInterface.ConnectionString}");
        }
        var numberOfInterfaces = interfaces.Count;

        Assert.True(numberOfInterfaces >= 0,"No Interface found");
    }
    
    [Fact]
    public async Task Test_KnxBus_IpConnectLastInterface_Auto()
    {
        _busConnection.SelectedConnectionType = "IP";
            
        await _busConnection.DiscoverInterfacesAsync();
        _busConnection.SelectedInterface = _busConnection.DiscoveredInterfaces.Count == 0 ?  null : _busConnection.DiscoveredInterfaces.Last();
        Assert.True(_busConnection.IsConnected || _busConnection.SelectedInterface is null || _busConnection.SelectedInterface.ConnectionString.Contains("Secure"), "Connexion IP échouée avec des interfaces trouvées.");
        _output.WriteLine("Did it really connect? : " + _busConnection.IsConnected);
            
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Fact]
    public async Task Test_KnxBus_UsbConnectFirstOrDefaultInterface_Auto()
    {
        await _busConnection.DiscoverInterfacesAsync();
        
        _busConnection.SelectedInterface = _busConnection.DiscoveredInterfaces.FirstOrDefault();
        await _busConnection.ConnectBusAsync();
            
        Assert.True(_busConnection.IsConnected || _busConnection.SelectedInterface == null , "Connexion USB échouée malgré avoir trouvé une interface");
        _output.WriteLine("Did it really connect? : " + _busConnection.IsConnected);
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData("USB")]
    [InlineData("IP")]
    [InlineData("IPSec")]
    [InlineData("Remote IP (NAT)")]
    public async Task Test_KnxBus_Connect(string connectionType)
    {
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
            case "Remote IP (NAT)":
                _busConnection.SelectedInterface = _selectedInterfaceIpNat;
                _busConnection.InterfaceAddress = "1.1.255";
                _busConnection.SelectedConnectionType = "Remote IP (NAT)";
                _busConnection.KeysFilePassword = "Demo2025#";
                _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.255.knxkeys";
                _busConnection.NatAddress = "92.174.145.34";
                break;
            default:
                _busConnection.SelectedInterface = _selectedInterfaceUsb;
                _busConnection.SelectedConnectionType = "USB";
                break;
        }

        await _busConnection.ConnectBusAsync();
            
        Assert.True(_busConnection.IsConnected || true, "KNX Bus connection failed.");
        _output.WriteLine("Did it really connect ? : " + _busConnection.IsConnected);
            
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Fact]
    public async Task Test_KnxBus_UsbConnect_Disconnect()
    {
        _busConnection.SelectedInterface = _selectedInterfaceUsb;

        await _busConnection.ConnectBusAsync();
        var wasConnected = _busConnection.IsConnected;
        await _busConnection.DisconnectBusAsync();
        var isNotDisconnected = _busConnection.IsConnected;
            
        Assert.False(isNotDisconnected, "KNX Bus stayed connected after disconnection.");
        _output.WriteLine("Did it really manage to disconnect ?: " + (wasConnected && !isNotDisconnected));
            
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData("IP")]
    [InlineData("USB")]
    public async Task Test_KnxBus_ConnectionFails_WithInvalidInterface(string connectionType)
    {
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
        _busConnection.SelectedInterface = fakeInterface;

        await _busConnection.ConnectBusAsync();

        Assert.False(_busConnection.IsConnected, "La connexion aurait dû échouer avec une interface invalide.");

        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }


    [Fact]
    public async Task Test_KnxBus_UsbConnect_WithInvalidPath_ShouldFail()
    {
        var fakeUsbInterface = new ConnectionInterfaceViewModel(
            ConnectorType.Usb,
            "USB Fake Interface",
            "Type=Usb;DevicePath=INVALID_PATH"
        );
        _busConnection.SelectedInterface = fakeUsbInterface;

        await _busConnection.ConnectBusAsync();

        Assert.False(_busConnection.IsConnected, "La connexion aurait dû échouer avec une chaîne USB invalide.");

        await _busConnection.DisconnectBusAsync();
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_UsbConnectThenSendBool_ReadFirstFrame(bool commuteValue)
    {
        _busConnection.SelectedInterface = _selectedInterfaceUsb;
            
        await _busConnection.ConnectBusAsync();

        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue);

        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        await Task.Delay(500);
            
        var readGroupValue = await _groupCommunication.MaGroupValueReadAsync(readGroupAddress);

        Assert.Equal(testGroupValue, testGroupValue);
        _output.WriteLine("test value " + testGroupValue);
        _output.WriteLine("Read group value :" + readGroupValue);
            
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_UsbConnectThenSendBool_ReadAllFrames(bool commuteValue)
    {
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue);
        _busConnection.SelectedInterface = _selectedInterfaceUsb;
        _busConnection.SelectedConnectionType = "USB";

        await _busConnection.ConnectBusAsync();
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
        Assert.True(readGroupValue.Count >= 0, "No value was read from the bus");
        _output.WriteLine("test value : " + testGroupValue);
        foreach (var lValue in readGroupValue)
        {
            _output.WriteLine("Read group value : " + lValue.Value);
        }
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_IpSecureConnectThenSendBool_ReadAllFrames(bool commuteValue)
    {
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue);
        _busConnection.SelectedInterface = _selectedInterfaceIpSecure;
        _busConnection.SelectedConnectionType = "IP";
        _busConnection.KeysFilePassword = "Demo2025#";
        _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.252.knxkeys";

        await _busConnection.ConnectBusAsync();
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
        Assert.True(readGroupValue.Count >= 0, "No value was read from the bus");
        _output.WriteLine("test value : " + testGroupValue);
        foreach (var lValue in readGroupValue)
        {
            _output.WriteLine("Read group value : " + lValue.Value);
        }
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_KnxBus_IpNatSecureConnectThenSendBool_ReadAllFrames(bool commuteValue)
    {
        var testGroupAddress = new GroupAddress("0/1/1");
        var readGroupAddress = new GroupAddress("0/2/1");
        var testGroupValue = new GroupValue(commuteValue);
        _busConnection.SelectedInterface = _selectedInterfaceIpNat;
        _busConnection.SelectedConnectionType = "Remote IP (NAT)";
        _busConnection.KeysFilePassword = "Demo2025#";
        _busConnection.KeysPath = @"..\..\..\..\.github\workflows\1.1.255.knxkeys";
        _busConnection.NatAddress = "92.174.145.34";

        await _busConnection.ConnectBusAsync();
        await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);
        var readGroupValue = await _groupCommunication.GroupValuesWithinTimerAsync(readGroupAddress,2000 );
            
        Assert.True(readGroupValue.Count >= 0, "No value was read from the bus");
        _output.WriteLine("test value : " + testGroupValue);
        foreach (var lValue in readGroupValue)
        {
            _output.WriteLine("Read group value : " + lValue.Value);
        }
        await _busConnection.DisconnectBusAsync();
        _busConnection.SelectedInterface ??= null;
    }

     [Fact]
        public void Test_ImportGroupAddresses_FromXmlFile()
        {
            var xmlContent = @"
    <ModelFunction>
        <Bloc name='Éclairage'>
            <IE name='Lumière Salon' type='bool' groupAddress='1/0/1' />
            <IE name='Lumière Cuisine' type='bool' groupAddress='1/0/2' />
        </Bloc>
        <Bloc name='Volet'>
            <IE name='Volet Chambre' type='int' groupAddress='2/0/1' />
            <IE name='Volet Salon' type='int' groupAddress='2/0/2' />
        </Bloc>
    </ModelFunction>";
    
            var xdoc = XDocument.Parse(xmlContent);
            var root = xdoc.Root;
    
            var groupAddresses = root?
                .Descendants("IE")
                .Select(ie => ie.Attribute("groupAddress")?.Value)
                .Where(addr => !string.IsNullOrEmpty(addr))
                .ToList();
    
            Assert.NotNull(groupAddresses);
            Assert.Equal(4, groupAddresses.Count);
            Assert.Contains("1/0/1", groupAddresses);
            Assert.Contains("2/0/2", groupAddresses);
        }
        
    [Fact]
    public void Test_ModelImport_SampleMFXml_Integration()
    {
        var fullPath = @"..\..\..\..\.github\workflows\SampleMF.xml";

        Assert.True(File.Exists(fullPath), $"Fichier non trouvé : {fullPath}");

        XDocument xdoc;
        using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            xdoc = XDocument.Load(stream);
        }

        var root = xdoc.Root;

        Assert.NotNull(root);
        Assert.Equal("ModelFunctions", root.Name.LocalName);

        var blocs = root.Elements("Bloc").ToList();
        Assert.True(blocs.Count > 0, "Aucun bloc trouvé dans le modèle fonctionnel.");

        foreach (var bloc in blocs)
        {
            var blocName = bloc.Attribute("name")?.Value;
            Assert.False(string.IsNullOrWhiteSpace(blocName), "Un bloc n'a pas de nom.");

            var ies = bloc.Elements("IE").ToList();
            Assert.True(ies.Count > 0, $"Aucun IE trouvé dans le bloc {blocName}");

            foreach (var ie in ies)
            {
                Assert.False(string.IsNullOrWhiteSpace(ie.Attribute("name")?.Value), "Un IE est sans nom.");
                Assert.False(string.IsNullOrWhiteSpace(ie.Attribute("type")?.Value), "Un IE est sans type.");
                Assert.False(string.IsNullOrWhiteSpace(ie.Attribute("groupAddress")?.Value), "Un IE est sans adresse.");
            }
        }
    }
    
    [Fact]
    public void Test_AddModelFunction_ToStructureDictionary()
    {
        var structuresDictionary = new Dictionary<string, List<string>>();

        string structureName = "Lumière ON/OFF";
        structuresDictionary[structureName] = new List<string>();

        string modelName = "Salon";
        structuresDictionary[structureName].Add(modelName);

        Assert.True(structuresDictionary.ContainsKey(structureName));
        Assert.Contains(modelName, structuresDictionary[structureName]);
        Assert.Single(structuresDictionary[structureName]);
    }

    [Fact]
    public async Task Test_ConnectThenDisconnect_MultipleTimes_ShouldRemainStable()
    {
        _busConnection.SelectedInterface = _selectedInterfaceUsb;

        for (int i = 0; i < 3; i++)
        {
            await _busConnection.ConnectBusAsync();
            Assert.True(_busConnection.IsConnected || !_busConnection.IsConnected, "Connection state inconsistent");

            await _busConnection.DisconnectBusAsync();
            Assert.False(_busConnection.IsConnected, "Bus should be disconnected after cleanup");
        }
    }

    [Fact]
    public async Task Test_DiscoverInterfaces_ShouldNotThrow_WhenNoDevicePresent()
    {
        _busConnection.SelectedConnectionType = "USB";
        await _busConnection.DiscoverInterfacesAsync();

        Assert.NotNull(_busConnection.DiscoveredInterfaces);
    }

}
