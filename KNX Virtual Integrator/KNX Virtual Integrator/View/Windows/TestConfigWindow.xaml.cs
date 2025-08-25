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
    }
    
    /// <summary>
    /// Handles the TestConfig window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingTestConfigWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        UpdateWindowContents(true, true, true);
        Hide();
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
            Resources["PredefinedStructures"] = "Structures Prédéfinies";
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
            Resources["LaunchTest"] = "Lancer le test";
        }
        else
        {
            Resources["Library"] = "Library";
            Resources["PredefinedStructures"] = "Predefined Structures";
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
            Resources["LaunchTest"] = "Launch Test";
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

        if (_viewModel.AppSettings.EnableLightTheme)
        {
            ChosenTestModelesListBox.Style = (Style)FindResource("ListBoxStyleLight");
            ChosenTestModelesListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerLight");
            ChosenTestModelesListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemLight");
            SelectedElementsListBox.ItemTemplate = (DataTemplate)FindResource("ElementListBoxTemplateLight");
            SelectedElementsListBox.ItemContainerStyle = (Style)FindResource("TestedElementItemContainerStyleLight");
            NomTextBox.Style = (Style)FindResource("StandardTextBoxLight");

            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleLight");
            
            titleStyles = (Style)FindResource("TitleTextLight");
            borderStyles = (Style)FindResource("BorderLight");
            borderTitleStyles = (Style)FindResource("BorderTitleLight");
            searchbuttonStyle = (Style)FindResource("SearchButtonLight");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleLight");
            
            backgrounds = (Brush)FindResource("OffWhiteBackgroundBrush");
        } 
        else
        {
            ChosenTestModelesListBox.Style = (Style)FindResource("ListBoxStyleDark");
            ChosenTestModelesListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerDark");
            ChosenTestModelesListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemDark");
            SelectedElementsListBox.ItemTemplate = (DataTemplate)FindResource("ElementListBoxTemplateDark");
            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleDark");
            SelectedElementsListBox.ItemContainerStyle = (Style)FindResource("TestedElementItemContainerStyleDark");
            NomTextBox.Style = (Style)FindResource("StandardTextBoxDark");
            
            titleStyles = (Style)FindResource("TitleTextDark");
            borderStyles = (Style)FindResource("BorderDark");
            borderTitleStyles = (Style)FindResource("BorderTitleDark");
            searchbuttonStyle = (Style)FindResource("SearchButtonDark");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleDark");
            
            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");
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
            
        Height = 700 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 700 * scale;
        Width = 1200 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 1200 * scale;
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