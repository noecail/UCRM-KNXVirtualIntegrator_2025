using System;
using System.Threading.Tasks;
using Knx.Falcon.Sdk;  // Par exemple pour BusConnection, GroupAddress
using KNX_Virtual_Integrator.Model.Implementations; // Pour d'autres classes si elles viennent de là.
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
            // Assignez l'interface sélectionnée à la connexion au bus
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
            
            // Étape 3 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            var isDisconnected = _busConnection.IsConnected;
            
            // Assertion pour vérifier si la connexion a réussi
            Assert.False(isDisconnected, "KNX Bus disconnection failed.");
        }
        
        [Fact]
        public async Task Test_KnxBus_USBConnectThenSendFrame_ReadValue()
        {
            // Étape 1 : Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés (ici, c'est dans le constructeur)
            // Assignez l'interface sélectionnée à la connexion au bus
            _busConnection.SelectedInterface = _selectedInterfaceUsb;

            // Étape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Étape 3 : Envoi d'une trame à une adresse de groupe définie
            var testGroupAddress = new GroupAddress("0/1/1");
            var readGroupAddress = new GroupAddress("0/2/1");
            var testGroupValue = new GroupValue(true); // Valeur d'exemple (1 bit)

            await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);

            // Étape 4 : Lecture de la valeur de l'adresse de groupe
            var readGroupValue = await _groupCommunication.MaGroupValueReadAsync(readGroupAddress);

            //Assertions pour vérifier si la valeur envoyée est bien celle lue
            Assert.NotNull(readGroupValue);
            //Assert.Equal(testGroupValue.Value, readGroupValue.Value);

            // Étape 5 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
        }
    }
}
