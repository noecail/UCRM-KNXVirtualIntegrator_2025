using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.ViewModel;
using KNX_Virtual_Integrator.ViewModel.Commands;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator.View.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    private readonly MainViewModel _viewModel;
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
    public MainWindow(MainViewModel viewModel, WindowManager wm)
    {
        InitializeComponent();
            
        _viewModel = viewModel;
        DataContext = _viewModel;
        _windowManager = wm;
        _cancellationTokenSource = new CancellationTokenSource();

    }

    private void ApplyScaling()
    {
        var scaleFactor = _viewModel.AppSettings.AppScaleFactor / 100f;
        float scale;
        if (scaleFactor <= 1f)
        {
            scale = scaleFactor - 0.1f;
        }
        else
        {
            scale = scaleFactor - 0.2f;
        }
        MainWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 1366 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 1366 * scale;
        Width = 786 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 786 * scale;
    }

    private void ApplyThemeToWindow()
    {
        Style titleStyles;
        Style borderStyles;
        Style borderTitleStyles;
        Style searchbuttonStyle;
        Style boxItemStyle;
        Style supprButtonStyle;
        Brush backgrounds;
        
        if (_viewModel.AppSettings.EnableLightTheme)
        {
            NomTextBox.Style = (Style)FindResource("StandardTextBoxLight");
            NameTextBlock.Style= (Style)FindResource("StandardTextBlockLight");
            
            titleStyles = (Style)FindResource("TitleTextLight");
            borderStyles = (Style)FindResource("BorderLight");
            borderTitleStyles = (Style)FindResource("BorderTitleLight");
            searchbuttonStyle = (Style)FindResource("SearchButtonLight");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleLight");
            supprButtonStyle = (Style)FindResource("DeleteStructureButtonStyleLight");
            backgrounds = (Brush)FindResource("OffWhiteBackgroundBrush");

        }
        else
        {
            NomTextBox.Style = (Style)FindResource("StandardTextBoxDark");
            NameTextBlock.Style= (Style)FindResource("StandardTextBlockDark");
            
            titleStyles = (Style)FindResource("TitleTextDark");
            borderStyles = (Style)FindResource("BorderDark");
            borderTitleStyles = (Style)FindResource("BorderTitleDark");
            searchbuttonStyle = (Style)FindResource("SearchButtonDark");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleDark");
            supprButtonStyle = (Style)FindResource("DeleteStructureButtonStyleDark");

            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");

        }
        
        Background = backgrounds;
        StructBibTitleText.Style = titleStyles;
        BorderDefStructTitleText.Style = titleStyles;
        BorderModelsTitleText.Style = titleStyles;
        ModelBibText.Style = titleStyles;
        ModelSettingsText.Style = titleStyles;
        AddressTitleText.Style = titleStyles;
        
        BorderAllStruct.Style = borderStyles;
        BorderDefStructTitle.Style = borderTitleStyles;
        BorderDefStruct.Style = borderStyles;
        BorderAllModels.Style = borderStyles;
        BorderModelTitle.Style = borderTitleStyles;
        BorderModels.Style = borderStyles;
        BorderAddModel.Style = borderStyles;
        BorderModelBib.Style = borderStyles;
        BorderStructBib.Style = borderStyles;
        
        
        SearchDefStructButton.Style = searchbuttonStyle;
        SearchModelButton.Style = searchbuttonStyle;
        SearchAddressButton.Style = searchbuttonStyle;
        ModelSupprButton.Style = supprButtonStyle;
        ModelSupprButton.Style = supprButtonStyle;
        StructuresBox.ItemContainerStyle = boxItemStyle;
        ModelsBox.ItemContainerStyle = boxItemStyle;
        
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("MainWindow.ApplyThemeToWindow is not implemented");

    }

    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (langChanged)
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("MainWindow.Translate is not implemented");
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
    /// Handles the button click event to open the connection window.
    /// Opens an instance of ConnectionWindow
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OpenConnectionWindow(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowConnectionWindow();
    }
    
    /// <summary>
    /// Handles the button click event to import a KNX project file.
    /// Displays an OpenFileDialog for the user to select the project file,
    /// extracts necessary files.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
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
        }
        else // Si l'utilisateur annule la sélection de fichier
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }


    }
    
    /// <summary>
    /// Handles the button click event to import a group addresses file.
    /// Displays an OpenFileDialog for the user to select the file,
    /// extracts necessary files.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
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
            Title = "Sélectionnez un fichier d'adresses de groupe à importer",
            // Applique un filtre pour n'afficher que les fichiers XML ou tous les fichiers
            Filter = "Fichiers d'adresses de groupes|*.xml|Tous les fichiers|*.*",
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
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }

    }

    /// <summary>
    /// Handles the button click event to open the settings window.
    /// Opens an instance of SettingsWindow
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void SettingsButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowSettingsWindow();
    }

    private void OnTestConfigButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowTestConfigWindow();
    }
    
    private void OnCreateTestReportButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowReportCreationWindow();
    }
    
    /// <summary>
    /// Handles the event of closing the main window.
    /// Shuts the application down.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
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
    private void CreateStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CreateStructureDictionaryCommand.Execute(null);
        
        // Also, selecting the newly created model and scrolling down to it
        _viewModel.SelectedStructure = _viewModel.Structures.Last();
        PredefinedStructuresScrollViewer.ScrollToEnd();
        
        _windowManager.ShowStructureEditWindow(); // ouverture de la fenêtre d'édition de MF
    }
    
    /// <summary>
    /// Handles the button click event to duplicate a Structure.
    /// Adds a Structure to the Dictionary of Functional Model Structures by copying the selected one.
    /// </summary>
    private void DuplicateStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.DuplicateStructureDictionaryCommand.Execute(null);
        // TODO : ajouter l'ouverture de la fenêtre d'édition de MF
    }

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
                var structure = StructuresBox.Items[i] as FunctionalModel; // La Structure

                // Si la case est cochée on supprime la structure
                if (checkBox.IsChecked == true)
                    if (structure != null)
                    {
                        indexesToDelete.Add(structure.Key-1); // supprimer la structure
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
    private void AddFunctionalModelToListButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddFunctionalModelToListCommand.Execute(null);
        FunctionalModelsScrollViewer.ScrollToEnd();
        StructuresBox.ApplyTemplate();
    }

    /// <summary>
    /// Handles the button click event to delete a Functional Model.
    /// </summary>
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
    /// Handles the button click event to add a Tested Element to an already existing Functional Model.
    /// Adds a Functional Model to the Dictionary of Functional Model Structures.
    /// </summary>
    private void AddTestedElementToStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddTestedElementToStructureCommand.Execute(_viewModel.SelectedModel);
    }
    
    /// <summary>
    /// Handles the button click event to remove a Tested Element from a Functional Model.
    /// </summary>
    private void RemoveTestedElementFromStructureButtonClick(object sender, RoutedEventArgs e)
    {
        // Code que je ne comprends pas qui sert à récupérer l'index de l'item depuis lequel le clic a été effectué
        // Dans ce cas, il s'agit de l'index du Tested Element qui est à supprimer
        // Pour utiliser ce segment de code, il faut avoir une référence sur la listbox
        var dep = (DependencyObject)e.OriginalSource;
        while (dep != null && !(dep is ListBoxItem)) { dep = VisualTreeHelper.GetParent(dep); }
        if (dep == null) return;
        var index = SelectedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        _viewModel.RemoveTestedElementFromStructureCommand.Execute((_viewModel.SelectedModel, index)); 
    }
    

    /// <summary>
    /// Handles the button click event to add a Test to a Tested Element
    /// Adds a line of values to the Tested Element
    /// The number of fields added is equal to the number of DPTs in the Tested Element
    /// </summary>
    private void AddTestToElementButtonClick(object sender, RoutedEventArgs e)
    {
        //_viewModel.AddTestToElement.Execute();
        // TODO : récupérer le selected element
    }
    
    /// <summary>
    /// Handles the button click event to remove a Test from a Tested Element
    /// Deletes a full line of values to the Tested Element
    /// </summary>
    private void RemoveTestFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        //_viewModel.RemoveTestFromElement.Execute();
        // TODO : récupérer le selected element et l'index du test
    }

    /// <summary>
    /// Currently used to display the SelectedModel in the console
    /// Delete later
    /// </summary>
    private void SaveModelButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedModelConsoleWriteCommand.Execute(null);
    }

    /// <summary>
    /// Currently used to display all the models in the console
    /// Delete later
    /// </summary>
    private void FilterModelButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AllModelsConsoleWriteCommand.Execute(null);
    }
    
    
    // <<<<<<<<<<<<<<<<<<<< FENÊTRE ÉDITION >>>>>>>>>>>>>>>>>>>>

    /// <summary>
    /// Handles the button click event to add a DPT to send to a tested element
    /// </summary>
    private void AddDptCmdToElementButtonClick(object sender, RoutedEventArgs e)
    {
        //_viewModel.AddDptCmdToElement.Execute();
        // TODO : récupérer le selected element
    }

    /// <summary>
    /// Handles the button click event to remove a DPT to send from a tested element
    /// </summary>
    private void RemoveCmdDptFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        //_viewModel.RemoveCmdDptFromElement.Execute();
        // TODO : récupérer le selected element et l'index du DPT    
    }
    
    /// <summary>
    /// Handles the button click event to add a DPT to send to a tested element
    /// </summary>
    private void AddDptIeToElementButtonClick(object sender, RoutedEventArgs e)
    {
        //_viewModel.AddDptIeToElement.Execute();
        // TODO : récupérer le selected element
    }

    /// <summary>
    /// Handles the button click event to remove a DPT to send from a tested element
    /// </summary>
    private void RemoveIeDptFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        //_viewModel.RemoveIeDptFromElement.Execute();
        // TODO : récupérer le selected element et l'index du DPT    
    }

    //public bool ScrollToEnd { get; set; } // pour demander à la UI de scroller vers le bas des modèles prédéfinis


    // Used when adding a new model
    // Scrolls to the end of the list of models
    private void PredefinedStructuresScrollToEnd()
    {
        PredefinedStructuresScrollViewer.ScrollToEnd();
    }
    
}