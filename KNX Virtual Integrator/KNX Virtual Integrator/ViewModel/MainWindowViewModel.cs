using System.Xml.Linq;

namespace KNX_Virtual_Integrator.ViewModel
{
    /// <summary>
    /// ViewModel for managing the layout and commands in the MainWindow.
    ///
    /// This class handles:
    /// - The box width in the TestedElements
    /// - The address file used to display addresses in the Main Window
    /// 
    /// Note: The `RelayCommand` instances are initialized in the constructor of `MainViewModel`.
    /// </summary>

    public partial class MainViewModel
    {
        /// <summary>
        /// The width of the boxes in the <see cref="Model.Entities.TestedElement"/> listBoxes (displayed element)
        /// </summary>
        public static int BoxWidth => 40;
    
        /// <summary>
        /// The group address file used to display the addresses in the <see cref="View.Windows.MainWindow"/>
        /// </summary>
        public XDocument? GroupAddressFile { get; private set; }
    }
}