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
        public static int BoxWidth => 40;
    }
}