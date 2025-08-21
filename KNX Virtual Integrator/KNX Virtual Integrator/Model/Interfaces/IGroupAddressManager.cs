using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Defines methods for managing group addresses extracted from XML files.
/// </summary>
public interface IGroupAddressManager
{
    public string[] Prefixes { get; set; } 
    
    /// <summary>
    /// Extracts group address information from a specified XML file.
    ///
    /// Determines the file path to use based on user input and whether a specific group address
    /// file is chosen or a default file is used. Processes the XML file to extract and group addresses
    /// from either a specific format or a standard format.
    /// </summary>
    XDocument? ExtractGroupAddress(IFunctionalModelList functionalModelList);

    /// <summary>
    /// Processes an XML file in the Zero format to extract and group addresses.
    ///
    /// This method extracts device references and their links, processes group addresses, and 
    /// groups them based on device links and common names. Handles the creation and updating 
    /// of grouped addresses, avoiding name collisions by appending suffixes if necessary.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in Zero format.</param>
    /// </summary>
    void ProcessZeroXmlFile(XDocument groupAddressFile, IFunctionalModelList functionalModelList);

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
    /// Processes a list of XElement obtained from a XDocument to get the group addresses and fill the functional models
    /// </summary>
    /// <param name="modelStructures"> The list containing all the Xelements with the gorup addresses. </param>
    /// <param name="functionalModelList"> The list where to put all the recognized models. </param>
    void NewProcessStandardXmlFile(IEnumerable<XElement>? modelStructures, IFunctionalModelList functionalModelList);


    /// <summary>
    /// Determines the level structure of group addresses in an 0 XML document to check for overlaps.
    /// 
    /// This method examines an XML document containing group address ranges and specific group addresses.
    /// It helps in identifying whether the group addresses are organized into 2 levels or 3 levels by detecting if there are any overlapping addresses.
    /// 
    /// If the addresses are detected to overlap, the method returns the value 3.
    /// If no overlaps are found, the method returns the value 2.
    /// 
    /// <param name="doc">The XML document (XDocument) containing the group address ranges and specific group addresses.</param>
    /// <returns>An integer indicating the overlap status: 3 for detected overlap, 2 for no overlap.</returns>
    /// </summary>
    public int DetermineGroupAddressStructure0Xml(XDocument doc);
    
    /// <summary>
    /// Determines the level structure of group addresses in an XML document to check for overlaps.
    /// 
    /// This method examines an XML document containing group address ranges and specific group addresses.
    /// It helps in identifying whether the group addresses are organized into 2 levels or 3 levels by detecting if there are any overlapping addresses.
    /// 
    /// If the addresses are detected to overlap, the method returns the value 3.
    /// If no overlaps are found, the method returns the value 2.
    /// 
    /// <param name="doc">The XML document (XDocument) containing the group address ranges and specific group addresses.</param>
    /// <returns>An integer indicating the overlap status: 3 for detected overlap, 2 for no overlap.</returns>
    /// </summary>
    public int DetermineGroupAddressStructureGroupAddressFile(IEnumerable<XElement>? modelStructures);
    
}