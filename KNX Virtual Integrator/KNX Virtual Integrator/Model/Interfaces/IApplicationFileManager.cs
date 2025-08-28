namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    ///     Provides an interface for managing application files, including log file management, 
    ///     archiving, and configuration settings.
    /// </summary>
    public interface IApplicationFileManager
    {
        /// <summary>
        /// Ensures that the log directory exists by creating it if it does not already exist.
        /// <para>
        /// If the directory cannot be created due to an exception, the application will be terminated with an error message.
        /// </para>
        /// </summary>
        void EnsureLogDirectoryExists();

        /// <summary>
        /// Archives the log files in the log directory by compressing them into a ZIP archive when the number of log files exceeds 50.
        /// <para>
        /// If there are more than 50 log files, the method will create a new ZIP archive containing all log files, excluding the current log file.
        /// If there are already 10 or more existing archives, it will delete the oldest ones before creating a new archive.
        /// This ensures that only the log files from the last 50 application runs are retained.
        /// </para>
        /// <para>
        /// If there are fewer than 50 log files, no archiving will be performed.
        /// </para>
        /// <para>
        /// If an error occurs during the process, it logs the error message to the console and log file.
        /// </para>
        /// </summary>
        void ArchiveLogs();

        /// <summary>
        /// Deletes all directories in the application directory except for those named 'logs' and 'resources'.
        /// <para>
        /// This method iterates through all subdirectories in the base directory and deletes them, excluding the directories 'logs' and 'resources'.
        /// This helps in cleaning up directories from previous sessions, retaining only the specified directories for future use.
        /// </para>
        /// <para>
        /// In case of an error during the deletion, such as unauthorized access or I/O errors, the method logs the error message to the console and continues processing other directories.
        /// </para>
        /// <para>
        /// The method logs the path of each successfully deleted directory to the application log for tracking purposes.
        /// </para>
        /// </summary>
        void DeleteAllExceptLogsAndResources();

        /// <summary>
        /// Ensures a configuration file exists at the specified path. If not, it creates the file and sets defaults 
        /// based on the system theme and language. Handles exceptions such as unauthorized access, invalid paths, 
        /// and I/O errors, displaying an error message and closing the application if an issue arises.
        ///
        /// <param name="settingsPath">The path to the configuration file.</param>
        /// </summary>
        bool EnsureSettingsFileExists(string settingsPath);
    }
}