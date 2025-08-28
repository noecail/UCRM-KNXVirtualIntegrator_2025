using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Class handling the comparison of strings
/// </summary>
/// <param name="groupAddressProcessor">To process the strings before comparison</param>
public class StringManagement(GroupAddressProcessor groupAddressProcessor) : IStringManagement
{
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
    public double CalculateSimilarity(string str1, string str2)
    {
        // Récupération de la longueur des deux chaînes.
        var len1 = str1.Length;
        var len2 = str2.Length;
    
        // Calcul de la longueur maximale entre les deux chaînes.
        var maxLen = Math.Max(len1, len2);

        // Si les deux chaînes sont vides, elles sont considérées comme identiques (similarité maximale de 1).
        if (maxLen == 0) return 1.0;

        // Calcul de la distance de Levenshtein entre les deux chaînes.
        var distance = LevenshteinDistance(str1, str2);

        // Retourner le ratio de similarité : 1 - (distance / longueur maximale).
        // Plus la distance est petite, plus les chaînes sont similaires.
        return 1.0 - (double)distance / maxLen;
    }

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
    public int LevenshteinDistance(string str1, string str2)
    {
        // Longueur de la première chaîne.
        var n = str1.Length;
        // Longueur de la deuxième chaîne.
        var m = str2.Length;
        // Matrice pour stocker les distances entre les sous-chaînes de str1 et str2.
        var d = new int[n + 1, m + 1];

        // Si la première chaîne est vide, la distance est la longueur de la deuxième chaîne (toutes les insertions).
        if (n == 0) return m;
        // Si la deuxième chaîne est vide, la distance est la longueur de la première chaîne (toutes les suppressions).
        if (m == 0) return n;

        // Initialisation de la première colonne (suppression de tous les caractères de str1).
        for (var i = 0; i <= n; i++) d[i, 0] = i;
    
        // Initialisation de la première ligne (insertion de tous les caractères de str2).
        for (var j = 0; j <= m; j++) d[0, j] = j;

        // Parcourir chaque caractère des deux chaînes pour calculer les distances.
        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                // Si les caractères sont identiques, le coût est 0, sinon 1.
                var cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

                // Calcul de la distance minimale en tenant compte des trois opérations possibles :
                // 1. Suppression (d[i-1, j] + 1)
                // 2. Insertion (d[i, j-1] + 1)
                // 3. Substitution (d[i-1, j-1] + cost)
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        // Retourner la distance de Levenshtein, c'est-à-dire le nombre minimal de modifications pour transformer str1 en str2.
        return d[n, m];
    }
    
    /// <summary>
    /// Compare two names based on the similarity of their first three words
    /// and exact match of the remaining words.
    /// </summary>
    /// <param name="name1">The first name to compare.</param>
    /// <param name="name2">The second name to compare.</param>
    /// <returns>True if the names are similar based on the criteria; otherwise, false.</returns>
    public bool AreNamesSimilar(string name1, string name2)
    {
        var words1 = groupAddressProcessor.NormalizeName(name1).Split(['_', ' '], StringSplitOptions.RemoveEmptyEntries);
        var words2 = groupAddressProcessor.NormalizeName(name2).Split(['_', ' '], StringSplitOptions.RemoveEmptyEntries);

        if (words1.Length < 3 || words2.Length < 3)
            return false; // Ensure we have at least three words to compare

        // Compare the first three words with 80% similarity
        var prefix1 = string.Join(" ", words1.Take(3));
        var prefix2 = string.Join(" ", words2.Take(3));

        if (CalculateSimilarity(prefix1, prefix2) < 0.8) 
            return false;

        // Ensure remaining words match exactly
        var remainingWords1 = words1.Skip(3);
        var remainingWords2 = words2.Skip(3);

        return remainingWords1.SequenceEqual(remainingWords2);
    }
}