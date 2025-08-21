using System.Collections.ObjectModel;
using System.ComponentModel;
using KNX_Virtual_Integrator.Model.Entities;

namespace KNX_Virtual_Integrator.Model.Interfaces;


/// <summary>
/// Interface for managing a list of functional models (FunctionalModel).
///
/// Provides methods to add, remove, retrieve, and update models in the list.
/// Each model is identified by a unique key (int). This interface enables centralized 
/// management of functional models, allowing standardized operations on the dictionary.
/// 
/// - AddFunctionalModel: Adds a functional model to the dictionary.
/// - RemoveFunctionalModel: Removes a functional model using its key.
/// - GetAllModels: Retrieves all functional models from the dictionary.
/// </summary>
public interface IFunctionalModelList : INotifyPropertyChanged
{
    public List<ObservableCollection<FunctionalModel>> FunctionalModels { get; set; }
    
    public IFunctionalModelDictionary FunctionalModelDictionary { get; set; }

    public void ExportDictionary(string path);
    public List<FunctionalModelStructure> GetAllModels();
    public void AddToDictionary(FunctionalModelStructure model, bool imported);
    public void DeleteFromDictionary(int index);
    public void AddToList(int index);
    public void AddToList(int index,FunctionalModel model, bool copy);
    public void DeleteFromList(int indexOfStructure, int indexOfModel);
    public void ImportDictionary(string path);
    public void ExportList(string path);
    public void ImportList(string path);
    public void ResetCount(int index);
    public void ReinitializeNbModels(int index);
    public void AddNewEmptyStruct();







}