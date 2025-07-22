using System.ComponentModel;
using System.Runtime.CompilerServices;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class FunctionalModelList : IFunctionalModelList, INotifyPropertyChanged
{
    public List<List<FunctionalModel>> FunctionalModels { get; set; } = [];
    public IFunctionalModelDictionary FunctionalModelDictionary { get; set; }
    private readonly int _nbModels;

    public FunctionalModelList()
    {
        FunctionalModelDictionary = new FunctionalModelDictionary();
        _nbModels = FunctionalModelDictionary.FunctionalModels.Count;
        foreach (var model in FunctionalModelDictionary.GetAllModels())
        {
            FunctionalModels.Add([new FunctionalModel(model,1)]);
        }

        FunctionalModelDictionary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FunctionalModelDictionary.FunctionalModels)) // notification de modification dans le dictionnaire
            {
                if (FunctionalModels.Count != FunctionalModelDictionary.FunctionalModels.Count) //If a model structure is created in the dictionary, creates a list with one element of this new model
                {
                    FunctionalModels.Add([]);
                    AddToList(FunctionalModelDictionary.FunctionalModels.Count - 1);
                }; 
                OnPropertyChanged(nameof(FunctionalModels)); //notifier le mainviewmodel
            }
        };
    }


    public FunctionalModelList(string path) //Takes the dictionary from a file's path
    {
        FunctionalModelDictionary = new FunctionalModelDictionary(path);
        _nbModels = FunctionalModelDictionary.FunctionalModels.Count;


    }


    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index in the dictionary of the Functional Model to copy in the list</param>
    public void AddToList(int index)
    {
        FunctionalModels[index].Add(new FunctionalModel(FunctionalModelDictionary.FunctionalModels[index],FunctionalModels[index].Count+1));
    }

    /// <summary>
    /// Deletes a functional model in the list at the desired index.
    /// </summary>
    /// <param name="index">Index of the Functional Model to delete in the list</param>
    public void DeleteFromList(int index)
    {
        FunctionalModels.RemoveAt(index);
    }

    /// <summary>
    /// Duplicates the model of a given index in a list
    /// </summary>
    /// <param name="models">List containing the model to be copied, and in which the copy will be</param>
    /// <param name="index">Index of the model to copy</param>
    public void DuplicateModel(List<FunctionalModel> models, int index)
    {
        models.Add(models[index]);
    }

    /// <summary>
    /// Adds a personalized model to the dictionary of models.
    /// </summary>
    /// <param name="model">The model to add to the dictionary</param>
    public void AddToDictionary(FunctionalModel model)
    {
        FunctionalModelDictionary.AddFunctionalModel(model);
    }

    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index of the Functional Model to delete from in the dictionary. </param>
    public void DeleteFromDictionary(int index)
    {
        if (index > _nbModels)
            return;
        FunctionalModelDictionary.RemoveFunctionalModel(index);
    }
    
    /// <summary>
    /// Creates an XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path where the XML has to be exported </param>
    public void ExportDictionary(string path)
    {
        FunctionalModelDictionary.ExportDictionary(path);
    }
    
    /// <summary>
    /// Imports an XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path of the xml. </param>
    public void ImportDictionary(string path)
    {
        FunctionalModelDictionary.ImportDictionary(path);
        foreach (var model in FunctionalModelDictionary.GetAllModels())
        {
            FunctionalModels.Add([model]);
        }
    }

    /// <summary>
    /// Method to get all the models in the dictionary.
    /// </summary>
    /// <returns>Returns a list containing all the functional models. </returns>
    public List<FunctionalModel> GetAllModels()
    {
        return FunctionalModelDictionary.GetAllModels();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}

