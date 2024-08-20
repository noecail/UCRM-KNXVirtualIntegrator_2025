using GalaSoft.MvvmLight;
using Knx.Falcon.Configuration;

namespace KNX_Virtual_Integrator.ViewModel;

public class ConnectionInterfaceViewModel
{
    public ConnectorType ConnectorType { get; }
    public string DisplayName { get; set; } // Ajout du set pour DisplayName
    public string ConnectionString { get; set; } // Ajout du set pour ConnectionString

    public ConnectionInterfaceViewModel(ConnectorType connectorType, string displayName, string connectionString)
    {
        ConnectorType = connectorType;
        DisplayName = displayName;
        ConnectionString = connectionString;
    }

    public override string ToString() => DisplayName;
}