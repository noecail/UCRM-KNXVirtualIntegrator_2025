using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IGroupAddressMerger
{
    /// <summary>
    /// Merges single-element groups in the grouped addresses dictionary with other groups if their names
    /// match with a similarity of 80% or more.
    ///
    /// This method compares the names of groups with a single element to other groups and merges them
    /// if they are similar enough, based on a similarity threshold of 80%.
    ///
    /// <param name="groupedAddresses">The dictionary of grouped addresses to be merged.</param>
    /// </summary>
    public void MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses);

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