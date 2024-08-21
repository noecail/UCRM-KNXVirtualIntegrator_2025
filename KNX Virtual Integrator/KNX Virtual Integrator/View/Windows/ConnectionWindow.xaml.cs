using System.Windows;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View.Windows;

public partial class ConnectionWindow : Window
{
    private readonly MainViewModel _viewModel;

    public ConnectionWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += ConnectionWindow_Loaded;
    }
    
    private async void ConnectionWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // await .DiscoverInterfacesAsync();
    }
}