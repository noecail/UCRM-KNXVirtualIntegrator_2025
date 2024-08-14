using System.IO;
using System.IO.Compression;
using System.Windows;

namespace KNX_Virtual_Integrator.Model;

public class ApplicationFileManager
{
    /// <summary>
    /// Ensures that the log directory exists by creating it if it does not already exist.
    /// <para>
    /// If the directory cannot be created due to an exception, the application will be terminated with an error message.
    /// </para>
    /// </summary>
    internal static void EnsureLogDirectoryExists()
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
            Logger.ConsoleAndLogWriteLine($"Error: Unable to create the log directory. {ex.Message}");
            Environment.Exit(1); // Terminates the application with an exit code indicating an error
        }
    }

    
    
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


    internal static void EnsureSettingsFileExists(string settingsPath)
    {
        try
        {
            // Si le fichier de paramétrage n'existe pas, on le crée
            // Note : comme File.Create ouvre un stream vers le fichier à la création, on le ferme directement avec Close().
            if (File.Exists(settingsPath)) return;
            
            File.Create(settingsPath).Close();

            if (App.WindowManager == null || App.WindowManager.SettingsWindow == null) return;
            
            // Le thème appliqué par défaut est le même que celui de windows
            App.WindowManager.SettingsWindow.EnableLightTheme = SystemSettingsDetector.DetectWindowsTheme();

            // La langue de l'application appliquée est la même que celle de windows
            App.WindowManager.SettingsWindow.AppLang = SystemSettingsDetector.DetectWindowsLanguage();
        }
        // Si le programme n'a pas accès en écriture pour créer le fichier
        catch (UnauthorizedAccessException)
        {
            // Définir les variables pour le texte du message et le titre du MessageBox
            var messageBoxText = App.WindowManager?.SettingsWindow?.AppLang switch
            {
                // Arabe
                "AR" => "خطأ: تعذر الوصول إلى ملف إعدادات التطبيق. يرجى التحقق من أنه ليس للقراءة فقط وحاول مرة أخرى، أو قم بتشغيل البرنامج كمسؤول.\nرمز الخطأ: 1",
                // Bulgare
                "BG" => "Грешка: Не може да се получи достъп до конфигурационния файл на приложението. Моля, проверете дали файлът не е само за четене и опитайте отново, или стартирайте програмата като администратор.\nКод за грешка: 1",
                // Tchèque
                "CS" => "Chyba: Nelze získat přístup k konfiguračnímu souboru aplikace. Zkontrolujte, zda není pouze ke čtení, a zkuste to znovu, nebo spusťte program jako správce.\nChybový kód: 1",
                // Danois
                "DA" => "Fejl: Kan ikke få adgang til applikationskonfigurationsfilen. Kontroller venligst, at filen ikke er skrivebeskyttet, og prøv igen, eller start programmet som administrator.\nFejlkode: 1",
                // Allemand
                "DE" => "Fehler: Zugriff auf die Konfigurationsdatei der Anwendung nicht möglich. Bitte überprüfen Sie, ob die Datei schreibgeschützt ist, und versuchen Sie es erneut, oder starten Sie das Programm als Administrator.\nFehlercode: 1",
                // Grec
                "EL" => "Σφάλμα: δεν είναι δυνατή η πρόσβαση στο αρχείο ρυθμίσεων της εφαρμογής. Παρακαλώ ελέγξτε αν δεν είναι μόνο για ανάγνωση και προσπαθήστε ξανά, ή ξεκινήστε το πρόγραμμα ως διαχειριστής.\nΚωδικός σφάλματος: 1",
                // Anglais
                "EN" => "Error: Unable to access the application configuration file. Please check if it is read-only and try again, or run the program as an administrator.\nError Code: 1",
                // Espagnol
                "ES" => "Error: No se puede acceder al archivo de configuración de la aplicación. Por favor, verifique si el archivo es de solo lectura y vuelva a intentarlo, o ejecute el programa como administrador.\nCódigo de error: 1",
                // Estonien
                "ET" => "Viga: rakenduse konfiguratsioonifailile ei saa juurde pääseda. Kontrollige, kas fail on ainult lugemiseks ja proovige uuesti või käivitage programm administraatorina.\nVeakood: 1",
                // Finnois
                "FI" => "Virhe: Sovelluksen asetustiedostoon ei pääse käsiksi. Tarkista, ettei tiedosto ole vain luku -tilassa, ja yritä uudelleen tai käynnistä ohjelma järjestelmänvalvojana.\nVirhekoodi: 1",
                // Hongrois
                "HU" => "Hiba: Nem lehet hozzáférni az alkalmazás konfigurációs fájljához. Kérjük, ellenőrizze, hogy a fájl nem csak olvasásra van-e beállítva, és próbálja újra, vagy futtassa a programot rendszergazdai jogosultságokkal.\nHibakód: 1",
                // Indonésien
                "ID" => "Kesalahan: tidak dapat mengakses file konfigurasi aplikasi. Silakan periksa apakah file tersebut hanya-baca dan coba lagi, atau jalankan program sebagai administrator.\nKode kesalahan: 1",
                // Italien
                "IT" => "Errore: impossibile accedere al file di configurazione dell'applicazione. Verifica se il file è solo in lettura e riprova, oppure avvia il programma come amministratore.\nCodice errore: 1",
                // Japonais
                "JA" => "エラー: アプリケーションの設定ファイルにアクセスできません。ファイルが読み取り専用でないか確認し、再試行するか、管理者としてプログラムを実行してください。\nエラーコード: 1",
                // Coréen
                "KO" => "오류: 애플리케이션 구성 파일에 액세스할 수 없습니다. 파일이 읽기 전용인지 확인하고 다시 시도하거나 관리자로 프로그램을 실행하세요.\n오류 코드: 1",
                // Letton
                "LV" => "Kļūda: nevar piekļūt lietojumprogrammas konfigurācijas failam. Lūdzu, pārbaudiet, vai fails nav tikai lasāms, un mēģiniet vēlreiz vai palaidiet programmu kā administrators.\nKļūdas kods: 1",
                // Lituanien
                "LT" => "Klaida: negalima prieiti prie programos konfigūracijos failo. Patikrinkite, ar failas nėra tik skaitymui ir bandykite dar kartą arba paleiskite programą kaip administratorius.\nKlaidos kodas: 1",
                // Norvégien
                "NB" => "Feil: Kan ikke få tilgang til applikasjonskonfigurasjonsfilen. Sjekk om filen er skrivebeskyttet og prøv igjen, eller kjør programmet som administrator.\nFeilkode: 1",
                // Néerlandais
                "NL" => "Fout: kan geen toegang krijgen tot het configuratiebestand van de applicatie. Controleer of het bestand alleen-lezen is en probeer het opnieuw, of voer het programma uit als administrator.\nFoutcode: 1",
                // Polonais
                "PL" => "Błąd: Nie można uzyskać dostępu do pliku konfiguracyjnego aplikacji. Sprawdź, czy plik nie jest tylko do odczytu, a następnie spróbuj ponownie lub uruchom program jako administrator.\nKod błędu: 1",
                // Portugais
                "PT" => "Erro: não foi possível acessar o arquivo de configuração do aplicativo. Verifique se o arquivo é somente leitura e tente novamente, ou execute o programa como administrador.\nCódigo de erro: 1",
                // Roumain
                "RO" => "Eroare: Nu se poate accesa fișierul de configurare al aplicației. Vă rugăm să verificați dacă fișierul este numai pentru citire și să încercați din nou sau să rulați programul ca administrator.\nCod eroare: 1",
                // Russe
                "RU" => "Ошибка: невозможно получить доступ к файлу конфигурации приложения. Проверьте, не является ли файл только для чтения, и попробуйте снова, или запустите программу от имени администратора.\nКод ошибки: 1",
                // Slovaque
                "SK" => "Chyba: nemožno získať prístup k konfiguračnému súboru aplikácie. Skontrolujte, či nie je súbor iba na čítanie, a skúste to znova, alebo spustite program ako správca.\nChybový kód: 1",
                // Slovène
                "SL" => "Napaka: dostop do konfiguracijske datoteke aplikacije ni mogoč. Preverite, ali je datoteka samo za branje, in poskusite znova, ali zaženite program kot skrbnik.\nKoda napake: 1",
                // Suédois
                "SV" => "Fel: Kan inte komma åt konfigurationsfilen för applikationen. Kontrollera om filen är skrivskyddad och försök igen, eller kör programmet som administratör.\nFelkod: 1",
                // Turc
                "TR" => "Hata: Uygulama yapılandırma dosyasına erişilemiyor. Dosyanın salt okunur olup olmadığını kontrol edin ve tekrar deneyin veya programı yönetici olarak çalıştırın.\nHata Kodu: 1",
                // Ukrainien
                "UK" => "Помилка: неможливо отримати доступ до файлу конфігурації програми. Будь ласка, перевірте, чи не є файл тільки для читання, і спробуйте ще раз або запустіть програму від імені адміністратора.\nКод помилки: 1",
                // Chinois simplifié
                "ZH" => "错误: 无法访问应用程序配置文件。请检查文件是否为只读，并重试，或者以管理员身份运行程序。\n错误代码: 1",
                // Cas par défaut (français)
                _ => "Erreur: impossible d'accéder au fichier de paramétrage de l'application. Veuillez vérifier qu'il n'est pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 1"
            };

            var caption = App.WindowManager?.SettingsWindow?.AppLang switch
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

            // Afficher le MessageBox avec les traductions appropriées
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(1);
        }
        // Si la longueur du path est incorrecte ou que des caractères non supportés sont présents
        catch (ArgumentException)
        {
            // Traductions des messages d'erreur et du titre en fonction de la langue
            var errorTitle = App.WindowManager?.SettingsWindow?.AppLang switch
            {
                "AR" => "خطأ",
                "BG" => "Грешка",
                "CS" => "Chyba",
                "DA" => "Fejl",
                "DE" => "Fehler",
                "EL" => "Σφάλμα",
                "EN" => "Error",
                "ES" => "Error",
                "ET" => "Viga",
                "FI" => "Virhe",
                "HU" => "Hiba",
                "ID" => "Kesalahan",
                "IT" => "Errore",
                "JA" => "エラー",
                "KO" => "오류",
                "LV" => "Kļūda",
                "LT" => "Klaida",
                "NB" => "Feil",
                "NL" => "Fout",
                "PL" => "Błąd",
                "PT" => "Erro",
                "RO" => "Eroare",
                "RU" => "Ошибка",
                "SK" => "Chyba",
                "SL" => "Napaka",
                "SV" => "Fel",
                "TR" => "Hata",
                "UK" => "Помилка",
                "ZH" => "错误",
                _ => "Erreur"
            };

            var errorMessage = App.WindowManager?.SettingsWindow?.AppLang switch
            {
                "AR" => $"خطأ: هناك أحرف غير مدعومة في مسار ملف الإعدادات ({settingsPath}). تعذر الوصول إلى الملف.\nرمز الخطأ: 2",
                "BG" => $"Грешка: Съдържа неразрешени символи в пътя на файла с настройки ({settingsPath}). Невъзможно е да се достъпи до файла.\nКод на грешката: 2",
                "CS" => $"Chyba: V cestě k souboru nastavení ({settingsPath}) jsou přítomny nepodporované znaky. Nelze přistupovat k souboru.\nKód chyby: 2",
                "DA" => $"Fejl: Ugyldige tegn findes i stien til konfigurationsfilen ({settingsPath}). Kan ikke få adgang til filen.\nFejlkode: 2",
                "DE" => $"Fehler: Im Pfad zur Einstellungsdatei ({settingsPath}) sind nicht unterstützte Zeichen vorhanden. Auf die Datei kann nicht zugegriffen werden.\nFehlercode: 2",
                "EL" => $"Σφάλμα: Υπάρχουν μη υποστηριγμένοι χαρακτήρες στη διαδρομή του αρχείου ρυθμίσεων ({settingsPath}). Δεν είναι δυνατή η πρόσβαση στο αρχείο.\nΚωδικός σφάλματος: 2",
                "EN" => $"Error: Unsupported characters are present in the settings file path ({settingsPath}). Unable to access the file.\nError code: 2",
                "ES" => $"Error: Hay caracteres no admitidos en la ruta del archivo de configuración ({settingsPath}). No se puede acceder al archivo.\nCódigo de error: 2",
                "ET" => $"Viga: Seadistusfaili tee ({settingsPath}) sisaldab toetamatuid märke. Failile ei ole võimalik juurde pääseda.\nVigakood: 2",
                "FI" => $"Virhe: Asetustiedoston polussa ({settingsPath}) on tukemattomia merkkejä. Tiedostoon ei voi käyttää.\nVirhekoodi: 2",
                "HU" => $"Hiba: Az beállítási fájl elérési útvonalán ({settingsPath}) nem támogatott karakterek találhatók. A fájlhoz nem lehet hozzáférni.\nHibakód: 2",
                "ID" => $"Kesalahan: Karakter yang tidak didukung ada di jalur file pengaturan ({settingsPath}). Tidak dapat mengakses file.\nKode kesalahan: 2",
                "IT" => $"Errore: Sono presenti caratteri non supportati nel percorso del file di configurazione ({settingsPath}). Impossibile accedere al file.\nCodice errore: 2",
                "JA" => $"エラー: 設定ファイルのパス ({settingsPath}) にサポートされていない文字が含まれています。ファイルにアクセスできません。\nエラーコード: 2",
                "KO" => $"오류: 설정 파일 경로 ({settingsPath})에 지원되지 않는 문자가 포함되어 있습니다. 파일에 접근할 수 없습니다.\n오류 코드: 2",
                "LV" => $"Kļūda: Iestatījumu faila ceļā ({settingsPath}) ir neatbalstīti rakstzīmes. Nevar piekļūt failam.\nKļūdas kods: 2",
                "LT" => $"Klaida: Nustatymų failo kelias ({settingsPath}) turi nepalaikomų simbolių. Nepavyksta pasiekti failo.\nKlaidos kodas: 2",
                "NB" => $"Feil: Det finnes ikke-støttede tegn i stien til innstillingsfilen ({settingsPath}). Kan ikke få tilgang til filen.\nFeilkode: 2",
                "NL" => $"Fout: Onondersteunde tekens zijn aanwezig in het pad naar het instellingenbestand ({settingsPath}). Kan niet toegang krijgen tot het bestand.\nFoutcode: 2",
                "PL" => $"Błąd: W ścieżce pliku ustawień ({settingsPath}) znajdują się nieobsługiwane znaki. Nie można uzyskać dostępu do pliku.\nKod błędu: 2",
                "PT" => $"Erro: Caracteres não suportados estão presentes no caminho do arquivo de configuração ({settingsPath}). Não é possível acessar o arquivo.\nCódigo de erro: 2",
                "RO" => $"Eroare: Caracterelor nesuportate sunt prezente în calea fișierului de configurare ({settingsPath}). Nu se poate accesa fișierul.\nCod eroare: 2",
                "RU" => $"Ошибка: В пути к файлу настроек ({settingsPath}) присутствуют неподдерживаемые символы. Невозможно получить доступ к файлу.\nКод ошибки: 2",
                "SK" => $"Chyba: V ceste k súboru nastavení ({settingsPath}) sú prítomné nepodporované znaky. Nie je možné pristupovať k súboru.\nKód chyby: 2",
                "SL" => $"Napaka: V poti do konfiguracijske datoteke ({settingsPath}) so prisotne nepodprte znake. Do datoteke ni mogoče dostopati.\nKoda napake: 2",
                "SV" => $"Fel: I inställningsfilens sökväg ({settingsPath}) finns tecken som inte stöds. Kan inte komma åt filen.\nFelkod: 2",
                "TR" => $"Hata: Ayar dosyası yolunda ({settingsPath}) desteklenmeyen karakterler bulunuyor. Dosyaya erişilemiyor.\nHata kodu: 2",
                "UK" => $"Помилка: У шляху до файлу налаштувань ({settingsPath}) є непідтримувані символи. Не вдалося отримати доступ до файлу.\nКод помилки: 2",
                "ZH" => $"错误: 配置文件路径 ({settingsPath}) 中存在不支持的字符。无法访问文件。\n错误代码: 2",
                _ => $"Erreur: des caractères non supportés sont présents dans le chemin d'accès du fichier de paramétrage ({settingsPath}). Impossible d'accéder au fichier.\nCode erreur: 2"
            };

            // Affichage de la MessageBox avec le titre et le message traduits
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(2);
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            // Traductions du titre et du message d'erreur en fonction de la langue
            var ioErrorTitle = App.WindowManager?.SettingsWindow?.AppLang switch
            {
                "AR" => "خطأ",
                "BG" => "Грешка",
                "CS" => "Chyba",
                "DA" => "Fejl",
                "DE" => "Fehler",
                "EL" => "Σφάλμα",
                "EN" => "Error",
                "ES" => "Error",
                "ET" => "Viga",
                "FI" => "Virhe",
                "HU" => "Hiba",
                "ID" => "Kesalahan",
                "IT" => "Errore",
                "JA" => "エラー",
                "KO" => "오류",
                "LV" => "Kļūda",
                "LT" => "Klaida",
                "NB" => "Feil",
                "NL" => "Fout",
                "PL" => "Błąd",
                "PT" => "Erro",
                "RO" => "Eroare",
                "RU" => "Ошибка",
                "SK" => "Chyba",
                "SL" => "Napaka",
                "SV" => "Fel",
                "TR" => "Hata",
                "UK" => "Помилка",
                "ZH" => "错误",
                _ => "Erreur"
            };

            var ioErrorMessage = App.WindowManager?.SettingsWindow?.AppLang switch
            {
                "AR" => $"خطأ: خطأ في الإدخال/الإخراج عند فتح ملف الإعدادات.\nرمز الخطأ: 3",
                "BG" => "Грешка: Грешка при четене/запис на файла с настройки.\nКод на грешката: 3",
                "CS" => "Chyba: Chyba I/O při otevírání souboru nastavení.\nKód chyby: 3",
                "DA" => "Fejl: I/O-fejl ved åbning af konfigurationsfilen.\nFejlkode: 3",
                "DE" => "Fehler: I/O-Fehler beim Öffnen der Einstellungsdatei.\nFehlercode: 3",
                "EL" => "Σφάλμα: Σφάλμα I/O κατά το άνοιγμα του αρχείου ρυθμίσεων.\nΚωδικός σφάλματος: 3",
                "EN" => "Error: I/O error while opening the settings file.\nError code: 3",
                "ES" => "Error: Error de I/O al abrir el archivo de configuración.\nCódigo de error: 3",
                "ET" => "Viga: I/O viga seadistusfaili avamisel.\nVigakood: 3",
                "FI" => "Virhe: I/O-virhe asetustiedoston avaamisessa.\nVirhekoodi: 3",
                "HU" => "Hiba: I/O hiba a beállítási fájl megnyitásakor.\nHibakód: 3",
                "ID" => "Kesalahan: Kesalahan I/O saat membuka file pengaturan.\nKode kesalahan: 3",
                "IT" => "Errore: Errore I/O durante l'apertura del file di configurazione.\nCodice errore: 3",
                "JA" => "エラー: 設定ファイルのオープン時にI/Oエラーが発生しました。\nエラーコード: 3",
                "KO" => "오류: 설정 파일 열기 중 I/O 오류가 발생했습니다.\n오류 코드: 3",
                "LV" => "Kļūda: I/O kļūda atverot iestatījumu failu.\nKļūdas kods: 3",
                "LT" => "Klaida: I/O klaida atidarant nustatymų failą.\nKlaidos kodas: 3",
                "NB" => "Feil: I/O-feil ved åpning av innstillingsfilen.\nFeilkode: 3",
                "NL" => "Fout: I/O-fout bij het openen van het instellingenbestand.\nFoutcode: 3",
                "PL" => "Błąd: Błąd I/O podczas otwierania pliku konfiguracyjnego.\nKod błędu: 3",
                "PT" => "Erro: Erro de I/O ao abrir o arquivo de configuração.\nCódigo de erro: 3",
                "RO" => "Eroare: Eroare I/O la deschiderea fișierului de configurare.\nCod eroare: 3",
                "RU" => "Ошибка: Ошибка ввода/вывода при открытии файла настроек.\nКод ошибки: 3",
                "SK" => "Chyba: Chyba I/O pri otváraní súboru nastavení.\nKód chyby: 3",
                "SL" => "Napaka: Napaka I/O pri odpiranju konfiguracijske datoteke.\nKoda napake: 3",
                "SV" => "Fel: I/O-fel vid öppning av inställningsfilen.\nFelkod: 3",
                "TR" => "Hata: Ayar dosyasını açarken I/O hatası oluştu.\nHata kodu: 3",
                "UK" => "Помилка: Помилка вводу/виводу під час відкриття файлу налаштувань.\nКод помилки: 3",
                "ZH" => "错误: 打开设置文件时发生I/O错误。\n错误代码: 3",
                _ => "Erreur: Erreur I/O lors de l'ouverture du fichier de paramétrage.\nCode erreur: 3"
            };

            // Affichage de la MessageBox avec le titre et le message traduits
            MessageBox.Show(ioErrorMessage, ioErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(3);
        }
    }
    
    
    // Fonction permettant de sauvegarder les paramètres dans le fichier appSettings
    /// <summary>
    /// Saves the application settings to the appSettings file, handling potential I/O errors during the process.
    /// </summary>
    internal static void SaveApplicationSettings()
    {
        // Création du stream d'écriture du fichier appSettings
        var writer = new StreamWriter("./appSettings");

        // Ecriture de toutes les lignes du fichier
        try
        {
            writer.WriteLine(
                "-----------------------------------------------------------------------------------------");
            writer.WriteLine(
                "|                            KNX VIRTUAL INTEGRATOR SETTINGS                            |");
            writer.WriteLine(
                "-----------------------------------------------------------------------------------------");
            writer.Write("theme : ");
            writer.WriteLine((bool)App.WindowManager?.SettingsWindow?.EnableLightTheme ? "light" : "dark");

            writer.Write("application language : ");
            writer.WriteLine(App.WindowManager.SettingsWindow?.AppLang);

            writer.Write("window scale factor : ");
            writer.WriteLine(App.WindowManager.SettingsWindow?.AppScaleFactor);

            writer.WriteLine(
                "-----------------------------------------------------------------------------------------");
            writer.Write(
                "/!\\ WARNING:\nAny value that you modify in this file and that is not correct will be replaced by a default value.");
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            Logger.ConsoleAndLogWriteLine("Error: an I/O error occured while writing appSettings.");
        }
        // Si le buffer d'écriture est plein
        catch (NotSupportedException)
        {
            Logger.ConsoleAndLogWriteLine("Error: the streamwriter buffer for appSettings is full. Flushing it.");
            writer.Flush(); // Vidage du buffer
        }
        // Si le stream a été fermé pendant l'écriture
        catch (ObjectDisposedException)
        {
            Logger.ConsoleAndLogWriteLine("Error: the streamwriter for appSettings was closed before finishing the writing operation.");
        }
        finally
        {
            // Fermeture du stream d'écriture
            writer.Close();
        }
    }
}