using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.ViewModel;

public class MainViewModel (ModelManager modelManager) : ICommand, INotifyPropertyChanged
{
    public string ProjectFolderPath = "";
    
    
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
            return modelManager.FileLoader.LoadXmlDocument(path);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException or UnauthorizedAccessException or XmlException)
        {
            modelManager.Logger.ConsoleAndLogWriteLine($"Error loading XML: {ex.Message}");
            return null;
        }
    }
    
    
    
    // ---------------------------------------- FILE FINDER ---------------------------------------- //
    /// <summary>
    /// Searches for a specific file within a given directory and its subdirectories.
    /// </summary>
    /// <param name="rootPath">The root directory path where the search begins.</param>
    /// <param name="fileNameToSearch">The name of the file to find.</param>
    /// <returns>Returns the full path of the file if found; otherwise, returns an empty string.</returns>
    public string FindFile(string rootPath, string fileNameToSearch)
    {
        return modelManager.FileFinder.FindFile(rootPath, fileNameToSearch);
    }

    /// <summary>
    /// Asynchronously searches for the '0.xml' file in the specified directory and its subdirectories.
    /// </summary>
    /// <param name="rootPath">The root directory path where the search begins.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task FindZeroXml(string rootPath)
    {
        await modelManager.FileFinder.FindZeroXml(rootPath);
    }

    
    
    // ------------------------------------------ manager.Logger ------------------------------------------ //
    /// <summary>
    /// Writes a message to the console and logs it using the manager.Logger service.
    /// </summary>
    /// <param name="msg">The message to be written to the console and the log.</param>
    /// <exception cref="Exception">Thrown if an error occurs while writing to the console and logs.</exception>
    public void ConsoleAndLogWrite(string msg)
    {
        try
        {
            modelManager.Logger.ConsoleAndLogWrite(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the console and the logs: {e.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Writes a message followed by a newline to the console and logs it using the manager.Logger service.
    /// </summary>
    /// <param name="msg">The message to be written to the console and the log.</param>
    /// <exception cref="Exception">Thrown if an error occurs while writing to the console and logs.</exception>
    public void ConsoleAndLogWriteLine(string msg)
    {
        try
        {
            modelManager.Logger.ConsoleAndLogWriteLine(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the console and the logs: {e.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Writes a message to the log using the manager.Logger service.
    /// </summary>
    /// <param name="msg">The message to be written to the log.</param>
    /// <exception cref="Exception">Thrown if an error occurs while writing to the logs.</exception>
    public void LogWrite(string msg)
    {
        try
        {
            modelManager.Logger.LogWrite(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the logs: {e.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Writes a message followed by a newline to the log using the manager.Logger service.
    /// </summary>
    /// <param name="msg">The message to be written to the log.</param>
    /// <exception cref="Exception">Thrown if an error occurs while writing to the logs.</exception>
    public void LogWriteLine(string msg)
    {
        try
        {
            modelManager.Logger.LogWriteLine(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: An error occured while writing in the logs: {e.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Closes the log writer and performs any necessary cleanup.
    /// </summary>
    /// <exception cref="Exception">Thrown if an error occurs while closing the log writer.</exception>
    public void CloseLogWriter()
    {
        try
        {
            modelManager.Logger.CloseLogWriter();
        }
        catch (Exception e)
        {
            ConsoleAndLogWriteLine($"Error: An error occured while closing the log writer: {e.Message}");
            Environment.Exit(1);
        }
    }
    
    
    
    // ------------------------------------ PROJECT FILE MANAGER ------------------------------------ //
    /// <summary>
    /// Extracts project files from the specified .knxproj file path.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file.</param>
    /// <returns>Returns <c>true</c> if the extraction was successful; otherwise, <c>false</c>.</returns>
    public bool ExtractProjectFiles(string knxprojSourceFilePath)
    {
        var extracted = modelManager.ProjectFileManager.ExtractProjectFiles(knxprojSourceFilePath);
        if (modelManager.ProjectFileManager is ProjectFileManager manager) ProjectFolderPath = manager.ProjectFolderPath;
        return extracted;
    }

    /// <summary>
    /// Extracts the group addresses file at the specified path and places it into the designated export folder.
    /// </summary>
    /// <param name="groupAddressesSourceFilePath">The path to the group addresses file that will be extracted.</param>
    /// <returns>Returns <c>true</c> if the file is successfully extracted and the process was not cancelled; otherwise, returns <c>false</c>.</returns>
    public bool ExtractGroupAddressFile(string groupAddressesSourceFilePath)
    {
        return modelManager.ProjectFileManager.ExtractGroupAddressFile(groupAddressesSourceFilePath);
    }

    /// <summary>
    /// Prompts the user to select a file path using an OpenFileDialog.
    /// </summary>
    /// <returns>Returns the selected file path as a string if a file is chosen; otherwise, returns an empty string.</returns>
    public string SelectAnotherFile()
    {
        return modelManager.ProjectFileManager.SelectAnotherFile();
    }
    
    
    
    // ------------------------------------ ZIP ARCHIVE MANAGER ------------------------------------ //
    /// <summary>
    /// Creates a ZIP archive at the specified path, adding files and/or directories to it.
    /// If the specified path is a directory, all files and subdirectories within it are included in the archive.
    /// If the path is a file, only that file is added to the archive.
    /// If the ZIP file already exists, it will be overwritten.
    /// </summary>
    /// <param name="zipFilePath">The path where the ZIP archive will be created.</param>
    /// <param name="paths">An array of file and/or directory paths to include in the archive.</param>
    public void CreateZipArchive(string zipFilePath, params string[] paths)
    {
        modelManager.ZipArchiveManager.CreateZipArchive(zipFilePath, paths);
    }

    /// <summary>
    /// Recursively adds all files and subdirectories from the specified directory to the ZIP archive.
    /// Only the contents of the directory are included in the archive, not the directory itself.
    /// </summary>
    /// <param name="archive">The ZIP archive to which files and subdirectories will be added.</param>
    /// <param name="directoryPath">The path of the directory whose contents will be added to the archive.</param>
    /// <param name="entryName">The relative path within the ZIP archive where the contents of the directory will be placed.</param>
    public void AddDirectoryToArchive(ZipArchive archive, string directoryPath, string entryName)
    {
        modelManager.ZipArchiveManager.AddDirectoryToArchive(archive, directoryPath, entryName);
    }
    
    
    
    // ------------------------------------ GROUP ADDRESS MANAGER ------------------------------------ //
    /// <summary>
    /// Extracts group address information from a specified XML file.
    ///
    /// Determines the file path to use based on user input and whether a specific group address
    /// file is chosen or a default file is used. Processes the XML file to extract and group addresses
    /// from either a specific format or a standard format.
    /// </summary>
    public void ExtractGroupAddress()
    {
        modelManager.GroupAddressManager.ExtractGroupAddress();
    }

    /// <summary>
    /// Processes an XML file in the Zero format to extract and group addresses.
    ///
    /// This method extracts device references and their links, processes group addresses, and 
    /// groups them based on device links and common names. Handles the creation and updating 
    /// of grouped addresses, avoiding name collisions by appending suffixes if necessary.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in Zero format.</param>
    /// </summary>
    public void ProcessZeroXmlFile(XDocument groupAddressFile)
    {
        modelManager.GroupAddressManager.ProcessZeroXmlFile(groupAddressFile);
    }

    /// <summary>
    /// Processes an XML file in the standard format to extract and group addresses.
    ///
    /// This method processes group addresses from the XML file, normalizing the names by removing
    /// specific prefixes ("Ie" or "Cmd") and grouping addresses based on the remaining common names.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in standard format.</param>
    /// </summary>
    public void ProcessStandardXmlFile(XDocument groupAddressFile)
    {
        modelManager.GroupAddressManager.ProcessStandardXmlFile(groupAddressFile);
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
    public void AddToGroupedAddresses(Dictionary<string, List<XElement>> groupedAddresses, XElement ga,
        string commonName)
    {
        modelManager.GroupAddressManager.AddToGroupedAddresses(groupedAddresses, ga, commonName);
    }

    /// <summary>
    /// Sets the global KNX XML namespace from the specified XML file.
    ///
    /// This method loads the XML file located at <paramref name="zeroXmlFilePath"/> and retrieves
    /// the namespace declaration from the root element. If a namespace is found, it updates the
    /// static field <c>_globalKnxNamespace</c> with the retrieved namespace. If the XML file cannot
    /// be loaded or an error occurs during processing, appropriate error messages are logged.
    ///
    /// <param name="zeroXmlFilePath">The path to the XML file from which to extract the namespace.</param>
    /// </summary>
    public void SetNamespaceFromXml(string zeroXmlFilePath)
    {
        modelManager.GroupAddressManager.SetNamespaceFromXml(zeroXmlFilePath);
    }

    /// <summary>
    /// Merges single-element groups in the grouped addresses dictionary with other groups if their names
    /// match with a similarity of 80% or more.
    ///
    /// This method compares the names of groups with a single element to other groups and merges them
    /// if they are similar enough, based on a similarity threshold of 80%.
    ///
    /// <param name="groupedAddresses">The dictionary of grouped addresses to be merged.</param>
    /// </summary>
    public void MergeSingleElementGroups(Dictionary<string, List<XElement>> groupedAddresses)
    {
        modelManager.GroupAddressManager.MergeSingleElementGroups(groupedAddresses);
    }

    /// <summary>
    /// Compares two names based on the similarity of their first three words
    /// and exact match of the remaining words.
    ///
    /// <param name="name1">The first name to compare.</param>
    /// <param name="name2">The second name to compare.</param>
    /// <returns>True if the names are similar based on the criteria; otherwise, false.</returns>
    /// </summary>
    public bool AreNamesSimilar(string name1, string name2)
    {
        return modelManager.GroupAddressManager.AreNamesSimilar(name1, name2);
    }

    /// <summary>
    /// Normalizes the name by removing specific prefixes.
    ///
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    /// </summary>
    public string NormalizeName(string name)
    {
        return modelManager.GroupAddressManager.NormalizeName(name);
    }

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
        return modelManager.GroupAddressManager.CalculateSimilarity(str1, str2);
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
        return modelManager.GroupAddressManager.LevenshteinDistance(str1, str2);
    }
    
    
    
    // --------------------------------- SYSTEM SETTINGS DETECTOR --------------------------------- //
    /// <summary>
    /// Detects the current Windows theme (light or dark).
    /// Attempts to read the theme setting from the Windows registry.
    /// Returns true if the theme is light, false if it is dark.
    /// If an error occurs or the registry value is not found, defaults to true (light theme).
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the Windows theme is light (true) or dark (false).
    /// </returns>
    public bool DetectWindowsTheme()
    {
        return modelManager.SystemSettingsDetector.DetectWindowsTheme();
    }

    /// <summary>
    /// Detects the current Windows language.
    /// If the language is supported by the application, it returns the corresponding language code.
    /// Otherwise, it returns an empty string.
    /// </summary>
    /// <returns>
    /// A string representing the Windows language code if supported; otherwise, an empty string.
    /// </returns>
    /// <remarks>
    /// This method reads the "LocaleName" value from the Windows registry under "Control Panel\International".
    /// It extracts the language code from this value and checks if it is in the set of valid language codes.
    /// If an error occurs during the registry access or if the language code is not supported, an empty string is returned.
    /// </remarks>
    public string DetectWindowsLanguage()
    {
        return modelManager.SystemSettingsDetector.DetectWindowsLanguage();
    }
    
    
    
    // ------------------------------------ DEBUG ARCHIVE GENERATOR ------------------------------------ //
    /// <summary>
    /// Creates a debug archive by collecting all debug-related files, including optional system and hardware information,
    /// imported projects, and a list of removed group addresses. The archive is then saved as a ZIP file.
    /// </summary>
    /// <param name="includeOsInfo">Specifies whether to include operating system information in the archive.</param>
    /// <param name="includeHardwareInfo">Specifies whether to include hardware information in the archive.</param>
    /// <param name="includeImportedProjects">Specifies whether to include imported projects in the archive.</param>
    public void CreateDebugArchive(bool includeOsInfo = true, bool includeHardwareInfo = true,
        bool includeImportedProjects = true)
    {
        modelManager.DebugArchiveGenerator.CreateDebugArchive(includeOsInfo, includeHardwareInfo, includeImportedProjects);
    }
    
    
    
    // ------------------------------------ APPLICATION FILE MANAGER ------------------------------------ //
    /// <summary>
    /// Ensures that the log directory exists by creating it if it does not already exist.
    /// </summary>
    public void EnsureLogDirectoryExists()
    {
        modelManager.ApplicationFileManager.EnsureLogDirectoryExists();
    }

    /// <summary>
    /// Archives the log files by compressing them into a ZIP archive when the number of log files exceeds 50.
    /// </summary>
    public void ArchiveLogs()
    {
        modelManager.ApplicationFileManager.ArchiveLogs();
    }

    /// <summary>
    /// Deletes all directories in the application directory except for 'logs' and 'resources'.
    /// </summary>
    public void DeleteAllExceptLogsAndResources()
    {
        modelManager.ApplicationFileManager.DeleteAllExceptLogsAndResources();
    }

    /// <summary>
    /// Ensures a configuration file exists at the specified path, creating it and setting defaults if necessary.
    /// </summary>
    /// <param name="settingsPath">The path to the configuration file.</param>
    public void EnsureSettingsFileExists(string settingsPath)
    {
        modelManager.ApplicationFileManager.EnsureSettingsFileExists(settingsPath);
    }

    /// <summary>
    /// Saves the application settings to the appSettings file.
    /// </summary>
    public void SaveApplicationSettings()
    {
        modelManager.ApplicationFileManager.SaveApplicationSettings();
    }
}