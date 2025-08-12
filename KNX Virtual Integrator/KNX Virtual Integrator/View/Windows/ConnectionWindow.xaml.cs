using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using KNX_Virtual_Integrator.ViewModel;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator.View.Windows;

public partial class ConnectionWindow
{
    private readonly MainViewModel _viewModel;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionWindow"/> class,
    /// loading and applying settings from the appSettings file, and setting default values where necessary.
    /// </summary>
    public ConnectionWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
        
        UpdateWindowContents(true, true, true);
        
    }

    /// <summary>
    /// Handles the Connection window closing event by canceling the closure, restoring previous settings, and hiding the window.
    /// </summary>
    private void ClosingConnectionWindow(object? sender, CancelEventArgs e)
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
        if (_viewModel.AppSettings.AppLang == "FR")
        {
            Resources["ConnectionWindowTitle"]="Connexion au Bus KNX";
            Resources["ConnectionType"]="Type de connexion :";
            Resources["AvailableInterfaces"]="Interfaces disponibles :";
            Resources["Refresh"]="Rafraîchir";
            Resources["NatConnection"]="Connexion à distance (NAT)";
            Resources["PublicIp"]="IP publique du routeur";
            Resources["ToFillWhenSecure"]="À ne remplir que pour une connexion sécurisée";
            Resources["IPSecureConnection"]="Connexion via IP Secure";
            Resources["KnxKeysFile"]="Fichier de clés (.knxkeys)";
            Resources["ImportKeys"]="importer clés";
            Resources["PwdKeys"]="Mot de passe du fichier de clés";
            Resources["ConnectionError"]="Erreur lors de la connexion";
            Resources["CurrentInterface"]="Interface actuellement connectée :";
            Resources["ConnectButtonText"]="Connexion";
            Resources["DisconnectButtonText"]="Déconnexion";
        }
        else
        {
            Resources["ConnectionWindowTitle"]="KNX Bus Connection";
            Resources["ConnectionType"]="Connection type :";
            Resources["AvailableInterfaces"]="Available interfaces :";
            Resources["Refresh"]="Refresh";
            Resources["NatConnection"]="Connection (NAT)";
            Resources["PublicIp"]="Router public IP";
            Resources["ToFillWhenSecure"]="To fill only when connection is IP Secure";
            Resources["IPSecureConnection"]="IP Secure connection";
            Resources["KnxKeysFile"]="Keys file .knxkeys";
            Resources["ImportKeys"]="Import keys";
            Resources["PwdKeys"]="Keys file password";
            Resources["ConnectionError"]="Connection error";
            Resources["CurrentInterface"]="Currently connected interface :";
            Resources["ConnectButtonText"]="Connect";
            Resources["DisconnectButtonText"]="Disconnect";
            
        }
    }

    private void ApplyThemeToWindow()
    {
        Brush textColorBrush;
        Brush backgroundColorBrush;
        
        if (_viewModel.AppSettings.EnableLightTheme)
        {
            textColorBrush = (Brush)FindResource("LightForegroundBrush");
            backgroundColorBrush = (Brush)FindResource("OffWhiteBackgroundBrush");
            NatAddressTextBox.Style = (Style)FindResource("TextBoxLight");
            ActualPwdKeysFileTextBox.Style = (Style)FindResource("TextBoxLight");
            ImportKnxKeys.Style = (Style)FindResource("ImportKeysButtonLight");
            ConnectionTypeComboBox.Style = (Style)FindResource("LightComboBoxStyle");
            InterfaceListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerLight");
            InterfaceListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemLight");
            InterfaceListBox.Style = (Style)FindResource("StandardListBoxLight");
            CurrCoInterfaceText.Style = (Style)FindResource("InterfaceTextBlockLight");
        }
        else
        {
            textColorBrush = (Brush)FindResource("DarkOffWhiteForegroundBrush");
            backgroundColorBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            NatAddressTextBox.Style = (Style)FindResource("TextBoxDark");
            ActualPwdKeysFileTextBox.Style = (Style)FindResource("TextBoxDark");
            ImportKnxKeys.Style = (Style)FindResource("ImportKeysButtonDark");   
            ConnectionTypeComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
            InterfaceListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerDark");
            InterfaceListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemDark");
            InterfaceListBox.Style = (Style)FindResource("StandardListBoxDark");
            CurrCoInterfaceText.Style = (Style)FindResource("InterfaceTextBlockDark");
        }
        
        Background = backgroundColorBrush;
        
        ConnectionTypeText.Foreground = textColorBrush;
        DiscoveredInterfacesText.Foreground = textColorBrush;
        
        IpRouterText.Foreground = textColorBrush;
        SecurisedConectionText.Foreground = textColorBrush;
        ConnectionIpSecText.Foreground = textColorBrush;
        ImportedKeysFileText.Foreground = textColorBrush;
        ImportKeyPathText.Foreground = textColorBrush;
        PwdKeysFileText.Foreground = textColorBrush;
        
        CurrentInterfaceText.Foreground = textColorBrush;
        FileKeysText.Foreground = textColorBrush;
        NatConnectionText.Foreground = textColorBrush;
        LockIconText.Foreground = textColorBrush;
        CurrCoInterfaceText.Tag = textColorBrush;
        NatIconText.Foreground = textColorBrush;
        
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
        ConnectionWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
        Height = 605 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 650 * scale;
        Width = 300 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 300 * scale;
    }
    
    
    /// <summary>
    /// Handles the button click event to import a keys file.
    /// Displays an OpenFileDialog for the user to select the file,
    /// extracts necessary files.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ImportKeysFileButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Waiting for user to select a keys file");

        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        OpenFileDialog openFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = "Sélectionnez un fichier de clés à importer",
            // Applique un filtre pour n'afficher que les fichiers knxkeys ou tous les fichiers
            Filter = "Fichiers de clés|*.knxkeys",
            // Définit l'index par défaut du filtre (fichiers XML d'adresses de groupes)
            FilterIndex = 1,
            // N'autorise pas la sélection de plusieurs fichiers à la fois
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"File selected: {openFileDialog.FileName}");

            // Donner le chemin au Model
            _viewModel.BusConnection.KeysPath = openFileDialog.FileName;
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the file selection operation");
        }

    }

}