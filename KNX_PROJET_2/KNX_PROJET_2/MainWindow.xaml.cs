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
            this.Loaded += MainWindow_Loaded;
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)this.DataContext;
            await viewModel.DiscoverInterfacesAsync();
        }

    }
}



