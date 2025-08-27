using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.ViewModel;
using KNX_Virtual_Integrator.ViewModel.Commands;
using Microsoft.Win32;
namespace KNX_Virtual_Integrator.View.Windows;

/// <summary>
/// Class for the Main Window. It implements interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Instance of <see cref="MainViewModel"/> to communicate with the backend
    /// </summary>
    private readonly MainViewModel _viewModel;
    /// <summary>
    /// Instance of <see cref="View.WindowManager"/> to access windows
    /// </summary>
    private readonly WindowManager _windowManager;
    
    /// <summary>
    /// True if the user choose to import a group addresses file, false if it's a project knx file 
    /// </summary>
    public bool UserChooseToImportGroupAddressFile { get; private set; }
        
    /// <summary>
    /// The token source used to signal cancellation requests for ongoing tasks.
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;
        
    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- MÉTHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Initializes the window with the specified MainViewModel and WindowManager.
    /// To ensure correct loading of content, the scaling is only applied once the window is mostly loaded.
    /// </summary>
    /// <param name="viewModel">The <see cref="ViewModel.MainViewModel"/> given to the windows</param>
    /// <param name="wm">The <see cref="View.WindowManager"/> given to this window</param>
    public MainWindow(MainViewModel viewModel, WindowManager wm)
    {
        InitializeComponent();
            
        _viewModel = viewModel;
        DataContext = _viewModel;
        _windowManager = wm;
        _cancellationTokenSource = new CancellationTokenSource();
        
        AllColumnBorder.Loaded += (_, _) =>
        {
            ApplyScaling();
        };
    }

    /// <summary>
    /// Update the size of the window and its contents according to <see cref="Model.Interfaces.IApplicationSettings.AppScaleFactor"/>
    /// </summary>
    private void ApplyScaling()
    {
        // Only scale when the border is loaded to  ensure it has the correct size before scaling
        if (AllColumnBorder.IsLoaded is false) return;
        // The scaleFactor is used to transform the window but the reason for -0.1f or -0.2f is not known
        var scaleFactor = _viewModel.AppSettings.AppScaleFactor / 100f;
        float scale;
        if (scaleFactor < 1f)
        {
            scale = scaleFactor - 0.1f;
        }
        else
        {
            scale = scaleFactor - 0.2f;
        }
        // This is done to automatically change the Width according to the others and the content needed for the grid.Column
        Col0.Width = new GridLength(3, GridUnitType.Star);
        Col1.Width = new GridLength(4, GridUnitType.Star);
        Col2.Width = new GridLength(4, GridUnitType.Star);
        Col3.Width = new GridLength(3, GridUnitType.Star);
        
        // Scales the whole window but does not allow it to become bigger than the screen
        MainWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
        if (1500 * scale >= 0.9 * SystemParameters.PrimaryScreenWidth)
        {
            // Limits the width of the window but not the content so we can use the scrollViewer to scroll its scaled content
            Width = 0.9 * SystemParameters.PrimaryScreenWidth;
            // Forces the width, so that the column width wont automatically change and ruin the layout
            Col0.Width = new GridLength(3f/14f * AllColumnBorder.ActualWidth, GridUnitType.Pixel);
            Col1.Width = new GridLength(4f/14f * AllColumnBorder.ActualWidth, GridUnitType.Pixel);
            Col2.Width = new GridLength(4f/14f * AllColumnBorder.ActualWidth, GridUnitType.Pixel);
            Col3.Width = new GridLength(1, GridUnitType.Star);
            //Col3.Width = new GridLength(3f/14f * AllColumnBorder.ActualWidth, GridUnitType.Pixel);
        }
        else
        {
            Width = 1500 * scale;
        }
        Height = 786 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 786 * scale;
        
    }

    /// <summary>
    /// Updates the color theme of the window according to <see cref="Model.Interfaces.IApplicationSettings.EnableLightTheme"/> state.
    /// </summary>
    private void ApplyThemeToWindow()
    {
        Style titleStyles;
        Style borderStyles;
        Style borderTitleStyles;
        Style searchbuttonStyle;
        Style boxItemStyle;
        Style supprButtonStyle;
        Brush backgrounds;
        Brush foregrounds;
        if (_viewModel.AppSettings.EnableLightTheme)
        {
            NomTextBox.Style = (Style)FindResource("StandardTextBoxLight");
            GroupAddressTreeView.ItemContainerStyle = (Style)FindResource("TreeViewItemStyleLight");
            SelectedElementsListBox.ItemTemplate = (DataTemplate)FindResource("ElementListBoxTemplateLight");
            SelectedElementsListBox.ItemContainerStyle = (Style)FindResource("TestedElementItemContainerStyleLight");
            EditStructButton.Style = (Style)FindResource("EditStructureButtonStyleLight");

            titleStyles = (Style)FindResource("TitleTextLight");
            borderStyles = (Style)FindResource("BorderLight");
            borderTitleStyles = (Style)FindResource("BorderTitleLight");
            searchbuttonStyle = (Style)FindResource("SearchButtonLight");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleLight");
            supprButtonStyle = (Style)FindResource("DeleteStructureButtonStyleLight");
            backgrounds = (Brush)FindResource("OffWhiteBackgroundBrush");
            foregrounds = (Brush)FindResource("LightForegroundBrush");
        }
        else
        {
            NomTextBox.Style = (Style)FindResource("StandardTextBoxDark");
            GroupAddressTreeView.ItemContainerStyle = (Style)FindResource("TreeViewItemStyleDark");
            SelectedElementsListBox.ItemTemplate = (DataTemplate)FindResource("ElementListBoxTemplateDark");
            SelectedElementsListBox.ItemContainerStyle = (Style)FindResource("TestedElementItemContainerStyleDark");
            EditStructButton.Style = (Style)FindResource("EditStructureButtonStyleDark");
            
            titleStyles = (Style)FindResource("TitleTextDark");
            borderStyles = (Style)FindResource("BorderDark");
            borderTitleStyles = (Style)FindResource("BorderTitleDark");
            searchbuttonStyle = (Style)FindResource("SearchButtonDark");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleDark");
            supprButtonStyle = (Style)FindResource("DeleteStructureButtonStyleDark");

            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");
            foregrounds = (Brush)FindResource("DarkForegroundBrush");
        }
        
        Background = backgrounds;
        StructuresBox.Background = backgrounds;
        ModelsBox.Background = backgrounds;
        SelectedElementsListBox.Background = backgrounds;
        
        GroupAddressTreeView.Foreground = foregrounds;
        Number1.Foreground = foregrounds;
        Number2.Foreground = foregrounds;
        Number3.Foreground = foregrounds;
        Number4.Foreground = foregrounds;
        Number5.Foreground = foregrounds;
        Number6.Foreground = foregrounds;
        
        StructBibTitleText.Style = titleStyles;
        BorderDefStructTitleText.Style = titleStyles;
        BorderModelsTitleText.Style = titleStyles;
        ModelBibText.Style = titleStyles;
        ModelSettingsText.Style = titleStyles;
        AddressTitleText.Style = titleStyles;
        NameTextBlock.Style = titleStyles;
        
        BorderAllStruct.Style = borderStyles;
        BorderDefStructTitle.Style = borderTitleStyles;
        BorderAllModels.Style = borderStyles;
        BorderModelTitle.Style = borderTitleStyles;
        BorderAddModel.Style = borderStyles;
        BorderModelBib.Style = borderStyles;
        BorderStructBib.Style = borderStyles;
        BorderAddress.Style = borderStyles;
        BorderElement.Style = borderStyles;
        BorderModelNameTitle.Style = borderTitleStyles;
        
        SearchModelButton.Style = searchbuttonStyle;
        SearchAddressButton.Style = searchbuttonStyle;
        StructSupprButton.Style = supprButtonStyle;
        ModelSupprButton.Style = supprButtonStyle;
        StructuresBox.ItemContainerStyle = boxItemStyle;
        ModelsBox.ItemContainerStyle = boxItemStyle;
        
    }

    /// <summary>
    /// Updates the text contents of the connection window (only French and English).
    /// </summary>
    private void TranslateWindowContents()
    {
        if (_viewModel.AppSettings.AppLang == "FR")
        {
            Resources["ImportButton"] = "Importer un projet";
            Resources["ImportXmlButton"] = "Importer des adresses";
            Resources["TestButton"] = "Paramètres de test";
            Resources["ExportButton"] = "Exporter le rapport";
            
            Resources["Library"] = "Bibliothèque";
            Resources["PredefinedStructures"] = "Structures Prédéfinies";
            Resources["NewStructure"] = "Nouvelle Structure";
            Resources["ModelsTitle"] = "Modèles Fonctionnels";
            Resources["CreateFunctionalModel"] = "Créer un Modèle Fonctionnel";
            Resources["ModelsParametersTitle"] = "Paramètres du Modèle Fonctionnel";
            Resources["Name:"] = "Nom :";
            
            Resources["TestedElement"] = "Élément à Tester";
            Resources["DptType"] = "Type de DPT :";
            Resources["LinkedAddress"] = "Adresse liée :";
            Resources["Values"] = "Valeurs :";
            Resources["Dispatch(es)"] = "Envoi(s)";
            Resources["Reception(s)"] = "Réception(s)";
            Resources["ShowAllModels"] = "Afficher tous les modèles";
            Resources["ShowTheModel"] = "Afficher le modèle sélectionné";
            
            Resources["GroupAdressesTitle"] = "Adresses de Groupe";
        }
        else
        {
            Resources["ImportButton"] = "Import new project";
            Resources["ImportXmlButton"] = "Import addresses";
            Resources["TestButton"] = "Test parameters";
            Resources["ExportButton"] = "Export test report";
            
            Resources["Library"] = "Library";
            Resources["PredefinedStructures"] = "Structures of Functional Models";
            Resources["NewStructure"] = "New Structure";
            Resources["ModelsTitle"] = "Functional Models";
            Resources["CreateFunctionalModel"] = "New Functional Model";
            Resources["ModelsParametersTitle"] = "Model Parameters";
            Resources["Name:"] = "Name:";
            
            Resources["TestedElement"] = "Tested Element";
            Resources["DptType"] = "DPT Type:";
            Resources["LinkedAddress"] = "Linked Address:";
            Resources["Values"] = "Values:";
            Resources["Dispatch(es)"] = "Dispatch(es)";
            Resources["Reception(s)"] = "Reception(s)";
            Resources["ShowAllModels"] = "Show all models";
            Resources["ShowTheModel"] = "Show selected model";
            
            Resources["GroupAdressesTitle"] = "Group Addresses";
        }
    }

    /// <summary>
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the connection window according to the application settings.
    /// </summary>
    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (langChanged)
        {
            TranslateWindowContents();
        }
        if (themeChanged)
        {
            ApplyThemeToWindow();
        }
        if (scaleChanged)
        {
            ApplyScaling();
        }


    }
    
    
    //--------------------- Gestion des boutons -----------------------------------------------------//
    
    
    // <<<<<<<<<<<<<<<<<<<< BANDEAU SUPÉRIEUR >>>>>>>>>>>>>>>>>>>>
    
    /// <summary>
    /// Handles the button click event to open the <see cref="View.Windows.ConnectionWindow"/>.
    /// Opens an instance of ConnectionWindow or focuses on it.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OpenConnectionWindow(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowConnectionWindow();
    }
    
    /// <summary>
    /// Handles the button click event to import a KNX project file.
    /// Displays an OpenFileDialog for the user to select the project file and extracts necessary files.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ImportProjectButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Waiting for user to select KNX project file");

        // Initialise la variable indiquant que l'utilisateur n'a pas choisi d'importer un fichier d'adresses de groupes
        UserChooseToImportGroupAddressFile = false;
        
        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        OpenFileDialog openFileDialog = new()
        {
            // Définit le titre de la boîte de dialogue en fonction de la langue de l'application (pas encore implémenté) 
            Title = _viewModel.AppSettings.AppLang switch
            {
                // Arabe
                "AR" => "اختر مشروع KNX للاستيراد",
                // Bulgare
                "BG" => "Изберете KNX проект за импортиране",
                // Tchèque
                "CS" => "Vyberte projekt KNX k importu",
                // Danois
                "DA" => "Vælg et KNX-projekt til import",
                // Allemand
                "DE" => "Wählen Sie ein KNX-Projekt zum Importieren",
                // Grec
                "EL" => "Επιλέξτε ένα έργο KNX προς εισαγωγή",
                // Anglais
                "EN" => "Select a KNX project to import",
                // Espagnol
                "ES" => "Seleccione un proyecto KNX para importar",
                // Estonien
                "ET" => "Valige KNX projekt importimiseks",
                // Finnois
                "FI" => "Valitse KNX-projekti tuominen",
                // Hongrois
                "HU" => "Válasszon egy KNX projektet importálásra",
                // Indonésien
                "ID" => "Pilih proyek KNX untuk diimpor",
                // Italien
                "IT" => "Seleziona un progetto KNX da importare",
                // Japonais
                "JA" => "インポートするKNXプロジェクトを選択",
                // Coréen
                "KO" => "가져올 KNX 프로젝트를 선택하세요",
                // Letton
                "LV" => "Izvēlieties KNX projektu importēšanai",
                // Lituanien
                "LT" => "Pasirinkite KNX projektą importuoti",
                // Norvégien
                "NB" => "Velg et KNX-prosjekt å importere",
                // Néerlandais
                "NL" => "Selecteer een KNX-project om te importeren",
                // Polonais
                "PL" => "Wybierz projekt KNX do importu",
                // Portugais
                "PT" => "Selecione um projeto KNX para importar",
                // Roumain
                "RO" => "Selectați un proiect KNX pentru import",
                // Russe
                "RU" => "Выберите проект KNX для импорта",
                // Slovaque
                "SK" => "Vyberte projekt KNX na import",
                // Slovène
                "SL" => "Izberite KNX projekt za uvoz",
                // Suédois
                "SV" => "Välj ett KNX-projekt att importera",
                // Turc
                "TR" => "İçe aktarılacak bir KNX projesi seçin",
                // Ukrainien
                "UK" => "Виберіть проект KNX для імпорту",
                // Chinois simplifié
                "ZH" => "选择要导入的KNX项目",
                // Cas par défaut (français)
                _ => "Sélectionnez un projet KNX à importer"
            },
            // Définit le filtre de fichiers pour n'afficher que les fichiers de projet KNX (*.knxproj) et tous les fichiers
            Filter = _viewModel.AppSettings.AppLang switch
            {
                // Arabe
                "AR" => "ملفات مشروع KNX|*.knxproj|جميع الملفات|*.*",
                // Bulgare
                "BG" => "KNX проектни файлове|*.knxproj|Всички файлове|*.*",
                // Tchèque
                "CS" => "Soubor projektu KNX|*.knxproj|Všechny soubory|*.*",
                // Danois
                "DA" => "KNX projektfiler|*.knxproj|Alle filer|*.*",
                // Allemand
                "DE" => "KNX-Projektdateien|*.knxproj|Alle Dateien|*.*",
                // Grec
                "EL" => "Αρχεία έργου KNX|*.knxproj|Όλα τα αρχεία|*.*",
                // Anglais
                "EN" => "KNX Project Files|*.knxproj|All Files|*.*",
                // Espagnol
                "ES" => "Archivos de proyecto KNX|*.knxproj|Todos los archivos|*.*",
                // Estonien
                "ET" => "KNX projekti failid|*.knxproj|Kõik failid|*.*",
                // Finnois
                "FI" => "KNX-projektitiedostot|*.knxproj|Kaikki tiedostot|*.*",
                // Hongrois
                "HU" => "KNX projektfájlok|*.knxproj|Minden fájl|*.*",
                // Indonésien
                "ID" => "File Proyek KNX|*.knxproj|Semua file|*.*",
                // Italien
                "IT" => "File di progetto KNX|*.knxproj|Tutti i file|*.*",
                // Japonais
                "JA" => "KNXプロジェクトファイル|*.knxproj|すべてのファイル|*.*",
                // Coréen
                "KO" => "KNX 프로젝트 파일|*.knxproj|모든 파일|*.*",
                // Letton
                "LV" => "KNX projekta faili|*.knxproj|Visi faili|*.*",
                // Lituanien
                "LT" => "KNX projekto failai|*.knxproj|Visi failai|*.*",
                // Norvégien
                "NB" => "KNX prosjektfiler|*.knxproj|Alle filer|*.*",
                // Néerlandais
                "NL" => "KNX-projectbestanden|*.knxproj|Alle bestanden|*.*",
                // Polonais
                "PL" => "Pliki projektu KNX|*.knxproj|Wszystkie pliki|*.*",
                // Portugais
                "PT" => "Arquivos de projeto KNX|*.knxproj|Todos os arquivos|*.*",
                // Roumain
                "RO" => "Fișiere proiect KNX|*.knxproj|Toate fișierele|*.*",
                // Russe
                "RU" => "Файлы проекта KNX|*.knxproj|Все файлы|*.*",
                // Slovaque
                "SK" => "Súbory projektu KNX|*.knxproj|Všetky súbory|*.*",
                // Slovène
                "SL" => "Datoteke projekta KNX|*.knxproj|Vse datoteke|*.*",
                // Suédois
                "SV" => "KNX-projektfiler|*.knxproj|Alla filer|*.*",
                // Turc
                "TR" => "KNX Proje Dosyaları|*.knxproj|Tüm Dosyalar|*.*",
                // Ukrainien
                "UK" => "Файли проекту KNX|*.knxproj|Усі файли|*.*",
                // Chinois simplifié
                "ZH" => "KNX 项目文件|*.knxproj|所有文件|*.*",
                // Cas par défaut (français)
                _ => "Fichiers projet ETS|*.knxproj|Tous les fichiers|*.*"
            },
            FilterIndex = 1, // Définit l'index par défaut du filtre
            Multiselect = false // Empêche la sélection de plusieurs fichiers à la fois
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();

        if (result == true) // Si l'utilisateur a sélectionné un fichier
        {
            // Récupérer le chemin du fichier sélectionné
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"File selected: {openFileDialog.FileName}");

            // Si le file manager n'existe pas ou que l'on n'a pas réussi à extraire les fichiers du projet, on annule l'opération
            if (_viewModel.ExtractProjectFilesCommand is RelayCommandWithResult<string, bool> extractProjectFilesCommand &&
                !extractProjectFilesCommand.ExecuteWithResult(openFileDialog.FileName)) return;
            
            _cancellationTokenSource = new CancellationTokenSource(); // à VOIR SI UTILE ICI
            // Partie management des adresses de groupes
            // Exécute la commande pour trouver les fichiers XML contenant les adresses de groupes dans le dossier du projet
            _viewModel.FindZeroXmlCommand.Execute(_viewModel.ProjectFolderPath);
            // Exécute la commande pour extraire les adresses de groupe du projet
            _viewModel.ExtractGroupAddressCommand.Execute(null);
            // Modifie l'affichage pour que les adresses de groupes soient mises dans un TreeView
            Application.Current.Dispatcher.InvokeAsync( async() =>
            {
                await LoadAddressesOntoTreeViewAsync(GroupAddressTreeView);
            });
            
        }
        else // Si l'utilisateur annule la sélection de fichier
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }


    }

    private void PrintStructuresButtonClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("############-- STRUCTURES --############");
        Console.WriteLine("A total of " + _viewModel.Structures.Count + " structures");
        foreach(var structure in _viewModel.Structures)
            Console.WriteLine("structure " + structure.FullName);
    }

    private void PrintStructureDictionaryButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.PrintStructureDictionaryCommand.Execute(null);
    }

    
    /// <summary>
    /// Handles the button click event to import a group addresses file.
    /// Displays an OpenFileDialog for the user to select the file and extracts necessary files.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ImportGroupAddressFileButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Waiting for user to select group addresses file");

        // Met à jour la variable indiquant que l'utilisateur a choisi d'importer un fichier d'adresses de groupe
        UserChooseToImportGroupAddressFile = true;
        
        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        OpenFileDialog openFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Sélectionnez un fichier d'adresses de groupe à importer",
                _ => "Select a group address file to import"
            },
                
            // Applique un filtre pour n'afficher que les fichiers XML ou tous les fichiers
            Filter = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Fichiers d'adresses de groupes|*.xml|Tous les fichiers|*.*",
                _ => "group address file|*.xml|All files|*.*"
            },
            
            // Définit l'index par défaut du filtre (fichiers XML d'adresses de groupes)
            FilterIndex = 1,
            // N'autorise pas la sélection de plusieurs fichiers à la fois
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"File selected: {openFileDialog.FileName}");

            // Si le file manager n'existe pas ou que l'on n'a pas réussi à extraire les fichiers du projet, on annule l'opération
            if (_viewModel.ExtractGroupAddressFileCommand is RelayCommandWithResult<string, bool> command &&
                !command.ExecuteWithResult(openFileDialog.FileName)) return;
            
            _cancellationTokenSource = new CancellationTokenSource(); // à VOIR SI UTILE ICI
            // Exécute la commande pour extraire les adresses de groupes du fichier sélectionné
            _viewModel.ExtractGroupAddressCommand.Execute(null);
            
            // Fonction permettant d'afficher les adresses de groupe sur la fenêtre principale
            Application.Current.Dispatcher.InvokeAsync( async() =>
            {
                await LoadAddressesOntoTreeViewAsync(GroupAddressTreeView);
            });
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }

    }

    /// <summary>
    /// Handles the button click event to export a dictionary file.
    /// Displays an SaveFileDialog for the user to select the filepath to which should be saved the dictionary.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ExportDictionnaryButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Waiting for user to select dictionary file");

        // Créer une nouvelle instance de SaveFileDialog pour permettre à l'utilisateur de sauvegarder le fichier
        SaveFileDialog saveFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Sélectionnez un dictionnaire de modèles à exporter",
                _ => "Select a model dictionary file to export"
            },

            // Applique un filtre pour n'afficher que les fichiers XML
            Filter = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Fichier de dictionnaire|*.xml",
                _ => "dictionary file|*.xml"
            },

            // Définit l'index par défaut du filtre (fichiers XML du dictionnaire)
            FilterIndex = 1,
            FileName = $"KNXVI-dictionnary-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xml",
            DefaultExt = ".xml"
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = saveFileDialog.ShowDialog();
        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"Dictionary saved at: {saveFileDialog.FileName}");
            
            if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);
            
            _cancellationTokenSource = new CancellationTokenSource(); // à VOIR SI UTILE ICI
            _viewModel.ExportListAndDictionaryCommand.Execute(saveFileDialog.FileName);
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }
    }

    /// <summary>
    /// Handles the button click event to import a dictionary file.
    /// Displays an OpenFileDialog for the user to select the file and extracts the necessary information.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ImportDictionnaryButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Waiting for user to select dictionary file");

        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        OpenFileDialog openFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Sélectionnez un dictionnaire de modèles à importer",
                _ => "Select a model dictionary file to import"
            },

            // Applique un filtre pour n'afficher que les fichiers XML ou tous les fichiers
            Filter = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Fichier de dictionnaire|*.xml|Tous les fichiers|*.*",
                _ => "dictionary file|*.xml|All files|*.*"
            },

            // Définit l'index par défaut du filtre (fichiers XML du dictionnaire)
            FilterIndex = 1,
            // N'autorise pas la sélection de plusieurs fichiers à la fois
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();
        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"File selected: {openFileDialog.FileName}");

            _cancellationTokenSource = new CancellationTokenSource(); // à VOIR SI UTILE ICI
            _viewModel.ImportListAndDictionaryCommand
                .Execute(openFileDialog.FileName);
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }
    }

    /// <summary>
    /// Handles the button click event to open the <see cref="View.Windows.SettingsWindow"/>..
    /// Opens an instance of SettingsWindow or focuses on it.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void SettingsButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowSettingsWindow();
    }
    
    /// <summary>
    /// Handles the button click event to open the <see cref="View.Windows.TestConfigWindow"/>.
    /// Opens an instance of the Test Configuration Window or focuses on it.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OnTestConfigButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowTestConfigWindow();
    }
    
    /// <summary>
    /// Handles the button click event to open the <see cref="View.Windows.ReportCreationWindow"/>.
    /// Opens an instance of Report Creation Window or focuses on it.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OnCreateTestReportButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowReportCreationWindow();
    }
    
    /// <summary>
    /// Handles the event of closing the main window.
    /// Shuts the application down.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ClosingMainWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        _cancellationTokenSource.Cancel();
        Application.Current.Shutdown();
    }

    
    // <<<<<<<<<<<<<<<<<<<< COLONNE 1 >>>>>>>>>>>>>>>>>>>>
    
    /// <summary>
    /// Handles the button click event to create a Structure.
    /// Adds a Structure to the Dictionary of Functional Model Structures.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void CreateStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CreateStructureDictionaryCommand.Execute(null);
        
        // Also, selecting the newly created model and scrolling down to it
        _viewModel.SelectedStructure = _viewModel.Structures.Last();
        _viewModel.EditedStructureSave = new FunctionalModelStructure(_viewModel.SelectedStructure);
        
        var predefinedStructuresScrollViewer = FindVisualChild<ScrollViewer>(StructuresBox);
        predefinedStructuresScrollViewer?.ScrollToEnd();
        
        _windowManager.ShowStructureEditWindow(); // ouverture de la fenêtre d'édition de MF
    }
    
    /// <summary>
    /// Handles the button click event to duplicate a Structure.
    /// Adds a Structure to the Dictionary of Functional Model Structures by copying the selected one.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void DuplicateStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.DuplicateStructureDictionaryCommand.Execute(null);
        // TODO : ajouter l'ouverture de la fenêtre d'édition de MF
    }
    
    /// <summary>
    /// Handles the button click event to modify a Structure.
    /// Opens an instance of <see cref="View.Windows.StructureEditWindow"/> and disables the other windows.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void EditStructureButtonClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedStructure != null)
        {
            _viewModel.EditedStructureSave = new FunctionalModelStructure(_viewModel.SelectedStructure);
            _windowManager.ShowStructureEditWindow(); // ouvre la fenêtre d'édition selon la structure sélectionnée
        }
    }
    
    /// <summary>
    /// Handles the button click event to delete a Structure.
    /// Removes a Structure from the Dictionary of Functional Model Structures and all its models.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void DeleteStructuresButtonClick(object sender, RoutedEventArgs e)
    {
        // Store all indexes to delete
        var indexesToDelete = new HashSet<int>();
            
        // On cherche à récupérer l'index des structures à supprimer
        // les index sont disponibles à partir des checkbox associées au listbox items
        // on parcourt tous les listbox items
        for (var i = 0; i < StructuresBox.Items.Count; i++)
        {
            // Récupère le ListBoxItem correspondant
            var itemContainer = StructuresBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
            if (itemContainer == null)
                continue;

            // Récupère la CheckBox dans le template
            if (itemContainer.Template.FindName("DeleteCheckBox", itemContainer) is CheckBox checkBox)
            {
                var structure = StructuresBox.Items[i] as FunctionalModelStructure; // La Structure

                // Si la case est cochée on supprime la structure
                if (checkBox.IsChecked == true)
                    if (structure != null)
                    {
                        indexesToDelete.Add(structure.Model.Key-1); // supprimer la structure
                    }
            }
        }
        
        // toutes les supprimer dans l'ordre inverse pour éviter que les structures changent d'index avant d'avoir été supprimées
        foreach (var indexToDelete in indexesToDelete.OrderByDescending(i => i))
            _viewModel.DeleteStructureDictionaryCommand.Execute(indexToDelete);

    }
    
    // <<<<<<<<<<<<<<<<<<<< COLONNE 2 >>>>>>>>>>>>>>>>>>>>

    /// <summary>
    /// Handles the button click event to add a Functional Model to the list corresponding to the selected structure.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void AddFunctionalModelToListButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddFunctionalModelToListCommand.Execute(null);
        var functionalModelsScrollViewer = FindVisualChild<ScrollViewer>(ModelsBox);
        functionalModelsScrollViewer?.ScrollToEnd();
        StructuresBox.ApplyTemplate();
    }

    /// <summary>
    /// Handles the button click event to delete a Functional Model.
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void DeleteFunctionalModelFromListButtonClick(object sender, RoutedEventArgs e)
    {
        // Store all indexes to delete
        var indexesToDelete = new HashSet<int>();
            
        // On cherche à récupérer l'index des structures à supprimer
        // les index sont disponibles à partir des checkbox associées au listbox items
        // on parcourt tous les listbox items
        for (var i = 0; i < ModelsBox.Items.Count; i++)
        {
            // Récupère le ListBoxItem correspondant
            var itemContainer = ModelsBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
            if (itemContainer == null)
                continue;

            // Récupère la CheckBox dans le template
            if (itemContainer.Template.FindName("DeleteCheckBox", itemContainer) is CheckBox checkBox)
            {
                var model = ModelsBox.Items[i] as FunctionalModel; // Le Modèle Fonctionnel

                // Si la case est cochée on supprime lae modèle fonctionnel
                if (checkBox.IsChecked == true)
                    if (model != null)
                    {
                        indexesToDelete.Add(model.Key-1); // supprimer le modèle
                    }
            }
        }
        
        // tous les supprimer dans l'ordre inverse pour éviter que les modèles changent d'index avant d'avoir été supprimés
        foreach (var indexToDelete in indexesToDelete.OrderByDescending(i => i))
            _viewModel.DeleteFunctionalModelFromListCommand.Execute(indexToDelete);
        
    }
    
    
    // <<<<<<<<<<<<<<<<<<<< COLONNE 3 >>>>>>>>>>>>>>>>>>>>
    
    /// <summary>
    /// Handles the button click event to add a Test to a Tested Element
    /// Adds a line of values to the Tested Element
    /// The number of fields added is equal to the number of DPTs in the Tested Element
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void AddTestToElementButtonClick(object sender, RoutedEventArgs e)
    {
        // Code que je ne comprends pas qui sert à récupérer l'index de l'item depuis lequel le clic a été effectué
        // Dans ce cas, il s'agit de l'index du Tested Element qui est à supprimer
        // Pour utiliser ce segment de code, il faut avoir une référence sur la listbox
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep);
        
        if (dep is null) return;
        // Trouve l'élément qui possède le bouton qui a été cliqué et exécute l'ajout du test
        var indexElement = SelectedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        _viewModel.AddTestToElementCommand.Execute(_viewModel.SelectedModel?.ElementList[indexElement]);
    }
    
    /// <summary>
    /// Handles the button click event to remove a Test from a Tested Element
    /// Deletes a full line of values to the Tested Element
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void RemoveTestFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        if (dep is null) return;
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        if (dep is null) return;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep is null) return;
        var indexElement = SelectedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        // Find the Test's index
        var button = (Button)sender;
        var currentItem = button.DataContext; // L'élément lié à ce bouton (BigIntegerValue)
        var itemsControl = FindParent<ItemsControl>(button); // L'ItemsControl parent (lié à IntValue)
        var indexTest = 0;
        if (itemsControl != null)
            indexTest = itemsControl.Items.IndexOf(currentItem); // L'index dans la collection
        _viewModel.RemoveTestFromElementCommand.Execute((_viewModel.SelectedModel?.ElementList[indexElement], indexTest));
    }

    /// <summary>
    /// Handles the button click to deactivate a testCmd value(not used)
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void DeactivateValueCmdButtonClick(object sender, RoutedEventArgs e)
    {
        DeactivateValue(sender, e, "TestsCmd");
    }
    
    /// <summary>
    /// Handles the button click to deactivate a testIE value
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void DeactivateValueIeButtonClick(object sender, RoutedEventArgs e)
    {
        DeactivateValue(sender, e, "TestsIe");
    }

    /// <summary>
    /// Handles the button click to deactivate a test value
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    /// <param name="tests">the type of Test value that is deactivated (TestCmd or TestIe).</param>
    private void DeactivateValue(object sender, RoutedEventArgs e, string tests)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        if (dep is null) return;
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        if (dep is null) return;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep is null) return;
        var indexElement = SelectedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        var button = (Button)sender;
        
        // Find the DPT's index
        var listBoxItem = FindParent<ListBoxItem>(button); // On remonte jusqu’au ListBoxItem parent
        var testsIeListBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem); // Le ListBox parent est celui lié à TestsIe
        DataPointType? testsIeItem = null;
        if (listBoxItem != null)
            testsIeItem = (DataPointType)listBoxItem.DataContext; // Élément TestsIe parent
        int indexDpt = 0;
        if (testsIeItem != null) 
            indexDpt = testsIeListBox.Items.IndexOf(testsIeItem); // Index de cet élément dans TestsIe
        
        // Find the Test's index
        var currentItem = button.DataContext; // L'élément lié à ce bouton (BigIntegerValue)
        var itemsControl = FindParent<ItemsControl>(button); // L'ItemsControl parent (lié à IntValue)
        int indexValue = 0;
        if (itemsControl != null)
            indexValue = itemsControl.Items.IndexOf(currentItem); // L'index dans la collection

        // Effectively deactivate the value
        switch (tests)
        {
            case "TestsCmd":
            {
                if (_viewModel.SelectedModel != null)
                    _viewModel.SelectedModel.ElementList[indexElement].TestsCmd[indexDpt].IntValue[indexValue].IsEnabled = false;
                break;
            }
            case "TestsIe":
            {
                if (_viewModel.SelectedModel != null)
                    _viewModel.SelectedModel.ElementList[indexElement].TestsIe[indexDpt].IntValue[indexValue].IsEnabled = false;
                break;
            }
        }
    }
    
    /// <summary>
    /// Handles the button click to reset to 0 a Cmd Value that has been deactivated because it was unknown 
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void ResetValueCmdButtonClick(object sender, RoutedEventArgs e)
    {
        ResetValue(sender, e, "TestsCmd");
    }
    
    /// <summary>
    /// Handles the button click to reset to 0 a Ie Value that has been deactivated because it was unknown 
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void ResetValueIeButtonClick(object sender, RoutedEventArgs e)
    {
        ResetValue(sender, e, "TestsIe");
    }
    
    /// <summary>
    /// Handles the reset to 0 a Value that has been deactivated because it was unknown 
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    /// <param name="tests">the type of Test value that is reset (TestCmd or TestIe).</param>
    private void ResetValue(object sender, RoutedEventArgs e, string tests)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        if (dep is null) return;
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        if (dep is null) return;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep is null) return;
        var indexElement = SelectedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        var button = (Button)sender;
        
        // Find the DPT's index
        var listBoxItem = FindParent<ListBoxItem>(button); // On remonte jusqu’au ListBoxItem parent
        var testsIeListBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem); // Le ListBox parent est celui lié à TestsIe
        DataPointType? testsIeItem = null;
        if (listBoxItem != null)
            testsIeItem = (DataPointType)listBoxItem.DataContext; // Élément TestsIe parent
        int indexDpt = 0;
        if (testsIeItem != null) 
            indexDpt = testsIeListBox.Items.IndexOf(testsIeItem); // Index de cet élément dans TestsIe
        
        // Find the Test's index
        var currentItem = button.DataContext; // L'élément lié à ce bouton (BigIntegerValue)
        var itemsControl = FindParent<ItemsControl>(button); // L'ItemsControl parent (lié à IntValue)
        int indexValue = 0;
        if (itemsControl != null)
            indexValue = itemsControl.Items.IndexOf(currentItem); // L'index dans la collection

        if (_viewModel.SelectedModel != null)
        {
            // Effectively reset the value
            switch (tests)
            {
                case "TestsCmd":
                {
                    var bigIntegerItem = _viewModel.SelectedModel.ElementList[indexElement].TestsCmd[indexDpt].IntValue[indexValue];
                    
                    if (bigIntegerItem.IsEnabled == false)
                        _viewModel.SelectedModel.ElementList[indexElement].TestsCmd[indexDpt].IntValue[indexValue].IsEnabled = true;
                    else
                        _viewModel.SelectedModel.ElementList[indexElement].TestsCmd[indexDpt].IntValue[indexValue].BigIntegerValue = 0;
                    break;
                }
                case "TestsIe":
                {
                    var bigIntegerItem = _viewModel.SelectedModel.ElementList[indexElement].TestsIe[indexDpt].IntValue[indexValue];
                    
                    if (bigIntegerItem.IsEnabled == false)
                        _viewModel.SelectedModel.ElementList[indexElement].TestsIe[indexDpt].IntValue[indexValue].IsEnabled = true;
                    else
                        _viewModel.SelectedModel.ElementList[indexElement].TestsIe[indexDpt].IntValue[indexValue].BigIntegerValue = 0;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Aynschronously loads addresses onto a treeView to be displayed.
    /// </summary>
    /// <param name="treeView">The treeView onto which will be loaded the addresses.</param>
    private async Task LoadAddressesOntoTreeViewAsync(TreeView treeView)
    {
        var doc = _viewModel.GroupAddressFile?? new XDocument();
        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeView.Items.Clear();

                // Ajouter tous les nœuds récursivement
                if (doc.Root == null) return;
                
                var index = 0;
                
                foreach (var node in doc.Root.Nodes())
                {
                    AddNodeRecursively(node, treeView.Items, 0, index++);
                }
            });
        }
        catch (Exception ex)
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute(ex);
        }
        
    }
    
    /// <summary>
    /// Adds XML nodes recursively to a TreeView.
    /// </summary>
    /// <param name="xmlNode">The XML node to add.</param>
    /// <param name="parentItems">The parent collection of TreeView items.</param>
    /// <param name="level">The depth level of the current XML node.</param>
    /// <param name="index">The index of the current XML node among its siblings.</param>
    private void AddNodeRecursively(XNode xmlNode, ItemCollection parentItems, int level, int index)
    {
        if (xmlNode.NodeType != XmlNodeType.Element) return;
        var treeNode = CreateTreeViewItemFromXmlNode(xmlNode, level, index);
        parentItems.Add(treeNode);
        // Parcourir récursivement les enfants
        var elementNode = xmlNode as XElement;
        if (elementNode == null) return;
        var childIndex = 0;
        foreach (XNode childNode in elementNode.Elements())
        {
            AddNodeRecursively(childNode, treeNode.Items, level + 1, childIndex++);
        }
    }
    
    /// <summary>
    /// Creates a TreeViewItem from an XML node, with its corresponding image.
    /// </summary>
    /// <param name="xmlNode">The XML node to create a TreeViewItem from.</param>
    /// <param name="level">The depth level of the XML node.</param>
    /// <param name="index">The index of the XML node among its siblings.</param>
    /// <returns>A TreeViewItem representing the XML node.</returns>
    private TreeViewItem CreateTreeViewItemFromXmlNode(XNode xmlNode, int level, int index)
    {
        var stack = new StackPanel { Orientation = Orientation.Horizontal };

        // Définir l'icône en fonction du niveau
        var drawingImageKey = level switch
        {
            0 => "Iconlevel1", 1 => "Iconlevel2", _ => "Iconlevel3"
        };

        var drawingImage = Application.Current.Resources[drawingImageKey] as DrawingImage;

        var icon = new Image
        {
            Width = 16, Height = 16,
            Margin = new Thickness(0, 0, 5, 0),
            Source = drawingImage
        };
        var textName = new TextBlock
        {
            Text = ((XElement)xmlNode).Attribute("Name")?.Value,
            FontSize = 12
        };
        var text = ((XElement)xmlNode).Attribute("Address") is not null? " - " + ((XElement)xmlNode).Attribute("Address")?.Value : "";
        var textAddress = new TextBlock
        {
            Text = text,
            FontSize = 12
        };
        text = ((XElement)xmlNode).Attribute("DPTs") is not null? " - " + ((XElement)xmlNode).Attribute("DPTs")?.Value : "";
        var textDpts = new TextBlock
        {
            Text = text,
            FontSize = 12
        };
        stack.Children.Add(icon);
        stack.Children.Add(textName);
        stack.Children.Add(textAddress);
        stack.Children.Add(textDpts);

        var treeNode = new TreeViewItem
        {
            Header = stack,
            Tag = $"{level}-{index}"
        };
        
        if (_viewModel.AppSettings.EnableLightTheme)
            treeNode.Style = (Style)FindResource("TreeViewItemStyleLight");
        else
            treeNode.Style = (Style)FindResource("TreeViewItemStyleDark");
        

        return treeNode;
    }
    
    // <<<<<<<<<<<<<<<<<<<< UTILS >>>>>>>>>>>>>>>>>>>>
    
    // Méthode récursive qui remonte les parents d'un objet jusqu'à atteindre le parent du type passé en paramètre
    // Utilisée dans RemoveTestedElementFromStructureButtonClick
    // Utilisée dans AddTestToElementButtonClick
    // Utilisée dans RemoveTestFromElementButtonClick
    /// <summary>
    /// Recursive method that searches for the parent of the child object of a specific type in the visual tree.
    /// </summary>
    /// <param name="child">The child from which we search for the parent.</param>
    /// <typeparam name="T">The type of item that should be the parent.</typeparam>
    /// <returns>Either null if not parent is found or the parent (found recursively or not).</returns>
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
    }
    
    /// <summary>
    /// Recursive method that searches for the child of the parent that is of a specific type in the visual tree.
    /// </summary>
    /// <param name="parent">The parent from which to search.</param>
    /// <typeparam name="T">The type of the child.</typeparam>
    /// <returns>The child of the specified type, found recursively.</returns>
   private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T correctlyTyped)
                return correctlyTyped;

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }
}