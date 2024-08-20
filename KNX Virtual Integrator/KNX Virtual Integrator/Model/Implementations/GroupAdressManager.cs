using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class GroupAddressManager(Logger logger, ProjectFileManager projectFileManager, FileLoader loader, NamespaceResolver namespaceResolver, GroupAddressProcessor groupAddressProcessor, GroupAddressMerger groupAddressMerger) : IGroupAddressManager
{
    private readonly ILogger _logger = logger;

    public static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";
    private static readonly Dictionary<string, List<XElement>> GroupedAddresses = new ();

    /// <summary>
    /// Extracts group address information from a specified XML file.
    ///
    /// Determines the file path to use based on user input and whether a specific group address
    /// file is chosen or a default file is used. Depending on the file path, it processes the XML
    /// file to extract and group addresses either from a specific format or a standard format.
    /// </summary>
    public void ExtractGroupAddress()
    {
        if (projectFileManager is not { } manager) return;
        
        var filePath = App.WindowManager != null && App.WindowManager.MainWindow.UserChooseToImportGroupAddressFile
            ? manager.GroupAddressFilePath
            : manager.ZeroXmlPath;

        var groupAddressFile = loader.LoadXmlDocument(filePath);
        if (groupAddressFile == null) return;

        if (filePath == manager.ZeroXmlPath)
        {
            namespaceResolver.SetNamespaceFromXml(filePath);
            ProcessZeroXmlFile(groupAddressFile);
        }
        else
        {
            _globalKnxNamespace = "http://knx.org/xml/ga-export/01";
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
    public void ProcessZeroXmlFile(XDocument groupAddressFile)
    {
        GroupedAddresses.Clear();

        var deviceRefs = groupAddressFile.Descendants(_globalKnxNamespace + "DeviceInstance")
            .Select(di => (
                Id: di.Attribute("Id")?.Value,
                Links: di.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
                    .Where(cir => cir.Attribute("Links") != null)
                    .SelectMany(cir => cir.Attribute("Links")?.Value.Split(' ') ?? Array.Empty<string>())
                    .ToHashSet()
            ))
            .ToList();

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

        var suffixCounter = 1;

        foreach (var entry in tempGroupedAddresses)
        {
            var commonName = entry.Key.CommonName;
            var gaIds = entry.Value;

            var existingEntry = GroupedAddresses.FirstOrDefault(g =>
                gaIds.All(id => g.Value.Any(x => x.Attribute("Id")?.Value == id)) ||
                g.Value.Select(x => x.Attribute("Id")?.Value).All(id => gaIds.Contains(id ?? string.Empty)));

            if (existingEntry.Value != null)
            {
                _logger.ConsoleAndLogWriteLine($"Matching or subset found for: {existingEntry.Key}. Adding missing IDs.");

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

                _logger.ConsoleAndLogWriteLine($"Creating a new entry for: {commonName}");
                GroupedAddresses[commonName] = gaIds.Select(id => groupAddresses.First(x => x.Attribute("Id")?.Value == id)).ToList();
            }
        }
        groupAddressMerger.MergeSingleElementGroups(GroupedAddresses);
    }

    /// <summary>
    /// Processes an XML file in the standard format to extract and group addresses.
    ///
    /// This method processes group addresses from the XML file, normalizing the names by removing
    /// specific prefixes ("Ie" or "Cmd") and grouping addresses based on the remaining common names.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in standard format.</param>
    /// </summary>
    public void ProcessStandardXmlFile(XDocument groupAddressFile)
    {
        GroupedAddresses.Clear();
        var groupAddresses = groupAddressFile.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
        
        foreach (var ga in groupAddresses)
        {
            var name = ga.Attribute("Name")?.Value;
            if (name != null)
            {
                if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
                {
                    groupAddressProcessor.AddToGroupedAddresses(GroupedAddresses, ga, name.Substring(2));
                }
                else if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
                {
                    groupAddressProcessor.AddToGroupedAddresses(GroupedAddresses, ga, name.Substring(3));
                }
            }
        }
        groupAddressMerger.MergeSingleElementGroups(GroupedAddresses);
    }
}