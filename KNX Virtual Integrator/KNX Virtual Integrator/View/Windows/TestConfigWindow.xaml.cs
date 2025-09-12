using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.ViewModel;
using Microsoft.Extensions.DependencyModel;

namespace KNX_Virtual_Integrator.View.Windows;

// Cette fenêtre peut éventuellement être changée en onglet de fenêtre principale
/// <summary>
/// The class of the window that handles the test configuration.
/// </summary>
public partial class TestConfigWindow
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
    /// <summary>
    /// Initializes a new instance of the <see cref="TestConfigWindow"/> class,
    /// loading and applying settings from the appSettings, and subscribing its checkBox handlers to the listBox events
    /// </summary>
    public TestConfigWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        
        UpdateWindowContents(true, true, true);
        
        ModelsBox.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(CheckedModelsHandler));
        ModelsBox.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(UncheckedModelsHandler));
        ModelsBox.LayoutUpdated += CheckIfModelsWasCheckedHandler;
        DefStructureBox.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(CheckedStructureHandler));
        DefStructureBox.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(UncheckedStructureHandler));
        
        // C'est la manière la plus simple que je connais pour déclencher l'event "CollectionChanged" pour
        // appeler à nouveau le ItemTemplateSelector
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(_viewModel.AnalysisState)) return;
            ObservableCollection<TestedFunctionalModel> tempCollection = [];
            foreach (var testedModel in _viewModel.ChosenModelsAndState)
                tempCollection.Add(testedModel);
            _viewModel.ChosenModelsAndState.Clear();
            foreach (var testedModel in tempCollection)
                _viewModel.ChosenModelsAndState.Add(testedModel);
        };
    }
    
    /// <summary>
    /// Handles the TestConfig window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingTestConfigWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        UpdateWindowContents(true, true, true);
        Hide();
        _viewModel.CollapseAnalysisErrorMessageCommand.Execute(null);
        _viewModel.CollapseAnalysisSuccessMessageCommand.Execute(null);
    }
    
    /// <summary>
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the test configuration window according to the application settings.
    /// </summary>
    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (langChanged) TranslateWindowContents();
        if (themeChanged) ApplyThemeToWindow();
        if (scaleChanged) ApplyScaling();
    }
    /// <summary>
    /// This function translates all the texts contained in the test configuration window to the application language.
    /// Only English and French are supported
    /// </summary>
    private void TranslateWindowContents()
    {
        if (_viewModel.AppSettings.AppLang == "FR")
        {
            Resources["Library"] = "Bibliothèque";
            Resources["PredefinedStructures"] = "Structures de Modèles";
            Resources["ModelsTitle"] = "Modèles Fonctionnels";
            Resources["ModelsParametersTitle"] = "Paramètres du Modèle Fonctionnel";
            Resources["Name:"] = "Nom :";
            
            Resources["TestedElement"] = "Élément à Tester";
            Resources["DptType"] = "Type de DPT :";
            Resources["LinkedAddress"] = "Adresse liée :";
            Resources["Values"] = "Valeurs :";
            Resources["Dispatch(es)"] = "Envoi(s)";
            Resources["Reception(s)"] = "Réception(s)";
            
            Resources["TestWindowTitle"] = "Configuration du test";
            Resources["ChosenTestModelsTitle"] = "Modèles Fonctionnels Choisis";
            Resources["AnalysisDelayText"] = "Délai entre chaque élément\r\n (en ms) :";
            Resources["AnalysisTimeoutText"] = "Temps de réponse maximal\r\n (en ms) :";
            Resources["AnalysisError"] = "Erreur lors du test";
            Resources["AnalysisSuccess"] = "Test réussi";
            Resources["ClearTest"] = "Réinitialiser sélection";
            Resources["LaunchTest"] = "Lancer le test";

            Resources["ValuesTooltipTitle"] = "Aide - Valeurs à envoyer et Valeurs attendues en réception";
            Resources["ValuesTooltipMessage"] =
                "Attention. Les valeurs sont à rentrer en décimal.\r\n" +
                "Une valeur rentrée en hexadécimal pourrait être mal comprise et compromettre la validité du test.\r\n" +
                "exemple : Si vous voulez écrire en hexadécimal (hex)4F, écrivez en décimal (dec)79";
            Resources["TestConfigurationTooltipTitle"] = "Aide - Configuration du test";
            Resources["TestConfigurationTooltipMessage"] =
                "Configuration du lancement du test.\r\n" +
                "Choisissez les Modèles Fonctionnels que vous souhaitez inclure dans le test.\r\n" +
                "Le nom des Modèles qui seront testés s'affiche dans l'encadré ci-dessus.\r\n" +
                "Les DPTs de chaque ligne de test des Éléments à Tester des Modèles Fonctionnels inclus seront envoyés sur le bus KNX.\r\n" +
                "La réponse reçue sera ensuite analysée pour déterminée si la ligne de test est une réussite ou un échec.";
            Resources["ClearTooltipTitle"] = "Aide - Annuler";
            Resources["ClearTooltipMessage"] =
                "Annuler la sélection de Modèles.\r\n" +
                "La liste de Modèles sélectionnés sera réinitialisée.\r\n" +
                "Dans le cas où certaines Structures ou certains Modèles seraient encore cochés, \r\n" +
                "il convient de se fier aux noms affichés dans l'encadré ci-dessus pour savoir\r\n" +
                "quels Modèles sont réellement sélectionnés.";
        }
        else
        {
            Resources["Library"] = "Library";
            Resources["PredefinedStructures"] = "Structures of Models";
            Resources["ModelsTitle"] = "Functional Models";
            Resources["ModelsParametersTitle"] = "Model Parameters";
            Resources["Name:"] = "Name:";
            
            Resources["TestedElement"] = "Tested Element";
            Resources["DptType"] = "DPT Type:";
            Resources["LinkedAddress"] = "Linked Address:";
            Resources["Values"] = "Values:";
            Resources["Dispatch(es)"] = "Dispatch(es)";
            Resources["Reception(s)"] = "Reception(s)";
            
            Resources["TestWindowTitle"] = "Test Configuration";
            Resources["ChosenTestModelsTitle"] = "Chosen Functional Models";
            Resources["AnalysisDelayText"] = "Delay between elements\r\n (in ms) :";
            Resources["AnalysisTimeoutText"] = "Maximal answer timeout\r\n (in ms) :";
            Resources["AnalysisError"] = "Error during the test";
            Resources["AnalysisSuccess"] = "Test was successful";
            Resources["ClearTest"] = "Reset selection";
            Resources["LaunchTest"] = "Launch Test";

            Resources["ValuesTooltipTitle"] = "Help - Values to be sent and expected values upon receipt";
            Resources["ValuesTooltipMessage"] = 
                "Caution. Values must be entered in decimal format.\r\n" +
                "A value entered in hexadecimal format may be misinterpreted and compromise the validity of the test.\r\n" +
                "Example: If you want to write hexadecimal (hex)4F, write decimal (dec)79";
            Resources["TestConfigurationTooltipTitle"] = "Help - Test configuration";
            Resources["TestConfigurationTooltipMessage"] =
                "Configuration of the test execution.\r\n" +
                "Choose which Functional Models you wish to include in the test.\r\n" +
                "The name of the Models which will be tested is displayed in the box above.\r\n" +
                "The DPTs of each test line from the Tested Elements of the Functional Models that are included will be sent on the KNX bus.\r\n" +
                "The response received will then be analyzed to determine whether the test line is a success or a failure.";
            Resources["ClearTooltipTitle"] = "Help - Reset";
            Resources["ClearTooltipMessage"] =
                "Rested the selection of Models.\r\n" +
                "The liste of selected Models will be reset.\r\n" +
                "In the event that certain Structures or Models are still checked,\r\n" +
                "you should refer to the names displayed in the box above\r\n" +
                "to find out which Models are actually selected.";
        }
    }

    /// <summary>
    /// This functions applies the light/dark theme to the test configuration window
    /// </summary>
    private void ApplyThemeToWindow()
    {
        Style titleStyles;
        Style borderStyles;
        Style borderTitleStyles;
        Style searchbuttonStyle;
        Style boxItemStyle;
        Brush backgrounds;
        Brush tooltipBackgroundBrush;
        Style tooltipTextBlockStyle;

        if (_viewModel.AppSettings.EnableLightTheme)
        {
            ChosenTestModelesListBox.Style = (Style)FindResource("ListBoxStyleLight");
            ChosenTestModelesListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerLight");
            SelectedElementsListBox.ItemTemplate = (DataTemplate)FindResource("ElementListBoxTemplateLight");
            SelectedElementsListBox.ItemContainerStyle = (Style)FindResource("TestedElementItemContainerStyleLight");
            NomTextBox.Style = (Style)FindResource("StandardTextBoxLight");
            DelayBox.Style = (Style)FindResource("StandardTextBoxLight");
            TimeoutBox.Style = (Style)FindResource("StandardTextBoxLight");
            AnalysisDelay.Style = (Style)FindResource("StandardTextBlockLight");
            AnalysisTimeout.Style = (Style)FindResource("StandardTextBlockLight");

            ClearTestButton.Style = (Style)FindResource("LaunchTestButtonStyleLight");
            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleLight");
            
            titleStyles = (Style)FindResource("TitleTextLight");
            borderStyles = (Style)FindResource("BorderLight");
            borderTitleStyles = (Style)FindResource("BorderTitleLight");
            searchbuttonStyle = (Style)FindResource("SearchButtonLight");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleLight");
            
            backgrounds = (Brush)FindResource("OffWhiteBackgroundBrush");
            
            tooltipBackgroundBrush = (Brush)FindResource("WhiteBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockLight");
        } 
        else
        {
            ChosenTestModelesListBox.Style = (Style)FindResource("ListBoxStyleDark");
            ChosenTestModelesListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerDark");
            SelectedElementsListBox.ItemTemplate = (DataTemplate)FindResource("ElementListBoxTemplateDark");
            
            ClearTestButton.Style = (Style)FindResource("LaunchTestButtonStyleDark");
            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleDark");
            SelectedElementsListBox.ItemContainerStyle = (Style)FindResource("TestedElementItemContainerStyleDark");
            NomTextBox.Style = (Style)FindResource("StandardTextBoxDark");
            DelayBox.Style = (Style)FindResource("StandardTextBoxDark");
            TimeoutBox.Style = (Style)FindResource("StandardTextBoxDark");
            AnalysisDelay.Style = (Style)FindResource("StandardTextBlockDark");
            AnalysisTimeout.Style = (Style)FindResource("StandardTextBlockDark");
            
            titleStyles = (Style)FindResource("TitleTextDark");
            borderStyles = (Style)FindResource("BorderDark");
            borderTitleStyles = (Style)FindResource("BorderTitleDark");
            searchbuttonStyle = (Style)FindResource("SearchButtonDark");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleDark");
            
            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");
            
            tooltipBackgroundBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockDark");
        }

        Background = backgrounds;
        SelectedElementsListBox.Background = backgrounds;
        DefStructureBox.Background = backgrounds;
        ModelsBox.Background = backgrounds;
        TestConfigWindowBorder.Background = backgrounds;
        
        ChosenTestModelesTitle.Style = titleStyles;
        StructBibTitleText.Style = titleStyles;
        BorderDefStructTitleText.Style = titleStyles;
        BorderModelsTitleText.Style = titleStyles;
        ModelBibText.Style = titleStyles;
        NameTextBlock.Style = titleStyles;
        ModelSettingsText.Style = titleStyles;
        
        ChosenModelsColumn.Style = borderStyles;
        BorderAllStruct.Style = borderStyles;
        BorderDefStructTitle.Style = borderTitleStyles;
        BorderAllModels.Style = borderStyles;
        BorderModelTitle.Style = borderTitleStyles;
        BorderModelBib.Style = borderStyles;
        BorderStructBib.Style = borderStyles;
        BorderModelNameTitle.Style = borderTitleStyles;
        BorderElement.Style = borderStyles;
        
        SearchDefStructButton.Style = searchbuttonStyle;
        SearchModelButton.Style = searchbuttonStyle;
        
        DefStructureBox.ItemContainerStyle = boxItemStyle;
        ModelsBox.ItemContainerStyle = boxItemStyle;

        Resources["CurrentTooltipBackgroundBrush"] = tooltipBackgroundBrush;
        Resources["CurrentTooltipTextBlockStyle"] = tooltipTextBlockStyle;
        
        // C'est la manière la plus simple que je connais pour déclencher l'event "CollectionChanged" pour
        // appeler à nouveau le ItemTemplateSelector
        ObservableCollection<TestedFunctionalModel> tempCollection = [];
        foreach (var testedModel in _viewModel.ChosenModelsAndState)
            tempCollection.Add(testedModel);
        _viewModel.ChosenModelsAndState.Clear();
        foreach (var testedModel in tempCollection)
        {
            testedModel.LightTheme = _viewModel.AppSettings.EnableLightTheme;
            _viewModel.ChosenModelsAndState.Add(testedModel);
        }
    }
    
    /// <summary>
    /// Applies scaling to the window by adjusting the layout transform and resizing the window based on the specified scale factor.
    /// </summary>
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
        TestConfigWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 710 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 710 * scale;
        Width = 1225 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 1225 * scale;
    }
    
    /// <summary>
    /// Handles the check event of the checkbox for the model to add it to the list to test
    /// See also <see cref="ViewModel.MainViewModel.SelectedTestModels"/>
    /// </summary>
    /// <param name="sender">The source of the event, the checkbox of a model list item.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void CheckedModelsHandler(object? sender, RoutedEventArgs? e)
    {
        // On ne continue que si le modèle existe réellement
        if (e is null) return;
        var newTestModel = ((FrameworkElement)e.OriginalSource).DataContext as FunctionalModel; // La Structure
        if (newTestModel is null) return;
        
        // On vérifie que le modèle n'existe pas déjà pour éviter les doublons puis on l'ajoute à la liste
        if (_viewModel.SelectedTestModels.Contains(newTestModel)) return;
        _viewModel.SelectedTestModels.Add(newTestModel);
        if (_viewModel.ChosenModelsAndState.Contains(new TestedFunctionalModel(newTestModel))) return;
        _viewModel.ChosenModelsAndState.Add(new TestedFunctionalModel(newTestModel,_viewModel.AppSettings.EnableLightTheme));
    }
    
    /// <summary>
    /// Handles the uncheck event of the checkbox for the model to remove it from the list to test
    /// See also <see cref="ViewModel.MainViewModel.SelectedTestModels"/>
    /// </summary>
    /// <param name="sender">The source of the event, the checkbox of a model list item.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void UncheckedModelsHandler(object? sender, RoutedEventArgs? e)
    {
        // On ne continue que si le modèle existe réellement
        if (e is null) return;
        var newTestModel = ((FrameworkElement)e.OriginalSource).DataContext as FunctionalModel; // La Structure
        if (newTestModel is null) return;
        // La méthode remove enlève le modèle du test en vérifiant son appartenance éventuelle
        _viewModel.SelectedTestModels.Remove(newTestModel);
        _viewModel.ChosenModelsAndState.Remove(new TestedFunctionalModel(newTestModel));
    }
    
    /// <summary>
    /// Checks all models of the current structure to see if they were checked at some point in time
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void CheckIfModelsWasCheckedHandler(object? sender, EventArgs? e)
    {
        if (_viewModel.SelectedModelsTestWindow == null) return;
        
        // On cherche à récupérer l'index des modèles à vérifier
        // les index sont disponibles à partir des checkbox associées au listbox items
        // on parcourt tous les listbox items
        for (var i = 0; i < _viewModel.SelectedModelsTestWindow.Count; i++)
        {
            // Récupère le ListBoxItem correspondant
            var itemContainer = ModelsBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

            // Récupère la CheckBox dans le template
            if (itemContainer?.Template.FindName("DeleteCheckBox", itemContainer) is not CheckBox checkBox)
                continue;
            var model = ModelsBox.Items[i] as FunctionalModel; // Le Functional Model
            if (model == null) continue;
            
            // Vérifie que le modèle a été coché en le cherchant dans la liste des tests
            checkBox.IsChecked = _viewModel.SelectedTestModels.Contains(model);
        }
            
    }

    /// <summary>
    /// Handles the check event of the checkbox for structures to check all of its models.
    /// See also <see cref="ViewModel.MainViewModel.AddStructToTestModels"/>
    /// </summary>
    /// <param name="sender">The source of the event, the checkbox of a structure list item.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void CheckedStructureHandler(object? sender, RoutedEventArgs? e)
    {
        // On ne continue que si la checkbox existe et que sa structure aussi
        if (e is null) return;
        var newTestStruct = ((FrameworkElement)e.OriginalSource).DataContext as FunctionalModelStructure;
        if (newTestStruct is null) return;
        // On coche tous les modèles de la structure
        _viewModel.AddStructToTestModels(newTestStruct.Model.Key - 1);
        // Mettre à jour l'affichage
        if (newTestStruct.Equals(_viewModel.SelectedStructureTestWindow))
        {
            CheckIfModelsWasCheckedHandler(sender, e);
        }
    }

    /// <summary>
    /// Handles the uncheck event of the checkbox for structures to uncheck all of its models.
    /// See also <see cref="ViewModel.MainViewModel.RmvStructFromTestModels"/>
    /// </summary>
    /// <param name="sender">The source of the event, the checkbox of a structure list item.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void UncheckedStructureHandler(object? sender, RoutedEventArgs? e)
    {
        // On ne continue que si la checkbox existe et que sa structure aussi
        if (e is null) return;
        var newTestStruct = ((FrameworkElement)e.OriginalSource).DataContext as FunctionalModelStructure;
        if (newTestStruct is null) return;
        // On décoche tous les modèles de la structure
        _viewModel.RmvStructFromTestModels(newTestStruct.Model.Key - 1);
        // Mettre à jour l'affichage
        if (newTestStruct.Equals(_viewModel.SelectedStructureTestWindow))
        {
            CheckIfModelsWasCheckedHandler(sender, e);
        }
    }
    /// <summary>
    /// Handles the clear event when the user clicks on the clear button of the window.
    /// It will clear the Models to test, reset the timeout and the latency.
    /// </summary>
    /// <param name="sender">The source of the event, typically the header control.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void OnClearButtonClick_ClearModelsToTest(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearModelsToTestAndResetTimes();
        CheckIfModelsWasCheckedHandler(sender, e);
    }
    
    
    /// <summary>
    /// Handles the launch of the analysis on a different thread to not freeze the UI (WIP)
    /// </summary>
    /// <param name="sender">The source of the event, typically the header control.</param>
    /// <param name="e">Event data containing information about the mouse button event.</param>
    private void LaunchTestButton_OnClick(object sender, RoutedEventArgs e)
    {

        Dispatcher.InvokeAsync(() =>
        {
            _viewModel.LaunchAnalysisCommand.Execute(_viewModel.SelectedTestModels);
        });
    }
}

