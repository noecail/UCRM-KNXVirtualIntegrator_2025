using System.Windows;
using System.Windows.Media;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

/// <summary>
/// Provides an implementation of IParentFinder for finding the parent of a specific type in the visual tree.
/// </summary>
public class ParentFinder (ILogger logger) : IParentFinder
{
    /// <summary>
    /// Utility method to find the parent of a specific type in the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the parent to find.</typeparam>
    /// <param name="child">The starting child object from which to search up the visual tree.</param>
    /// <returns>The parent of type T, or null if no such parent is found.</returns>
    public T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        try
        {
            // Boucle jusqu'à ce qu'un parent de type T soit trouvé ou qu'il n'y ait plus de parents
            while (true)
            {
                // Obtient le parent visuel de l'objet enfant actuel
                var parentObject = VisualTreeHelper.GetParent(child);
            
                // Gère les différents cas possibles pour le parentObject
                switch (parentObject)
                {
                    // Si aucun parent n'est trouvé, retourne null
                    case null:
                        return null;
                    // Si le parent est du type T, retourne ce parent
                    case T parent:
                        return parent;
                    // Sinon, continue la recherche avec le parentObject comme nouvel enfant
                    default:
                        child = parentObject;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            logger.ConsoleAndLogWriteLine($"An error occurred while finding parent: {ex.Message}");
            return null;
        }
    }
}