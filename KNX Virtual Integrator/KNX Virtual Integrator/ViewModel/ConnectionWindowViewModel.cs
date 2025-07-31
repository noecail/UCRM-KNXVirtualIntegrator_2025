using System.Windows;

namespace KNX_Virtual_Integrator.ViewModel;

public partial class MainViewModel
{
    
    // --------------------- Sections visibilities -------------------------
    
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
    
    private Visibility _errorMessageVisibility = Visibility.Collapsed;
    public Visibility ErrorMessageVisibility
    {
        get => _errorMessageVisibility;
        set
        {
            if (_errorMessageVisibility == value) return;
            _errorMessageVisibility = value;
            WhenPropertyChanged(nameof(ErrorMessageVisibility));
        }
    }
}