using Knx.Falcon.Configuration;

namespace KNX_Virtual_Integrator.ViewModel;

/// <summary>
/// Represents a view model for a connection interface, including connection details and display information.
/// </summary>
public class ConnectionInterfaceViewModel
{
    /// <summary>
    /// Gets the type of connector for the connection interface.
    /// </summary>
    public ConnectorType ConnectorType { get; }

    /// <summary>
    /// Gets or sets the display name of the connection interface.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the connection string used for the connection interface.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionInterfaceViewModel"/> class.
    /// </summary>
    /// <param name="connectorType">The type of connector for the connection interface.</param>
    /// <param name="displayName">The display name of the connection interface.</param>
    /// <param name="connectionString">The connection string for the connection interface.</param>
    public ConnectionInterfaceViewModel(ConnectorType connectorType, string displayName, string connectionString)
    {
        ConnectorType = connectorType;
        DisplayName = displayName;
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>The display name of the connection interface.</returns>
    public override string ToString() => DisplayName;
}