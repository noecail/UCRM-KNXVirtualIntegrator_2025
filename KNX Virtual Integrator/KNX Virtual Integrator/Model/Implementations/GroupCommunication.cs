using System.Windows;
using GalaSoft.MvvmLight;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Collections.ObjectModel;
using Knx.Falcon.Sdk;


namespace KNX_Virtual_Integrator.Model.Implementations;

public class GroupCommunication : ObservableObject, IGroupCommunication
{

    private readonly BusConnection _busConnection;

    private GroupAddress _groupAddress;
    public GroupAddress GroupAddress
    {
        get => _groupAddress;
        set => Set(() => GroupAddress, ref _groupAddress, value);
    }

    private GroupValue? _groupValue;
    public GroupValue? GroupValue
    {
        get => _groupValue;
        set => Set(() => GroupValue, ref _groupValue, value);
    }

    public GroupCommunication(BusConnection busConnection)
    {
        _busConnection = busConnection;

        // Initialisation @ de groupe + GroupValue = type booleen 1 bit
        //_groupAddress = new GroupAddress("0/1/1"); // Exemple d'adresse par défaut
        //_groupValue = new GroupValue(true); // Initialisation par défaut

        // Evenement un bus est detecté
        BusChanged(null, _busConnection.Bus);

        // Dès que j'ai BusConnectedReady je m'abonne à l'événement OnBusConnectedReady donc je suis prêt à faire ce qu'il y a dedans
        _busConnection.BusConnectedReady += OnBusConnectedReady;
    }

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
                MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }

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
                MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }

    //TACHE ENVOYER TRAME NORMALE
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
                MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

        }
    }


    //TACHE LECTURE TRAME NORMALE
    public async Task<GroupValue> MaGroupValueReadAsync(GroupAddress groupAddress)
    {
        if (_busConnection is { IsConnected: false, IsBusy: true })
        {
            MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        // TaskCompletionSource pour capturer la valeur lue
        var tcs = new TaskCompletionSource<GroupValue>();

        // Handler temporaire pour capturer la valeur lue
        EventHandler<GroupEventArgs> handler = null;
        handler = (sender, e) =>
        {
            // Vérifie si l'adresse correspond à l'adresse demandée
            if (e.DestinationAddress == groupAddress)
            {
                // Définis le résultat pour terminer la tâche
                tcs.SetResult(e.Value);

                // Désabonne l'handler une fois que la valeur est capturée
                _busConnection.Bus.GroupMessageReceived -= handler;
            }
        };

        // S'abonner à l'événement
        _busConnection.Bus.GroupMessageReceived += handler;

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
            MessageBox.Show($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

            _busConnection.Bus.GroupMessageReceived -= handler; // Assurez-vous de désabonner même en cas d'exception
            return null;
        }

        // Attendre la tâche de capture de la valeur
        return await tcs.Task;
    }



        






    //Evenement un bus est detecte
    

    private readonly ObservableCollection<GroupEventArgs> _groupEvents = new ObservableCollection<GroupEventArgs>();
    public ObservableCollection<GroupEventArgs> GroupEvents => _groupEvents;

    private void OnBusConnectedReady(object sender, KnxBus newBus)
    {
        // Appelle BusChanged avec le nouveau bus
        BusChanged(null, newBus);
        //attention potentiellement si je me connecte à un nouveau bus lancien va pas se desabonner de levenement dans BusChanged
    }
    internal void BusChanged(KnxBus oldBus, KnxBus newBus)
    {
        if (oldBus != null)
            oldBus.GroupMessageReceived -= OnGroupMessageReceived;
        if (newBus != null)
            newBus.GroupMessageReceived += OnGroupMessageReceived;
        _groupEvents.Clear();
    }

    // Liste observable pour les messages reçus
    public ObservableCollection<GroupMessage> Messages { get; private set; } = new ObservableCollection<GroupMessage>();

    public class GroupMessage
    {
        public GroupAddress DestinationAddress { get; set; }
        public IndividualAddress SourceAddress { get; set; }
        public GroupValue Value { get; set; }
        public GroupEventType EventType { get; set; } // Type d'événement, si nécessaire
    }


    //Ce qui ce declenche à l'evenement
    private void OnGroupMessageReceived(object sender, GroupEventArgs e)
    {
        if (sender == null)
        {
            Console.WriteLine("Le sender est null.");
        }

        if (e == null)
        {
            Console.WriteLine("Les paramètres de l'événement sont null.");
            return; // Arrêter l'exécution si e est null
        }


        // Crée une nouvelle entrée pour le message reçu
        var newMessage = new GroupMessage
        {
            SourceAddress = e.SourceAddress,
            DestinationAddress = e.DestinationAddress,
            Value = e.Value,
            EventType = e.EventType
        };

        // Assure-toi que l'ajout à la collection est fait sur le thread du Dispatcher ???????
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Ajoute la nouvelle entrée à la liste observable
            Messages.Add(newMessage);
        });

    }


}