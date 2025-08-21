using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Interfaces;
using KNX_Virtual_Integrator.Model.Entities;


namespace KNX_Virtual_Integrator.ViewModel
{
    /// <summary>
    /// ViewModel for managing functional models in the system.
    ///
    /// This class holds attributes and methods related to functional models, including:
    /// - A dictionary for storing functional models (`IFunctionalModelDictionary`).
    /// - The currently selected model (`SelectedModel`).
    /// 
    /// It provides an observable collection (`Models`) to track and manage a list of models,
    /// and includes properties for selecting a model and updating its name. 
    /// Property changes are tracked using `INotifyPropertyChanged` to update the UI dynamically.
    /// 
    /// Note: The `RelayCommand` instances are initialized in the constructor of `MainViewModel`.
    /// </summary>

    public partial class MainViewModel
    {
        private IFunctionalModelList _functionalModelList;

        private int _boxWidth = 40;
        public int BoxWidth{
            get => _boxWidth;
        }


        /// <summary>
        /// Gets the collection of functional models.
        /// </summary>
        public ObservableCollection<FunctionalModelStructure> Structures { get; set; }
        
        /// <summary>
        /// Not seen on the UI
        /// List of Models that corresponds to the Selected Structure
        /// </summary>
        private ObservableCollection<FunctionalModel>? _selectedModels = [];
        public ObservableCollection<FunctionalModel>? SelectedModels
        {
            get => _selectedModels;
            set
            {
                if (_selectedModels != null && value != null && _selectedModels.Equals(value) && _selectedModels.Count == value.Count)
                    return;
                _selectedModels = value;
                
                SelectedModel = null;
                WhenPropertyChanged(nameof(SelectedModel));
                WhenPropertyChanged(nameof(SelectedModels)); 
            }
        }
        
        /// <summary>
        /// Column 2. Selected Model among the Selected Models
        /// </summary>  
        private FunctionalModel? _selectedModel;
        public FunctionalModel? SelectedModel
        {
            get => _selectedModel;
            set
            {
                
                if (_selectedModel != null && _selectedModel.Key == value?.Key)
                    return;
                _selectedModel = value;
                WhenPropertyChanged(nameof(SelectedModel));
                
            }
        }
        

        /// <summary>
        /// Column 1. Selected model structure.
        /// </summary>  
        private FunctionalModelStructure? _selectedStructure;
        public FunctionalModelStructure? SelectedStructure
        {
            get => _selectedStructure;
            set
            {
                if (_selectedStructure?.Model.Key == value?.Model.Key)
                    return;
                /*var key = _selectedStructure.Model.Key;
                _functionalModelList.FunctionalModelDictionary.FunctionalModels[key-1] = new FunctionalModelStructure(_selectedStructure); 
                _functionalModelList.FunctionalModelDictionary.FunctionalModels[key-1].Model.Key = key;*/
                //Console.WriteLine("key "+value?.Model.Key);
                _selectedStructure = value;
                WhenPropertyChanged(nameof(SelectedStructure));
                SelectedStructureModel = SelectedStructure?.Model;
                
                // updating the second column
                if (value is null) return;
                var source = _functionalModelList.FunctionalModels[value.Model.Key-1];
                SelectedModels = new ObservableCollection<FunctionalModel>(source);
                
                
                //SelectedModel = SelectedModels?.First(); //bonne idée mais rallonge le temps d'affichage alors que parfois on veut simplement balayer les structures
            }
        }
        
        /// <summary>
        /// StructureEditWindow and Column 2. Name of the selected model structure.
        /// </summary>
        private FunctionalModel? _selectedStructureModel;
        public FunctionalModel? SelectedStructureModel
        {
            get => _selectedStructureModel;
            set
            {
                if (_selectedStructureModel != null && _selectedStructureModel.Key == value?.Key)
                    return;
                _selectedStructureModel = value;
                WhenPropertyChanged(nameof(SelectedStructureModel));
            }
        }
    }
}