using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using KNX_Virtual_Integrator.Model.Entities;

namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Interface for managing a dictionary of functional models (FunctionalModel).
    ///
    /// Provides methods to add, remove, retrieve, and update models in the dictionary.
    /// Each model is identified by a unique key (int). This interface enables centralized 
    /// management of functional models, allowing standardized operations on the dictionary.
    /// 
    /// - AddFunctionalModel: Adds a functional model to the dictionary.
    /// - RemoveFunctionalModel: Removes a functional model using its key.
    /// - GetAllModels: Retrieves all functional models from the dictionary.
    /// </summary>
    public interface IFunctionalModelDictionary
    {
        void AddFunctionalModel(FunctionalModel functionalModel);

        void RemoveFunctionalModel(int index);

        List<FunctionalModel> GetAllModels();
        
        public ObservableCollection<FunctionalModel> FunctionalModels {get;set;}
        
        public event PropertyChangedEventHandler? PropertyChanged;

        
        public void ImportDictionary(string path);
        
        public void ExportDictionary(string path);

    }
}

