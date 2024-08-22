using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IBusConnection
{ 
    ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; }
    ConnectionInterfaceViewModel? SelectedInterface { get; set; }
    string SelectedConnectionType { get; set; }
    
    Task ConnectBusAsync();

    Task DisconnectBusAsync();

    Task DiscoverInterfacesAsync();

    void OnSelectedConnectionTypeChanged();
}