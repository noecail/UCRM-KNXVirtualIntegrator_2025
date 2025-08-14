using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Xml;


namespace KNX_Virtual_Integrator.Model.Implementations
{
    //Find Summary in the interface
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        private ObservableCollection<FunctionalModelStructure> _functionalModels =[];
        
        
        private int _nbStructuresCreated ;

        public ObservableCollection<FunctionalModelStructure> FunctionalModels
        {
            get => _functionalModels;
            set
            {
                if (_functionalModels.Equals(value)) return;
                _functionalModels = value;
            }
        }


        public FunctionalModelDictionary()
        {
            FunctionalModels = [];
            /*AddFunctionalModel(new FunctionalModelStructure("Lumiere_ON_OFF",
                        new Dictionary<int, FunctionalModelStructure.DptAndKeywords>()
                        {
                            {
                                1, new FunctionalModelStructure.DptAndKeywords { Keywords = ["On/off"], Dpt = new DataPointType(1) }
                            }, //CMD
                            {
                                2, new FunctionalModelStructure.DptAndKeywords { Keywords = ["etat On/Off"], Dpt = new DataPointType(1) }
                            } //IE
                        },
                        [new FunctionalModelStructure.ElementStructure([1],[2])]
                    ),false,["Lumiere on/off", "Lumiere on-off", "Lumiere on_off", "Light on/off", "Eclairage_Simple"]
                );  //Adding On/Off light functional model
            AddFunctionalModel(new FunctionalModelStructure("Lumiere_Variation",
                        new Dictionary<int, FunctionalModelStructure.DptAndKeywords>()
                        {
                            {
                                1, new FunctionalModelStructure.DptAndKeywords { Keywords = ["On/off"], Dpt = new DataPointType(1) }
                            }, //CMD on/off
                            {
                                2, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Etat On/off"], Dpt = new DataPointType(1) }
                            }, //IE  on/off
                            {
                                3, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Variations"], Dpt = new DataPointType(3) }
                            }, //CMD relative
                            {
                                4, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Valeurs variation"], Dpt = new DataPointType(5) }
                            }, //CMD absolue
                            {
                                5, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Etats variation"], Dpt = new DataPointType(5) }
                            }, //IE  absolue
                        },
                        [new FunctionalModelStructure.ElementStructure([1],[2,5]), // test on/off
                            new FunctionalModelStructure.ElementStructure([3],[2,2,5]), //test relative command
                            new FunctionalModelStructure.ElementStructure([4],[2,2,5]) //test absolute command
                        ]
                    ),false, ["Lumiere variation", "variation", "Lumiere_variation", "Light variation", "Eclairages_variable"]
                );
            
            
            AddFunctionalModel(new FunctionalModelStructure("Store",
                    new Dictionary<int, FunctionalModelStructure.DptAndKeywords>()
                    {
                        {
                            1, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Montee/descente"], Dpt = new DataPointType(1) }
                        }, //CMD on/off
                        {
                            2, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Etats Montee/descente"], Dpt = new DataPointType(1) }
                        }, //IE  on/off
                        {
                            3, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Stop"], Dpt = new DataPointType(1) }
                        }, //Stop
                        {
                            4, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Position"], Dpt = new DataPointType(5) }
                        }, //CMD absolue
                        {
                            5, new FunctionalModelStructure.DptAndKeywords { Keywords = ["Etat Position"], Dpt = new DataPointType(5) }
                        }, //IE  absolue
                    },
                    [new FunctionalModelStructure.ElementStructure([1],[2,5]), // test on/off
                        new FunctionalModelStructure.ElementStructure([1,3],[2,2,5]), //test stop
                        new FunctionalModelStructure.ElementStructure([4],[2,2,5]) //test absolute command
                    ]
                ),false, ["stre", "blind", "Volets_roulants"]
            );
            AddFunctionalModel(new FunctionalModelStructure("Commutation",
                        new Dictionary<int, FunctionalModelStructure.DptAndKeywords>()
                        {
                            {
                                1,
                                new FunctionalModelStructure.DptAndKeywords
                                    { Keywords = ["On/Off"], Dpt = new DataPointType(1) }
                            }, //CMD
                            {
                                2,
                                new FunctionalModelStructure.DptAndKeywords
                                    { Keywords = ["Etat On/off"], Dpt = new DataPointType(1) }
                            } //IE
                        },
                        [new FunctionalModelStructure.ElementStructure([1], [2])]
                    ), false,["Commute", "Commutation", "Convecteur", "Prise", "Arrosage", "Ouvrant"]
                );*/
            ImportDictionary(@"C:\Users\manui\Documents\Stage 4A\Test\Pray.xml");
        }

        public void AddKeyword(int index, string word)
        {
            FunctionalModels[index].Keywords.Add(word.ToLower());
        }
        
        public FunctionalModelDictionary(string path)
        {
            FunctionalModels = [];
            ImportDictionary(path);

        }

        
        public void AddFunctionalModel(FunctionalModelStructure functionalModel, bool imported)
        {
            var newModel = new FunctionalModelStructure(functionalModel);
            _nbStructuresCreated++;
            if (newModel.Model.Name == "New_Structure")
                newModel.Model.Name += "_" + _nbStructuresCreated;
            else if (!functionalModel.Model.Name.Contains("Structure"))
                newModel.Model.Name += "_Structure";
            if (imported == false)
                newModel.Model = newModel.BuildFunctionalModel(newModel.Model.Name);
            newModel.Model.Key = FunctionalModels.Count +1 ;

            FunctionalModels.Add(newModel);
            OnPropertyChanged(nameof(FunctionalModels));
            
        }

        public void AddFunctionalModel(FunctionalModelStructure functionalModel, bool imported, List<string> keywords)
        {
            var newModel = new FunctionalModelStructure(functionalModel);
            foreach (var keyword in keywords)
            {
                newModel.Keywords.Add(keyword);
                newModel.Keywords.Add(keyword.Replace(' ','_'));
                newModel.Keywords.Add(keyword.Replace('_',' '));
            }

            _nbStructuresCreated++;
            if (newModel.Model.Name == "New_Structure")
                newModel.Model.Name += "_" + _nbStructuresCreated;
            else if (!functionalModel.Model.Name.Contains("Structure"))
                newModel.Model.Name += "_Structure";
            if (imported == false)
                newModel.Model = newModel.BuildFunctionalModel(newModel.Model.Name);
            newModel.Model.Key = FunctionalModels.Count + 1;

            FunctionalModels.Add(newModel);
            OnPropertyChanged(nameof(FunctionalModels));
            Console.WriteLine(FunctionalModels[0].AllKeywords);

    }

        public void RemoveFunctionalModel(int index)
        {
            FunctionalModels.RemoveAt(index);
            OnPropertyChanged(nameof(FunctionalModels));
        }

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
        /// Imports a functional model dictionary from a path.
        /// </summary>
        /// <param name="path">Path of the dictionary</param>
        public void ImportDictionary(string path)
        {
            FunctionalModels.Clear();
            foreach (var functionalModel in FunctionalModels)
            {
                functionalModel.Keywords =[];
            }
            var doc = new XmlDocument();
            doc.Load(path);
            var xnList = doc.DocumentElement?.ChildNodes;
            if (xnList == null)
                return;
            for (var i = 0; i < xnList.Count;i++) // pour chaque modèle
            {
                var model = xnList[i];
                if (model != null){
                    AddFunctionalModel(FunctionalModelStructure.ImportFunctionalModelStructure(model),true);
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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


