using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
public class GroupAddressMerger( StringManagement stringManagement, Logger logger) : IGroupAddressMerger
{
    private readonly ILogger _logger = logger;
        
    /// <summary>
    /// Merges single-element groups in the grouped addresses dictionary with entries from the provided 
    /// IeAddressesSet if their names have a similarity of 80% or more.
    /// 
    /// This method iterates over groups in the groupedAddresses dictionary that contain a single XElement 
    /// and attempts to find matching entries in the IeAddressesSet based on a similarity threshold of 80%. 
    /// If a similar entry is found, it is added to the corresponding group.
    /// 
    /// <param name="groupedAddresses">The dictionary of grouped addresses that will be modified and potentially merged with elements from IeAddressesSet.</param>
    /// <param name="ieAddressesSet">A list of XElement entries that will be compared against single-element groups in groupedAddresses for potential merging.</param>
    /// <returns>Returns the modified dictionary of grouped addresses with merged entries.</returns>
    /// </summary>
    public Dictionary<string, List<XElement>> MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses, List<XElement> ieAddressesSet)
    {
        var singleElementGroups = groupedAddresses.Where(g => g.Value.Count == 1).ToList();

        // Parcourir chaque groupe isolé dans groupedAddresses
        foreach (var group in singleElementGroups)
        {
            var groupName = group.Key;

            // Rechercher dans IeAddressesSet les éléments similaires à au moins 80%
            foreach (var ieElement in ieAddressesSet)
            {
                var ieElementName = ieElement.Attribute("Name")?.Value; // Supposant que les éléments dans IeAddressesSet ont un élément "Name"

                if (ieElementName != null && stringManagement.CalculateSimilarity(groupName, ieElementName) >= 0.8)
                {
                    _logger.ConsoleAndLogWriteLine($"Adding '{ieElementName}' from IeAddressesSet to single-element group '{groupName}'.");

                    group.Value.Add(ieElement);
                }
            }
        }

        // Retourner le dictionnaire modifié
        return groupedAddresses;
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