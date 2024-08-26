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
    /// Retrieves a list of XElement objects from a list, sorted by their similarity to a given search string,
    /// ignoring specific prefixe ("Ie") in the names of the elements.
    /// 
    /// This method processes a list of XElement objects, removes the prefixe "Ie" from the element names,
    /// and then sorts the elements based on their similarity to the provided search string. The similarity is calculated
    /// using the modified names.
    /// 
    /// <param name="searchString">The string to compare against the element names after removing prefixes.</param>
    /// <param name="ieAddressesSet">A list of XElement objects to be filtered and sorted.</param>
    /// <returns>A sorted list of XElement objects based on their similarity to the search string.</returns>
    /// </summary>
    public List<XElement> GetElementsBySimilarity(string searchString, List<XElement> ieAddressesSet)
    {
        var sortedElements = ieAddressesSet
            .OrderByDescending(element => 
            {
                var name = element.Attribute("Name")?.Value;

                // Remove the prefixe "Ie" if it exist
                if (name != null && name.StartsWith("Ie"))
                {
                    name = name.Substring(2); // Remove the first 2 characters
                }
                
                // Calculate the similarity based on the modified name
                return stringManagement.CalculateSimilarity(searchString, name ?? string.Empty);
            })
            .ToList();

        return sortedElements;
    }
        
}