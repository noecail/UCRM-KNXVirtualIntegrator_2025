using System.IO;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class NamespaceResolver(Logger logger) : INamespaceResolver 
{
    public XNamespace GlobalKnxNamespace = "http://knx.org/xml/ga-export/01";
    private readonly ILogger _logger = logger;
    
    // Method that retrieves the namespace to use for searching in .xml files from the zeroFilePath (since the namespace varies depending on the ETS version)
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
    public void SetNamespaceFromXml(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var document = XDocument.Load(stream);

            var defaultNamespace = document.Root?.GetDefaultNamespace();
            if (defaultNamespace != null)
            {
                GlobalKnxNamespace = defaultNamespace.NamespaceName;
            }
        }
        catch (Exception ex)
        {
            _logger.ConsoleAndLogWriteLine($"Error setting namespace from XML: {ex.Message}");
        }
    }
}