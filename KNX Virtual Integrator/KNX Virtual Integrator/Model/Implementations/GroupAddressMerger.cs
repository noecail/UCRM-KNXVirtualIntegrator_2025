using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
public class GroupAddressMerger(GroupAddressProcessor groupAddressProcessor, StringManagement stringManagement, Logger logger) : IGroupAddressMerger
{
    private readonly ILogger _logger = logger;
        
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
        
        for (var i = 0; i < singleElementGroups.Count; i++)
        {
            var group1 = singleElementGroups[i];
            var name1 = group1.Key;

            for (var j = i + 1; j < singleElementGroups.Count; j++)
            {
                var group2 = singleElementGroups[j];
                var name2 = group2.Key;

                if (stringManagement.AreNamesSimilar(name1, name2))
                {
                    _logger.ConsoleAndLogWriteLine($"Merging single-element groups '{name1}' and '{name2}'.");

                    group1.Value.Add(group2.Value.First());
                    groupedAddresses.Remove(name2);
                    mergedGroups.Add(name1);
                    mergedGroups.Add(name2);
                    break;
                }
            }
        }

        foreach (var singleGroup in singleElementGroups)
        {
            var singleName = singleGroup.Key;

            if (mergedGroups.Contains(singleName)) continue;

            foreach (var otherGroup in groupedAddresses.ToList())
            {
                if (singleGroup.Key == otherGroup.Key || otherGroup.Value.Count == 1) continue;

                var otherName = otherGroup.Key;
                if (stringManagement.AreNamesSimilar(singleName, otherName))
                {
                    _logger.ConsoleAndLogWriteLine($"Merging single-element group '{singleName}' with group '{otherName}'.");

                    otherGroup.Value.Add(singleGroup.Value.First());
                    groupedAddresses.Remove(singleName);

                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Retrieves a list of XElement objects from a dictionary, sorted by their similarity to a given search string,
    /// ignoring specific prefixes ("Ie" and "Cmd") in the names of the elements.
    /// 
    /// This method filters the dictionary to only include entries with a single XElement, removes the prefixes 
    /// "Ie" and "Cmd" from the element names, and then sorts the resulting elements by their similarity 
    /// to the provided search string. The similarity is calculated using the modified names.
    /// 
    /// <param name="searchString">The string to compare against the element names after removing prefixes.</param>
    /// <param name="dictionary">A dictionary where the key is a string and the value is a list of XElement objects.</param>
    /// <returns>A sorted list of XElement objects based on their similarity to the search string.</returns>
    /// </summary>
    public List<XElement> GetElementsBySimilarity(string searchString, Dictionary<string, List<XElement>> dictionary)
    {
        // Step 1: Filter the dictionary to get entries with only one XElement
        var filteredElements = dictionary
            .Where(kv => kv.Value.Count == 1)
            .Select(kv => kv.Value.First()) // Since we only have one element, take the first (and only) one
            .ToList();

        // Step 2: Sort the elements by similarity to the search string
        var sortedElements = filteredElements
            .OrderByDescending(element => 
            {
                var name = element.Attribute("Name").Value;
            
                // Remove the prefixes "Ie" and "Cmd" if they exist
                if (name.StartsWith("Ie"))
                {
                    name = name.Substring(2); // Remove the first 2 characters
                }
                else if (name.StartsWith("Cmd"))
                {
                    name = name.Substring(3); // Remove the first 3 characters
                }
            
                // Calculate the similarity based on the modified name
                return stringManagement.CalculateSimilarity(searchString, name);
            })
            .ToList();

        return sortedElements;
    }
        
}