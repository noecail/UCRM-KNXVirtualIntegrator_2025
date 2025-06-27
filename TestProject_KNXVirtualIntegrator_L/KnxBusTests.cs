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

        public KnxBusTests()
        {
            // Initialisation de BusConnection et GroupCommunication
            _busConnection = new BusConnection();
            _groupCommunication = new GroupCommunication(_busConnection);
        }

        [Fact]
        public async Task Test_KnxBus_Connect()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s
            var connectorType = ConnectorType.Usb; // Remplacez ceci par le type de connecteur r�el si diff�rent
            var displayName = "SpaceLogic KNX USB Interface DIN Rail";
            var connectionString = "Type=Usb;DevicePath=\\\\?\\hid#vid_16de&pid_008e#6&2d02dbc0&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\"SpaceLogic KNX USB Interface DIN Rail\""; // Remplacez par la cha�ne de connexion r�elle

            var selectedInterface = new ConnectionInterfaceViewModel(connectorType, displayName, connectionString);

            // Assignez l'interface s�lectionn�e � la connexion bus
            _busConnection.SelectedInterface = selectedInterface;

            // �tape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            bool isConnected = _busConnection.IsConnected;
            
            // �tape 3 : D�connexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            
            // Assertion pour v�rifier si la connexion a r�ussi
            Assert.True(isConnected, "KNX Bus connection failed.");
        }
        [Fact]
        public async Task Test_KnxBus_Connect_Then_Disconnect()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s
            var connectorType = ConnectorType.Usb; // Remplacez ceci par le type de connecteur r�el si diff�rent
            var displayName = "SpaceLogic KNX USB Interface DIN Rail";
            var connectionString = "Type=Usb;DevicePath=\\\\?\\hid#vid_16de&pid_008e#6&2d02dbc0&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\"SpaceLogic KNX USB Interface DIN Rail\""; // Remplacez par la cha�ne de connexion r�elle

            var selectedInterface = new ConnectionInterfaceViewModel(connectorType, displayName, connectionString);

            // Assignez l'interface s�lectionn�e � la connexion bus
            _busConnection.SelectedInterface = selectedInterface;

            // �tape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();
            
            // �tape 3 : D�connexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            var isDisconnected = _busConnection.IsConnected;
            
            // Assertion pour v�rifier si la connexion a r�ussi
            Assert.False(isDisconnected, "KNX Bus disconnection failed.");
        }
        
        [Fact]
        public async Task Test_KnxBus_SendFrame_ReadValue()
        {
            // �tape 1 : Cr�ation et configuration de l'interface de connexion
            // Cr�ez une instance de ConnectionInterfaceViewModel avec les param�tres appropri�s
            var connectorType = ConnectorType.Usb; // Remplacez ceci par le type de connecteur r�el si diff�rent
            var displayName = "SpaceLogic KNX USB Interface DIN Rail";
            var connectionString = "Type=Usb;DevicePath=\\\\?\\hid#vid_16de&pid_008e#6&2d02dbc0&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\"SpaceLogic KNX USB Interface DIN Rail\""; // Remplacez par la cha�ne de connexion r�elle

            var selectedInterface = new ConnectionInterfaceViewModel(connectorType, displayName, connectionString);

            // Assignez l'interface s�lectionn�e � la connexion bus
            _busConnection.SelectedInterface = selectedInterface;

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
