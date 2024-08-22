using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IGroupAddressProcessor
{
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
    public void AddToGroupedAddresses(Dictionary<string, List<XElement>> groupedAddresses, XElement ga,
        string commonName);

    /// <summary>
    /// Normalizes the name by removing specific prefixes.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    public string NormalizeName(string name);

    /// <summary>
    /// Filters a dictionary of XElement lists, retaining only those lists where all elements
    /// share the same first word in their "Name" attribute.
    /// 
    /// This method processes each list in the dictionary that contains more than one XElement. 
    /// It checks if all elements in the list start with the same word (separated by spaces or underscores) 
    /// in their "Name" attribute. If they do, the list is added to the resulting dictionary.
    /// 
    /// <param name="dictionary">A dictionary where the key is a string and the value is a list of XElement objects.</param>
    /// <returns>A dictionary containing only the lists of XElement objects where all elements have the same first word in their "Name" attribute.</returns>
    /// </summary>
    public Dictionary<string, List<XElement>> FilterElements(Dictionary<string, List<XElement>> dictionary);

}
