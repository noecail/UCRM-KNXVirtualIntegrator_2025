using System.ComponentModel;
using System.Windows;
using KNX_Virtual_Integrator.Model;

namespace KNX_Virtual_Integrator.ViewModel;

public partial class MainViewModel
{
    private Visibility _discoveredInterfacesVisibility = Visibility.Visible;
    public Visibility DiscoveredInterfacesVisibility
    {
        get => _discoveredInterfacesVisibility;
        set
        {
            if (_discoveredInterfacesVisibility == value) return;
            _discoveredInterfacesVisibility = value;
            WhenPropertyChanged(nameof(DiscoveredInterfacesVisibility));
        }
    }
    
    private Visibility _remoteConnexionVisibility = Visibility.Visible;
    public Visibility RemoteConnexionVisibility
    {
        get => _remoteConnexionVisibility;
        set
        {
            if (_remoteConnexionVisibility == value) return;
            _remoteConnexionVisibility = value;
            WhenPropertyChanged(nameof(RemoteConnexionVisibility));
        }
    }
    
    private Visibility _secureConnectionVisibility = Visibility.Visible;
    public Visibility SecureConnectionVisibility
    {
        get => _secureConnectionVisibility;
        set
        {
            if (_secureConnectionVisibility == value) return;
            _secureConnectionVisibility = value;
            WhenPropertyChanged(nameof(SecureConnectionVisibility));
        }
    }
}