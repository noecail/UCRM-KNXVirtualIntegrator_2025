using CommunityToolkit.Mvvm.ComponentModel;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;
using System.Windows.Threading;
using Timer = System.Timers.Timer;
using KNX_Virtual_Integrator.Model.Wrappers;

namespace KNX_Virtual_Integrator.Model.Implementations;

/// <summary>
/// Represents the communication with the bus in terms of group addresses and values.
/// Allows writing and reading information from the bus while checking its state.
/// </summary>
public class GroupCommunication : ObservableObject, IGroupCommunication
{
    private readonly BusConnection _busConnection;

    private readonly ILogger _logger;
    
    private GroupAddress _groupAddress;

    /// <summary>
    /// Gets or sets the address with which the program will communicate with the bus.
    /// It specifies which Group Address will make use or read the <see cref="GroupValue"/>.
    /// </summary>
    public GroupAddress GroupAddress
    {
        get => _groupAddress;
        set => SetProperty(ref _groupAddress, value);
    }

    private GroupValue? _groupValue;

    /// <summary>
    /// Gets the group value to send to the bus.
    /// It represents which state the object will be in.
    /// </summary>
    public GroupValue? GroupValue
    {
        get => _groupValue;
        set => SetProperty(ref _groupValue, value);
    }

    /// <summary>
    /// Initializes an instance of <see cref="GroupCommunication"/> with the specified bus connection.
    /// Subscribes to the event <see cref="BusConnection.BusConnectedReady"/> to be informed when the bus is ready.
    /// </summary>
    /// <param name="busConnection">The object <see cref="BusConnection"/> used to communicate with the KNX Bus.</param>
    /// <param name="logger">The object <see cref="Logger"/> used for logging events.</param>
    public GroupCommunication(BusConnection busConnection,ILogger logger)
    {
        _busConnection = busConnection;
        _logger = logger;
        BusChanged(null, _busConnection.Bus);
        _busConnection.BusConnectedReady += OnBusConnectedReady;
    }

