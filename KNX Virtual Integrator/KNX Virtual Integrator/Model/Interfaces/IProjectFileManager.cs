namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Interface for managing project files including extraction and handling operations.
/// </summary>
public interface IProjectFileManager
{
    string ProjectFolderPath { get; set; }
    
    /// <summary>
    /// Extracts project files from the specified .knxproj file path.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file.</param>
    /// <returns>Returns <c>true</c> if the extraction was successful; otherwise, <c>false</c>.</returns>
    bool ExtractProjectFiles(string knxprojSourceFilePath);

    /// <summary>
    /// Extracts the group addresses file at the specified path and places it into the designated export folder.
    /// </summary>
    /// <param name="groupAddressesSourceFilePath">The path to the group addresses file that will be extracted.</param>
    /// <returns>Returns <c>true</c> if the file is successfully extracted and the process was not cancelled; otherwise, returns <c>false</c>.</returns>
    bool ExtractGroupAddressFile(string groupAddressesSourceFilePath);

    /// <summary>
    /// Prompts the user to select a file path using an OpenFileDialog.
    /// </summary>
    /// <returns>Returns the selected file path as a string if a file is chosen; otherwise, returns an empty string.</returns>
    string SelectAnotherFile();
}