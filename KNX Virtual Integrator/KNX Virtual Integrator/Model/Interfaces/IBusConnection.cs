using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.ViewModel;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

namespace KNX_Virtual_Integrator.Model.Interfaces;

/// <summary>
/// Interface pour gérer la connexion au bus KNX.
/// Fournit des propriétés et des méthodes pour se connecter, se déconnecter, découvrir les interfaces disponibles et gérer les états de connexion.
/// Cette interface implémente également <see cref="INotifyPropertyChanged"/> pour notifier les changements de propriété.
/// </summary>
public interface IBusConnection : INotifyPropertyChanged
{ 
    
    // ------------------------------------ PROPRIÉTÉS ------------------------------------ 
    
    /// <summary>
    /// Obtient la collection observable des interfaces découvertes.
    /// Cette collection contient les instances de <see cref="ConnectionInterfaceViewModel"/> représentant les interfaces disponibles.
    /// </summary>
    ObservableCollection<ConnectionInterfaceViewModel> DiscoveredInterfaces { get; }

    /// <summary>
    /// Obtient ou définit l'interface de connexion actuellement sélectionnée.
    /// Cette propriété est une instance de <see cref="ConnectionInterfaceViewModel"/> qui représente l'interface de connexion sélectionnée par l'utilisateur.
    /// </summary>
    ConnectionInterfaceViewModel? SelectedInterface { get; set; }

    /// <summary>
    /// Obtient ou définit le type de connexion sélectionné sous forme de chaîne.
    /// Cette propriété indique le type de connexion sélectionné par l'utilisateur (par exemple, "IP", "USB").
    /// </summary>
    string? SelectedConnectionType { get; set; }

    /// <summary>
    /// Obtient un indicateur qui spécifie si le bus est actuellement connecté.
    /// Cette propriété est vraie si le bus est connecté, sinon elle est fausse.
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Contient le nom de l'interface par laquelle la connexion au bus est actuellement établie.
    /// </summary>
    string CurrentInterface { get; }
    
    /// <summary>
    /// Contient l'adresse du routeur IP distant permettant de se connecter à distance
    /// </summary>
    string NatAddress { get; set; }

    /// <summary>
    /// Choix d'accès à distance via NAT.
    /// </summary>
    public bool NatAccess { get; set; }

    /// <summary>
    /// Contient le mot de passe du fichier de clés sécurisant une connexion IP Secure
    /// </summary>
    string Password { get; set; }
    
    /// <summary>
    /// Contient le chemin vers le fichier de clés permettant la connexion IP Secure. Ce fichier a été exporté par l'utilisateur au préalable depuis ETS
    /// </summary>
    string KeysPath { get; set; }

    /// <summary>
    /// Contient le message d'erreur à afficher dans la fenêtre de connexion après une connexion ratée
    /// </summary>
    string ConnectionErrorMessage { get; set; }

    
    // ------------------------------------ TACHES ------------------------------------
    
    /// <summary>
    /// Établit une connexion asynchrone au bus KNX.
    /// Cette méthode initialise la connexion en utilisant les paramètres fournis, gère les erreurs éventuelles et met à jour l'état de connexion.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de connexion asynchrone.</returns>
    Task ConnectBusAsync();

    /// <summary>
    /// Déconnecte de manière asynchrone du bus KNX.
    /// Cette méthode gère la déconnexion, libère les ressources associées et met à jour l'état de connexion.
    /// </summary>
    /// <returns>Une tâche représentant l'opération de déconnexion asynchrone.</returns>
    Task DisconnectBusAsync();

    /// <summary>
    /// Découvre les interfaces disponibles de manière asynchrone en fonction du type de connexion sélectionné.
    /// Cette méthode met à jour la collection des interfaces découvertes et peut gérer différents types de connexions (comme IP ou USB).
    /// </summary>
    /// <returns>Une tâche représentant l'opération de découverte asynchrone des interfaces.</returns>
    Task DiscoverInterfacesAsync();

    /// <summary>
    /// Gère le changement du type de connexion sélectionné.
    /// Cette méthode est appelée lorsque la sélection du type de connexion change, entraînant la découverte des interfaces disponibles pour ce type.
    /// </summary>
    void OnSelectedConnectionTypeChanged();

    
}