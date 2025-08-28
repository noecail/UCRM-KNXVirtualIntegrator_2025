using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using System.Xml;


namespace KNX_Virtual_Integrator.Model.Implementations
{
    /// <summary>
    /// Manages a dictionary of functional models (FunctionalModel).
    ///
    /// Provides methods to add, remove, retrieve, and update models in the dictionary.
    /// Each model is identified by a unique key (int). This interface enables centralized 
    /// management of functional models, allowing standardized operations on the dictionary.
    /// 
    /// - AddFunctionalModel: Adds a functional model to the dictionary.
    /// - RemoveFunctionalModel: Removes a functional model using its key.
    /// - GetAllModels: Retrieves all functional models from the dictionary.
    /// </summary>
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        /// <summary>
        /// List of models
        /// </summary>
        private ObservableCollection<FunctionalModelStructure> _functionalModels =[];
        
        /// <summary>
        /// The number of structure created.
        /// </summary>
        private int _nbStructuresCreated ;
        /// <summary>
        /// Gets or sets the list of models.
        /// </summary>
        public ObservableCollection<FunctionalModelStructure> FunctionalModels
        {
            get => _functionalModels;
            set
            {
                if (_functionalModels.Equals(value)) return;
                _functionalModels = value;
            }
        }

        /// <summary>
        /// Default constructor of the dictionary. Gives a default list of structures
        /// </summary>
        public FunctionalModelDictionary()
        {
            FunctionalModels = [];
            AddFunctionalModel(new FunctionalModelStructure("Lumiere_ON_OFF",
                        new Dictionary<int, DptAndKeywords>()
                        {
                            {
                                1, new DptAndKeywords { Keywords = ["On/off"], Dpt = new DataPointType(1) }
                            }, //CMD
                            {
                                2, new DptAndKeywords { Keywords = ["etat On/Off"], Dpt = new DataPointType(1) }
                            } //IE
                        },
                        [new ElementStructure([1],[2])]
                        //,1
                        ,[[[1,0]]],[[[1,0]]],1
            ),false,["Lumiere on/off", "Lumiere on-off", "Lumiere on_off", "Light on/off", "Eclairage_Simple"]);  //Adding On/Off light functional model
            AddFunctionalModel(new FunctionalModelStructure("Lumiere_Variation",
                        new Dictionary<int, DptAndKeywords>()
                        {
                            {
                                1, new DptAndKeywords { Keywords = ["On/off"], Dpt = new DataPointType(1) }
                            }, //CMD on/off
                            {
                                2, new DptAndKeywords { Keywords = ["Etat On/off"], Dpt = new DataPointType(1) }
                            }, //IE  on/off
                            {
                                3, new DptAndKeywords { Keywords = ["Variations"], Dpt = new DataPointType(3) }
                            }, //CMD relative
                            {
                                4, new DptAndKeywords { Keywords = ["Valeurs variation"], Dpt = new DataPointType(5) }
                            }, //CMD absolue
                            {
                                5, new DptAndKeywords { Keywords = ["Etats variation"], Dpt = new DataPointType(5) }
                            }, //IE  absolue
                        },
                        [new ElementStructure([1],[2,5]), // test on/off
                            new ElementStructure([3],[2,2,5]), //test relative command
                            new ElementStructure([4],[2,2,5]) //test absolute command
                        ]
                        ,[[[1,0]],[[4]],[[0xFF,0]]],[[[1,0],[-1,-1]],[[1],[0],[-1]],[[1,0],[0,1],[0xFF,0]]],2
                    ),false, ["Lumiere variation", "variation", "Lumiere_variation", "Light variation", "Eclairages_variable"]
                );
            
            
            AddFunctionalModel(new FunctionalModelStructure("Store",
                    new Dictionary<int, DptAndKeywords>()
                    {
                        {
                            1, new DptAndKeywords { Keywords = ["Montee/descente"], Dpt = new DataPointType(1) }
                        }, //CMD on/off
                        {
                            2, new DptAndKeywords { Keywords = ["Etats Montee/descente"], Dpt = new DataPointType(1) }
                        }, //IE  on/off
                        {
                            3, new DptAndKeywords { Keywords = ["Stop"], Dpt = new DataPointType(1) }
                        }, //Stop
                        {
                            4, new DptAndKeywords { Keywords = ["Position"], Dpt = new DataPointType(5) }
                        }, //CMD absolue
                        {
                            5, new DptAndKeywords { Keywords = ["Etat Position"], Dpt = new DataPointType(5) }
                        }, //IE  absolue
                    },
                    [new ElementStructure([1],[2,5]), // test on/off
                        new ElementStructure([1,3],[2,2,5]), //test stop
                        new ElementStructure([4],[2,2,5]) //test absolute command
                    ],
                    [[[1,0]],[[1],[1]],[[0xFF,0]]],[[[1,0],[-1,-1]],[[1],[0],[-1]],[[1,0],[0,1],[0xFF,0]]],3
                ),false, ["store", "blind", "Volets_roulants","Volet_roulant"]
            );
            AddFunctionalModel(new FunctionalModelStructure("Commutation",
                        new Dictionary<int, DptAndKeywords>()
                        {
                            {
                                1,
                                new DptAndKeywords
                                    { Keywords = ["On/Off"], Dpt = new DataPointType(1) }
                            }, //CMD
                            {
                                2,
                                new DptAndKeywords
                                    { Keywords = ["Etat On/off"], Dpt = new DataPointType(1) }
                            } //IE
                        },
                        [new ElementStructure([1], [2])],
                        [[[1,0]]],[[[1,0]]],4
                    ), false,["Commute", "Commutation", "Convecteur", "Prise", "Arrosage", "Ouvrant"]
                );//Adding commutation functional model*/
        }
        /// <summary>
        /// Adds a keyword to the model at an index in the dictionary
        /// </summary>
        /// <param name="index">the index in the dictionary</param>
        /// <param name="word">the keyword</param>
        public void AddKeyword(int index, string word)
        {
            FunctionalModels[index].Keywords.Add(word.ToLower());
        }
        /// <summary>
        /// Constructor of the dictionary
        /// </summary>
        /// <param name="path">file path from which to import the dictionary</param>
        public FunctionalModelDictionary(string path)
        {
            FunctionalModels = [];
            ImportDictionary(path);

        }

