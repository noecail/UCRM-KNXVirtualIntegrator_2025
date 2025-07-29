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


        /// <summary>
        /// Gets the collection of functional models.
        /// </summary>
        public ObservableCollection<FunctionalModel> Structures { get; set; }
        
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
                if (_selectedModels != null && _selectedModels.Equals(value) && _selectedModels.Count == value.Count)
                    return;
                _selectedModels = value;
                SelectedModel = null; // reset SelectedModel if SelectedModels has changed
                ShowModelColumn(); // Affiche le panneau de modification de modèle fonctionnel
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
                ShowModelColumn(); // Affiche le panneau de modification de modèle fonctionnel
                WhenPropertyChanged(nameof(SelectedModel));
                
            }
        }
        

        /// <summary>
        /// Column 1. Selected model structure.
        /// </summary>  
        private FunctionalModel? _selectedStructure;
        public FunctionalModel? SelectedStructure
        {
            get => _selectedStructure;
            set
            {
                if (_selectedStructure?.Key == value?.Key)
                    return;
                _selectedStructure = value;
                WhenPropertyChanged(nameof(SelectedStructure));
                
                // updating the second column
                SelectedModels = SelectedStructure != null ? _functionalModelList.FunctionalModels[SelectedStructure.Key-1] : null;
                ShowModelColumn(); // Affiche le panneau avec la liste de modèles fonctionnels
                
            }
        }
        
        /// <summary>
        /// Gets or sets the currently selected tested element.
        /// </summary>  
        private FunctionalModel _selectedTestedElement;
        public FunctionalModel SelectedTestedElement
        {
            get => _selectedTestedElement;
            set
            {
                if (_selectedTestedElement.Equals(value)) return;
                _selectedTestedElement = value;
                //ShowModelColumn(); // Affiche le panneau de modification de modèle fonctionnel
                WhenPropertyChanged(nameof(SelectedTestedElement));
                
            }
            // ++ Ajouter notamment tout le mécanisme de sauvegarde des paramètres
        }
        
        
        
    }
}