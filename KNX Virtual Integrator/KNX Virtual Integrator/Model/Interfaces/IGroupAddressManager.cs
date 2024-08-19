using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Defines methods for managing group addresses extracted from XML files.
/// </summary>
public interface IGroupAddressManager
{
    /// <summary>
    /// Extracts group address information from a specified XML file.
    ///
    /// Determines the file path to use based on user input and whether a specific group address
    /// file is chosen or a default file is used. Processes the XML file to extract and group addresses
    /// from either a specific format or a standard format.
    /// </summary>
    void ExtractGroupAddress();

    /// <summary>
    /// Processes an XML file in the Zero format to extract and group addresses.
    ///
    /// This method extracts device references and their links, processes group addresses, and 
    /// groups them based on device links and common names. Handles the creation and updating 
    /// of grouped addresses, avoiding name collisions by appending suffixes if necessary.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in Zero format.</param>
    /// </summary>
    void ProcessZeroXmlFile(XDocument groupAddressFile);

    /// <summary>
    /// Processes an XML file in the standard format to extract and group addresses.
    ///
    /// This method processes group addresses from the XML file, normalizing the names by removing
    /// specific prefixes ("Ie" or "Cmd") and grouping addresses based on the remaining common names.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in standard format.</param>
    /// </summary>
    void ProcessStandardXmlFile(XDocument groupAddressFile);

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
    void AddToGroupedAddresses(Dictionary<string, List<XElement>> groupedAddresses, XElement ga, string commonName);

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
    void SetNamespaceFromXml(string zeroXmlFilePath);

    /// <summary>
    /// Merges single-element groups in the grouped addresses dictionary with other groups if their names
    /// match with a similarity of 80% or more.
    ///
    /// This method compares the names of groups with a single element to other groups and merges them
    /// if they are similar enough, based on a similarity threshold of 80%.
    ///
    /// <param name="groupedAddresses">The dictionary of grouped addresses to be merged.</param>
    /// </summary>
    void MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses);

    /// <summary>
    /// Compares two names based on the similarity of their first three words
    /// and exact match of the remaining words.
    ///
    /// <param name="name1">The first name to compare.</param>
    /// <param name="name2">The second name to compare.</param>
    /// <returns>True if the names are similar based on the criteria; otherwise, false.</returns>
    /// </summary>
    bool AreNamesSimilar(string name1, string name2);

    /// <summary>
    /// Normalizes the name by removing specific prefixes.
    ///
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    /// </summary>
    string NormalizeName(string name);

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
    double CalculateSimilarity(string str1, string str2);

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
    int LevenshteinDistance(string str1, string str2);
}