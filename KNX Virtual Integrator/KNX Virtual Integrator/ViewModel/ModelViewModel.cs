using System.Collections.ObjectModel;
using KNXIntegrator.Models;

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
        private IFunctionalModelDictionary _functionalModelDictionary;
        private FunctionalModel _selectedModel;


        /// <summary>
        /// Gets the collection of functional models.
        /// </summary>
        public ObservableCollection<FunctionalModel> Models { get; }

        /// <summary>
        /// Gets or sets the currently selected model.
        /// </summary>  
        //Utile pour connaitre le modèle a afficher en paramètre et potentiellement modifier ces attributs
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
                }
            }
            // ++ Ajouter nottement tout le mecanisme de sauvegarde des paramètres

        }
    }
}