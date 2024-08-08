﻿using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
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
    
    /// <summary>
    /// Gets the path to the 0.xml file of the project.
    /// </summary>
    /// <remarks>
    /// This property holds the file path to the 0.xml file associated with the project.
    /// </remarks>
    public string ZeroXmlPath { get; private set; } = ""; // Chemin d'accès au fichier 0.xml du projet
    
    /// <summary>
    /// Gets the path to the exported of the group addresses file.
    /// </summary>
    /// <remarks>
    /// This property holds the file path  of the group addresses file
    /// </remarks>
    public string GroupAddressFilePath { get; private set; } = ""; // Chemin d'accès au dossier exporté du projet
    
    /// <summary>
    ///  Gets the name of the group addresses file the application is currently working on.
    /// </summary>
    public string GroupAddressFileName { get; private set; } = "";
    
    
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
                        
                    var messageBoxText = App.DisplayElements?.SettingsWindow!.AppLang switch
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

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                        
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
                
            App.DisplayElements!.MainWindow.Title = App.DisplayElements.SettingsWindow!.AppLang switch
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
    public bool ExtractGroupAddressFile(string groupAddressesSourceFilePath)
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
                    App.ConsoleAndLogWriteLine("User cancelled the group addresses file extraction process.");
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
                    App.ConsoleAndLogWriteLine(
                        "Error: the group addresses file source file path is empty. Please try selecting the file again.");
                    groupAddressesSourceFilePath = AskForPath();
                    continue;
                }
                catch (PathTooLongException)
                {
                    // Si l'adresse du fichier du projet est trop longue
                    App.ConsoleAndLogWriteLine(
                        $"Error: the path {groupAddressesSourceFilePath} is too long (more than 255 characters). " +
                        $"Please try selecting another path.");
                    groupAddressesSourceFilePath = AskForPath();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    App.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
                    groupAddressesSourceFilePath = AskForPath();
                    continue;
                }

                managedToNormalizePaths = true;
            }

            
            App.ConsoleAndLogWriteLine($"Extracting {Path.GetFileName(groupAddressesSourceFilePath)}...");
            
            var newFilePath =$"./{Path.GetFileName(groupAddressesSourceFilePath)}"; 
            
            // S'il existe déjà un fichier avec le même nom, le supprime
            if (File.Exists(newFilePath))
            {
                App.ConsoleAndLogWriteLine(
                    $"{newFilePath} already exists. Removing the file before creating the new archive.");
                try
                {
                    File.Delete(newFilePath);
                }
                catch (IOException ex)
                {
                    App.ConsoleAndLogWriteLine($"Error deleting existing file {newFilePath}: {ex.Message}");
                    groupAddressesSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si on n'a pas les droits de supprimer le fichier
                    App.ConsoleAndLogWriteLine($"Error deleting existing file {newFilePath}: {ex.Message}. " +
                                               $"Please change the rights of the file so the program can delete {newFilePath}");
                    groupAddressesSourceFilePath = AskForPath();
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
                App.ConsoleAndLogWriteLine(
                    $"Error: the file {groupAddressesSourceFilePath} was not found. Please check the selected file path and try again.");
                groupAddressesSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (UnauthorizedAccessException)
            {
                // Si le fichier n'est pas accessible en écriture
                msg = $"Unable to write to the file {groupAddressesSourceFilePath}. "
                      + "Please check that the program has access to the file or try running it "
                      + "as an administrator.";
                App.ConsoleAndLogWriteLine(msg);
                groupAddressesSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (DirectoryNotFoundException)
            {
                // Si le dossier destination n'a pas été trouvé
                msg = $"The folder {Path.GetDirectoryName(groupAddressesSourceFilePath)} cannot be found. "
                      + "Please check the entered path and try again.";
                App.ConsoleAndLogWriteLine(msg);
                groupAddressesSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (PathTooLongException)
            {
                // Si le chemin est trop long
                App.ConsoleAndLogWriteLine(
                    $"Error: the path {groupAddressesSourceFilePath} is too long (more than 255 characters). Please try again.");
                groupAddressesSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (Exception ex)
            {
                // Gestion générique des exceptions non prévues
                App.ConsoleAndLogWriteLine(
                    $"Error copying file {groupAddressesSourceFilePath} to {newFilePath}: {ex.Message}");
                groupAddressesSourceFilePath = AskForPath();
                continue;
            }
            
            App.ConsoleAndLogWriteLine($"Done! Copy the file: {Path.GetFullPath(groupAddressesSourceFilePath)}");

            // On stocke le nouveau path d'exportation du projet
            GroupAddressFilePath = newFilePath;
                
            managedToExtractXml = true;

            // On stocke le nom du nouveau projet
            GroupAddressFileName = Path.GetFileNameWithoutExtension(groupAddressesSourceFilePath);
                
            App.DisplayElements!.MainWindow.Title = App.DisplayElements.SettingsWindow!.AppLang switch
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
    private static string AskForPath()
    {
        try
        {
            // Créer une nouvelle instance de OpenFileDialog
            var openFileDialog = new OpenFileDialog();

            if (App.DisplayElements != null && App.DisplayElements.MainWindow.UserChooseToImportGroupAddressFile)
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
    private static string FindFile(string rootPath, string fileNameToSearch)
    {
        if (!Directory.Exists(rootPath))
        {
            App.ConsoleAndLogWriteLine($"Directory {rootPath} does not exist.");
            return "";
        }

        // Création d'une file d'attente pour les répertoires à explorer
        Queue<string> directoriesQueue = new Queue<string>();
        directoriesQueue.Enqueue(rootPath);

        while (directoriesQueue.Count > 0)
        {
            var currentDirectory = directoriesQueue.Dequeue();
            try
            {
                // Vérifier les fichiers dans le répertoire actuel
                string[] files = Directory.GetFiles(currentDirectory);
                foreach (var file in files)
                {
                    if (Path.GetFileName(file).Equals(fileNameToSearch, StringComparison.OrdinalIgnoreCase))
                    {
                        return file; // Fichier trouvé, on retourne son chemin
                    }
                }

                // Ajouter les sous-répertoires à la file d'attente
                string[] subDirectories = Directory.GetDirectories(currentDirectory);
                foreach (var subDirectory in subDirectories)
                {
                    directoriesQueue.Enqueue(subDirectory);
                }
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                // Si l'accès au répertoire est refusé
                App.ConsoleAndLogWriteLine($"Access refused to {currentDirectory} : {unAuthEx.Message}");
            }
            catch (DirectoryNotFoundException dirNotFoundEx)
            {
                // Si le répertoire est introuvable
                App.ConsoleAndLogWriteLine($"Directory not found : {currentDirectory} : {dirNotFoundEx.Message}");
            }
            catch (IOException ioEx)
            {
                // Si une erreur d'entrée/sortie survient
                App.ConsoleAndLogWriteLine($"I/O Error while accessing {currentDirectory} : {ioEx.Message}");
            }
            catch (Exception ex)
            {
                // Gérer toutes autres exceptions génériques
                App.ConsoleAndLogWriteLine(
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
    public async Task FindZeroXml()
    {
        try
        {
            var foundPath = FindFile(ProjectFolderPath, "0.xml");

            // Si le fichier n'a pas été trouvé
            if (string.IsNullOrEmpty(foundPath))
            {
                App.ConsoleAndLogWriteLine("Unable to find the file '0.xml' in the project folders. "
                                           + "Please ensure that the extracted archive is indeed a KNX ETS project.");
                // Utilisation de Dispatcher.Invoke pour fermer l'application depuis un thread non-UI
                await Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
            }
            else // Sinon
            {
                ZeroXmlPath = foundPath;
                App.ConsoleAndLogWriteLine($"Found '0.xml' file at {Path.GetFullPath(ZeroXmlPath)}.");
            }
        }
        catch (UnauthorizedAccessException unAuthEx)
        {
            // Gérer les erreurs d'accès non autorisé
            App.ConsoleAndLogWriteLine($"Access refused while searching for '0.xml': {unAuthEx.Message}");
        }
        catch (DirectoryNotFoundException dirNotFoundEx)
        {
            // Gérer les erreurs où le répertoire n'est pas trouvé
            App.ConsoleAndLogWriteLine($"Directory not found while searching for '0.xml': {dirNotFoundEx.Message}");
        }
        catch (IOException ioEx)
        {
            // Gérer les erreurs d'entrée/sortie
            App.ConsoleAndLogWriteLine($"I/O Error while searching for '0.xml': {ioEx.Message}");
        }
        catch (Exception ex)
        {
            // Gérer toutes autres exceptions génériques
            App.ConsoleAndLogWriteLine($"An unexpected error occurred while searching for '0.xml': {ex.Message}");
        }
    }
    
    /// <summary>
    /// Loads an XML document from a specified path.
    /// </summary>
    /// <param name="path">The path to the XML document to load.</param>
    /// <returns>Returns an XDocument if the file is successfully loaded; otherwise, returns null.</returns>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item>Attempts to load the XML document from the specified path.</item>
    /// <item>Catches and logs specific exceptions such as FileNotFoundException, DirectoryNotFoundException, IOException, UnauthorizedAccessException, and XmlException.</item>
    /// <item>Logs an error message and returns null if an exception is thrown.</item>
    /// </list>
    /// </remarks>
    public XDocument? LoadXmlDocument(string path)
    {
        try
        {
            return XDocument.Load(path);
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException ||
                                   ex is IOException || ex is UnauthorizedAccessException || ex is XmlException)
        {
            App.ConsoleAndLogWriteLine($"Error loading XML: {ex.Message}");
            return null;
        }
    }
    
    
    // Fonction générant les fichiers de débogage de l'application
    /// <summary>
    /// Generates a debug file for the application.
    /// Creates a debug directory if it does not exist and writes system, software, and hardware information
    /// to a file named "debugInfo.txt". System and hardware information are optional and can be included based on the parameters provided.
    /// </summary>
    /// <param name="includeOsInfo">Indicates whether to include operating system information. (Optional)</param>
    /// <param name="includeHardwareInfo">Indicates whether to include hardware information. (Optional)</param>
    private static void WriteSystemInformationDebugFile(bool includeOsInfo = true, bool includeHardwareInfo = true)
    {
        string filePath;

        try
        {
            // Créez le répertoire "./debug" s'il n'existe pas
            Directory.CreateDirectory("./debug");

            // Définir le chemin du fichier de sortie
            filePath = "./debug/debugInfo.txt";
        }
        catch (DirectoryNotFoundException)
        {
            App.ConsoleAndLogWriteLine("Error : could not find path './debug'. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }
        catch (IOException)
        {
            App.ConsoleAndLogWriteLine("Error : an I/O error occured while creating the folder './debug'. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }
        catch (UnauthorizedAccessException)
        {
            App.ConsoleAndLogWriteLine($"Error : the application cannot write to {Path.GetFullPath("./debug")}. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }
        catch (ArgumentException)
        {
            App.ConsoleAndLogWriteLine($"Error : could not create the directory {Path.GetFullPath("./debug")} because the path is too long, too small or contains illegal characters. The creation of the debug archive was aborted.");
            return;
        }
        catch (NotSupportedException)
        {
            App.ConsoleAndLogWriteLine($"Error: path {Path.GetFullPath("./debug")} is incorrect. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }

        try
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Si on choisit d'inclure les informations sur le système d'exploitation
                if (includeOsInfo)
                {
                    writer.WriteLine("--------------------------------------------------------------------");
                    writer.WriteLine("|                      INFORMATIONS MACHINE                        |");
                    writer.WriteLine("--------------------------------------------------------------------");

                    try
                    {
                        // Requête WMI pour obtenir les informations sur le système d'exploitation
                        var wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                        var searcher = new ManagementObjectSearcher(wql);
                        var results = searcher.Get();

                        foreach (var o in results)
                        {
                            var resultat = (ManagementObject)o;
                            // Obtenir le nom de l'OS
                            var osName = resultat["Caption"].ToString();
                            // Afficher les informations sur l'OS
                            writer.WriteLine($"Système d'exploitation : {osName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(
                            $"Système d'exploitation : Erreur lors de la récupération des informations sur l'OS : {ex.Message}");
                    }

                    writer.WriteLine($"Version de l'OS : {Environment.OSVersion}");
                    writer.WriteLine($"OS 64 bits ? {(Environment.Is64BitOperatingSystem ? "oui" : "non")}");
                    writer.WriteLine($"Dossier système: {Environment.SystemDirectory}");
                    writer.WriteLine();

                    writer.WriteLine(
                        $"Langue du système d'exploitation : {CultureInfo.CurrentCulture.DisplayName}");
                    writer.WriteLine(
                        $"Code de langue (ISO 639-1) : {CultureInfo.CurrentCulture.TwoLetterISOLanguageName}");
                    writer.WriteLine($"Nom de la culture : {CultureInfo.CurrentCulture.Name}");
                    writer.WriteLine();

                    writer.WriteLine($"Nom de la machine: {Environment.MachineName}");
                    writer.WriteLine();

                    writer.WriteLine($"Nom de domaine utilisateur: {Environment.UserDomainName}");
                    writer.WriteLine($"Nom d'utilisateur: {Environment.UserName}");
                    writer.WriteLine();
                }

                writer.WriteLine("--------------------------------------------------------------------");
                writer.WriteLine("|                      INFORMATIONS LOGICIEL                       |");
                writer.WriteLine("--------------------------------------------------------------------");

                writer.WriteLine($"Version .NET utilisée par le logiciel: {Environment.Version}");
                writer.WriteLine($"Logiciel exécuté depuis le dossier : {Environment.CurrentDirectory}");
                writer.WriteLine();

                writer.WriteLine(
                    $"Version de {App.AppName} : {App.AppVersion.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine($"Build de {App.AppName} : {App.AppBuild}");
                writer.WriteLine();

                writer.WriteLine($"ID du process: {Environment.ProcessId}");
                writer.WriteLine($"Process lancé en 64 bits ? {(Environment.Is64BitProcess ? "oui" : "non")}");
                writer.WriteLine(
                    $"Process lancé en mode administrateur ? {(Environment.IsPrivilegedProcess ? "oui" : "non")}");
                writer.WriteLine();

                // Affichage de la RAM utilisée par le logiciel
                double bytes = Environment.WorkingSet;
                var kilobytes = bytes / 1024;
                var megabytes = kilobytes / 1024;
                var gigabytes = megabytes / 1024;

                // Affichage dans différents formats
                writer.Write($"Mémoire vive utilisée par le logiciel : ");
                if (kilobytes >= 0.5 && megabytes <= 0.5) writer.WriteLine($"{kilobytes:0.00} Ko");
                else if (megabytes >= 0.5 && gigabytes <= 0.5) writer.WriteLine($"{megabytes:0.00} Mo");
                else if (gigabytes >= 0.5) writer.WriteLine($"{gigabytes:0.00} Go");
                else writer.WriteLine($"{bytes} octets");
                writer.WriteLine();

                // Si on choisit d'inclure les informations sur le matériel de l'ordinateur
                if (includeHardwareInfo)
                {
                    writer.WriteLine("--------------------------------------------------------------------");
                    writer.WriteLine("|                 INFORMATIONS SUR LE MATERIEL                     |");
                    writer.WriteLine("--------------------------------------------------------------------");

                    writer.WriteLine("-- Informations sur le processeur (CPU) --");
                    try
                    {
                        // Requête WMI pour obtenir les informations sur le processeur
                        var wql = new ObjectQuery("SELECT * FROM Win32_Processor");
                        var searcher = new ManagementObjectSearcher(wql);
                        var results = searcher.Get();

                        foreach (var o in results)
                        {
                            var resultaat = (ManagementObject)o;
                            writer.WriteLine($"Nom : {resultaat["Name"]}");
                            writer.WriteLine($"Fabricant : {resultaat["Manufacturer"]}");
                            writer.WriteLine($"Description : {resultaat["Description"]}");
                            writer.WriteLine($"Nombre de coeurs : {resultaat["NumberOfCores"]}");
                            writer.WriteLine(
                                $"Nombre de processeurs logiques : {resultaat["NumberOfLogicalProcessors"]}");
                            writer.WriteLine($"Vitesse d'horloge actuelle : {resultaat["CurrentClockSpeed"]} MHz");
                            writer.WriteLine($"Vitesse d'horloge maximale : {resultaat["MaxClockSpeed"]} MHz");
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(
                            $"Erreur lors de la récupération des informations sur le processeur : {ex.Message}");
                    }

                    writer.WriteLine();

                    writer.WriteLine("-- Informations sur le processeur graphique (GPU) --");
                    try
                    {
                        // Créer une requête WMI pour obtenir les informations sur la carte graphique
                        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

                        foreach (var obj in searcher.Get())
                        {
                            // Obtenir les propriétés de la carte graphique
                            var name = obj["Caption"]?.ToString();
                            var driverVersion = obj["DriverVersion"]?.ToString();
                            var driverDate = obj["DriverDate"]?.ToString();
                            var videoMemory = obj["AdapterRAM"] != null
                                ? (Convert.ToUInt64(obj["AdapterRAM"]) / 1024 / 1024) + " MB"
                                : "N/A";

                            // Formater la date du pilote vidéo
                            if (driverDate != null)
                            {
                                var formattedDriverDate = FormatDriverDate(driverDate);

                                // Afficher les informations
                                writer.WriteLine($"Nom de la carte graphique : {name}");
                                writer.WriteLine($"Version du pilote : {driverVersion}");
                                writer.WriteLine($"Date du pilote : {formattedDriverDate}");
                            }

                            writer.WriteLine($"Mémoire vidéo : {videoMemory}");
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine($"Erreur lors de l'accès aux informations WMI : {ex.Message}");
                    }

                    writer.WriteLine();

                    writer.WriteLine("-- Informations sur la mémoire physique (RAM) --");
                    try
                    {
                        var wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                        var searcher = new ManagementObjectSearcher(wql);
                        var results = searcher.Get();

                        foreach (var o in results)
                        {
                            var rezult = (ManagementObject)o;
                            var totalMemoryBytes = (ulong)rezult["TotalVisibleMemorySize"] * 1024;
                            var availableMemoryBytes = (ulong)rezult["FreePhysicalMemory"] * 1024;

                            var totalMemoryGb = totalMemoryBytes / (1024.0 * 1024 * 1024);
                            var availableMemoryGb = availableMemoryBytes / (1024.0 * 1024 * 1024);

                            writer.WriteLine(
                                $"Mémoire Physique : {availableMemoryGb:F2} Go libre sur {totalMemoryGb:F2} Go total");
                        }
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(
                            $"Erreur lors de la récupération des informations sur la mémoire : {ex.Message}");
                    }

                    writer.WriteLine();

                    writer.WriteLine("-- Informations sur la mémoire de masse --");

                    // Stocker les informations des lecteurs
                    var diskInfo = "Espace libre dans les volumes : \n";

                    // Parcourir les lecteurs et obtenir les informations
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (!drive.IsReady) continue; // Vérifie si le lecteur est prêt

                        // Formatage de l'espace libre en Go avec 1 décimale
                        var freeSpaceGb = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                        var formattedFreeSpace = $"{freeSpaceGb:F1} Go";

                        // Ajouter les informations au résultat
                        diskInfo += $" {drive.Name} {formattedFreeSpace},\n";
                    }

                    // Supprimer la dernière virgule
                    if (diskInfo.EndsWith(",\n"))
                    {
                        diskInfo = diskInfo.Remove(diskInfo.Length - 2);
                    }

                    // Afficher le résultat
                    writer.WriteLine(diskInfo);
                }
            }

            App.ConsoleAndLogWriteLine($"System information debug file created at {filePath}");
        }
        catch (Exception e)
        {
            App.ConsoleAndLogWriteLine($"An exception was raised while writing the system information debug file : {e.Message}");
        }
    }

        
        
    // Fonction récupérant tous les fichiers de débogage, les stocke dans une archive zip et permet d'enregistrer
    /// <summary>
    /// Creates a debug archive by collecting all debug-related files, including optional system and hardware information,
    /// imported projects, and a list of removed group addresses. The archive is then saved as a ZIP file.
    /// </summary>
    /// <param name="includeOsInfo">Specifies whether to include operating system information in the archive.</param>
    /// <param name="includeHardwareInfo">Specifies whether to include hardware information in the archive.</param>
    /// <param name="includeImportedProjects">Specifies whether to include imported projects in the archive.</param>
    /// <param name="includeRemovedGroupAddressList">Specifies whether to include a list of removed group addresses in the archive.</param>
    public static void CreateDebugArchive(bool includeOsInfo = true, bool includeHardwareInfo = true, bool includeImportedProjects = true, bool includeRemovedGroupAddressList = true)
    {
        // Ecriture du fichier d'informations système
        WriteSystemInformationDebugFile(includeOsInfo, includeHardwareInfo);

        try
        {
            if (File.Exists("debug/latest-log.txt")) File.Delete("debug/latest-log.txt");
        }
        // Si le fichier n'est pas accessible
        catch (UnauthorizedAccessException)
        {
            string errorText;
            string title;

            // Traduction de la fenêtre d'erreur
            switch (App.DisplayElements?.SettingsWindow!.AppLang)
            {
                // Arabe
                case "AR":
                    errorText = $"خطأ: لا يمكن الوصول إلى {Path.GetFullPath("debug/latest-log.txt")}. لا يمكن حذفه لاستبداله. عملية التصحيح ملغاة.";
                    title = "خطأ";
                    break;

                // Bulgare
                case "BG":
                    errorText = $"Грешка: Не може да се получи достъп до {Path.GetFullPath("debug/latest-log.txt")}. Не може да бъде изтрит за замяна. Операцията за отстраняване на грешки е отменена.";
                    title = "ГРЕШКА";
                    break;

                // Tchèque
                case "CS":
                    errorText = $"Chyba: Nelze přistoupit k {Path.GetFullPath("debug/latest-log.txt")}. Nelze jej smazat, aby byl nahrazen. Operace ladění byla zrušena.";
                    title = "CHYBA";
                    break;

                // Danois
                case "DA":
                    errorText = $"Fejl: Kan ikke få adgang til {Path.GetFullPath("debug/latest-log.txt")}. Kan ikke slettes for at blive erstattet. Fejlfinding operation annulleret.";
                    title = "FEJL";
                    break;

                // Allemand
                case "DE":
                    errorText = $"Fehler: Zugriff auf {Path.GetFullPath("debug/latest-log.txt")} nicht möglich. Kann nicht gelöscht werden, um ersetzt zu werden. Debugging-Vorgang abgebrochen.";
                    title = "FEHLER";
                    break;

                // Grec
                case "EL":
                    errorText = $"Σφάλμα: Δεν είναι δυνατή η πρόσβαση στο {Path.GetFullPath("debug/latest-log.txt")}. Δεν μπορεί να διαγραφεί για να αντικατασταθεί. Η διαδικασία αποσφαλμάτωσης ακυρώθηκε.";
                    title = "ΣΦΑΛΜΑ";
                    break;

                // Anglais
                case "EN":
                    errorText = $"Error: Unable to access {Path.GetFullPath("debug/latest-log.txt")}. Cannot delete it to replace it. Debugging operation canceled.";
                    title = "ERROR";
                    break;

                // Espagnol
                case "ES":
                    errorText = $"Error: No se puede acceder a {Path.GetFullPath("debug/latest-log.txt")}. No se puede eliminar para reemplazarlo. La operación de depuración ha sido cancelada.";
                    title = "ERROR";
                    break;

                // Estonien
                case "ET":
                    errorText = $"Viga: Ei saa juurde pääseda {Path.GetFullPath("debug/latest-log.txt")}. Ei saa kustutada, et asendada. Silumisoperatsioon tühistatud.";
                    title = "VIGA";
                    break;

                // Finnois
                case "FI":
                    errorText = $"Virhe: Ei voi käyttää {Path.GetFullPath("debug/latest-log.txt")}. Ei voi poistaa korvattavaksi. Virheenkorjaustoiminto peruutettu.";
                    title = "VIRHE";
                    break;

                // Hongrois
                case "HU":
                    errorText = $"Hiba: Nem lehet hozzáférni a {Path.GetFullPath("debug/latest-log.txt")}. Nem lehet törölni a helyettesítéshez. A hibaelhárítási művelet megszakítva.";
                    title = "HIBA";
                    break;

                // Indonésien
                case "ID":
                    errorText = $"Kesalahan: Tidak dapat mengakses {Path.GetFullPath("debug/latest-log.txt")}. Tidak dapat dihapus untuk diganti. Operasi debugging dibatalkan.";
                    title = "KESALAHAN";
                    break;

                // Italien
                case "IT":
                    errorText = $"Errore: Impossibile accedere a {Path.GetFullPath("debug/latest-log.txt")}. Non può essere eliminato per essere sostituito. Operazione di debug annullata.";
                    title = "ERRORE";
                    break;

                // Japonais
                case "JA":
                    errorText = $"エラー: {Path.GetFullPath("debug/latest-log.txt")} にアクセスできません。置き換えるために削除できません。デバッグ操作がキャンセルされました。";
                    title = "エラー";
                    break;

                // Coréen
                case "KO":
                    errorText = $"오류: {Path.GetFullPath("debug/latest-log.txt")}에 접근할 수 없습니다. 교체를 위해 삭제할 수 없습니다. 디버깅 작업이 취소되었습니다.";
                    title = "오류";
                    break;

                // Letton
                case "LV":
                    errorText = $"Kļūda: nevar piekļūt {Path.GetFullPath("debug/latest-log.txt")}. Nevar izdzēst, lai aizstātu. Debuga darbība atcelta.";
                    title = "KĻŪDA";
                    break;

                // Lituanien
                case "LT":
                    errorText = $"Klaida: Nepavyksta pasiekti {Path.GetFullPath("debug/latest-log.txt")}. Negalima ištrinti, kad pakeistumėte. Derinimo operacija nutraukta.";
                    title = "KLAIDA";
                    break;

                // Norvégien
                case "NB":
                    errorText = $"Feil: Kan ikke få tilgang til {Path.GetFullPath("debug/latest-log.txt")}. Kan ikke slettes for å bli erstattet. Feilsøkingsoperasjon avbrutt.";
                    title = "FEIL";
                    break;

                // Néerlandais
                case "NL":
                    errorText = $"Fout: Kan geen toegang krijgen tot {Path.GetFullPath("debug/latest-log.txt")}. Kan niet worden verwijderd om te worden vervangen. Debuggingoperatie geannuleerd.";
                    title = "FOUT";
                    break;

                // Polonais
                case "PL":
                    errorText = $"Błąd: Nie można uzyskać dostępu do {Path.GetFullPath("debug/latest-log.txt")}. Nie można usunąć, aby zastąpić. Operacja debugowania anulowana.";
                    title = "BŁĄD";
                    break;

                // Portugais
                case "PT":
                    errorText = $"Erro: Não é possível aceder a {Path.GetFullPath("debug/latest-log.txt")}. Não é possível excluí-lo para substituí-lo. Operação de depuração cancelada.";
                    title = "ERRO";
                    break;

                // Roumain
                case "RO":
                    errorText = $"Eroare: Nu se poate accesa {Path.GetFullPath("debug/latest-log.txt")}. Nu poate fi șters pentru a fi înlocuit. Operațiunea de depanare a fost anulată.";
                    title = "EROARE";
                    break;

                // Russe
                case "RU":
                    errorText = $"Ошибка: невозможно получить доступ к {Path.GetFullPath("debug/latest-log.txt")}. Невозможно удалить для замены. Операция отладки отменена.";
                    title = "ОШИБКА";
                    break;

                // Slovaque
                case "SK":
                    errorText = $"Chyba: Nie je možné získať prístup k {Path.GetFullPath("debug/latest-log.txt")}. Nie je možné ho odstrániť, aby bol nahradený. Operácia ladenia bola zrušená.";
                    title = "CHYBA";
                    break;

                // Slovène
                case "SL":
                    errorText = $"Napaka: Dostop do {Path.GetFullPath("debug/latest-log.txt")} ni mogoč. Ne more se izbrisati za zamenjavo. Postopek odpravljanja napak je bil preklican.";
                    title = "NAPAKA";
                    break;

                // Suédois
                case "SV":
                    errorText = $"Fel: Det går inte att komma åt {Path.GetFullPath("debug/latest-log.txt")}. Kan inte tas bort för att ersättas. Felsökningsoperationen avbröts.";
                    title = "FEL";
                    break;

                // Turc
                case "TR":
                    errorText = $"Hata: {Path.GetFullPath("debug/latest-log.txt")} erişilemiyor. Değiştirmek için silinemiyor. Hata ayıklama işlemi iptal edildi.";
                    title = "HATA";
                    break;

                // Ukrainien
                case "UK":
                    errorText = $"Помилка: неможливо отримати доступ до {Path.GetFullPath("debug/latest-log.txt")}. Не можна видалити для заміни. Операцію налагодження скасовано.";
                    title = "ПОМИЛКА";
                    break;

                // Chinois simplifié
                case "ZH":
                    errorText = $"错误：无法访问 {Path.GetFullPath("debug/latest-log.txt")}。无法删除以进行替换。调试操作已取消。";
                    title = "错误";
                    break;

                // Langue par défaut (français)
                default:
                    errorText = $"Erreur : impossible d'accéder à {Path.GetFullPath("debug/latest-log.txt")}. Impossible de le supprimer pour le remplacer. Opération de débogage annulée.";
                    title = "ERREUR";
                    break;
            }
                
            MessageBox.Show(errorText, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string debugArchiveName;

        try
        {
            if (App.LogPath != null) File.Copy(App.LogPath, "debug/latest-log.txt");

            debugArchiveName = $"debug-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.zip";

            // Création de l'archive zip et ajout des fichiers
            CreateZipArchive(debugArchiveName, "debug/debugInfo.txt", "debug/latest-log.txt");
        }
        catch (Exception e)
        {
            App.ConsoleAndLogWriteLine($"Error: could not copy the latest log file : {e.Message}");
            return;
        }

        try
        {
            // Récupération des dossiers projets et stockage dans des archives zip
            if (includeImportedProjects)
            {
                // Itération sur tous les répertoires dans le répertoire de base
                foreach (var directory in Directory.GetDirectories("./"))
                {
                    // Exclure le dossier 'logs', 'resources', 'debug', 'de' et 'runtimes'
                    if (Path.GetFileName(directory).Equals("logs", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetFileName(directory).Equals("resources", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetFileName(directory).Equals("debug", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetFileName(directory).Equals("runtimes", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetFileName(directory).Equals("de", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Ajout du dossier du projet dans l'archive de debug
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        CreateZipArchive(debugArchiveName,
                            $"{directory.TrimStart('.', '/')}/{Path.GetFileName(file)}");
                    }

                    foreach (var dir in Directory.GetDirectories(directory))
                    {
                        App.ConsoleAndLogWriteLine(dir.TrimStart('.', '/'));
                        CreateZipArchive(debugArchiveName, dir.TrimStart('.', '/'));
                    }
                }
            }
        }
        catch (Exception e)
        {
            App.ConsoleAndLogWriteLine($"Error: an error occured while adding the necessary folders and files to the debug archive : {e.Message}");
            return;
        }

        // Afficher la boîte de dialogue de sauvegarde
        var saveFileDialog = new SaveFileDialog
        {
            Title = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "حفظ أرشيف التصحيح باسم...",
                // Bulgare
                "BG" => "Запазване на архива за отстраняване на грешки като...",
                // Tchèque
                "CS" => "Uložit archiv ladění jako...",
                // Danois
                "DA" => "Gem fejlfinding-arkiv som...",
                // Allemand
                "DE" => "Debug-Archiv speichern unter...",
                // Grec
                "EL" => "Αποθήκευση αρχείου αποσφαλμάτωσης ως...",
                // Anglais
                "EN" => "Save debug archive as...",
                // Espagnol
                "ES" => "Guardar archivo de depuración como...",
                // Estonien
                "ET" => "Salvesta silumisfail nimega...",
                // Finnois
                "FI" => "Tallenna virheenkorjausarkisto nimellä...",
                // Hongrois
                "HU" => "Hibakeresési archívum mentése másként...",
                // Indonésien
                "ID" => "Simpan arsip debug sebagai...",
                // Italien
                "IT" => "Salva archivio di debug come...",
                // Japonais
                "JA" => "デバッグアーカイブを名前を付けて保存...",
                // Coréen
                "KO" => "디버그 아카이브 이름으로 저장...",
                // Letton
                "LV" => "Saglabāt atkļūdošanas arhīvu kā...",
                // Lituanien
                "LT" => "Išsaugoti derinimo archyvą kaip...",
                // Norvégien
                "NB" => "Lagre feilsøkingsarkiv som...",
                // Néerlandais
                "NL" => "Sla foutopsporingsarchief op als...",
                // Polonais
                "PL" => "Zapisz archiwum debugowania jako...",
                // Portugais
                "PT" => "Salvar arquivo de depuração como...",
                // Roumain
                "RO" => "Salvează arhiva de depanare ca...",
                // Russe
                "RU" => "Сохранить архив отладки как...",
                // Slovaque
                "SK" => "Uložiť archív ladenia ako...",
                // Slovène
                "SL" => "Shrani razhroščevalni arhiv kot...",
                // Suédois
                "SV" => "Spara felsökningsarkiv som...",
                // Turc
                "TR" => "Hata ayıklama arşivini farklı kaydet...",
                // Ukrainien
                "UK" => "Зберегти архів налагодження як...",
                // Chinois simplifié
                "ZH" => "将调试存档另存为...",
                // Cas par défaut (français)
                _ => "Enregistrer l'archive de debug sous..."
            },
            FileName = $"debug-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.zip",
            DefaultExt = ".zip",
            Filter = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "ملفات ZIP|*.zip",
                // Bulgare
                "BG" => "ZIP файлове|*.zip",
                // Tchèque
                "CS" => "ZIP soubory|*.zip",
                // Danois
                "DA" => "ZIP-filer|*.zip",
                // Allemand
                "DE" => "ZIP-Dateien|*.zip",
                // Grec
                "EL" => "Αρχεία ZIP|*.zip",
                // Anglais
                "EN" => "ZIP files|*.zip",
                // Espagnol
                "ES" => "Archivos ZIP|*.zip",
                // Estonien
                "ET" => "ZIP failid|*.zip",
                // Finnois
                "FI" => "ZIP-tiedostot|*.zip",
                // Hongrois
                "HU" => "ZIP fájlok|*.zip",
                // Indonésien
                "ID" => "File ZIP|*.zip",
                // Italien
                "IT" => "File ZIP|*.zip",
                // Japonais
                "JA" => "ZIPファイル|*.zip",
                // Coréen
                "KO" => "ZIP 파일|*.zip",
                // Letton
                "LV" => "ZIP faili|*.zip",
                // Lituanien
                "LT" => "ZIP failai|*.zip",
                // Norvégien
                "NB" => "ZIP-filer|*.zip",
                // Néerlandais
                "NL" => "ZIP-bestanden|*.zip",
                // Polonais
                "PL" => "Pliki ZIP|*.zip",
                // Portugais
                "PT" => "Arquivos ZIP|*.zip",
                // Roumain
                "RO" => "Fișiere ZIP|*.zip",
                // Russe
                "RU" => "ZIP файлы|*.zip",
                // Slovaque
                "SK" => "ZIP súbory|*.zip",
                // Slovène
                "SL" => "ZIP datoteke|*.zip",
                // Suédois
                "SV" => "ZIP-filer|*.zip",
                // Turc
                "TR" => "ZIP dosyaları|*.zip",
                // Ukrainien
                "UK" => "ZIP файли|*.zip",
                // Chinois simplifié
                "ZH" => "ZIP 文件|*.zip",
                // Cas par défaut (français)
                _ => "Archive ZIP|*.zip"
            }
        };

        var result = saveFileDialog.ShowDialog();

        try
        {
            if (result == true)
            {
                App.ConsoleAndLogWriteLine($"Debug archive saved at {saveFileDialog.FileName}.");

                if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);
                File.Copy(debugArchiveName, $"{saveFileDialog.FileName}");
            }
            else
            {
                App.ConsoleAndLogWriteLine("User did not save the debug archived and cancelled the operation.");
            }
        }
        catch (Exception e)
        {
            App.ConsoleAndLogWriteLine($"Error: an error occured while saving the debug archive : {e.Message}");
        }
    }
    
        
    // Fonction permettant de créer une archive zip et d'ajouter des fichiers dedans
    /// <summary>
    /// Creates a ZIP archive at the specified path, adding files and/or directories to it.
    /// If the specified path is a directory, all files and subdirectories within it are included in the archive.
    /// If the path is a file, only that file is added to the archive.
    /// If the ZIP file already exists, it will be overwritten.
    /// </summary>
    /// <param name="zipFilePath">The path where the ZIP archive will be created.</param>
    /// <param name="paths">An array of file and/or directory paths to include in the archive.</param>
    private static void CreateZipArchive(string zipFilePath, params string[] paths)
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
                        App.ConsoleAndLogWriteLine($"{path} {Path.GetDirectoryName(path)}");
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
                        App.ConsoleAndLogWriteLine(
                            $"Le chemin {path} n'a pas été trouvé et ne sera pas ajouté à l'archive en cours de création.");
                    }
                }
            }
            catch (Exception e)
            {
                App.ConsoleAndLogWriteLine($"Error: an error occured while creating and adding files to the archive at {zipFilePath} : {e.Message}");
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
                        App.ConsoleAndLogWriteLine(
                            $"Le chemin {path} n'a pas été trouvé et ne sera pas ajouté à l'archive en cours de création.");
                    }
                }
            }
            catch (Exception e)
            {
                App.ConsoleAndLogWriteLine($"Error: an error occured while creating and adding files to the archive at {zipFilePath} : {e.Message}");
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
    private static void AddDirectoryToArchive(ZipArchive archive, string directoryPath, string entryName)
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
            App.ConsoleAndLogWriteLine($"Error: an error occured while adding a directory to the debug archive : {e.Message}");
        }
    }
       
      
    // Fonction pour le formattage des dates de pilotes
    /// <summary>
    /// Formats driver date strings into a readable "DD/MM/YYYY" format.
    /// </summary>
    /// <param name="driverDate">
    /// The driver date string in the format "YYYYMMDDHHMMSS.SSSSSS-UUU".
    /// </param>
    /// <returns>
    /// A formatted date string in "DD/MM/YYYY" format if the input is valid; 
    /// otherwise, "Date inconnue" or "Date invalide".
    /// </returns>
    /// <remarks>
    /// This function takes a driver date string typically formatted as "YYYYMMDDHHMMSS.SSSSSS-UUU" and
    /// extracts the date part (first 8 characters). It then converts this part into a DateTime object 
    /// and formats it into "DD/MM/YYYY" format. If the input string is null, empty, or less than 8 characters,
    /// it returns "Date inconnue". If the conversion to DateTime fails, it returns "Date invalide".
    /// </remarks>
    private static string FormatDriverDate(string driverDate)
    {
        // Exemple de format : 20220902000000.000000-000
        // Le format souhaité est DD/MM/YYYY

        if (string.IsNullOrEmpty(driverDate) || driverDate.Length < 8)
        {
            return "Date inconnue";
        }

        // Extraire la partie date de la chaîne (YYYYMMDD)
        var datePart = driverDate[..8];

        try
        {
            // Convertir en DateTime
            return DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date)
                ? date.ToString("dd/MM/yyyy")
                : "Date invalide";
        }
        catch (Exception)
        {
            return "Date invalide";
        }
    }

}