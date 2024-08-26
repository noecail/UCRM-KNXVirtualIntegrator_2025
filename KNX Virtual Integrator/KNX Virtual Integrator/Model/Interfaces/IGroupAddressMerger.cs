using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IGroupAddressMerger
{
    /// <summary>
    /// Merges single-element groups in the grouped addresses dictionary with entries from the provided 
    /// IeAddressesSet if their names have a similarity of 80% or more.
    /// 
    /// This method iterates over groups in the groupedAddresses dictionary that contain a single XElement 
    /// and attempts to find matching entries in the IeAddressesSet based on a similarity threshold of 80%. 
    /// If a similar entry is found, it is added to the corresponding group.
    /// 
    /// <param name="groupedAddresses">The dictionary of grouped addresses that will be modified and potentially merged with elements from IeAddressesSet.</param>
    /// <param name="IeAddressesSet">A list of XElement entries that will be compared against single-element groups in groupedAddresses for potential merging.</param>
    /// <returns>Returns the modified dictionary of grouped addresses with merged entries.</returns>
    /// </summary>
    public Dictionary<string, List<XElement>> MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses, List<XElement> IeAddressesSet);

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
    public List<XElement> GetElementsBySimilarity(string searchString, List<XElement> ieAddressesSet);
}