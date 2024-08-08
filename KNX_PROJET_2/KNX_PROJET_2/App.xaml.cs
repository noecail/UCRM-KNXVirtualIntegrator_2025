using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Knx.Falcon.Configuration;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon.Sdk;

namespace KNX_PROJET_2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private async Task DiscoverInterfacesAsync()
        {
            try
            {
                var discoveredInterfaces = new ObservableCollection<InterfaceViewModel>();

                var ipDiscoveryTask = Task.Run(async () =>
                {
                    var results = KnxBus.DiscoverIpDevicesAsync(CancellationToken.None);
                    await foreach (var result in results)
                    {
                        foreach (var tunnelingServer in result.GetTunnelingConnections())
                        {
                            discoveredInterfaces.Add(new InterfaceViewModel(
                                ConnectorType.IpTunneling,
                                tunnelingServer.Name,
                                tunnelingServer.ToConnectionString()));
                        }

                        if (result.Supports(ServiceFamily.Routing, 1))
                        {
                            var routingParameters = IpRoutingConnectorParameters.FromDiscovery(result);
                            discoveredInterfaces.Add(new InterfaceViewModel(
                                ConnectorType.IpRouting,
                                $"{result.MulticastAddress} on {result.LocalIPAddress}",
                                routingParameters.ToConnectionString()));
                        }
                    }
                });

                var usbDiscoveryTask = Task.Run(() =>
                {
                    foreach (var usbDevice in KnxBus.GetAttachedUsbDevices())
                    {
                        discoveredInterfaces.Add(new InterfaceViewModel(
                            ConnectorType.Usb,
                            usbDevice.DisplayName,
                            usbDevice.ToConnectionString()));
                    }
                });

                await Task.WhenAll(ipDiscoveryTask, usbDiscoveryTask);

                // Mettre à jour l'interface utilisateur avec les résultats
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow.UpdateDiscoveredInterfaces(discoveredInterfaces);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la découverte des interfaces : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
