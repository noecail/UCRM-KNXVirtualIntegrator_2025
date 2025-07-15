using Knx.Falcon;
using Knx.Falcon.Configuration;
using Knx.Falcon.Discovery;
using Knx.Falcon.Sdk;

namespace KNX_Virtual_Integrator.Model.Wrappers;

public class KnxBusWrapper : IKnxBusWrapper
{
    private KnxBus? _knxBus;

    public bool IsNull => _knxBus == null;

    public KnxBus? SetNull
    {
        get => _knxBus;
        set => _knxBus = value;
    }

    public KnxBusWrapper()
    {
        _knxBus = null;
    }

    public void NewKnxBusWrapper(ConnectorParameters parameters)
    {
        _knxBus = new KnxBus(parameters);
        _knxBus.ConnectionStateChanged += StateChangedInvoker;
        _knxBus.GroupMessageReceived += GroupMessageInvoker;
    }
    public async Task DisposeAsync()
    {
        if (_knxBus != null)
            await _knxBus.DisposeAsync();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_knxBus != null)
            await _knxBus.ConnectAsync(cancellationToken);
    }

    public BusConnectionState ConnectionState
    {
        get
        {
            if (_knxBus == null) return BusConnectionState.Closed;
            return _knxBus.ConnectionState;
        }
    }

    public IAsyncEnumerable<IpDeviceDiscoveryResult> DiscoverIpDevicesAsync(CancellationToken cancellationToken)
    {
        return KnxBus.DiscoverIpDevicesAsync(cancellationToken);
    }

    public IEnumerable<UsbDeviceDiscoveryResult> GetAttachedUsbDevices()
    {
        return KnxBus.GetAttachedUsbDevices(); 
    }

    public event EventHandler<EventArgs>? ConnectionStateChanged;

    public void StateChangedInvoker(object? sender, EventArgs args)
    {
        ConnectionStateChanged?.Invoke(sender, args);
    }

    public async Task<bool> WriteGroupValueAsync(GroupAddress groupAddress, GroupValue value,MessagePriority priority = MessagePriority.High, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (_knxBus == null) return false;
        return await _knxBus.WriteGroupValueAsync(groupAddress, value, priority, cancellationToken);
    }

    public async Task RequestGroupValueAsync(GroupAddress groupAddress, MessagePriority priority = MessagePriority.High,
        CancellationToken cancellationToken = default)
    {
        if (_knxBus != null)
            await _knxBus.RequestGroupValueAsync(groupAddress, priority, cancellationToken);
    }
    
    public event EventHandler<GroupEventArgs>? GroupMessageReceived;
    public void GroupMessageInvoker(object? sender, GroupEventArgs args)
    {
        GroupMessageReceived?.Invoke(sender, args);
    }
        
}