using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
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
        /// <summary>
        /// Gets or sets the list of models.
        /// </summary>
        public ObservableCollection<FunctionalModelStructure> FunctionalModels {get;set;}

        /// <summary>
        /// Adds a keyword to the model at an index in the dictionary
        /// </summary>
        /// <param name="index">the index in the dictionary</param>
        /// <param name="word">the keyword</param>
        public void AddKeyword(int index, string word);

        /// <summary>
        /// Adds a model to the dictionary
        /// </summary>
        /// <param name="functionalModel">the structure of the model</param>
        /// <param name="imported">if the model is imported</param>
        public void AddFunctionalModel(FunctionalModelStructure functionalModel, bool imported);

        /// <summary>
        /// Adds a model to the dictionary
        /// </summary>
        /// <param name="functionalModel">the structure of the model</param>
        /// <param name="imported">if the model is imported</param>
        /// <param name="keywords">the keywords of the model</param>
        public void AddFunctionalModel(FunctionalModelStructure functionalModel, bool imported, List<string> keywords);

        /// <summary>
        /// Removes a model at a certain index in the dictionary
        /// </summary>
        /// <param name="index">the index of the model</param>
        public void RemoveFunctionalModel(int index);

        /// <summary>
        /// Gets all the models of the dictionary.
        /// </summary>
        /// <returns>the list of models</returns>
        public List<FunctionalModelStructure> GetAllModels();

        /// <summary>
        /// Creates an XML file representing the dictionary.
        /// </summary>
        /// <param name="path">Path where the XML has to be exported </param>
        public void ExportDictionary(string path);

        /// <summary>
        /// Exports the dictionary to an XmlElement 
        /// </summary>
        /// <param name="doc">The document to which should be exported the dictionary</param>
        /// <returns>The created document</returns>
        public XmlElement ExportDictionary(XmlDocument doc);

        /// <summary>
        /// Imports a functional model dictionary from a path.
        /// </summary>
        /// <param name="path">Path of the dictionary</param>
        public void ImportDictionary(string path);

        /// <summary>
        /// Imports a functional model dictionary
        /// </summary> 
        /// <param name="xnList">the list from which to import the dictionary</param>
        public void ImportDictionary(XmlNodeList xnList);

        /// <summary>
        /// Checks if a Functional Model has the same structure as the ones in the dictionary
        /// </summary>
        /// <param name="functionalModel">Structure to find in the dictionary</param>
        /// <returns>Index of the corresponding structure, or null if not found</returns>
        public int HasSameStructure(FunctionalModel functionalModel);
        
        /// <summary>
        /// The event that occurs when the Dictionary changes. 
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Checks the index of a model with a certain name
        /// </summary>
        /// <param name="name">the name of the model</param>
        /// <returns>the index of the model if found; -1 otherwise.</returns>
        public int CheckName(string name);


    }
}

