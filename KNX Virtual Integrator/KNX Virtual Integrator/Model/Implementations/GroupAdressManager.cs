using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class GroupAddressManager(Logger logger, ProjectFileManager projectFileManager) : IGroupAddressManager
{
    private readonly ILogger _logger = logger;

    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";
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

        var groupAddressFile = App.ModelManager?.LoadXmlDocument(filePath);
        if (groupAddressFile == null) return;

        if (filePath == manager.ZeroXmlPath)
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
    public void ProcessZeroXmlFile(XDocument groupAddressFile)
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
        MergeSingleElementGroups(GroupedAddresses);
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

        MergeSingleElementGroups(GroupedAddresses);
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
    public void AddToGroupedAddresses(Dictionary<string, List<XElement>> groupedAddresses, XElement ga, string commonName)
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
    public void SetNamespaceFromXml(string zeroXmlFilePath)
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
            _logger.ConsoleAndLogWriteLine($"Error loading XML file (XML exception): {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.ConsoleAndLogWriteLine($"An unexpected error occurred during SetNamespaceFromXml(): {ex.Message}");
        }
    }
    
    /// <summary>
    /// Merges single-element groups in the grouped addresses dictionary with other groups if their names
    /// match with a similarity of 80% or more.
    ///
    /// This method compares the names of groups with a single element to other groups and merges them
    /// if they are similar enough, based on a similarity threshold of 80%.
    ///
    /// <param name="groupedAddresses">The dictionary of grouped addresses to be merged.</param>
    /// </summary>
    public void MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses)
    {
        var singleElementGroups = groupedAddresses.Where(g => g.Value.Count == 1).ToList();
        var mergedGroups = new HashSet<string>();

        // Fusionner les groupes d'un seul élément entre eux
        for (var i = 0; i < singleElementGroups.Count; i++)
        {
            var group1 = singleElementGroups[i];
            var name1 = group1.Key;

            for (var j = i + 1; j < singleElementGroups.Count; j++)
            {
                var group2 = singleElementGroups[j];
                var name2 = group2.Key;

                if (AreNamesSimilar(name1, name2))
                {
                    _logger.ConsoleAndLogWriteLine($"Merging single-element groups '{name1}' and '{name2}'.");

                    // Fusionner les éléments
                    group1.Value.Add(group2.Value.First());
                    groupedAddresses.Remove(name2);
                    mergedGroups.Add(name1);
                    mergedGroups.Add(name2);
                    break;
                }
            }
        }

        // Fusionner les groupes restants d'un seul élément avec les groupes déjà existants
        foreach (var singleGroup in singleElementGroups)
        {
            var singleName = singleGroup.Key;

            if (mergedGroups.Contains(singleName)) continue;

            foreach (var otherGroup in groupedAddresses.ToList())
            {
                if (singleGroup.Key == otherGroup.Key || otherGroup.Value.Count == 1) continue;

                var otherName = otherGroup.Key;
                if (AreNamesSimilar(singleName, otherName))
                {
                    _logger.ConsoleAndLogWriteLine($"Merging single-element group '{singleName}' with group '{otherName}'.");

                    // Ajouter l'élément unique au groupe existant
                    otherGroup.Value.Add(singleGroup.Value.First());

                    // Supprimer le groupe à un seul élément
                    groupedAddresses.Remove(singleName);

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Compare two names based on the similarity of their first three words
    /// and exact match of the remaining words.
    /// </summary>
    /// <param name="name1">The first name to compare.</param>
    /// <param name="name2">The second name to compare.</param>
    /// <returns>True if the names are similar based on the criteria; otherwise, false.</returns>
    public bool AreNamesSimilar(string name1, string name2)
    {
        var words1 = NormalizeName(name1).Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var words2 = NormalizeName(name2).Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (words1.Length < 3 || words2.Length < 3)
            return false; // Ensure we have at least three words to compare

        // Compare the first three words with 80% similarity
        var prefix1 = string.Join(" ", words1.Take(3));
        var prefix2 = string.Join(" ", words2.Take(3));

        if (CalculateSimilarity(prefix1, prefix2) < 0.8)
            return false;

        // Ensure remaining words match exactly
        var remainingWords1 = words1.Skip(3);
        var remainingWords2 = words2.Skip(3);

        return remainingWords1.SequenceEqual(remainingWords2);
    }

    /// <summary>
    /// Normalizes the name by removing specific prefixes.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    public string NormalizeName(string name)
    {
        if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
            return name.Substring(2);
        if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
            return name.Substring(3);
        return name;
    }

    /// <summary>
    /// Calculates the similarity between two strings using a similarity ratio.
    ///
    /// This method calculates the similarity ratio between two strings. The similarity ratio is
    /// a measure of how closely the two strings match, ranging from 0 to 1. A ratio of 1 means
    /// the strings are identical, while a ratio of 0 means they have no similarity.
    ///
    /// <param name="str1">The first string to compare.</param>
    /// <param name="str2">The second string to compare.</param>
    /// <returns>A similarity ratio between 0 and 1.</returns>
    /// </summary>
    public double CalculateSimilarity(string str1, string str2)
    {
        var len1 = str1.Length;
        var len2 = str2.Length;
        var maxLen = Math.Max(len1, len2);

        if (maxLen == 0) return 1.0; // Both strings are empty

        var distance = LevenshteinDistance(str1, str2);
        return 1.0 - (double)distance / maxLen;
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    ///
    /// The Levenshtein distance is a measure of the difference between two sequences. It is defined
    /// as the minimum number of single-character edits (insertions, deletions, or substitutions)
    /// required to change one string into the other.
    ///
    /// <param name="str1">The first string.</param>
    /// <param name="str2">The second string.</param>
    /// <returns>The Levenshtein distance between the two strings.</returns>
    /// </summary>
    public int LevenshteinDistance(string str1, string str2)
    {
        var n = str1.Length;
        var m = str2.Length;
        var d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        return d[n, m];
    }

    /// <summary>
    /// Normalizes the name by removing specific prefixes.
    ///
    /// This method removes "Ie" or "Cmd" prefixes from the name if present, and returns the
    /// normalized name. If neither prefix is present, the name is returned as-is.
    ///
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    /// </summary>
    /*private static string NormalizeName(string name)
    {
        if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
            return name.Substring(2);
        if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
            return name.Substring(3);
        return name;
    }*/
}