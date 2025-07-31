using System.Windows;
using CommunityToolkit.Mvvm.Input;
using KNX_Virtual_Integrator.Model.Entities;


namespace KNX_Virtual_Integrator.ViewModel
{
    /// <summary>
    /// ViewModel for managing the layout and commands in the MainWindow.
    ///
    /// This class handles:
    /// - Column width settings for the model and address columns (`ModelColumnWidth` and `AddressColumnWidth`).
    /// - Commands to show or hide these columns (`HideModelColumnCommand`, `HideAddressColumnCommand`, `ShowModelColumnCommand`, `ShowAddressColumnCommand`).
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
                WhenPropertyChanged(nameof(ModelColumnWidth)); // Notify of property change
            }
        }

        private GridLength _adressColumnWidth = new GridLength(1, GridUnitType.Auto);
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
                WhenPropertyChanged(nameof(AdressColumnWidth)); // Notify of property change
            }
        }


    }
}