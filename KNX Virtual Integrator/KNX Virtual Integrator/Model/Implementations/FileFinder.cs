using System.IO;
using System.Windows;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class FileFinder(Logger logger, ProjectFileManager projectFileManager) : IFileFinder
{
    private readonly ILogger _logger = logger;
    private readonly IProjectFileManager _projectFileManager = projectFileManager;
    
    
    // Fonction permettant de trouver un fichier dans un dossier donné
    /// <summary>
    /// Searches for a specific file within a given directory and its subdirectories.
    /// </summary>
    /// <param name="rootPath">The root directory path where the search begins.</param>
    /// <param name="fileNameToSearch">The name of the file to find.</param>
    /// <returns>Returns the full path of the file if found; otherwise, returns an empty string.</returns>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item>Checks if the root directory exists; logs an error if it does not.</item>
    /// <item>Uses a breadth-first search approach with a queue to explore the directory and its subdirectories.</item>
    /// <item>Attempts to find the file by comparing the file names in a case-insensitive manner.</item>
    /// <item>Handles exceptions such as unauthorized access, directory not found, and general I/O errors.</item>
    /// </list>
    /// </remarks>
    public string FindFile(string rootPath, string fileNameToSearch)
    {
        if (!Directory.Exists(rootPath))
        {
            _logger.ConsoleAndLogWriteLine($"Directory {rootPath} does not exist.");
            return "";
        }

        // Création d'une file d'attente pour les répertoires à explorer
        var directoriesQueue = new Queue<string>();
        directoriesQueue.Enqueue(rootPath);

        while (directoriesQueue.Count > 0)
        {
            var currentDirectory = directoriesQueue.Dequeue();
            try
            {
                // Vérifier les fichiers dans le répertoire actuel
                var files = Directory.GetFiles(currentDirectory);
                foreach (var file in files)
                {
                    if (Path.GetFileName(file).Equals(fileNameToSearch, StringComparison.OrdinalIgnoreCase))
                    {
                        return file; // Fichier trouvé, on retourne son chemin
                    }
                }

                // Ajouter les sous-répertoires à la file d'attente
                var subDirectories = Directory.GetDirectories(currentDirectory);
                foreach (var subDirectory in subDirectories)
                {
                    directoriesQueue.Enqueue(subDirectory);
                }
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                // Si l'accès au répertoire est refusé
                _logger.ConsoleAndLogWriteLine($"Access refused to {currentDirectory} : {unAuthEx.Message}");
            }
            catch (DirectoryNotFoundException dirNotFoundEx)
            {
                // Si le répertoire est introuvable
                _logger.ConsoleAndLogWriteLine($"Directory not found : {currentDirectory} : {dirNotFoundEx.Message}");
            }
            catch (IOException ioEx)
            {
                // Si une erreur d'entrée/sortie survient
                _logger.ConsoleAndLogWriteLine($"I/O Error while accessing {currentDirectory} : {ioEx.Message}");
            }
            catch (Exception ex)
            {
                // Gérer toutes autres exceptions génériques
                _logger.ConsoleAndLogWriteLine(
                    $"An unexpected error occurred while accessing {currentDirectory} : {ex.Message}");
            }
        }

        return ""; // Fichier non trouvé
    }
    
    
    // Fonction permettant de trouver le fichier 0.xml dans le projet exporté
    // ATTENTION : Nécessite que le projet .knxproj ait déjà été extrait avec la fonction extractProjectFiles().
    /// <summary>
    /// Asynchronously searches for the '0.xml' file in the exported KNX project directory.
    /// </summary>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item>Updates the loading window with progress messages in the application's selected language.</item>
    /// <item>Calls the <see cref="FindFile"/> method to search for the '0.xml' file within the directory specified by <see cref="ProjectFolderPath"/>.</item>
    /// <item>If the file is found, updates the <see cref="ZeroXmlPath"/> property and logs the result.</item>
    /// <item>If the file is not found, logs an error message and shuts down the application.</item>
    /// <item>Handles exceptions related to file access, directory not found, and general I/O errors.</item>
    /// </list>
    /// </remarks>
    public async Task FindZeroXml(string rootPath)
    {
        try
        {
            var foundPath = FindFile(rootPath, "0.xml");

            // Si le fichier n'a pas été trouvé
            if (string.IsNullOrEmpty(foundPath))
            {
                _logger.ConsoleAndLogWriteLine("Unable to find the file '0.xml' in the project folders. "
                                               + "Please ensure that the extracted archive is indeed a KNX ETS project.");
                // Utilisation de Dispatcher.Invoke pour fermer l'application depuis un thread non-UI
                await Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
            }
            else // Sinon
            {
                if (_projectFileManager is ProjectFileManager manager)
                {
                    manager.ZeroXmlPath = foundPath;
                    _logger.ConsoleAndLogWriteLine($"Found '0.xml' file at {Path.GetFullPath(manager.ZeroXmlPath)}.");
                }
            }
        }
        catch (UnauthorizedAccessException unAuthEx)
        {
            // Gérer les erreurs d'accès non autorisé
            _logger.ConsoleAndLogWriteLine($"Access refused while searching for '0.xml': {unAuthEx.Message}");
        }
        catch (DirectoryNotFoundException dirNotFoundEx)
        {
            // Gérer les erreurs où le répertoire n'est pas trouvé
            _logger.ConsoleAndLogWriteLine($"Directory not found while searching for '0.xml': {dirNotFoundEx.Message}");
        }
        catch (IOException ioEx)
        {
            // Gérer les erreurs d'entrée/sortie
            _logger.ConsoleAndLogWriteLine($"I/O Error while searching for '0.xml': {ioEx.Message}");
        }
        catch (Exception ex)
        {
            // Gérer toutes autres exceptions génériques
            _logger.ConsoleAndLogWriteLine($"An unexpected error occurred while searching for '0.xml': {ex.Message}");
        }
    }
}