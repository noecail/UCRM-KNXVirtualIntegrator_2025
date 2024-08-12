using System.Xml;
using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model;

public class GroupAddressManagement
{
    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

    public static void ExtractGroupAddress()
    {
        if (App.WindowManager != null && App.WindowManager.MainWindow.UserChooseToImportGroupAddressFile)
        {
            var groupAddressFile = FileLoader.LoadXmlDocument(App.Fm.GroupAddressFilePath);
            var groupAddresses = groupAddressFile?.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
            
            if (groupAddresses != null)
            {
                var ieGroupAddresses = groupAddresses
                    .Where(ga => 
                    {
                        var name = (string?)ga.Attribute("Name");
                        return name != null && name.StartsWith("Ie",StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                var cmdGroupAddresses = groupAddresses
                    .Where(ga => 
                    {
                        var name = (string?)ga.Attribute("Name");
                        return name != null && name.StartsWith("Cmd",StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();
                
                // Dictionnaire pour regrouper les éléments par nom commun sans préfixe
                var groupedAddresses = new Dictionary<string, List<XElement>>();

                foreach (var ie in ieGroupAddresses)
                {
                    var name = (string)ie.Attribute("Name");
                    if (name != null)
                    {
                        var commonName = name.Substring(2); // Supprimer le préfixe "Ie"
                        if (!groupedAddresses.ContainsKey(commonName))
                        {
                            groupedAddresses[commonName] = new List<XElement>();
                        }
                        groupedAddresses[commonName].Add(ie);
                    }
                }

                foreach (var cmd in cmdGroupAddresses)
                {
                    var name = (string)cmd.Attribute("Name");
                    if (name != null)
                    {
                        var commonName = name.Substring(3); // Supprimer le préfixe "Cmd"
                        if (!groupedAddresses.ContainsKey(commonName))
                        {
                            groupedAddresses[commonName] = new List<XElement>();
                        }
                        groupedAddresses[commonName].Add(cmd);
                    }
                }

            }
        }
        else
        {
            var groupAddressFile = FileLoader.LoadXmlDocument(ProjectFileManager.ZeroXmlPath);
            SetNamespaceFromXml(ProjectFileManager.ZeroXmlPath!);
            var groupAddresses = groupAddressFile?.Descendants(_globalKnxNamespace + "GroupAddress").ToList();

            if (groupAddresses != null)
                {
                    var ieGroupAddresses = groupAddresses
                        .Where(ga =>
                        {
                            var name = (string?)ga.Attribute("Name");
                            return name != null && name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase);
                        })
                        .ToList();

                    var cmdGroupAddresses = groupAddresses
                        .Where(ga =>
                        {
                            var name = (string?)ga.Attribute("Name");
                            return name != null && name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase);
                        })
                        .ToList();

                    // Dictionnaire pour regrouper les éléments par nom commun sans préfixe
                    var groupedAddresses = new Dictionary<string, List<XElement>>();

                    foreach (var ie in ieGroupAddresses)
                    {
                        var name = (string)ie.Attribute("Name");
                        if (name != null)
                        {
                            var commonName = name.Substring(2); // Supprimer le préfixe "Ie"
                            if (!groupedAddresses.ContainsKey(commonName))
                            {
                                groupedAddresses[commonName] = new List<XElement>();
                            }

                            groupedAddresses[commonName].Add(ie);
                        }
                    }

                    foreach (var cmd in cmdGroupAddresses)
                    {
                        var name = (string)cmd.Attribute("Name");
                        if (name != null)
                        {
                            var commonName = name.Substring(3); // Supprimer le préfixe "Cmd"
                            if (!groupedAddresses.ContainsKey(commonName))
                            {
                                groupedAddresses[commonName] = new List<XElement>();
                            }

                            groupedAddresses[commonName].Add(cmd);
                        }
                    }

                }
        }
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