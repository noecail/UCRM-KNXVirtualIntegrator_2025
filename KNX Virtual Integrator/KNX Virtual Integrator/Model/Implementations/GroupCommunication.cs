using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Collections.ObjectModel;
using Knx.Falcon.Sdk;


namespace KNX_Virtual_Integrator.Model.Implementations;

/// <summary>
/// Représente la communication avec le bus KNX pour gérer les adresses de groupe et les valeurs de groupe.
/// Permet d'écrire des valeurs sur le bus KNX en fonction de l'état de la connexion et de l'activité du bus.
/// </summary>
public class GroupCommunication : ObservableObject, IGroupCommunication
{
    private readonly BusConnection _busConnection;

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
    public GroupCommunication(BusConnection busConnection)
    {
        _busConnection = busConnection;
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du test de l'envoi de la trame : {ex.Message}");
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du test de l'envoi de la trame : {ex.Message}");
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
                Console.WriteLine("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'envoi de la trame : {ex.Message}");
        }
    }

    ///<summary>
    /// Convertit une valeur ulong en tableau de bytes pour l'écriture sur le bus.
    ///</summary>
    /// <param name="toSend">La valeur à envoyer.</param>
    /// <param name="groupValue">Le tableau à remplir.</param>
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

    /// <summary>
    /// Lit de manière asynchrone la valeur d'un groupe depuis le bus KNX pour une adresse de groupe donnée.
    /// Vérifie la connexion et l'état du bus avant d'envoyer la requête.
    /// Utilise un <see cref="TaskCompletionSource{T}"/> pour capturer la valeur lue.
    /// </summary>
    /// <param name="groupAddress">L'adresse de groupe dont la valeur doit être lue.</param>
    /// <returns>Une tâche contenant la valeur lue.</returns>
    public async Task<GroupValue> MaGroupValueReadAsync(GroupAddress groupAddress)
    {
        if (_busConnection is { IsConnected: false, IsBusy: true })
        {
            return null;
        }

        var tcs = new TaskCompletionSource<GroupValue>();
        EventHandler<GroupEventArgs> handler = null;
        handler = (sender, e) =>
        {
            if (e.DestinationAddress == groupAddress)
            {
                tcs.SetResult(e.Value);
                _busConnection.Bus.GroupMessageReceived -= handler;
            }
        };

        _busConnection.Bus.GroupMessageReceived += handler;

        try
        {
            if (_busConnection is { CancellationTokenSource: not null, Bus: not null })
                await _busConnection.Bus.RequestGroupValueAsync(
                    groupAddress, MessagePriority.High,
                    _busConnection.CancellationTokenSource.Token
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}");
            _busConnection.Bus.GroupMessageReceived -= handler;
            return null;
        }

        return await tcs.Task;
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
    private void OnBusConnectedReady(object sender, KnxBus newBus)
    {
        BusChanged(null, newBus);
    }

    /// <summary>
    /// Gère les changements de bus en désabonnant l'ancien bus et en abonnissant le nouveau bus aux événements.
    /// Réinitialise la liste des événements de groupe.
    /// </summary>
    internal void BusChanged(KnxBus oldBus, KnxBus newBus)
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
        public GroupAddress DestinationAddress { get; set; }
        public IndividualAddress SourceAddress { get; set; }
        public GroupValue Value { get; set; }
        public GroupEventType EventType { get; set; }
    }

    /// <summary>
    /// Gestionnaire d'événement appelé lorsqu'un message de groupe est reçu.
    /// Crée une entrée pour le message reçu et met à jour la liste observable des messages.
    /// </summary>
    private void OnGroupMessageReceived(object sender, GroupEventArgs e)
    {
        var newMessage = new GroupMessage
        {
            SourceAddress = e.SourceAddress,
            DestinationAddress = e.DestinationAddress,
            Value = e.Value,
            EventType = e.EventType
        };

        Messages.Add(newMessage);
    }
}