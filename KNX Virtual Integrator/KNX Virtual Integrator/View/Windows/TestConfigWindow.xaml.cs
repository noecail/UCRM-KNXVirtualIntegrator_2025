using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using KNX_Virtual_Integrator.ViewModel;
using Microsoft.Win32;

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

    }
    
    /// <summary>
    /// Handles the Connection window closing event by canceling the closure, restoring previous settings, and hiding the window.
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
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Connection Translation not implemented");
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
        BorderStructTitleText.Style = titleStyles;
        BorderModelsTitleText.Style = titleStyles;
        ModelBibText.Style = titleStyles;
        
        AllModelsAndElementsColumn.Style = borderStyles;
        ChosenModelsColumn.Style = borderStyles;
        BorderAllStruct.Style = borderStyles;
        BorderDefStructTitle.Style = borderTitleStyles;
        BorderDefStruct.Style = borderStyles;
        BorderStructTitle.Style = borderTitleStyles;
        BorderStruct.Style = borderStyles;
        BorderAllModels.Style = borderStyles;
        BorderModelTitle.Style = borderTitleStyles;
        BorderModels.Style = borderStyles;
        BorderModelBib.Style = borderStyles;
        BorderStructBib.Style = borderStyles;
        
        SearchDefStructButton.Style = searchbuttonStyle;
        SearchModelButton.Style = searchbuttonStyle;
        
        DefStructureBox.ItemContainerStyle = boxItemStyle;
        StructureBox.ItemContainerStyle = boxItemStyle;
        ModelsBox.ItemContainerStyle = boxItemStyle;
        
        
        
        
    }
    
    private void ApplyScaling()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Apply Scaling not implemented");
    }

}