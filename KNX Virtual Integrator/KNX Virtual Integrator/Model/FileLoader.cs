using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model;

public class FileLoader
{
    /// <summary>
    /// Loads an XML document from a specified path.
    /// </summary>
    /// <param name="path">The path to the XML document to load.</param>
    /// <returns>Returns an XDocument if the file is successfully loaded; otherwise, returns null.</returns>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item>Attempts to load the XML document from the specified path.</item>
    /// <item>Catches and logs specific exceptions such as FileNotFoundException, DirectoryNotFoundException, IOException, UnauthorizedAccessException, and XmlException.</item>
    /// <item>Logs an error message and returns null if an exception is thrown.</item>
    /// </list>
    /// </remarks>
    public static XDocument? LoadXmlDocument(string path)
    {
        try
        {
            return XDocument.Load(path);
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException ||
                                   ex is IOException || ex is UnauthorizedAccessException || ex is XmlException)
        {
            App.ConsoleAndLogWriteLine($"Error loading XML: {ex.Message}");
            return null;
        }
    }
}