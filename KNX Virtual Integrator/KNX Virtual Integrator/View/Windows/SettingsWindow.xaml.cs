using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using KNX_Virtual_Integrator.ViewModel;
// ReSharper disable StringLiteralTypo

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable ConvertToUsingDeclaration


namespace KNX_Virtual_Integrator.View.Windows;

/// <summary>
///  Window used to set the application settings and create reports.
/// </summary>
public partial class SettingsWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    //Permet à la fenêtre à accéder aux services du ViewModel
    /// <summary>
    /// MainViewModel instance to allow communication with the backend
    /// </summary>
    private readonly MainViewModel _viewModel;
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- MÉTHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Constructeur. Charge les paramètres contenus dans le fichier appSettings et les affiche également
    // dans la fenêtre de paramétrage de l'application. Si la valeur est incorrecte ou vide, une valeur par défaut
    // est affectée.
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsWindow"/> class,
    /// loading and applying settings from the appSettings file, and setting default values where necessary.
    /// </summary>
    public SettingsWindow(MainViewModel viewModel)
    {
        InitializeComponent(); // Initialisation de la fenêtre de paramétrage

        _viewModel = viewModel;
        DataContext = _viewModel;
        
        UpdateWindowContents(true, true); // Affichage des paramètres dans la fenêtre
        
        ScaleSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(_viewModel.SliderMouseLeftButtonDown), true);
        ScaleSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(_viewModel.SliderMouseLeftButtonUp), true);
        ScaleSlider.AddHandler(MouseMoveEvent, new MouseEventHandler(_viewModel.SliderMouseMove), true);
    }
    
    // Fonction s'exécutant à la fermeture de la fenêtre de paramètres
    /// <summary>
    /// Handles the settings window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true; // Pour éviter de tuer l'instance de SettingsWindow, on annule la fermeture
        UpdateWindowContents(); // Mise à jour du contenu de la fenêtre pour remettre les valeurs précédentes
        Hide(); // On masque la fenêtre à la place
    }
    
    // Fonction permettant de mettre à jour les champs dans la fenêtre de paramétrage
    /// <summary>
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the settings window accordingly to the application settings.
    /// </summary>
    private void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (_viewModel.AppSettings.AppLang != AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0])
        {
            FrAppLanguageComboBoxItem.IsSelected = _viewModel.AppSettings.AppLang == "FR"; // Sélection/Désélection

            // Sélection du langage de l'application (même fonctionnement que le code ci-dessus)
            foreach (ComboBoxItem item in AppLanguageComboBox.Items)
            {
                if (!item.Content.ToString()!.StartsWith(_viewModel.AppSettings.AppLang)) continue;
                item.IsSelected = true;
                break;
            }
        }
        
        // Sélection du thème clair ou sombre
        LightThemeComboBoxItem.IsSelected = _viewModel.AppSettings.EnableLightTheme;
        DarkThemeComboBoxItem.IsSelected = !_viewModel.AppSettings.EnableLightTheme;

        // Mise à jour du slider
        ScaleSlider.Value = _viewModel.AppSettings.AppScaleFactor;
            
        // Traduction du menu settings
        if (langChanged) TranslateWindowContents();
            
        // Application du thème
        if (themeChanged) ApplyThemeToWindow();
        
        // Application des modifications d'échelle
        if (scaleChanged)
        {
            // Mise à jour de l'échelle de toutes les fenêtres
            var scaleFactor = _viewModel.AppSettings.AppScaleFactor / 100f;
            if (scaleFactor <= 1f)
            {
                ApplyScaling(scaleFactor - 0.1f);
            }
            else
            {
                ApplyScaling(scaleFactor - 0.2f);
            }
        }

    }
    
    // Fonction traduisant tous les textes de la fenêtre paramètres
    /// <summary>
    /// This function translates all the texts contained in the setting window to the application language
    /// </summary>
    private void TranslateWindowContents()
    {
        switch (_viewModel.AppSettings.AppLang)
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
                CreateArchiveDebugText.Text = "إنشاء ملف التصحيح";
                OngletDebug.Header = "تصحيح";
                OngletInformations.Header = "معلومات";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nالإصدار {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nالبناء {App.AppBuild}" +
                                        $"\n" +
                                        $"\nبرنامج تم إنشاؤه كجزء من تدريب هندسي بواسطة طلاب INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA و Raphaël MARQUES" +
                                        $"\n" +
                                        $"\nبإشراف:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nشراكة بين المعهد الوطني للعلوم التطبيقية (INSA) في تولوز واتحاد Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nإنشاء: 2024 - 2025\n";
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
                CreateArchiveDebugText.Text = "Създаване на файл за отстраняване на грешки";
                OngletDebug.Header = "Отстраняване на грешки";
                OngletInformations.Header = "Информация";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nВерсия {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nИзграждане {App.AppBuild}" +
                                        $"\n" +
                                        $"\nСофтуер, създаден като част от инженерно стаж от студенти на INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA и Raphaël MARQUES" +
                                        $"\n" +
                                        $"\nПод наблюдението на:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nПартньорство между Националния институт по приложни науки (INSA) в Тулуза и Съюза Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nСъздаване: 2024 - 2025\n";
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
                CreateArchiveDebugText.Text = "Vytvořit soubor pro ladění";
                OngletDebug.Header = "Ladění";
                OngletInformations.Header = "Informace";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nVerze {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nBuild {App.AppBuild}" +
                                        $"\n" +
                                        $"\nSoftware vytvořený jako součást inženýrského stáže studenty INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA a Raphaël MARQUES" +
                                        $"\n" +
                                        $"\nPod dohledem:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nPartnerství mezi Národním institutem aplikovaných věd (INSA) v Toulouse a Union Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nVytvořeno: 2024 - 2025\n";
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
                CreateArchiveDebugText.Text = "Opret fejlfindingsfil";
                OngletDebug.Header = "Fejlfindings";
                OngletInformations.Header = "Information";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nBuild {App.AppBuild}" +
                                        $"\n" +
                                        $"\nSoftware skabt som en del af en ingeniørpraktik af studerende fra INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA og Raphaël MARQUES" +
                                        $"\n" +
                                        $"\nUnder vejledning af:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nPartnerskab mellem National Institute of Applied Sciences (INSA) i Toulouse og Union Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nOprettelse: 2024 - 2025\n";
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

                CreateArchiveDebugText.Text = "Debug-Datei erstellen";

                OngletDebug.Header = "Debuggen";
                    
                OngletInformations.Header = "Informationen";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware im Rahmen eines Ingenieurpraktikums von Studenten der INSA Toulouse entwickelt:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA und Raphaël MARQUES" +
                    $"\n" +
                    $"\nUnter der Aufsicht von:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerschaft zwischen dem Institut National des Sciences Appliquées (INSA) de Toulouse und der Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nUmsetzung: 2024 - 2025\n";
                        
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
                CreateArchiveDebugText.Text = "Δημιουργία αρχείου εντοπισμού σφαλμάτων";
                OngletDebug.Header = "Εντοπισμός σφαλμάτων";
                OngletInformations.Header = "Πληροφορίες";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nΈκδοση {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nΚατασκευή {App.AppBuild}" +
                                        $"\n" +
                                        $"\nΛογισμικό που δημιουργήθηκε ως μέρος της μηχανικής πρακτικής από φοιτητές της INSA Toulouse:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA και Raphaël MARQUES" +
                                        $"\n" +
                                        $"\nΥπό την επίβλεψη:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nΣυνεργασία μεταξύ του Εθνικού Ινστιτούτου Εφαρμοσμένων Επιστημών (INSA) της Τουλούζης και της Ένωσης Cépière Robert Monnier (UCRM)." +
                                        $"\n" +
                                        $"\nΔημιουργία: 2024 - 2025\n";
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

                CreateArchiveDebugText.Text = "Create debug file";

                OngletDebug.Header = "Debugging";
                    
                OngletInformations.Header = "Information";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware developed as part of an engineering internship by students of INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA and Raphaël MARQUES" +
                    $"\n" +
                    $"\nUnder the supervision of:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnership between the Institut National des Sciences Appliquées (INSA) de Toulouse and the Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nImplementation: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Crear archivo de depuración";

                OngletDebug.Header = "Depuración";
                    
                OngletInformations.Header = "Información";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersión {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nCompilación {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware desarrollado como parte de una pasantía de ingeniería por estudiantes de INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA y Raphaël MARQUES" +
                    $"\n" +
                    $"\nBajo la supervisión de:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nAsociación entre el Instituto Nacional de Ciencias Aplicadas (INSA) de Toulouse y la Unión Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nImplementación: 2024 - 2025\n";
                        
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
                CreateArchiveDebugText.Text = "Loo silumisfail";
                OngletDebug.Header = "Silumine";
                OngletInformations.Header = "Teave";
                InformationsText.Text = $"{App.AppName}" +
                                        $"\nVersioon {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                        $"\nKoostamine {App.AppBuild}" +
                                        $"\n" +
                                        $"\nTarkvara, mille lõid osana inseneripraktikast INSA Toulouse üliõpilased:" +
                                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA ja Raphaël MARQUES" +
                                        $"\n" +
                                        $"\nJärelevalve all:" +
                                        $"\nDidier BESSE (UCRM)" +
                                        $"\nThierry COPPOLA (UCRM)" +
                                        $"\nJean-François KLOTZ (LECS)" +
                                        $"\n" +
                                        $"\nPartnerlus Toulouse'i Rakendusteaduste Riikliku Instituudi (INSA) ja Union Cépière Robert Monnier (UCRM) vahel." +
                                        $"\n" +
                                        $"\nLoomine: 2024 - 2025\n";
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

                CreateArchiveDebugText.Text = "Luo virheenkorjaustiedosto";
                    
                OngletDebug.Header = "Virheenkorjaus";
                    
                OngletInformations.Header = "Tiedot";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersio {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nKokoelma {App.AppBuild}" +
                    $"\n" +
                    $"\nOhjelmisto kehitetty osana INSAn Toulousen insinööriharjoittelua:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA ja Raphaël MARQUES" +
                    $"\n" +
                    $"\nValvonnassa:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nYhteistyö Instituutti National des Sciences Appliquées (INSA) de Toulousen ja Union Cépière Robert Monnier (UCRM) välillä." +
                    $"\n" +
                    $"\nToteutus: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Hibakeresési fájl létrehozása";

                OngletDebug.Header = "Hibakeresés";
                    
                OngletInformations.Header = "Információk";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVerzió {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSzoftver az INSA Toulouse mérnöki szakmai gyakorlat keretében fejlesztett:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA és Raphaël MARQUES" +
                    $"\n" +
                    $"\nFelügyelete alatt:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerség az Institut National des Sciences Appliquées (INSA) de Toulouse és az Union Cépière Robert Monnier (UCRM) között." +
                    $"\n" +
                    $"\nMegvalósítás: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Buat file debug";

                OngletDebug.Header = "Debug";
                    
                OngletInformations.Header = "Informasi";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersi {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nPerangkat lunak dikembangkan sebagai bagian dari magang teknik oleh siswa INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA dan Raphaël MARQUES" +
                    $"\n" +
                    $"\nDi bawah pengawasan:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nKemitraan antara Institut National des Sciences Appliquées (INSA) de Toulouse dan Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nImplementasi: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Crea file di debug";

                OngletDebug.Header = "Debug";
                    
                OngletInformations.Header = "Informazioni";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersione {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware sviluppato nell'ambito di uno stage di ingegneria da studenti dell'INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA e Raphaël MARQUES" +
                    $"\n" +
                    $"\nSotto la supervisione di:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartenariato tra l'Istituto Nazionale delle Scienze Applicate (INSA) di Tolosa e l'Unione Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizzazione: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "デバッグファイルを作成";

                OngletDebug.Header = "デバッグ";
                    
                OngletInformations.Header = "情報";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nバージョン {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nビルド {App.AppBuild}" +
                    $"\n" +
                    $"\nINSAトゥールーズの学生によるエンジニアリングインターンシップの一環として開発されたソフトウェア:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA, Raphaël MARQUES" +
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

                CreateArchiveDebugText.Text = "디버그 파일 생성";

                OngletDebug.Header = "디버그";
                    
                OngletInformations.Header = "정보";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\n버전 {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\n빌드 {App.AppBuild}" +
                    $"\n" +
                    $"\nINSA 툴루즈 학생들이 엔지니어링 인턴십의 일환으로 개발한 소프트웨어:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA, Raphaël MARQUES" +
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

                CreateArchiveDebugText.Text = "Izveidot problēmu novēršanas failu";

                OngletDebug.Header = "Problēmu novēršana";
                                    
                OngletInformations.Header = "Informācija";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersija {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBūvēt {App.AppBuild}" +
                    $"\n" +
                    $"\nProgrammatūra izstrādāta INSA Toulouse inženierijas prakses ietvaros:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA un Raphaël MARQUES" +
                    $"\n" +
                    $"\nPārraudzīja:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerība starp National Institute of Applied Sciences (INSA) Toulouse un Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nIzstrāde: 2024 - 2025\n";
                                        
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

                CreateArchiveDebugText.Text = "Sukurti derinimo failą";

                OngletDebug.Header = "Derinimas";
                            
                OngletInformations.Header = "Informacija";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersija {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nKūrimas {App.AppBuild}" +
                    $"\n" +
                    $"\nPrograminė įranga sukurta INSA Toulouse inžinerijos praktikos metu:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA ir Raphaël MARQUES" +
                    $"\n" +
                    $"\nPrižiūrėtojai:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerystė tarp Nacionalinio taikomųjų mokslų instituto (INSA) Tulūzoje ir Sąjungos Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizacija: 2024 - 2025\n";
                                
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

                CreateArchiveDebugText.Text = "Opprett feilsøkingsfil";

                OngletDebug.Header = "Feilsøking";
                            
                OngletInformations.Header = "Informasjon";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersjon {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBygg {App.AppBuild}" +
                    $"\n" +
                    $"\nProgramvare laget som en del av et ingeniørpraksis ved INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA og Raphaël MARQUES" +
                    $"\n" +
                    $"\nUnder veiledning av:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerskap mellom National Institute of Applied Sciences (INSA) i Toulouse og Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nUtførelse: 2024 - 2025\n";
                                
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

                CreateArchiveDebugText.Text = "Maak debugbestand aan";

                OngletDebug.Header = "Debug";
                    
                OngletInformations.Header = "Informatie";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersie {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware gemaakt in het kader van een ingenieursstage door studenten van INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA en Raphaël MARQUES" +
                    $"\n" +
                    $"\nOnder supervisie van:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerschap tussen het Institut National des Sciences Appliquées (INSA) van Toulouse en de Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealisatie: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Utwórz plik debugowania";

                OngletDebug.Header = "Debugowanie";
                    
                OngletInformations.Header = "Informacje";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nWersja {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nOprogramowanie stworzone w ramach praktyk inżynierskich przez studentów INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA i Raphaël MARQUES" +
                    $"\n" +
                    $"\nPod nadzorem:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerstwo między Institut National des Sciences Appliquées (INSA) w Tuluzie a Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizacja: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Criar arquivo de depuração";

                OngletDebug.Header = "Depuração";
                    
                OngletInformations.Header = "Informações";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersão {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware realizado no âmbito de um estágio de engenharia por estudantes da INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA e Raphaël MARQUES" +
                    $"\n" +
                    $"\nSob a supervisão de:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nParceria entre o Institut National des Sciences Appliquées (INSA) de Toulouse e a Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealização: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Creați fișierul de depanare";

                OngletDebug.Header = "Depanare";
                            
                OngletInformations.Header = "Informații";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersiune {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftware creat în cadrul unui stagiu de inginerie la INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA și Raphaël MARQUES" +
                    $"\n" +
                    $"\nSub supravegherea lui:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nParteneriat între Institutul Național de Științe Aplicate (INSA) din Toulouse și Uniunea Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizare: 2024 - 2025\n";
                                
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

                CreateArchiveDebugText.Text = "Vytvoriť ladiaci súbor";

                OngletDebug.Header = "Ladenie";
                            
                OngletInformations.Header = "Informácie";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVerzia {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nSoftvér vytvorený v rámci inžinierskej stáže na INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA a Raphaël MARQUES" +
                    $"\n" +
                    $"\nPod dohľadom:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerstvo medzi Národným inštitútom aplikovaných vied (INSA) v Toulouse a Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRealizácia: 2024 - 2025\n";
                                
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

                CreateArchiveDebugText.Text = "Ustvari datoteko za odpravljanje napak";

                OngletDebug.Header = "Odpravljanje napak";
                            
                OngletInformations.Header = "Informacije";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nRazličica {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nIzgradnja {App.AppBuild}" +
                    $"\n" +
                    $"\nProgramska oprema, izdelana v okviru inženirskega pripravništva na INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA in Raphaël MARQUES" +
                    $"\n" +
                    $"\nPod nadzorom:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartnerstvo med Nacionalnim inštitutom za uporabne znanosti (INSA) v Toulouseu in Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nIzvedba: 2024 - 2025\n";
                                
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

                CreateArchiveDebugText.Text = "Skapa felsökningsfil";

                OngletDebug.Header = "Felsökning";
                    
                OngletInformations.Header = "Information";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nProgramvara utvecklad inom ramen för en ingenjörspraktik av studenter från INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA och Raphaël MARQUES" +
                    $"\n" +
                    $"\nUnder överinseende av:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nSamarbete mellan Institut National des Sciences Appliquées (INSA) i Toulouse och Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nGenomförande: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Hata ayıklama dosyası oluştur";

                OngletDebug.Header = "Hata ayıklama";
                    
                OngletInformations.Header = "Bilgiler";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nSürüm {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nYapı {App.AppBuild}" +
                    $"\n" +
                    $"\nINSA Toulouse öğrencileri tarafından bir mühendislik stajı kapsamında yapılan yazılım:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA ve Raphaël MARQUES" +
                    $"\n" +
                    $"\nGözetim altında:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nInstitut National des Sciences Appliquées (INSA) Toulouse ve Union Cépière Robert Monnier (UCRM) arasındaki ortaklık." +
                    $"\n" +
                    $"\nGerçekleştirme: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Створити файл налагодження";

                OngletDebug.Header = "Налагодження";
                    
                OngletInformations.Header = "Інформація";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nВерсія {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nЗбірка {App.AppBuild}" +
                    $"\n" +
                    $"\nПрограмне забезпечення розроблене в рамках інженерного стажування студентами INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA та Raphaël MARQUES" +
                    $"\n" +
                    $"\nПід наглядом:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nПартнерство між Institut National des Sciences Appliquées (INSA) в Тулузі та Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nРеалізація: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Создать файл отладки";

                OngletDebug.Header = "Отладка";
                    
                OngletInformations.Header = "Информация";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nВерсия {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nСборка {App.AppBuild}" +
                    $"\n" +
                    $"\nПрограммное обеспечение, разработанное в рамках инженерной стажировки студентами INSA Toulouse:" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA и Raphaël MARQUES" +
                    $"\n" +
                    $"\nПод руководством:" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nПартнерство между Institut National des Sciences Appliquées (INSA) в Тулузе и Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nРеализация: 2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "创建调试文件";

                OngletDebug.Header = "调试";
                    
                OngletInformations.Header = "信息";
                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\n版本 {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\n构建 {App.AppBuild}" +
                    $"\n" +
                    $"\n由INSA Toulouse的学生在工程实习中开发的软件：" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA 和 Raphaël MARQUES" +
                    $"\n" +
                    $"\n在以下人员的指导下：" +
                    $"\nDidier BESSE（UCRM）" +
                    $"\nThierry COPPOLA（UCRM）" +
                    $"\nJean-François KLOTZ（LECS）" +
                    $"\n" +
                    $"\nToulouse的Institut National des Sciences Appliquées（INSA）与Union Cépière Robert Monnier（UCRM）之间的合作。" +
                    $"\n" +
                    $"\n实施：2024 - 2025\n";
                        
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

                CreateArchiveDebugText.Text = "Créer le fichier de débogage";
                    
                OngletDebug.Header = "Débogage";
                OngletInformations.Header = "Informations";

                InformationsText.Text =
                    $"{App.AppName}" +
                    $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                    $"\nBuild {App.AppBuild}" +
                    $"\n" +
                    $"\nLogiciel réalisé dans le cadre d'un stage d'ingénierie par des étudiants de l'INSA Toulouse :" +
                    $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES, Fatine AZZABI, Noé CAILLET, Manuel IBARLUCIA et Raphaël MARQUES" +
                    $"\n" +
                    $"\nSous la supervision de :" +
                    $"\nDidier BESSE (UCRM)" +
                    $"\nThierry COPPOLA (UCRM)" +
                    $"\nJean-François KLOTZ (LECS)" +
                    $"\n" +
                    $"\nPartenariat entre l'Institut National des Sciences Appliquées (INSA) de Toulouse et l'Union Cépière Robert Monnier (UCRM)." +
                    $"\n" +
                    $"\nRéalisation: 2024 - 2025\n";

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
        Style checkboxStyle;
        Brush textColorBrush;
        Brush backgroundColorBrush;
        Brush deepBackgroundColorBrush;
        Brush pathColorBrush;
        Brush borderBrush;

        if (_viewModel.AppSettings.EnableLightTheme) // Si le thème clair est actif,
        {
            textColorBrush = (Brush)FindResource("LightForegroundBrush");
            backgroundColorBrush = (Brush)FindResource("OffWhiteBackgroundBrush");
            deepBackgroundColorBrush = (Brush)FindResource("WhiteBackgroundBrush");
            pathColorBrush = (Brush)FindResource("LightGrayBorderBrush");
            borderBrush = (Brush)FindResource("LightGrayBorderBrush");
            checkboxStyle = (Style)FindResource("StandardCheckBoxLight");
            ThemeComboBox.Style = (Style)FindResource("LightComboBoxStyle");
            AppLanguageComboBox.Style = (Style)FindResource("LightComboBoxStyle");
            ScaleSlider.Style = (Style)FindResource("LightSlider");
            SaveButton.Style = (Style)FindResource("BottomButtonLight");
            CancelButton.Style = (Style)FindResource("BottomButtonLight");
            CreateArchiveDebugButton.Style = (Style)FindResource("BottomButtonLight");
            OngletDebug.Style = (Style)FindResource("LightOnglet");
            OngletInformations.Style = (Style)FindResource("LightOnglet");
            OngletParametresApplication.Style = (Style)FindResource("LightOnglet");
            SaveSettImage.Source = (DrawingImage)FindResource("CheckmarkLight");
            CancelButtonImage.Source = (DrawingImage)FindResource("CrossmarkLight");

            Resources["CurrentComboBoxItemContainerStyle"] = (Style)FindResource("LightComboBoxItemStyle");
        }
        else // Sinon, on met le thème sombre
        {
            textColorBrush = (Brush)FindResource("DarkOffWhiteForegroundBrush");
            backgroundColorBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            deepBackgroundColorBrush = (Brush)FindResource("DarkerGrayBackgroundBrush");
            pathColorBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            checkboxStyle = (Style)FindResource("StandardCheckBoxDark");
            borderBrush = (Brush)FindResource("GrayBorderBrush");
            ThemeComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
            AppLanguageComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
            ScaleSlider.Style = (Style)FindResource("DarkSlider");
            SaveButton.Style = (Style)FindResource("BottomButtonDark");
            CancelButton.Style = (Style)FindResource("BottomButtonDark");
            CreateArchiveDebugButton.Style = (Style)FindResource("BottomButtonDark");
            OngletDebug.Style = (Style)FindResource("DarkOnglet");
            OngletInformations.Style = (Style)FindResource("DarkOnglet");
            OngletParametresApplication.Style = (Style)FindResource("DarkOnglet");
            SaveSettImage.Source = (DrawingImage)FindResource("CheckmarkDark");
            CancelButtonImage.Source = (DrawingImage)FindResource("CrossmarkDark");
            
            Resources["CurrentComboBoxItemContainerStyle"] = (Style)FindResource("DarkComboBoxItemStyle");
        }
        // Arrière plan de la fenêtre
        Background = backgroundColorBrush;

        // En-tête de la fenêtre
        SettingsIconPath1.Brush = textColorBrush;
        SettingsIconPath2.Brush = textColorBrush;
        SettingsWindowTopTitle.Foreground = textColorBrush;
        HeaderPath.Stroke = pathColorBrush;

        // Corps de la fenêtre
        MainContentBorder.BorderBrush = pathColorBrush;
        GeneralSettingsTab.Background = deepBackgroundColorBrush;
        AppSettingsTitle.Foreground = textColorBrush;
        ThemeTextBox.Foreground = textColorBrush;
        AppLanguageTextBlock.Foreground = textColorBrush;
            

        // Pied de page avec les boutons save et cancel
        SettingsWindowFooter.Background = deepBackgroundColorBrush;
        FooterPath.Stroke = pathColorBrush;
        CancelButtonText.Foreground = textColorBrush;
        SaveButtonText.Foreground = textColorBrush;
        CreateArchiveDebugText.Foreground = textColorBrush;

        // Menu debug
        ControlOnglet.BorderBrush = borderBrush;
        DebugPanel.Background = deepBackgroundColorBrush;
        InformationsGrid.Background = deepBackgroundColorBrush;
        AddInfosOsCheckBox.Style = checkboxStyle;
        AddInfosHardCheckBox.Style = checkboxStyle;
        AddImportedFilesCheckBox.Style = checkboxStyle;
        AddInfosOsCheckBox.Foreground = textColorBrush;
        AddInfosHardCheckBox.Foreground = textColorBrush;
        AddImportedFilesCheckBox.Foreground = textColorBrush;
        OngletParametresApplication.BorderBrush = borderBrush;
        OngletDebug.Foreground = textColorBrush;
        OngletParametresApplication.Foreground = textColorBrush;
        DebugBrush1.Brush = textColorBrush;
        DebugBrush2.Brush = textColorBrush;
        
        // Menu Informations
        OngletInformations.Foreground = textColorBrush;
        InformationsText.Foreground = textColorBrush;
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
        var previousEnableLightTheme = _viewModel.AppSettings.EnableLightTheme;
        var previousAppLang = _viewModel.AppSettings.AppLang;
        var previousAppScaleFactor = _viewModel.AppSettings.AppScaleFactor;

        // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
        _viewModel.AppSettings.EnableLightTheme = LightThemeComboBoxItem.IsSelected;
        _viewModel.AppSettings.AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
        _viewModel.AppSettings.AppScaleFactor = (int)ScaleSlider.Value;

        // Si on a changé un des paramètres, on les sauvegarde. Sinon, inutile de réécrire le fichier.
        if (previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme || 
            previousAppLang != _viewModel.AppSettings.AppLang ||
            previousAppScaleFactor != _viewModel.AppSettings.AppScaleFactor)
        {
            // Sauvegarde des paramètres dans le fichier appSettings
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"Settings changed. Saving application settings at {Path.GetFullPath("./appSettings")}");
            _viewModel.SaveSettingsCommand.Execute(null);
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("Settings saved successfully");
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("Settings are unchanged. No need to save them.");
        }

        // Mise à jour éventuellement du contenu pour update le menu
        UpdateWindowContents(previousAppLang != _viewModel.AppSettings.AppLang, previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme, _viewModel.AppSettings.AppScaleFactor != previousAppScaleFactor);

        // Mise à jour des autres fenêtres
        App.WindowManager?.MainWindow.UpdateWindowContents(
            previousAppLang != _viewModel.AppSettings.AppLang, 
            previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme, 
            previousAppScaleFactor != _viewModel.AppSettings.AppScaleFactor);
        App.WindowManager?.ConnectionWindow.UpdateWindowContents(
            previousAppLang != _viewModel.AppSettings.AppLang, 
            previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme, 
            previousAppScaleFactor != _viewModel.AppSettings.AppScaleFactor);
        App.WindowManager?.TestConfigWindow.UpdateWindowContents(
            previousAppLang != _viewModel.AppSettings.AppLang, 
            previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme, 
            previousAppScaleFactor != _viewModel.AppSettings.AppScaleFactor);
        App.WindowManager?.ReportCreationWindow.UpdateWindowContents(
            previousAppLang != _viewModel.AppSettings.AppLang, 
            previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme, 
            previousAppScaleFactor != _viewModel.AppSettings.AppScaleFactor);
        App.WindowManager?.StructureEditWindow.UpdateWindowContents(
            previousAppLang != _viewModel.AppSettings.AppLang, 
            previousEnableLightTheme != _viewModel.AppSettings.EnableLightTheme, 
            previousAppScaleFactor != _viewModel.AppSettings.AppScaleFactor);

        Hide(); // fermeture de la fenêtre
    }
    
    // Fonction s'exécutant lors du clic sur le bouton annuler
    /// <summary>
    /// Handles the cancel button click event by restoring previous settings and hiding the settings window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        UpdateWindowContents(); // Restauration des paramètres précédents dans la fenêtre de paramétrage
        Hide(); // Masquage de la fenêtre de paramétrage
    }
    
    // Gère l'évènement pour rendre visible les objets du nouvel onglet et cache les anciens
    /// <summary>
    /// Handles the <see cref="TabControl_SelectionChanged"/> event to adjust the visibility of buttons based on the selected tab.
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

        _viewModel.CreateDebugArchiveCommand.Execute(((bool)includeOsInfo!, (bool)includeHardwareInfo!, (bool)includeImportedProjects!));
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
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("Error: cannot redirect to the clicked link.");
        }
        catch (ArgumentException)
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("Error: cannot redirect to the clicked link.");
        }
        catch (PlatformNotSupportedException)
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("Error: cannot redirect to the clicked link.");
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
            case Key.Escape :
                UpdateWindowContents(true, true); // Restauration des paramètres précédents dans la fenêtre de paramétrage
                Hide(); // Masquage de la fenêtre de paramétrage
                break;

            // Si on appuie sur entrée, on sauvegarde les modifications et on ferme
            // case Key.Enter :
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
        if (e.LeftButton != MouseButtonState.Pressed) return;
        DragMove();
    }
    
    // ----- GESTION DU SCALING -----
    /// <summary>
    /// Applies scaling to the window by adjusting the layout transform and resizing the window based on the specified scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply.</param>
    private void ApplyScaling(float scale)
    {
        
        SettingsWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 605 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 605 * scale;
        Width = 500 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 500 * scale;
    }
    
    // ----- GESTION DU CLIC SUR UN SLIDER -----
    /// <summary>
    /// Handles the click event on the slider by delegating it to <see cref="ViewModel.MainViewModel.OnSliderClick"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSliderClick(object sender, RoutedEventArgs e)
    {
        _viewModel.OnSliderClick(sender, e);
    }
}
