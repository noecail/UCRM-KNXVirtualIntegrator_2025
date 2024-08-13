using System.Xml;
using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model;

public class GroupAddressManagement
{
    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";
    private static readonly Dictionary<string, List<XElement>> GroupedAddresses = new ();

    /// <summary>
    /// Extracts group address information from a specified XML file.
    ///
    /// Determines the file path to use based on user input and whether a specific group address
    /// file is chosen or a default file is used. Depending on the file path, it processes the XML
    /// file to extract and group addresses either from a specific format or a standard format.
    /// </summary>
    public static void ExtractGroupAddress()
    {
        string filePath = App.WindowManager != null && App.WindowManager.MainWindow.UserChooseToImportGroupAddressFile
            ? ProjectFileManager.GroupAddressFilePath
            : ProjectFileManager.ZeroXmlPath;

        XDocument? groupAddressFile = FileLoader.LoadXmlDocument(filePath);
        if (groupAddressFile == null) return;

        if (filePath == ProjectFileManager.ZeroXmlPath)
        {
            SetNamespaceFromXml(filePath);
            ProcessZeroXmlFile(groupAddressFile);
        }
        else
        {
            ProcessStandardXmlFile(groupAddressFile);
        }
    }

    /// <summary>
    /// Processes an XML file in the Zero format to extract and group addresses.
    ///
    /// This method extracts device references and their links, processes group addresses, and 
    /// groups them based on device links and common names. It handles the creation and updating 
    /// of grouped addresses, avoiding name collisions by appending suffixes if necessary.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in Zero format.</param>
    /// </summary>
    private static void ProcessZeroXmlFile(XDocument groupAddressFile)
    {
        var deviceRefs = groupAddressFile.Descendants(_globalKnxNamespace + "DeviceInstance")
            .Select(di => new
            {
                Id = di.Attribute("Id")?.Value,
                Links = di.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
                          .Where(cir => cir.Attribute("Links") != null)
                          .SelectMany(cir => cir.Attribute("Links")?.Value.Split(' ') ?? [])
                          .ToHashSet()
            }).ToList();

        var groupAddresses = groupAddressFile.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
        var tempGroupedAddresses = new Dictionary<(string CommonName, string DeviceId), HashSet<string>>();

        foreach (var ga in groupAddresses)
        {
            var id = ga.Attribute("Id")?.Value;
            var name = ga.Attribute("Name")?.Value;

            if (id == null || name == null) continue;

            var gaId = id.Contains("GA-") ? id.Substring(id.IndexOf("GA-", StringComparison.Ordinal)) : id;
            var linkedDevices = deviceRefs.Where(dr => dr.Links.Contains(gaId));

            foreach (var device in linkedDevices)
            {
                var commonName = name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase)
                    ? name.Substring(2)
                    : name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase)
                        ? name.Substring(3)
                        : name;

                var key = (CommonName: commonName, DeviceId: device.Id);

                if (!tempGroupedAddresses.ContainsKey(key))
                {
                    tempGroupedAddresses[key] = new HashSet<string>();
                }

                tempGroupedAddresses[key].Add(id);
            }
        }

        int suffixCounter = 1;

        foreach (var entry in tempGroupedAddresses)
        {
            var commonName = entry.Key.CommonName;
            var gaIds = entry.Value;

            var existingEntry = GroupedAddresses.FirstOrDefault(g =>
                gaIds.All(id => g.Value.Any(x => x.Attribute("Id")?.Value == id)) ||
                g.Value.Select(x => x.Attribute("Id")?.Value).All(id => gaIds.Contains(id ?? string.Empty)));

            if (existingEntry.Value != null)
            {
                App.ConsoleAndLogWriteLine($"Matching or subset found for: {existingEntry.Key}. Adding missing IDs.");

                foreach (var gaId in gaIds)
                {
                    if (existingEntry.Value.All(x => x.Attribute("Id")?.Value != gaId))
                    {
                        var ga = groupAddresses.FirstOrDefault(x => x.Attribute("Id")?.Value == gaId);
                        if (ga != null) existingEntry.Value.Add(ga);
                    }
                }
            }
            else
            {
                while (GroupedAddresses.ContainsKey(commonName))
                {
                    commonName = $"{entry.Key.CommonName}_{suffixCounter++}";
                }

                App.ConsoleAndLogWriteLine($"Creating a new entry for: {commonName}");
                GroupedAddresses[commonName] = gaIds.Select(id => groupAddresses.First(x => x.Attribute("Id")?.Value == id)).ToList();
            }
        }
    }

    /// <summary>
    /// Processes an XML file in the standard format to extract and group addresses.
    ///
    /// This method processes group addresses from the XML file, normalizing the names by removing
    /// specific prefixes ("Ie" or "Cmd") and grouping addresses based on the remaining common names.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in standard format.</param>
    /// </summary>
    private static void ProcessStandardXmlFile(XDocument groupAddressFile)
    {
        var groupAddresses = groupAddressFile.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
        
        foreach (var ga in groupAddresses)
        {
            var name = ga.Attribute("Name")?.Value;
            if (name != null)
            {
                if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
                {
                    AddToGroupedAddresses(GroupedAddresses, ga, name.Substring(2));
                }
                else if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
                {
                    AddToGroupedAddresses(GroupedAddresses, ga, name.Substring(3));
                }
            }
        }

        // Log and use `groupedAddresses` as needed
    }

    /// <summary>
    /// Adds a group address to the grouped addresses dictionary with a normalized common name.
    ///
    /// This method ensures that the group address is added to the list associated with the specified
    /// common name. If the common name does not already exist in the dictionary, it is created.
    ///
    /// <param name="groupedAddresses">The dictionary of grouped addresses where the group address will be added.</param>
    /// <param name="ga">The group address element to be added.</param>
    /// <param name="commonName">The common name used for grouping the address.</param>
    /// </summary>
    private static void AddToGroupedAddresses(Dictionary<string, List<XElement>> groupedAddresses, XElement ga, string commonName)
    {
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