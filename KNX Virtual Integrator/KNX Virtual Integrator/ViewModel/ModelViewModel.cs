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
        private FunctionalModel _selectedModel;


        /// <summary>
        /// Gets the collection of functional models.
        /// </summary>
        public ObservableCollection<FunctionalModel> Models { get; }

        /// <summary>
        /// Gets or sets the currently selected model.
        /// </summary>  
        //Utile pour connaitre le modèle à afficher en paramètre et potentiellement modifier ces attributs
        public FunctionalModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel.Equals(value)) return;
                _selectedModel = value;
                ShowModelColumn(); // Affiche le panneau de modification de modèle fonctionnel
                WhenPropertyChanged(nameof(SelectedModel));
                
            }
            // ++ Ajouter notamment tout le mécanisme de sauvegarde des paramètres
        }
    }
}