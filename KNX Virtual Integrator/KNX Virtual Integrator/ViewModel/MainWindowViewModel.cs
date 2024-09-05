using System.Windows;
using GalaSoft.MvvmLight.Command;

namespace KNX_Virtual_Integrator.ViewModel;

/// <summary>
/// Represents the main view model for managing the user interface and commands in the application.
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

    // Model saving management

    /// <summary>
    /// Gets the command to save the current model.
    /// </summary>
    public RelayCommand SaveCommand { get; }

    /// <summary>
    /// Executes the save command, updating the selected model's name and saving it to the functional model dictionary.
    /// </summary>
    private void ExecuteSaveCommand()
    {
        SelectedModel.Name = "nouveau nom"; // Updates the name of the selected model

        _functionalModelDictionary.UpdateModel(SelectedModel);
        OnPropertyChanged(nameof(SelectedModel));
    }
}