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

        //________________________________________________________________________________________________________//
        public ICommand GroupValueWriteONCommand { get; set; }
        public ICommand GroupValueWrite0FFCommand { get; set; }

        public ICommand GroupValueReadCommand { get; set; }
        public ICommand GroupValueWriteCommand { get; set; }

        public ICommand SendGroupValuesCommand { get; set; }

        //________________________________________________________________________________________________________//

        public GroupCommunicationViewModel(MainViewModel globalViewModel)
        {
            _globalViewModel = globalViewModel;

            _groupAddressone = new GroupAddress("0/1/1"); // Exemple d'adresse par défaut
            GroupValueWriteONCommand = new RelayCommand(async () => await GroupValueWriteONAsync());
            GroupValueWrite0FFCommand = new RelayCommand(async () => await GroupValueWrite0FFAsync());


            GroupValueReadCommand = new RelayCommand(
                async () => await GroupValueReadAsync(), () => _globalViewModel.IsConnected && !_globalViewModel.IsBusy);

            GroupValueWriteCommand = new RelayCommand(
                async () => await GroupValueWriteAsync(), () => _globalViewModel.IsConnected && !_globalViewModel.IsBusy);

            SendGroupValuesCommand = new RelayCommand<object>(async (parameter) => 
            await SendGroupValuesAsync(parameter as List<(GroupAddress, GroupValue)>));

            // Initialisation de la liste GroupValues
            GroupValues = new List<(GroupAddress, GroupValue)>
                {
                    (new GroupAddress("0/1/1"), new GroupValue(true)),
                    (new GroupAddress("1/0/1"), new GroupValue(true)),
                    
                };

            //Initialisation @ de groupe + GroupValue = type booleen 1 bit
            //_groupAddress = new GroupAddress("0/1/2"); // Exemple d'adresse par défaut
            _groupValue = new GroupValueViewModel(new GroupValue(false));
            //EST CE QUE CA SERT A QQCHOSE DE METTRE PAR DEFAUT ?
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


        //fonction a verifier plus tard = creer endroit ou lire les trames
        private async Task GroupValueReadAsync()
        {

            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    await _globalViewModel._bus.RequestGroupValueAsync(
                        GroupAddress, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
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
                switch (ex)
                {
                    case NullReferenceException _ when GroupAddress == "0/0/0":
                        MessageBox.Show("L'adresse de groupe est nulle. Veuillez remplir le champ d'adresse de groupe.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    default:
                        MessageBox.Show($"Erreur lors de l'envoi de la trame : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }

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

