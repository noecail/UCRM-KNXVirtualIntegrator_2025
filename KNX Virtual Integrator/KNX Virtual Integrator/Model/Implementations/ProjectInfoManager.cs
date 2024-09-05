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
        // Extraction des informations de localisation à partir du fichier XML.
        var locationInfo = zeroXmlFile.Descendants(namespaceResolver.GlobalKnxNamespace! + "Space")
            // Filtrer les éléments ayant le type "Room" (pièce) ou "Corridor" (couloir).
            .Where(s => 
            {
                var type = s.Attribute("Type")?.Value;
                return type == "Room" || type == "Corridor";
            })
            .Select(room =>
            {
                // Fonction pour récupérer le nom de l'ancêtre d'un certain type (par exemple, "Floor", "Building").
                var getAncestorName = new Func<string, string>(type =>
                        room.Ancestors(namespaceResolver.GlobalKnxNamespace! + "Space")
                            // Chercher le premier ancêtre du type spécifié et récupérer l'attribut "Name".
                            .FirstOrDefault(s => s.Attribute("Type")?.Value == type)
                            ?.Attribute("Name")?.Value ?? string.Empty // Retourner une chaîne vide si aucun ancêtre trouvé.
                );

                // Fonction pour récupérer le nom du descendant d'un certain type (par exemple, "DistributionBoard").
                var getDescendantName = new Func<string, string>(type =>
                        room.Descendants(namespaceResolver.GlobalKnxNamespace! + "Space")
                            // Chercher le premier descendant du type spécifié et récupérer l'attribut "Name".
                            .FirstOrDefault(s => s.Attribute("Type")?.Value == type)
                            ?.Attribute("Name")?.Value ?? string.Empty // Retourner une chaîne vide si aucun descendant trouvé.
                );

                // Retourner un objet anonyme avec les différentes informations de localisation.
                return new
                {
                    RoomName = room.Attribute("Name")?.Value, // Nom de la pièce ou du couloir.
                    FloorName = getAncestorName("Floor"), // Nom de l'étage.
                    BuildingPartName = getAncestorName("BuildingPart"), // Partie du bâtiment.
                    BuildingName = getAncestorName("Building"), // Nom du bâtiment.
                    DistributionBoardName = getDescendantName("DistributionBoard"), // Nom du tableau de distribution.
                };
            })
            .ToList(); // Convertir le résultat en liste.

        // Retourner les informations de localisation extraites.
        return locationInfo;
    }

}