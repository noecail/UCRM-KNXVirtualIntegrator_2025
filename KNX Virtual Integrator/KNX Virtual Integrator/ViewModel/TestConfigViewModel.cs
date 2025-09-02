using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.ViewModel;

public partial class MainViewModel
{
    /// <summary>
    /// The full list of all the test results.
    /// It is structured by : Structures -> Models -> Test Elements ->
    /// Commands (value lines in Test CMD) -> Receptions (value column of Test IE).
    /// <seealso cref="ResultType"/> 
    /// </summary>  
    public List<List<List<List<ResultType>>>> LastTestResults=[];

    /// <summary>
    /// The list of the model current analysis state.
    /// It can have 4 states : Waiting, Running, Finished, None.
    /// "None" means that the correspondingly displayed image is hidden.
    /// </summary>
    public List<string> AnalysisState = [];

    /// <summary>
    /// The models selected to be tested.
    /// </summary>  
    private ObservableCollection<FunctionalModel> _selectedTestModels = [];
    /// <summary>
    /// Gets or sets the models selected to be tested.
    /// </summary>  
    public ObservableCollection<FunctionalModel> SelectedTestModels
    {
        get => _selectedTestModels;
        set
        {
            if (_selectedTestModels.Equals(value)) return;
            _selectedTestModels = value;
            ChosenModelsAndState = new ObservableCollection<TestedFunctionalModel>();
            foreach (var model in value)
                ChosenModelsAndState.Add(new TestedFunctionalModel(model,AppSettings.EnableLightTheme));
            WhenPropertyChanged(nameof(SelectedTestModels));
        }
    }
    
    /// <summary>
    /// Selected structure of functional models.
    /// </summary>  
    private FunctionalModelStructure? _selectedStructureTestWindow;
    /// <summary>
    /// Gets or sets the selected structure of functional models.
    /// </summary>
    public FunctionalModelStructure? SelectedStructureTestWindow
    {
        get => _selectedStructureTestWindow;
        set
        {
            if (_selectedStructureTestWindow?.Model.Key == value?.Model.Key)
                return;
            _selectedStructureTestWindow = value;
            WhenPropertyChanged(nameof(SelectedStructureTestWindow));
            
            // updating the second column
            SelectedModelsTestWindow = SelectedStructureTestWindow != null ? _functionalModelList.FunctionalModels[SelectedStructureTestWindow.Model.Key-1] : null;
        }
    }
    
    /// <summary>
    /// List of Models that corresponds to the Selected Structure.
    /// </summary>
    private ObservableCollection<FunctionalModel>? _selectedModelsTestWindow = [];
    /// <summary>
    /// Gets or sets the list of <see cref="FunctionalModel"/> that corresponds to the <see cref="SelectedStructureTestWindow"/>.
    /// </summary>
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
    /// The selected Model among the Selected Models.
    /// </summary>  
    private FunctionalModel? _selectedModelTestWindow;
    /// <summary>
    /// Gets or sets the selected Model among the <see cref="SelectedModelsTestWindow"/>.
    /// </summary>  
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
    
    /// <summary>
    /// Adds all the <see cref="FunctionalModel"/> of the Structure to the <see cref="SelectedTestModels"/>
    /// for them to be tested.
    /// </summary>
    /// <param name="structKey">The index of the structure in the functionalModelList.</param>
    public void AddStructToTestModels(int structKey)
    {
        foreach (var testedModel in _functionalModelList.FunctionalModels[structKey])
        {
            if (SelectedTestModels.Contains(testedModel))
                continue;
            SelectedTestModels.Add(testedModel);
            if (ChosenModelsAndState.Contains(new TestedFunctionalModel(testedModel))) 
                continue;
            ChosenModelsAndState.Add(new TestedFunctionalModel(testedModel, AppSettings.EnableLightTheme));
        }
    }

    /// <summary>
    /// Removes all the <see cref="FunctionalModel"/> of the Structure from the <see cref="SelectedTestModels"/>
    /// for them to not be tested anymore if they were supposed to.
    /// </summary>
    /// <param name="structKey">The index of the structure in the functionalModelList.</param>
    public void RmvStructFromTestModels(int structKey)
    {
        foreach (var testedModel in _functionalModelList.FunctionalModels[structKey])
        {
            SelectedTestModels.Remove(testedModel);
            ChosenModelsAndState.Remove(new TestedFunctionalModel(testedModel));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<TestedFunctionalModel> ChosenModelsAndState { get; set; } = [];

}

/// <summary>
/// 
/// </summary>
public class TestedFunctionalModel
{
    /// <summary>
    /// 
    /// </summary>
    public FunctionalModel FunctionalModel { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string State;
    /// <summary>
    /// 
    /// </summary>
    public bool LightTheme;

    public TestedFunctionalModel()
    {
        FunctionalModel = new FunctionalModel("b");
        State = "";
        LightTheme = true;
    }
    public TestedFunctionalModel(FunctionalModel functionalModel, string state)
    {
        FunctionalModel = functionalModel;
        State = state;
        LightTheme = true;
    }

    public TestedFunctionalModel(FunctionalModel functionalModel)
    {
        FunctionalModel = functionalModel;
        State = "";
        LightTheme = true;
    }

    public TestedFunctionalModel(FunctionalModel functionalModel, string state, bool lightTheme)
    {
        FunctionalModel = functionalModel;
        State = state;
        LightTheme = lightTheme;
    }
    public TestedFunctionalModel(FunctionalModel functionalModel, bool lightTheme)
    {
        FunctionalModel = functionalModel;
        State = "";
        LightTheme = lightTheme;
    }
    public TestedFunctionalModel(bool lightTheme)
    {
        FunctionalModel = new FunctionalModel("b");
        State = "";
        LightTheme = lightTheme;
    }
}
