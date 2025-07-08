using System.Text.RegularExpressions;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
public class GroupAddressMerger( StringManagement stringManagement, Logger logger) : IGroupAddressMerger
{
    private readonly ILogger _logger = logger;
        
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
    public Dictionary<string, List<XElement>> MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses, List<XElement> ieAddressesSet)
    {
        // Extraire les groupes de 'groupedAddresses' qui contiennent un seul élément
        var singleElementGroups = groupedAddresses.Where(g => g.Value.Count == 1).ToList();

        // Parcourir chaque groupe isolé dans groupedAddresses
        foreach (var group in singleElementGroups)
        {
            var groupName = group.Key;

            // Pour chaque groupe, parcourir les éléments de 'IeAddressesSet'
            foreach (var ieElement in ieAddressesSet)
            {
                // Supposant que les éléments dans 'IeAddressesSet' ont un attribut "Name", on récupère cette valeur
                var ieElementName = ieElement.Attribute("Name")?.Value;

                // Si le nom de l'élément 'ieElementName' n'est pas nul,
                // et que la similarité entre 'groupName' et 'ieElementName' est supérieure ou égale à 80%.
                if (ieElementName != null && stringManagement.CalculateSimilarity(groupName, ieElementName) >= 0.8)
                {
                    // Log l'ajout de l'élément 'ieElementName' au groupe 'groupName'
                    _logger.ConsoleAndLogWriteLine($"Adding '{ieElementName}' from IeAddressesSet to single-element group '{groupName}'.");

                    // Ajouter l'élément 'ieElement' au groupe actuel
                    group.Value.Add(ieElement);
                }
            }
        }

        // Retourner le dictionnaire modifié avec les groupes potentiellement mis à jour
        return groupedAddresses;
    }
    
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

    public List<XElement> GetElementsBySimilarity(XElement cmdElement, List<XElement> ieAddressesSet)
    {
        // Récupérer la valeur de l'attribut "Name" de l'élément 'cmdElement' et l'assigner à 'searchString'
        var searchStringTemp = cmdElement.Attribute("Name")?.Value;
        var searchString = searchStringTemp ?? "   Adresse vide";
        // Supprimer les premiers 3 caractères de 'searchString', généralement le préfixe "Cmd"
        searchString = searchString.Substring(3);

        // Utiliser une expression régulière pour enlever l'adresse à la fin de 'searchString', laissant le reste de la chaîne intact
        searchString = Regex.Replace(searchString, @"\d+(/\d+)*$", string.Empty);
        
        // Trier les éléments de 'ieAddressesSet' en ordre décroissant de similarité avec 'searchString'
        var sortedElements = ieAddressesSet
            .OrderByDescending(element => 
            {
                // Récupérer la valeur de l'attribut "Name" de l'élément actuel
                var name = element.Attribute("Name")?.Value;

                // Si le nom commence par "Ie", supprimer ce préfixe
                if (name != null && name.StartsWith("Ie"))
                {
                    name = name.Substring(2); // Supprimer les 2 premiers caractères ("Ie")
                }
        
                // Calculer la similarité entre 'searchString' et le nom modifié (sans le préfixe "Ie")
                return stringManagement.CalculateSimilarity(searchString, name ?? string.Empty);
            })
            .ToList(); // Convertir le résultat trié en liste

        // Retourner la liste des éléments triés
        return sortedElements;
    }
        
}