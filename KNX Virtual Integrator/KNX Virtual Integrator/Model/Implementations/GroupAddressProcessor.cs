using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations ;

public class GroupAddressProcessor(Logger logger) : IGroupAddressProcessor
{
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
    /// Adds a group address to the grouped addresses dictionary with a normalized common name.
    ///
    /// This method ensures that the group address is added to the list associated with the specified
    /// common name. If the common name does not already exist in the dictionary, it is created.
    ///
    /// <param name="groupedAddresses">The dictionary of grouped addresses where the group address will be added.</param>
    /// <param name="ga">The group address element to be added.</param>
    /// <param name="commonName">The common name used for grouping the address.</param>
    /// </summary>
    public void AddToGroupedAddresses(
        Dictionary<string, List<XElement>> groupedAddresses,
        XElement ga,
        string commonName)
    {
        if (!groupedAddresses.ContainsKey(commonName))
        { 
            groupedAddresses[commonName] = new List<XElement>();
        } 
        groupedAddresses[commonName].Add(ga);
    }
    
}

