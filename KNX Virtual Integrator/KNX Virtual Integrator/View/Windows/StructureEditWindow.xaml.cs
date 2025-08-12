using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KNX_Virtual_Integrator.Model.Entities;
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

    /// <summary>
    /// Handles the button click event to add a DPT to send to a tested element
    /// </summary>
    private void AddDptCmdToElementButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        _viewModel.AddDptCmdToElementCommand.Execute(_viewModel.SelectedStructureModel?.ElementList[indexElement]);
    }
    
    /// <summary>
    /// Handles the button click event to add a DPT to send to a tested element
    /// </summary>
    private void AddDptIeToElementButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        _viewModel.AddDptIeToElementCommand.Execute(_viewModel.SelectedStructureModel?.ElementList[indexElement]);
    }
    
    private void RemoveCmdDptFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        var button = (Button)sender;
        // Find the DPT's index
        var listBoxItem = FindParent<ListBoxItem>(button); // On remonte jusqu’au ListBoxItem parent
        var testsIeListBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem); // Le ListBox parent est celui lié à TestsIe
        var testsIeItem = (DataPointType)listBoxItem.DataContext; // Élément TestsIe parent
        int indexCmd = testsIeListBox.Items.IndexOf(testsIeItem); // Index de cet élément dans TestsIe
        
        _viewModel.RemoveCmdDptFromElementCommand.Execute((_viewModel.SelectedStructureModel?.ElementList[indexElement], indexCmd));
    }
    
    private void RemoveIeDptFromElementButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        var button = (Button)sender;
        // Find the DPT's index
        var listBoxItem = FindParent<ListBoxItem>(button); // On remonte jusqu’au ListBoxItem parent
        var testsIeListBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem); // Le ListBox parent est celui lié à TestsIe
        var testsIeItem = (DataPointType)listBoxItem.DataContext; // Élément TestsIe parent
        int indexIe = testsIeListBox.Items.IndexOf(testsIeItem); // Index de cet élément dans TestsIe
        
        _viewModel.RemoveIeDptFromElementCommand.Execute((_viewModel.SelectedStructureModel?.ElementList[indexElement], indexIe));
    }

    private void AddDptToDictionaryButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddDptToDictionaryCommand.Execute(_viewModel.SelectedStructure);
    }

    private void RemoveDptFromDictionaryButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the DPT's index
        var button = sender as Button; // Récupère le bouton
        var kvp = button?.DataContext as KeyValuePair<int,FunctionalModelStructure.DptAndKeywords>? ; // l'élément du dictionnaire
        //var listBox = FindParent<ListBox>(button); // Récupère le ListBox parent
        //int indexDpt = listBox.ItemContainerGenerator.IndexFromContainer(listBox.ItemContainerGenerator.ContainerFromItem(kvp)); // Récupère l'index
        int key = 0;
        if (kvp?.Value != null) key = kvp.Value.Key;
        _viewModel.RemoveDptFromDictionaryCommand.Execute((key,_viewModel.SelectedStructure));
    }
    
    
    
    
    // Méthode récursive qui remonte les parents d'un objet jusqu'à atteindre le parent du type passé en paramètre
    // Utilisée dans RemoveTestedElementFromStructureButtonClick
    // Utilisée dans AddTestToElementButtonClick
    // Utilisée dans RemoveTestFromElementButtonClick
    // etc
    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
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