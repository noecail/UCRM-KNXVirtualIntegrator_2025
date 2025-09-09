using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// manages a list of functional models (FunctionalModel).
///
/// Provides methods to add, remove, retrieve, and update models in the list.
/// Each model is identified by a unique key (int). This interface enables centralized 
/// management of functional models, allowing standardized operations on the dictionary.
/// 
/// - AddFunctionalModel: Adds a functional model to the dictionary.
/// - RemoveFunctionalModel: Removes a functional model using its key.
/// - GetAllModels: Retrieves all functional models from the dictionary.
/// </summary>
public class FunctionalModelList : IFunctionalModelList
{
    /// <summary>
    /// The number of models created
    /// </summary>
    private readonly List<int> _nbModelsCreated = [];
    /// <summary>
    /// The list structure of functional models.
    /// </summary>
    public List<ObservableCollection<FunctionalModel>> FunctionalModels { get; set; } = [];
    /// <summary>
    /// The dictionary of models and structures
    /// </summary>
    public IFunctionalModelDictionary FunctionalModelDictionary { get; set; }
    /// <summary>
    /// Default model list constructor
    /// </summary>
    public FunctionalModelList()
    {
        FunctionalModelDictionary = new FunctionalModelDictionary();
        var index = 0; 
        foreach (var unused in FunctionalModelDictionary.GetAllModels())
        {
            FunctionalModels.Add([]);
            _nbModelsCreated.Add(0);
            AddToList(index);
            index++;
        }

        FunctionalModelDictionary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FunctionalModelDictionary.FunctionalModels)) // notification de modification dans le dictionnaire
            {
                if (FunctionalModels.Count <= FunctionalModelDictionary.FunctionalModels.Count) //If a model structure is created in the dictionary, creates a list with one element of this new model
                {
                    FunctionalModels.Add([]);
                    _nbModelsCreated.Add(0);

                    AddToList(FunctionalModelDictionary.FunctionalModels.Count - 1);
                } 
                OnPropertyChanged(nameof(FunctionalModelDictionary)); //notifier le mainViewModel
            }
        };
    }
    /// <summary>
    /// Model list constructor. Takes the dictionary from a file's path.
    /// </summary>
    /// <param name="path"></param>
    public FunctionalModelList(string path)
    {
        FunctionalModelDictionary = new FunctionalModelDictionary(path);
        FunctionalModelDictionary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FunctionalModelDictionary.FunctionalModels)) // notification de modification dans le dictionnaire
            {
                if (FunctionalModels.Count <= FunctionalModelDictionary.FunctionalModels.Count) //If a model structure is created in the dictionary, creates a list with one element of this new model
                {
                    FunctionalModels.Add([]);
                    _nbModelsCreated.Add(0);
                    AddToList(FunctionalModelDictionary.FunctionalModels.Count - 1);
                } 
                OnPropertyChanged(nameof(FunctionalModels)); //notifier le mainViewModel
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
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (FunctionalModels[index].Count > 0)
            newModel = new FunctionalModel(FunctionalModelDictionary.FunctionalModels[index].Model, FunctionalModels[index][^1].Key + 1, false);
        else
            newModel = new FunctionalModel(FunctionalModelDictionary.FunctionalModels[index].Model,1,false);
        
        _nbModelsCreated[index]++;
        if (newModel.Name.Contains("Structure"))
        {
            if (newModel.Name.Contains("New_Structure"))
                newModel.Name = "New_Model_" +_nbModelsCreated[index];
            else
            {
                newModel.Name = string.Join("_",newModel.Name.Split('_')[..^1]) +"_" + _nbModelsCreated[index];
            }
        }
        FunctionalModels[index].Add(newModel);
    }
    /// <summary>
    /// Resets the count of models in a structure
    /// </summary>
    /// <param name="index">the structure index</param>
    public void ResetCount(int index)
    {
        _nbModelsCreated[index] = 0;
    }
    
    /// <summary>
    /// Creates a new counter associated to a new list in the list of lists
    /// </summary>
    public void AddNewCount()
    {
        _nbModelsCreated.Add(0);
    }

    /// <summary>
    /// Copies a functional model to the list.
    /// </summary>
    /// <param name="functionalModel">FunctionalModel to add</param>
    /// <param name="index"> Index of the structure</param>
    /// <param name="copy"> boolean indicating whether the model is a copy or not</param>    
    public void AddToList(int index, FunctionalModel functionalModel, bool copy)
    {

        var newModel = new FunctionalModel(functionalModel,FunctionalModels[index].Count + 1,false);
        if (_nbModelsCreated.Count < index)
        {
            _nbModelsCreated.Add(0);
        }
        _nbModelsCreated[index]++;
        if (newModel.Name.Contains("Structure"))
        {
            if (newModel.Name.Contains("New_Structure"))
                newModel.Name = "New_Model_" +_nbModelsCreated[index];
            else
            {
                newModel.Name = string.Join("_",newModel.Name.Split('_')[..^1]) +"_" + _nbModelsCreated[index];
            }
        }


        FunctionalModels[index].Add(newModel);
        // FunctionalModels[index].Add(new FunctionalModel(functionalModel,FunctionalModels[index].Count+1,copy));
    }

    /// <summary>
    /// Deletes a functional model in the list at the desired index.
    /// </summary>
    /// <param name="indexOfStructure">Index of the structure of the Functional Model to delete in the list</param>
    /// <param name="indexOfModel">Index of the Functional Model to delete in the list</param>
    public void DeleteFromList(int indexOfStructure, int indexOfModel)
    {
        FunctionalModels[indexOfStructure].RemoveAt(indexOfModel);
        foreach (var model in FunctionalModels[indexOfStructure])
            if (model.Key > indexOfModel + 1) 
                model.Key--;
    }

    /// <summary>
    /// Duplicates the model of a given index in a list
    /// </summary>
    /// <param name="models">List containing the model to be copied, and in which the copy will be</param>
    /// <param name="index">Index of the model to copy</param>
    public void DuplicateModel(List<FunctionalModel> models, int index)
    {
        models.Add(new FunctionalModel(models[index], models.Count + 1, true));    }

    /// <summary>
    /// Adds a personalized model to the dictionary of models.
    /// </summary>
    /// <param name="model">The model to add to the dictionary</param>
    /// <param name="imported">Boolean to check if the functionalModelStructure to add is created manually or by the application during importation</param>
    public void AddToDictionary(FunctionalModelStructure model, bool imported)
    {
        var newModel = new FunctionalModelStructure(model);
        if (string.IsNullOrEmpty(model.Model.Name))
            newModel.Model.Name = "New_Structure";
        FunctionalModelDictionary.AddFunctionalModel(newModel, imported);
    }

    /// <summary>
    /// Deletes a Structure from the dictionary .
    /// </summary>
    /// <param name="index">Index of the Structure to delete from in the dictionary. </param>
    public void DeleteFromDictionary(int index)
    {
        if (index >= FunctionalModelDictionary.FunctionalModels.Count) return;
        for (var i = FunctionalModelDictionary.FunctionalModels[index].Model.Key; i < FunctionalModelDictionary.FunctionalModels[^1].Model.Key; i++)
            FunctionalModelDictionary.FunctionalModels[i].Model.Key--;
        FunctionalModelDictionary.RemoveFunctionalModel(index);
        FunctionalModels.RemoveAt(index);
        OnPropertyChanged(nameof(FunctionalModels)); //notifier la UI
    }
    /// <summary>
    /// Resets the saved structure by clearing the dictionary then putting back in the structure at the index.
    /// </summary>
    /// <param name="index">The index at which the structure was saved (and to save)</param>
    /// <param name="savedStructure">the structure to be saved</param>
    public void ResetInDictionary(int index, FunctionalModelStructure savedStructure)
    {
        List<FunctionalModelStructure> firstPart = [];
        List<FunctionalModelStructure> secondPart = [];
        foreach (var structure in FunctionalModelDictionary.FunctionalModels)
            if (structure.Model.Key-1 < index)
                firstPart.Add(structure); 
            else if (structure.Model.Key-1 > index)
                    secondPart.Add(structure);
        
        FunctionalModelDictionary.FunctionalModels.Clear();
        foreach(var structure in firstPart)
            FunctionalModelDictionary.FunctionalModels.Add(structure);
        FunctionalModelDictionary.FunctionalModels.Add(new FunctionalModelStructure(savedStructure));
        foreach(var structure in secondPart)
            FunctionalModelDictionary.FunctionalModels.Add(structure);
        OnPropertyChanged(nameof(FunctionalModelDictionary.FunctionalModels));
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
    /// Creates an XMLElement representing the dictionary.
    /// </summary>
    /// <param name="doc">The document in which the element is created.</param>
    /// <returns>The created XmlElement</returns>
    public XmlElement ExportDictionary(XmlDocument doc)
    {
        return (FunctionalModelDictionary.ExportDictionary(doc));
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
    /// Imports a functional model dictionary after clearing the list of models.
    /// </summary>
    /// <param name="xnList">the list from which to import the dictionary</param>
    public void ImportDictionary(XmlNodeList xnList)
    {
        FunctionalModels.Clear();
        FunctionalModelDictionary.ImportDictionary(xnList);
    }

    /// <summary>
    /// Method to get all the models in the dictionary.
    /// </summary>
    /// <returns>Returns a list containing all the functional models. </returns>
    public List<FunctionalModelStructure> GetAllModels()
    {
        return FunctionalModelDictionary.GetAllModels();
    }
    /// <summary>
    /// The event that occurs when the list changes. 
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes the event <see cref="PropertyChanged"/> when the list changes.
    /// </summary>
    /// <param name="propertyName">The name of the changed property</param>
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
               var structure = doc.CreateElement(FunctionalModelDictionary.FunctionalModels[FunctionalModels.FindIndex(l => l == modelStructure)].Model.Name);
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
    /// <summary>
    /// Exports the list of models in each structure from an XmlDocument
    /// </summary>
    /// <param name="doc">The XmlDocument in which is created the XmlElement.</param>
    /// <returns>The created XmlElement</returns>
    public XmlElement ExportList(XmlDocument doc)
    {
        var project = doc.CreateElement("Project");
        foreach (var modelStructure in FunctionalModels)
        {
            var structure = doc.CreateElement(FunctionalModelDictionary.FunctionalModels[FunctionalModels.FindIndex(l => l == modelStructure)].Model.Name.Replace(' ','_'));
            foreach (var model in modelStructure)
            {
                var functionalModel = model.ExportFunctionalModel(doc);
                structure.AppendChild(functionalModel);
            }

            project.AppendChild(structure);
        }
        doc.AppendChild(project);
        return project;
    }
    /// <summary>
    /// see <see cref="ExportList(string)"/> and <see cref="ExportDictionary(string)"/>
    /// </summary>
    /// <param name="path">Path of the file where everything has to be exported to.</param>
    /// <param name="projectName">Name of the imported project or file.</param>
    public void ExportListAndDictionary(string path, string projectName)
    {
        var doc = new XmlDocument();
        var xDictionary = doc.CreateElement("Dictionary");
        xDictionary.AppendChild(ExportDictionary(doc));
        var xList = doc.CreateElement("List");
        var xDocName = doc.CreateAttribute("Name");
        xDocName.Value = projectName;
        xList.AppendChild(ExportList(doc));
        XmlElement Root = doc.CreateElement("Root");
        Root.AppendChild(xList);
        Root.Attributes.Append(xDocName);
        Root.AppendChild(xDictionary);
        doc.AppendChild(Root);
        doc.Save(path);
    }
    /// <summary>
    /// Imports the structure list from a file
    /// </summary>
    /// <param name="path">the path of the file</param>
    public void ImportList(string path)
    {
        FunctionalModels.Clear();
        var doc = new XmlDocument();
        doc.Load(path);
        var xnList = doc.DocumentElement?.ChildNodes;
        for (var i = 0;i<xnList?.Count;i++) // pour chaque structure
        {
            FunctionalModels.Add([]);
            _nbModelsCreated.Add(0);
            foreach (XmlNode model in xnList[i]?.ChildNodes!) // pour chaque modèle
            {
                FunctionalModels[i].Add(FunctionalModel.ImportFunctionalModel(model));
                _nbModelsCreated[i]++;
            }
        }
    }
    /// <summary>
    /// Imports the structure list from an XmlNodeList
    /// </summary>
    /// <param name="xnList">The XmlNodeList to import from</param>
    public void ImportList(XmlNodeList? xnList)
    {
        FunctionalModels.Clear();
        _nbModelsCreated.Clear();
        for (var i = 0;i<xnList?.Count;i++) // pour chaque structure
        {
            FunctionalModels.Add([]);
            _nbModelsCreated.Add(0);
            foreach (XmlNode model in xnList[i]?.ChildNodes!) // pour chaque modèle
            {
                FunctionalModels[i].Add(FunctionalModel.ImportFunctionalModel(model));
                if (FunctionalModels[i].Count > 1)
                {
                    FunctionalModels[i][^1].Key = FunctionalModels[i][^2].Key +1;
                }
                else
                {
                    FunctionalModels[i][^1].Key = 1;
                }
                _nbModelsCreated[i]++;
            }
        }
    }
    /// <summary>
    /// see <see cref="ImportList(string)"/> and <see cref="ImportDictionary(string)"/>.
    /// </summary>
    /// <param name="path">the path of the file to import from.</param>
    /// <returns> The name of the importef file or project. </returns>
    public string ImportListAndDictionary(string path)
    {
        var doc = new XmlDocument();
        doc.Load(path);
        XmlNodeList? xnList = doc.DocumentElement?.ChildNodes;
        if (xnList != null)
        {
            foreach (XmlNode ok in xnList){
                if (ok.Name == "Dictionary"&& ok.ChildNodes[0]?.ChildNodes!=null)
                {
                    ImportDictionary(ok.ChildNodes[0]?.ChildNodes!);
                }
            }
            foreach (XmlNode ok in xnList)
            {
                if (ok.Name == "List"&& ok.ChildNodes[0]?.ChildNodes!=null)
                {
                    ImportList(ok.ChildNodes[0]?.ChildNodes!);
                }
            }
        }
        var res = doc.DocumentElement?.Attributes?["Name"]?.Value ?? "";
        return res;
    }
    /// <summary>
    /// see <see cref="ResetCount"/>.
    /// </summary>
    /// <param name="index">The index of the structure.</param>
    public void ReinitializeNbModels(int index)
    {
        _nbModelsCreated[index]=0;
    }
    /// <summary>
    /// Adds a new empty structure. <seealso cref="FunctionalModels"/> <seealso cref="_nbModelsCreated"/>
    /// </summary>
    public void AddNewEmptyStruct()
    {
        FunctionalModels.Add([]);
        _nbModelsCreated.Add(0);
    }
    
}

