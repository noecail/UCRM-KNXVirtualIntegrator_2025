using System.Collections.Generic;

namespace KNXIntegrator.Models
{
    /// <summary>
    /// Interface for managing a dictionary of functional models (FunctionalModel).
    ///
    /// Provides methods to add, remove, retrieve, and update models in the dictionary.
    /// Each model is identified by a unique key (int). This interface enables centralized 
    /// management of functional models, allowing standardized operations on the dictionary.
    /// 
    /// - Add_FunctionalModel: Adds a functional model to the dictionary.
    /// - Remove_FunctionalModel: Removes a functional model using its key.
    /// - GetAllModels: Retrieves all functional models from the dictionary.
    /// </summary>
    public interface IFunctionalModelDictionary
    {
        void Add_FunctionalModel(FunctionalModel functionalModel);

        void Remove_FunctionalModel(int key);

        List<FunctionalModel> GetAllModels();

    }
}

