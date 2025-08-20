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
        DataContext = mainViewModel;
        UpdateWindowContents(true, true, true);
    }
    
    
    /// <summary>
    /// Handles the Model Edit window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingStructureEditWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        UpdateWindowContents(true, true, true);
        Hide();
    }    
    
    
    
    // TODO : implement later
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

    private void TranslateWindowContents()
    {
        if (_viewModel.AppSettings.AppLang == "FR")
        {
            Resources["StructEditWindowTitle"]="Fenêtre d'édition de Structure de Modèle Fonctionnel";
            Resources["StructRmvText"]="Supprimer la structure";
            Resources["TestedElementsListTitle"]="Liste d'Éléments à Tester";
            Resources["DptPersonalizationTitle"]="Personnalisation de DPTs";
            Resources["TestedElement"]="Élément à tester";
            Resources["DptType"] = "Type de DPT :";
            Resources["Dispatch(es)"] = "Envoi(s)";
            Resources["Reception(s)"] = "Réception(s)";
            Resources["AddTestedElement"]="Ajouter un Élément à tester";
            Resources["Key"]="Clé";
            Resources["Type"]="Type";
            Resources["Keywords"]="Mots-clés";
            Resources["AddDpt"]="Ajouter un DataPointType";
        }
        else
        {
            Resources["StructEditWindowTitle"]="Edition of the Structures";
            Resources["StructRmvText"]="Remove the structure";
            Resources["TestedElementsListTitle"]="List of Test Elements";
            Resources["TestedElement"]="Test Element";
            Resources["DptType"] = "DPT Type:";
            Resources["Dispatch(es)"] = "Dispatch(s)";
            Resources["Reception(s)"] = "Reception(s)";
            Resources["DptPersonalizationTitle"]="DPT Customization";
            Resources["AddTestedElement"]="Add a Test Element";
            Resources["Key"]="Key";
            Resources["Type"]="Type";
            Resources["Keywords"]="Keywords";
            Resources["AddDpt"]="Add a DataPointType";
        }
    }


    private void ApplyThemeToWindow()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("StructureEditWindow.ApplyThemeToWindow is not implemented");
    }
    private void ApplyScaling()
    {
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
        StructEditWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
        Width = 1500 * scale >= 0.9*SystemParameters.PrimaryScreenWidth? 0.9*SystemParameters.PrimaryScreenWidth : 1500 * scale;
        Height = 786 * scale >= 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 786 * scale;
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
    private void RemoveTestedElementFromModelStructureButtonClick(object sender, RoutedEventArgs e)
    {
        // Code que je ne comprends pas qui sert à récupérer l'index de l'item depuis lequel le clic a été effectué
        // Dans ce cas, il s'agit de l'index du Tested Element qui est à supprimer
        // Pour utiliser ce segment de code, il faut avoir une référence sur la listbox
        var dep = (DependencyObject)e.OriginalSource;
        while (dep != null && !(dep is ListBoxItem)) { dep = VisualTreeHelper.GetParent(dep); }
        if (dep == null) return;
        var index = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        _viewModel.RemoveTestedElementFromModelStructureCommand.Execute((_viewModel.SelectedStructure?.ModelStructure, index));
    }
    
    /// <summary>
    /// Handles the button click event to add a Tested Element to a Model Structure.
    /// </summary>
    private void AddTestedElementToModelStructureButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddTestedElementToModelStructureCommand.Execute(_viewModel.SelectedStructure?.ModelStructure);
        //scroll to end if possible
        var testedElementsScrollViewer = FindVisualChild<ScrollViewer>(TestedElementsListBox);
        testedElementsScrollViewer?.ScrollToEnd();
    }

    /// <summary>
    /// Handles the button click event to add a DPT to send to a tested element structure
    /// </summary>
    private void AddDptCmdToElementStructureButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep != null)
        {
            var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
            _viewModel.AddDptCmdToElementStructureCommand.Execute(_viewModel.SelectedStructure?.ModelStructure[indexElement]);
        }
    }
    
    /// <summary>
    /// Handles the button click event to add a DPT to send to a tested element structure
    /// </summary>
    private void AddDptIeToElementStructureButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep != null)
        {
            var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
            _viewModel.AddDptIeToElementStructureCommand.Execute(_viewModel.SelectedStructure?.ModelStructure[indexElement]);
        }
    }

    private void AddDptToDictionaryButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddDptToDictionaryCommand.Execute(_viewModel.SelectedStructure);
    }

    private void RemoveDptFromDictionaryButtonClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button; // Récupérer le bouton cliqué
        if (button == null) return;
        var item = button.DataContext; // Le DataContext est l'élément du dictionnaire (KeyValuePair<,>)
        if (item is KeyValuePair<int, FunctionalModelStructure.DptAndKeywords> kvp) // Si c'est un KeyValuePair<int, FunctionalModelStructure.DptAndKeywords>
            _viewModel.RemoveDptFromDictionaryCommand.Execute((kvp.Key,_viewModel.SelectedStructure));
    }


    private void PrintDptDictionaryButtonClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("SelectedStructure : " + _viewModel.SelectedStructure?.FullName);
        Console.WriteLine("with allkeywords : " + _viewModel.SelectedStructure?.AllKeywords);
        if (_viewModel.SelectedStructure?.Keywords != null)
            foreach (var kw in _viewModel.SelectedStructure.Keywords)
                Console.WriteLine("with keywords : " + kw);

        var dptDico = _viewModel.SelectedStructure?.DptDictionary;
        if (dptDico == null) return;
        foreach (var kvp in dptDico)
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine("DICO Printing key " + kvp.Key);
            Console.WriteLine("DICO Printing value.Dpt.Name " + kvp.Value.Dpt.Name);
            Console.WriteLine("DICO Printing value.Dpt.Type " + kvp.Value.Dpt.Type);
            Console.WriteLine("DICO Printing value.AllKeywords " + kvp.Value.AllKeywords);
            foreach (var kw in kvp.Value.Keywords)
                Console.WriteLine("DICO Printing value.Dpt.Keywords " + kw);
            Console.WriteLine("--------------------------");
        }
    }
    
    private void PrintModelStructureButtonClick(object sender, RoutedEventArgs e)
    {
        var modelStructure = _viewModel.SelectedStructure?.ModelStructure;
        if (modelStructure == null) return;
        Console.WriteLine(modelStructure.ToString());
        foreach (var elementStructure in modelStructure)
        {
            Console.WriteLine("one element structure");
            foreach(var cmd in elementStructure.Cmd)
                Console.WriteLine("--- one cmd : " + cmd);
            foreach (var ie in elementStructure.Ie)
                Console.WriteLine("--- one ie : " + ie);
        }
    }
    
    
    // Méthode récursive qui remonte les parents d'un objet jusqu'à atteindre le parent du type passé en paramètre
    // Utilisée dans RemoveTestedElementFromStructureButtonClick
    // Utilisée dans AddTestToElementButtonClick
    // Utilisée dans RemoveTestFromElementButtonClick
    // etc
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
    }
    
    
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