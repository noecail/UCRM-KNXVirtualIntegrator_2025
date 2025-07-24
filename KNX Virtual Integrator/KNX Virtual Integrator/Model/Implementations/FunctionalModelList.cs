using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class FunctionalModelList : IFunctionalModelList, INotifyPropertyChanged
{
    public List<ObservableCollection<FunctionalModel>> FunctionalModels { get; set; } = [];
    public IFunctionalModelDictionary FunctionalModelDictionary { get; set; }

    public FunctionalModelList()
    {
        FunctionalModelDictionary = new FunctionalModelDictionary();
        var index = 0; 
        foreach (var model in FunctionalModelDictionary.GetAllModels())
        {
            FunctionalModels.Add([]);
            AddToList(index);
            index++;
        }

        //FunctionalModels[0][0].Name = "Lumière_ON_OFF_Salon";

        FunctionalModelDictionary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FunctionalModelDictionary.FunctionalModels)) // notification de modification dans le dictionnaire
            {
                if (FunctionalModels.Count != FunctionalModelDictionary.FunctionalModels.Count) //If a model structure is created in the dictionary, creates a list with one element of this new model
                {
                    FunctionalModels.Add([]);
                    AddToList(FunctionalModels.Count - 1);
                } 
                OnPropertyChanged(nameof(FunctionalModelDictionary)); //notifier le mainviewmodel
            }
        };
    }


    public FunctionalModelList(string path) //Takes the dictionary from a file's path
    {
        FunctionalModelDictionary = new FunctionalModelDictionary(path);
        FunctionalModelDictionary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FunctionalModelDictionary.FunctionalModels)) // notification de modification dans le dictionnaire
            {
                if (FunctionalModels.Count != FunctionalModelDictionary.FunctionalModels.Count) //If a model structure is created in the dictionary, creates a list with one element of this new model
                {
                    FunctionalModels.Add([]);
                    AddToList(FunctionalModelDictionary.FunctionalModels.Count - 1);
                } 
                OnPropertyChanged(nameof(FunctionalModels)); //notifier le mainviewmodel
            }
        };

    }


    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index in the dictionary of the Functional Model to copy in the list</param>
    public void AddToList(int index)
    {
        FunctionalModel newModel;
        if (FunctionalModels[index].Count > 0)
        {
            newModel = new FunctionalModel(FunctionalModelDictionary.FunctionalModels[index], FunctionalModels[index][^1].Key + 1, false);
        }
        else
        {
            newModel = new FunctionalModel(FunctionalModelDictionary.FunctionalModels[index],1,false);
        }
        if (newModel.Name.Contains("Structure"))
        {
            if (newModel.Name.Contains("New_Structure"))
                newModel.Name = "New_Model_" + newModel.Key;
            else
            {
                newModel.Name = newModel.Name[..^10];
            }
        }
        FunctionalModels[index].Add(newModel);
    }
    
    /// <summary>
    /// Copies a functional model to the list.
    /// </summary>
    /// <param name="functionalModel">FunctionalModel to add</param>
    /// <param name="index"> Index of the structure</param>
    public void AddToList(int index, FunctionalModel functionalModel)
    {
        FunctionalModels[index].Add(new FunctionalModel(functionalModel,FunctionalModels[index].Count+1,true));
    }

    /// <summary>
    /// Deletes a functional model in the list at the desired index.
    /// </summary>
    /// <param name="indexOfStructure">Index of the structure of the Functional Model to delete in the list</param>
    /// <param name="indexOfModel">Index of the Functional Model to delete in the list</param>
    
    public void DeleteFromList(int indexOfStructure,int indexOfModel)
    {
        FunctionalModels[indexOfStructure].RemoveAt(indexOfModel);
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
        var newModel = model;
        newModel.Name = "New_Structure";
        FunctionalModelDictionary.AddFunctionalModel(model);
    }

    /// <summary>
    /// Deletes a Structure from the dictionary .
    /// </summary>
    /// <param name="index">Index of the Structure to delete from in the dictionary. </param>
    public void DeleteFromDictionary(int index)
    {
        if (index > FunctionalModelDictionary.FunctionalModels.Count)
            return;
        FunctionalModelDictionary.RemoveFunctionalModel(index);
        FunctionalModels.RemoveAt(index);
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
        FunctionalModels.Clear();
        FunctionalModelDictionary.ImportDictionary(path);
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
    
    /// <summary>
    /// Creates an XML file representing the list of list.
    /// </summary>
    /// <param name="path">Path where the XML has to be exported </param>
    public void ExportList(string path)
    {
        var doc = new XmlDocument();
        var project = doc.CreateElement("Project");
        foreach (var modelStructure in FunctionalModels)
        {
            var structure = doc.CreateElement(FunctionalModelDictionary.FunctionalModels[FunctionalModels.FindIndex(l => l == modelStructure)].Name);
            foreach (var model in modelStructure)
            {
                var functionalModel = model.ExportFunctionalModel(doc);
                structure.AppendChild(functionalModel);
            }

            project.AppendChild(structure);
        }
        doc.AppendChild(project);
        doc.Save(path + ".xml");
    }

    public void ImportList(string path)
    {
        FunctionalModels.Clear();
        var doc = new XmlDocument();
        doc.Load(path);
        var xnList = doc.DocumentElement?.ChildNodes;
        for (var i = 0;i<xnList?.Count;i++) // pour chaque structure
        {
            FunctionalModels.Add([]);
            foreach (XmlNode model in xnList[i]?.ChildNodes!) // pour chaque modèle
                FunctionalModels[i].Add(FunctionalModel.ImportFunctionalModel(model));
        }
    }

}

