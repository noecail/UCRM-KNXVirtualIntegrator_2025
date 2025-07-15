using KNX_Virtual_Integrator.Model.Entities;

namespace KNX_Virtual_Integrator.Model.Interfaces;


/// <summary>
/// Interface for managing a list of functional models (FunctionalModel).
///
/// Provides methods to add, remove, retrieve, and update models in the list.
/// Each model is identified by a unique key (int). This interface enables centralized 
/// management of functional models, allowing standardized operations on the dictionary.
/// 
/// - Add_FunctionalModel: Adds a functional model to the dictionary.
/// - Remove_FunctionalModel: Removes a functional model using its key.
/// - GetAllModels: Retrieves all functional models from the dictionary.
/// </summary>
public interface IFunctionalModelList
{
    public void ExportDictionary(string path);
    public List<FunctionalModel> GetAllModels();
    public void AddToDictionary(FunctionalModel model);
    public void DeleteFromDictionary(int index);

}