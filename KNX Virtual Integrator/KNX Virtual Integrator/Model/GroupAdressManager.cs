using System.Xml;
using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model;

public class GroupAddressManagement
{
    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

    public static void ExtractGroupAddress()
    {
        // Vérification si l'utilisateur a choisi d'importer un fichier spécifique ou non
        string? filePath = App.DisplayElements != null && App.DisplayElements.MainWindow.UserChooseToImportGroupAddressFile 
            ? App.Fm?.GroupAddressFilePath 
            : App.Fm?.ZeroXmlPath;

        if (filePath != null)
        {
            XDocument? groupAddressFile = App.Fm?.LoadXmlDocument(filePath);
            if (groupAddressFile != null)
            {
                if (filePath == App.Fm?.ZeroXmlPath)
                {
                    SetNamespaceFromXml(filePath); // Définir le namespace si nécessaire
                }

                var groupAddresses = groupAddressFile.Descendants(_globalKnxNamespace + "GroupAddress").ToList();

                var groupedAddresses = new Dictionary<string, List<XElement>>();

                foreach (var ga in groupAddresses)
                {
                    var name = (string?)ga.Attribute("Name");
                    if (name != null)
                    {
                        if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
                        {
                            AddToGroupedAddresses(groupedAddresses, ga, name, 2); // Supprimer le préfixe "Ie"
                        }
                        else if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
                        {
                            AddToGroupedAddresses(groupedAddresses, ga, name, 3); 
                        }
                    }
                }
            }
        }
    }
    
    private static void AddToGroupedAddresses(Dictionary<string, List<XElement>> groupedAddresses, XElement ga, string name, int prefixLength)
    {
        var commonName = name.Substring(prefixLength);
        if (!groupedAddresses.ContainsKey(commonName))
        {
            groupedAddresses[commonName] = new List<XElement>();
        }
        groupedAddresses[commonName].Add(ga);
    }
    
    // Method that retrieves the namespace to use for searching in .xml files from the zeroFilePath (since the namespace varies depending on the ETS version)
    /// <summary>
    /// Sets the global KNX XML namespace from the specified XML file.
    ///
    /// This method loads the XML file located at <paramref name="zeroXmlFilePath"/> and retrieves
    /// the namespace declaration from the root element. If a namespace is found, it updates the
    /// static field <c>_globalKnxNamespace</c> with the retrieved namespace. If the XML file cannot
    /// be loaded or an error occurs during processing, appropriate error messages are logged.
    ///
    /// <param name="zeroXmlFilePath">The path to the XML file from which to extract the namespace.</param>
    /// </summary>
    private static void SetNamespaceFromXml(string zeroXmlFilePath)
    {
        try
        {
            var doc = new XmlDocument();

            // Load XML file
            doc.Load(zeroXmlFilePath);

            // Check the existence of the namespace in the root element
            var root = doc.DocumentElement;
            if (root != null)
            {
                // Get the namespace
                var xmlns = root.GetAttribute("xmlns");
                if (!string.IsNullOrEmpty(xmlns))
                {
                    _globalKnxNamespace = XNamespace.Get(xmlns);
                }
            }
        }
        catch (XmlException ex)
        {
            App.ConsoleAndLogWriteLine($"Error loading XML file (XML exception): {ex.Message}");
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during SetNamespaceFromXml(): {ex.Message}");
        }
    }
}