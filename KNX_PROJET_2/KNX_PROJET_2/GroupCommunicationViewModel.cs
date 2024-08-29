using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Knx.Falcon;
using Knx.Falcon.KnxnetIp;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Knx.Falcon.Configuration;
using Knx.Falcon.Sdk;
using System.Windows.Threading;
using System.Net;



namespace KNX_PROJET_2
{
    public class GroupCommunicationViewModel : ViewModelBase
    {
        private readonly MainViewModel _globalViewModel;

        //________________________________________________________________________________________________________//

        private GroupAddress _groupAddressone;
        public GroupAddress GroupAddressone
        {
            get => _groupAddressone;
            set => Set(() => GroupAddressone, ref _groupAddressone, value);
        }

        private GroupValue _groupValueone;
        public GroupValue GroupValueone
        {
            get => _groupValueone;
            set => Set(() => GroupValueone, ref _groupValueone, value);
        }



        //________________________________________________________________________________________________________//


        private GroupAddress _groupAddress;
        public GroupAddress GroupAddress
        {
            get => _groupAddress;
            set => Set(() => GroupAddress, ref _groupAddress, value);
        }
        
        private readonly GroupValueViewModel _groupValue;
        public GroupValueViewModel GroupValue => _groupValue;

        private List<(GroupAddress addr, GroupValue value)> _groupValues;
        public List<(GroupAddress addr, GroupValue value)> GroupValues
        {
            get => _groupValues;
            set => Set(() => GroupValues, ref _groupValues, value);
        }

        private List<GroupAddress> _groupaddr;
        public List<GroupAddress> ListGroupAddr
        {
            get => _groupaddr;
            set => Set(() => ListGroupAddr, ref _groupaddr, value);
        }

        /*public List<(GroupAddress addr, GroupValue value)> GroupValues
        {
            get => _groupValues;
            set
            {
                if (Set(() => GroupValues, ref _groupValues, value))
                {
                    // Notifiez les modifications
                    RaisePropertyChanged(nameof(GroupValues));
                }
            }
        }*/

        //________________________________________________________________________________________________________//
        public ICommand GroupValueWriteONCommand { get; set; }
        public ICommand GroupValueWrite0FFCommand { get; set; }

        public ICommand GroupValueReadCommand { get; set; }
        public ICommand GroupValueWriteCommand { get; set; }

        public ICommand SendGroupValuesCommand { get; set; }
        public ICommand ReadGroupAddressCommand { get; set; }

        //________________________________________________________________________________________________________//

        public GroupCommunicationViewModel(MainViewModel globalViewModel)
        {
            _globalViewModel = globalViewModel;
            //_dispatcher = Dispatcher.CurrentDispatcher;

            _groupAddressone = new GroupAddress("0/1/1"); // Exemple d'adresse par défaut
            GroupValueWriteONCommand = new RelayCommand(async () => await GroupValueWriteONAsync());
            GroupValueWrite0FFCommand = new RelayCommand(async () => await GroupValueWrite0FFAsync());


            GroupValueReadCommand = new RelayCommand<object>(async (parameter) => 
            await GroupValueReadAsync());

            GroupValueWriteCommand = new RelayCommand(
            async () => await GroupValueWriteAsync(), () => _globalViewModel.IsConnected && !_globalViewModel.IsBusy);

            SendGroupValuesCommand = new RelayCommand<object>(async (parameter) => 
            await SendGroupValuesAsync(parameter as List<(GroupAddress, GroupValue)>));

            ReadGroupAddressCommand = new RelayCommand<object>(async (parameter) =>
            await ReadGroupAddressAsync(parameter as List<GroupAddress>));

            // Initialisation de la liste GroupValues
            GroupValues = new List<(GroupAddress, GroupValue)>
            {
                (new GroupAddress("0/1/1"), new GroupValue(true)),
                (new GroupAddress("1/0/1"), new GroupValue(true)),
                    
            };

            ListGroupAddr = new List<GroupAddress>
            {
                new GroupAddress("0/2/1"),
                new GroupAddress("1/4/1")
            };

            //Initialisation @ de groupe + GroupValue = type booleen 1 bit
            //_groupAddress = new GroupAddress("0/1/2"); // Exemple d'adresse par défaut
            _groupValue = new GroupValueViewModel(new GroupValue(false));
            //EST CE QUE CA SERT A QQCHOSE DE METTRE PAR DEFAUT ?

            BusChanged(null, _globalViewModel._bus);

            // Abonnez-vous à l'événement GroupMessageReceived
            //_globalViewModel._bus.GroupMessageReceived += OnGroupMessageReceived;

            // Abonne-toi à l'événement BusConnected

            _globalViewModel.BusConnectedReady += OnBusConnectedReady;
        }
        
        
        
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

