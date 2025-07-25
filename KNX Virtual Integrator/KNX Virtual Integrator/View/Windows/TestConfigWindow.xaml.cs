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
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Apply Theme not implemented");

        Brush borderBrushes;
        Brush backgrounds;

        if (_viewModel.AppSettings.EnableLightTheme)
        {
            ChosenTestModelesListBox.Style = (Style)FindResource("StandardListBoxLight");
            ChosenTestModelesTitle.Style = (Style)FindResource("TitleTextLight");
            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleLight");
            borderBrushes = (Brush)FindResource("LightGrayBorderBrush");
            backgrounds = (Brush)FindResource("OffWhiteBackgroundBrush");
        } 
        else
        {
            ChosenTestModelesListBox.Style = (Style)FindResource("StandardListBoxDark");
            ChosenTestModelesTitle.Style = (Style)FindResource("TitleTextDark");
            LaunchTestButton.Style = (Style)FindResource("LaunchTestButtonStyleDark");
            borderBrushes = (Brush)FindResource("LightGrayBorderBrush");
            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");
        }

        Background = backgrounds;
        AllModelsAndElementsColumn.Background = backgrounds;
        AllModelsAndElementsColumn.BorderBrush = borderBrushes;
        ChosenModelsColumn.Background = backgrounds;
        ChosenModelsColumn.BorderBrush = borderBrushes;
        
    }
    
    private void ApplyScaling()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Apply Scaling not implemented");
    }

}