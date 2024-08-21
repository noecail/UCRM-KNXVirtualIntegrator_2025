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
}