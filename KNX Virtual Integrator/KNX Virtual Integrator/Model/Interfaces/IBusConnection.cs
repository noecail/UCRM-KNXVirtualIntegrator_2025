using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.ViewModel;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IBusConnection : INotifyPropertyChanged
{ 
    ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; }
    ConnectionInterfaceViewModel? SelectedInterface { get; set; }
    string SelectedConnectionType { get; set; }
    bool IsConnected { get; }
    
    Task ConnectBusAsync();

    Task DisconnectBusAsync();

    Task DiscoverInterfacesAsync();

    void OnSelectedConnectionTypeChanged();
}