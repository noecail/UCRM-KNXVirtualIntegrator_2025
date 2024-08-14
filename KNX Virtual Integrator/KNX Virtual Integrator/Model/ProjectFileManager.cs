using System.IO;
using System.IO.Compression;
using System.Windows;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator.Model;

public class ProjectFileManager
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
    public static string ProjectFolderPath { get; private set; } = ""; // Chemin d'accès au dossier exporté du projet
    
    
    /// <summary>
    ///  Gets the name of the project the application is currently working on.
    /// </summary>
    public static string ProjectName { get; private set; } = "";
    
    
    /// <summary>
    /// Gets the path to the 0.xml file of the project.
    /// </summary>
    /// <remarks>
    /// This property holds the file path to the 0.xml file associated with the project.
    /// </remarks>
    public static string ZeroXmlPath { get; internal set; } = ""; // Chemin d'accès au fichier 0.xml du projet
    
    
    /// <summary>
    /// Gets the path to the exported of the group addresses file.
    /// </summary>
    /// <remarks>
    /// This property holds the file path  of the group addresses file
    /// </remarks>
    public static string GroupAddressFilePath { get; private set; } = ""; // Chemin d'accès au dossier exporté du projet
    
    
    /// <summary>
    ///  Gets the name of the group addresses file the application is currently working on.
    /// </summary>
    public static string GroupAddressFileName { get; private set; } = "";
    
    
    
    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Fonction permettant de récupérer le contenu de l'archive .knxproj situé à knxprojSourcePath et de le placer dans le dossier knxprojExportPath
    /// <summary>
    /// Extracts the contents of a .knxproj archive located at the specified path and places them into the designated export folder.
    /// </summary>
    /// <param name="knxprojSourceFilePath">The path to the .knxproj file that will be extracted.</param>
    /// <returns>Returns <c>true</c> if the project files were successfully extracted and the process was not cancelled; otherwise, returns <c>false</c>.</returns>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Normalizes the path of the .knxproj file and handles potential path-related exceptions.</item>
    /// <item>Converts the .knxproj file into a .zip archive if the file type is valid.</item>
    /// <item>Deletes any existing export folders or zip files to avoid conflicts.</item>
    /// <item>Attempts to extract the .zip archive into the export folder, handling extraction-related exceptions.</item>
    /// <item>Checks for password-protected project files and notifies the user if the project is locked.</item>
    /// <item>Sets the project folder path and indicates successful extraction if no cancellation occurred.</item>
    /// </list>
    /// </remarks>
    public static bool ExtractProjectFiles(string knxprojSourceFilePath)
    {
        var managedToExtractProject = false;
        var managedToNormalizePaths = false;
        var cancelOperation = false;

        // Tant que l'on n'a pas réussi à extraire le projet ou que l'on n'a pas demandé l'annulation de l'extraction
        while (!managedToExtractProject && !cancelOperation)
        {
            /* ------------------------------------------------------------------------------------------------
            ---------------------------------------- GESTION DES PATH -----------------------------------------
            ------------------------------------------------------------------------------------------------ */

            // Répéter tant que l'on n'a pas réussi à normaliser les chemins d'accès ou que l'on n'a pas demandé
            // à annuler l'extraction
            string msg;

            while (!managedToNormalizePaths && !cancelOperation)
            {
                if (knxprojSourceFilePath.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                {
                    cancelOperation = true;
                    Logger.ConsoleAndLogWriteLine("User cancelled the project extraction process.");
                    continue;
                }

                // On tente d'abord de normaliser l'adresse du fichier du projet
                try
                {
                    knxprojSourceFilePath =
                        Path.GetFullPath(knxprojSourceFilePath); // Normalisation de l'adresse du fichier du projet
                }
                catch (ArgumentException)
                {
                    // Si l'adresse du fichier du projet est vide
                    Logger.ConsoleAndLogWriteLine(
                        "Error: the .knxproj source file path is empty. Please try selecting the file again.");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (PathTooLongException)
                {
                    // Si l'adresse du fichier du projet est trop longue
                    Logger.ConsoleAndLogWriteLine(
                        $"Error: the path {knxprojSourceFilePath} is too long (more than 255 characters). " +
                        $"Please try selecting another path.");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    Logger.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue;
                }

                managedToNormalizePaths = true;
            }

            /* ------------------------------------------------------------------------------------------------
            ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
            ------------------------------------------------------------------------------------------------ */

            Logger.ConsoleAndLogWriteLine($"Starting to extract {Path.GetFileName(knxprojSourceFilePath)}...");

            string
                zipArchivePath; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)
            var knxprojExportFolderPath =
                $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/knxproj_exported/";

            // Transformation du knxproj en zip
            if (knxprojSourceFilePath.EndsWith(".knxproj"))
            {
                // Si le fichier entré est un .knxproj
                zipArchivePath =
                    $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}.zip"; // On enlève .knxproj et on ajoute .zip
            }
            else
            {
                // Sinon, ce n'est pas le type de fichier que l'on veut
                msg = "Error: the selected file is not a .knxproj file. "
                      + "Please try again. To obtain a .knxproj file, "
                      + "please head into the ETS app and click the \"Export Project\" button.";
                Logger.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }

            if (File.Exists(zipArchivePath))
            {
                Logger.ConsoleAndLogWriteLine(
                    $"{zipArchivePath} already exists. Removing the file before creating the new archive.");
                try
                {
                    File.Delete(zipArchivePath);
                }
                catch (IOException ex)
                {
                    Logger.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    Logger.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}. " +
                                                  $"Please change the rights of the file so the program can delete {zipArchivePath}");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
            }

            try
            {
                // On essaie de transformer le fichier .knxproj en archive .zip
                File.Copy(knxprojSourceFilePath, zipArchivePath);
            }
            catch (FileNotFoundException)
            {
                // Si le fichier n'existe pas ou que le path est incorrect
                Logger.ConsoleAndLogWriteLine(
                    $"Error: the file {knxprojSourceFilePath} was not found. Please check the selected file path and try again.");
                knxprojSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (UnauthorizedAccessException)
            {
                // Si le fichier n'est pas accessible en écriture
                msg = $"Unable to write to the file {knxprojSourceFilePath}. "
                      + "Please check that the program has access to the file or try running it "
                      + "as an administrator.";
                Logger.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (DirectoryNotFoundException)
            {
                // Si le dossier destination n'a pas été trouvé
                msg = $"The folder {Path.GetDirectoryName(knxprojSourceFilePath)} cannot be found. "
                      + "Please check the entered path and try again.";
                Logger.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (PathTooLongException)
            {
                // Si le chemin est trop long
                Logger.ConsoleAndLogWriteLine(
                    $"Error: the path {knxprojSourceFilePath} is too long (more than 255 characters). Please try again.");
                knxprojSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                Logger.ConsoleAndLogWriteLine(
                    $"Error copying file {knxprojSourceFilePath} to {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }


            // Si le dossier d'exportation existe déjà, on le supprime pour laisser place au nouveau
            if (Path.Exists(knxprojExportFolderPath))
            {
                try
                {
                    Logger.ConsoleAndLogWriteLine($"The folder {knxprojExportFolderPath} already exists, deleting...");
                    Directory.Delete(knxprojExportFolderPath, true);
                }
                catch (IOException ex)
                {
                    Logger.ConsoleAndLogWriteLine(
                        $"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    Logger.ConsoleAndLogWriteLine(
                        $"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}" +
                        $"Please change the rights of the file so the program can delete {zipArchivePath}");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    Logger.ConsoleAndLogWriteLine($"Error deleting folder {knxprojExportFolderPath}: {ex.Message}");
                    knxprojSourceFilePath = SelectAnotherFile();
                    continue;
                }
            }


            // Si le fichier a bien été transformé en zip, tentative d'extraction
            try
            {
                ZipFile.ExtractToDirectory(zipArchivePath,
                    knxprojExportFolderPath); // On extrait le zip
                File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
            }
            catch (NotSupportedException)
            {
                // Si le type d'archive n'est pas supporté
                msg =
                    $"Error: The archive type of the file {Path.GetFileName(knxprojSourceFilePath)} is not supported. "
                    + "Please check that the file is not corrupted. \nIf necessary, please export your "
                    + "ETS project again and try to extract it.";
                Logger.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }
            catch (IOException ex)
            {
                // Gestion des erreurs d'entrée/sortie générales
                Logger.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Si l'accès aux fichiers ou aux répertoires n'est pas autorisé
                Logger.ConsoleAndLogWriteLine($"Unauthorized access extracting file {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                Logger.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }


            /* ------------------------------------------------------------------------------------------------
            -------------------------------- GESTION DES PROJETS KNX PROTEGES ---------------------------------
            ------------------------------------------------------------------------------------------------ */

            // S'il existe un fichier P-XXXX.zip, alors le projet est protégé par un mot de passe
            try
            {
                if (Directory.GetFiles(knxprojExportFolderPath, "P-*.zip", SearchOption.TopDirectoryOnly).Length >
                    0)
                {
                    Logger.ConsoleAndLogWriteLine(
                        $"Encountered an error while extracting {knxprojSourceFilePath} : the project is locked with a password in ETS6");
                        
                    var messageBoxText = App.WindowManager?.SettingsWindow!.AppLang switch
                    {
                        // Arabe
                        "AR" => "خطأ: المشروع الذي اخترته محمي بكلمة مرور ولا يمكن تشغيله. يرجى إلغاء قفله في ETS والمحاولة مرة أخرى.",
                        // Bulgare
                        "BG" => "Грешка: Избраният проект е защитен с парола и не може да бъде опериран. Моля, отключете го в ETS и опитайте отново.",
                        // Tchèque
                        "CS" => "Chyba: Vybraný projekt je chráněn heslem a nelze s ním pracovat. Odemkněte jej prosím v ETS a zkuste to znovu.",
                        // Danois
                        "DA" => "Fejl: Det valgte projekt er adgangskodebeskyttet og kan ikke betjenes. Lås det op i ETS og prøv igen.",
                        // Allemand
                        "DE" => "Fehler: Das ausgewählte Projekt ist passwortgeschützt und kann nicht bearbeitet werden. Bitte entsperren Sie es in ETS und versuchen Sie es erneut.",
                        // Grec
                        "EL" => "Σφάλμα: Το επιλεγμένο έργο είναι προστατευμένο με κωδικό πρόσβασης και δεν μπορεί να λειτουργήσει. Ξεκλειδώστε το στο ETS και δοκιμάστε ξανά.",
                        // Anglais
                        "EN" => "Error: The project you have selected is password-protected and cannot be operated. Please unlock it in ETS and try again.",
                        // Espagnol
                        "ES" => "Error: El proyecto que ha seleccionado está protegido con contraseña y no se puede operar. Desbloquéelo en ETS e inténtelo de nuevo.",
                        // Estonien
                        "ET" => "Viga: Valitud projekt on parooliga kaitstud ja seda ei saa kasutada. Palun vabastage see ETS-is ja proovige uuesti.",
                        // Finnois
                        "FI" => "Virhe: Valitsemasi projekti on suojattu salasanalla eikä sitä voi käyttää. Avaa se ETS:ssä ja yritä uudelleen.",
                        // Hongrois
                        "HU" => "Hiba: A kiválasztott projekt jelszóval védett és nem használható. Kérjük, oldja fel az ETS-ben és próbálja újra.",
                        // Indonésien
                        "ID" => "Kesalahan: Proyek yang Anda pilih dilindungi kata sandi dan tidak dapat dioperasikan. Silakan buka kuncinya di ETS dan coba lagi.",
                        // Italien
                        "IT" => "Errore: Il progetto selezionato è protetto da password e non può essere operato. Sbloccalo in ETS e riprova.",
                        // Japonais
                        "JA" => "エラー: 選択したプロジェクトはパスワードで保護されており、操作できません。 ETSでロックを解除して再試行してください。",
                        // Coréen
                        "KO" => "오류: 선택한 프로젝트는 비밀번호로 보호되어 있으며 작동할 수 없습니다. ETS에서 잠금을 해제하고 다시 시도하십시오.",
                        // Letton
                        "LV" => "Kļūda: Izvēlētais projekts ir aizsargāts ar paroli un to nevar darbināt. Lūdzu, atbloķējiet to ETS un mēģiniet vēlreiz.",
                        // Lituanien
                        "LT" => "Klaida: Pasirinktas projektas yra apsaugotas slaptažodžiu ir jo negalima valdyti. Atrakinkite jį ETS ir bandykite dar kartą.",
                        // Norvégien
                        "NB" => "Feil: Prosjektet du har valgt er passordbeskyttet og kan ikke betjenes. Lås det opp i ETS og prøv igjen.",
                        // Néerlandais
                        "NL" => "Fout: Het geselecteerde project is met een wachtwoord beveiligd en kan niet worden bediend. Ontgrendel het in ETS en probeer het opnieuw.",
                        // Polonais
                        "PL" => "Błąd: Wybrany projekt jest chroniony hasłem i nie można nim operować. Odblokuj go w ETS i spróbuj ponownie.",
                        // Portugais
                        "PT" => "Erro: O projeto selecionado está protegido por senha e não pode ser operado. Desbloqueie-o no ETS e tente novamente.",
                        // Roumain
                        "RO" => "Eroare: Proiectul selectat este protejat prin parolă și nu poate fi operat. Vă rugăm să-l deblocați în ETS și încercați din nou.",
                        // Russe
                        "RU" => "Ошибка: Выбранный проект защищен паролем и не может быть использован. Пожалуйста, разблокируйте его в ETS и попробуйте снова.",
                        // Slovaque
                        "SK" => "Chyba: Vybraný projekt je chránený heslom a nie je možné s ním pracovať. Odomknite ho v ETS a skúste to znova.",
                        // Slovène
                        "SL" => "Napaka: Izbrani projekt je zaščiten z geslom in ga ni mogoče uporabljati. Odklenite ga v ETS in poskusite znova.",
                        // Suédois
                        "SV" => "Fel: Projektet du har valt är lösenordsskyddat och kan inte användas. Lås upp det i ETS och försök igen.",
                        // Turc
                        "TR" => "Hata: Seçtiğiniz proje parola korumalıdır ve çalıştırılamaz. Lütfen ETS'de kilidini açın ve tekrar deneyin.",
                        // Ukrainien
                        "UK" => "Помилка: Вибраний проект захищений паролем і не може бути використаний. Будь ласка, розблокуйте його в ETS і спробуйте знову.",
                        // Chinois simplifié
                        "ZH" => "错误：您选择的项目受密码保护，无法操作。 请在 ETS 中解锁并重试。",
                        // Cas par défaut (français)
                        _ => "Erreur : Le projet que vous avez sélectionné est protégé par mot de passe et ne peut pas être opéré. Veuillez le déverrouiller dans ETS et réessayez."
                    };

                    var caption = App.WindowManager?.SettingsWindow!.AppLang switch
                    {
                        // Arabe
                        "AR" => "خطأ",
                        // Bulgare
                        "BG" => "Грешка",
                        // Tchèque
                        "CS" => "Chyba",
                        // Danois
                        "DA" => "Fejl",
                        // Allemand
                        "DE" => "Fehler",
                        // Grec
                        "EL" => "Σφάλμα",
                        // Anglais
                        "EN" => "Error",
                        // Espagnol
                        "ES" => "Error",
                        // Estonien
                        "ET" => "Viga",
                        // Finnois
                        "FI" => "Virhe",
                        // Hongrois
                        "HU" => "Hiba",
                        // Indonésien
                        "ID" => "Kesalahan",
                        // Italien
                        "IT" => "Errore",
                        // Japonais
                        "JA" => "エラー",
                        // Coréen
                        "KO" => "오류",
                        // Letton
                        "LV" => "Kļūda",
                        // Lituanien
                        "LT" => "Klaida",
                        // Norvégien
                        "NB" => "Feil",
                        // Néerlandais
                        "NL" => "Fout",
                        // Polonais
                        "PL" => "Błąd",
                        // Portugais
                        "PT" => "Erro",
                        // Roumain
                        "RO" => "Eroare",
                        // Russe
                        "RU" => "Ошибка",
                        // Slovaque
                        "SK" => "Chyba",
                        // Slovène
                        "SL" => "Napaka",
                        // Suédois
                        "SV" => "Fel",
                        // Turc
                        "TR" => "Hata",
                        // Ukrainien
                        "UK" => "Помилка",
                        // Chinois simplifié
                        "ZH" => "错误",
                        // Cas par défaut (français)
                        _ => "Erreur"
                    };

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                        
                    cancelOperation = true;
                    continue;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Gestion des erreurs d'accès non autorisé
                Logger.ConsoleAndLogWriteLine(
                    $"Unauthorized access checking for protected project files: {ex.Message}.");
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                Logger.ConsoleAndLogWriteLine($"Error checking for protected project files: {ex.Message}");
                knxprojSourceFilePath = SelectAnotherFile();
                continue;
            }


            /* ------------------------------------------------------------------------------------------------
            ------------------------------- SUPPRESSION DES FICHIERS RESIDUELS --------------------------------
            ------------------------------------------------------------------------------------------------ */

            // Suppression du fichier zip temporaire
            Logger.ConsoleAndLogWriteLine($"Done! New folder created: {Path.GetFullPath(knxprojExportFolderPath)}");

            // On stocke le nouveau path d'exportation du projet
            ProjectFolderPath = $@"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/";
                
            managedToExtractProject = true;

            // On stocke le nom du nouveau projet
            ProjectName = Path.GetFileNameWithoutExtension(knxprojSourceFilePath);
                
            App.WindowManager!.MainWindow.Title = App.WindowManager.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => $"المشروع المستورد: {ProjectName}",
                // Bulgare
                "BG" => $"Импортиран проект: {ProjectName}",
                // Tchèque
                "CS" => $"Importovaný projekt: {ProjectName}",
                // Danois
                "DA" => $"Importerede projekt: {ProjectName}",
                // Allemand
                "DE" => $"Importiertes Projekt: {ProjectName}",
                // Grec
                "EL" => $"Εισαγόμενο έργο: {ProjectName}",
                // Anglais
                "EN" => $"Imported Project: {ProjectName}",
                // Espagnol
                "ES" => $"Proyecto importado: {ProjectName}",
                // Estonien
                "ET" => $"Imporditud projekt: {ProjectName}",
                // Finnois
                "FI" => $"Tuotu projekti: {ProjectName}",
                // Hongrois
                "HU" => $"Importált projekt: {ProjectName}",
                // Indonésien
                "ID" => $"Proyek yang diimpor: {ProjectName}",
                // Italien
                "IT" => $"Progetto importato: {ProjectName}",
                // Japonais
                "JA" => $"インポートされたプロジェクト: {ProjectName}",
                // Coréen
                "KO" => $"가져온 프로젝트: {ProjectName}",
                // Letton
                "LV" => $"Importēts projekts: {ProjectName}",
                // Lituanien
                "LT" => $"Importuotas projektas: {ProjectName}",
                // Norvégien
                "NB" => $"Importert prosjekt: {ProjectName}",
                // Néerlandais
                "NL" => $"Geïmporteerd project: {ProjectName}",
                // Polonais
                "PL" => $"Zaimportowany projekt: {ProjectName}",
                // Portugais
                "PT" => $"Projeto importado: {ProjectName}",
                // Roumain
                "RO" => $"Proiect importat: {ProjectName}",
                // Russe
                "RU" => $"Импортированный проект: {ProjectName}",
                // Slovaque
                "SK" => $"Importovaný projekt: {ProjectName}",
                // Slovène
                "SL" => $"Uvožen projekt: {ProjectName}",
                // Suédois
                "SV" => $"Importerade projekt: {ProjectName}",
                // Turc
                "TR" => $"İçe aktarılan proje: {ProjectName}",
                // Ukrainien
                "UK" => $"Імпортований проект: {ProjectName}",
                // Chinois simplifié
                "ZH" => $"导入项目: {ProjectName}",
                // Cas par défaut (français)
                _ => $"Projet importé : {ProjectName}"
            };
        }

        return !cancelOperation && managedToExtractProject;
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
    public static bool ExtractGroupAddressFile(string groupAddressesSourceFilePath)
    {
        var managedToExtractXml= false;
        var managedToNormalizePaths = false;
        var cancelOperation = false;

        // Tant que l'on n'a pas réussi à extraire le projet ou que l'on n'a pas demandé l'annulation de l'extraction
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
                    Logger.ConsoleAndLogWriteLine("User cancelled the group addresses file extraction process.");
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
                    Logger.ConsoleAndLogWriteLine(
                        "Error: the group addresses file source file path is empty. Please try selecting the file again.");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (PathTooLongException)
                {
                    // Si l'adresse du fichier du projet est trop longue
                    Logger.ConsoleAndLogWriteLine(
                        $"Error: the path {groupAddressesSourceFilePath} is too long (more than 255 characters). " +
                        $"Please try selecting another path.");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    Logger.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue;
                }

                managedToNormalizePaths = true;
            }

            
            Logger.ConsoleAndLogWriteLine($"Extracting {Path.GetFileName(groupAddressesSourceFilePath)}...");
            
            var newFilePath =$"./{Path.GetFileName(groupAddressesSourceFilePath)}"; 
            
            // S'il existe déjà un fichier avec le même nom, le supprime
            if (File.Exists(newFilePath))
            {
                Logger.ConsoleAndLogWriteLine(
                    $"{newFilePath} already exists. Removing the file before creating the new archive.");
                try
                {
                    File.Delete(newFilePath);
                }
                catch (IOException ex)
                {
                    Logger.ConsoleAndLogWriteLine($"Error deleting existing file {newFilePath}: {ex.Message}");
                    groupAddressesSourceFilePath = SelectAnotherFile();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    Logger.ConsoleAndLogWriteLine($"Error deleting existing file {newFilePath}: {ex.Message}. " +
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
                Logger.ConsoleAndLogWriteLine(
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
                Logger.ConsoleAndLogWriteLine(msg);
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (DirectoryNotFoundException)
            {
                // Si le dossier destination n'a pas été trouvé
                msg = $"The folder {Path.GetDirectoryName(groupAddressesSourceFilePath)} cannot be found. "
                      + "Please check the entered path and try again.";
                Logger.ConsoleAndLogWriteLine(msg);
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (PathTooLongException)
            {
                // Si le chemin est trop long
                Logger.ConsoleAndLogWriteLine(
                    $"Error: the path {groupAddressesSourceFilePath} is too long (more than 255 characters). Please try again.");
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                Logger.ConsoleAndLogWriteLine(
                    $"Error copying file {groupAddressesSourceFilePath} to {newFilePath}: {ex.Message}");
                groupAddressesSourceFilePath = SelectAnotherFile();
                continue;
            }
            
            Logger.ConsoleAndLogWriteLine($"Done! Copy the file: {Path.GetFullPath(groupAddressesSourceFilePath)}");

            // On stocke le nouveau path d'exportation du projet
            GroupAddressFilePath = newFilePath;
                
            managedToExtractXml = true;

            // On stocke le nom du nouveau projet
            GroupAddressFileName = Path.GetFileNameWithoutExtension(groupAddressesSourceFilePath);
                
            App.WindowManager!.MainWindow.Title = App.WindowManager.SettingsWindow!.AppLang switch
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
    private static string SelectAnotherFile()
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
            Logger.ConsoleAndLogWriteLine($"Error: Could not open file dialog. Details: {ex.Message}");
        }
        catch (System.Runtime.InteropServices.ExternalException ex)
        {
            // Gérer les exceptions liées aux erreurs internes des bibliothèques de l'OS
            Logger.ConsoleAndLogWriteLine(
                $"Error: An external error occurred while trying to open the file dialog. Details: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Gérer toutes autres exceptions génériques
            Logger.ConsoleAndLogWriteLine($"Error: An unexpected error occurred. Details: {ex.Message}");
        }

        return "";
    }
}