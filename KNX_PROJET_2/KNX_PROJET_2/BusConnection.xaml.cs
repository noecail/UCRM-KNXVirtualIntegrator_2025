using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Knx.Falcon;
using Knx.Falcon.Configuration;
using Knx.Falcon.DataSecurity;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon.Sdk;

namespace KNX_PROJET_2
{
    /// <summary>
    /// Logique d'interaction pour BusConnection.xaml
    /// </summary>
    public partial class BusConnection : UserControl
    {
        private bool _isBusy;    //operande en cours ?
        private KnxBus _bus;    //objet connexion a un bus
        private CancellationTokenSource _cancellationTokenSource;      //pour annuler les operations en cours 
        private readonly Dispatcher _dispatcher;    //pour execution des taches sur le thread utilisateur
        private BusConnection _busConnection;    //gérer les aspects de la communication de groupe dans le bus KNX

        public BusConnection()
        {
            InitializeComponent();

            _dispatcher = Dispatcher.CurrentDispatcher; //initialisation du thread = on peut utiliser opérations sur le thread ^m si ces operations sont déclenchées en arrière plan


            ConnectBusCommand = new RelayCommand(
                async () => await ConnectBusAsync(),            //Action à exécuter : FALCON établit une connexion au bus KNX de manière asynchrone
                () => !IsBusy && !string.IsNullOrEmpty(ConnectionString) && !IsBusConnected);   //Condition : application pas occupée/chaine de connexion pas vide/bus pas deja connecte
            DisconnectBusCommand = new RelayCommand(
                async () => await DisconnectBusAsync(),         //Action à exécuter : déconnecte bus KNX de manière asynchrone
                () => !IsBusy && IsBusConnected);               //Condition : application pas occupée/bus bien connecte
                              
            _connectionString = Settings.Default.ConnectionString;
            //Initialiser par defaut ConnectionString = Type en USB (type = string)
            //ConnectionString servira plus tard à se connecter au bus (et donc plus tard avoir ConnectorParameters = TypeUSB)
        }


        //definition des membres privés
        private string _connectionString;

        //avoir le ConnexionString qu'a donné l'utilisateur
        public string ConnectionString
        {
            get => _connectionString;
            set => Set(() => ConnectionString, ref _connectionString, value);
        }

        public ICommand ConnectBusCommand { get; }  //établir une connexion avec le bus - utilisateur

        //TACHE POUR EFFECTUER UNE CONNEXION AU BUS
        //ATENTION important = methode asynchrone pour pas bloquer le thread principal
        private async Task ConnectBusAsync()
        {
            using (StartLongOperation())
            {
                try
                {
                    if (_bus != null)   //regarde si le bus n'est pas deja connecté
                    {   //si c'est le cas : on met a jour 
                        _bus.ConnectionStateChanged -= BusConnectionStateChanged;
                        _groupCommunication?.BusChanged(_bus, null);    //le nouveau bus sera null
                        await _bus.DisposeAsync();  //on dispose = on sort de la connection au bus
                        _bus = null;
                        RaisePropertyChanged(() => BusConnectionState);         //on informe l'utilisateur/pour interface/ 
                        RaisePropertyChanged(() => IsBusConnected);             //que les valeurs de connection ont changé
                    }

                    var connectorParameters = ConnectorParameters.FromConnectionString(ConnectionString);
                    if (connectorParameters.SupportsSecurity && !string.IsNullOrEmpty(KeyringFile))
                    {
                        await connectorParameters.LoadSecurityDataAsync(KeyringFile, KeyringFilePassword);
                    }

                    //CONNEXION !!!
                    var bus = new KnxBus(connectorParameters);
                    await bus.ConnectAsync(_cancellationTokenSource.Token);
                    _bus = bus;



                    if (!string.IsNullOrEmpty(KeyringFile))
                    {
                        bus.GroupCommunicationSecurity = GroupCommunicationSecurity.Load(KeyringFile, KeyringFilePassword);
                        bus.DeviceCommunicationSecurity = DeviceCommunicationSecurity.Load(KeyringFile, KeyringFilePassword, bus.InterfaceConfiguration.IndividualAddress);
                    }


                    //mise a jour pour dire que le bus a ete etablit/utilisateur
                    _bus.ConnectionStateChanged += BusConnectionStateChanged;
                    _groupCommunication?.BusChanged(null, _bus);
                    Settings.Default.ConnectionString = ConnectionString;       //ici au lieu de mettre _bus=null comme précédemment
                                                                                //redefinition du defaut
                    RaisePropertyChanged(() => BusConnectionState);
                    RaisePropertyChanged(() => IsBusConnected);
                }
                catch (Exception exc)
                {
                    Exception = new ExceptionViewModel(exc);
                }
            }
        }

        public ICommand DisconnectBusCommand { get; }   //déconnexion avec le bus - utilisateur

        //TACHE POUR EFFECTUER UNE DECONNEXION AU BUS  - voir connexion 1ere partie
        private async Task DisconnectBusAsync()
        {
            using (StartLongOperation())
            {
                try
                {
                    if (_bus != null)
                    {
                        _bus.ConnectionStateChanged -= BusConnectionStateChanged;
                        _groupCommunication?.BusChanged(_bus, null);
                        await _bus.DisposeAsync();
                        _bus = null;
                        RaisePropertyChanged(() => BusConnectionState);
                        RaisePropertyChanged(() => IsBusConnected);
                    }
                }
                catch (Exception exc)
                {
                    Exception = new ExceptionViewModel(exc);
                }
            }
        }

        private void BusConnectionStateChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => BusConnectionState);
            RaisePropertyChanged(() => IsBusConnected);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
