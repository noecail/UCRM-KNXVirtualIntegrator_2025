namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Defines the contract for file finding operations.
    /// </summary>
    public interface IFileFinder
    {
        /// <summary>
        /// Asynchronously searches for a specific file within a given directory and its subdirectories.
        /// </summary>
        /// <param name="rootPath">The root directory path where the search begins.</param>
        /// <param name="fileNameToSearch">The name of the file to find.</param>
        /// <returns>Returns the full path of the file if found; otherwise, returns an empty string.</returns>
        string FindFile(string rootPath, string fileNameToSearch);

        /// <summary>
        /// Asynchronously searches for the '0.xml' file in the exported KNX project directory.
        /// </summary>
        /// <param name="rootPath">The root directory path where the search begins.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task FindZeroXml(string rootPath);
    }
}