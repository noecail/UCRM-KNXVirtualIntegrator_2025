using System.Windows;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Interop;
using System.IO;
using Knx.Falcon.Sdk;
using System.Windows.Controls;
using Knx.Falcon.Configuration;
using Knx.Falcon;
using Knx.Falcon.KnxnetIp;
using System.Collections.ObjectModel;

using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;


using GalaSoft.MvvmLight;
using System.Threading;
using System.Windows.Threading;

namespace KNX_PROJET_2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _cancellationTokenSource = new CancellationTokenSource();
            IsBusy = false;
            this.Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await DiscoverInterfacesAsync();
        }
    


        //ATTRIBUTS

        /// <summary>
        /// Represents the global XML namespace for KNX projects.
        /// </summary>
        private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

        /// <summary>
        /// Gets the path to the exported project folder.
        /// </summary>
        public string ProjectFolderPath { get; private set; } = "";

        



        // Gestion du clic sur le bouton Importer
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // Créer une instance de OpenFileDialog pour sélectionner un fichier XML
            OpenFileDialog openFileDialog = new()
            {
                Title = "Sélectionner un fichier XML",
                Filter = "Fichiers XML|*.xml|Tous les fichiers|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            // Afficher la boîte de dialogue et vérifier la sélection
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Appeler la méthode asynchrone pour afficher les adresses de groupe
                await ImportListGroupAddress(filePath);
            }
        }
        
        


        
        //TACHE IMPORTER LISTE DES ADRESSES DE GROUPE
        private async Task ImportListGroupAddress(string filePath)
        {
            try
            {
                // Charger les adresses de groupe à partir du fichier XML
                XDocument doc = XDocument.Load(filePath);

                // Supposez que les adresses de groupe sont stockées sous une balise <GroupAddress> dans le XML
                var allGroupAddresses = doc.Descendants(_globalKnxNamespace + "GroupAddress").ToList();

                var addresses = new List<string>();

                foreach (var groupAddress in allGroupAddresses)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("--------------------------------------------------------------------");
                    msg.AppendLine($"Name: {groupAddress.Attribute("Name")?.Value}");
                    msg.AppendLine($"Adresse: {groupAddress.Attribute("Address")?.Value}");

                    // Ajouter les adresses au message
                    addresses.Add(msg.ToString());

                    // Mettre à jour le contrôle UI avec les adresses
                    if (GroupAddressListControl != null)
                    {
                        GroupAddressListControl.UpdateGroupAddresses(addresses);
                    }
                    else
                    {
                        MessageBox.Show("Le contrôle de la liste des adresses de groupe n'est pas initialisé correctement.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier XML : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }












        private KnxBus _bus;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsBusy { get; private set; }
        public bool IsConnected => _bus != null && _bus.ConnectionState == BusConnectionState.Connected;

        //Gestion du clic sur le bouton Connect
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            await DiscoverInterfacesAsync();
            await ConnectBusAsync();
            
        }

        //Gestion du clic sur le bouton Disconnect
        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await DisconnectBusAsync();
        }
        
       

        //TACHE POUR CONNECTION AU BUS
        private async Task ConnectBusAsync()
        {
            if (IsBusy)
                return;

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Récupérer le type de connexion sélectionné
                var connectionString = ((ComboBoxItem)ConnectionTypeComboBox.SelectedItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    MessageBox.Show("Le type de connexion et la chaîne de connexion doivent être fournis.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Vérifier si un bus est déjà connecté, et le déconnecter si nécessaire
                if (_bus != null)
                {
                    _bus.ConnectionStateChanged -= BusConnectionStateChanged;
                    await _bus.DisposeAsync();
                    _bus = null;
                    UpdateConnectionState();
                }

                var connectorParameters = CreateConnectorParameters(connectionString);
                
                //var connectorParameters = ConnectorParameters.FromConnectionString(connectionString);

                // Connexion au bus
                var bus = new KnxBus(connectorParameters);
                await bus.ConnectAsync(_cancellationTokenSource.Token);

                // Vérifier si la connexion est établie
                if (bus.ConnectionState == BusConnectionState.Connected)
                {
                    _bus = bus;
                    _bus.ConnectionStateChanged += BusConnectionStateChanged;
                    UpdateConnectionState();
                    MessageBox.Show("Connexion réussie au bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    throw new InvalidOperationException("La connexion au bus a échoué.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion au bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Dispose de CancellationTokenSource après la tâche
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }


        //TACHE POUR DECONNECTION AU BUS

        private async Task DisconnectBusAsync()
        {
            
            if (IsBusy || !IsConnected)
                return; //test

            // Indiquer que la déconnexion est en cours
            _cancellationTokenSource?.Cancel(); // Annule toute opération en cours si nécessaire
            //_cancellationTokenSource.Dispose(); //déja fait avant
            _cancellationTokenSource = null;

            try
            {
                if (_bus != null)
                {
                    _bus.ConnectionStateChanged -= BusConnectionStateChanged;
                    await _bus.DisposeAsync();
                    _bus = null;
                

                    UpdateConnectionState();

                    // Affichage d'un message de succès
                    MessageBox.Show("Déconnexion réussie du bus.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Si le bus est déjà déconnecté, informer l'utilisateur
                    MessageBox.Show("Le bus est déjà déconnecté.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
        }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la déconnexion du bus : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateConnectionState()
        {
            // Met à jour l'état de la connexion dans l'interface utilisateur
            ConnectionStateTextBlock.Text = IsConnected ? "Connected" : "Disconnected";
        }

        private void BusConnectionStateChanged(object sender, EventArgs e)
        {
            // Cette méthode sera appelée lorsque l'état de la connexion change
            // Mettre à jour l'état de la connexion
            Dispatcher.Invoke(() => UpdateConnectionState());

        }

        private ConnectorParameters CreateConnectorParameters(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La chaîne de connexion ne peut pas être vide.", nameof(connectionString));
            }

            // Extrait les parties de la chaîne de connexion
            var parts = connectionString.Split(';');
            var typePart = parts.FirstOrDefault(p => p.StartsWith("Type=", StringComparison.OrdinalIgnoreCase));
            var multicastAddressPart = parts.FirstOrDefault(p => p.StartsWith("MulticastAddress=", StringComparison.OrdinalIgnoreCase));
            var localIpAddressPart = parts.FirstOrDefault(p => p.StartsWith("LocalIPAddress=", StringComparison.OrdinalIgnoreCase));
            var portPart = parts.FirstOrDefault(p => p.StartsWith("Port=", StringComparison.OrdinalIgnoreCase));
            var namePart = parts.FirstOrDefault(p => p.StartsWith("Name=", StringComparison.OrdinalIgnoreCase));

            if (typePart == null)
            {
                throw new InvalidOperationException("Le type de connexion est manquant dans la chaîne de connexion.");
            }

            var connectionType = typePart.Substring("Type=".Length).Trim();

            switch (connectionType)
            {
                case "USB":
                    // Créer les paramètres pour la connexion USB
                    return ConnectorParameters.FromConnectionString(connectionString);

                case "IpRouting":
                    // Assurez-vous que toutes les informations nécessaires sont présentes pour IP Routing 
                    if (multicastAddressPart == null || localIpAddressPart == null || portPart == null)
                    {
                        throw new InvalidOperationException("Les informations nécessaires pour la connexion IP Routing sont manquantes.");
                    }

                    var multicastAddress = multicastAddressPart.Substring("MulticastAddress=".Length).Trim();
                    var localIpAddress = localIpAddressPart.Substring("LocalIPAddress=".Length).Trim();
                    var port = int.Parse(portPart.Substring("Port=".Length).Trim());

                    // Utiliser les informations extraites pour créer les paramètres de connexion IP Routing
                    return new IpRoutingConnectorParameters
                    {
                        //MulticastAddress = multicastAddress,
                        //LocalIPAddress = localIpAddress,
                        //Port = port,
        

                        Name = namePart?.Substring("Name=".Length).Trim() // Utiliser le nom 
                    };

                default:
                    throw new InvalidOperationException("Type de connexion inconnu.");
            }
        }





        public void UpdateDiscoveredInterfaces(ObservableCollection<InterfaceViewModel> interfaces)
        {
            InterfaceListBox.ItemsSource = interfaces;
        }

        private async void RefreshInterfacesButton_Click(object sender, RoutedEventArgs e)
        {
            await DiscoverInterfacesAsync();
        }

        private async Task DiscoverInterfacesAsync()
        {
            try
            {
                // Créer un objet ObservableCollection pour stocker les interfaces découvertes
                var discoveredInterfaces = new ObservableCollection<InterfaceViewModel>();

                // Découverte des interfaces IP
                var ipDiscoveryTask = Task.Run(async () =>
                {
                    
                    using (var cts = new CancellationTokenSource())
                    {

                        var results = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);
                        

                        await foreach (var result in results)
                        {
                            // Traitement des connexions tunneling
                            foreach (var tunnelingServer in result.GetTunnelingConnections())
                            {
                                // Mettre à jour les connexions existantes ou ajouter de nouvelles connexions
                                if (result.IsExtensionOf != null)
                                {
                                    var existing = discoveredInterfaces.FirstOrDefault(i => i.ConnectionString == result.IsExtensionOf.ToConnectionString());
                                    if (existing != null)
                                    {
                                        existing.ConnectionString = tunnelingServer.ToConnectionString();
                                        continue;
                                    }
                                }

                                var displayName = tunnelingServer.IndividualAddress.HasValue
                                    ? $"{tunnelingServer.Name} {tunnelingServer.HostAddress} ({tunnelingServer.IndividualAddress.Value})"
                                    : $"{tunnelingServer.Name} {tunnelingServer.HostAddress}";
                                var tunneling = new InterfaceViewModel(ConnectorType.IpTunneling, displayName, tunnelingServer.ToConnectionString());
                                discoveredInterfaces.Add(tunneling);
                            }

                            // Traitement des connexions IP routing
                            if (result.Supports(ServiceFamily.Routing, 1))
                            {
                                var routingParameters = IpRoutingConnectorParameters.FromDiscovery(result);
                                var routing = new InterfaceViewModel(ConnectorType.IpRouting, $"{result.MulticastAddress} on {result.LocalIPAddress}", routingParameters.ToConnectionString());
                                discoveredInterfaces.Add(routing);
                            }
                        }
                    }
                });

                // Découverte des périphériques USB
                var usbDiscoveryTask = Task.Run(() =>
                {
                    foreach (var usbDevice in KnxBus.GetAttachedUsbDevices())
                    {
                        var interfaceViewModel = new InterfaceViewModel(ConnectorType.Usb, usbDevice.DisplayName, usbDevice.ToConnectionString());
                        discoveredInterfaces.Add(interfaceViewModel);
                    }
                });

                // Attendre que toutes les découvertes soient terminées
                await Task.WhenAll(ipDiscoveryTask, usbDiscoveryTask);

                // Mettre à jour l'interface utilisateur avec les résultats
                InterfaceListBox.ItemsSource = discoveredInterfaces;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la découverte des interfaces : {ex.Message}");
            }
        }





    }


}



