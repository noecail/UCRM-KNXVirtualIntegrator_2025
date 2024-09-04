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

using System;
using System.Threading.Tasks;
using Knx.Falcon;
using KNX_Virtual_Integrator.Model.Implementations;
using KNX_Virtual_Integrator.ViewModel;
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
        public async Task Test_KnxBus_Connection_SendFrame_ReadValue()
        {
            // Étape 1 : Création et configuration de l'interface de connexion
            // Créez une instance de ConnectionInterfaceViewModel avec les paramètres appropriés
            var connectorType = ConnectorType.Usb; // Remplacez ceci par le type de connecteur réel si différent
            var displayName = "SpaceLogic KNX USB Interface DIN Rail";
            var connectionString = "\"Type=Usb;DevicePath=\\\\\\\\?\\\\hid#vid_16de&pid_008e#6&2d02dbc0&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030};Name=\\\"SpaceLogic KNX USB Interface DIN Rail\\\"\""; // Remplacez par la chaîne de connexion réelle

            var selectedInterface = new ConnectionInterfaceViewModel(connectorType, displayName, connectionString);

            // Assignez l'interface sélectionnée à la connexion bus
            _busConnection.SelectedInterface = selectedInterface;

            // Étape 2 : Connexion au bus KNX
            await _busConnection.ConnectBusAsync();

            // Assertion pour vérifier si la connexion a réussi
            Assert.True(_busConnection.IsConnected, "La connexion au bus KNX a échoué.");

            // Étape 3 : Envoi d'une trame à une adresse de groupe définie
            var testGroupAddress = new GroupAddress("0/1/1");
            var readGroupAddress = new GroupAddress("0/2/1");
            var testGroupValue = new GroupValue(true); // Valeur d'exemple (1 bit)

            await _groupCommunication.GroupValueWriteAsync(testGroupAddress, testGroupValue);

            // Étape 4 : Lecture de la valeur de l'adresse de groupe
            var readGroupValue = await _groupCommunication.MaGroupValueReadAsync(readGroupAddress);

            // Assertions pour vérifier si la valeur envoyée est bien celle lue
            //Assert.NotNull(readGroupValue);
            //Assert.Equal(testGroupValue.Value, readGroupValue.Value);

            // Étape 5 : Déconnexion du bus KNX (optionnel)
            await _busConnection.DisconnectBusAsync();
            Assert.False(_busConnection.IsConnected, "La déconnexion du bus KNX a échoué.");
        }
    }
}