        /// <summary>
        /// Adds a model to the dictionary
        /// </summary>
        /// <param name="functionalModel">the structure of the model</param>
        /// <param name="imported">if the model is imported</param>
        public void AddFunctionalModel(FunctionalModelStructure functionalModel, bool imported)
        {
            var newModel = new FunctionalModelStructure(functionalModel);
            _nbStructuresCreated++;
            if (newModel.Model.Name == "New_Model")
                newModel.Model.Name += "_Structure_" + _nbStructuresCreated;
            else if (!functionalModel.Model.Name.Contains("Structure"))
                newModel.Model.Name += "_Structure";
            if (imported == false && newModel.Model.ElementList.Count ==0)
                newModel.Model = newModel.BuildFunctionalModel(newModel.Model.Name, FunctionalModels.Count +1);
            FunctionalModels.Add(newModel);
            OnPropertyChanged(nameof(FunctionalModels));
        }
        /// <summary>
        /// Adds a model to the dictionary
        /// </summary>
        /// <param name="functionalModel">the structure of the model</param>
        /// <param name="imported">if the model is imported</param>
        /// <param name="keywords">the keywords of the model</param>
        public void AddFunctionalModel(FunctionalModelStructure functionalModel, bool imported, List<string> keywords)
        {
            var newModel = new FunctionalModelStructure(functionalModel);
            foreach (var keyword in keywords)
            {
                newModel.Keywords.Add(keyword);
                newModel.Keywords.Add(keyword.Replace(' ','_'));
                newModel.Keywords.Add(keyword.Replace('_',' '));
            }
            newModel.UpdateKeywordList();

            _nbStructuresCreated++;
            if (newModel.Model.Name == "New_Structure")
                newModel.Model.Name += "_" + _nbStructuresCreated;
            else if (!functionalModel.Model.Name.Contains("Structure"))
                newModel.Model.Name += "_Structure";
            FunctionalModels.Add(newModel);
            OnPropertyChanged(nameof(FunctionalModels));

        }
        /// <summary>
        /// Removes a model at a certain index in the dictionary
        /// </summary>
        /// <param name="index">the index of the model</param>
        public void RemoveFunctionalModel(int index)
        {
            FunctionalModels.RemoveAt(index);
            OnPropertyChanged(nameof(FunctionalModels));
        }
        /// <summary>
        /// Gets all the models of the dictionary.
        /// </summary>
        /// <returns>the list of models</returns>
        public List<FunctionalModelStructure> GetAllModels()
        {
            var liste = new List<FunctionalModelStructure>();
            foreach (var model in FunctionalModels) 
            {
                liste.Add(model);
            }
            return liste;
        }

