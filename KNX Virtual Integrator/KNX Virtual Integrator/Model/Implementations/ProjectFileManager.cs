using System.IO;
using System.IO.Compression;
using KNX_Virtual_Integrator.Model.Interfaces;
using Microsoft.Win32;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace KNX_Virtual_Integrator.Model.Implementations;

public class ProjectFileManager(ILogger logger, ApplicationSettings settings) : IProjectFileManager
{

    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Gets the path to the exported project folder.
    /// </summary>
    /// <remarks>
    /// This property holds the file path of the project folder where the project files are exported.
    /// </remarks>
    public string ProjectFolderPath { get; set; } = ""; // Chemin d'accès au dossier exporté du projet
    
    
    /// <summary>
    ///  Gets the name of the project the application is currently working on.
    /// </summary>
    public string ProjectName { get; private set; } = "";
    
    
    /// <summary>
    /// Gets the path to the 0.xml file of the project.
    /// </summary>
    /// <remarks>
    /// This property holds the file path to the 0.xml file associated with the project.
    /// </remarks>
    public string ZeroXmlPath { get; internal set; } = ""; // Chemin d'accès au fichier 0.xml du projet
    
    
    /// <summary>
    /// Gets the path to the exported of the group addresses file.
    /// </summary>
    /// <remarks>
    /// This property holds the file path  of the group addresses file
    /// </remarks>
    public string GroupAddressFilePath { get; set; } = ""; // Chemin d'accès au dossier exporté du projet
    
    
    /// <summary>
    ///  Gets the name of the group addresses file the application is currently working on.
    /// </summary>
    private string GroupAddressFileName { get; set; } = "";
    
    
    
    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- MÉTHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Extracts project files from a .knxproj file. Continuously attempts to process the file until successful or the user cancels.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj source file.</param>
    /// <returns>Returns true if the extraction is successful, otherwise returns false.</returns>
    public bool ExtractProjectFiles(string knxprojSourceFilePath)
    {
        while (true)
        {
            if (string.Equals(knxprojSourceFilePath, "null", StringComparison.CurrentCultureIgnoreCase))
            {
                logger.ConsoleAndLogWriteLine("User cancelled the project extraction process.");
                return false;
            }

            knxprojSourceFilePath = NormalizePath(knxprojSourceFilePath);

            var zipArchivePath = GetZipArchivePath(knxprojSourceFilePath);

            if (!HandleExistingFile(zipArchivePath)) continue; // File delete operation

            if (!CopyToZipArchive(knxprojSourceFilePath, zipArchivePath)) continue; // File copy operation

            if (!HandleExportFolder(knxprojSourceFilePath)) continue; // Export folder management

            if (ExtractZipFile(zipArchivePath, knxprojSourceFilePath)) return true; // Zip extraction success

            // On error, loop to retry
        }
    }

