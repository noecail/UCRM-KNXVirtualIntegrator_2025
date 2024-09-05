using System.Collections.ObjectModel;
using KNXIntegrator.Models;

namespace KNX_Virtual_Integrator.ViewModel;

/// <summary>
/// Represents the main view model for managing models and their properties in the application.
/// </summary>
public partial class MainViewModel
{
    private IFunctionalModelDictionary _functionalModelDictionary;
    private FunctionalModel _selectedModel;
    private string? _newModelName;

    /// <summary>
    /// Gets the collection of functional models.
    /// </summary>
    public ObservableCollection<FunctionalModel> Models { get; }

    /// <summary>
    /// Gets or sets the currently selected model.
    /// </summary>
    public FunctionalModel SelectedModel
    {
        get => _selectedModel;
        set
        {
            if (_selectedModel.Equals(value)) return;
            _selectedModel = value;
            ShowModelColumn();
            OnPropertyChanged(nameof(SelectedModel));
            NewModelName = _selectedModel.Name; // Updates the name in the TextBox
        }
    }

    /// <summary>
    /// Gets or sets the name of the new model.
    /// </summary>
    public string? NewModelName
    {
        get => _newModelName;
        set
        {
            if (_newModelName != value)
            {
                _newModelName = value;
                OnPropertyChanged(nameof(NewModelName));
            }
        }
    }
}