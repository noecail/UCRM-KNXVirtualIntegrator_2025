using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Knx.Falcon.Sdk;
using System.Timers;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace KNX_Virtual_Integrator.Model.Implementations;

/// <summary>
/// Représente la communication avec le bus KNX pour gérer les adresses de groupe et les valeurs de groupe.
/// Permet d'écrire des valeurs sur le bus KNX en fonction de l'état de la connexion et de l'activité du bus.
/// </summary>
public class GroupCommunication : ObservableObject, IGroupCommunication
{
    private readonly BusConnection _busConnection;

    private readonly ILogger _logger;
    
    private GroupAddress _groupAddress;

    /// <summary>
    /// Obtient ou définit l'adresse de groupe utilisée pour la communication sur le bus KNX.
    /// Cette adresse spécifie l'adresse du groupe pour lequel les valeurs sont lues ou écrites.
    /// </summary>
    public GroupAddress GroupAddress
    {
        get => _groupAddress;
        set => SetProperty(ref _groupAddress, value);
    }

    private GroupValue? _groupValue;

    /// <summary>
    /// Obtient ou définit la valeur de groupe à envoyer au bus KNX.
    /// Cette valeur représente l'état du groupe, par exemple, vrai ou faux pour un booléen.
    /// </summary>
    public GroupValue? GroupValue
    {
        get => _groupValue;
        set => SetProperty(ref _groupValue, value);
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="GroupCommunication"/> avec la connexion au bus spécifiée.
    /// S'abonne à l'événement <see cref="BusConnection.BusConnectedReady"/> pour être informé lorsque le bus est prêt.
    /// </summary>
    /// <param name="busConnection">L'objet <see cref="BusConnection"/> utilisé pour la communication avec le bus KNX.</param>
    public GroupCommunication(BusConnection busConnection,ILogger logger)
    {
        _busConnection = busConnection;
        _logger = logger;
        BusChanged(null, _busConnection.Bus);
        _busConnection.BusConnectedReady += OnBusConnectedReady;
    }

    /// <summary>
    /// Envoie une valeur de groupe "On" (vraie) de manière asynchrone au bus KNX.
    /// Vérifie d'abord si le bus est connecté et disponible avant d'envoyer la valeur.
    /// Affiche un message d'erreur si la connexion au bus échoue.
    /// </summary>
    /// <returns>Une tâche représentant l'opération d'écriture asynchrone.</returns>
    public async Task GroupValueWriteOnAsync()
    {
        try
        {
            if (_busConnection is { IsConnected: true, IsBusy: false })
            {
                _groupValue = new GroupValue(true);
                if (_busConnection is { CancellationTokenSource: not null, Bus: not null })
                    await _busConnection.Bus.WriteGroupValueAsync(
                        _groupAddress, _groupValue, MessagePriority.High,
                        _busConnection.CancellationTokenSource.Token
                    );
            }
            else
            {
                //MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du test de l'envoi de la trame : {ex.Message}");
            //MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Envoie une valeur de groupe "Off" (fausse) de manière asynchrone au bus KNX.
    /// Vérifie d'abord si le bus est connecté et disponible avant d'envoyer la valeur.
    /// Affiche un message d'erreur si la connexion au bus échoue.
    /// </summary>
    /// <returns>Une tâche représentant l'opération d'écriture asynchrone.</returns>
    public async Task GroupValueWriteOffAsync()
    {
        try
        {
            if (_busConnection is { IsConnected: true, IsBusy: false })
            {
                _groupValue = new GroupValue(false);
                if (_busConnection is { CancellationTokenSource: not null, Bus: not null })
                    await _busConnection.Bus.WriteGroupValueAsync(
                        _groupAddress, _groupValue, MessagePriority.High,
                        _busConnection.CancellationTokenSource.Token
                    );
            }
            else
            {
                //MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du test de l'envoi de la trame : {ex.Message}");
            //MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Envoie une valeur de groupe spécifique de manière asynchrone au bus KNX pour une adresse de groupe donnée.
    /// Vérifie d'abord si le bus est connecté et disponible avant d'envoyer la valeur.
    /// Affiche un message d'erreur si la connexion au bus échoue.
    /// </summary>
    /// <param name="addr">L'adresse de groupe à laquelle la valeur est envoyée.</param>
    /// <param name="value">La valeur de groupe à envoyer.</param>
    /// <returns>Une tâche représentant l'opération d'écriture asynchrone.</returns>
    public async Task GroupValueWriteAsync(GroupAddress addr, GroupValue value)
    {
        try
        {
            if (_busConnection is { IsConnected: true, IsBusy: false })
            {
                if (_busConnection is { CancellationTokenSource: not null, Bus: not null })
                    await _busConnection.Bus.WriteGroupValueAsync(
                        addr, value, MessagePriority.High,
                        _busConnection.CancellationTokenSource.Token
                    );
            }
            else
            {
                Console.WriteLine("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour écrire.");
                //MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour écrire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                //inclure un "return", comme il faut renvoyer du "void"?
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'envoi de la trame : {ex.Message}");
            //MessageBox.Show($"Erreur lors de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    ///<summary>
    /// Convertit une valeur ulong en tableau de bytes pour l'écriture sur le bus.
    ///</summary>
    /// <param name="toSend">La valeur à envoyer.</param>
    /// <param name="groupValue">Le tableau à remplir pour envoyer plus tard.</param>
    public void ConvertToGroupValue(ulong toSend, byte[] groupValue)
    {
        var intBytes = BitConverter.GetBytes(toSend);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(intBytes);
        for (var i = 0; i < intBytes.Length; i++)
        {
            groupValue[i] = intBytes[i];
        }
    }
    
    //TACHE LECTURE TRAME NORMALE UNIQUE
    /// <summary>
    /// Lit de manière asynchrone la valeur d'un groupe depuis le bus KNX pour une adresse de groupe donnée.
    /// Vérifie la connexion et l'état du bus avant d'envoyer la requête.
    /// Utilise un <see cref="TaskCompletionSource{T}"/> pour capturer la valeur lue.
    /// </summary>
    /// <param name="groupAddress">L'adresse de groupe dont la valeur doit être lue.</param>
    /// <returns>Une tâche représentant l'opération de lecture asynchrone, contenant la valeur lue.</returns>
    public async Task<GroupValue?> MaGroupValueReadAsync(GroupAddress groupAddress)
    {
        if (_busConnection is { IsConnected: false, IsBusy: true, Bus: not null })
        {
            Console.WriteLine("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour lire une valeur.");
            //MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour lire une valeur.",
            //                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        // TaskCompletionSource pour capturer la valeur lue
        var tcs = new TaskCompletionSource<GroupValue>();

        // Handler temporaire pour capturer la valeur lue
        EventHandler<GroupEventArgs>? handler = null;
        handler = (_, e ) =>
        {
            // Vérifie si l'adresse correspond à l'adresse demandée

            if (e.DestinationAddress != groupAddress) return;

            // Définis le résultat pour terminer la tâche
            tcs.SetResult(e.Value);
            // Désabonne le handler une fois que la valeur est capturée
            if (_busConnection is { Bus: not null })
                _busConnection.Bus.GroupMessageReceived -= handler;

        };

        // S'abonner à l'événement
        if (_busConnection is { Bus: not null })
            _busConnection.Bus!.GroupMessageReceived += handler;

        try
        {
            // Envoie la requête pour lire la valeur du groupe
            if (_busConnection is { CancellationTokenSource: not null, Bus: not null })
                await _busConnection.Bus.RequestGroupValueAsync(
                    groupAddress, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}",
            //                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            if (_busConnection is {Bus: not null })
                _busConnection.Bus.GroupMessageReceived -= handler; // Assurez-vous de désabonner même en cas d'exception
            return null;
        }

        // Attendre la tâche de capture de la valeur
        return await tcs.Task;
    }
    
    //TACHE LECTURE TRAME NORMALE MULTIPLE SOUS TIMER
    /// <summary>
    /// Lit de manière asynchrone les valeurs d'un groupe depuis le bus KNX pour une adresse de groupe donnée.
    /// Vérifie la connexion et l'état du bus avant d'envoyer la requête.
    /// Utilise un <see cref="TaskCompletionSource{T}"/> pour capturer la valeur lue.
    /// </summary>
    /// <param name="groupAddress">L'adresse de groupe dont la valeur doit être lue.</param>
    ///  <param name="timerDuration">Le timer sous lequel les trames doivent être reçues</param>
    /// <returns>Une tâche représentant l'opération de lecture asynchrone, contenant la valeur lue.</returns>
    public async Task<List<GroupMessage>> GroupValuesWithinTimerAsync(GroupAddress groupAddress, int timerDuration)
    {
        var theList = new List<GroupMessage>();
        Timer timer = new Timer(timerDuration);
        if (_busConnection is { IsConnected: false, IsBusy: true })
        {
            Console.WriteLine("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour lire une valeur.");
            //MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour lire une valeur.",
            //                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return [];
        }
 
        timer.AutoReset = false;
        ElapsedEventHandler? timerHandler = null;
        timerHandler = (_, _) =>
        {
            timer.Enabled = false;
            timer.Elapsed -= timerHandler;
        };
        timer.Elapsed += timerHandler;
        try
        {
            //subscribe to Message updates
            timer.Enabled = true;
            NotifyCollectionChangedEventHandler messageHandler = (_, _) =>
            {
                if (Messages.Last().DestinationAddress != groupAddress) return;
                theList.Add(Messages.Last());
            };
            
            Messages.CollectionChanged += messageHandler;
            
            if (_busConnection is { CancellationTokenSource: not null, Bus: not null })
                await _busConnection.Bus.RequestGroupValueAsync(
                    groupAddress, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token
                );
            
            while (timer.Enabled) { }
            
            Messages.CollectionChanged -= messageHandler;
            timer.Enabled = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}",
            //                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return [];
        }

        // Retourne toutes les valeurs reçues sans traitement
        return theList;
    }
    
    
    // Liste observable pour les messages reçus
    public ObservableCollection<GroupMessage> Messages { get; private set; } = new ObservableCollection<GroupMessage>();

    private readonly ObservableCollection<GroupEventArgs> _groupEvents = new ObservableCollection<GroupEventArgs>();

    /// <summary>
    /// Obtient la collection observable des événements de groupe reçus.
    /// </summary>
    public ObservableCollection<GroupEventArgs> GroupEvents => _groupEvents;

    /// <summary>
    /// Gestionnaire d'événement appelé lorsque le bus KNX est prêt à être utilisé.
    /// Met à jour la connexion du bus et réinitialise la liste des événements de groupe.
    /// </summary>
    /// <param name="sender">L'objet source de l'événement.</param>
    /// <param name="newBus">Le nouveau bus KNX connecté.</param>
    private void OnBusConnectedReady(object? sender, KnxBus newBus)
    {
        // Appelle BusChanged avec le nouveau bus
        BusChanged(null, newBus);
        // Attention potentiellement si je me connecte à un nouveau bus l'ancien ne se désabonne pas de l'événement dans BusChanged
    }
    /// <summary>
    /// Gère les changements de bus en désabonnant l'ancien bus et en abonnissant le nouveau bus aux événements.
    /// Réinitialise la liste des événements de groupe.
    /// </summary>
    /// <param name="oldBus">L'ancien bus KNX.</param>
    /// <param name="newBus">Le nouveau bus KNX.</param>
    internal void BusChanged(KnxBus? oldBus, KnxBus? newBus)
    {
        if (oldBus != null)
            oldBus.GroupMessageReceived -= OnGroupMessageReceived;
        if (newBus != null)
            newBus.GroupMessageReceived += OnGroupMessageReceived;
        _groupEvents.Clear();
    }

    /// <summary>
    /// Représente un message de groupe reçu, incluant l'adresse de destination, l'adresse source, la valeur et le type d'événement.
    /// </summary>
    public class GroupMessage
    {
        /// <summary>
        /// L'adresse de groupe à laquelle le message est destiné.
        /// </summary>
        public GroupAddress DestinationAddress { get; init; }

        /// <summary>
        /// L'adresse individuelle de la source du message.
        /// </summary>
        public IndividualAddress SourceAddress { get; init; }

        /// <summary>
        /// La valeur du groupe associée au message.
        /// </summary>
        public GroupValue? Value { get; init; }

        /// <summary>
        /// Le type d'événement associé au message, si nécessaire.
        /// </summary>
        public GroupEventType EventType { get; init; }
    }

    /// <summary>
    /// Gestionnaire d'événement appelé lorsqu'un message de groupe est reçu.
    /// Crée une entrée pour le message reçu et met à jour la liste observable des messages.
    /// </summary>
    /// <param name="sender">L'objet source de l'événement.</param>
    /// <param name="e">Les arguments de l'événement contenant les détails du message reçu.</param>
    private void OnGroupMessageReceived(object? sender, GroupEventArgs e)
    {
        // Crée une nouvelle entrée pour le message reçu
        var newMessage = new GroupMessage
        {
            SourceAddress = e.SourceAddress,
            DestinationAddress = e.DestinationAddress,
            Value = e.Value,
            EventType = e.EventType
        };

        //TODO : Dé-commenter lorsque l'interface Dispatcher wrapper sera créée pour permettre de tester
        
        // Assure-toi que l'ajout à la collection est fait sur le thread du Dispatcher
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            // Ajoute la nouvelle entrée à la liste observable
            Messages.Add(newMessage);
        });
    }
}