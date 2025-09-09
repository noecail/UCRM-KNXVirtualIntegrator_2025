using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
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
    /// <summary>
    /// The list structure of functional models.
    /// </summary>
    public List<ObservableCollection<FunctionalModel>> FunctionalModels { get; set; }
    /// <summary>
    /// The dictionary of models and structures
    /// </summary>
    public IFunctionalModelDictionary FunctionalModelDictionary { get; set; }

    /// <summary>
    /// Creates an XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path where the XML has to be exported </param>
    public void ExportDictionary(string path);
    /// <summary>>
    /// Method to get all the models in the dictionary.
    /// </summary>
    /// <returns>Returns a list containing all the functional models. </returns>
    public List<FunctionalModelStructure> GetAllModels();
    /// <summary>
    /// Adds a personalized model to the dictionary of models.
    /// </summary>
    /// <param name="model">The model to add to the dictionary</param>
    /// <param name="imported">Boolean to check if the functionalModelStructure to add is created manually or by the application during importation</param>
    public void AddToDictionary(FunctionalModelStructure model, bool imported);
    /// <summary>
    /// Deletes a Structure from the dictionary .
    /// </summary>
    /// <param name="index">Index of the Structure to delete from in the dictionary. </param>
    public void DeleteFromDictionary(int index);
    /// <summary>
    /// Resets the saved structure by clearing the dictionary then putting back in the structure at the index.
    /// </summary>
    /// <param name="index">The index at which the structure was saved (and to save)</param>
    /// <param name="savedStructure">the structure to be saved</param>
    public void ResetInDictionary(int index, FunctionalModelStructure savedStructure);
    /// <summary>
    /// Copies a functional model from the dictionary to the list.
    /// </summary>
    /// <param name="index">Index in the dictionary of the Functional Model to copy in the list</param>
    public void AddToList(int index);
    /// <summary>
    /// Copies a functional model to the list.
    /// </summary>
    /// <param name="functionalModel">FunctionalModel to add</param>
    /// <param name="index"> Index of the structure</param>
    /// <param name="copy"> boolean indicating whether the model is a copy or not</param>
    public void AddToList(int index,FunctionalModel functionalModel, bool copy);
    /// <summary>
    /// Deletes a functional model in the list at the desired index.
    /// </summary>
    /// <param name="indexOfStructure">Index of the structure of the Functional Model to delete in the list</param>
    /// <param name="indexOfModel">Index of the Functional Model to delete in the list</param>
    public void DeleteFromList(int indexOfStructure, int indexOfModel);

    /// <summary>
    /// Creates an XMLElement representing the dictionary.
    /// </summary>
    /// <param name="doc">The document in which the element is created.</param>
    /// <returns>The created XmlElement</returns>
    public XmlElement ExportDictionary(XmlDocument doc);
    /// <summary>
    /// Imports an XML file representing the dictionary.
    /// </summary>
    /// <param name="path">Path of the xml. </param>
    public void ImportDictionary(string path);

    /// <summary>
    /// Imports a functional model dictionary after clearing the list of models.
    /// </summary>
    /// <param name="xnList">the list from which to import the dictionary</param>
    public void ImportDictionary(XmlNodeList xnList);
    /// <summary>
    /// Creates an XML file representing the list of list.
    /// </summary>
    /// <param name="path">Path where the XML has to be exported </param>
    public void ExportList(string path);

    /// <summary>
    /// Exports the list of models in each structure from an XmlDocument
    /// </summary>
    /// <param name="doc">The XmlDocument in which is created the XmlElement.</param>
    /// <returns>The created XmlElement</returns>
    public XmlElement ExportList(XmlDocument doc);
    /// <summary>
    /// Imports the structure list from a file
    /// </summary>
    /// <param name="path">the path of the file</param>
    public void ImportList(string path);

    /// <summary>
    /// Imports the structure list from an XmlNodeList
    /// </summary>
    /// <param name="xnList">The XmlNodeList to import from</param>
    public void ImportList(XmlNodeList? xnList);
    /// <summary>
    /// see <see cref="ExportList(string)"/> and <see cref="ExportDictionary(string)"/>
    /// </summary>
    /// <param name="path">Path of the file where everything has to be exported to.</param>
    /// <param name="projectName">Name of the imported project or file.</param>
    public void ExportListAndDictionary(string path, string projectName);
    /// <summary>
    /// see <see cref="ImportList(string)"/> and <see cref="ImportDictionary(string)"/>.
    /// </summary>
    /// <param name="path">the path of the file to import from.</param>
    /// <returns> The name of the importef file or project. </returns>
    public string ImportListAndDictionary(string path);
    /// <summary>
    /// Resets the count of models in a structure
    /// </summary>
    /// <param name="index">the structure index</param>
    public void ResetCount(int index);
    /// <summary>
    /// see <see cref="ResetCount"/>.
    /// </summary>
    /// <param name="index">The index of the structure.</param>
    public void ReinitializeNbModels(int index);
    /// <summary>
    /// Adds a new empty structure. <seealso cref="FunctionalModels"/> <seealso cref="Model.Implementations.FunctionalModelList._nbModelsCreated"/>
    /// </summary>
    public void AddNewEmptyStruct();
    /// <summary>
    /// Creates a new counter associated to a new list in the list of lists
    /// </summary>
    public void AddNewCount();

    /// <summary>
    /// Duplicates the model of a given index in a list
    /// </summary>
    /// <param name="models">List containing the model to be copied, and in which the copy will be</param>
    /// <param name="index">Index of the model to copy</param>
    public void DuplicateModel(List<FunctionalModel> models, int index);








}