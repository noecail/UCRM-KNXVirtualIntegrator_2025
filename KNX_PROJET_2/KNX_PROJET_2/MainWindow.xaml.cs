using System.Windows;
using System.Xml.Linq;
using Knx.Falcon.Sdk;
using Knx.Falcon.Configuration;
using Knx.Falcon.KnxnetIp;
using System.Collections.ObjectModel;


namespace KNX_PROJET_2
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
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
        private KnxBus _bus;

        public bool IsBusy { get; private set; }

        
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



