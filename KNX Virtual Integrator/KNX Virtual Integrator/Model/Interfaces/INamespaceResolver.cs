namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface INamespaceResolver
{
    /// <summary>
    /// Sets the global KNX XML namespace from the specified XML file.
    ///
    /// This method loads the XML file located at <paramref name="filePath"/> and retrieves
    /// the namespace declaration from the root element. If a namespace is found, it updates the
    /// static field <c>_globalKnxNamespace</c> with the retrieved namespace. If the XML file cannot
    /// be loaded or an error occurs during processing, appropriate error messages are logged.
    ///
    /// <param name="filePath">The path to the XML file from which to extract the namespace.</param>
    /// </summary>
    public void SetNamespaceFromXml(string filePath);

}