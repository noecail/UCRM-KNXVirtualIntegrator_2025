using System.Globalization;
using System.IO;
using System.Management;
using System.Windows;
using KNX_Virtual_Integrator.Model.Interfaces;
using Microsoft.Win32;
// ReSharper disable NullableWarningSuppressionIsUsed

namespace KNX_Virtual_Integrator.Model.Implementations;

public class DebugArchiveGenerator (ILogger logger, IZipArchiveManager zipManager) : IDebugArchiveGenerator
{
    /// <summary>
    /// Generates a debug file for the application.
    /// Creates a debug directory if it does not exist and writes system, software, and hardware information
    /// to a file named "debugInfo.txt". System and hardware information are optional and can be included based on the parameters provided.
    /// </summary>
    /// <param name="includeOsInfo">Indicates whether to include operating system information. (Optional)</param>
    /// <param name="includeHardwareInfo">Indicates whether to include hardware information. (Optional)</param>
    private void WriteSystemInformationDebugFile(bool includeOsInfo = true, bool includeHardwareInfo = true)
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
            logger.ConsoleAndLogWriteLine("Error : could not find path './debug'. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }
        catch (IOException)
        {
            logger.ConsoleAndLogWriteLine("Error : an I/O error occured while creating the folder './debug'. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }
        catch (UnauthorizedAccessException)
        {
            logger.ConsoleAndLogWriteLine($"Error : the application cannot write to {Path.GetFullPath("./debug")}. Could not make the directory. The creation of the debug archive was aborted.");
            return;
        }
        catch (ArgumentException)
        {
            logger.ConsoleAndLogWriteLine($"Error : could not create the directory {Path.GetFullPath("./debug")} because the path is too long, too small or contains illegal characters. The creation of the debug archive was aborted.");
            return;
        }
        catch (NotSupportedException)
        {
            logger.ConsoleAndLogWriteLine($"Error: path {Path.GetFullPath("./debug")} is incorrect. Could not make the directory. The creation of the debug archive was aborted.");
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

            logger.ConsoleAndLogWriteLine($"System information debug file created at {filePath}");
        }
        catch (Exception e)
        {
            logger.ConsoleAndLogWriteLine($"An exception was raised while writing the system information debug file : {e.Message}");
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
    public void CreateDebugArchive(bool includeOsInfo = true, bool includeHardwareInfo = true, bool includeImportedProjects = true, bool includeRemovedGroupAddressList = true)
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
            switch (App.WindowManager?.SettingsWindow!.AppLang)
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
            if (logger is Logger log) File.Copy(log.LogPath, "debug/latest-log.txt");

            debugArchiveName = $"debug-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.zip";

            // Création de l'archive zip et ajout des fichiers
            zipManager.CreateZipArchive(debugArchiveName, "debug/debugInfo.txt", "debug/latest-log.txt");
        }
        catch (Exception e)
        {
            logger.ConsoleAndLogWriteLine($"Error: could not copy the latest log file : {e.Message}");
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
                        zipManager.CreateZipArchive(debugArchiveName,
                            $"{directory.TrimStart('.', '/')}/{Path.GetFileName(file)}");
                    }

                    foreach (var dir in Directory.GetDirectories(directory))
                    {
                        logger.ConsoleAndLogWriteLine(dir.TrimStart('.', '/'));
                        zipManager.CreateZipArchive(debugArchiveName, dir.TrimStart('.', '/'));
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.ConsoleAndLogWriteLine($"Error: an error occured while adding the necessary folders and files to the debug archive : {e.Message}");
            return;
        }

        // Afficher la boîte de dialogue de sauvegarde
        var saveFileDialog = new SaveFileDialog
        {
            Title = App.WindowManager?.SettingsWindow!.AppLang switch
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
            Filter = App.WindowManager?.SettingsWindow!.AppLang switch
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
                logger.ConsoleAndLogWriteLine($"Debug archive saved at {saveFileDialog.FileName}.");

                if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);
                File.Copy(debugArchiveName, $"{saveFileDialog.FileName}");
            }
            else
            {
                logger.ConsoleAndLogWriteLine("User did not save the debug archived and cancelled the operation.");
            }
        }
        catch (Exception e)
        {
            logger.ConsoleAndLogWriteLine($"Error: an error occured while saving the debug archive : {e.Message}");
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