using System.Xml;
using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model;

public class GroupAddressManagement
{
    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

    public static void ExtractGroupAddress()
    {
        // Vérification si l'utilisateur a choisi d'importer un fichier spécifique ou non
        string? filePath = App.WindowManager != null && App.WindowManager.MainWindow.UserChooseToImportGroupAddressFile 
            ? ProjectFileManager.GroupAddressFilePath 
            : ProjectFileManager.ZeroXmlPath;

        if (filePath != null)
        {
            XDocument? groupAddressFile = FileLoader.LoadXmlDocument(filePath);
            if (groupAddressFile != null)
            {
                if (filePath == ProjectFileManager.ZeroXmlPath)
                {
                    SetNamespaceFromXml(filePath);

                    // Extraction des références de dispositifs et leurs liens
                    var deviceRefs = groupAddressFile.Descendants(_globalKnxNamespace + "DeviceInstance").Select(di =>
                        new
                        {
                            Id = di.Attribute("Id")?.Value,
                            Links = di.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
                                .Where(cir => cir.Attribute("Links") != null)
                                .SelectMany(cir => cir.Attribute("Links")?.Value.Split(' ') ?? Array.Empty<string>())
                                .ToHashSet()
                        }).ToList();

                    var groupAddresses = groupAddressFile.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
                    var tempGroupedAddresses = new Dictionary<(string CommonName, string DeviceId), HashSet<string>>();

                    foreach (var ga in groupAddresses)
                    {
                        var id = (string?)ga.Attribute("Id");
                        var name = (string?)ga.Attribute("Name");

                        if (id != null && name != null)
                        {
                            var gaId = id.Contains("GA-") ? id.Substring(id.IndexOf("GA-", StringComparison.Ordinal)) : id;
                            var linkedDevices = deviceRefs.Where(dr => dr.Links.Contains(gaId));

                            foreach (var device in linkedDevices)
                            {
                                var commonName = name;
                                if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
                                    commonName = name.Substring(2);
                                else if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
                                    commonName = name.Substring(3);

                                var key = (CommonName: commonName, DeviceId: device.Id);

                                if (!tempGroupedAddresses.ContainsKey(key))
                                {
                                    tempGroupedAddresses[key] = new HashSet<string>();
                                }

                                tempGroupedAddresses[key].Add(id);
                            }
                        }
                    }

                    var groupedAddresses = new Dictionary<string, List<XElement>>();
                    // Transférer les éléments de tempGroupedAddresses vers groupedAddresses
                    int suffixCounter = 1; // Compteur pour différencier les noms communs identiques

                    foreach (var entry in tempGroupedAddresses)
                    {
                        var commonName = entry.Key.CommonName;
                        var gaIds = entry.Value;

                        // Chercher si tous les gaIds de tempGroupedAddresses sont déjà dans une entrée de groupedAddresses
                        var existingEntry = groupedAddresses.FirstOrDefault(g =>
                            gaIds.All(id => g.Value.Any(x => x.Attribute("Id")?.Value == id)) ||
                            g.Value.Select(x => x.Attribute("Id")?.Value).All(id => gaIds.Contains(id ?? string.Empty)));

                        if (existingEntry.Value != null)
                        {
                            // Un ensemble existant contient tous les gaIds ou est un sous-ensemble, ajouter les nouveaux IDs manquants
                            App.ConsoleAndLogWriteLine($"Matching or subset found for: {existingEntry.Key}. Adding missing IDs.");
                            
                            foreach (var gaId in gaIds)
                            {
                                var ga = groupAddresses.FirstOrDefault(x => x.Attribute("Id")?.Value == gaId);
                                if (ga != null && existingEntry.Value.All(x => x.Attribute("Id")?.Value != gaId))
                                {
                                    existingEntry.Value.Add(ga);
                                }
                            }
                        }
                        else
                        {
                            // Vérifier si un nom commun existe déjà mais avec des adresses différentes
                            while (groupedAddresses.ContainsKey(commonName))
                            {
                                // Si oui, modifier le nom pour éviter la collision
                                commonName = $"{entry.Key.CommonName}_{suffixCounter++}";
                            }

                            // Créer une nouvelle entrée
                            App.ConsoleAndLogWriteLine($"Creating a new entry for: {commonName}");
                            groupedAddresses[commonName] = new List<XElement>();

                            foreach (var gaId in gaIds)
                            {
                                var ga = groupAddresses.FirstOrDefault(x => x.Attribute("Id")?.Value == gaId);
                                if (ga != null)
                                {
                                    groupedAddresses[commonName].Add(ga);
                                }
                            }
                        }
                    }
                }
                else
                {

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