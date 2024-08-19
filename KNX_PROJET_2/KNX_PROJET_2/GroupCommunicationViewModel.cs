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

        
        
        private GroupAddress _groupAddress;
        public GroupAddress GroupAddress
        {
            get => _groupAddress;
            set => Set(() => GroupAddress, ref _groupAddress, value);
        }

        private GroupValue _groupValue;
        public GroupValue GroupValue
        {
            get => _groupValue;
            set => Set(() => GroupValue, ref _groupValue, value);
        }

        
        
        public ICommand GroupValueWriteCommand { get; set; }
        public ICommand GroupValueWrite0Command { get; set; }

        public GroupCommunicationViewModel(MainViewModel globalViewModel)
        {
            _globalViewModel = globalViewModel;

            _groupAddress = new GroupAddress("0/1/1"); // Exemple d'adresse par défaut
            

            GroupValueWriteCommand = new RelayCommand(async () => await GroupValueWriteAsync());
            GroupValueWrite0Command = new RelayCommand(async () => await GroupValueWrite0Async());

        }

        private async Task GroupValueWriteAsync()
        {
            
            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    _groupValue = new GroupValue(true); // Exemple de valeur par défaut
                    await _globalViewModel._bus.WriteGroupValueAsync(
                        _groupAddress, _groupValue, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
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

        private async Task GroupValueWrite0Async()
        {

            try
            {
                if (_globalViewModel.IsConnected && !_globalViewModel.IsBusy)
                {
                    _groupValue = new GroupValue(false); // Exemple de valeur par défaut
                    await _globalViewModel._bus.WriteGroupValueAsync(
                        _groupAddress, _groupValue, MessagePriority.High, _globalViewModel._cancellationTokenSource.Token
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