        //________________________________________________________________________________________________________//

        private async Task GroupValueWriteAsync()
        {

            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    await _globalViewModel._bus.WriteGroupValueAsync(
                        GroupAddress, GroupValue.Value, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
                    ); //si ca marche pas mettre _groupadresse _groupvalue , faut voir aussi si ca prend bien le par defaut
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

        /*
        private GroupAddress _readGroupAddress;
        public GroupAddress ReadGroupAddress
        {
            get => _readGroupAddress;
            set => Set(() => ReadGroupAddress, ref _readGroupAddress, value);
        }

        private string _readGroupValue;
        public string ReadGroupValue
        {
            get => _readGroupValue;
            set => Set(() => ReadGroupValue, ref _readGroupValue, value);
        }*/

        private async Task GroupValueReadAsync()
        {
            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                        var readValue = await _globalViewModel._bus.RequestGroupValueAsync(
                        GroupAddress, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token);
                }
                else
                {
                    MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.",
                                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            // Test de verification
            //MessageBox.Show($"Message reçu: Adresse - {e.DestinationAddress}, Valeur - {e.Value}");
            //bool messagerecu = true;
        
            // Crée une nouvelle entrée pour le message reçu
            var newMessage = new GroupMessage
            {
                SourceAddress = e.SourceAddress,
                DestinationAddress = e.DestinationAddress,
                Value = e.Value,
                EventType = e.EventType // Définir le type d'événement si nécessaire
            };

            // Assure-toi que l'ajout à la collection est fait sur le thread du Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Ajoute la nouvelle entrée à la liste observable
                Messages.Add(newMessage);
            });
            
        }



        private async Task SendGroupValuesAsync(List<(GroupAddress addr, GroupValue value)> groupValues)
        {
            if (groupValues == null || !groupValues.Any())
            {
                MessageBox.Show("La liste des valeurs de groupe est vide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_globalViewModel.IsConnected || _globalViewModel.IsBusy)
            {
                MessageBox.Show("Le bus KNX n'est pas connecté ou est occupé. Veuillez réessayer plus tard.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                foreach (var (addr, value) in groupValues)
                {
                    await _globalViewModel._bus.WriteGroupValueAsync(
                        addr, value, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async Task ReadGroupAddressAsync(List<GroupAddress> groupAddresses)
        {
            if (groupAddresses == null)
            {
                MessageBox.Show("La liste des adresses de groupe est nulle.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    foreach (var address in groupAddresses)
                    {
                        var readValue = await _globalViewModel._bus.RequestGroupValueAsync(
                            address, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token);

                        // Traite la valeur lue comme nécessaire
                    }
                }
                else
                {
                    MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.",
                                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la lecture des valeurs de groupe : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //________________________________________________________________________________________________________//

        private async Task GroupValueWriteONAsync()
        {
            
            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    _groupValueone = new GroupValue(true); // Exemple de valeur par défaut
                    await _globalViewModel._bus.WriteGroupValueAsync(
                        _groupAddressone, _groupValueone, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
                    );
                    //Settings.Default.GroupAddress = GroupAddress;
                }
                else
                {
                     MessageBox.Show("Le bus KNX n'est pas connecté. Veuillez vous connecter d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                     return;
                }
                    
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                
            }
            
        }

        private async Task GroupValueWrite0FFAsync()
        {

            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    _groupValueone = new GroupValue(false); // Exemple de valeur par défaut
                    await _globalViewModel._bus.WriteGroupValueAsync(
                        _groupAddressone, _groupValueone, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
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
                MessageBox.Show($"Erreur lors du test de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                
            }

        }
















    }

}

