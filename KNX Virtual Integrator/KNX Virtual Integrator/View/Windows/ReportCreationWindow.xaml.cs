using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KNX_Virtual_Integrator.ViewModel;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator.View.Windows;

/// <summary>
/// Represents the window for creating reports.
/// </summary>
public partial class ReportCreationWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    //Permet à la fenêtre à accéder aux services du ViewModel
    /// <summary>
    /// MainViewModel instance to allow communication with the backend
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- MÉTHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportCreationWindow"/> class.
    /// </summary>
    /// <param name="mv">The view model associated with the window.</param>
    public ReportCreationWindow(MainViewModel mv)
    {
        InitializeComponent();
        _mainViewModel = mv;
        DataContext = _mainViewModel;
        MyBrowser.RenderSize = new Size(400, 600);
        UpdateWindowContents(true, true, true);
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

    /// <summary>
    /// Updates the text contents of the report window (only French and English).
    /// </summary>
    private void TranslateWindowContents()
    {
        if (_mainViewModel.AppSettings.AppLang == "FR")
        {
            Resources["ReportWindowTitle"] = "Création du rapport de test";
            Resources["TestGenerationTitle"] = "Génération du rapport de test";
            Resources["DocParametersTitle"] = "Paramètres du document";
            Resources["AuthorReport"] = "Auteur du rapport :";
            Resources["ReportPath"] = "Sauvegardez le rapport :";
            Resources["ReportFullPath"] = "Sauvegardez le rapport";
            Resources["ReportPreview"] = "Prévisualisation du rapport (peut causer des erreurs):";
            Resources["GenerationButton"] = "Prévisualiser le rapport";
            Resources["CancelButton"] = "Réinitialiser paramètres";
            Resources["ReportGenerationSuccessMessage"] = "Rapport de test correctement généré.";
            Resources["ReportCreationTooltipTitle"] = "Aide - Rapport de Test";
            Resources["ReportCreationTooltipMessage"] =
                "Création du rapport de test.\r\n" +
                "Le nom de l'auteur est optionnel.\r\n" +
                "Ensuite, appuyez sur le bouton Sauvegardez le rapport de test pour qu'il soit exporté en PDF.\r\n" +
                "Une fenêtre de dialogue s'ouvre, vous permettant de définir le nom et la localisation du rapport\r\n." +
                "À la fermeture de la fenêtre de dialogue, le rapport est généré à la localisation voulue.\r\n" +
                "La prévisualisation du rapport peut présenter des problèmes, selon l'éditeur de PDF qui est installé sur l'ordinateur utilisé.";
        }
        else
        {
            Resources["ReportWindowTitle"] = "Test report creation";
            Resources["TestGenerationTitle"] = "Test report generation";
            Resources["DocParametersTitle"] = "Document parameters";
            Resources["AuthorReport"] = "Report author :";
            Resources["ReportPath"] = "Save the report :";
            Resources["ReportFullPath"] = "Save the report";
            Resources["ReportPreview"] = "Report preview (can cause errors):";
            Resources["GenerationButton"] = "Preview the report";
            Resources["CancelButton"] = "Reset parameters";
            Resources["ReportGenerationSuccessMessage"] = "Test report successfully generated.";
            Resources["ReportCreationTooltipTitle"] = "Help - Test Report";
            Resources["ReportCreationTooltipMessage"] =
                "Creating the test report.\r\n" +
                "The author's name is optional.\r\n" +
                "Then, press the save the report button to export the test report as a PDF.\r\n" +
                "A dialog window will open, allowing you to define the name and location of the report.\r\n" +
                "When you close the dialog window, the report will be generated in the desired location.\r\n" +
                "The report preview may have issues, depending on the PDF editor installed on the computer being used.";
        }
    }
    
    /// <summary>
    /// Updates the color theme of the window according to <see cref="Model.Interfaces.IApplicationSettings.EnableLightTheme"/> state.
    /// </summary>
    private void ApplyThemeToWindow()
    {
        Style titleStyles;
        Style textBlockStyles;
        Brush backgrounds;
        Brush tooltipBackgroundBrush;
        Style tooltipTextBlockStyle;
        
        if (_mainViewModel.AppSettings.EnableLightTheme)
        {
            titleStyles = (Style)FindResource("TitleTextLight");
            textBlockStyles = (Style)FindResource("StandardTextBlockLight");
            
            backgrounds = (Brush)FindResource("OffWhiteBackgroundBrush");
            PdfImageReport.Source = (DrawingImage)FindResource("PdfCreationImageLight");
            AuthorNameTextBox.Style = (Style)FindResource("StandardTextBoxLight");
            SetPdfPathButton.Style = (Style)FindResource("ImportKeysButtonLight");
            SaveButton.Style = (Style)FindResource("BottomButtonLight");
            CancelButton.Style = (Style)FindResource("BottomButtonLight");
            GenerateRepImage.Source = (DrawingImage)FindResource("CheckmarkLight");
            CancelRepImage.Source = (DrawingImage)FindResource("CrossmarkLight");
            ReportWindowFooter.Background = (Brush)FindResource("WhiteBackgroundBrush");
            Background = (Brush)FindResource("WhiteBackgroundBrush");
            tooltipBackgroundBrush = (Brush)FindResource("WhiteBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockLight");
        }
        else
        {
            titleStyles = (Style)FindResource("TitleTextDark");
            textBlockStyles = (Style)FindResource("StandardTextBlockDark");
            
            backgrounds = (Brush)FindResource("DarkGrayBackgroundBrush");
            
            PdfImageReport.Source = (DrawingImage)FindResource("PdfCreationImageDark");
            AuthorNameTextBox.Style = (Style)FindResource("StandardTextBoxDark");
            SetPdfPathButton.Style = (Style)FindResource("ImportKeysButtonDark");
            SaveButton.Style = (Style)FindResource("BottomButtonDark");
            CancelButton.Style = (Style)FindResource("BottomButtonDark");
            GenerateRepImage.Source = (DrawingImage)FindResource("CheckmarkDark");
            CancelRepImage.Source = (DrawingImage)FindResource("CrossmarkDark");
            ReportWindowFooter.Background = (Brush)FindResource("DarkGrayBackgroundBrush");
            Background = (Brush)FindResource("DarkGrayBackgroundBrush");
            tooltipBackgroundBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockDark");
        }
        
        ReportWindowTopTitle.Style = titleStyles;
        DocParamTextTitle.Style = titleStyles;
        ReportAuthorText.Style = textBlockStyles;
        ReportPathTextBlock.Style = textBlockStyles;
        PdfPathText.Style = textBlockStyles;
        ReportPreviewText.Style = textBlockStyles;
        SaveButtonText.Style = textBlockStyles;
        CancelButtonText.Style = textBlockStyles;

        ReportCreationWindowBorder.Background = backgrounds;

        Resources["CurrentTooltipBackgroundBrush"] = tooltipBackgroundBrush;
        Resources["CurrentTooltipTextBlockStyle"] = tooltipTextBlockStyle;
        //Background = backgrounds;
    }
    
    /// <summary>
    /// Update the size of the window and its contents according to <see cref="Model.Interfaces.IApplicationSettings.AppScaleFactor"/>
    /// </summary>
    private void ApplyScaling()
    {
        var scaleFactor = _mainViewModel.AppSettings.AppScaleFactor / 100f;
        float scale;
        if (scaleFactor <= 1f)
        {
            scale = scaleFactor - 0.1f;
        }
        else
        {
            scale = scaleFactor - 0.2f;
        }
        ReportCreationWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 500 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 500 * scale;
        Width = 515 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 515 * scale;
    }
    
    

    /// <summary>
    /// Handles the MouseLeftButtonDown event to initiate dragging of the window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        DragMove();
    }

    /// <summary>
    /// Handles the click event of the save button to generate a report.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _mainViewModel.AuthorName = AuthorNameTextBox.Text;
            Uri uri = new Uri(_mainViewModel.PdfPath, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                _mainViewModel.ConsoleAndLogWriteLineCommand.Execute("The pdf URI Address has to be absolute");
                return;
            }
            
            //Because using the same can cause issues, we return
            if (uri.Equals(MyBrowser.Source))
            {
                _mainViewModel.ConsoleAndLogWriteLineCommand.Execute(
                    "The pdf URI Address has to be changed before viewing the file");
                return;
            }
            
            if (_mainViewModel.PdfPath.Length <= 0)
                return;
            // Disabled due to issues
            MyBrowser.Navigate(uri);
            MyBrowser.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.HelpLink + "       "  + ex.Message + "      " + ex.Data);
        }
    }

    /// <summary>
    /// Handles the click event of the cancel button. It only clears the text
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        AuthorNameTextBox.Text = string.Empty;
        _mainViewModel.PdfPath = "";
        MyBrowser.Visibility = Visibility.Hidden;
        MyBrowser.Source = null;
    }

    /// <summary>
    /// Handles the Connection window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingReportCreationWindow(object? sender, CancelEventArgs e)
    {
        AuthorNameTextBox.Text = string.Empty;
        _mainViewModel.PdfPath = "";
        e.Cancel = true;
        Hide();
        UpdateWindowContents(true, true, true);
        _mainViewModel.HideReportGenerationSuccessMessageCommand.Execute(null);
    }

    /// <summary>
    /// Handles the button click event to choose the file path to which the PDF report should be saved.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void SetPdfPathButton_OnClick(object sender, RoutedEventArgs e)
    {
        _mainViewModel.HideReportGenerationSuccessMessageCommand.Execute(null);
        MyBrowser.Visibility = Visibility.Hidden;
        MyBrowser.Source = null;
        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        SaveFileDialog saveFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = _mainViewModel.AppSettings.AppLang switch
            {
                "FR" => "Choisissez un nom pour le rapport",
                _ => "Choose a name for the report"
            },
            // Applique un filtre pour n'afficher que les fichiers pdf ou tous les fichiers
            Filter = _mainViewModel.AppSettings.AppLang switch
            {
                "FR" => "Fichiers pdf|*.pdf",
                _ => "PDF File|*.pdf"
            },
            // Définit l'index par défaut du filtre (fichiers XML d'adresses de groupes)
            FilterIndex = 1,
            FileName = $"KNXVI-Analysis_Report-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf",
            DefaultExt = ".pdf"
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _mainViewModel.ConsoleAndLogWriteLineCommand.Execute($"Saving the pdf to : {saveFileDialog.FileName}");
            
            // Supprimer le fichier s'il existe déjà
            if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);
            
            // Donner le chemin au Model
            _mainViewModel.AuthorName = AuthorNameTextBox.Text;
            _mainViewModel.PdfPath = saveFileDialog.FileName;
            PdfPathText.Text = saveFileDialog.FileName;
            try
            {
                _mainViewModel.GenerateReportCommand.Execute((_mainViewModel.PdfPath, _mainViewModel.AuthorName,
                    _mainViewModel.SelectedTestModels, _mainViewModel.LastTestResults));
                _mainViewModel.ShowReportGenerationSuccessMessageCommand.Execute(null);
            }
            catch (Exception ex)
            {
                _mainViewModel.ConsoleAndLogWriteLineCommand.Execute(ex.Message + " - " +  ex.Source + " - " + ex.HelpLink + " - " + ex.Data);
                MessageBox.Show(ex.Message + " - " +  ex.Source + " - " + ex.HelpLink + " - " + ex.Data);
            }
        }
        else
        {
            _mainViewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }
    }
}