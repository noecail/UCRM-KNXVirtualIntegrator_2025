using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class ProjectInfoManager(NamespaceResolver namespaceResolver) : IProjectInfoManager
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
    public dynamic ExtractLocationInfo(XDocument zeroXmlFile)
    {
        var locationInfo = zeroXmlFile.Descendants(namespaceResolver.GlobalKnxNamespace! + "Space")
            .Where(s => 
            {
                var type = s.Attribute("Type")?.Value;
                return type == "Room" || type == "Corridor";
            })
            .Select(room =>
            {
                var getAncestorName = new Func<string, string>(type =>
                    room.Ancestors(namespaceResolver.GlobalKnxNamespace! + "Space")
                        .FirstOrDefault(s => s.Attribute("Type")?.Value == type)
                        ?.Attribute("Name")?.Value ?? string.Empty
                );

                var getDescendantName = new Func<string, string>(type =>
                    room.Descendants(namespaceResolver.GlobalKnxNamespace! + "Space")
                        .FirstOrDefault(s => s.Attribute("Type")?.Value == type)
                        ?.Attribute("Name")?.Value ?? string.Empty
                );
                    
                return new
                {
                    RoomName = room.Attribute("Name")?.Value,
                    FloorName = getAncestorName("Floor"),
                    BuildingPartName = getAncestorName("BuildingPart"),
                    BuildingName = getAncestorName("Building"),
                    DistributionBoardName = getDescendantName("DistributionBoard"),
                };
            })
            .ToList();

        return locationInfo;
    }
}