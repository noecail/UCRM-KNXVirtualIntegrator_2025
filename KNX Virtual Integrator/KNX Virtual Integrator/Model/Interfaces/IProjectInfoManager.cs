﻿using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;
/// <summary>
/// Interface of the class managing the extraction of physical information of the project : building, rooms,...
/// UNUSED CLASS AT THE TIME OF THE BETA
/// </summary>
public interface IProjectInfoManager
{
    /// <summary>
    /// Extracts location information from a specified XML document.
    ///
    /// This method processes the XML document to extract information about spaces categorized as
    /// "Room" or "Corridor". For each such space, it retrieves various hierarchical details including
    /// the names of the floor, building part, building, and distribution board associated with the space.
    /// The hierarchical relationships are determined by traversing ancestor and descendant nodes in the XML structure.
    ///
    /// The XML structure is expected to use namespaces, which are resolved using `namespaceResolver.GlobalKnxNamespace`.
    /// For each space element, the method extracts the following information:
    /// - RoomName: The name of the room or corridor.
    /// - FloorName: The name of the ancestor element of type "Floor".
    /// - BuildingPartName: The name of the ancestor element of type "BuildingPart".
    /// - BuildingName: The name of the ancestor element of type "Building".
    /// - DistributionBoardName: The name of the descendant element of type "DistributionBoard".
    ///
    /// The extracted information is returned as a list of anonymous objects, each containing the aforementioned details.
    ///
    /// </summary>
    /// <param name="zeroXmlFile">The XML document to extract information from.</param>
    /// <returns>A list of anonymous objects representing the extracted location information, where each object
    /// contains the names of the room, floor, building part, building, and distribution board.</returns>
    public dynamic ExtractLocationInfo(XDocument zeroXmlFile);
}