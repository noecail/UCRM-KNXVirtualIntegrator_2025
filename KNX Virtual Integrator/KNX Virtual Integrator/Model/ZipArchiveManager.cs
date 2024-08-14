using System.IO;
using System.IO.Compression;

namespace KNX_Virtual_Integrator.Model;

public class ZipArchiveManager
{
    // Fonction permettant de créer une archive zip et d'ajouter des fichiers dedans
    /// <summary>
    /// Creates a ZIP archive at the specified path, adding files and/or directories to it.
    /// If the specified path is a directory, all files and subdirectories within it are included in the archive.
    /// If the path is a file, only that file is added to the archive.
    /// If the ZIP file already exists, it will be overwritten.
    /// </summary>
    /// <param name="zipFilePath">The path where the ZIP archive will be created.</param>
    /// <param name="paths">An array of file and/or directory paths to include in the archive.</param>
    public static void CreateZipArchive(string zipFilePath, params string[] paths)
    {
        // Si l'archive existe déjà, on va juste la mettre à jour
        if (File.Exists(zipFilePath))
        {
            try
            {
                using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update);

                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        // Ajouter tous les fichiers du répertoire (et sous-répertoires) à l'archive
                        Logger.ConsoleAndLogWriteLine($"{path} {Path.GetDirectoryName(path)}");
                        AddDirectoryToArchive(archive, path, path);
                    }
                    else if (File.Exists(path))
                    {
                        // Créer le dossier dans l'archive si nécessaire
                        var directoryInArchive = Path.GetDirectoryName(path)?.Replace("\\", "/")!;

                        // Ajouter le fichier dans l'archive
                        archive.CreateEntryFromFile(path,
                            directoryInArchive.Equals("debug", StringComparison.OrdinalIgnoreCase)
                                ? $"{Path.GetFileName(path)}"
                                : $"{directoryInArchive}/{Path.GetFileName(path)}", CompressionLevel.Optimal);
                    }
                    else
                    {
                        Logger.ConsoleAndLogWriteLine(
                            $"Le chemin {path} n'a pas été trouvé et ne sera pas ajouté à l'archive en cours de création.");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ConsoleAndLogWriteLine($"Error: an error occured while creating and adding files to the archive at {zipFilePath} : {e.Message}");
            }
        }
        else
        {
            try
            {
                // Créer l'archive ZIP et ajouter les fichiers/répertoires
                using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
                
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        // Ajouter tous les fichiers du répertoire (et sous-répertoires) à l'archive
                        AddDirectoryToArchive(archive, path, Path.GetFileName(path));
                    }
                    else if (File.Exists(path))
                    {
                        // Créer le dossier dans l'archive si nécessaire
                        var directoryInArchive = Path.GetDirectoryName(path)?.Replace("\\", "/")!;

                        // Ajouter le fichier dans l'archive
                        archive.CreateEntryFromFile(path,
                            directoryInArchive.Equals("debug", StringComparison.OrdinalIgnoreCase)
                                ? $"{Path.GetFileName(path)}" : $"{directoryInArchive}/{Path.GetFileName(path)}", CompressionLevel.Optimal);
                    }
                    else
                    {
                        Logger.ConsoleAndLogWriteLine(
                            $"Le chemin {path} n'a pas été trouvé et ne sera pas ajouté à l'archive en cours de création.");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ConsoleAndLogWriteLine($"Error: an error occured while creating and adding files to the archive at {zipFilePath} : {e.Message}");
            }
        }
    }
        
    
    // Fonction permettant d'ajouter le contenu d'un dossier dans une archive zip
    /// <summary>
    /// Recursively adds all files and subdirectories from the specified directory to the ZIP archive.
    /// Only the contents of the directory are included in the archive, not the directory itself.
    /// </summary>
    /// <param name="archive">The ZIP archive to which files and subdirectories will be added.</param>
    /// <param name="directoryPath">The path of the directory whose contents will be added to the archive.</param>
    /// <param name="entryName">The relative path within the ZIP archive where the contents of the directory will be placed.</param>
    public static void AddDirectoryToArchive(ZipArchive archive, string directoryPath, string entryName)
    {
        try
        {
            // Ajouter les fichiers du répertoire à l'archive ZIP
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                // Créer un chemin relatif à stocker dans le fichier ZIP
                var entryPath = Path.Combine(entryName, Path.GetFileName(file));

                using var entryStream = archive.CreateEntry(entryPath).Open();

                using var fileStream = File.OpenRead(file);

                fileStream.CopyTo(entryStream);
            }

            // Ajouter récursivement les sous-répertoires
            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                // Ajouter le contenu du sous-répertoire au ZIP
                AddDirectoryToArchive(archive, directory, Path.Combine(entryName, Path.GetFileName(directory)));
            }
        }
        catch (Exception e)
        {
            Logger.ConsoleAndLogWriteLine($"Error: an error occured while adding a directory to the debug archive : {e.Message}");
        }
    }
}