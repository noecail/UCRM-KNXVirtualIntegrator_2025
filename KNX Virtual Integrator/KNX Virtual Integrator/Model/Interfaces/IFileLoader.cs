using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Defines the contract for file loading operations.
    /// </summary>
    public interface IFileLoader
    {
        /// <summary>
        /// Loads an XML document from a specified path.
        /// </summary>
        /// <param name="path">The path to the XML document to load.</param>
        /// <returns>Returns an XDocument if the file is successfully loaded; otherwise, returns null.</returns>
        XDocument? LoadXmlDocument(string path);
    }
}