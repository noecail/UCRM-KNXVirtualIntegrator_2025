using System.Collections.ObjectModel;
using System.ComponentModel;
using KNXIntegrator.Models;

namespace KNX_Virtual_Integrator.ViewModel
{
public partial class MainViewModel
    { 
    private IFunctionalModelDictionary _functionalModelDictionary;
    private FunctionalModel _selectedModel;
    private string? _newModelName;

        // Collection observable des modèles
        public ObservableCollection<FunctionalModel> Models { get; }

        public FunctionalModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    ShowModelColumn();
                    OnPropertyChanged(nameof(SelectedModel));
                    NewModelName = _selectedModel?.Name; // Met à jour le nom dans le TextBox
                }
            }
        }

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

}

