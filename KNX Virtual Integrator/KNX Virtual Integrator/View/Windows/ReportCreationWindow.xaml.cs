using System.ComponentModel;
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
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportCreationWindow"/> class.
    /// </summary>
    /// <param name="mv">The view model associated with the window.</param>
    public ReportCreationWindow(MainViewModel mv)
    {
        InitializeComponent();
        _mainViewModel = mv;
        MyBrowser.RenderSize = new Size(400, 600);
        UpdateWindowContents(true, true, true);
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
        if (_mainViewModel.AppSettings.AppLang == "FR")
        {
            Resources["ReportWindowTitle"] = "Création du rapport de test";
            Resources["TestGenerationTitle"] = "Génération du rapport de test";
            Resources["DocParametersTitle"] = "Paramètres du document";
            Resources["AuthorReport"] = "Auteur du rapport :";
            Resources["ReportPath"] = "Chemin du rapport :";
            Resources["ReportFullPath"] = "Emplacement du rapport";
            Resources["ReportPreview"] = "Prévisualisation du rapport :";
            Resources["GenerationButton"] = "Générer le rapport";
            Resources["CancelButton"] = "Annuler";
        }
        else
        {
            Resources["ReportWindowTitle"] = "Test report creation";
            Resources["TestGenerationTitle"] = "Test report generation";
            Resources["DocParametersTitle"] = "Document parameters";
            Resources["AuthorReport"] = "Report author :";
            Resources["ReportPath"] = "Report path :";
            Resources["ReportFullPath"] = "Report full path :";
            Resources["ReportPreview"] = "Report preview :";
            Resources["GenerationButton"] = "Generate report";
            Resources["CancelButton"] = "Cancel";
        }
    }

    private void ApplyThemeToWindow()
    {
        Style titleStyles;
        Style textBlockStyles;
        Brush backgrounds;
        
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
        //Background = backgrounds;
    }
    
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
            
        Height = 525 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 525 * scale;
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
        _mainViewModel.AuthorName = AuthorNameTextBox.Text;
        Uri uri = new Uri(_mainViewModel.PdfPath, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
        {
            _mainViewModel.ConsoleAndLogWriteLineCommand.Execute("The pdf URI Address has to be absolute");
            return;
        }
        if (uri.Equals(MyBrowser.Source))
        {
            _mainViewModel.ConsoleAndLogWriteLineCommand.Execute("The pdf URI Address has to be changed before modifying the file");
            return;
        }
        _mainViewModel.GenerateReportCommand.Execute((_mainViewModel.PdfPath, _mainViewModel.AuthorName,
            _mainViewModel.SelectedTestModels, _mainViewModel.LastTestResults));
        if (_mainViewModel.PdfPath.Length <= 0)
            return;
        MyBrowser.Navigate(uri);
        MyBrowser.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Handles the click event of the cancel button. This method is not yet implemented.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        AuthorNameTextBox.Text = string.Empty;
    }

    private void ClosingReportCreationWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        UpdateWindowContents(true, true, true);
    }

    private void SetPdfPathButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        OpenFileDialog openFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = "Choisissez un nom pour le rapport",
            // Applique un filtre pour n'afficher que les fichiers pdf ou tous les fichiers
            Filter = "Fichiers pdf|*.pdf",
            // Définit l'index par défaut du filtre (fichiers XML d'adresses de groupes)
            FilterIndex = 1,
            // N'autorise pas la sélection de plusieurs fichiers à la fois
            Multiselect = false,
            // N'indique pas de warning si le fichier n'existe pas
            CheckFileExists = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _mainViewModel.ConsoleAndLogWriteLineCommand.Execute($"File selected: {openFileDialog.FileName}");

            // Donner le chemin au Model
            _mainViewModel.PdfPath = openFileDialog.FileName;
            PdfPathText.Text = openFileDialog.FileName;
        }
        else
        {
            _mainViewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }
    }
}