using System.Collections.ObjectModel;
using System.Windows;
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
        /// <summary>
        /// The attribute storing the list of Functional Models
        /// </summary>
        private IFunctionalModelList _functionalModelList;

        /// <summary>
        /// Gets the collection of structure of functional models
        /// to be displayed in the <see cref="View.Windows.MainWindow"/>.
        /// </summary>
        public ObservableCollection<FunctionalModelStructure> Structures { get; }
        
        /// <summary>
        /// List of Models that corresponds to the Selected Structure.
        /// </summary>
        private ObservableCollection<FunctionalModel>? _selectedModels = [];
        /// <summary>
        /// Gets or sets the list of Models that corresponds to the Selected Structure.
        /// </summary>
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
        /// The Selected Model among the <see cref="SelectedModels"/>.
        /// </summary>  
        private FunctionalModel? _selectedModel;
        /// <summary>
        /// Gets or sets the Selected Model among the <see cref="SelectedModels"/>.
        /// </summary>
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
        /// The Selected Structure among the list of Structures of Functional Models.
        /// </summary>  
        private FunctionalModelStructure? _selectedStructure;
        /// <summary>
        /// Gets or sets the Selected Structure among the list of Structures of Functional Models
        /// </summary>  
        public FunctionalModelStructure? SelectedStructure
        {
            get => _selectedStructure;
            set
            {
                if (_selectedStructure?.Model.Key == value?.Model.Key)
                    return;
                _selectedStructure = value;
                WhenPropertyChanged(nameof(SelectedStructure));
                
                // updating the second column
                if (value is null) return;
                var source = _functionalModelList.FunctionalModels[value.Model.Key-1];
                SelectedModels = new ObservableCollection<FunctionalModel>(source);
            }
        }
        
        /// <summary>
        /// The temporary Structure to buffer its modifications in the <see cref="View.Windows.StructureEditWindow"/>.
        /// </summary>
        public FunctionalModelStructure? EditedStructureSave;

        private Visibility _applyChangesErrorMessageVisibility = Visibility.Hidden;
        public Visibility ApplyChangesErrorMessageVisibility
        {
            get => _applyChangesErrorMessageVisibility;
            set
            {
                if (ApplyChangesErrorMessageVisibility == value) return;
                _applyChangesErrorMessageVisibility = value;
                WhenPropertyChanged(nameof(ApplyChangesErrorMessageVisibility));
            }
        }

    }
}