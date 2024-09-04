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
    /// <param name="ieAddressesSet">A list of XElement entries that will be compared against single-element groups in groupedAddresses for potential merging.</param>
    /// <returns>Returns the modified dictionary of grouped addresses with merged entries.</returns>
    /// </summary>
    public Dictionary<string, List<XElement>> MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses, List<XElement> ieAddressesSet);

    /// <summary>
    /// Processes the name attribute of the provided 'cmdElement' XElement to extract a relevant search string, 
    /// and then finds and sorts elements from the 'ieAddressesSet' based on their similarity to this search string.
    /// 
    /// This method performs the following steps:
    /// 1. Extracts the value of the "Name" attribute from the 'cmdElement' XElement and assigns it to 'searchString'.
    /// 2. Removes the prefix "Cmd" from the beginning of 'searchString', if present.
    /// 3. Uses a regular expression to strip off any trailing numeric segments from 'searchString', 
    ///    leaving only the core part of the name for comparison.
    /// 4. Sorts the elements in 'ieAddressesSet' by their similarity to the cleaned 'searchString', 
    ///    in descending order of similarity. The similarity is computed using a custom similarity function.
    /// 
    /// <param name="cmdElement">The XElement representing the command from which the search string is derived.</param>
    /// <param name="ieAddressesSet">The collection of XElement entries to be compared against the search string.</param>
    /// <returns>Returns a list of elements from 'ieAddressesSet', sorted by their similarity to the cleaned 'searchString'.</returns>
    /// </summary>
    public List<XElement> GetElementsBySimilarity(XElement cmdElement, List<XElement> ieAddressesSet);
}