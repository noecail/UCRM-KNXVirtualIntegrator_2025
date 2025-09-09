using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using KNX_Virtual_Integrator.ViewModel;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator.View.Windows;
/// <summary>
/// Class handling any change in the UI of the connection Window.
/// The Connection Window handles every part that concerns the connection to the KNX Bus, be it through IP, IP NAT or USB.
/// </summary>
public partial class ConnectionWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    //Permet à la fenêtre à accéder aux services du ViewModel
    /// <summary>
    /// MainViewModel instance to allow communication with the backend
    /// </summary>
    private readonly MainViewModel _viewModel;
    
    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- MÉTHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionWindow"/> class,
    /// loading and applying settings from the appSettings file, and setting default values where necessary.
    /// Allows the viewModel to be accessed from this window.
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
    /// Updates the contents (texts, textboxes, checkboxes, ...) of the connection window according to the application settings.
    /// </summary>
    public void UpdateWindowContents(bool langChanged = false, bool themeChanged = false, bool scaleChanged = false)
    {
        if (langChanged) TranslateWindowContents();
        if (themeChanged) ApplyThemeToWindow();
        if (scaleChanged) ApplyScaling();
    }
    
    /// <summary>
    /// Updates the text contents of the connection window (only French and English).
    /// </summary>
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
            Resources["IndivAddressInterface"]="Adresse Individuelle de l'interface";
            Resources["ToFillWhenSecure"]="À ne remplir que pour une connexion sécurisée";
            Resources["IPSecureConnection"]="Connexion via IP Secure";
            Resources["KnxKeysFile"]="Fichier de clés (.knxkeys)";
            Resources["ImportKeys"]="importer clés";
            Resources["ImportedKeysFile"] = "Fichier de clés importé :";
            Resources["PwdKeys"]="Mot de passe du fichier de clés";
            Resources["ConnectionError"]="Erreur lors de la connexion";
            Resources["CurrentInterface"]="Interface actuellement connectée :";
            Resources["ConnectButtonText"]="Connexion";
            Resources["DisconnectButtonText"]="Déconnexion";

            Resources["RemoteConnectionTooltipTitle"] = "Aide - Connexion à distance (NAT)";
            Resources["RemoteConnectionTooltipMessage"] =
                "Cette section permet de se connecter au bus KNX à distance depuis un autre réseau IP.\r\n" +
                "Renseignez l'adresse IPv4 publique du routeur permettant l'accès au réseau privé sur lequel le bus est connecté.\r\n" +
                "Cliquez sur Connexion.";
            
            Resources["SecureConnectionTooltipTitle"] = "Aide - Connexion via IP Secure";
            Resources["SecureConnectionTooltipMessage"] =
                "Cette section permet de se connecter à un bus protégé par une interface IP Secure.\r\n" +
                "Connexion IP.........................................Sélectionnez dans la liste ci-dessus l'interface IP Secure.\r\n" +
                "Connexion IP à distance (NAT).......Renseignez ci-dessus l'adresse de l'interface IP Secure.\r\n" +
                "Sélectionnez le fichier de clés exporté au préalable depuis ETS (Propriétés > IP Secure).\r\n" +
                "Renseignez le mot de passe correspondant au fichier de clés.\r\n" +
                "Cliquez sur Connexion.";
        }
        else
        {
            Resources["ConnectionWindowTitle"]="KNX Bus Connection";
            Resources["ConnectionType"]="Connection type:";
            Resources["AvailableInterfaces"]="Available interfaces:";
            Resources["Refresh"]="Refresh";
            Resources["NatConnection"]="Connection (NAT)";
            Resources["PublicIp"]="Router public IP";
            Resources["IndivAddressInterface"]="Individual address of the interface";
            Resources["ToFillWhenSecure"]="To fill only when connection is IP Secure";
            Resources["IPSecureConnection"]="IP Secure connection";
            Resources["KnxKeysFile"]="Keys file .knxkeys";
            Resources["ImportKeys"]="Import keys";
            Resources["ImportedKeysFile"] = "Imported keys file :";
            Resources["PwdKeys"]="Keys file password";
            Resources["ConnectionError"]="Connection error";
            Resources["CurrentInterface"]="Currently connected interface:";
            Resources["ConnectButtonText"]="Connect";
            Resources["DisconnectButtonText"]="Disconnect";
            
            Resources["RemoteConnectionTooltipTitle"] = "Help - Remote Connection (NAT)";
            Resources["RemoteConnectionTooltipMessage"] =
                "This section allows you to connect to the KNX bus remotely from another IP network.\r\n" +
                "Enter the public IPv4 address of the router allowing access to the private network to which the bus is connected.\r\n" + 
                "Click Connect.";
            
            Resources["SecureConnectionTooltipTitle"] = "Help - Secure IP connection";
            Resources["SecureConnectionTooltipMessage"] = 
                "This section allows you to connect to a bus protected by an IP Secure interface.\r\n" + 
                "IP connection.....................................Select the IP Secure interface from the list above.\r\n" + 
                "Remote IP connection (NAT).......Enter the address of the IP Secure interface above.\r\n" + 
                "Select the key file previously exported from ETS (Properties > IP Secure).\r\n" + 
                "Enter the password for the key file.\r\n" + 
                "Click Connect.";

        }
    }
    
    /// <summary>
    /// Updates the color theme of the window according to <see cref="Model.Interfaces.IApplicationSettings.EnableLightTheme"/> state.
    /// </summary>
    private void ApplyThemeToWindow()
    {
        Brush textColorBrush;
        Brush backgroundColorBrush;
        Brush tooltipBackgroundBrush;
        Style tooltipTextBlockStyle;
        
        if (_viewModel.AppSettings.EnableLightTheme)
        {
            textColorBrush = (Brush)FindResource("LightForegroundBrush");
            backgroundColorBrush = (Brush)FindResource("OffWhiteBackgroundBrush");
            tooltipBackgroundBrush = (Brush)FindResource("WhiteBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockLight");
            NatAddressTextBox.Style = (Style)FindResource("TextBoxLight");
            IndivAddressTextBox.Style = (Style)FindResource("TextBoxLight");
            ActualPwdKeysFileTextBox.Style = (Style)FindResource("TextBoxLight");
            ImportKnxKeys.Style = (Style)FindResource("ImportKeysButtonLight");
            ConnectionTypeComboBox.Style = (Style)FindResource("LightComboBoxStyle");
            InterfaceListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerLight");
            InterfaceListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemLight");
            InterfaceListBox.Style = (Style)FindResource("StandardListBoxLight");
            CurrCoInterfaceText.Style = (Style)FindResource("InterfaceTextBlockLight");
            IPSecureBorder.BorderBrush = (Brush)FindResource("DarkGrayBorderBrush");
        }
        else
        {
            textColorBrush = (Brush)FindResource("DarkOffWhiteForegroundBrush");
            backgroundColorBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            tooltipBackgroundBrush = (Brush)FindResource("DarkGrayBackgroundBrush");
            tooltipTextBlockStyle = (Style)FindResource("StandardTextBlockDark");
            NatAddressTextBox.Style = (Style)FindResource("TextBoxDark");
            IndivAddressTextBox.Style = (Style)FindResource("TextBoxDark");
            ActualPwdKeysFileTextBox.Style = (Style)FindResource("TextBoxDark");
            ImportKnxKeys.Style = (Style)FindResource("ImportKeysButtonDark");   
            ConnectionTypeComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
            InterfaceListBox.ItemContainerStyle = (Style)FindResource("ListBoxContainerDark");
            InterfaceListBox.ItemTemplate = (DataTemplate)FindResource("ListBoxItemDark");
            InterfaceListBox.Style = (Style)FindResource("StandardListBoxDark");
            CurrCoInterfaceText.Style = (Style)FindResource("InterfaceTextBlockDark");
            IPSecureBorder.BorderBrush = (Brush)FindResource("LightGrayBorderBrush");
        }
        
        Background = backgroundColorBrush;
        
        ConnectionTypeText.Foreground = textColorBrush;
        DiscoveredInterfacesText.Foreground = textColorBrush;
        
        IpRouterText.Foreground = textColorBrush;
        SecurisedConnectionText.Foreground = textColorBrush;
        ConnectionIpSecText.Foreground = textColorBrush;
        ImportedKeysFileText.Foreground = textColorBrush;
        ImportKeyPathText.Foreground = textColorBrush;
        PwdKeysFileText.Foreground = textColorBrush;
        
        CurrentInterfaceText.Foreground = textColorBrush;
        FileKeysText.Foreground = textColorBrush;
        NatConnectionText.Foreground = textColorBrush;
        IndivAddressText.Foreground = textColorBrush;
        LockIconText.Foreground = textColorBrush;
        CurrCoInterfaceText.Tag = textColorBrush;
        NatIconText.Foreground = textColorBrush;

        Resources["CurrentTooltipBackgroundBrush"] = tooltipBackgroundBrush;
        Resources["CurrentTooltipTextBlockStyle"] = tooltipTextBlockStyle;

    }
    
    /// <summary>
    /// Updates the size of the window and its contents according to <see cref="Model.Interfaces.IApplicationSettings.AppScaleFactor"/>
    /// </summary>
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
    /// then extracts necessary files.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ImportKeysFileButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ConsoleAndLogWriteLineCommand.Execute("Waiting for user to select a KNX keys file");

        // Créer une nouvelle instance de OpenFileDialog pour permettre à l'utilisateur de sélectionner un fichier
        OpenFileDialog openFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = _viewModel.AppSettings.AppLang switch
            { 
                "FR" =>"Sélectionnez un fichier de clés KNX",
                _ => "Select a KNX keys file"
            },
            // Applique un filtre pour n'afficher que les fichiers knxkeys
            Filter = _viewModel.AppSettings.AppLang switch
            {
                "FR" => "Fichier de clés KNX|*.knxkeys",
                _ => "KNX keys file|*.knxkeys"
            },
            // Définit l'index par défaut du filtre
            FilterIndex = 1,
            // N'autorise pas la sélection de plusieurs fichiers à la fois
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            _viewModel.ConsoleAndLogWriteLineCommand.Execute($"KNX keys file selected: {openFileDialog.FileName}");

            // Donner le chemin au Model
            _viewModel.BusConnection.KeysPath = openFileDialog.FileName;
        }
        else
        {
            _viewModel.ConsoleAndLogWriteLineCommand.Execute("User aborted the knxkeys file selection operation");
        }

    }

}