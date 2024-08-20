using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

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
        var len1 = str1.Length;
        var len2 = str2.Length;
        var maxLen = Math.Max(len1, len2);

        if (maxLen == 0) return 1.0; // Both strings are empty

        var distance = LevenshteinDistance(str1, str2);
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
        var n = str1.Length;
        var m = str2.Length;
        var d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

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
        var words1 = groupAddressProcessor.NormalizeName(name1).Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var words2 = groupAddressProcessor.NormalizeName(name2).Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);

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