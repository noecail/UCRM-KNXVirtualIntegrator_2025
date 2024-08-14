using System.IO;
using System.IO.Compression;

namespace KNX_Virtual_Integrator.Model;

public class ApplicationFileManager
{
    // Fonction d'archivage des logs
    // Fonctionnement : S'il y a plus de 50 fichiers logs.txt, ces fichiers sont rassemblés et compresses dans une archive zip
    // S'il y a plus de 10 archives, ces dernieres sont supprimees avant la creation de la nouvelle archive
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
    internal static void ArchiveLogs()
    {
        var logDirectory = @"./logs/"; // Chemin du dossier de logs
            
        try
        {
            // Verifier si le repertoire existe
            if (!Directory.Exists(logDirectory))
            {
                Logger.ConsoleAndLogWriteLine($"--> The specified directory does not exist : {logDirectory}");
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

                    Logger.ConsoleAndLogWriteLine("--> Deleted all existing archive files as they exceeded the limit of 10.");
                }

                // Creer le nom du fichier zip avec la date actuelle
                var zipFileName = Path.Combine(logDirectory, $"LogsArchive-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");

                // Creer l'archive zip et y ajouter les fichiers log
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                {
                    foreach (var logFile in logFiles)
                    {
                        if (logFile != Logger.LogPath) // Si le fichier logs n'est pas celui que l'on vient de creer pour le lancement actuel
                        {
                            zip.CreateEntryFromFile(logFile, Path.GetFileName(logFile)); // On l'ajoute e l'archive
                            File.Delete(logFile); // Puis, on le supprime
                        }
                    }
                }

                Logger.ConsoleAndLogWriteLine($"--> Successfully archived log files to {zipFileName}");
            }
            else
            {
                Logger.ConsoleAndLogWriteLine("--> Not enough log files to archive.");
            }
        }
        catch (Exception ex)
        {
            Logger.ConsoleAndLogWriteLine($"--> An error occured while creating the log archive : {ex.Message}");
        }
    }

    
    // Fonction permettant de supprimer tous les dossiers presents dans le dossier courant
    // Sauf le fichier logs. Cela permet de supprimer tous les projets exportés à la session precedente.
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
    internal static void DeleteAllExceptLogsAndResources()
    {
        if (Directory.GetDirectories("./").Length <= 3 && Directory.GetFiles("./", "*.zip").Length == 0)
        {
            Logger.ConsoleAndLogWriteLine("--> No folder or zip file to delete");
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
                Logger.ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {directory}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                Logger.ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {directory}: {ex.Message}");
                continue;
            }

            Logger.ConsoleAndLogWriteLine($"--> Deleted directory: {directory}");
        }

        foreach (var zipFile in Directory.GetFiles("./", "*.zip"))
        {
            try
            {
                File.Delete(zipFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {zipFile}: {ex.Message}");
                continue;
            }
            catch (IOException ex)
            {
                Logger.ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {zipFile}: {ex.Message}");
                continue;
            }

            Logger.ConsoleAndLogWriteLine($"--> Deleted file: {zipFile}");
        }
            
    }
}