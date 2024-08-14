using System.IO;
using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model;

public class ModelManager
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    private readonly IFileLoader _fileLoader;
    private readonly IFileFinder _fileFinder;
    private readonly IProjectFileManager _projectFileManager; // TODO A implémenter
    private readonly ILogger _logger;
    private readonly IZipArchiveManager _zipArchiveManager;
    private readonly IGroupAddressManager _groupAddressManager;
    private readonly ISystemSettingsDetector _systemSettingsDetector;
    private readonly IDebugArchiveGenerator _debugArchiveGenerator;
    private readonly IApplicationFileManager _applicationFileManager;

    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public ModelManager(
        IFileLoader fileLoader,
        IFileFinder fileFinder,
        IProjectFileManager projectFileManager,
        ILogger logger,
        IZipArchiveManager zipArchiveManager,
        IGroupAddressManager groupAddressManager,
        ISystemSettingsDetector systemSettingsDetector,
        IDebugArchiveGenerator debugArchiveGenerator,
        IApplicationFileManager applicationFileManager)
    {
        _fileLoader = fileLoader;
        _fileFinder = fileFinder;
        _projectFileManager = projectFileManager;
        _logger = logger;
        _zipArchiveManager = zipArchiveManager;
        _groupAddressManager = groupAddressManager;
        _systemSettingsDetector = systemSettingsDetector;
        _debugArchiveGenerator = debugArchiveGenerator;
        _applicationFileManager = applicationFileManager;
    }
    
    // ---------------------------------------- FILE LOADER ---------------------------------------- //
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
    public XDocument? LoadXmlDocument(string path)
    {
        try
        {
            return _fileLoader.LoadXmlDocument(path);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException or UnauthorizedAccessException or XmlException)
        {
            _logger.ConsoleAndLogWriteLine($"Error loading XML: {ex.Message}");
            return null;
        }
    }
    
    
    // ---------------------------------------- FILE FINDER ---------------------------------------- //
    
    
    // ------------------------------------------ LOGGER ------------------------------------------ //
    public void ConsoleAndLogWrite(string msg)
    {
        try
        {
            _logger.ConsoleAndLogWrite(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the console and the logs: {e.Message}");
            Environment.Exit(1);
        }
    }
    
    public void ConsoleAndLogWriteLine(string msg)
    {
        try
        {
            _logger.ConsoleAndLogWriteLine(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the console and the logs: {e.Message}");
            Environment.Exit(1);
        }
    }
    
    public void LogWrite(string msg)
    {
        try
        {
            _logger.LogWrite(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the logs: {e.Message}");
            Environment.Exit(1);
        }
    }
    
    public void LogWriteLine(string msg)
    {
        try
        {
            _logger.LogWriteLine(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the logs: {e.Message}");
            Environment.Exit(1);
        }
    }

    public void CloseLogWriter()
    {
        try
        {
            _logger.CloseLogWriter();
        }
        catch (Exception e)
        {
            ConsoleAndLogWriteLine($"Error: An error occured while closing the log writer: {e.Message}");
            Environment.Exit(1);
        }
    }
}