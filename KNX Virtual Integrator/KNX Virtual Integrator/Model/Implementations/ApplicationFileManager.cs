using System.IO;
using System.IO.Compression;
using System.Windows;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Provides an interface for managing application files, including log file management, 
/// archiving, and configuration settings.
/// </summary>
/// <param name="logger"></param>
public class ApplicationFileManager (ILogger logger) : IApplicationFileManager
{
    /// <summary>
    /// Ensures that the log directory exists by creating it if it does not already exist.
    /// <para>
    /// If the directory cannot be created due to an exception, the application will be terminated with an error message.
    /// </para>
    /// </summary>
    public void EnsureLogDirectoryExists()
    {
        try
        {
            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }
        }
        catch (Exception ex)
        {
            logger.ConsoleAndLogWriteLine($"Error: Unable to create the log directory. {ex.Message}");
            Environment.Exit(1); // Terminates the application with an exit code indicating an error
        }
    }
    
    
    // Fonction d'archivage des logs
    // Fonctionnement : S'il y a plus de 50 fichiers logs.txt, ces fichiers sont rassemblés et compresses dans une archive zip
    // S'il y a plus de 10 archives, ces dernières sont supprimées avant la creation de la nouvelle archive
    // Conséquence : on ne stocke les logs que des 50 derniers lancements de l'application
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
    public void ArchiveLogs()
    {
        var logDirectory = @"./logs/"; // Chemin du dossier de logs
            
        try
        {
            // Vérifier si le repertoire existe
            if (!Directory.Exists(logDirectory))
            {
                logger.ConsoleAndLogWriteLine($"--> The specified directory does not exist : {logDirectory}");
                return;
            }

            // Obtenir tous les fichiers log dans le repertoire
            var logFiles = Directory.GetFiles(logDirectory, "*.txt");

            // Verifier s'il y a plus de 50 fichiers log
            if (logFiles.Length > 50)
            {
                // Obtenir tous les fichiers d'archive dans le repertoire
                var archiveFiles = Directory.GetFiles(logDirectory, "LogsArchive-*.zip");

                // Supprimer les archives existantes si elles sont plus de 10
                if (archiveFiles.Length >= 10)
                {
                    foreach (var archiveFile in archiveFiles)
                    {
                        File.Delete(archiveFile);
                    }

                    logger.ConsoleAndLogWriteLine("--> Deleted all existing archive files as they exceeded the limit of 10.");
                }

                // Créer le nom du fichier zip avec la date actuelle
                var zipFileName = Path.Combine(logDirectory, $"LogsArchive-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");

                // Créer l'archive zip et y ajouter les fichiers log
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                {
                    foreach (var logFile in logFiles)
                    {
                        if (logger is Logger log && logFile != log.LogPath) // Si le fichier logs n'est pas celui que l'on vient de créer pour le lancement actuel
                        {
                            zip.CreateEntryFromFile(logFile, Path.GetFileName(logFile)); // On l'ajoute à l'archive
                            File.Delete(logFile); // Puis, on le supprime
                        }
                    }
                }

                logger.ConsoleAndLogWriteLine($"--> Successfully archived log files to {zipFileName}");
            }
            else
            {
                logger.ConsoleAndLogWriteLine("--> Not enough log files to archive.");
            }
        }
        catch (Exception ex)
        {
            logger.ConsoleAndLogWriteLine($"--> An error occured while creating the log archive : {ex.Message}");
        }
    }

    
    // Fonction permettant de supprimer tous les dossiers presents dans le dossier courant
    // Sauf le fichier logs. Cela permet de supprimer tous les projets exportés à la session précédente.
    // Fonction pour supprimer tous les dossiers sauf le dossier 'logs'
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
    public void DeleteAllExceptLogsAndResources()
    {
        if (Directory.GetDirectories("./").Length <= 3 && Directory.GetFiles("./", "*.zip").Length == 0)
        {
            logger.ConsoleAndLogWriteLine("--> No folder or zip file to delete");
        }
            
        // Itération sur tous les répertoires dans le répertoire de base
        foreach (var directory in Directory.GetDirectories("./"))
        {
            // Exclure le dossier 'logs', 'de' et 'runtimes'
            if ((Path.GetFileName(directory).Equals("logs", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("runtimes", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("de", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            // Supprimer le dossier et son contenu
            try
            {
                Directory.Delete(directory, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {directory}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                logger.ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {directory}: {ex.Message}");
                continue;
            }

            logger.ConsoleAndLogWriteLine($"--> Deleted directory: {directory}");
        }

        foreach (var zipFile in Directory.GetFiles("./", "*.zip"))
        {
            try
            {
                File.Delete(zipFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {zipFile}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                logger.ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {zipFile}: {ex.Message}");
                continue;
            }

            logger.ConsoleAndLogWriteLine($"--> Deleted file: {zipFile}");
        }
            
    }


    /// <summary>
    /// Ensures a configuration file exists at the specified path. If not, it creates the file and sets defaults 
    /// based on the system theme and language. Handles exceptions such as unauthorized access, invalid paths, 
    /// and I/O errors, displaying an error message and closing the application if an issue arises.
    ///
    /// <param name="settingsPath">The path to the configuration file.</param>
    /// </summary>
    public bool EnsureSettingsFileExists(string settingsPath)
    {
        try
        {
            // Si le fichier de paramétrage n'existe pas, on le crée
            // Note : comme File.Create ouvre un stream vers le fichier à la création, on le ferme directement avec Close().
            if (File.Exists(settingsPath)) return true;

            File.Create(settingsPath).Close();

            if (App.WindowManager is null) return false;
        }
        // Si une exception a lieu
        catch (Exception e)
        {
            logger.ConsoleAndLogWriteLine($"An error occured while creating the file located at {settingsPath} : {e.Message}");
            Application.Current.Shutdown(1);
        }

        return false;
    }
}