    /// <summary>
    /// Normalizes the file path to an absolute path. Handles exceptions related to invalid or overly long paths.
    /// </summary>
    /// <param name="path">The file path to normalize.</param>
    /// <returns>Returns the normalized path or prompts the user to select another file in case of an error.</returns>
    private string NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (ArgumentException)
        {
            logger.ConsoleAndLogWriteLine("Error: the .knxproj source file path is empty. Please try selecting the file again.");
            return SelectAnotherFile();
        }
        catch (PathTooLongException)
        {
            logger.ConsoleAndLogWriteLine($"Error: the path {path} is too long (more than 255 characters). Please try selecting another path.");
            return SelectAnotherFile();
        }
        catch (Exception ex)
        {
            logger.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
            return SelectAnotherFile();
        }
    }

    /// <summary>
    /// Retrieves the path for the zip archive based on the .knxproj file path. Validates that the file is a .knxproj file.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file.</param>
    /// <returns>Returns the path for the zip archive or prompts the user to select another file if the input is invalid.</returns>
    private string GetZipArchivePath(string knxprojSourceFilePath)
    {
        if (knxprojSourceFilePath.EndsWith(".knxproj"))
        
            return $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}.zip";
        
        logger.ConsoleAndLogWriteLine("Error: the selected file is not a .knxproj file. Please try again.");
        return SelectAnotherFile();
    }

    /// <summary>
    /// Handles the existence of an existing zip file. Deletes it if it exists, and logs errors if the delete operation fails.
    /// </summary>
    /// <param name="zipArchivePath">The path to the zip archive.</param>
    /// <returns>Returns true if the file was handled successfully, otherwise returns false.</returns>
    private bool HandleExistingFile(string zipArchivePath)
    {
        if (File.Exists(zipArchivePath))
        {
            logger.ConsoleAndLogWriteLine($"{zipArchivePath} already exists. Removing the file before creating the new archive.");
            try
            {
                File.Delete(zipArchivePath);
            }
            catch (IOException ex)
            {
                logger.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}. Please change the rights of the file.");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Copies the .knxproj file to a new zip archive. Handles various file-related exceptions and logs errors if the operation fails.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file.</param>
    /// <param name="zipArchivePath">The path to the new zip archive.</param>
    /// <returns>Returns true if the file was copied successfully, otherwise returns false.</returns>
    private bool CopyToZipArchive(string knxprojSourceFilePath, string zipArchivePath)
    {
        try
        {
            File.Copy(knxprojSourceFilePath, zipArchivePath);
            return true;
        }
        catch (FileNotFoundException)
        {
            logger.ConsoleAndLogWriteLine($"Error: the file {knxprojSourceFilePath} was not found.");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            logger.ConsoleAndLogWriteLine($"Unable to write to the file {knxprojSourceFilePath}. Please check permissions.");
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            logger.ConsoleAndLogWriteLine($"The folder {Path.GetDirectoryName(knxprojSourceFilePath)} cannot be found.");
            return false;
        }
        catch (PathTooLongException)
        {
            logger.ConsoleAndLogWriteLine($"Error: the path {knxprojSourceFilePath} is too long.");
            return false;
        }
        catch (Exception ex)
        {
            logger.ConsoleAndLogWriteLine($"Error copying file {knxprojSourceFilePath} to {zipArchivePath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Manages the export folder by deleting any existing folder if it exists. Logs errors if the delete operation fails.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file used to determine the export folder path.</param>
    /// <returns>Returns true if the folder was handled successfully, otherwise returns false.</returns>
    private bool HandleExportFolder(string knxprojSourceFilePath)
    {
        var knxprojExportFolderPath = $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/knxproj_exported/";

        if (Directory.Exists(knxprojExportFolderPath))
        {
            logger.ConsoleAndLogWriteLine($"The folder {knxprojExportFolderPath} already exists, deleting...");
            try
            {
                Directory.Delete(knxprojExportFolderPath, true);
                return true;
            }
            catch (IOException ex)
            {
                logger.ConsoleAndLogWriteLine($"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.ConsoleAndLogWriteLine($"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                logger.ConsoleAndLogWriteLine($"Error deleting folder {knxprojExportFolderPath}: {ex.Message}");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Extracts the contents of the zip archive to the export folder and deletes the zip file upon successful extraction.
    /// </summary>
    /// <param name="zipArchivePath">The path to the zip archive.</param>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file used to determine the export folder path.</param>
    /// <returns>Returns true if the extraction was successful, otherwise returns false.</returns>
    private bool ExtractZipFile(string zipArchivePath, string knxprojSourceFilePath)
    {
        try
        {
            var knxprojExportFolderPath = $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/knxproj_exported/";

            ProjectFolderPath = knxprojExportFolderPath;
            
            ZipFile.ExtractToDirectory(zipArchivePath, knxprojExportFolderPath);
            File.Delete(zipArchivePath);
            return true;
        }
        catch (NotSupportedException)
        {
            logger.ConsoleAndLogWriteLine("Error: The archive type is not supported.");
            return false;
        }
        catch (IOException ex)
        {
            logger.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.ConsoleAndLogWriteLine($"Unauthorized access extracting file {zipArchivePath}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            logger.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
            return false;
        }
    }

    
    
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
    public bool ExtractGroupAddressFile(string groupAddressesSourceFilePath)
    {
        var managedToExtractXml= false;
        var managedToNormalizePaths = false;
        var cancelOperation = false;

        // Tant que l'on n'a pas réussi à extraire le projet ou que l'on n'a pas demandé l'annulation de l'extraction.
        while (!managedToExtractXml && !cancelOperation)
        {
            /* ------------------------------------------------------------------------------------------------
            ---------------------------------------- GESTION DES PATH -----------------------------------------
            ------------------------------------------------------------------------------------------------ */

            // Répéter tant que l'on n'a pas réussi à normaliser les chemins d'accès ou que l'on n'a pas demandé
            // à annuler l'extraction
            string msg;

            while (!managedToNormalizePaths && !cancelOperation)
            {
                if (groupAddressesSourceFilePath.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                {
                    cancelOperation = true;
                    logger.ConsoleAndLogWriteLine("User cancelled the group addresses file extraction process.");
                    continue;
                }

                // On tente d'abord de normaliser l'adresse du fichier du projet
                try
                {
                    groupAddressesSourceFilePath =
                        Path.GetFullPath(groupAddressesSourceFilePath); // Normalisation de l'adresse du fichier du projet
                }
                catch (ArgumentException)
                {
                    // Si l'adresse du fichier du projet est vide
                    logger.ConsoleAndLogWriteLine(
                        "Error: the group addresses file source file path is empty. Please try selecting the file again.");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (PathTooLongException)
                {
                    // Si l'adresse du fichier du projet est trop longue
                    logger.ConsoleAndLogWriteLine(
                        $"Error: the path {groupAddressesSourceFilePath} is too long (more than 255 characters). " +
                        $"Please try selecting another path.");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    logger.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue;
                }

                managedToNormalizePaths = true;
            }

            
            logger.ConsoleAndLogWriteLine($"Extracting {Path.GetFileName(groupAddressesSourceFilePath)}...");
            
            var newFilePath =$"./{Path.GetFileName(groupAddressesSourceFilePath)}"; 
            
            // S'il existe déjà un fichier avec le même nom, le supprime
            if (File.Exists(newFilePath))
            {
                logger.ConsoleAndLogWriteLine(
                    $"{newFilePath} already exists. Removing the file before creating the new archive.");
                try
                {
                    File.Delete(newFilePath);
                }
                catch (IOException ex)
                {
                    logger.ConsoleAndLogWriteLine($"Error deleting existing file {newFilePath}: {ex.Message}");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    logger.ConsoleAndLogWriteLine($"Error deleting existing file {newFilePath}: {ex.Message}. " +
                                                   $"Please change the rights of the file so the program can delete {newFilePath}");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
            }
            
            try
            {
                // On essaie extrait le fichier dans le bon path
                File.Copy(groupAddressesSourceFilePath, newFilePath);
            }
            catch (FileNotFoundException)
            {
                // Si le fichier n'existe pas ou que le path est incorrect
                logger.ConsoleAndLogWriteLine(
                    $"Error: the file {groupAddressesSourceFilePath} was not found. Please check the selected file path and try again.");
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (UnauthorizedAccessException)
            {
                // Si le fichier n'est pas accessible en écriture
                msg = $"Unable to write to the file {groupAddressesSourceFilePath}. "
                      + "Please check that the program has access to the file or try running it "
                      + "as an administrator.";
                logger.ConsoleAndLogWriteLine(msg);
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (DirectoryNotFoundException)
            {
                // Si le dossier destination n'a pas été trouvé
                msg = $"The folder {Path.GetDirectoryName(groupAddressesSourceFilePath)} cannot be found. "
                      + "Please check the entered path and try again.";
                logger.ConsoleAndLogWriteLine(msg);
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (PathTooLongException)
            {
                // Si le chemin est trop long
                logger.ConsoleAndLogWriteLine(
                    $"Error: the path {groupAddressesSourceFilePath} is too long (more than 255 characters). Please try again.");
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                logger.ConsoleAndLogWriteLine(
                    $"Error copying file {groupAddressesSourceFilePath} to {newFilePath}: {ex.Message}");
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue;
            }
            
            logger.ConsoleAndLogWriteLine($"Done! Copy the file: {Path.GetFullPath(groupAddressesSourceFilePath)}");

            // On stocke le nouveau path d'exportation du projet
            GroupAddressFilePath = newFilePath;
                
            managedToExtractXml = true;

            // On stocke le nom du nouveau projet
            GroupAddressFileName = Path.GetFileNameWithoutExtension(groupAddressesSourceFilePath);
            ProjectName = GroupAddressFileName;
            
            App.WindowManager!.MainWindow.Title = settings.AppLang switch
            {
                // Arabe
                "AR" => $"الملف المستورد :  {GroupAddressFileName}",
                // Bulgare
                "BG" => $"Импортиран файл: {GroupAddressFileName}",
                // Tchèque
                "CS" => $"Importovaný soubor : {GroupAddressFileName}",
                // Danois
                "DA" => $"Importeret fil : {GroupAddressFileName}",
                // Allemand
                "DE" => $"Importierte Datei : {GroupAddressFileName}",
                // Grec
                "EL" => $"Εισαγόμενο αρχείο : {GroupAddressFileName}",
                // Anglais
                "EN" => $"Imported file : {GroupAddressFileName}",
                // Espagnol
                "ES" => $"Fichero importado : {GroupAddressFileName}",
                // Estonien
                "ET" => $"Imporditud fail : {GroupAddressFileName}",
                // Finnois
                "FI" => $"Tuotu tiedosto : {GroupAddressFileName}",
                // Hongrois
                "HU" => $"Importált fájl : {GroupAddressFileName}",
                // Indonésien
                "ID" => $"File yang diimpor : {GroupAddressFileName}",
                // Italien
                "IT" => $"File importato : {GroupAddressFileName}",
                // Japonais
                "JA" => $"インポートされたファイル：{GroupAddressFileName}",
                // Coréen
                "KO" => $"가져온 파일 : {GroupAddressFileName}",
                // Letton
                "LV" => $"Importētais fails : {GroupAddressFileName}",
                // Lituanien
                "LT" => $"Importuotas failas : {GroupAddressFileName}",
                // Norvégien
                "NB" => $"Importert fil : {GroupAddressFileName}",
                // Néerlandais
                "NL" => $"Geïmporteerd bestand : {GroupAddressFileName}",
                // Polonais
                "PL" => $"Zaimportowany plik : {GroupAddressFileName}",
                // Portugais
                "PT" => $"Fișier importat : {GroupAddressFileName}",
                // Roumain
                "RO" => $"Proiect importat: {GroupAddressFileName}",
                // Russe
                "RU" => $"Импортированный файл : {GroupAddressFileName}",
                // Slovaque
                "SK" => $"Importovaný súbor : {GroupAddressFileName}",
                // Slovène
                "SL" => $"Uvožena datoteka : {GroupAddressFileName}",
                // Suédois
                "SV" => $"Importerad fil : {GroupAddressFileName}",
                // Turc
                "TR" => $"İçe aktarılan dosya : {GroupAddressFileName}",
                // Ukrainien
                "UK" => $"Імпортований файл : {GroupAddressFileName}",
                // Chinois simplifié
                "ZH" => $"导入文件 ： {GroupAddressFileName}",
                // Cas par défaut (français)
                _ => $" Fichier importé : {GroupAddressFileName}"
            };
        }

        return !cancelOperation && managedToExtractXml;
    }
    
    
    // Fonction permettant de demander à l'utilisateur d'entrer un path
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
    public string SelectAnotherFile()
    {
        try
        {
            // Créer une nouvelle instance de OpenFileDialog
            var openFileDialog = new OpenFileDialog();

            if (App.WindowManager != null && App.WindowManager.MainWindow.UserChooseToImportGroupAddressFile)
            {
                // Définir des propriétés pour les fichiers XML
                openFileDialog.Title = "Sélectionnez un fichier d'adresses de groupe à importer";
                openFileDialog.Filter = "Fichiers d'adresses de groupes|*.xml|Tous les fichiers|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
            }
            else
            {
                // Définir des propriétés pour les fichiers KNX
                openFileDialog.Title = "Sélectionnez un projet KNX à importer";
                openFileDialog.Filter = "ETS KNX Project File (*.knxproj)|*.knxproj|Tous les fichiers|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
            }

            // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Récupérer le chemin du fichier sélectionné
                return openFileDialog.FileName;
            }
            else
            {
                return "";
            }
        }
        catch (InvalidOperationException ex)
        {
            // Gérer les exceptions liées à l'état non valide de l'OpenFileDialog
            logger.ConsoleAndLogWriteLine($"Error: Could not open file dialog. Details: {ex.Message}");
        }
        catch (System.Runtime.InteropServices.ExternalException ex)
        {
            // Gérer les exceptions liées aux erreurs internes des bibliothèques de l'OS
            logger.ConsoleAndLogWriteLine(
                $"Error: An external error occurred while trying to open the file dialog. Details: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Gérer toutes autres exceptions génériques
            logger.ConsoleAndLogWriteLine($"Error: An unexpected error occurred. Details: {ex.Message}");
        }

        return "";
    }
}