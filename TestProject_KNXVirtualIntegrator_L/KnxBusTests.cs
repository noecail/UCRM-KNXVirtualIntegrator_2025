using System;
using System.Threading.Tasks;
using Knx.Falcon.Sdk;  // Par exemple pour BusConnection, GroupAddress
using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de l�.
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.ViewModel;
using System.Windows;
using GalaSoft.MvvmLight;
using Knx.Falcon;

using Moq;
using Xunit;

using Knx.Falcon.Configuration;

namespace TestProject_KNXVirtualIntegrator_L
{
    public class KnxBusTests
    {
        private readonly BusConnection _busConnection;
        private readonly GroupCommunication _groupCommunication;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceUsb;
        private readonly ConnectionInterfaceViewModel _selectedInterfaceIp;
        
        public KnxBusTests()
        {
            // Initialisation de BusConnection et GroupCommunication
            _busConnection = new BusConnection();
            _groupCommunication = new GroupCommunication(_busConnection);
            // Initialisation des interfaces de la maquette 
            // Pour modifier les interfaces de test (changement de maquette, rafraichissement,...), rajouter des lignes
            // Console.Write au niveau de la fonction DiscoverInterfaceAsync dans les blocs if
            _selectedInterfaceUsb = new ConnectionInterfaceViewModel(ConnectorType.Usb, 
                "SpaceLogic KNX USB Interface DIN Rail",
                "Type=Usb;DevicePath=\\\\?\\hid#vid_16de&pid_008e#6&2d02dbc0&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\"SpaceLogic KNX USB Interface DIN Rail\"");
            _selectedInterfaceIp = new ConnectionInterfaceViewModel(ConnectorType.IpTunneling, 
                "IP-Interface Secure 192.168.10.132",
                "Type=IpTunneling;HostAddress=192.168.10.132;SerialNumber=0001:0051F02C;MacAddress=000E8C00B56A;ProtocolType=Tcp;UseNat=True;Name=\"IP-Interface Secure\"");
        }

      [Fact]
        public async Task Discover_KnxInterfaces()
    {
            // Étape 1 : Récupération de toutes les interfaces détectées par Falcon
            var interfaces = await BusConnection.GetAvailableInterfacesAsync();

            // Étape 2 : Vérifications basiques sur la liste d'interfaces
            Assert.NotNull(interfaces);
            Assert.NotEmpty(interfaces);

            // Étape 3 : Affichage console pour visualiser les interfaces disponibles
            foreach (var knxInterface in interfaces)
            {
                Console.WriteLine($"[DETECTED] {knxInterface.DisplayName} → {knxInterface.ConnectionString}");
            }
        }

        [Fact]
        public async Task Test_KnxBus_IPConnect_Auto()
        {
            // Étape 1 : Récupération des interfaces disponibles
            var interfaces = await BusConnection.GetAvailableInterfacesAsync();

            // Étape 2 : Sélection automatique d'une interface IP compatible (mode tunneling)
            var selectedInterface = interfaces.FirstOrDefault(i =>
                i.ConnectorType == ConnectorType.IpTunneling ||
                (i.ConnectorType == ConnectorType.Ip && i.ConnectionString.Contains("Tunnel=true")) ||
                i.ConnectionString.Contains("Type=IpTunneling")
            );

            // Étape 3 : Vérification que l'interface IP a bien été trouvée
            Assert.NotNull(selectedInterface);
            Console.WriteLine("Interface IP détectée : " + selectedInterface.DisplayName);

            // Étape 4 : Connexion au bus via l'interface détectée
            _busConnection.SelectedInterface = selectedInterface;
            await _busConnection.ConnectBusAsync();

            // Étape 5 : Vérification de la connexion
            Assert.True(_busConnection.IsConnected, "Connexion IP échouée.");

            // Étape 6 : Déconnexion propre du bus
            await _busConnection.DisconnectBusAsync();
        }

        [Fact]
        public async Task Test_KnxBus_IPConnect()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s (ici, c'est dans le constructeur)
            // Assignez l'interface s�lectionn�e � la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceIp;

            // �tape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            bool isConnected = _busConnection.IsConnected;

            // �tape 3 : D�connexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();

            // Assertion pour v�rifier si la connexion a r�ussi
            Assert.True(isConnected, "KNX IP Bus connection failed.");
        }
        
        
        [Fact]
        public async Task Test_KnxBus_USBConnect()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s (ici, c'est dans le constructeur)
            // Assignez l'interface s�lectionn�e � la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // �tape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            bool isConnected = _busConnection.IsConnected;
            
            // �tape 3 : D�connexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            
            // Assertion pour v�rifier si la connexion a r�ussi
            Assert.True(isConnected, "KNX Bus connection failed.");
        }
        
        
        [Fact]
        public async Task Test_KnxBus_USBConnect_Disconnect()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s (ici, c'est dans le constructeur)
            // Assignez l'interface s�lectionn�e � la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // �tape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            
            // �tape 3 : D�connexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            var isDisconnected = _busConnection.IsConnected;
            
            // Assertion pour v�rifier si la connexion a r�ussi
            Assert.False(isDisconnected, "KNX Bus disconnection failed.");
        }
        
        [Fact]
        public async Task Test_KnxBus_USBConnectThenSendFrame_ReadValue()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s (ici, c'est dans le constructeur)
            // Assignez l'interface s�lectionn�e � la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // �tape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // �tape 3 : Envoi d'une trame � une adresse de groupe d�finie
            var testGroupAddress = new GroupAddress("0/1/1");
            var readGroupAddress = new GroupAddress("0/2/1");
            var testGroupValue = new GroupValue(true); // Valeur d'exemple (1 bit)

            await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);

            // �tape 4 : Lecture de la valeur de l'adresse de groupe
            var readGroupValue = await _groupCommunication.MaGroupValueReadAsync(readGroupAddress);

            //Assertions pour v�rifier si la valeur envoy�e est bien celle lue
            Assert.NotNull(readGroupValue);
            //Assert.Equal(testGroupValue.Value, readGroupValue.Value);

            // �tape 5 : D�connexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
        }
    }
}
