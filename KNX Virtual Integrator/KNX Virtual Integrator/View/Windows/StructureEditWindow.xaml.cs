using System.Collections.ObjectModel;
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
    
    /// <summary>
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the report window according to the application settings.
    /// </summary>
    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (langChanged) TranslateWindowContents();
        if (themeChanged) ApplyThemeToWindow();
        if (scaleChanged) ApplyScaling();
    }

    private void TranslateWindowContents()
    {
        if (_viewModel.AppSettings.AppLang == "FR")
        {
            Resources["StructEditWindowTitle"]="Fenêtre d'édition de Structure de Modèle Fonctionnel";
            Resources["StructRmvText"]="Supprimer la structure";
            Resources["FunctionalModelStructure"]="Structure du Modèle Fonctionnel";
            Resources["DptPersonalizationTitle"]="Personnalisation de DPTs";
            Resources["TestedElement"]="Élément à tester";
            Resources["DptType"] = "Type de DPT :";
            Resources["Dispatch(es)"] = "Envoi(s)";
            Resources["Reception(s)"] = "Réception(s)";
            Resources["AddTestedElement"]="Ajouter un Élément à tester";
            Resources["Key"]="Clé";
            Resources["Name"]="Nom :";
            Resources["Type"]="Type :";
            Resources["Keywords"]="Mots-clés";
            Resources["DptNumber"]="Dpt de numéro :";
            Resources["AddDpt"]="Ajouter un DataPointType";
            Resources["ApplyChangesErrorMessage"] = "Vérifiez votre Structure. Il ne peut y avoir d'Élément à Tester avec des DPTs non assignés. Il faut au moins 1 Élément à Tester.";
            Resources["ApplyChanges"]="Appliquer les changements";
            Resources["UndoChanges"]="Annuler les changements";

            Resources["StructureKeywordsTooltipTitle"] = "Aide - Mots-clés";
            Resources["StructureKeywordsTooltipMessage"] =
                "Rentrer des mots-clés pour faciliter l'association des participants à la Structure.\r\n" +
                "Rentrer les mots-clés à la suite, en les séparant par une virgule (,) sans espace entre les mots-clés.\r\n" +
                "Le mot-clé peut être n'importe quel mot ou groupe de mot permettant de reconnaître le participant.\r\n" +
                "exemple : Lumiere on/off,Lumiere_on/off,Lumiere on/off,Lumiere on-off,Eclairage_Simple,Eclairage_Simple,Eclairage Simple";
            Resources["DptPersonalizationTooltipTitle"] = "Aide - Personnalisation de DPTs";
            Resources["DptPersonalizationTooltipMessage"] =
                "Créer des DPTs personnalisés.\r\n" +
                "Les DPTs crées dans cette section seront utilisables dans les éléments à tester de la Structure du Modèle Fonctionnel.\r\n" +
                "Pour chaque DPT, choisir le nom, le type de DPT et ajouter des mots-clés pour faciliter l'assignation des adresses de groupe aux DPTs.";
            Resources["DptKeywordsTooltipTitle"] = "Aide - Mots-clés";
            Resources["DptKeywordsTooltipMessage"] =
                "Rentrer des mots-clés pour faciliter la reconnaissance des adresses de groupe de votre projet.\r\n" +
                "Rentrer les mots-clés à la suite, en les séparant par une virgule (,) sans espace entre les mots-clés.\r\n" +
                "Le mot-clé devrait être ce par quoi commence le nom de votre adresse de groupe.\r\n" +
                "exemple : Cmd_Eclairage_OnOff_MaisonDupre_RezDeChaussee_Etage_Salon. Rentrer dans les mots clés Cmd_Eclairage_OnOff.";
            Resources["FunctionalModelStructureTooltipTitle"] = "Aide - Structure du Modèle Fonctionnel";
            Resources["FunctionalModelStructureTooltipMessage"] =
                "Créer ou modifier une structure de Modèle Fonctionnel.\r\n" +
                "Les Modèles Fonctionnels déjà créés ne seront pas modifiés tant que le bouton Valider n'est pas pressé.\r\n" +
                "Un élément à tester contient 1 ou + DPTs à envoyer, et contient 0 ou + DPTs attendus en réception.";
            Resources["ValuesTooltipTitle"] = "Aide - Valeurs à envoyer et Valeurs attendues en réception";
            Resources["ValuesTooltipMessage"] =
                "Attention. Les valeurs sont à rentrer en décimal.\r\n" +
                "Une valeur rentrée en hexadécimal pourrait être mal comprise et compromettre la validité du test.\r\n" +
                "exemple : Si vous voulez écrire en hexadécimal (hex)4F, écrivez en décimal (dec)79";
            Resources["UndoChangesTooltipTitle"] = "Aide - Annuler les changements";
            Resources["UndoChangesTooltipMessage"] =
                "Annuler les changements faits à la Structure de Modèle Fonctionnel.\r\n" +
                "La Structure sera restaurée à la version qui existait lors de la dernière ouverture de cette fenêtre.";
            Resources["ApplyChangesTooltipTitle"] = "Aide - Valider les changements";
            Resources["ApplyChangesTooltipMessage"] =
                "Valider les changements faits à la Structure de Modèle Fonctionnel.\r\n" +
                "La liste de Modèles Fonctionnels associés à cette Structure sera supprimée.\r\n" +
                "Un nouveau Modèle Fonctionnel correspondant à la nouvelle Structure sera créé.";
            
        }
        else
        {
            Resources["StructEditWindowTitle"]="Edition of the Structures";
            Resources["StructRmvText"]="Remove the structure";
            Resources["FunctionalModelStructure"]="Your Functional Model Structure";
            Resources["TestedElement"]="Tested Element";
            Resources["DptType"] = "DPT Type:";
            Resources["Dispatch(es)"] = "Dispatch(es)";
            Resources["Reception(s)"] = "Reception(s)";
            Resources["DptPersonalizationTitle"]="DPT Customization";
            Resources["AddTestedElement"]="Add a Tested Element";
            Resources["Key"]="Key";
            Resources["Name"]="Name:";
            Resources["Type"]="Type:";
            Resources["Keywords"]="Keywords";
            Resources["DptNumber"]="Dpt number:";
            Resources["AddDpt"]="Add a DataPointType";
            Resources["ApplyChangesErrorMessage"] = "Check your Structure. There can not be any Tested Element with unassigned DPTs. There has to be at least one Tested Element.";
            Resources["ApplyChanges"]="Apply changes";
            Resources["UndoChanges"]="Undo changes";
            
            Resources["StructureKeywordsTooltipTitle"] = "Help - Keywords";
            Resources["StructureKeywordsTooltipMessage"] =
                "Enter keywords to facilitate the association of participants with the Structure.\r\n" +
                "Enter keywords one after the other, separating them with a comma (,) without spaces between keywords.\r\n" +
                "The keyword can be any word or group of words that identifies the participant.\r\n" +
                "example: Light on/off,Light_on/off,Light on/off,Light on-off,Simple_Lighting,Simple_Lighting,Simple Lighting";
            Resources["DptPersonalizationTooltipTitle"] = "Help - Customising DPTs";
            Resources["DptPersonalizationTooltipMessage"] =
                "Create custom DPTs.\r\n" +
                "The DPTs created in this section will be usable in the Tested Elements in the Functional Model Structure.\r\n" +
                "For each DPT, choose the name, DPT type and add keywords to facilitate the assignment of group addresses to DPTs.";
            Resources["DptKeywordsTooltipTitle"] = "Help - Keywords";
            Resources["DptKeywordsTooltipMessage"] =
                "Enter keywords to facilitate recognition of your project's group addresses.\r\n" +
                "Enter keywords in sequence, separating them with a comma (,) without spaces between keywords.\r\n" +
                "The keyword should be the first part of your group address name.\r\n" +
                "example: Cmd_Lighting_OnOff_DupreHouse_GroundFloor_FirstFloor_LivingRoom. Enter the keywords Cmd_Lighting_OnOff.";
            Resources["FunctionalModelStructureTooltipTitle"] = "Help - Functional Model Structure";
            Resources["FunctionalModelStructureTooltipMessage"] =
                "Create or modify a Functional Model structure.\r\n" +
                "Functional Models that have already been created will not be modified until the Validate button is pressed.\r\n" +
                "An Tested Element contains 1 or more DPTs to be sent, and contains 0 or more DPTs expected to be received.";
            Resources["ValuesTooltipTitle"] = "Help - Values to be sent and expected values upon receipt";
            Resources["ValuesTooltipMessage"] = 
                "Caution. Values must be entered in decimal format.\r\n" +
                "A value entered in hexadecimal format may be misinterpreted and compromise the validity of the test.\r\n" +
                "Example: If you want to write hexadecimal (hex)4F, write decimal (dec)79";
            Resources["UndoChangesTooltipTitle"] = "Help - Undo Changes";
            Resources["UndoChangesTooltipMessage"] =
                "Undo changes made to the Functional Model Structure.\r\n" +
                "The Structure will be restored to the version that existed when this window was last opened. ";
            Resources["ApplyChangesTooltipTitle"] = "Help - Apply changes";
            Resources["ApplyChangesTooltipMessage"] =
                "Apply changes made to the Functional Model Structure.\r\n" +
                "The list of Functional Models associated with this Structure will be deleted.\r\n" +
                "A new Functional Model corresponding to the new Structure will be created.";
        }
    }


    private void ApplyThemeToWindow()
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("StructureEditWindow.ApplyThemeToWindow is not implemented");

        Style textBoxStyle;
        Style deleteStructureButtonStyle;
        Style borderStyle;
        Style titleTextStyle;
        Style comboBoxStyle;
        Style dptListBoxItemContainerStyle;
        Brush textBlockForegroundBrush;
        Brush elementContainersBackgroundBrush;
        Style elementContainersStyle;
        Brush tooltipBackgroundBrush;
        Style tooltipTextBlockStyle;
        
        if (_viewModel.AppSettings.EnableLightTheme)
        {
            PencilImage.Source = (DrawingImage)FindResource("PencilDrawingImage");
            TopBannerBorder.Style = (Style)FindResource("BorderTitleLight");
            textBoxStyle = (Style)FindResource("StructureEditWindowStandardTextBoxLight");
            textBlockForegroundBrush = (Brush)FindResource("LightForegroundBrush");
            deleteStructureButtonStyle = (Style)FindResource("DeleteStructureButtonStyleLight");
            borderStyle = (Style)FindResource("BorderLight");
            titleTextStyle = (Style)FindResource("TitleTextLight");
            comboBoxStyle = (Style)FindResource("StructureEditWindowLightComboBoxStyle");
            elementContainersBackgroundBrush = (Brush)FindResource("OffWhiteBackgroundBrush");
            elementContainersStyle = (Style)FindResource("TestedElementItemContainerStyleLight");
            dptListBoxItemContainerStyle = (Style)FindResource("DptListBoxItemContainerStyleLight");
            Resources["CurrentRemoveTestButtonStyle"] = (Style)FindResource("RemoveTestButtonStyleLight");
            Resources["CurrentAddTestButtonStyle"] = (Style)FindResource("AddTestButtonStyleLight");
            Resources["CurrentDPTAndElementBackgroundBrush"] = (Brush)FindResource("WhiteBackgroundBrush");
            Resources["CurrentDPTBorderBorderBrush"] = (Brush)FindResource("DarkGrayBorderBrush");
            Resources["CurrentDPTTypeListBoxBackground"] = (Brush)FindResource("OffWhiteBackgroundBrush");
            Resources["CurrentElementBorderBorderBrush"] = (Brush)FindResource("DarkGrayBorderBrush");
            tooltipBackgroundBrush = (Brush)FindResource("WhiteBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockLight");
        }
        else
        { 
            PencilImage.Source = (DrawingImage)FindResource("WhitePencilDrawingImage");
            TopBannerBorder.Style = (Style)FindResource("BorderTitleDark");
            textBoxStyle = (Style)FindResource("StandardTextBoxDark");
            textBlockForegroundBrush = (Brush)FindResource("DarkForegroundBrush");
            deleteStructureButtonStyle = (Style)FindResource("DeleteStructureButtonStyleDark");
            borderStyle = (Style)FindResource("BorderDark");
            titleTextStyle = (Style)FindResource("TitleTextDark");
            comboBoxStyle = (Style)FindResource("DarkComboBoxStyle");
            elementContainersBackgroundBrush = (Brush)FindResource("DarkerGrayBackgroundBrush");
            elementContainersStyle = (Style)FindResource("TestedElementItemContainerStyleDark");    
            dptListBoxItemContainerStyle = (Style)FindResource("DptListBoxItemContainerStyleLight");
            Resources["CurrentRemoveTestButtonStyle"] = (Style)FindResource("RemoveTestButtonStyleDark");
            Resources["CurrentAddTestButtonStyle"] = (Style)FindResource("AddTestButtonStyleDark");
            Resources["CurrentDPTAndElementBackgroundBrush"] = (Brush)FindResource("DarkGrayBackgroundBrush");
            Resources["CurrentDPTBorderBorderBrush"] = (Brush)FindResource("GrayBorderBrush");
            Resources["CurrentDPTTypeListBoxBackground"] = (Brush)FindResource("DarkGrayBackgroundBrush");
            Resources["CurrentElementBorderBorderBrush"] = (Brush)FindResource("GrayBorderBrush");
            tooltipBackgroundBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockDark");
        }

        StructureKeywordsTextBox.Style = textBoxStyle;
        StructureNameTextBox.Style = textBoxStyle;
        Resources["CurrentDPTNameTextBoxStyle"] = textBoxStyle;
        Resources["CurrentDPTKeywordsTextBoxStyle"] = textBoxStyle;
            
        StructureSuppressionButton.Style = deleteStructureButtonStyle;
        Resources["CurrentTESuppressionButtonStyle"] = deleteStructureButtonStyle;
        Resources["CurrentDPTSuppressionButtonStyle"] = deleteStructureButtonStyle;

        StructEditWindowBorder.Style = borderStyle;
        MainContainerBorder.Style = borderStyle;
        DPTsBannerBorder.Style = borderStyle;
        TEsBorder.Style = borderStyle;
        BottomBannerBorder.Style = borderStyle;

        DPTsBannerTitleTextBlock.Style = titleTextStyle;
        TEsSectionTitleTextBlock.Style = titleTextStyle;
        Resources["CurrentTitleTextStyle"] = titleTextStyle;

        Resources["CurrentComboBoxStyle"] = comboBoxStyle;
        
        Resources["CurrentDispatchDPTTypeListBoxStyle"] = dptListBoxItemContainerStyle;
        Resources["CurrentDispatchValuesListBoxStyle"] = dptListBoxItemContainerStyle;
        Resources["CurrentReceptionDPTTypeListboxStyle"] = dptListBoxItemContainerStyle;
        Resources["CurrentReceptionValueListBoxStyle"] = dptListBoxItemContainerStyle;

        StructureKeywordsTextBlock.Foreground = textBlockForegroundBrush;
        TextBlock1.Foreground = textBlockForegroundBrush;
        TextBlock2.Foreground = textBlockForegroundBrush;
        Resources["CurrentTextBlockForeground"] = textBlockForegroundBrush;

        TestedElementsListBox.Background = elementContainersBackgroundBrush;
        DPTDictionaryListBox.Background = elementContainersBackgroundBrush;
        Resources["CurrentValueTextBoxBackgroundBrush"] = elementContainersBackgroundBrush;

        TestedElementsListBox.ItemContainerStyle = elementContainersStyle;
        DPTDictionaryListBox.ItemContainerStyle = elementContainersStyle;

        Resources["CurrentTooltipBackgroundBrush"] = tooltipBackgroundBrush;
        Resources["CurrentTooltipTextBlockStyle"] = tooltipTextBlockStyle;
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

    private void UndoChangesButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.UndoChangesSelectedStructureCommand.Execute(null);
    }
    
    private void ApplyChangesButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ApplyChangesSelectedStructureCommand.Execute(null);
    }
    
    private void PrintSelectedStructureButtonClick(object sender, RoutedEventArgs e)
    {
        PrintStructure(_viewModel.SelectedStructure);
    }
    
    private void PrintEditedStructureSaveButtonClick(object sender, RoutedEventArgs e)
    {
        PrintStructure(_viewModel.EditedStructureSave);
    }
    
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

    /// <summary>
    /// Handles the button click to reset to 0 a Cmd Value that has been deactivated because it was unknown 
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void ResetValueCmdStructureButtonClick(object sender, RoutedEventArgs e)
    {
        ResetValueStructure(sender, e, "TestsCmd");
    }
    
    /// <summary>
    /// Handles the button click to reset to 0 an Ie Value that has been deactivated because it was unknown 
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void ResetValueIeStructureButtonClick(object sender, RoutedEventArgs e)
    {
        ResetValueStructure(sender, e, "TestsIe");
    }
    
    /// <summary>
    /// Handles the reset to 0 a Value that has been deactivated because it was unknown 
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    /// <param name="tests">the type of Test value that is reset (TestCmd or TestIe).</param>
    private void ResetValueStructure(object sender, RoutedEventArgs e, string tests)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        if (dep is null) return;
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        if (dep is null) return;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep is null) return;
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        var button = (Button)sender;
        
        // Find the DPT's index
        var listBoxItem = FindParent<ListBoxItem>(button); // On remonte jusqu’au ListBoxItem parent
        var ieValuesListBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem); // Le ListBox parent est celui lié à TestsIe
        ObservableCollection<BigIntegerItem>? ieValuesItem = null;
        if (listBoxItem != null)
            ieValuesItem = (ObservableCollection<BigIntegerItem>)listBoxItem.DataContext; // Élément TestsIe parent
        int indexDpt = 0;
        if (ieValuesItem != null) 
            indexDpt = ieValuesListBox.Items.IndexOf(ieValuesItem); // Index de cet élément dans TestsIe
        
        // Find the Test's index
        var currentItem = button.DataContext; // L'élément lié à ce bouton (BigIntegerValue)
        var itemsControl = FindParent<ItemsControl>(button); // L'ItemsControl parent (lié à IntValue)
        int indexValue = 0;
        if (itemsControl != null)
            indexValue = itemsControl.Items.IndexOf(currentItem); // L'index dans la collection

        if (_viewModel.SelectedStructure != null)
        {
            // Effectively reset the value
            switch (tests)
            {
                case "TestsCmd":
                {
                    var bigIntegerItem = _viewModel.SelectedStructure.ModelStructure[indexElement].CmdValues[indexDpt][indexValue];
                    
                    if (bigIntegerItem.IsEnabled == false)
                        bigIntegerItem.IsEnabled = true;
                    else
                        bigIntegerItem.BigIntegerValue = 0;
                    break;
                }
                case "TestsIe":
                {
                    var bigIntegerItem = _viewModel.SelectedStructure.ModelStructure[indexElement].IeValues[indexDpt][indexValue];
                    
                    if (bigIntegerItem.IsEnabled == false)
                        bigIntegerItem.IsEnabled = true;
                    else
                        bigIntegerItem.BigIntegerValue = 0;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Handles the button click to deactivate a testIE value
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void DeactivateValueIeStructureButtonClick(object sender, RoutedEventArgs e)
    {
        DeactivateValueStructure(sender, e, "TestsIe");
    }

    /// <summary>
    /// Handles the button click to deactivate a test value
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    /// <param name="tests">the type of Test value that is deactivated (TestCmd or TestIe).</param>
    private void DeactivateValueStructure(object sender, RoutedEventArgs e, string tests)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        if (dep is null) return;
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        if (dep is null) return;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep is null) return;
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        var button = (Button)sender;
        
        // Find the DPT's index
        var listBoxItem = FindParent<ListBoxItem>(button); // On remonte jusqu’au ListBoxItem parent
        var ieValuesListBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem); // Le ListBox parent est celui lié à TestsIe
        ObservableCollection<BigIntegerItem>? ieValuesItem = null;
        if (listBoxItem != null)
            ieValuesItem = (ObservableCollection<BigIntegerItem>)listBoxItem.DataContext; // Élément TestsIe parent
        int indexDpt = 0;
        if (ieValuesItem != null) 
            indexDpt = ieValuesListBox.Items.IndexOf(ieValuesItem); // Index de cet élément dans TestsIe
        
        // Find the Test's index
        var currentItem = button.DataContext; // L'élément lié à ce bouton (BigIntegerValue)
        var itemsControl = FindParent<ItemsControl>(button); // L'ItemsControl parent (lié à IntValue)
        int indexValue = 0;
        if (itemsControl != null)
            indexValue = itemsControl.Items.IndexOf(currentItem); // L'index dans la collection

        // Effectively deactivate the value
        switch (tests)
        {
            case "TestsCmd":
            {
                if (_viewModel.SelectedStructure != null)
                    _viewModel.SelectedStructure.ModelStructure[indexElement].CmdValues[indexDpt][indexValue].IsEnabled = false;
                break;
            }
            case "TestsIe":
            {
                if (_viewModel.SelectedStructure != null)
                    _viewModel.SelectedStructure.ModelStructure[indexElement].IeValues[indexDpt][indexValue].IsEnabled = false;
                break;
            }
        }
    }
    
    /// <summary>
    /// Handles the button click event to add a Test to a Tested Element
    /// Adds a line of values to the Tested Element
    /// The number of fields added is equal to the number of DPTs in the Tested Element
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void AddTestToElementStructureButtonClick(object sender, RoutedEventArgs e)
    {
        // Code que je ne comprends pas qui sert à récupérer l'index de l'item depuis lequel le clic a été effectué
        // Dans ce cas, il s'agit de l'index du Tested Element qui est à supprimer
        // Pour utiliser ce segment de code, il faut avoir une référence sur la listbox
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep);
        
        if (dep is null) return;
        // Trouve l'élément qui possède le bouton qui a été cliqué et exécute l'ajout du test
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);
        
        _viewModel.AddTestToElementStructureCommand.Execute(_viewModel.SelectedStructure?.ModelStructure[indexElement]);
    }
    
    /// <summary>
    /// Handles the button click event to remove a Test from a Tested Element
    /// Deletes a full line of values to the Tested Element
    /// </summary>
    /// <param name="sender">The button that raised the event.</param>
    /// <param name="e">The click event data.</param>
    private void RemoveTestFromElementStructureButtonClick(object sender, RoutedEventArgs e)
    {
        // Find the Tested Element's index 
        var dep = (DependencyObject)e.OriginalSource;
        dep = FindParent<ListBoxItem>(dep); // reach the data point type
        if (dep is null) return;
        dep = VisualTreeHelper.GetParent(dep); // jump on parent higher
        if (dep is null) return;
        dep = FindParent<ListBoxItem>(dep); // reach the tested element
        if (dep is null) return;
        var indexElement = TestedElementsListBox.ItemContainerGenerator.IndexFromContainer(dep);

        // Find the Test's index
        var button = (Button)sender;
        var currentItem = button.DataContext; // L'élément lié à ce bouton (BigIntegerValue)
        var itemsControl = FindParent<ItemsControl>(button); // L'ItemsControl parent (lié à IntValue)
        var indexTest = 0;
        if (itemsControl != null)
            indexTest = itemsControl.Items.IndexOf(currentItem); // L'index dans la collection
        
        _viewModel.RemoveTestFromElementStructureCommand.Execute((_viewModel.SelectedStructure?.ModelStructure[indexElement], indexTest));
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
        if (item is KeyValuePair<int, DptAndKeywords> kvp) // Si c'est un KeyValuePair<int, FunctionalModelStructure.DptAndKeywords>
            _viewModel.RemoveDptFromDictionaryCommand.Execute((kvp.Key,_viewModel.SelectedStructure));
    }

    private void PrintStructure(FunctionalModelStructure? structure)
    {
        if (structure == null) return;
        Console.WriteLine("oooooooooooooooooooooooooo");
        Console.WriteLine("SelectedStructure : " + structure.FullName);
        Console.WriteLine("with allkeywords : " + structure.AllKeywords);
        foreach (var kw in structure.Keywords)
            Console.WriteLine("with keywords : " + kw);
        
        var dptDico = structure.DptDictionary;
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

        var modelStructure = structure.ModelStructure;
        foreach (var elementStructure in modelStructure)
        {
            Console.WriteLine(">> element structure");
            var cmd = elementStructure.Cmd;
            foreach (var item in cmd)
            {
                Console.WriteLine("MODEL Printing cmd " + item.Value + " : ");
                foreach (var value in elementStructure.CmdValues[cmd.IndexOf(item)])
                    Console.Write(value.BigIntegerValue + "/");
                Console.WriteLine();
            }
            var ie = elementStructure.Ie;
            foreach (var item in ie)
            {
                Console.WriteLine("MODEL Printing ie " + item.Value + " : ");
                foreach(var value in elementStructure.IeValues[cmd.IndexOf(item)])
                    Console.Write(value.BigIntegerValue + "/");
                Console.WriteLine();
            }
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