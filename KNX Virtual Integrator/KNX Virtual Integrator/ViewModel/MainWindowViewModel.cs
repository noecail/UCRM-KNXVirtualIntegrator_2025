using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using KNX_Virtual_Integrator.Model;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.View;
using KNX_Virtual_Integrator.ViewModel.Commands;
using ICommand = KNX_Virtual_Integrator.ViewModel.Commands.ICommand;
using System.ComponentModel;
using System.Diagnostics;


namespace KNX_Virtual_Integrator.ViewModel
{
    /// <summary>
    /// ViewModel for managing the layout and commands in the MainWindow.
    ///
    /// This class handles:
    /// - Column width settings for the model and address columns (`ModelColumnWidth` and `AdressColumnWidth`).
    /// - Commands to show or hide these columns (`HideModelColumnCommand`, `HideAdressColumnCommand`, `ShowModelColumnCommand`, `ShowAdressColumnCommand`).
    /// 
    /// It provides properties to control the visibility and size of columns in the MainWindow,
    /// and commands to toggle their visibility. Property changes are tracked using `INotifyPropertyChanged`,
    /// allowing the UI to update dynamically based on user interactions.
    /// 
    /// Note: The `RelayCommand` instances are initialized in the constructor of `MainViewModel`.
    /// </summary>

    public partial class MainViewModel
    {
        // Column management
        private GridLength _modelColumnWidth = new GridLength(0);
        private GridLength _adressColumnWidth = new GridLength(1, GridUnitType.Auto);

        /// <summary>
        /// Gets or sets the width of the model column.
        /// </summary>
        public GridLength ModelColumnWidth
        {
            get => _modelColumnWidth;
            set
            {
                if (_modelColumnWidth == value) return;
                _modelColumnWidth = value;
                OnPropertyChanged(nameof(ModelColumnWidth)); // Notify of property change
            }
        }

        /// <summary>
        /// Gets or sets the width of the address column.
        /// </summary>
        public GridLength AdressColumnWidth
        {
            get => _adressColumnWidth;
            set
            {
                if (_adressColumnWidth == value) return;
                _adressColumnWidth = value;
                OnPropertyChanged(nameof(AdressColumnWidth)); // Notify of property change
            }
        }

        /// <summary>
        /// Gets the command to hide the model column.
        /// </summary>
        public RelayCommand HideModelColumnCommand { get; private set; }

        /// <summary>
        /// Gets the command to hide the address column.
        /// </summary>
        public RelayCommand HideAdressColumnCommand { get; private set; }

        /// <summary>
        /// Gets the command to show the model column.
        /// </summary>
        public RelayCommand ShowModelColumnCommand { get; private set; }

        /// <summary>
        /// Gets the command to show the address column.
        /// </summary>
        public RelayCommand ShowAdressColumnCommand { get; private set; }

        /// <summary>
        /// Hides the model column by setting its width to zero.
        /// </summary>
        private void HideModelColumn()
        {
            ModelColumnWidth = new GridLength(0);
        }

        /// <summary>
        /// Hides the address column by setting its width to zero.
        /// </summary>
        private void HideAdressColumn()
        {
            AdressColumnWidth = new GridLength(0);
        }

        /// <summary>
        /// Shows the model column by setting its width to auto.
        /// </summary>
        private void ShowModelColumn()
        {
            ModelColumnWidth = GridLength.Auto;
        }

        /// <summary>
        /// Shows the address column by setting its width to auto.
        /// </summary>
        private void ShowAdressColumn()
        {
            AdressColumnWidth = GridLength.Auto;
        }

    }
}