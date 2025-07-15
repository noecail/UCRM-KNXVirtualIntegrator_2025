using Knx.Falcon;
using Knx.Falcon.Configuration;
using Knx.Falcon.Discovery;
using Knx.Falcon.Sdk;

namespace KNX_Virtual_Integrator.Model.Wrappers;

public interface IKnxBusWrapper
{
    Task DisposeAsync();

    Task ConnectAsync(CancellationToken cancellationToken);
    
    void NewKnxBusWrapper(ConnectorParameters parameters);
    
    bool IsNull { get; }
    
    KnxBus? SetNull { get; set;}
    
    
    BusConnectionState ConnectionState { get; }

    IAsyncEnumerable<IpDeviceDiscoveryResult> DiscoverIpDevicesAsync(CancellationToken cancellationToken);

    IEnumerable<UsbDeviceDiscoveryResult> GetAttachedUsbDevices();

    event EventHandler<EventArgs>? ConnectionStateChanged;
    
    Task<bool> WriteGroupValueAsync(GroupAddress groupAddress, GroupValue value,
        MessagePriority priority = MessagePriority.High,
        CancellationToken cancellationToken = default(CancellationToken));

    Task RequestGroupValueAsync(GroupAddress groupAddress, MessagePriority priority = MessagePriority.High,
        CancellationToken cancellationToken = default(CancellationToken));

    event EventHandler<GroupEventArgs>? GroupMessageReceived;

}