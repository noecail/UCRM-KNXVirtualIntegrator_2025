namespace KNX_Virtual_Integrator.Model.Interfaces;
/// <summary>
/// Interface of the class handling the comparison of strings
/// </summary>
public interface IStringManagement
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
    public double CalculateSimilarity(string str1, string str2);
    
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
    public int LevenshteinDistance(string str1, string str2);
    
    /// <summary>
    /// Compare two names based on the similarity of their first three words
    /// and exact match of the remaining words.
    /// </summary>
    /// <param name="name1">The first name to compare.</param>
    /// <param name="name2">The second name to compare.</param>
    /// <returns>True if the names are similar based on the criteria; otherwise, false.</returns>
    public bool AreNamesSimilar(string name1, string name2);
}