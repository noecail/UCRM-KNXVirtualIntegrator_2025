using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.ViewModel;

public partial class MainViewModel
{

    public List<List<List<List<ResultType>>>> LastTestResults=[];
    
    /// <summary>
    /// Gets or sets the currently selected tested element.
    /// </summary>  
    private ObservableCollection<FunctionalModel> _selectedTestModels = [];
    public ObservableCollection<FunctionalModel> SelectedTestModels
    {
        get => _selectedTestModels;
        set
        {
            if (_selectedTestModels.Equals(value)) return;
            _selectedTestModels = value;
            WhenPropertyChanged(nameof(SelectedTestModels));
        }
    }
    
    /// <summary>
    /// Column 1. Selected model structure.
    /// </summary>  
    private FunctionalModel? _selectedStructureTestWindow;
    public FunctionalModel? SelectedStructureTestWindow
    {
        get => _selectedStructureTestWindow;
        set
        {
            if (_selectedStructureTestWindow?.Key == value?.Key)
                return;
            _selectedStructureTestWindow = value;
            WhenPropertyChanged(nameof(SelectedStructureTestWindow));
            
            // updating the second column
            SelectedModelsTestWindow = SelectedStructureTestWindow != null ? _functionalModelList.FunctionalModels[SelectedStructureTestWindow.Key-1] : null;
        }
    }
    
    
    /// <summary>
    /// Not seen on the UI
    /// List of Models that corresponds to the Selected Structure
    /// </summary>
    private ObservableCollection<FunctionalModel>? _selectedModelsTestWindow = [];
    public ObservableCollection<FunctionalModel>? SelectedModelsTestWindow
    {
        get => _selectedModelsTestWindow;
        set
        {
                    
            if (_selectedModelsTestWindow != null && _selectedModelsTestWindow.Equals(value))
                return;
            _selectedModelsTestWindow = value;
            SelectedModelTestWindow = null; // reset the selected model if selectedModels has changed
            WhenPropertyChanged(nameof(SelectedModelsTestWindow));
                
        }
    }
    
    /// <summary>
    /// Column 2. Selected Model among the Selected Models
    /// </summary>  
    private FunctionalModel? _selectedModelTestWindow;
    public FunctionalModel? SelectedModelTestWindow
    {
        get => _selectedModelTestWindow;
        set
        {
                
            if (_selectedModelTestWindow != null && _selectedModelTestWindow.Key == value?.Key)
                return;
            _selectedModelTestWindow = value;
            WhenPropertyChanged(nameof(SelectedModelTestWindow));
                
        }
    }

    public void AddStructToTestModels(int structKey)
    {
        foreach (var testedModel in _functionalModelList.FunctionalModels[structKey])
        {
            if (SelectedTestModels.Contains(testedModel))
                continue;
            SelectedTestModels.Add(testedModel);
        }
    }

    public void RmvStructFromTestModels(int structKey)
    {
        foreach (var testedModel in _functionalModelList.FunctionalModels[structKey])
        {
            SelectedTestModels.Remove(testedModel);
        }
    }
    
}