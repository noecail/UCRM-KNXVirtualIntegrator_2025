using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.ViewModel;

public partial class MainViewModel
{
    /// <summary>
    /// Default at 2000 ms and set when calling <see cref="Model.Implementations.Analyze.TestAll"/>.
    /// Used to set an all around timeout for commands.
    /// </summary>
    public int CommandTimeout { get; set; } = 2000;

    /// <summary>
    /// Default at 0 ms and set when calling <see cref="Model.Implementations.Analyze.TestAll"/>.
    /// Used to space out tests, to not saturate the installation.
    /// </summary>
    public int ElementLatency { get; set; } = 0;

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
    /// Collection used to display the chosen test models along with various information.
    /// </summary>
    public ObservableCollection<TestedFunctionalModel> ChosenModelsAndState { get; set; } = [];
    
    /// <summary>
    /// Clears all the lists used to hold the functional models and resets the timeout and latency.
    /// </summary>
    public void ClearModelsToTestAndResetTimes()
    {
        CommandTimeout = 2000;
        ElementLatency = 0;
        SelectedTestModels.Clear();
        ChosenModelsAndState.Clear();
    }

    private string _analysisErrorMessage;
    /// <summary>
    /// Error message for an eventual error on Launch Analysis
    /// </summary>
    public string AnalysisErrorMessage
    {
        get => _analysisErrorMessage;
        set
        {
            if (_analysisErrorMessage == value) return;
            _analysisErrorMessage = value;
            WhenPropertyChanged(nameof(AnalysisErrorMessage));
        }
    }
}

/// <summary>
/// Class handling the grouping of various information. It is used to display the chosen test models,
/// the state of their analysis and handle window theme changes.
/// </summary>
public class TestedFunctionalModel
{
    /// <summary>
    /// The chosen functional model. Linked in some ways with <see cref="ViewModel.MainViewModel.SelectedTestModels"/>.
    /// </summary>
    public FunctionalModel FunctionalModel { get; set; }
    /// <summary>
    /// Status of the model analysis. It has 3 (or 4) states : "", "Waiting", "Running", "Finished".
    /// Except for "", each state is associated with a corresponding image in the UI.
    /// "" is associated with a collapsed image.
    /// </summary>
    public string State;
    /// <summary>
    /// The theme of the App. <seealso cref="ApplicationSettings.EnableLightTheme"/>
    /// It is changed when the collection <see cref="ViewModel.MainViewModel.ChosenModelsAndState"/> is
    /// either updated or there is a new TestedFunctionalModel.
    /// </summary>
    public bool LightTheme;
    /// <summary>
    /// Constructor with only a functional model
    /// </summary>
    /// <param name="functionalModel">The functional model to test</param>
    public TestedFunctionalModel(FunctionalModel functionalModel)
    {
        FunctionalModel = functionalModel;
        State = "";
        LightTheme = true;
    }
    /// <summary>
    /// Constructor handling the theme
    /// </summary>
    /// <param name="functionalModel">The functional model to test</param>
    /// <param name="lightTheme">The current app theme</param>
    public TestedFunctionalModel(FunctionalModel functionalModel, bool lightTheme)
    {
        FunctionalModel = functionalModel;
        State = "";
        LightTheme = lightTheme;
    }
    /// <summary>
    /// Override to only test the equality of the <see cref="FunctionalModel"/>.
    /// Done to reduces issues when adding or checking the chosen test models.
    /// </summary>
    /// <param name="obj">The compared object (should be a TestedFunctionalModel)</param>
    /// <returns>True if the FunctionalModels are equal; false otherwise.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is  TestedFunctionalModel functionalModel)
            return FunctionalModel.Equals(functionalModel.FunctionalModel);
        return false;
    }
    /// <summary>
    /// Override to allow Equals to work as intended.
    /// </summary>
    /// <returns>The <see cref="FunctionalModel"/> <see cref="FunctionalModel.GetHashCode"/> .</returns>
    public override int GetHashCode()
    {
        return FunctionalModel.GetHashCode();
    }
}
