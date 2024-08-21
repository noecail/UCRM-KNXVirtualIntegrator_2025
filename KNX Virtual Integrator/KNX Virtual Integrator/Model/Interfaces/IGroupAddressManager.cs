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
    
}