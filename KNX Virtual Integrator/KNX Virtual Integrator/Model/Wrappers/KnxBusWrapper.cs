using Knx.Falcon;
using Knx.Falcon.Configuration;
using Knx.Falcon.Discovery;
using Knx.Falcon.Sdk;

namespace KNX_Virtual_Integrator.Model.Wrappers;

public class KnxBusWrapper : IKnxBusWrapper
{
    /// <summary>
    ///     This is the real KNX Bus hidden in the backend of the wrapper
    /// </summary>
    private KnxBus? _knxBus;

    /// <summary>
    ///     Property getter to check whether the bus is null or not
    /// </summary>
    public bool IsNull => _knxBus == null;

    /// <summary>
    ///     Property setter, mostly created to set the KNX Bus to null 
    /// </summary>
    public KnxBus? KnxBusSetter
    {
        set => _knxBus = value;
    }

    /// <summary>
    ///     Wrapper to get information about the current state of the connection
    /// </summary>
    public BusConnectionState ConnectionState => _knxBus?.ConnectionState ?? BusConnectionState.Closed;
    
    /// <summary>
    ///     Wrapper of KnxBus.<see cref="KnxBus.DiscoverIpDevicesAsync"/>. Discovers KNX/IP devices asynchronously
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the operation.</param>
    /// <returns><see cref="IpDeviceDiscoveryResult"/>s of the detected devices</returns>
    public IAsyncEnumerable<IpDeviceDiscoveryResult> DiscoverIpDevicesAsync(CancellationToken cancellationToken)
    {
        return KnxBus.DiscoverIpDevicesAsync(cancellationToken);
    }
    
    /// <summary>
    ///     Wrapper of KnxBus.<see cref="KnxBus.GetAttachedUsbDevices"/>. Gets the currently attached KNX USB devices
    /// </summary>
    /// <returns>The found KNX USB interfaces</returns>
    public IEnumerable<UsbDeviceDiscoveryResult> GetAttachedUsbDevices()
    {
        return KnxBus.GetAttachedUsbDevices(); 
    }
    
    /// <summary>
    ///     New way to initialize a KNX Bus. It subscribes its wrapper event handlers to relay the real events 
    /// </summary>
    /// <param name="parameters"> The parameters used for the KnxBus constructor : <see cref="ConnectorParameters"/></param>
    public void NewKnxBusWrapper(ConnectorParameters parameters)
    {
        _knxBus = new KnxBus(parameters);
        _knxBus.ConnectionStateChanged += StateChangedInvoker;
        _knxBus.GroupMessageReceived += GroupMessageInvoker;
    }
    
    /// <summary>
    ///     Wrapper method to <see cref="KnxBus.ConnectAsync"/>. Connects to the bus interface
    /// </summary>
    /// <param name="cancellationToken">Can be used to cancel the operation.</param>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_knxBus != null)
            await _knxBus.ConnectAsync(cancellationToken);
    }
    
    /// <summary>
    ///     Wrapper method to KnxBux.<see cref="KnxBus.DisposeAsync"/>. Releases
    ///     the <see cref="KnxBus"/> object and frees all ressources asynchronously
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_knxBus != null)
            await _knxBus.DisposeAsync();
    }
    
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
    public async Task<bool> WriteGroupValueAsync(GroupAddress groupAddress, GroupValue value,MessagePriority priority = MessagePriority.High, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (_knxBus == null) return false;
        return await _knxBus.WriteGroupValueAsync(groupAddress, value, priority, cancellationToken);
    }

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
    public async Task<bool> RequestGroupValueAsync(GroupAddress groupAddress, MessagePriority priority = MessagePriority.High,
        CancellationToken cancellationToken = default)
    {
        if (_knxBus != null)
            return await _knxBus.RequestGroupValueAsync(groupAddress, priority, cancellationToken);
        return false;
    }
    
    /// <summary>
    ///     Wrapper event to relay the event <see cref="KnxBus.ConnectionStateChanged"/> to notify when
    ///     <see cref="KnxBus.ConnectionState"/> has changed
    /// </summary>
    public event EventHandler<EventArgs>? ConnectionStateChanged;

    
    /// <summary>
    ///     Wrapper event to relay the event <see cref="KnxBus.GroupMessageReceived"/> to notify when
    ///     a group message was received from the bus
    /// </summary>
    public event EventHandler<GroupEventArgs>? GroupMessageReceived;
    
    /// <summary>
    ///     Method used to relay the event <see cref="KnxBus.ConnectionStateChanged"/> from the KNX Bus
    ///     to the handlers subscribed to it
    /// </summary>
    /// <param name="sender">Who fires the event</param>
    /// <param name="args">The new state</param>
    public void StateChangedInvoker(object? sender, EventArgs args)
    {
        ConnectionStateChanged?.Invoke(sender, args);
    }
    
    /// <summary>
    ///     Method used to relay the event <see cref="KnxBus.GroupMessageReceived"/> from the KNX Bus
    ///     to the handlers subscribed to it    
    /// </summary>
    /// <param name="sender"> Who fires the event </param>
    /// <param name="args"> The new group message </param>
    public void GroupMessageInvoker(object? sender, GroupEventArgs args)
    {
        GroupMessageReceived?.Invoke(sender, args);
    }
        
}