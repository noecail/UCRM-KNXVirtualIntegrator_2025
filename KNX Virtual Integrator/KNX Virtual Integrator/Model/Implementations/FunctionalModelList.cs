using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Automation;
using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class FunctionalModelList : IFunctionalModelList, INotifyPropertyChanged
{
    public List<FunctionalModel> FunctionalModels { get; set; }
    private readonly FunctionalModelDictionary _functionalModelDictionary;
    private readonly int _nbModels;

    public FunctionalModelList()
    {
        _functionalModelDictionary = new FunctionalModelDictionary();
        _nbModels = _functionalModelDictionary.FunctionalModels.Count;
        FunctionalModels = _functionalModelDictionary.GetAllModels();

        _functionalModelDictionary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_functionalModelDictionary.FunctionalModels)) // notification de modification dans le dictionnaire
            {
                FunctionalModels = _functionalModelDictionary.GetAllModels(); //mettre à jour dans la liste
                OnPropertyChanged(nameof(FunctionalModels)); //notifier le mainviewmodel
            }
        };
    }


    public FunctionalModelList(string path) //Takes the dictionary from a file's path
    {
        _functionalModelDictionary = new FunctionalModelDictionary(path);
        _nbModels = _functionalModelDictionary.FunctionalModels.Count;


    }


    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index in the dictionary of the Functional Model to copy in the list</param>
    public void AddToList(int index)
    {
        var model = new FunctionalModel(_functionalModelDictionary.FunctionalModels[index]);
        FunctionalModels.Add(model);
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
    /// Adds a personalized model to the dictionary of models.
    /// </summary>
    /// <param name="model">The model to add to the dictionary</param>
    public void AddToDictionary(FunctionalModel model)
    {
        _functionalModelDictionary.AddFunctionalModel(model);
    }

    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index of the Functional Model to delete from in the dictionary. </param>
    public void DeleteFromDictionary(int index)
    {
        if (index < _nbModels)
            return;
        _functionalModelDictionary.RemoveFunctionalModel(index);
    }
    
    /// <summary>
    /// Creates an XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path where the XML has to be exported </param>
    public void ExportDictionary(string path)
    {
        _functionalModelDictionary.ExportDictionary(path);
    }
    
    /// <summary>
    /// Imports an XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path of the xml. </param>
    public void ImportDictionary(string path)
    {
        _functionalModelDictionary.ImportDictionary(path);
    }

    /// <summary>
    /// Method to get all the models in the dictionary.
    /// </summary>
    /// <returns>Returns a list containing all the functional models. </returns>
    public List<FunctionalModel> GetAllModels()
    {
        return _functionalModelDictionary.GetAllModels();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}

