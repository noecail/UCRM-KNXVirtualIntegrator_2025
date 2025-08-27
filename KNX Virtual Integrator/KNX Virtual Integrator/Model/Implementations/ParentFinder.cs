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
    /*
    // Méthode récursive qui remonte les parents d'un objet jusqu'à atteindre le parent du type passé en paramètre
    // Utilisée dans RemoveTestedElementFromStructureButtonClick
    // Utilisée dans AddTestToElementButtonClick
    // Utilisée dans RemoveTestFromElementButtonClick
    /// <summary>
    /// Recursive method that searches for the parent of the child object of a specific type in the visual tree.
    /// </summary>
    /// <param name="child">The child from which we search for the parent.</param>
    /// <typeparam name="T">The type of item that should be the parent.</typeparam>
    /// <returns>Either null if not parent is found or the parent (found recursively or not).</returns>
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
    }*/
    
    /// <summary>
    /// Recursive method that searches for the child of the parent that is of a specific type in the visual tree.
    /// </summary>
    /// <param name="parent">The parent from which to search.</param>
    /// <typeparam name="T">The type of the child.</typeparam>
    /// <returns>The child of the specified type, found recursively.</returns>
    public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T correctlyTyped)
                return correctlyTyped;

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }
}