using System.IO;
using System.IO.Compression;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator;

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
    public string ProjectFolderPath { get; private set; } = ""; // Chemin d'accès au dossier exporté du projet
    
    /// <summary>
    ///  Gets the name of the project the application is currently working on.
    /// </summary>
    public string ProjectName { get; private set; } = "";
    
    
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
    public bool ExtractProjectFiles(string knxprojSourceFilePath)
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
                    App.ConsoleAndLogWriteLine("User cancelled the project extraction process.");
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
                    App.ConsoleAndLogWriteLine(
                        "Error: the .knxproj source file path is empty. Please try selecting the file again.");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (PathTooLongException)
                {
                    // Si l'adresse du fichier du projet est trop longue
                    App.ConsoleAndLogWriteLine(
                        $"Error: the path {knxprojSourceFilePath} is too long (more than 255 characters). " +
                        $"Please try selecting another path.");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    App.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }

                managedToNormalizePaths = true;
            }

            /* ------------------------------------------------------------------------------------------------
            ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
            ------------------------------------------------------------------------------------------------ */

            App.ConsoleAndLogWriteLine($"Starting to extract {Path.GetFileName(knxprojSourceFilePath)}...");

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
                App.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }

            if (File.Exists(zipArchivePath))
            {
                App.ConsoleAndLogWriteLine(
                    $"{zipArchivePath} already exists. Removing the file before creating the new archive.");
                try
                {
                    File.Delete(zipArchivePath);
                }
                catch (IOException ex)
                {
                    App.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    App.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}. " +
                                               $"Please change the rights of the file so the program can delete {zipArchivePath}");
                    knxprojSourceFilePath = AskForPath();
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
                App.ConsoleAndLogWriteLine(
                    $"Error: the file {knxprojSourceFilePath} was not found. Please check the selected file path and try again.");
                knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (UnauthorizedAccessException)
            {
                // Si le fichier n'est pas accessible en écriture
                msg = $"Unable to write to the file {knxprojSourceFilePath}. "
                      + "Please check that the program has access to the file or try running it "
                      + "as an administrator.";
                App.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (DirectoryNotFoundException)
            {
                // Si le dossier destination n'a pas été trouvé
                msg = $"The folder {Path.GetDirectoryName(knxprojSourceFilePath)} cannot be found. "
                      + "Please check the entered path and try again.";
                App.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (PathTooLongException)
            {
                // Si le chemin est trop long
                App.ConsoleAndLogWriteLine(
                    $"Error: the path {knxprojSourceFilePath} is too long (more than 255 characters). Please try again.");
                knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                App.ConsoleAndLogWriteLine(
                    $"Error copying file {knxprojSourceFilePath} to {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = AskForPath();
                continue;
            }


            // Si le dossier d'exportation existe déjà, on le supprime pour laisser place au nouveau
            if (Path.Exists(knxprojExportFolderPath))
            {
                try
                {
                    App.ConsoleAndLogWriteLine($"The folder {knxprojExportFolderPath} already exists, deleting...");
                    Directory.Delete(knxprojExportFolderPath, true);
                }
                catch (IOException ex)
                {
                    App.ConsoleAndLogWriteLine(
                        $"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    App.ConsoleAndLogWriteLine(
                        $"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}" +
                        $"Please change the rights of the file so the program can delete {zipArchivePath}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    App.ConsoleAndLogWriteLine($"Error deleting folder {knxprojExportFolderPath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
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
                App.ConsoleAndLogWriteLine(msg);
                knxprojSourceFilePath = AskForPath();
                continue;
            }
            catch (IOException ex)
            {
                // Gestion des erreurs d'entrée/sortie générales
                App.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = AskForPath();
                continue;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Si l'accès aux fichiers ou aux répertoires n'est pas autorisé
                App.ConsoleAndLogWriteLine($"Unauthorized access extracting file {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = AskForPath();
                continue;
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                App.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
                knxprojSourceFilePath = AskForPath();
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
                    App.ConsoleAndLogWriteLine(
                        $"Encountered an error while extracting {knxprojSourceFilePath} : the project is locked with a password in ETS6");
                        
                    /*var messageBoxText = App.DisplayElements?.SettingsWindow!.AppLang switch
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

                    var caption = App.DisplayElements?.SettingsWindow!.AppLang switch
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

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);*/
                        
                    cancelOperation = true;
                    continue;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Gestion des erreurs d'accès non autorisé
                App.ConsoleAndLogWriteLine(
                    $"Unauthorized access checking for protected project files: {ex.Message}.");
                knxprojSourceFilePath = AskForPath();
                continue;
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                App.ConsoleAndLogWriteLine($"Error checking for protected project files: {ex.Message}");
                knxprojSourceFilePath = AskForPath();
                continue;
            }


            /* ------------------------------------------------------------------------------------------------
            ------------------------------- SUPPRESSION DES FICHIERS RESIDUELS --------------------------------
            ------------------------------------------------------------------------------------------------ */

            // Suppression du fichier zip temporaire
            App.ConsoleAndLogWriteLine($"Done! New folder created: {Path.GetFullPath(knxprojExportFolderPath)}");

            // On stocke le nouveau path d'exportation du projet
            ProjectFolderPath = $@"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/";
                
            managedToExtractProject = true;

            // On stocke le nom du nouveau projet
            ProjectName = Path.GetFileNameWithoutExtension(knxprojSourceFilePath);
                
            /*App.DisplayElements!.MainWindow.Title = App.DisplayElements.SettingsWindow!.AppLang switch
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
            };*/
        }

        return !cancelOperation && managedToExtractProject;
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
    private static string AskForPath()
    {
        try
        {
            // Créer une nouvelle instance de OpenFileDialog
            var openFileDialog = new OpenFileDialog
            {
                // Définir des propriétés optionnelles
                Title = "Sélectionnez un projet KNX à importer",
                Filter = "ETS KNX Project File (*.knxproj)|*.knxproj|other file|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

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
            App.ConsoleAndLogWriteLine($"Error: Could not open file dialog. Details: {ex.Message}");
        }
        catch (System.Runtime.InteropServices.ExternalException ex)
        {
            // Gérer les exceptions liées aux erreurs internes des bibliothèques de l'OS
            App.ConsoleAndLogWriteLine(
                $"Error: An external error occurred while trying to open the file dialog. Details: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Gérer toutes autres exceptions génériques
            App.ConsoleAndLogWriteLine($"Error: An unexpected error occurred. Details: {ex.Message}");
        }

        return "";
    }

}