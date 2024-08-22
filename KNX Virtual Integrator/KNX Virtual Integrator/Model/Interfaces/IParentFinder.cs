using System.Windows;

namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Interface defining a utility to find the parent of a specific type in the visual tree.
    /// </summary>
    public interface IParentFinder
    {
        /// <summary>
        /// Finds the parent of a specified type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the parent to find.</typeparam>
        /// <param name="child">The starting child object from which to search up the visual tree.</param>
        /// <returns>The parent of type T, or null if no such parent is found.</returns>
        T? FindParent<T>(DependencyObject child) where T : DependencyObject;
    }
}