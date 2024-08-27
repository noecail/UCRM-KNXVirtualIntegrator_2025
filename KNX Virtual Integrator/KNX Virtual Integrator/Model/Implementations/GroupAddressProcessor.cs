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
    
    // Inutile maintenant je pense mais je laisse au cas où
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
    public Dictionary<string, List<XElement>> FilterElements(Dictionary<string, List<XElement>> dictionary)
    {
        var result = new Dictionary<string, List<XElement>>();

        foreach (var kvp in dictionary)
        {
            var elements = kvp.Value;

            // Ne traiter que les listes contenant plus d'un élément
            if (elements.Count > 1)
            {
                // Extraire le premier mot du premier élément
                var separators = new[] { ' ', '_' };
                var firstElementName = elements.First().Attribute("Name")?.Value;
                var firstWord = firstElementName?.Split(separators, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                // Vérifier si tous les éléments commencent par le même premier mot
                bool allStartWithSameWord = elements.All(el =>
                {
                    var nameAttribute = el.Attribute("Name")?.Value;
                    var currentFirstWord = nameAttribute?.Split(separators, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    return currentFirstWord == firstWord;
                });

                // Si tous commencent par le même premier mot, les ajouter au dictionnaire résultant
                if (allStartWithSameWord)
                {
                    result.Add(kvp.Key, elements);
                }
            }
        }
        return result;
    }
    
    /// <summary>
    /// Decodes a string representing a numeric value into a formatted string based on the group address structure.
    /// 
    /// This method takes a string representation of a numeric value and converts it into a formatted string. The format of the output string depends on the specified group address structure:
    /// 
    /// - For a 3-level group address structure, the value is decoded into three components: "champ1/champ2/champ3".
    /// - For a 2-level group address structure, the value is decoded into two components: "champ1/champ2".
    /// 
    /// The decoding is performed as follows:
    /// - For 3-level structure:
    ///   - The third component (`champ3`) is extracted as the least significant 8 bits.
    ///   - The second component (`champ2`) is extracted as the next 3 bits.
    ///   - The first component (`champ1`) is extracted as the most significant 5 bits.
    /// - For 2-level structure:
    ///   - The second component (`champ2`) is extracted as the least significant 11 bits.
    ///   - The first component (`champ1`) is extracted as the next 5 bits.
    /// 
    /// If the input string cannot be converted to an integer, or if the group address structure is not recognized, the method logs an error message and returns the original input string.
    /// 
    /// <param name="valueString">The string representation of the numeric value to decode.</param>
    /// <param name="groupAddressStructure">An integer indicating the group address structure: 2 for 2-level and 3 for 3-level.</param>
    /// <returns>A formatted string representing the decoded value based on the group address structure. Returns the original string if conversion fails or if the structure is unrecognized.</returns>
    /// </summary>
    public string DecodeValueToString(string valueString, int groupAddressStructure)
    {
        // Convertir la chaîne en entier
        if (!int.TryParse(valueString, out int value))
        {
            logger.ConsoleAndLogWriteLine($"Impossible to convert {valueString} in integer");
            return valueString;
        }

        if (groupAddressStructure == 3)
        {
            // Extraire le troisième champ (8 bits)
            int champ3 = value & 0xFF;

            // Extraire le deuxième champ (3 bits)
            int champ2 = (value >> 8) & 0x7;

            // Extraire le premier champ (5 bits)
            int champ1 = (value >> 11) & 0x1F;

            // Construire la chaîne de caractères au format "champ1/champ2/champ3"
            return $"{champ1}/{champ2}/{champ3}";
        }
        else if (groupAddressStructure == 2)
        {
            // Extraire le deuxième champ (11 bits)
            int champ2 = value & 0x7FF; // 0x7FF est 2047 en hexadécimal

            // Extraire le premier champ (5 bits)
            int champ1 = (value >> 11) & 0x1F; // 0x1F est 31 en hexadécimal

            // Construire la chaîne de caractères au format "champ1/champ2"
            return $"{champ1}/{champ2}";
        }
        
        logger.ConsoleAndLogWriteLine($"Impossible conversion for {valueString}");
        return valueString;

    }
    
    
}

