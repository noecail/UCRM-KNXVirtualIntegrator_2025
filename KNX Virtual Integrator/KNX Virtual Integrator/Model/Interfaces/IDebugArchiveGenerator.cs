namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Interface for generating debug archives.
    /// </summary>
    public interface IDebugArchiveGenerator
    {
        /// <summary>
        /// Creates a debug archive by collecting all debug-related files, including optional system and hardware information,
        /// imported projects, and a list of removed group addresses. The archive is then saved as a ZIP file.
        /// </summary>
        /// <param name="includeOsInfo">Specifies whether to include operating system information in the archive.</param>
        /// <param name="includeHardwareInfo">Specifies whether to include hardware information in the archive.</param>
        /// <param name="includeImportedProjects">Specifies whether to include imported projects in the archive.</param>
        void CreateDebugArchive(bool includeOsInfo = true, bool includeHardwareInfo = true, bool includeImportedProjects = true);
    }
}