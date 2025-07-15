using System.Windows.Automation;
using System.Xml;
using System.Xml.Linq;
using KNX_Virtual_Integrator.Model.Entities;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class FunctionalModelList
{
    public List<FunctionalModel> FunctionalModels = [];
    private FunctionalModelDictionary _functionalModelDictionary;
    private int _nbmodels;

    public FunctionalModelList()
    {
        _functionalModelDictionary = new FunctionalModelDictionary();
        _nbmodels = _functionalModelDictionary.FunctionalModels.Count;

    }


    public FunctionalModelList(string path) //Takes the dictionary from a file's path
    {
        _functionalModelDictionary = new FunctionalModelDictionary(path);
        _nbmodels = _functionalModelDictionary.FunctionalModels.Count;


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
        _functionalModelDictionary.Add_FunctionalModel(model);
    }

    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index of the Functional Model to delete from in the dictionary. </param>
    public void DeleteFromDictionary(int index)
    {
        if (index < _nbmodels)
            return;
        _functionalModelDictionary.Remove_FunctionalModel(index);
    }
    
    /// <summary>
    /// Creates a XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path where the XML has to be exported </param>
    public void ExportDictionary(string path)
    {
        _functionalModelDictionary.ExportDictionary(path);
    }
    
    /// <summary>
    /// Imports a XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path of the xml. </param>
    public void ImportDictionary(string path)
    {
        _functionalModelDictionary.ImportDictionary(path);
    }
}

