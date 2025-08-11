using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View.Windows;

/// <summary>
/// Window used to edit a Functional Model structure
/// Is opened when a new Functional Model Structure is created
/// Can also be opened later to edit an already existing Functional Model Structure
/// </summary>


public partial class StructureEditWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    private readonly MainViewModel _viewModel;
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- MÉTHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    
    /// <summary>
    /// Default constructor for StructureEditWindow
    /// </summary>
    public StructureEditWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        
        _viewModel = mainViewModel;
        DataContext = _viewModel;
        
        UpdateWindowContents();
    }
    
    
    /// <summary>
    /// Handles the Model Edit window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingStructureEditWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        //UpdateWindowContents(true, true, true);
        Hide();
    }    
    
    
    
    // TODO : implement later
    
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
            ApplyScaling();
        }
    }
    
    private void ApplyThemeToWindow()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("StructureEditWindow.ApplyThemeToWindow is not implemented");
    }
    private void ApplyScaling()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("StructureEditWindow.ApplyScaling is not implemented");
    }
    
    
    // --------------------------------------  Boutons  --------------------------------------
    private void DeleteStructureButtonClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedStructure != null)
        {
            _viewModel.DeleteStructureDictionaryCommand.Execute(_viewModel.SelectedStructure.Model.Key - 1);
        }

        // fermer la fenetre edit
        Hide();
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
        var index = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        _viewModel.RemoveTestedElementFromStructureCommand.Execute((_viewModel.SelectedStructureModel, index));
    }
    
    /// <summary>
    /// Handles the button click event to add a Tested Element to an already existing Functional Model.
    /// Adds a Functional Model to the Dictionary of Functional Model Structures.
    /// </summary>
    private void AddTestedElementToStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddTestedElementToStructureCommand.Execute(_viewModel.SelectedStructureModel);
        var testedElementsScrollViewer = FindVisualChild<ScrollViewer>(TestedElementsListBox);
        testedElementsScrollViewer.ScrollToEnd();
    }

    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
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