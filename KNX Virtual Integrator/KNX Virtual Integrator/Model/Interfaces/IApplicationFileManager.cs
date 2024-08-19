namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Provides an interface for managing application files, including log file management, 
    /// archiving, and configuration settings.
    /// </summary>
    public interface IApplicationFileManager
    {
        /// <summary>
        /// Ensures that the log directory exists by creating it if it does not already exist.
        /// </summary>
        void EnsureLogDirectoryExists();

        /// <summary>
        /// Archives the log files by compressing them into a ZIP archive when the number of log files exceeds 50.
        /// </summary>
        void ArchiveLogs();

        /// <summary>
        /// Deletes all directories in the application directory except for 'logs' and 'resources'.
        /// </summary>
        void DeleteAllExceptLogsAndResources();

        /// <summary>
        /// Ensures a configuration file exists at the specified path, creating it and setting defaults if necessary.
        /// </summary>
        /// <param name="settingsPath">The path to the configuration file.</param>
        void EnsureSettingsFileExists(string settingsPath);

        /// <summary>
        /// Saves the application settings to the appSettings file.
        /// </summary>
        void SaveApplicationSettings();
    }
}