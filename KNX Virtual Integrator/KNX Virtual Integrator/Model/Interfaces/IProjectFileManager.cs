namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Interface for managing project files including extraction and handling operations.
/// </summary>
public interface IProjectFileManager
{
    /// <summary>
    /// Gets the path to the exported project folder.
    /// </summary>
    /// <remarks>
    /// This property holds the file path of the project folder where the project files are exported.
    /// </remarks>

    string ProjectFolderPath { get; set; }

    /// <summary>
    ///  Gets the name of the project the application is currently working on.
    /// </summary>
    string ProjectName { get; }

    /// <summary>
    /// Gets the path to the 0.xml file of the project.
    /// </summary>
    /// <remarks>
    /// This property holds the file path to the 0.xml file associated with the project.
    /// </remarks>
    string ZeroXmlPath { get; }

    /// <summary>
    /// Gets the path to the exported of the group addresses file.
    /// </summary>
    /// <remarks>
    /// This property holds the file path  of the group addresses file
    /// </remarks>
    public string GroupAddressFilePath { get; set; } // Chemin d'accès au dossier exporté du projet

    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- MÉTHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Extracts project files from the specified .knxproj file path.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file.</param>
    /// <returns>Returns <c>true</c> if the extraction was successful; otherwise, <c>false</c>.</returns>
    bool ExtractProjectFiles(string knxprojSourceFilePath);

    /// <summary>
    /// Extracts the group addresses file at the specified path and place it into the designated export folder.
    /// </summary>
    /// <param name="groupAddressesSourceFilePath">The path to the group addresses file that will be extracted.</param>
    /// <returns>Returns <c>true</c> if the file is successfully extracted and the process was not cancelled; otherwise, returns <c>false</c>.</returns>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Normalizes the path of the group addresses file and handles potential path-related exceptions.</item>
    /// <item>Deletes any existing group addresses file to avoid conflicts.</item>
    /// <item>Copy the file to the right folder path and indicates successful extraction if no cancellation occurred.</item>
    /// </list>
    /// </remarks>
    bool ExtractGroupAddressFile(string groupAddressesSourceFilePath);

    /// <summary>
    /// Prompts the user to select a file path using an OpenFileDialog.
    /// </summary>
    /// <returns>Returns the selected file path as a string if a file is chosen; otherwise, returns an empty string.</returns>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item>Displays a file open dialog with predefined filters and settings.</item>
    /// <item>Returns the path of the selected file if the user confirms their choice.</item>
    /// <item>Handles potential exceptions, including invalid dialog state, external errors, and other unexpected issues.</item>
    /// </list>
    /// </remarks>
    string SelectAnotherFile();
}