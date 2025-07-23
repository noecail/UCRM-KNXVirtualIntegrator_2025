using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KNX_Virtual_Integrator.ViewModel;
using KNX_Virtual_Integrator.ViewModel.Commands;
using Microsoft.Win32;
using Knx.Falcon;

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

    private readonly ConnectionWindow _connectionWindow;
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
    public MainWindow(MainViewModel viewModel, ConnectionWindow cw, WindowManager wm)
    {
        InitializeComponent();
            
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        _connectionWindow = cw;
        _windowManager = wm;
        _cancellationTokenSource = new CancellationTokenSource();

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.ScrollToEnd))
                PredefinedModelsScrollToEnd();
        };
    }

    public void ApplyScaling(float scaleFactor)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("MainWindow.ApplyScaling is not implemented");
    }

    private void ApplyThemeToWindow()
    {
        
        if (_viewModel.AppSettings.EnableLightTheme) 
            _viewModel.ConsoleAndLogWriteLineCommand.Execute(""); // Si le thème clair est actif
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("MainWindow.ApplyThemeToWindow is not implemented");

    }

    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("MainWindow.UpdateWindowContents is not implemented");

        if (langChanged)
        {
            return;
        }
        if (themeChanged)
        {
            ApplyThemeToWindow();
        }
        if (scaleChanged)
        {
            ApplyScaling(1);
        }


    }
    
    
    //--------------------- Gestion des boutons -----------------------------------------------------//
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

    private void ClosingMainWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        _cancellationTokenSource.Cancel();
        Application.Current.Shutdown();
    }

    private void SettingsButtonClick(object sender, RoutedEventArgs e)
    {
        _windowManager.ShowSettingsWindow();
    }

    private void OnWriteButtonClick(object sender, RoutedEventArgs e)
    {
        var groupAddress = new GroupAddress("1/3/1");
        var groupValue = new GroupValue(Convert.ToByte(255));
        _viewModel.GroupValueWriteCommand.Execute((groupAddress, groupValue));
    }

    private void OnReadButtonClick(object sender, RoutedEventArgs e)
    {
        var groupAddress = new GroupAddress("1/4/1");
        _viewModel.MaGroupValueReadCommand.Execute((groupAddress));
    }

    private void OpenConnectionWindow(object sender, RoutedEventArgs e)
    {
        _connectionWindow.Show();
    }
    // private void ListBox_Selected(object sender, RoutedEventArgs e)
    // {
    //
    // }

    private void SelectStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectStructureCommand.Execute(null);
    }
    

    
    /// <summary>
    /// Handles the button click event to create a Functional Model.
    /// Adds a Functional Model to the Dictionary of Functional Model Structures.
    /// </summary>
    private void CreateFunctionalModelButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CreateFunctionalModelDictionaryCommand.Execute(null);
    }

    /// <summary>
    /// Handles the button click event to delete a Functional Model.
    /// </summary>
    private void DeleteFunctionalModelButtonClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("On est iciiiiiii");

        _viewModel.DeleteFunctionalModelFromList.Execute(_viewModel.SelectedModel);
    }

    /// <summary>
    /// Handles the button click event to add a Tested Element to an already existing Functional Model.
    /// Adds a Functional Model to the Dictionary of Functional Model Structures.
    /// </summary>
    private void AddTestedElementToModelButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddTestedElementToModel.Execute(_viewModel.SelectedModel);
    }
    
    /// <summary>
    /// Handles the button click event to remove a Tested Element from a Functional Model.
    /// </summary>
    private void RemoveTestedElementFromModelButtonClick(object sender, RoutedEventArgs e)
    {
        // Code que je ne comprends pas qui sert à récupérer l'index de l'item depuis lequel le clic a été effectué
        // Dans ce cas, il s'agit de l'index du Tested Element qui est à supprimer
        // Pour utiliser ce segment de code, il faut avoir une référence
        var dep = (DependencyObject)e.OriginalSource;
        while (dep != null && !(dep is ListBoxItem)) { dep = VisualTreeHelper.GetParent(dep); }
        if (dep == null) return;
        var index = SelectedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        _viewModel.RemoveTestedElementFromModel.Execute((_viewModel.SelectedModel, index)); 
    }

    /*private void AddTestToElementButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddTestToElement(); //récupérer le selected element
    }*/

    /*private void RemoveTestFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveTestFromElement.Execute(); //récupérer le selected element et l'index du test
    }*/

    /*private void AddDptToElementButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddDptToElement.Execute(); //récupérer le selected element
    }*/

    /*private void RemoveDptFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveDptFromElement.Execute(); //récupérer le selected element et l'index du DPT    
    }*/

    // Used when adding a new model
    // Scrolls to the end of the list of models
    private void PredefinedModelsScrollToEnd()
    {
        PredefinedModelsScrollViewer.ScrollToEnd();
    }
}