/// <summary>
/// Implementation of DataTemplateSelector in order to update the status of the analysis between 3 images :
/// Waiting, Running and Finished.
/// Handles Theme changes.
/// </summary>
public class ChosenModelDataTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// The method called when the CollectionChanged event of the listBox ItemSource is raised.
    /// </summary>
    /// <param name="item">The item concerned by CollectionChanged.</param>
    /// <param name="container">The visual container.</param>
    /// <returns>The concerned item data template.</returns>
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        FrameworkElement? element = container as FrameworkElement;
        if (element is not null && item is not null && item is TestedFunctionalModel)
        {
                TestedFunctionalModel? model = item as TestedFunctionalModel;
                if (model is null) return null;
                if (model.LightTheme)
                {
                    if (model.State == "Waiting") return element.FindResource("WaitListBoxItemLight") as DataTemplate;
                    if (model.State == "Running") return element.FindResource("RunListBoxItemLight") as DataTemplate;
                    if (model.State == "Finished") return element.FindResource("FinListBoxItemLight") as DataTemplate;
                    return element.FindResource("InvListBoxItemLight") as DataTemplate;
                }
                if (model.State == "Waiting") return element.FindResource("WaitListBoxItemDark") as DataTemplate;
                if (model.State == "Running") return element.FindResource("RunListBoxItemDark") as DataTemplate;
                if (model.State == "Finished") return element.FindResource("FinListBoxItemDark") as DataTemplate;
                return element.FindResource("InvListBoxItemDark") as DataTemplate;
        }
        return null;
    }
}