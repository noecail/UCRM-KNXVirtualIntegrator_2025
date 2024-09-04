using Knx.Falcon.Configuration;

namespace KNX_Virtual_Integrator.ViewModel;

public class ConnectionInterfaceViewModel
{
    public ConnectorType ConnectorType { get; }
    public string DisplayName { get; set; } 
    public string ConnectionString { get; set; }

    public ConnectionInterfaceViewModel(ConnectorType connectorType, string displayName, string connectionString)
    {
        ConnectorType = connectorType;
        DisplayName = displayName;
        ConnectionString = connectionString;
    }

    public override string ToString() => DisplayName;
}