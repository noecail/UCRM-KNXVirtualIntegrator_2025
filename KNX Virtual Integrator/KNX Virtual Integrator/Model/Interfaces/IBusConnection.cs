using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.ViewModel;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Interface pour gérer la connexion au bus KNX.
/// Fournit des propriétés et des méthodes pour se connecter, se déconnecter, découvrir les interfaces disponibles et gérer les états de connexion.
/// Cette interface implémente également <see cref="INotifyPropertyChanged"/> pour notifier les changements de propriété.
/// </summary>
public interface IBusConnection : INotifyPropertyChanged
{ 
    
    // ------------------------------------ PROPRIÉTÉS ------------------------------------ 
    
    /// <summary>
    ///     Observable collection of the discovered bus interfaces for the current connection type.
    ///     It contains instances of <see cref="ConnectionInterfaceViewModel"/> of the discovered interfaces
    /// </summary>
    ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; }

    /// <summary>
    ///    Currently selected bus interface. It is automatically updated and linked with the UI.
    /// </summary>
    ConnectionInterfaceViewModel? SelectedInterface { get; set; }
    
    /// <summary>
    ///     Indicate whether there is an ongoing activity (like a connection).
    ///     Used to deactivate the UI during the activity.
    /// </summary>
    bool IsBusy { get; set; }

    /// <summary>
    ///     Indicates whether the bus is connected or not. It is linked with the UI.
    /// </summary>
    bool IsConnected { get; set; }
    
    /// <summary>
    ///     Current connection state (i.e. : "Connected" ou "Disconnected"). Automatically updated
    /// </summary>
    string? ConnectionState { get; }
    
    /// <summary>
    ///     Current connection interface
    /// </summary>
    string CurrentInterface { get; }
    
    /// <summary>
    ///     IP address of the distant router to allow NAT connection
    /// </summary>
    string NatAddress { get; set; }

    /// <summary>
    ///     Property that indicates whether we use NAT to access the interface.
    /// </summary>
    public bool NatAccess { get; set; }

    /// <summary>
    ///     Individual Address for the given IP Secure interface
    /// </summary>
    public string InterfaceAddress { get;set; }
    
    /// <summary>
    ///     Password that allows access to the file that holds the knxkeys. See <see cref="KeysPath"/>
    /// </summary>
    string KeysFilePassword { get; set; }
    
    /// <summary>
    ///      The path to the file that holds the keys for the IP secure connection
    /// </summary>
    string KeysPath { get; set; }
    
    /// <summary>
    ///     Connection Type chosen by the user (IP, IP NAT, USB). Its changes are shared with the user interface
    /// </summary>
    string? SelectedConnectionType { get; set; }
    
    /// <summary>
    /// Property that possesses the error message to be printed to the user interface.
    ///     It is collected through <see cref="Implementations.BusConnection.CheckError"/>.
    ///     Different cases : 
    /// </summary>
    string ConnectionErrorMessage { get; set; }
    
    /// <summary>
    ///     Asynchronous method called when then <see cref="SelectedConnectionType"/> changes.
    ///     It tries to discover new interfaces with <see cref="DiscoverInterfacesAsync"/>.
    ///     If there is an error, it catches it and prints it.
    /// </summary>
    void OnSelectedConnectionTypeChanged();
       
    
/*--------------------------------------------------------------------------------------------------------------------*/
/********************************************** Various Methods *******************************************************/
/*--------------------------------------------------------------------------------------------------------------------*/

    
    /// <summary>
    ///     Establishes a connection to the Knx Bus asynchronously
    ///     It first verifies whether there is an ongoing operation. If not, it proceeds with the connection.
    ///     When it succeeds, it updates the connection state and its subscribers.
    ///     When it fails, it prints an error message.
    /// </summary>
    /// <returns>A task representing the completion to the connection.</returns>
    Task ConnectBusAsync();

    /// <summary>
    ///     Asynchronously disconnects from the KNX Bus.
    ///     It first verifies whether the bus is connected or if there is an ongoing operation.
    ///     If not, it starts the disconnection. When it succeeds, it updates the interface and prints a log.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de déconnexion asynchrone.</returns>
    Task DisconnectBusAsync();

    /// <summary>
    ///     Discover asynchronously the available interfaces according to the <see cref="SelectedConnectionType"/>.
    ///     This method discovers USB and IP interfaces and adds them to <see cref="DiscoveredInterfaces"/>.
    ///     The results are updated to the user interface.
    /// </summary>
    /// <returns>A task representing the completion of the method</returns>
    Task DiscoverInterfacesAsync();

    /// <summary>
    ///     Used for error handling in the case of NAT.
    ///     Checks if given IP address is in fact a correctly written IPv4 address
    /// </summary>
    /// <param name="ipString"> The IP address to check.</param>
    /// <returns> Returns true if the address has the form of an IPv4 address</returns>
    bool ValidateIPv4(string ipString);



}