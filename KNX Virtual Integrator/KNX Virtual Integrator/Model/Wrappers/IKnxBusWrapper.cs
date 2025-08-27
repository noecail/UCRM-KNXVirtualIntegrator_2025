using Knx.Falcon;
using Knx.Falcon.Configuration;
using Knx.Falcon.Discovery;
using Knx.Falcon.Sdk;

namespace KNX_Virtual_Integrator.Model.Wrappers;
/// <summary>
///     Class wrapper of <see cref="KnxBus"/>. A Wrapper is used to reduce code dependency from the wrapped class
///     (for tests or if you want to replace some functionalities).
///     The Interface is the type of all variables <see cref="Model.Implementations.BusConnection.Bus"/>,
///     so that during multiple implementations can exist, especially during tests where we may
///     not want to really communicate with the bus.
/// </summary>
public interface IKnxBusWrapper
{
    /// <summary>
    ///     Property getter to check whether the bus is null or not
    /// </summary>
    bool IsNull { get; }
    
    /// <summary>
    ///     Property setter, mostly created to set the KNX Bus to null 
    /// </summary>
    KnxBus? KnxBusSetter { set;}
    
    /// <summary>
    ///     Wrapper to get information about the current state of the connection
    /// </summary>
    BusConnectionState ConnectionState { get; }
    
    /// <summary>
    ///     Wrapper of KnxBus.<see cref="KnxBus.DiscoverIpDevicesAsync"/>. Discovers KNX/IP devices asynchronously
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the operation.</param>
    /// <returns><see cref="IpDeviceDiscoveryResult"/>s of the detected devices</returns>
    IAsyncEnumerable<IpDeviceDiscoveryResult> DiscoverIpDevicesAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Wrapper of KnxBus.<see cref="KnxBus.GetAttachedUsbDevices"/>. Gets the currently attached KNX USB devices
    /// </summary>
    /// <returns>The found KNX USB interfaces</returns>
    IEnumerable<UsbDeviceDiscoveryResult> GetAttachedUsbDevices();
    
    /// <summary>
    ///     New way to initialize a KNX Bus. It subscribes its wrapper event handlers to relay the real events 
    /// </summary>
    /// <param name="parameters"> The parameters used for the KnxBus constructor : <see cref="ConnectorParameters"/></param>
    void NewKnxBusWrapper(ConnectorParameters parameters);
    
    /// <summary>
    ///     Wrapper method to <see cref="KnxBus.ConnectAsync"/>. Connects to the bus interface
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the operation.</param>
    Task ConnectAsync(CancellationToken cancellationToken);
    
    /// <summary>
    ///     Wrapper method to KnxBux.<see cref="KnxBus.DisposeAsync"/>. Releases
    ///     the <see cref="KnxBus"/> object and frees all ressources asynchronously
    /// </summary>
    Task DisposeAsync();
    
    /// <summary>
    ///     Wrapper of <see cref="KnxBus.WriteGroupValueAsync"/>. Writes a group value to
    ///     the bus with the specified priority
    /// </summary>
    /// <param name="groupAddress">The group address</param>
    /// <param name="value">The value to write</param>
    /// <param name="priority">The message priority to use</param>
    /// <param name="cancellationToken">Can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.
    ///     The result is true if the message was acknowledged positively,
    ///     true if the message was acknowledged negatively.
    ///     When sent on multiple RF modes, true is returned only if acknowledged positively on all modes.
    /// </returns>
    Task<bool> WriteGroupValueAsync(GroupAddress groupAddress, GroupValue value,
        MessagePriority priority = MessagePriority.High,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Wrapper to <see cref="KnxBus.RequestGroupValueAsync"/>. Sends a group value read request
    ///     to the bus with the specified priority without waiting for a response.
    /// </summary>
    /// <param name="groupAddress">The group address</param>
    /// <param name="priority">The message priority to use</param>
    /// <param name="cancellationToken">Can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.
    ///     The result is true if the message was acknowledged positively,
    ///     true if the message was acknowledged negatively.
    ///     When sent on multiple RF modes, true is returned only if acknowledged positively on all modes.
    /// </returns>
    Task<bool> RequestGroupValueAsync(GroupAddress groupAddress, MessagePriority priority = MessagePriority.High,
        CancellationToken cancellationToken = default);
    
    
    /// <summary>
    ///     Wrapper event to relay the event <see cref="KnxBus.ConnectionStateChanged"/> to notify when
    ///     <see cref="KnxBus.ConnectionState"/> has changed
    /// </summary>
    event EventHandler<EventArgs>? ConnectionStateChanged;
    
    /// <summary>
    ///     Wrapper event to relay the event <see cref="KnxBus.GroupMessageReceived"/> to notify when
    ///     a group message was received from the bus
    /// </summary>
    event EventHandler<GroupEventArgs>? GroupMessageReceived;

    /// <summary>
    ///     Method used to relay the event <see cref="KnxBus.ConnectionStateChanged"/> from the KNX Bus
    ///     to the handlers subscribed to it
    /// </summary>
    /// <param name="sender">Who fires the event</param>
    /// <param name="args">The new state</param>
    void StateChangedInvoker(object? sender, EventArgs args);
    
    /// <summary>
    ///     Method used to relay the event <see cref="KnxBus.GroupMessageReceived"/> from the KNX Bus
    ///     to the handlers subscribed to it    
    /// </summary>
    /// <param name="sender"> Who fires the event </param>
    /// <param name="args"> The new group message </param>
    void GroupMessageInvoker(object? sender, GroupEventArgs args);

}