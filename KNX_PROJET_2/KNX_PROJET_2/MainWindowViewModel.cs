using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using Knx.Falcon;
using Knx.Falcon.Sdk;

namespace KNX_PROJET_2
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isBusy;                                       
        private KnxBus _bus;                                        // bus utilisé pour la connexion
        private CancellationTokenSource _cancellationTokenSource;   // token pour pouvoir annuler la connexion à tout moment
        private readonly Dispatcher _dispatcher;

        public MainWindowViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            
        }
        
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }
        
        
        // Utile pour faire une tâche que l'on peut annuler
        public IDisposable StartLongOperation()
        {
            return new LongOperation(this);
        }
        // Tâche annulable avec le ViewModel de la MainWindow
        class LongOperation : IDisposable
        {
            private readonly MainWindowViewModel _viewModel;

            public LongOperation(MainWindowViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModel.IsBusy = true;
                _viewModel._cancellationTokenSource = new CancellationTokenSource();
            }

            public void Dispose()
            {
                _viewModel.IsBusy = false;
                _viewModel._cancellationTokenSource = null;
            }
        }
        
        public BusConnectionState BusConnectionState => _bus?.ConnectionState ?? BusConnectionState.Closed;

        public bool IsBusConnected => BusConnectionState == BusConnectionState.Connected;

        public KnxBus Bus => _bus;
        
        public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

        
        // -------------------------------------------------------- \\
        private string _connectionString;

        public string ConnectionString
        {
            get => _connectionString;
            set => Set(() => ConnectionString, ref _connectionString, value);
        }
        
        private void UpdateConnectionState()
        {
            // Met à jour l'état de la connexion dans l'interface utilisateur
            ConnectionStateTextBlock.Text = IsConnected ? "Connected" : "Disconnected";
        }

        private void BusConnectionStateChanged(object sender, EventArgs e)
        {
            // Cette méthode sera appelée lorsque l'état de la connexion change
            // Mettre à jour l'état de la connexion
            Dispatcher.Invoke(() => UpdateConnectionState());

        }
        public ICommand ConnectBusCommand { get; }

        private async Task ConnectBusAsync()
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

                    var connectorParameters = ConnectorParameters.FromConnectionString(ConnectionString);
                    if (connectorParameters.SupportsSecurity && !string.IsNullOrEmpty(KeyringFile))
                    {
                        await connectorParameters.LoadSecurityDataAsync(KeyringFile, KeyringFilePassword);
                    }

                    var bus = new KnxBus(connectorParameters);
                    await bus.ConnectAsync(_cancellationTokenSource.Token);
                    _bus = bus;
                    if (!string.IsNullOrEmpty(KeyringFile))
                    {
                        bus.GroupCommunicationSecurity = GroupCommunicationSecurity.Load(KeyringFile, KeyringFilePassword);
                        bus.DeviceCommunicationSecurity = DeviceCommunicationSecurity.Load(KeyringFile, KeyringFilePassword, bus.InterfaceConfiguration.IndividualAddress);
                    }

                    _bus.ConnectionStateChanged += BusConnectionStateChanged;
                    _groupCommunication?.BusChanged(null, _bus);
                    Settings.Default.ConnectionString = ConnectionString;

                    RaisePropertyChanged(() => BusConnectionState);
                    RaisePropertyChanged(() => IsBusConnected);
                }
                catch (Exception exc)
                {
                    Exception = new ExceptionViewModel(exc);
                }
            }
        }
    }
}