    /// <summary>
    /// Dev method. Sends asynchronously the value "on" to a specified address.
    /// First verifies the bus state before sending the value.
    /// Logs an error if it fails. 
    /// </summary>
    /// <returns>A task representing the completion of the writing.</returns>
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
            _logger.ConsoleAndLogWriteLine($"Error sending the message : {ex.Message}");
        }
    }

    /// <summary>
    /// Dev method. Sends asynchronously the value "off" to a specified address.
    /// First verifies the bus state before sending the value.
    /// Logs an error if it fails. 
    /// </summary>
    /// <returns>A task representing the completion of the writing.</returns>
    public async Task GroupValueWriteOffAsync()
    {
        try
        {
            if (_busConnection is { IsConnected: true, IsBusy: false })
            {
                _groupValue = new GroupValue(false);
                if (_busConnection is { CancellationTokenSource: not null, Bus.IsNull : false })
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
            _logger.ConsoleAndLogWriteLine($"Erreur lors du test de l'envoi de la trame : {ex.Message}");
            //MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Sends asynchronously a value to a specified address.
    /// First verifies the bus state before sending the value.
    /// Logs an error if it fails.
    /// </summary>
    /// <param name="addr">The address at which the value is sent.</param>
    /// <param name="value">The value to send.</param>
    /// <returns>A task representing the completion of the writing.</returns>
    public async Task<bool> GroupValueWriteAsync(GroupAddress addr, GroupValue value)
    {
        try
        {
            if (_busConnection is
                { IsConnected: true, IsBusy: false, CancellationTokenSource: not null, Bus.IsNull: false })
                return await _busConnection.Bus.WriteGroupValueAsync(
                    addr, value, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token);

            _logger.ConsoleAndLogWriteLine(
                "The Knx bus is not connected. Please connect before writing.");
            return false;

        }
        catch (Exception ex)
        {
            _logger.ConsoleAndLogWriteLine($"Error sending the message : {ex.Message}");
            return false;
        }
    }

    ///<summary>
    /// Converts a uLong value to a byte table to write on the bus
    ///</summary>
    /// <param name="toSend">The value to send.</param>
    /// <param name="groupValue">The table to fill before writing.</param>
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
    /// Reads asynchronously values from a group address.
    /// Verifies the bus connection state before sending the request.
    /// uses a <see cref="TaskCompletionSource{T}"/> to capture the read value.
    /// </summary>
    /// <param name="groupAddress">The group address at which the value should be read.</param>
    /// <returns>A task representing the completion of the task, containing the received messages.</returns>
    public async Task<GroupValue?> MaGroupValueReadAsync(GroupAddress groupAddress)
    {
        if (_busConnection is { IsConnected: false, IsBusy: true, Bus: not null })
        {
            _logger.ConsoleAndLogWriteLine("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour lire une valeur.");
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
            if (_busConnection.Bus is { IsNull: false })
                _busConnection.Bus.GroupMessageReceived -= handler;

        };

        // S'abonner à l'événement
        if (_busConnection is { Bus.IsNull : false })
            _busConnection.Bus.GroupMessageReceived += handler;

        try
        {
            // Envoie la requête pour lire la valeur du groupe
            if (_busConnection is { CancellationTokenSource: not null, Bus.IsNull: false })
                await _busConnection.Bus.RequestGroupValueAsync(
                    groupAddress, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token
                );
        }
        catch (Exception ex)
        {
            _logger.ConsoleAndLogWriteLine($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}");
            //MessageBox.Show($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}",
            //                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            if (_busConnection is { Bus.IsNull : false })
                _busConnection.Bus.GroupMessageReceived -= handler; // Assurez-vous de désabonner même en cas d'exception
            return null;
        }

        // Attendre la tâche de capture de la valeur
        return await tcs.Task;
    }
    
    //TACHE LECTURE TRAME NORMALE MULTIPLE SOUS TIMER
    /// <summary>
    /// Reads asynchronously values from a group address until the timer runs out.
    /// Verifies the bus connection state before sending the request.
    /// uses a <see cref="TaskCompletionSource{T}"/> to capture the read value.
    /// </summary>
    /// <param name="groupAddress">The group address at which the value should be read.</param>
    ///  <param name="timerDuration">Timer in ms under which the message should be received.</param>
    /// <returns>A task representing the completion of the task, containing the received messages..</returns>
    public async Task<List<GroupMessage>> GroupValuesWithinTimerAsync(GroupAddress groupAddress, int timerDuration)
    {
        var theList = new List<GroupMessage>();
        Timer timer = new Timer(timerDuration);
        if (!_busConnection.IsConnected)
        {
            _logger.ConsoleAndLogWriteLine("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord pour lire une valeur.");
            return [];
        }

        if (_busConnection.IsBusy)
        {
            _logger.ConsoleAndLogWriteLine("Le bus est occupé");
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
                if (!Messages.Last().DestinationAddress.Equals(groupAddress)) return;
                // La 2e vérification est nécessitée par le dédoublement des messages reçus ( influence du wrapper? )
                if (theList.Count > 0 && theList.Last().Equals(Messages.Last())) return;
                theList.Add(Messages.Last());
            };
            
            Messages.CollectionChanged += messageHandler;
            
            // Possibilité (fine) que les messages de Response arrivent en même temps que les messages de Write
            if (_busConnection is { CancellationTokenSource: not null, Bus.IsNull : false })
                await _busConnection.Bus.RequestGroupValueAsync(
                    groupAddress, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token
                );
            
            while (timer.Enabled) { }
            
            Messages.CollectionChanged -= messageHandler;
            timer.Enabled = false; // Juste au cas où il y a un problème avec le timer
        }
        catch (Exception ex)
        {
            _logger.ConsoleAndLogWriteLine($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}");
            return [];
        }

        // Retourne toutes les valeurs reçues sans traitement
        return theList;
    }
    
    
    //TACHE LECTURE TRAME NORMALE MULTIPLE SOUS TIMER OU RECEPTION D'UNE MESSAGE DE TYPE WRITE
    /// <summary>
    /// Reads asynchronously values from a group address until a Write is received or the timer runs out.
    /// Verifies the bus connection state before sending the request.
    /// uses a <see cref="TaskCompletionSource{T}"/> to capture the read value.
    /// </summary>
    /// <param name="groupAddress">The group address at which the value should be read.</param>
    ///  <param name="timerDuration">Timer in ms under which the message should be received.</param>
    /// <returns>A task representing the completion of the task, containing the received messages.</returns>
    public async Task<List<GroupMessage>> GroupValuesTimerOrRecievedAWriteAsync(GroupAddress groupAddress, int timerDuration)
    {
        var theList = new List<GroupMessage>();
        Timer timer = new Timer(timerDuration);
        if (!_busConnection.IsConnected)
        {
            _logger.ConsoleAndLogWriteLine("The Knx bus is not connected. Please connect before reading a value.");
            return [];
        }

        if (_busConnection.IsBusy)
        {
            _logger.ConsoleAndLogWriteLine("The bus is occupied");
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
                if (!Messages.Last().DestinationAddress.Equals(groupAddress)) return;
                // La 2e vérification est nécessitée par le dédoublement des messages reçus ( influence du wrapper? )
                if (theList.Count > 0 && theList.Last().Equals(Messages.Last())) return;
                theList.Add(Messages.Last());
            };
            Messages.CollectionChanged += messageHandler;
            
            
            // Possibilité (fine) que les messages de Response arrivent après les messages de Write
            if (_busConnection is { CancellationTokenSource: not null, Bus.IsNull : false })
                await _busConnection.Bus.RequestGroupValueAsync(
                    groupAddress, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token
                );
            
            SpinWait.SpinUntil(() => !timer.Enabled || (theList.Count > 0 && theList.Last().EventType.Equals(GroupEventType.ValueWrite)));
            
            Messages.CollectionChanged -= messageHandler;
            timer.Enabled = false; // Juste au cas où il y a un problème avec le timer ou si on sort avant sa fin
        }
        catch (Exception ex)
        {
            _logger.ConsoleAndLogWriteLine($"Error reading the group value : {ex.Message}");
            return [];
        }

        // Retourne toutes les valeurs reçues sans traitement
        return theList;
    }
    
    
    // Liste observable pour les messages reçus
    /// <summary>
    /// Observable collection of received messages.
    /// </summary>
    private ObservableCollection<GroupMessage> Messages { get; } = new ();
    
    /// <summary>
    /// The collection of group event args.
    /// </summary>
    private readonly ObservableCollection<GroupEventArgs> _groupEvents = new ();

    /// <summary>
    /// Gets the collection of group event args.
    /// </summary>
    public ObservableCollection<GroupEventArgs> GroupEvents => _groupEvents;

    /// <summary>
    /// Event handler called when the new bus is ready for interaction.
    /// Resets the event handlers of message reception.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="newBus">The new connected knx bus.</param>
    private void OnBusConnectedReady(object? sender, IKnxBusWrapper newBus)
    {
        // Appelle BusChanged avec le nouveau bus
        BusChanged(null, newBus);
        // Attention potentiellement si je me connecte à un nouveau bus l'ancien ne se désabonne pas de l'événement dans BusChanged
    }
    /// <summary>
    /// Unsubscribes the old bus and subscribes the new bus to messages reception.
    /// Resets the event handlers.
    /// </summary>
    /// <param name="oldBus">The old knx bus.</param>
    /// <param name="newBus">The new knx bus.</param>
    private void BusChanged(IKnxBusWrapper? oldBus, IKnxBusWrapper? newBus)
    {
        if (oldBus != null)
            oldBus.GroupMessageReceived -= OnGroupMessageReceived;
        if (newBus != null)
            newBus.GroupMessageReceived += OnGroupMessageReceived;
        _groupEvents.Clear();
    }

    /// <summary>
    /// Represents a message received.
    /// </summary>
    public class GroupMessage
    {
        /// <summary>
        /// The GroupAddress of the destination 
        /// </summary>
        public GroupAddress DestinationAddress { get; init; }

        /// <summary>
        /// The individual address of the source.
        /// </summary>
        public IndividualAddress SourceAddress { get; init; }

        /// <summary>
        /// The GroupValue transported by the message.
        /// </summary>
        public GroupValue? Value { get; init; }

        /// <summary>
        /// The type of event associated with the message (Write, Response,..)
        /// </summary>
        public GroupEventType EventType { get; init; }
        /// <summary>
        /// Checks the equivalence of the Destination and Source address, the EventType and the Value
        /// </summary>
        /// <param name="obj">The message to compare</param>
        /// <returns>true is all are equal(and all except Value are not null). False otherwise</returns>
        public bool Equals(GroupMessage? obj)
        {
            if (obj is null) 
                return false;
            if (Value is null && obj.Value is null) 
                return DestinationAddress.Equals(obj.DestinationAddress) 
                       && SourceAddress.Equals(obj.SourceAddress) 
                       && EventType.Equals(obj.EventType);
            if (Value is null || obj.Value is null)
                return false;
            return Value.Equals(obj.Value) 
                   && DestinationAddress.Equals(obj.DestinationAddress) 
                   && SourceAddress.Equals(obj.SourceAddress) 
                   && EventType.Equals(obj.EventType);
        }
    }

    /// <summary>
    /// Event handler called when a message is received.
    /// Creates an entry in the list of group messages received.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The args containing the details of the message.</param>
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
        
        //TODO : Dé-commenter lorsque l'interface Dispatcher wrapper sera créée pour permettre de tester (si nécessaire?)
        
        // Assure-toi que l'ajout à la collection est fait sur le thread du Dispatcher
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            // Ajoute la nouvelle entrée à la liste observable
            Messages.Add(newMessage);
        });
    }
}