        /// <summary>
        /// Creates an XML file representing the dictionary.
        /// </summary>
        /// <param name="path">Path where the XML has to be exported </param>
        public void ExportDictionary(string path)
        {
            var doc = new XmlDocument();
            var project = doc.CreateElement("Dictionary");
            foreach (var model in GetAllModels())
            {
                var functionalModel = model.ExportFunctionalModelStructure(doc);
                var keywords = doc.CreateElement("Keywords");
                foreach (var keyword in model.Keywords)
                {
                    var xKeyword = doc.CreateElement("Keyword");
                    xKeyword.InnerText = keyword;
                    keywords.AppendChild(xKeyword);
                }
                functionalModel.AppendChild(keywords);
                project.AppendChild(functionalModel);
            }

            doc.AppendChild(project);
            doc.Save(path + ".xml");
        }
        /// <summary>
        /// Exports the dictionary to an XmlElement 
        /// </summary>
        /// <param name="doc">The document to which should be exported the dictionary</param>
        /// <returns>The created document</returns>
        public XmlElement ExportDictionary(XmlDocument doc)
        {
            var project = doc.CreateElement("Dictionary");
            foreach (var model in GetAllModels())
            {
                var functionalModel = model.ExportFunctionalModelStructure(doc);
                var keywords = doc.CreateElement("Keywords");
                foreach (var keyword in model.Keywords)
                {
                    var xKeyword = doc.CreateElement("Keyword");
                    xKeyword.InnerText = keyword;
                    keywords.AppendChild(xKeyword);
                }
                functionalModel.AppendChild(keywords);
                project.AppendChild(functionalModel);
            }

            return(project);
        }

        /// <summary>
        /// Imports a functional model dictionary from a path.
        /// </summary>
        /// <param name="path">Path of the dictionary</param>
        public void ImportDictionary(string path)
        {
            FunctionalModels.Clear();
            var doc = new XmlDocument();
            doc.Load(path);
            var xnList = doc.DocumentElement?.ChildNodes;
            if (xnList == null)
                return;
            for (var i = 0; i < xnList.Count;i++) // pour chaque modèle
            {
                var model = xnList[i];
                if (model != null){
                    AddFunctionalModel(FunctionalModelStructure.ImportFunctionalModelStructure(model,i+1),true);
                    foreach (XmlNode element in model.ChildNodes!)
                    {
                        if (element.Name == "Keywords")
                        {
                            foreach (XmlNode keyword in element.ChildNodes)
                            {
                                FunctionalModels[i].Keywords.Add(keyword.InnerText);
                            }
                        }
                    }
                    FunctionalModels[i].UpdateKeywordList();
                }
            }
        }
        /// <summary>
        /// Imports a functional model dictionary
        /// </summary>
        /// <param name="xnList">the list from which to import the dictionary</param>
        public void ImportDictionary(XmlNodeList xnList)
        {
            FunctionalModels.Clear();
            _nbStructuresCreated = 0;
            if (xnList == null)
                return;
            for (var i = 0; i < xnList.Count;i++) // pour chaque modèle
            {
                var model = xnList[i];
                if (model != null){
                    AddFunctionalModel(FunctionalModelStructure.ImportFunctionalModelStructure(model,i+1),true);
                    foreach (XmlNode element in model.ChildNodes!)
                    {
                        if (element.Name == "Keywords")
                        {
                            foreach (XmlNode keyword in element.ChildNodes)
                            {
                                FunctionalModels[i].Keywords.Add(keyword.InnerText);
                            }
                        }
                    }
                    FunctionalModels[i].UpdateKeywordList();
                }
            }
        }

        /// <summary>
        /// Checks if a Functional Model has the same structure as the ones in the dictionary
        /// </summary>
        /// <param name="functionalModel">Structure to find in the dictionary</param>
        /// <returns>Index of the corresponding structure, or null if not found</returns>
        public int HasSameStructure(FunctionalModel functionalModel)
        {
            var result = -1;
            var i = 0;
            while (i < FunctionalModels.Count && result == -1)
            {
                if (FunctionalModels[i].Model.HasSameStructure(functionalModel))
                    result = i;
                i++;
            }
            return result;
        }
        /// <summary>
        /// The event that occurs when the Dictionary changes. 
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Invokes the event <see cref="PropertyChanged"/> when the BigIntegerItem changes.
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Checks the index of a model with a certain name
        /// </summary>
        /// <param name="name">the name of the model</param>
        /// <returns>the index of the model if found; -1 otherwise.</returns>
        public int CheckName(string name)
        {
            var result = -1;
            for (var i = 0;i<FunctionalModels.Count;i++)
            {
                var model = FunctionalModels[i];
                foreach (var keyword in model.Keywords)
                {
                    if (name.Contains(keyword,StringComparison.OrdinalIgnoreCase))
                    {
                        result = i;
                        return result;
                    }
                }
            }
            return result;
        }
    }
}


