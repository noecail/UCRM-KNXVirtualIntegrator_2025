using System.Collections.ObjectModel;
using System.Windows;

namespace KNX_Virtual_Integrator.ViewModel;

public partial class MainViewModel
{
    /// <summary>
    /// The list of strings that represents all three possible connection types handled by the application.
    /// The three types are : "IP", "Remote IP (NAT)" and "USB".
    /// </summary>
    public ObservableCollection<string> ConnectionTypes { get; } = [
        "IP",
        "Remote IP (NAT)",
        "USB"
    ];
    
    // --------------------- Sections visibilities -------------------------
    /// <summary>
    /// The visibility state of the discovered interface listBox in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
    private Visibility _discoveredInterfacesVisibility = Visibility.Visible;
    /// <summary>
    /// Gets or sets the visibility state of the discovered interface
    /// listBox in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
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
    /// <summary>
    /// The visibility state of the content related to
    /// remote connection in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
    private Visibility _remoteConnexionVisibility = Visibility.Visible;
    /// <summary>
    /// Gets or sets the visibility state of the content related to
    /// remote connection in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
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
    /// <summary>
    /// The visibility state of the content related to
    /// secure connection in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
    private Visibility _secureConnectionVisibility = Visibility.Visible;
    /// <summary>
    /// Gets or sets the visibility state of the content related to
    /// secure connection in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
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
    /// <summary>
    /// The visibility state of the error messages in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
    private Visibility _errorMessageVisibility = Visibility.Collapsed;
    /// <summary>
    /// Gets or sets the visibility state of the error messages
    /// in <see cref="View.Windows.ConnectionWindow"/>.
    /// </summary>
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