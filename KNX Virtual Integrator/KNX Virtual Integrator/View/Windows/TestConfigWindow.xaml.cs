using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View.Windows;

public partial class TestConfigWindow
{
    private readonly MainViewModel _viewModel;
    
    
    public TestConfigWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        
        UpdateWindowContents(true, true, true);
        
        ModelsBox.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(CheckedModelsHandler));
        ModelsBox.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(UncheckedModelsHandler));
        ModelsBox.LayoutUpdated += CheckIfModelsWasCheckedHandler;
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
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the settings window accordingly to the application settings.
    /// </summary>
    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (langChanged) TranslateWindowContents();
        if (themeChanged) ApplyThemeToWindow();
        if (scaleChanged) ApplyScaling();
    }

    private void TranslateWindowContents()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("TestConfigWindow.Translate is not implemented");
    }

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
            ChosenTestModelesListBox.Style = (Style)FindResource("StandardListBoxLight");
            ChosenTestModelesListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerLight");
            ChosenTestModelesListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemLight");
            

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
            
            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleDark");
            
            titleStyles = (Style)FindResource("TitleTextDark");
            borderStyles = (Style)FindResource("BorderDark");
            borderTitleStyles = (Style)FindResource("BorderTitleDark");
            searchbuttonStyle = (Style)FindResource("SearchButtonDark");
            boxItemStyle = (Style)FindResource("ModelListBoxItemStyleDark");
            
            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");
        }

        Background = backgrounds;
        
        ChosenTestModelesTitle.Style = titleStyles;
        StructBibTitleText.Style = titleStyles;
        BorderDefStructTitleText.Style = titleStyles;
        BorderModelsTitleText.Style = titleStyles;
        ModelBibText.Style = titleStyles;
        
        AllModelsAndElementsColumn.Style = borderStyles;
        ChosenModelsColumn.Style = borderStyles;
        BorderAllStruct.Style = borderStyles;
        BorderDefStructTitle.Style = borderTitleStyles;
        BorderDefStruct.Style = borderStyles;
        BorderAllModels.Style = borderStyles;
        BorderModelTitle.Style = borderTitleStyles;
        BorderModels.Style = borderStyles;
        BorderModelBib.Style = borderStyles;
        BorderStructBib.Style = borderStyles;
        
        SearchDefStructButton.Style = searchbuttonStyle;
        SearchModelButton.Style = searchbuttonStyle;
        
        DefStructureBox.ItemContainerStyle = boxItemStyle;
        ModelsBox.ItemContainerStyle = boxItemStyle;
        
        
        
        
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
        TestConfigWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 700 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 700 * scale;
        Width = 1200 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 1200 * scale;
    }

    private void CheckedModelsHandler(object? sender, EventArgs? e)
    {
        // On cherche à récupérer l'index des modèles/structures qui ont été cochées
        // les index sont disponibles à partir des checkbox associées au listbox items
        // on parcourt tous les listbox items
        for (var i = 0; i < ModelsBox.Items.Count; i++)
        {
            // Récupère le ListBoxItem correspondant
            var itemContainer = ModelsBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

            // Récupère la CheckBox dans le template
            if (itemContainer?.Template.FindName("DeleteCheckBox", itemContainer) is not CheckBox checkBox)
                continue;
            
            var newTestModel = ModelsBox.Items[i] as FunctionalModel; // La Structure
            // Si la case est cochée on supprime la structure
            if (newTestModel is null || checkBox.IsChecked is false) 
                continue;
            if (_viewModel.SelectedTestModels.Contains(newTestModel))
                continue;
            _viewModel.SelectedTestModels.Add(newTestModel);
        }
        
    }
    
    private void UncheckedModelsHandler(object? sender, EventArgs? e)
    {
        // On cherche à récupérer l'index des structures à supprimer
        // les index sont disponibles à partir des checkbox associées au listbox items
        // on parcourt tous les listbox items
        for (var i = 0; i < ModelsBox.Items.Count; i++)
        {
            // Récupère le ListBoxItem correspondant
            var itemContainer = ModelsBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

            // Récupère la CheckBox dans le template
            if (itemContainer?.Template.FindName("DeleteCheckBox", itemContainer) is not CheckBox checkBox)
                continue;
            var newTestModel = ModelsBox.Items[i] as FunctionalModel; // La Structure

            // Si la case est cochée on supprime la structure
            if (newTestModel is null || checkBox.IsChecked is true) 
                continue;
            _viewModel.SelectedTestModels.Remove(newTestModel);
        }
        
    }
    
    private void CheckIfModelsWasCheckedHandler(object? sender, EventArgs? e)
    {
        if (_viewModel.SelectedModelsTestWindow == null || _viewModel.SelectedTestModels.Count == 0)
            return;
        
        // On cherche à récupérer l'index des structures à supprimer
        // les index sont disponibles à partir des checkbox associées au listbox items
        // on parcourt tous les listbox items
        for (var i = 0; i < _viewModel.SelectedModelsTestWindow.Count; i++)
        {
            // Récupère le ListBoxItem correspondant
            var itemContainer = ModelsBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

            // Récupère la CheckBox dans le template
            if (itemContainer?.Template.FindName("DeleteCheckBox", itemContainer) is not CheckBox checkBox)
                continue;
            var structure = ModelsBox.Items[i] as FunctionalModel; // La Structure

            // Si la case est cochée on supprime la structure
            if (structure == null) continue;
            if (_viewModel.SelectedTestModels.Contains(structure))
                checkBox.IsChecked = true;
        }
            
    }

    private void LaunchTestButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.LaunchAnalysisCommand.Execute(_viewModel.SelectedTestModels);
    }
}