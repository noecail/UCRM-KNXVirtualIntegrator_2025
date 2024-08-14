using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using KNX_Virtual_Integrator.Model;

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable ConvertToUsingDeclaration


namespace KNX_Virtual_Integrator.View;

/// <summary>
///  Window used to set the application settings.
/// </summary>
public partial class SettingsWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Gets or sets a value indicating whether the light theme is enabled for the application.
    /// </summary>
    public bool EnableLightTheme { get; internal set; } // Thème de l'application (sombre/clair)

    /// <summary>
    /// Gets or sets the application language, with French as the default.
    /// </summary>
    public string AppLang { get; internal set; } // Langue de l'application (français par défaut)

    /// <summary>
    ///  Gets or sets the scale factor for the content of every window of the application
    /// </summary>
    public int AppScaleFactor { get; private set; } // Facteur d'échelle pour les fenêtres de l'application

    /// <summary>
    /// Variable used to know if the user is dragging the scaling slider cursor.
    /// </summary>
    private bool _isDragging;
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Constructeur par défaut. Charge les paramètres contenus dans le fichier appSettings et les affiche également
    // dans la fenêtre de paramétrage de l'application. Si la valeur est incorrecte ou vide, une valeur par défaut
    // est affectée.
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsWindow"/> class,
    /// loading and applying settings from the appSettings file, and setting default values where necessary.
    /// </summary>
    public SettingsWindow()
    {
        InitializeComponent(); // Initialisation de la fenêtre de paramétrage

        // Initialement, l'application dispose des paramètres par défaut, qui seront potentiellement modifiés après par
        // la lecture du fichier settings. Cela permet d'éviter un crash si le fichier 
        EnableLightTheme = true;
        AppLang = "FR";
        AppScaleFactor = 100;

        const string settingsPath = "./appSettings"; // Chemin du fichier paramètres

        ApplicationFileManager.EnsureSettingsFileExists(settingsPath);

        // Déclaration du stream pour la lecture du fichier appSettings, initialement null
        StreamReader? reader = null;

        try
        {
            // Création du stream
            reader = new StreamReader(settingsPath);
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            // Traductions du titre et du message d'erreur en fonction de la langue
            var ioErrorTitle = AppLang switch
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

            var ioErrorMessage = AppLang switch
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

        try
        {
            // On parcourt toutes les lignes tant qu'elle n'est pas 'null'
            while (reader?.ReadLine() is { } line)
            {
                // Créer un HashSet avec tous les codes de langue valides
                var validLanguageCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "AR", "BG", "CS", "DA", "DE", "EL", "EN", "ES", "ET", "FI",
                    "HU", "ID", "IT", "JA", "KO", "LT", "LV", "NB", "NL", "PL",
                    "PT", "RO", "RU", "SK", "SL", "SV", "TR", "UK", "ZH"
                };

                // On coupe la ligne en deux morceaux : la partie avant le ' : ' qui contient le type de paramètre contenu dans la ligne,
                // la partie après qui contient la valeur du paramètre
                var parts = line.Split(':');

                // S'il n'y a pas de ' : ' ou qu'il n'y a rien après les deux points, on skip car la ligne nous intéresse pas
                if (parts.Length < 2) continue;

                var parameter = parts[0].Trim().ToLower();
                var value = parts[1].Trim();

                switch (parameter)
                {
                    case "theme":
                        // Si la valeur n'est pas dark, on mettra toujours le thème clair (en cas d'erreur, ou si la value est "light")
                        EnableLightTheme = !value.Equals("dark", StringComparison.CurrentCultureIgnoreCase);
                        break;

                    case "application language":
                        // Vérifier si value est un code de langue valide, si elle est valide, on assigne la valeur, sinon on met la langue par défaut
                        AppLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
                        break;

                    case "window scale factor":
                        try
                        {
                            AppScaleFactor = Convert.ToInt32(value) > 300 || Convert.ToInt32(value) < 50 ? 100 : Convert.ToInt32(value);
                            if (AppScaleFactor <= 100)
                            {
                                ApplyScaling(AppScaleFactor/100f - 0.1f);
                            }
                            else
                            {
                                ApplyScaling(AppScaleFactor/100f - 0.2f);
                            }
                        }
                        catch (Exception)
                        {
                            Logger.ConsoleAndLogWriteLine("Error: Could not parse the integer value of the window scale factor. Restoring default value (100%).");
                        }
                        break;
                        
                }
            }

        }
        // Si l'application a manqué de mémoire pendant la récupération des lignes
        catch (OutOfMemoryException)
        {
            Logger.ConsoleAndLogWriteLine("Error: The program does not have sufficient memory to run. Please try closing a few applications before trying again.");
            return;
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            Logger.ConsoleAndLogWriteLine("Error: An I/O error occured while reading the settings file.");
            return;
        }
        finally
        {
            reader?.Close(); // Fermeture du stream de lecture
            ApplicationFileManager.SaveApplicationSettings(); // Mise à jour du fichier appSettings
        }
            
        UpdateWindowContents(false, true, true); // Affichage des paramètres dans la fenêtre
        ScaleSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(SliderMouseLeftButtonDown), true);
        ScaleSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(SliderMouseLeftButtonUp), true);
        ScaleSlider.AddHandler(MouseMoveEvent, new MouseEventHandler(SliderMouseMove), true);
    }


    // Fonction s'exécutant à la fermeture de la fenêtre de paramètres
    /// <summary>
    /// Handles the settings window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true; // Pour éviter de tuer l'instance de SettingsWindow, on annule la fermeture
        UpdateWindowContents(true); // Mise à jour du contenu de la fenêtre pour remettre les valeurs précédentes
        Hide(); // On masque la fenêtre à la place
    }


    // Fonction permettant de mettre à jour les champs dans la fenêtre de paramétrage
    /// <summary>
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the settingswindow accordingly to the application settings.
    /// </summary>
    private void UpdateWindowContents(bool isClosing = false, bool langChanged = false, bool themeChanged = false)
    {
        if (AppLang != "FR")
        {
            FrAppLanguageComboBoxItem.IsSelected = (AppLang == "FR"); // Sélection/Désélection

            // Sélection du langage de l'application (même fonctionnement que le code ci-dessus)
            foreach (ComboBoxItem item in AppLanguageComboBox.Items)
            {
                if (!item.Content.ToString()!.StartsWith(AppLang)) continue;
                item.IsSelected = true;
                break;
            }
        }
        
        // Sélection du thème clair ou sombre
        LightThemeComboBoxItem.IsSelected = EnableLightTheme;
        DarkThemeComboBoxItem.IsSelected = !EnableLightTheme;

        // Mise à jour du slider
        ScaleSlider.Value = AppScaleFactor;
            
        // Traduction du menu settings
        if (langChanged) TranslateWindowContents();
            
        // Application du thème
        if (themeChanged) ApplyThemeToWindow();
    }


    // Fonction traduisant tous les textes de la fenêtre paramètres
    /// <summary>
    /// This function translates all the texts contained in the setting window to the application language
    /// </summary>
    private void TranslateWindowContents()
    {
        switch (AppLang)
        {
            // Arabe
            case "AR":
                SettingsWindowTopTitle.Text = "الإعدادات";
                AppSettingsTitle.Text = "إعدادات التطبيق";
                ThemeTextBox.Text = "الموضوع:";
                LightThemeComboBoxItem.Content = "فاتح (افتراضي)";
                DarkThemeComboBoxItem.Content = "داكن";
                AppLanguageTextBlock.Text = "لغة التطبيق:";
                MenuDebug.Text = "قائمة التصحيح";
                AddInfosOsCheckBox.Content = "تضمين معلومات نظام التشغيل";
                AddInfosHardCheckBox.Content = "تضمين معلومات الأجهزة";
                AddImportedFilesCheckBox.Content = "تضمين الملفات المستوردة منذ بدء التشغيل";
                IncludeAddressListCheckBox.Content = "تضمين قائمة العناوين المحذوفة من المشاريع";
                CreateArchiveDebugText.Text = "إنشاء ملف التصحيح";
                OngletDebug.Header = "تصحيح";
                OngletInformations.Header = "معلومات";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nالإصدار {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nالبناء {App.AppBuild}" +
                                        $"\n" +
                                        $"\nبرنامج تم إنشاؤه كجزء من تدريب هندسي بواسطة طلاب INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE و Maxime OLIVEIRA LOPES" +
                                        $"\n" +
                                        $"\nبإشراف:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nشراكة بين المعهد الوطني للعلوم التطبيقية (INSA) في تولوز واتحاد Cepière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nإنشاء: 06/2024 - 07/2024\n";
                SaveButtonText.Text = "حفظ";
                CancelButtonText.Text = "إلغاء";
                    
                ScalingText.Text = "التحجيم:";
                OngletParametresApplication.Header = "عام";
                    
                NoteImportante.Text = "\nملاحظة هامة:";
                NoteImportanteContenu.Text = "الاسم والشعارات وأي صورة مرتبطة بـ KNX هي ملك لا يتجزأ لجمعية KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("موقع جمعية KNX");
                break;

            // Bulgare
            case "BG":
                SettingsWindowTopTitle.Text = "Настройки";
                AppSettingsTitle.Text = "Настройки на приложението";
                ThemeTextBox.Text = "Тема:";
                LightThemeComboBoxItem.Content = "Светло (по подразбиране)";
                DarkThemeComboBoxItem.Content = "Тъмно";
                AppLanguageTextBlock.Text = "Език на приложението:";
                MenuDebug.Text = "Меню за отстраняване на грешки";
                AddInfosOsCheckBox.Content = "Включване на информация за операционната система";
                AddInfosHardCheckBox.Content = "Включване на информация за хардуера на компютъра";
                AddImportedFilesCheckBox.Content = "Включване на файлове, импортирани след стартиране";
                IncludeAddressListCheckBox.Content = "Включване на списък с адреси на групи, премахнати от проекти";
                CreateArchiveDebugText.Text = "Създаване на файл за отстраняване на грешки";
                OngletDebug.Header = "Отстраняване на грешки";
                OngletInformations.Header = "Информация";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nВерсия {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nИзграждане {App.AppBuild}" +
                                        $"\n" +
                                        $"\nСофтуер, създаден като част от инженерно стаж от студенти на INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE и Maxime OLIVEIRA LOPES" +
                                        $"\n" +
                                        $"\nПод наблюдението на:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nПартньорство между Националния институт по приложни науки (INSA) в Тулуза и Съюза Cepière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nСъздаване: 06/2024 - 07/2024\n";
                SaveButtonText.Text = "Запази";
                CancelButtonText.Text = "Отказ";
                    
                ScalingText.Text = "Мащабиране:";
                OngletParametresApplication.Header = "Общ";
                    
                NoteImportante.Text = "\nВажна бележка:";
                NoteImportanteContenu.Text = "името, логото и всички изображения, свързани с KNX, са неотменима собственост на асоциацията KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Уебсайт на асоциация KNX");
                break;

            // Tchèque
            case "CS":
                SettingsWindowTopTitle.Text = "Nastavení";
                AppSettingsTitle.Text = "Nastavení aplikace";
                ThemeTextBox.Text = "Motiv:";
                LightThemeComboBoxItem.Content = "Světlý (výchozí)";
                DarkThemeComboBoxItem.Content = "Tmavý";
                AppLanguageTextBlock.Text = "Jazyk aplikace:";
                MenuDebug.Text = "Nabídka ladění";
                AddInfosOsCheckBox.Content = "Zahrnout informace o operačním systému";
                AddInfosHardCheckBox.Content = "Zahrnout informace o hardwaru počítače";
                AddImportedFilesCheckBox.Content = "Zahrnout soubory importované od spuštění";
                IncludeAddressListCheckBox.Content = "Zahrnout seznam odstraněných skupinových adres v projektech";
                CreateArchiveDebugText.Text = "Vytvořit soubor pro ladění";
                OngletDebug.Header = "Ladění";
                OngletInformations.Header = "Informace";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nVerze {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nBuild {App.AppBuild}" +
                                        $"\n" +
                                        $"\nSoftware vytvořený jako součást inženýrského stáže studenty INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE a Maxime OLIVEIRA LOPES" +
                                        $"\n" +
                                        $"\nPod dohledem:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nPartnerství mezi Národním institutem aplikovaných věd (INSA) v Toulouse a Union Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nVytvořeno: 06/2024 - 07/2024\n";
                SaveButtonText.Text = "Uložit";
                CancelButtonText.Text = "Zrušit";
                    
                ScalingText.Text = "Měřítko:";
                OngletParametresApplication.Header = "Obecné";
                
                NoteImportante.Text = "\nDůležitá poznámka:";
                NoteImportanteContenu.Text = "název, loga a jakékoli obrázky související s KNX jsou neoddělitelným vlastnictvím asociace KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Webová stránka asociace KNX");
                break;

            // Danois
            case "DA":
                SettingsWindowTopTitle.Text = "Indstillinger";
                AppSettingsTitle.Text = "App-indstillinger";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Lys (standard)";
                DarkThemeComboBoxItem.Content = "Mørk";
                AppLanguageTextBlock.Text = "App-sprog:";
                MenuDebug.Text = "Fejlfindingsmenu";
                AddInfosOsCheckBox.Content = "Inkluder oplysninger om operativsystemet";
                AddInfosHardCheckBox.Content = "Inkluder oplysninger om computerhardware";
                AddImportedFilesCheckBox.Content = "Inkluder filer importeret siden opstart";
                IncludeAddressListCheckBox.Content = "Inkluder liste over gruppeadresser slettet fra projekter";
                CreateArchiveDebugText.Text = "Opret fejlfindingsfil";
                OngletDebug.Header = "Fejlfindings";
                OngletInformations.Header = "Information";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nBuild {App.AppBuild}" +
                                        $"\n" +
                                        $"\nSoftware skabt som en del af en ingeniørpraktik af studerende fra INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE og Maxime OLIVEIRA LOPES" +
                                        $"\n" +
                                        $"\nUnder vejledning af:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nPartnerskab mellem National Institute of Applied Sciences (INSA) i Toulouse og Union Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nOprettelse: 06/2024 - 07/2024\n";
                SaveButtonText.Text = "Gem";
                CancelButtonText.Text = "Annuller";
                    
                ScalingText.Text = "Skalering:";
                OngletParametresApplication.Header = "Generel";
                    
                NoteImportante.Text = "\nVigtig bemærkning:";
                NoteImportanteContenu.Text = "navnet, logoerne og alle billeder relateret til KNX er uadskillelig ejendom tilhørende KNX-foreningen. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX-foreningens hjemmeside");
                break;

            // Allemand
            case "DE":
                SettingsWindowTopTitle.Text = "Einstellungen";

                AppSettingsTitle.Text = "Anwendungseinstellungen";
                ThemeTextBox.Text = "Thema:";
                LightThemeComboBoxItem.Content = "Hell (Standard)";
                DarkThemeComboBoxItem.Content = "Dunkel";

                AppLanguageTextBlock.Text = "Anwendungssprache:";

                MenuDebug.Text = "Debug-Menü";
                AddInfosOsCheckBox.Content = "Betriebssysteminformationen hinzufügen";
                AddInfosHardCheckBox.Content = "Hardwareinformationen hinzufügen";
                AddImportedFilesCheckBox.Content = "Importierte Projektdateien seit dem Start hinzufügen";
                IncludeAddressListCheckBox.Content = "Liste der gelöschten Gruppenadressen in Projekten hinzufügen";

                CreateArchiveDebugText.Text = "Debug-Datei erstellen";

                OngletDebug.Header = "Debuggen";
                    
                OngletInformations.Header = "Informationen";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware im Rahmen eines Ingenieurpraktikums von Studenten der INSA Toulouse entwickelt:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE und Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nUnter der Aufsicht von:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerschaft zwischen dem Institut National des Sciences Appliquées (INSA) de Toulouse und der Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nUmsetzung: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Speichern";
                CancelButtonText.Text = "Abbrechen";
                    
                ScalingText.Text = "Skalierung:";
                OngletParametresApplication.Header = "Allgemein";
                    
                NoteImportante.Text = "\nWichtiger Hinweis:";
                NoteImportanteContenu.Text = "der Name, die Logos und alle Bilder im Zusammenhang mit KNX sind unveräußerliches Eigentum der KNX-Vereinigung. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Website der KNX-Vereinigung");
                break;

            // Grec
            case "EL":
                SettingsWindowTopTitle.Text = "Ρυθμίσεις";
                AppSettingsTitle.Text = "Ρυθμίσεις εφαρμογής";
                ThemeTextBox.Text = "Θέμα:";
                LightThemeComboBoxItem.Content = "Φωτεινό (προεπιλογή)";
                DarkThemeComboBoxItem.Content = "Σκοτεινό";
                AppLanguageTextBlock.Text = "Γλώσσα εφαρμογής:";
                MenuDebug.Text = "Μενού εντοπισμού σφαλμάτων";
                AddInfosOsCheckBox.Content = "Συμπερίληψη πληροφοριών λειτουργικού συστήματος";
                AddInfosHardCheckBox.Content = "Συμπερίληψη πληροφοριών υλικού υπολογιστή";
                AddImportedFilesCheckBox.Content = "Συμπερίληψη αρχείων που εισάγονται από την εκκίνηση";
                IncludeAddressListCheckBox.Content = "Συμπερίληψη λίστας διαγραμμένων ομαδικών διευθύνσεων στα έργα";
                CreateArchiveDebugText.Text = "Δημιουργία αρχείου εντοπισμού σφαλμάτων";
                OngletDebug.Header = "Εντοπισμός σφαλμάτων";
                OngletInformations.Header = "Πληροφορίες";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nΈκδοση {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nΚατασκευή {App.AppBuild}" +
                                        $"\n" +
                                        $"\nΛογισμικό που δημιουργήθηκε ως μέρος της μηχανικής πρακτικής από φοιτητές της INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE και Maxime OLIVEIRA LOPES" +
                                        $"\n" +
                                        $"\nΥπό την επίβλεψη:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nΣυνεργασία μεταξύ του Εθνικού Ινστιτούτου Εφαρμοσμένων Επιστημών (INSA) της Τουλούζης και της Ένωσης Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nΔημιουργία: 06/2024 - 07/2024\n";
                SaveButtonText.Text = "Αποθήκευση";
                CancelButtonText.Text = "Άκυρο";
                    
                ScalingText.Text = "Κλιμάκωση:";
                OngletParametresApplication.Header = "Γενικός";
                    
                NoteImportante.Text = "\nΣημαντική σημείωση:";
                NoteImportanteContenu.Text = "το όνομα, τα λογότυπα και οποιαδήποτε εικόνα που σχετίζεται με το KNX είναι αδιαίρετη ιδιοκτησία του συλλόγου KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Ιστότοπος του συλλόγου KNX");
                break;

            // Anglais
            case "EN":
                SettingsWindowTopTitle.Text = "Settings";
                AppSettingsTitle.Text = "Application settings";
                ThemeTextBox.Text = "Theme:";
                LightThemeComboBoxItem.Content = "Light (default)";
                DarkThemeComboBoxItem.Content = "Dark";

                AppLanguageTextBlock.Text = "Application language:";

                MenuDebug.Text = "Debug menu";
                AddInfosOsCheckBox.Content = "Include operating system information";
                AddInfosHardCheckBox.Content = "Include computer hardware information";
                AddImportedFilesCheckBox.Content = "Include project files imported since launch";
                IncludeAddressListCheckBox.Content = "Include list of deleted group addresses in projects";

                CreateArchiveDebugText.Text = "Create debug file";

                OngletDebug.Header = "Debugging";
                    
                OngletInformations.Header = "Information";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware developed as part of an engineering internship by students of INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE and Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nUnder the supervision of:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnership between the Institut National des Sciences Appliquées (INSA) de Toulouse and the Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nImplementation: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Save";
                CancelButtonText.Text = "Cancel";
                    
                ScalingText.Text = "Scaling:";
                OngletParametresApplication.Header = "General";
                    
                NoteImportante.Text = "\nImportant note:";
                NoteImportanteContenu.Text = "the name, logos, and any images related to KNX are the inalienable property of the KNX association. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX association website");
                break;

            // Espagnol
            case "ES":
                SettingsWindowTopTitle.Text = "Configuraciones";
                AppSettingsTitle.Text = "Configuraciones de la aplicación";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Claro (predeterminado)";
                DarkThemeComboBoxItem.Content = "Oscuro";

                AppLanguageTextBlock.Text = "Idioma de la aplicación:";

                MenuDebug.Text = "Menú de depuración";
                AddInfosOsCheckBox.Content = "Incluir información del sistema operativo";
                AddInfosHardCheckBox.Content = "Incluir información de hardware del ordenador";
                AddImportedFilesCheckBox.Content = "Incluir archivos de proyectos importados desde el inicio";
                IncludeAddressListCheckBox.Content = "Incluir lista de direcciones de grupo eliminadas en los proyectos";

                CreateArchiveDebugText.Text = "Crear archivo de depuración";

                OngletDebug.Header = "Depuración";
                    
                OngletInformations.Header = "Información";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersión {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nCompilación {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware desarrollado como parte de una pasantía de ingeniería por estudiantes de INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE y Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nBajo la supervisión de:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nAsociación entre el Instituto Nacional de Ciencias Aplicadas (INSA) de Toulouse y la Unión Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nImplementación: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Guardar";
                CancelButtonText.Text = "Cancelar";
                    
                ScalingText.Text = "Escalado:";
                OngletParametresApplication.Header = "General";
                    
                NoteImportante.Text = "\nNota importante:";
                NoteImportanteContenu.Text = "el nombre, los logotipos y cualquier imagen relacionada con KNX son propiedad inalienable de la asociación KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Sitio web de la asociación KNX");
                break;

            // Estonien
            case "ET":
                SettingsWindowTopTitle.Text = "Seaded";
                AppSettingsTitle.Text = "Rakenduse seaded";
                ThemeTextBox.Text = "Teema:";
                LightThemeComboBoxItem.Content = "Hele (vaikimisi)";
                DarkThemeComboBoxItem.Content = "Tume";
                AppLanguageTextBlock.Text = "Rakenduse keel:";
                MenuDebug.Text = "Silumise menüü";
                AddInfosOsCheckBox.Content = "Lisage teave operatsioonisüsteemi kohta";
                AddInfosHardCheckBox.Content = "Lisage teave arvuti riistvara kohta";
                AddImportedFilesCheckBox.Content = "Lisage käivitamisest imporditud failid";
                IncludeAddressListCheckBox.Content = "Lisage projektidest eemaldatud rühma aadresside loend";
                CreateArchiveDebugText.Text = "Loo silumisfail";
                OngletDebug.Header = "Silumine";
                OngletInformations.Header = "Teave";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nVersioon {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nKoostamine {App.AppBuild}" +
                                        $"\n" +
                                        $"\nTarkvara, mille lõid osana inseneripraktikast INSA Toulouse üliõpilased:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ja Maxime OLIVEIRA LOPES" +
                                        $"\n" +
                                        $"\nJärelevalve all:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nPartnerlus Toulouse'i Rakendusteaduste Riikliku Instituudi (INSA) ja Union Cépière Robert Monnier (UCRM) vahel." +
                                        $"\n" +
                                        $"\nLoomine: 06/2024 - 07/2024\n";
                SaveButtonText.Text = "Salvesta";
                CancelButtonText.Text = "Tühista";
                    
                ScalingText.Text = "Skaala:";
                OngletParametresApplication.Header = "Üldine";
                NoteImportante.Text = "\nOluline märkus:";
                NoteImportanteContenu.Text = "KNX-i nimi, logod ja kõik pildid, mis on seotud KNX-iga, on KNX-i ühenduse võõrandamatu omand. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX ühenduse veebisait");
                break;

            // Finnois
            case "FI":
                SettingsWindowTopTitle.Text = "Asetukset";
                AppSettingsTitle.Text = "Sovelluksen asetukset";
                ThemeTextBox.Text = "Teema:";
                LightThemeComboBoxItem.Content = "Vaalea (oletus)";
                DarkThemeComboBoxItem.Content = "Tumma";

                AppLanguageTextBlock.Text = "Sovelluksen kieli:";

                MenuDebug.Text = "Virheenkorjausvalikko";
                AddInfosOsCheckBox.Content = "Sisällytä käyttöjärjestelmän tiedot";
                AddInfosHardCheckBox.Content = "Sisällytä tietokoneen laitteistotiedot";
                AddImportedFilesCheckBox.Content = "Sisällytä aloituksen jälkeen tuodut projektitiedostot";
                IncludeAddressListCheckBox.Content = "Sisällytä poistettujen ryhmäosoitteiden luettelo projekteihin";

                CreateArchiveDebugText.Text = "Luo virheenkorjaustiedosto";
                    
                OngletDebug.Header = "Virheenkorjaus";
                    
                OngletInformations.Header = "Tiedot";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersio {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nKokoelma {App.AppBuild}" +
                    $"\n" +
                    $"\nOhjelmisto kehitetty osana INSAn Toulousen insinööriharjoittelua:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ja Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nValvonnassa:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nYhteistyö Instituutti National des Sciences Appliquées (INSA) de Toulousen ja Union Cépière Robert Monnier (UCRM) välillä." +
                    $"\n" +
                    $"\nToteutus: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Tallenna";
                CancelButtonText.Text = "Peruuta";
                    
                ScalingText.Text = "Skaalaus:";
                OngletParametresApplication.Header = "Yleinen";
                
                NoteImportante.Text = "\nTärkeä huomautus:";
                NoteImportanteContenu.Text = "KNX:n nimi, logot ja kaikki kuvat, jotka liittyvät KNX:ään, ovat KNX-yhdistyksen luovuttamatonta omaisuutta. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX-yhdistyksen verkkosivusto");
                break;

            // Hongrois
            case "HU":
                SettingsWindowTopTitle.Text = "Beállítások";
                AppSettingsTitle.Text = "Alkalmazás beállításai";
                ThemeTextBox.Text = "Téma:";
                LightThemeComboBoxItem.Content = "Világos (alapértelmezett)";
                DarkThemeComboBoxItem.Content = "Sötét";

                AppLanguageTextBlock.Text = "Alkalmazás nyelve:";

                MenuDebug.Text = "Hibakeresési menü";
                AddInfosOsCheckBox.Content = "Tartalmazza az operációs rendszer információit";
                AddInfosHardCheckBox.Content = "Tartalmazza a számítógép hardverinformációit";
                AddImportedFilesCheckBox.Content = "Tartalmazza az indítás óta importált projektek fájljait";
                IncludeAddressListCheckBox.Content = "Tartalmazza a projektekben törölt csoport címek listáját";

                CreateArchiveDebugText.Text = "Hibakeresési fájl létrehozása";

                OngletDebug.Header = "Hibakeresés";
                    
                OngletInformations.Header = "Információk";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVerzió {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSzoftver az INSA Toulouse mérnöki szakmai gyakorlat keretében fejlesztett:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE és Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nFelügyelete alatt:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerség az Institut National des Sciences Appliquées (INSA) de Toulouse és az Union Cépière Robert Monnier (UCRM) között." +
                    $"\n" +
                    $"\nMegvalósítás: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Mentés";
                CancelButtonText.Text = "Mégse";
                    
                ScalingText.Text = "Méretezés:";
                OngletParametresApplication.Header = "Általános";
                    
                NoteImportante.Text = "\nFontos megjegyzés:";
                NoteImportanteContenu.Text = "a KNX név, logók és bármilyen kép, amely a KNX-hez kapcsolódik, a KNX egyesület elidegeníthetetlen tulajdona. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX egyesület weboldala");
                break;

            // Indonésien
            case "ID":
                SettingsWindowTopTitle.Text = "Pengaturan";
                AppSettingsTitle.Text = "Pengaturan aplikasi";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Terang (default)";
                DarkThemeComboBoxItem.Content = "Gelap";

                AppLanguageTextBlock.Text = "Bahasa aplikasi:";

                MenuDebug.Text = "Menu debug";
                AddInfosOsCheckBox.Content = "Sertakan informasi sistem operasi";
                AddInfosHardCheckBox.Content = "Sertakan informasi perangkat keras komputer";
                AddImportedFilesCheckBox.Content = "Sertakan file proyek yang diimpor sejak diluncurkan";
                IncludeAddressListCheckBox.Content = "Sertakan daftar alamat grup yang dihapus dalam proyek";

                CreateArchiveDebugText.Text = "Buat file debug";

                OngletDebug.Header = "Debug";
                    
                OngletInformations.Header = "Informasi";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersi {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nPerangkat lunak dikembangkan sebagai bagian dari magang teknik oleh siswa INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE dan Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nDi bawah pengawasan:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nKemitraan antara Institut National des Sciences Appliquées (INSA) de Toulouse dan Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nImplementasi: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Simpan";
                CancelButtonText.Text = "Batal";
                    
                ScalingText.Text = "Skalasi:";
                OngletParametresApplication.Header = "Umum";
                    
                NoteImportante.Text = "\nCatatan penting:";
                NoteImportanteContenu.Text = "nama, logo, dan gambar apapun yang terkait dengan KNX adalah milik tidak terpisahkan dari asosiasi KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Situs web asosiasi KNX");
                break;

            // Italien
            case "IT":
                SettingsWindowTopTitle.Text = "Impostazioni";
                AppSettingsTitle.Text = "Impostazioni dell'app";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Chiaro (predefinito)";
                DarkThemeComboBoxItem.Content = "Scuro";

                AppLanguageTextBlock.Text = "Lingua dell'applicazione:";

                MenuDebug.Text = "Menu di debug";
                AddInfosOsCheckBox.Content = "Includi informazioni sul sistema operativo";
                AddInfosHardCheckBox.Content = "Includi informazioni sull'hardware del computer";
                AddImportedFilesCheckBox.Content = "Includi i file dei progetti importati dall'avvio";
                IncludeAddressListCheckBox.Content = "Includi l'elenco degli indirizzi di gruppo eliminati nei progetti";

                CreateArchiveDebugText.Text = "Crea file di debug";

                OngletDebug.Header = "Debug";
                    
                OngletInformations.Header = "Informazioni";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersione {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware sviluppato nell'ambito di uno stage di ingegneria da studenti dell'INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE e Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nSotto la supervisione di:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartenariato tra l'Istituto Nazionale delle Scienze Applicate (INSA) di Tolosa e l'Unione Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizzazione: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Salva";
                CancelButtonText.Text = "Annulla";
                    
                ScalingText.Text = "Ridimensionamento:";
                OngletParametresApplication.Header = "Generale";
                    
                NoteImportante.Text = "\nNota importante:";
                NoteImportanteContenu.Text = "il nome, i loghi e tutte le immagini relative a KNX sono proprietà inalienabile dell'associazione KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Sito web dell'associazione KNX");
                break;

            // Japonais
            case "JA":
                SettingsWindowTopTitle.Text = "設定";
                AppSettingsTitle.Text = "アプリ設定";
                ThemeTextBox.Text = "テーマ:";
                LightThemeComboBoxItem.Content = "ライト（デフォルト）";
                DarkThemeComboBoxItem.Content = "ダーク";

                AppLanguageTextBlock.Text = "アプリの言語:";

                MenuDebug.Text = "デバッグメニュー";
                AddInfosOsCheckBox.Content = "オペレーティングシステム情報を含める";
                AddInfosHardCheckBox.Content = "コンピュータのハードウェア情報を含める";
                AddImportedFilesCheckBox.Content = "起動以来インポートされたプロジェクトファイルを含める";
                IncludeAddressListCheckBox.Content = "プロジェクトに削除されたグループアドレスのリストを含める";

                CreateArchiveDebugText.Text = "デバッグファイルを作成";

                OngletDebug.Header = "デバッグ";
                    
                OngletInformations.Header = "情報";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nバージョン {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nビルド {App.AppBuild}" +
                    $"\n" +
                    $"\nINSAトゥールーズの学生によるエンジニアリングインターンシップの一環として開発されたソフトウェア:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\n監督の下:" +
                    $"\nDidier BESSE（UCRM）" +
                    $"\nThierry COPPOLA（UCRM）" +
                    $"\nJean-François KLOTZ（LECS）" +
                    $"\n" +
                    $"\nトゥールーズ国立応用科学研究所（INSA）とシェピエールロバートモニエ連合（UCRM）のパートナーシップ。" +
                    $"\n" +
                    $"\n実装: 2024年06月 - 2024年07月\n";
                        
                SaveButtonText.Text = "保存";
                CancelButtonText.Text = "キャンセル";
                    
                ScalingText.Text = "スケーリング:";
                OngletParametresApplication.Header = "一般";
                    
                NoteImportante.Text = "\n重要な注意:";
                NoteImportanteContenu.Text = "KNXの名前、ロゴ、およびKNXに関連するすべての画像は、KNX協会の不可分の財産です。 \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX協会のウェブサイト");
                break;

            // Coréen
            case "KO":
                SettingsWindowTopTitle.Text = "설정";
                AppSettingsTitle.Text = "앱 설정";
                ThemeTextBox.Text = "테마:";
                LightThemeComboBoxItem.Content = "라이트 (기본)";
                DarkThemeComboBoxItem.Content = "다크";

                AppLanguageTextBlock.Text = "앱 언어:";

                MenuDebug.Text = "디버그 메뉴";
                AddInfosOsCheckBox.Content = "운영 체제 정보 포함";
                AddInfosHardCheckBox.Content = "컴퓨터 하드웨어 정보 포함";
                AddImportedFilesCheckBox.Content = "시작 후 가져온 프로젝트 파일 포함";
                IncludeAddressListCheckBox.Content = "프로젝트에서 삭제된 그룹 주소 목록 포함";

                CreateArchiveDebugText.Text = "디버그 파일 생성";

                OngletDebug.Header = "디버그";
                    
                OngletInformations.Header = "정보";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\n버전 {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\n빌드 {App.AppBuild}" +
                    $"\n" +
                    $"\nINSA 툴루즈 학생들이 엔지니어링 인턴십의 일환으로 개발한 소프트웨어:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\n감독 하에:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\n툴루즈 국립 응용 과학 연구소 (INSA)와 Union Cépière Robert Monnier (UCRM) 간의 파트너십." +
                    $"\n" +
                    $"\n실행: 2024년 6월 - 2024년 7월\n";
                        
                SaveButtonText.Text = "저장";
                CancelButtonText.Text = "취소";
                    
                ScalingText.Text = "확대/축소:";
                OngletParametresApplication.Header = "일반";
                NoteImportante.Text = "\n중요한 참고 사항:";
                NoteImportanteContenu.Text = "KNX의 이름, 로고 및 KNX와 관련된 모든 이미지는 KNX 협회의 양도할 수 없는 자산입니다. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX 협회 웹사이트");
                break;

            // Letton
            case "LV":
                SettingsWindowTopTitle.Text = "Iestatījumi";
                AppSettingsTitle.Text = "Lietotnes iestatījumi";
                ThemeTextBox.Text = "Tēma:";
                LightThemeComboBoxItem.Content = "Gaišs (noklusējums)";
                DarkThemeComboBoxItem.Content = "Tumšs";

                AppLanguageTextBlock.Text = "Lietotnes valoda:";

                MenuDebug.Text = "Problēmu novēršana";
                AddInfosOsCheckBox.Content = "Iekļaut operētājsistēmas informāciju";
                AddInfosHardCheckBox.Content = "Iekļaut datora aparatūras informāciju";
                AddImportedFilesCheckBox.Content = "Iekļaut projektos importēto failu informāciju";
                IncludeAddressListCheckBox.Content = "Iekļaut grupu adreses, kas dzēstas no projektiem";

                CreateArchiveDebugText.Text = "Izveidot problēmu novēršanas failu";

                OngletDebug.Header = "Problēmu novēršana";
                                    
                OngletInformations.Header = "Informācija";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersija {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBūvēt {App.AppBuild}" +
                    $"\n" +
                    $"\nProgrammatūra izstrādāta INSA Toulouse inženierijas prakses ietvaros:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE un Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nPārraudzīja:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerība starp National Institute of Applied Sciences (INSA) Toulouse un Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nIzstrāde: 06/2024 - 07/2024\n";
                                        
                SaveButtonText.Text = "Saglabāt";
                CancelButtonText.Text = "Atcelt";
                
                ScalingText.Text = "Mērogošana:";
                OngletParametresApplication.Header = "Vispārīgs";
                    
                NoteImportante.Text = "\nSvarīga piezīme:";
                NoteImportanteContenu.Text = "KNX nosaukums, logotipi un jebkādi attēli, kas saistīti ar KNX, ir KNX asociācijas neatņemams īpašums. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX asociācijas tīmekļa vietne");
                break;

            // Lituanien
            case "LT":
                SettingsWindowTopTitle.Text = "Nustatymai";
                AppSettingsTitle.Text = "Programėlės nustatymai";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Šviesi (numatytoji)";
                DarkThemeComboBoxItem.Content = "Tamsi";

                AppLanguageTextBlock.Text = "Programėlės kalba:";

                MenuDebug.Text = "Derinimo meniu";
                AddInfosOsCheckBox.Content = "Įtraukti operacinės sistemos informaciją";
                AddInfosHardCheckBox.Content = "Įtraukti kompiuterio aparatūros informaciją";
                AddImportedFilesCheckBox.Content = "Įtraukti importuotų projektų failus";
                IncludeAddressListCheckBox.Content = "Įtraukti iš projektų ištrintų grupių adresų sąrašą";

                CreateArchiveDebugText.Text = "Sukurti derinimo failą";

                OngletDebug.Header = "Derinimas";
                            
                OngletInformations.Header = "Informacija";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersija {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nKūrimas {App.AppBuild}" +
                    $"\n" +
                    $"\nPrograminė įranga sukurta INSA Toulouse inžinerijos praktikos metu:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ir Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nPrižiūrėtojai:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerystė tarp Nacionalinio taikomųjų mokslų instituto (INSA) Tulūzoje ir Sąjungos Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizacija: 06/2024 - 07/2024\n";
                                
                SaveButtonText.Text = "Išsaugoti";
                CancelButtonText.Text = "Atšaukti";
                
                ScalingText.Text = "Mastelio keitimas:";
                OngletParametresApplication.Header = "Bendras";
                    
                NoteImportante.Text = "\nSvarbi pastaba:";
                NoteImportanteContenu.Text = "KNX pavadinimas, logotipai ir bet kokie su KNX susiję vaizdai yra neatsiejama KNX asociacijos nuosavybė. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX asociacijos svetainė");
                break;

            // Norvégien
            case "NB":
                SettingsWindowTopTitle.Text = "Innstillinger";
                AppSettingsTitle.Text = "Appinnstillinger";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Lys (standard)";
                DarkThemeComboBoxItem.Content = "Mørk";

                AppLanguageTextBlock.Text = "Appspråk:";

                MenuDebug.Text = "Feilsøkingsmeny";
                AddInfosOsCheckBox.Content = "Inkluder informasjon om operativsystemet";
                AddInfosHardCheckBox.Content = "Inkluder informasjon om datamaskinens maskinvare";
                AddImportedFilesCheckBox.Content = "Inkluder filer importert til prosjekter siden oppstart";
                IncludeAddressListCheckBox.Content = "Inkluder listen over fjernede gruppeadresser fra prosjekter";

                CreateArchiveDebugText.Text = "Opprett feilsøkingsfil";

                OngletDebug.Header = "Feilsøking";
                            
                OngletInformations.Header = "Informasjon";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersjon {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBygg {App.AppBuild}" +
                    $"\n" +
                    $"\nProgramvare laget som en del av et ingeniørpraksis ved INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE og Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nUnder veiledning av:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerskap mellom National Institute of Applied Sciences (INSA) i Toulouse og Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nUtførelse: 06/2024 - 07/2024\n";
                                
                SaveButtonText.Text = "Lagre";
                CancelButtonText.Text = "Avbryt";
                
                ScalingText.Text = "Skalering:";
                OngletParametresApplication.Header = "Generell";
                    
                NoteImportante.Text = "\nViktig merknad:";
                NoteImportanteContenu.Text = "navnet, logoene og alle bilder knyttet til KNX er udelelig eiendom tilhørende KNX-foreningen. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX-foreningens nettsted");
                break;

            // Néerlandais
            case "NL":
                SettingsWindowTopTitle.Text = "Instellingen";
                AppSettingsTitle.Text = "Applicatie instellingen";
                ThemeTextBox.Text = "Thema:";
                LightThemeComboBoxItem.Content = "Licht (standaard)";
                DarkThemeComboBoxItem.Content = "Donker";

                AppLanguageTextBlock.Text = "Applicatietaal:";

                MenuDebug.Text = "Debug-menu";
                AddInfosOsCheckBox.Content = "Inclusief OS-informatie";
                AddInfosHardCheckBox.Content = "Inclusief hardware-informatie";
                AddImportedFilesCheckBox.Content = "Inclusief geïmporteerde projectbestanden sinds de start";
                IncludeAddressListCheckBox.Content = "Inclusief verwijderde groepsadreslijst in projecten";

                CreateArchiveDebugText.Text = "Maak debugbestand aan";

                OngletDebug.Header = "Debug";
                    
                OngletInformations.Header = "Informatie";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersie {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware gemaakt in het kader van een ingenieursstage door studenten van INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE en Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nOnder supervisie van:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerschap tussen het Institut National des Sciences Appliquées (INSA) van Toulouse en de Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealisatie: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Opslaan";
                CancelButtonText.Text = "Annuleren";
                    
                ScalingText.Text = "Schaal:";
                OngletParametresApplication.Header = "Algemeen";
                    
                NoteImportante.Text = "\nBelangrijke opmerking:";
                NoteImportanteContenu.Text = "de naam, logo's en alle afbeeldingen die verband houden met KNX zijn het onvervreemdbaar eigendom van de KNX-vereniging. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Website van de KNX-vereniging");
                break;

            // Polonais
            case "PL":
                SettingsWindowTopTitle.Text = "Ustawienia";
                AppSettingsTitle.Text = "Ustawienia aplikacji";
                ThemeTextBox.Text = "Temat:";
                LightThemeComboBoxItem.Content = "Jasny (domyślnie)";
                DarkThemeComboBoxItem.Content = "Ciemny";

                AppLanguageTextBlock.Text = "Język aplikacji:";

                MenuDebug.Text = "Menu debugowania";
                AddInfosOsCheckBox.Content = "Dołącz informacje o systemie operacyjnym";
                AddInfosHardCheckBox.Content = "Dołącz informacje o sprzęcie";
                AddImportedFilesCheckBox.Content = "Dołącz pliki projektów zaimportowane od uruchomienia";
                IncludeAddressListCheckBox.Content = "Dołącz listę usuniętych adresów grup w projektach";

                CreateArchiveDebugText.Text = "Utwórz plik debugowania";

                OngletDebug.Header = "Debugowanie";
                    
                OngletInformations.Header = "Informacje";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nWersja {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nOprogramowanie stworzone w ramach praktyk inżynierskich przez studentów INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE i Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nPod nadzorem:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerstwo między Institut National des Sciences Appliquées (INSA) w Tuluzie a Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizacja: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Zapisz";
                CancelButtonText.Text = "Anuluj";
                    
                ScalingText.Text = "Skalowanie:";
                OngletParametresApplication.Header = "Ogólne";
                NoteImportante.Text = "\nWażna uwaga:";
                NoteImportanteContenu.Text = "nazwa, logo i wszystkie obrazy związane z KNX są niezbywalną własnością stowarzyszenia KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Strona internetowa stowarzyszenia KNX");
                break;

            // Portugais
            case "PT":
                SettingsWindowTopTitle.Text = "Configurações";
                AppSettingsTitle.Text = "Configurações do aplicativo";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Claro (padrão)";
                DarkThemeComboBoxItem.Content = "Escuro";

                AppLanguageTextBlock.Text = "Idioma do aplicativo:";

                MenuDebug.Text = "Menu de depuração";
                AddInfosOsCheckBox.Content = "Incluir informações do sistema operacional";
                AddInfosHardCheckBox.Content = "Incluir informações de hardware";
                AddImportedFilesCheckBox.Content = "Incluir arquivos de projetos importados desde o início";
                IncludeAddressListCheckBox.Content = "Incluir lista de endereços de grupo removidos nos projetos";

                CreateArchiveDebugText.Text = "Criar arquivo de depuração";

                OngletDebug.Header = "Depuração";
                    
                OngletInformations.Header = "Informações";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersão {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware realizado no âmbito de um estágio de engenharia por estudantes da INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE e Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nSob a supervisão de:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nParceria entre o Institut National des Sciences Appliquées (INSA) de Toulouse e a Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealização: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Salvar";
                CancelButtonText.Text = "Cancelar";
                    
                ScalingText.Text = "Dimensionamento:";
                OngletParametresApplication.Header = "Geral";
                NoteImportante.Text = "\nNota importante:";
                NoteImportanteContenu.Text = "o nome, logotipos e quaisquer imagens relacionadas com KNX são propriedade inalienável da associação KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Website da associação KNX");
                break;

            // Roumain
            case "RO":
                SettingsWindowTopTitle.Text = "Setări";
                AppSettingsTitle.Text = "Setările aplicației";
                ThemeTextBox.Text = "Temă:";
                LightThemeComboBoxItem.Content = "Deschis (implicit)";
                DarkThemeComboBoxItem.Content = "Întunecat";

                AppLanguageTextBlock.Text = "Limba aplicației:";

                MenuDebug.Text = "Meniu depanare";
                AddInfosOsCheckBox.Content = "Includeți informațiile despre sistemul de operare";
                AddInfosHardCheckBox.Content = "Includeți informațiile despre hardware-ul computerului";
                AddImportedFilesCheckBox.Content = "Includeți fișierele importate în proiecte de la pornire";
                IncludeAddressListCheckBox.Content = "Includeți lista adreselor de grup șterse din proiecte";

                CreateArchiveDebugText.Text = "Creați fișierul de depanare";

                OngletDebug.Header = "Depanare";
                            
                OngletInformations.Header = "Informații";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersiune {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware creat în cadrul unui stagiu de inginerie la INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE și Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nSub supravegherea lui:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nParteneriat între Institutul Național de Științe Aplicate (INSA) din Toulouse și Uniunea Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizare: 06/2024 - 07/2024\n";
                                
                SaveButtonText.Text = "Salvați";
                CancelButtonText.Text = "Anulați";
                
                ScalingText.Text = "Scalare:";
                OngletParametresApplication.Header = "General";
                    
                NoteImportante.Text = "\nNotă importantă:";
                NoteImportanteContenu.Text = "numele, siglele și orice imagine legată de KNX sunt proprietatea inalienabilă a asociației KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Site-ul asociației KNX");
                break;

            // Slovaque
            case "SK":
                SettingsWindowTopTitle.Text = "Nastavenia";
                AppSettingsTitle.Text = "Nastavenia aplikácie";
                ThemeTextBox.Text = "Téma:";
                LightThemeComboBoxItem.Content = "Svetlá (predvolená)";
                DarkThemeComboBoxItem.Content = "Tmavá";

                AppLanguageTextBlock.Text = "Jazyk aplikácie:";

                MenuDebug.Text = "Ladiace menu";
                AddInfosOsCheckBox.Content = "Zahrnúť informácie o operačnom systéme";
                AddInfosHardCheckBox.Content = "Zahrnúť informácie o hardvéri počítača";
                AddImportedFilesCheckBox.Content = "Zahrnúť súbory importované do projektov od spustenia";
                IncludeAddressListCheckBox.Content = "Zahrnúť zoznam odstránených skupinových adries z projektov";

                CreateArchiveDebugText.Text = "Vytvoriť ladiaci súbor";

                OngletDebug.Header = "Ladenie";
                            
                OngletInformations.Header = "Informácie";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVerzia {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftvér vytvorený v rámci inžinierskej stáže na INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE a Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nPod dohľadom:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerstvo medzi Národným inštitútom aplikovaných vied (INSA) v Toulouse a Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizácia: 06/2024 - 07/2024\n";
                                
                SaveButtonText.Text = "Uložiť";
                CancelButtonText.Text = "Zrušiť";
                
                ScalingText.Text = "Mierka:";
                OngletParametresApplication.Header = "Všeobecné";
                    
                NoteImportante.Text = "\nDôležitá poznámka:";
                NoteImportanteContenu.Text = "názov, logá a akékoľvek obrázky týkajúce sa KNX sú neoddeliteľným majetkom združenia KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Webová stránka združenia KNX");
                break;

            // Slovène
            case "SL":
                SettingsWindowTopTitle.Text = "Nastavitve";

                AppSettingsTitle.Text = "Nastavitve aplikacije";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Svetla (privzeta)";
                DarkThemeComboBoxItem.Content = "Temna";

                AppLanguageTextBlock.Text = "Jezik aplikacije:";

                MenuDebug.Text = "Meni za odpravljanje napak";
                AddInfosOsCheckBox.Content = "Vključi informacije o operacijskem sistemu";
                AddInfosHardCheckBox.Content = "Vključi informacije o strojni opremi računalnika";
                AddImportedFilesCheckBox.Content = "Vključi datoteke, uvožene v projekte od zagona";
                IncludeAddressListCheckBox.Content = "Vključi seznam izbrisanih skupinskih naslovov iz projektov";

                CreateArchiveDebugText.Text = "Ustvari datoteko za odpravljanje napak";

                OngletDebug.Header = "Odpravljanje napak";
                            
                OngletInformations.Header = "Informacije";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nRazličica {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nIzgradnja {App.AppBuild}" +
                    $"\n" +
                    $"\nProgramska oprema, izdelana v okviru inženirskega pripravništva na INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE in Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nPod nadzorom:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerstvo med Nacionalnim inštitutom za uporabne znanosti (INSA) v Toulouseu in Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nIzvedba: 06/2024 - 07/2024\n";
                                
                SaveButtonText.Text = "Shrani";
                CancelButtonText.Text = "Prekliči";
                
                ScalingText.Text = "Spreminjanje velikosti:";
                OngletParametresApplication.Header = "Splošno";
                    
                NoteImportante.Text = "\nPomembna opomba:";
                NoteImportanteContenu.Text = "ime, logotipi in vse slike, povezane s KNX, so neodtujljiva last združenja KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Spletna stran združenja KNX");
                break;

            // Suédois
            case "SV":
                SettingsWindowTopTitle.Text = "Inställningar";
                AppSettingsTitle.Text = "Appinställningar";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Ljus (standard)";
                DarkThemeComboBoxItem.Content = "Mörk";

                AppLanguageTextBlock.Text = "Appens språk:";

                MenuDebug.Text = "Felsökningsmeny";
                AddInfosOsCheckBox.Content = "Inkludera information om operativsystemet";
                AddInfosHardCheckBox.Content = "Inkludera information om hårdvara";
                AddImportedFilesCheckBox.Content = "Inkludera importerade projektfiler sedan start";
                IncludeAddressListCheckBox.Content = "Inkludera lista över borttagna gruppadresser i projekt";

                CreateArchiveDebugText.Text = "Skapa felsökningsfil";

                OngletDebug.Header = "Felsökning";
                    
                OngletInformations.Header = "Information";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nProgramvara utvecklad inom ramen för en ingenjörspraktik av studenter från INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE och Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nUnder överinseende av:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nSamarbete mellan Institut National des Sciences Appliquées (INSA) i Toulouse och Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nGenomförande: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Spara";
                CancelButtonText.Text = "Avbryt";
                    
                ScalingText.Text = "Skalning:";
                OngletParametresApplication.Header = "Allmänt";
                    
                NoteImportante.Text = "\nViktig anmärkning:";
                NoteImportanteContenu.Text = "namnet, logotyperna och alla bilder relaterade till KNX är KNX-föreningens omistliga egendom. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX-föreningens webbplats");
                break;

            // Turc
            case "TR":
                SettingsWindowTopTitle.Text = "Ayarlar";
                AppSettingsTitle.Text = "Uygulama ayarları";
                ThemeTextBox.Text = "Tema:";
                LightThemeComboBoxItem.Content = "Açık (varsayılan)";
                DarkThemeComboBoxItem.Content = "Koyu";

                AppLanguageTextBlock.Text = "Uygulama dili:";

                MenuDebug.Text = "Hata ayıklama menüsü";
                AddInfosOsCheckBox.Content = "İşletim sistemi bilgilerini dahil et";
                AddInfosHardCheckBox.Content = "Donanım bilgilerini dahil et";
                AddImportedFilesCheckBox.Content = "Başlatmadan bu yana ithal edilen proje dosyalarını dahil et";
                IncludeAddressListCheckBox.Content = "Projelerde silinen grup adresi listesini dahil et";

                CreateArchiveDebugText.Text = "Hata ayıklama dosyası oluştur";

                OngletDebug.Header = "Hata ayıklama";
                    
                OngletInformations.Header = "Bilgiler";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nSürüm {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nYapı {App.AppBuild}" +
                    $"\n" +
                    $"\nINSA Toulouse öğrencileri tarafından bir mühendislik stajı kapsamında yapılan yazılım:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ve Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nGözetim altında:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nInstitut National des Sciences Appliquées (INSA) Toulouse ve Union Cépière Robert Monnier (UCRM) arasındaki ortaklık." +
                    $"\n" +
                    $"\nGerçekleştirme: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Kaydet";
                CancelButtonText.Text = "İptal";
                    
                ScalingText.Text = "Ölçeklendirme:";
                OngletParametresApplication.Header = "Genel";
                NoteImportante.Text = "\nÖnemli not:";
                NoteImportanteContenu.Text = "KNX'in adı, logoları ve KNX ile ilgili tüm resimler, KNX derneğinin devredilemez mülküdür. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX derneği web sitesi");
                break;

            // Ukrainien
            case "UK":
                SettingsWindowTopTitle.Text = "Налаштування";
                AppSettingsTitle.Text = "Налаштування додатку";
                ThemeTextBox.Text = "Тема:";
                LightThemeComboBoxItem.Content = "Світла (за замовчуванням)";
                DarkThemeComboBoxItem.Content = "Темна";

                AppLanguageTextBlock.Text = "Мова додатку:";

                MenuDebug.Text = "Меню налагодження";
                AddInfosOsCheckBox.Content = "Включити інформацію про операційну систему";
                AddInfosHardCheckBox.Content = "Включити інформацію про апаратне забезпечення";
                AddImportedFilesCheckBox.Content = "Включити файли проектів, імпортовані з моменту запуску";
                IncludeAddressListCheckBox.Content = "Включити список видалених адрес груп у проектах";

                CreateArchiveDebugText.Text = "Створити файл налагодження";

                OngletDebug.Header = "Налагодження";
                    
                OngletInformations.Header = "Інформація";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nВерсія {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nЗбірка {App.AppBuild}" +
                    $"\n" +
                    $"\nПрограмне забезпечення розроблене в рамках інженерного стажування студентами INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE та Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nПід наглядом:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nПартнерство між Institut National des Sciences Appliquées (INSA) в Тулузі та Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nРеалізація: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Зберегти";
                CancelButtonText.Text = "Скасувати";
                    
                ScalingText.Text = "Масштабування:";
                OngletParametresApplication.Header = "Загальний";
                NoteImportante.Text = "\nВажлива примітка:";
                NoteImportanteContenu.Text = "назва, логотипи та будь-які зображення, пов'язані з KNX, є невід'ємною власністю асоціації KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Вебсайт асоціації KNX");
                break;

            // Russe
            case "RU":
                SettingsWindowTopTitle.Text = "Настройки";
                AppSettingsTitle.Text = "Настройки приложения";
                ThemeTextBox.Text = "Тема:";
                LightThemeComboBoxItem.Content = "Светлая (по умолчанию)";
                DarkThemeComboBoxItem.Content = "Темная";

                AppLanguageTextBlock.Text = "Язык приложения:";

                MenuDebug.Text = "Меню отладки";
                AddInfosOsCheckBox.Content = "Включить информацию о ОС";
                AddInfosHardCheckBox.Content = "Включить информацию о оборудовании";
                AddImportedFilesCheckBox.Content = "Включить файлы проектов, импортированные с момента запуска";
                IncludeAddressListCheckBox.Content = "Включить список удаленных адресов групп в проектах";

                CreateArchiveDebugText.Text = "Создать файл отладки";

                OngletDebug.Header = "Отладка";
                    
                OngletInformations.Header = "Информация";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nВерсия {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nСборка {App.AppBuild}" +
                    $"\n" +
                    $"\nПрограммное обеспечение, разработанное в рамках инженерной стажировки студентами INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE и Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nПод руководством:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nПартнерство между Institut National des Sciences Appliquées (INSA) в Тулузе и Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nРеализация: 06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "Сохранить";
                CancelButtonText.Text = "Отмена";
                    
                ScalingText.Text = "Масштабирование:";
                OngletParametresApplication.Header = "Общий";
                NoteImportante.Text = "\nВажное примечание:";
                NoteImportanteContenu.Text = "название, логотипы и любые изображения, связанные с KNX, являются неотъемлемой собственностью ассоциации KNX. \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Веб-сайт ассоциации KNX");
                break;
                
            // Chinois simplifié
            case "ZH":
                SettingsWindowTopTitle.Text = "设置";

                AppSettingsTitle.Text = "应用设置";
                ThemeTextBox.Text = "主题:";
                LightThemeComboBoxItem.Content = "浅色（默认）";
                DarkThemeComboBoxItem.Content = "深色";

                AppLanguageTextBlock.Text = "应用语言:";

                MenuDebug.Text = "调试菜单";
                AddInfosOsCheckBox.Content = "包括操作系统信息";
                AddInfosHardCheckBox.Content = "包括硬件信息";
                AddImportedFilesCheckBox.Content = "包括启动以来导入的项目文件";
                IncludeAddressListCheckBox.Content = "包括项目中删除的组地址列表";

                CreateArchiveDebugText.Text = "创建调试文件";

                OngletDebug.Header = "调试";
                    
                OngletInformations.Header = "信息";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\n版本 {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\n构建 {App.AppBuild}" +
                    $"\n" +
                    $"\n由INSA Toulouse的学生在工程实习中开发的软件：" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE 和 Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\n在以下人员的指导下：" +
                    $"\nDidier BESSE（UCRM）" +
                    $"\nThierry COPPOLA（UCRM）" +
                    $"\nJean-François KLOTZ（LECS）" +
                    $"\n" +
                    $"\nToulouse的Institut National des Sciences Appliquées（INSA）与Union Cépière Robert Monnier（UCRM）之间的合作。" +
                    $"\n" +
                    $"\n实施：06/2024 - 07/2024\n";
                        
                SaveButtonText.Text = "保存";
                CancelButtonText.Text = "取消";
                    
                ScalingText.Text = "缩放：";
                OngletParametresApplication.Header = "常规";
                
                NoteImportante.Text = "\n重要提示:";
                NoteImportanteContenu.Text = "KNX的名称、标识和任何与KNX相关的图片都是KNX协会不可分割的财产。 \u279e";
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("KNX协会网站");
                break;

            // Langue par défaut (français)
            default:
                SettingsWindowTopTitle.Text = "Paramètres";
                AppSettingsTitle.Text = "Paramètres de l'application";
                ThemeTextBox.Text = "Thème:";
                LightThemeComboBoxItem.Content = "Clair (par défaut)";
                DarkThemeComboBoxItem.Content = "Sombre";

                AppLanguageTextBlock.Text = "Langue de l'application:";

                MenuDebug.Text = "Menu de débogage";
                AddInfosOsCheckBox.Content = "Inclure les informations sur le système d'exploitation";
                AddInfosHardCheckBox.Content = "Inclure les informations sur le matériel de l'ordinateur";
                AddImportedFilesCheckBox.Content = "Inclure les fichiers des projets importés depuis le lancement";
                IncludeAddressListCheckBox.Content = "Inclure la liste des adresses de groupe supprimées sur les projets";

                CreateArchiveDebugText.Text = "Créer le fichier de débogage";
                    
                OngletDebug.Header = "Débogage";
                OngletInformations.Header = "Informations";

                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nLogiciel réalisé dans le cadre d'un stage d'ingénierie par des étudiants de l'INSA Toulouse :" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE et Maxime OLIVEIRA LOPES" +
                    $"\n" +
                    $"\nSous la supervision de :" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartenariat entre l'Institut National des Sciences Appliquées (INSA) de Toulouse et l'Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRéalisation: 06/2024 - 07/2024\n";

                NoteImportante.Text = "\nNote importante:";
                NoteImportanteContenu.Text =
                    " le nom, les logos et toute image liée à KNX sont la propriété inaliénable de l'association KNX. \u279e";
                    
                HyperlinkInfo.Inlines.Clear();
                HyperlinkInfo.Inlines.Add("Site web de l'association KNX");
                        
                SaveButtonText.Text = "Enregistrer";
                CancelButtonText.Text = "Annuler";
                    
                ScalingText.Text = "Mise à l'échelle :";
                OngletParametresApplication.Header = "Général";
                break;
        }
            
    }


    // Fonction qui applique le thème au contenu de la fenêtre
    /// <summary>
    /// This functions applies the light/dark theme to the settings window
    /// </summary>
    private void ApplyThemeToWindow()
    {
        string textColor;
        string darkBackgroundColor;
        string deepDarkBackgroundColor;
        string pathColor;

        var checkboxStyle = (Style)FindResource("CheckboxLightThemeStyle");
        Brush borderBrush;

        if (EnableLightTheme) // Si le thème clair est actif,
        {
            textColor = "#000000";
            darkBackgroundColor = "#F5F5F5";
            deepDarkBackgroundColor = "#FFFFFF";
            pathColor = "#D7D7D7";
            borderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b8b8b8"));

            ThemeComboBox.Style = (Style)FindResource("LightComboBoxStyle");
            AppLanguageComboBox.Style = (Style)FindResource("LightComboBoxStyle");
            ScaleSlider.Style = (Style)FindResource("LightSlider");
            SaveButton.Style = (Style)FindResource("BottomButtonLight");
            CancelButton.Style = (Style)FindResource("BottomButtonLight");
            CreateArchiveDebugButton.Style = (Style)FindResource("BottomButtonLight");
            
            OngletDebug.Style = (Style)FindResource("LightOnglet");
            OngletInformations.Style = (Style)FindResource("LightOnglet");
            OngletParametresApplication.Style = (Style)FindResource("LightOnglet");
            IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked! ? 
            MainWindow.ConvertStringColor(textColor) : new SolidColorBrush(Colors.Gray);
            HyperlinkInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4071B4"));
        }
        else // Sinon, on met le thème sombre
        {
            textColor = "#E3DED4";
            darkBackgroundColor = "#313131";
            deepDarkBackgroundColor = "#262626";
            pathColor = "#434343";
            checkboxStyle = (Style)FindResource("CheckboxDarkThemeStyle");
            borderBrush = (Brush)FindResource("DarkThemeCheckBoxBorderBrush");

            ThemeComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
            AppLanguageComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
            ScaleSlider.Style = (Style)FindResource("DarkSlider");
            SaveButton.Style = (Style)FindResource("BottomButtonDark");
            CancelButton.Style = (Style)FindResource("BottomButtonDark");
            CreateArchiveDebugButton.Style = (Style)FindResource("BottomButtonDark");
            OngletDebug.Style = (Style)FindResource("DarkOnglet");
            OngletInformations.Style = (Style)FindResource("DarkOnglet");
            OngletParametresApplication.Style = (Style)FindResource("DarkOnglet");
            IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked! ? 
                MainWindow.ConvertStringColor(textColor) : new SolidColorBrush(Colors.DimGray);
            HyperlinkInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4071B4"));


        }

        // Définition des brush pour les divers éléments
        var textColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textColor));

        // Arrière plan de la fenêtre
        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor));

        // En-tête de la fenêtre
        SettingsIconPath1.Brush = textColorBrush;
        SettingsIconPath2.Brush = textColorBrush;
        SettingsWindowTopTitle.Foreground = textColorBrush;
        HeaderPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pathColor));

        // Corps de la fenêtre
        MainContentBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pathColor));
        GeneralSettingsTab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
        AppSettingsTitle.Foreground = textColorBrush;
        ThemeTextBox.Foreground = textColorBrush;
        AppLanguageTextBlock.Foreground = textColorBrush;
            

        // Pied de page avec les boutons save et cancel
        SettingsWindowFooter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
        FooterPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pathColor));
        CancelButtonDrawing.Brush = textColorBrush;
        CancelButtonText.Foreground = textColorBrush;
        SaveButtonDrawing.Brush = textColorBrush;
        SaveButtonText.Foreground = textColorBrush;
        CreateArchiveDebugText.Foreground = textColorBrush;

        // Menu debug
        ControlOnglet.BorderBrush = borderBrush;
        DebugPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
        InformationsGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
        AddInfosOsCheckBox.Style = checkboxStyle;
        AddInfosHardCheckBox.Style = checkboxStyle;
        AddImportedFilesCheckBox.Style = checkboxStyle;
        IncludeAddressListCheckBox.Style = checkboxStyle;
        AddInfosOsCheckBox.Foreground = textColorBrush;
        AddInfosHardCheckBox.Foreground = textColorBrush;
        AddImportedFilesCheckBox.Foreground = textColorBrush;
        IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked ? textColorBrush : new SolidColorBrush(Colors.DimGray);



        OngletParametresApplication.BorderBrush = borderBrush;
        OngletDebug.Foreground = textColorBrush;
        OngletParametresApplication.Foreground = textColorBrush;
        DebugBrush1.Brush = textColorBrush;
        DebugBrush2.Brush = textColorBrush;
        OngletInformations.Foreground = textColorBrush;
        InformationsText.Foreground = textColorBrush;

        IncludeAddressListCheckBox.IsEnabled = (bool)AddImportedFilesCheckBox.IsChecked!;
        

        foreach (ComboBoxItem item in ThemeComboBox.Items)
        {
            item.Foreground = item.IsSelected ? new SolidColorBrush(Colors.White) : textColorBrush;
            item.Background = EnableLightTheme ? new SolidColorBrush(Colors.White) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor));
        }


        foreach (ComboBoxItem item in AppLanguageComboBox.Items)
        {
            item.Foreground = item.IsSelected ? new SolidColorBrush(Colors.White) : textColorBrush;
            item.Background = EnableLightTheme ? new SolidColorBrush(Colors.White) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor));
        }
    }


    // ----- GESTION DES BOUTONS -----
    // Fonction s'exécutant lors du clic sur le bouton sauvegarder
    /// <summary>
    /// Handles the save button click event by retrieving and validating settings from the settings window,
    /// saving them, and updating relevant UI elements.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        // Sauvegarde des anciens paramètres
        var previousEnableLightTheme = EnableLightTheme;
        var previousAppLang = AppLang;
        var previousAppScaleFactor = AppScaleFactor;

        // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
        EnableLightTheme = LightThemeComboBoxItem.IsSelected;
        AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
        AppScaleFactor = (int)ScaleSlider.Value;

        // Si on a changé un des paramètres, on les sauvegarde. Sinon, inutile de réécrire le fichier.
        if (previousEnableLightTheme != EnableLightTheme || previousAppLang != AppLang ||
            previousAppScaleFactor != AppScaleFactor)
        {
            // Sauvegarde des paramètres dans le fichier appSettings
            Logger.ConsoleAndLogWriteLine($"Settings changed. Saving application settings at {Path.GetFullPath("./appSettings")}");
            ApplicationFileManager.SaveApplicationSettings();
            Logger.ConsoleAndLogWriteLine("Settings saved successfully");
        }
        else
        {
            Logger.ConsoleAndLogWriteLine("Settings are unchanged. No need to save them.");
        }

        // Mise à jour éventuellement du contenu pour update la langue du menu
        UpdateWindowContents(false, previousAppLang != AppLang, previousEnableLightTheme != EnableLightTheme);

        // Si on a modifié l'échelle dans les paramètres
        if (AppScaleFactor != previousAppScaleFactor)
        {
            // Mise à jour de l'échelle de toutes les fenêtres
            var scaleFactor = AppScaleFactor / 100f;
            if (scaleFactor <= 1f)
            {
                ApplyScaling(scaleFactor - 0.1f);
            }
            else
            {
                ApplyScaling(scaleFactor - 0.2f);
            }
            App.WindowManager!.MainWindow.ApplyScaling(scaleFactor); }
        
        // Mise à jour de la fenêtre principale
        App.WindowManager?.MainWindow.UpdateWindowContents(previousAppLang != AppLang, previousEnableLightTheme != EnableLightTheme, previousAppScaleFactor == AppScaleFactor);

        // Masquage de la fenêtre de paramètres
        Hide();
    }


    // Fonction s'exécutant lors du clic sur le bouton annuler
    /// <summary>
    /// Handles the cancel button click event by restoring previous settings and hiding the settings window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        UpdateWindowContents(false, true, true); // Restauration des paramètres précédents dans la fenêtre de paramétrage
        Hide(); // Masquage de la fenêtre de paramétrage
    }


    // ----- GESTION DE DES CASES A COCHER -----
    /// <summary>
    /// Enables the <see cref="IncludeAddressListCheckBox"/> control and sets its foreground color based on the theme.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void EnableIncludeAddress(object sender, RoutedEventArgs e)
    {
        IncludeAddressListCheckBox.IsEnabled = true;

        IncludeAddressListCheckBox.Foreground = EnableLightTheme ?
            new SolidColorBrush(Colors.Black) : MainWindow.ConvertStringColor("#E3DED4");
    }


    /// <summary>
    /// Disables the <see cref="IncludeAddressListCheckBox"/> control, unchecks it, and sets its foreground color based on the theme.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void DisableIncludeAddress(object sender, RoutedEventArgs e)
    {
        IncludeAddressListCheckBox.IsEnabled = false;
        IncludeAddressListCheckBox.IsChecked = false;

        IncludeAddressListCheckBox.Foreground = EnableLightTheme ?
            new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.DimGray);
    }


    /// <summary>
    /// Handles the <see cref="TabControl.SelectionChanged"/> event to adjust the visibility of buttons based on the selected tab.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a <see cref="TabControl"/>.</param>
    /// <param name="e">The event data.</param>
    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is not TabControl) return;

        var selectedTab = (sender as TabControl)?.SelectedItem as TabItem;
        switch (selectedTab)
        {
            case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletDebug.Header:

                CancelButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Collapsed;

                CreateArchiveDebugButton.Visibility = Visibility.Visible;
                break;
            case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletInformations.Header:
                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Hidden;
                CreateArchiveDebugButton.Visibility = Visibility.Collapsed;
                break;
            case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletParametresApplication.Header:
                SaveButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                CreateArchiveDebugButton.Visibility = Visibility.Collapsed;
                break;
        }
    }


    /// <summary>
    /// Creates a debug report based on the state of various checkboxes and the include address list.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CreateDebugReport(object sender, RoutedEventArgs e)
    {
        var includeOsInfo = AddInfosOsCheckBox.IsChecked;
        var includeHardwareInfo = AddInfosHardCheckBox.IsChecked;
        var includeImportedProjects = AddImportedFilesCheckBox.IsChecked;
        var includeRemovedGroupAddressList = (bool)IncludeAddressListCheckBox.IsChecked! && (bool)AddImportedFilesCheckBox.IsChecked!;

        DebugArchiveGenerator.CreateDebugArchive((bool)includeOsInfo!, (bool)includeHardwareInfo!, (bool)includeImportedProjects!, includeRemovedGroupAddressList!);
    }


    // ----- GESTION DES LIENS HYPERTEXTE -----
    // Fonction gérant le clic sur un lien hypertexte
    /// <summary>
    /// Handles the click event on a hyperlink by attempting to open the URL in the default web browser.
    /// If an error occurs during the process, an error message is logged.
    /// </summary>
    /// <param name="sender">The source of the event, typically the hyperlink control.</param>
    /// <param name="e">Event data containing the URI to navigate to.</param>
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
        catch (InvalidOperationException)
        {
            Logger.ConsoleAndLogWriteLine("Error: cannot redirect to the clicked link.");
        }
        catch (ArgumentException)
        {
            Logger.ConsoleAndLogWriteLine("Error: cannot redirect to the clicked link.");
        }
        catch (PlatformNotSupportedException)
        {
            Logger.ConsoleAndLogWriteLine("Error: cannot redirect to the clicked link.");
        }

        e.Handled = true;
    }


        
    // ----- GESTION DES INPUTS CLAVIER/SOURIS -----
    // Fonction permettant d'effectuer des actions quand une touche spécifique du clavier est appuyée
    /// <summary>
    /// Handles the key down events in the settings window. Depending on the key pressed, 
    /// either restores previous settings and hides the window, or saves new settings and then hides the window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the settings window.</param>
    /// <param name="e">Event data containing information about the key pressed.</param>
    private void SettingsWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            // Si on appuie sur échap, on ferme la fenêtre et on annule les modifications
            case Key.Escape:
                UpdateWindowContents(false, true, true); // Restauration des paramètres précédents dans la fenêtre de paramétrage
                Hide(); // Masquage de la fenêtre de paramétrage
                break;

            // Si on appuie sur entrée, on sauvegarde les modifications et on ferme
            // case Key.Enter:
            //     SaveButtonClick(null!, null!);
            //     break;
        }
    }


    // Fonction gérant le clic sur l'en-tête de la fenêtre de paramètres, de manière que l'on puisse
    // déplacer la fenêtre avec la souris.
    /// <summary>
    /// Initiates a drag operation when the left mouse button is pressed on the header, allowing the window to be moved.
    /// </summary>
    /// <param name="sender">The source of the event, typically the header control.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }


        
    // ----- GESTION DU SCALING -----
    /// <summary>
    /// Applies scaling to the window by adjusting the layout transform and resizing the window based on the specified scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply.</param>
    private void ApplyScaling(double scale)
    {
        SettingsWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 605 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 605 * scale;
        Width = 500 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 500 * scale;
    }


    // ----- GESTION DU CLIC SUR UN SLIDER -----
    /// <summary>
    /// Handles the click event of a slider's RepeatButton, updating the slider's value based on the click position.
    /// </summary>
    private void OnSliderClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Vérifie si l'objet sender est un RepeatButton
            if (sender is not RepeatButton repeatButton) return;
        
            // Trouve le Slider parent du RepeatButton
            var slider = FindParent<Slider>(repeatButton);
        
            // Si le slider est null, quitter la méthode
            if (slider == null) 
            {
                Logger.ConsoleAndLogWrite("Slider not found.");
                return;
            }
        
            // Obtient la position de la souris relative au slider
            var position = Mouse.GetPosition(slider);
        
            // Calcule la nouvelle valeur du slider en fonction de la position de la souris
            var relativeX = position.X / slider.ActualWidth;
            var newValue = slider.Minimum + (relativeX * (slider.Maximum - slider.Minimum));
        
            // Met à jour la valeur du slider
            slider.Value = newValue;
        }
        catch (Exception ex)
        {
            // Logue l'erreur en cas d'exception
            Logger.ConsoleAndLogWrite($"An error occurred: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Utility method to find the parent of a specific type in the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the parent to find.</typeparam>
    /// <param name="child">The starting child object from which to search up the visual tree.</param>
    /// <returns>The parent of type T, or null if no such parent is found.</returns>
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        try
        {
            // Boucle jusqu'à ce qu'un parent de type T soit trouvé ou qu'il n'y ait plus de parents
            while (true)
            {
                // Obtient le parent visuel de l'objet enfant actuel
                var parentObject = VisualTreeHelper.GetParent(child);
            
                // Gère les différents cas possibles pour le parentObject
                switch (parentObject)
                {
                    // Si aucun parent n'est trouvé, retourne null
                    case null:
                        return null;
                    // Si le parent est du type T, retourne ce parent
                    case T parent:
                        return parent;
                    // Sinon, continue la recherche avec le parentObject comme nouvel enfant
                    default:
                        child = parentObject;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            Logger.ConsoleAndLogWrite($"An error occurred while finding parent: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Handles the event when the left mouse button is pressed down on the slider.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse button event.</param>
    private void SliderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Indique que le glissement est en cours
            _isDragging = true;
        
            // Capture le mouse pour suivre le mouvement au-dessus du slider
            Mouse.Capture(ScaleSlider);
        
            // Met à jour la valeur du slider en fonction de la position de la souris
            UpdateSliderValue(sender, e);
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            Logger.ConsoleAndLogWrite($"An error occurred in SliderMouseLeftButtonDown: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Handles the event when the left mouse button is released on the slider.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse button event.</param>
    private void SliderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Indique que le glissement est terminé
            _isDragging = false;
        
            // Relâche la capture de la souris
            Mouse.Capture(null);
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            Logger.ConsoleAndLogWrite($"An error occurred in SliderMouseLeftButtonUp: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Handles the event when the mouse is moved over the slider while dragging.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse movement event.</param>
    private void SliderMouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            // Met à jour la valeur du slider uniquement si le glissement est en cours
            if (_isDragging)
            {
                UpdateSliderValue(sender, e);
            }
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            Logger.ConsoleAndLogWrite($"An error occurred in SliderMouseMove: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Updates the slider's value based on the current mouse position.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse movement event.</param>
    private void UpdateSliderValue(object sender, MouseEventArgs e)
    {
        try
        {
            // Vérifie si l'objet sender est un Slider
            if (sender is not Slider slider) return;
        
            // Obtient la position de la souris relative au slider
            var position = e.GetPosition(slider);
        
            // Calcule la nouvelle valeur du slider en fonction de la position de la souris
            var relativeX = position.X / slider.ActualWidth;
            var newValue = slider.Minimum + (relativeX * (slider.Maximum - slider.Minimum));
        
            // Ajuste la valeur pour correspondre au tick le plus proche
            var tickFrequency = slider.TickFrequency;
            newValue = Math.Round(newValue / tickFrequency) * tickFrequency;
        
            // Met à jour la valeur du slider en s'assurant qu'elle reste dans les limites
            slider.Value = Math.Max(slider.Minimum, Math.Min(slider.Maximum, newValue));
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            Logger.ConsoleAndLogWrite($"An error occurred in UpdateSliderValue: {ex.Message}");
        }
    }


}
