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
        private ObservableCollection<FunctionalModel> _functionalModels =[];
        
        private List<List<string>> _keywordsDictionary = [];

        public ObservableCollection<FunctionalModel> FunctionalModels
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
            AddFunctionalModel(new FunctionalModel([new TestedElement([1],["0/1/1"],[[new GroupValue(true), new GroupValue(false)]],[1],["0/2/1"],[[new GroupValue(true), new GroupValue(false)]])],
                "Lumiere_ON_OFF")); //Adding On/Off light functional model
            AddFunctionalModel(new FunctionalModel([
                new TestedElement([1],[""], [[new GroupValue(true), new GroupValue(false)]], [1,5],["",""],
                    [[new GroupValue(true), new GroupValue(false)], [new GroupValue(0xFF),new GroupValue(0x00)]]), // Variation functional model : First element : On/Off
                new TestedElement([5], [""], [[new GroupValue(0xFF), new GroupValue(0x4F)]], [5], ["", ""],
                    [[new GroupValue(0xFF), new GroupValue(0x4F)]]), //Second element : absolute change
                new TestedElement([3], [""], [[new GroupValue(0x4)]],[5],[""],[[new GroupValue([0x2E])]])
            ], "Lumiere_variation")); //Third element : relative change
            AddFunctionalModel(new FunctionalModel([
            new TestedElement([1],[""], [[new GroupValue(true), new GroupValue(false)]], [1,1,5],["","",""],
                [[new GroupValue(true), new GroupValue(true)],[new GroupValue(false), new GroupValue(false)],[new GroupValue(0xFF),new GroupValue(0x00)]]), //On/Off command 
            new TestedElement([1,1],["",""],[[new GroupValue(true)],[new GroupValue(true)]],[1,1,5],["","",""],
                [[new GroupValue(true)],[new GroupValue(false)],[null]]),//Stop
            new TestedElement([5], [""],[[new GroupValue(0xFF),new GroupValue(0x00)]],[1,1,5],["","",""],//Absolute command
                [[new GroupValue(true),new GroupValue(true)],[new GroupValue(false),new GroupValue(false)],[new GroupValue(0xFF),new GroupValue(0x00)]]),
            ],"Store"));
            AddFunctionalModel(new FunctionalModel([
            new TestedElement([1],[""],[[new GroupValue(true), new GroupValue(false)]],[1],[""],[[new GroupValue(true), new GroupValue(false)]])  //On/Off
                ],"Commutation")); //Convection, Prise, Arrosage, Portail
            _keywordsDictionary.Add([]);
            _keywordsDictionary[0].Add("Lumiere on/off");
            _keywordsDictionary[0].Add("Lumiere on-off");
            _keywordsDictionary[0].Add("Lumiere on_off");
            _keywordsDictionary[0].Add("Light on/off");

            _keywordsDictionary.Add([]);
            _keywordsDictionary[1].Add("Lumiere variation");
            _keywordsDictionary[1].Add("variation");
            _keywordsDictionary[1].Add("Lumiere_variation");
            _keywordsDictionary[1].Add("Light variation");
            
            _keywordsDictionary.Add([]);
            _keywordsDictionary[2].Add("store");
            _keywordsDictionary[2].Add("blind");
            
            _keywordsDictionary.Add([]);
            _keywordsDictionary[3].Add("Commute");
            _keywordsDictionary[3].Add("Commutation");
        }

        public FunctionalModelDictionary(string path)
        {
            FunctionalModels = [];
            ImportDictionary(path);

        }


        public void AddFunctionalModel(FunctionalModel functionalModel)
        {
            if (functionalModel.Name == "New_Structure")
                functionalModel.Name += "_" + (FunctionalModels.Count+1);
            else if (!functionalModel.Name.Contains("Structure"))
                functionalModel.Name += "_Structure";
            FunctionalModels.Add(functionalModel);
            FunctionalModels.Last().Key = FunctionalModels.Count;
            OnPropertyChanged(nameof(FunctionalModels));
        }

        public void RemoveFunctionalModel(int index)
        {
            FunctionalModels.RemoveAt(index);
            OnPropertyChanged(nameof(FunctionalModels));
        }

        public List<FunctionalModel> GetAllModels()
        {
            var liste = new List<FunctionalModel>();
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
                var functionalModel = model.ExportFunctionalModel(doc);
                var keywords = doc.CreateElement("Keywords");
                foreach (var keyword in _keywordsDictionary[model.Key - 1])
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
            _keywordsDictionary.Clear();
            var doc = new XmlDocument();
            doc.Load(path);
            var xnList = doc.DocumentElement?.ChildNodes;
            if (xnList == null)
                return;
            for (var i = 0; i < xnList.Count;i++) // pour chaque modèle
            {
                var model = xnList[i];
                if (model != null)
                    AddFunctionalModel(FunctionalModel.ImportFunctionalModel(model));
                _keywordsDictionary.Add([]);
                foreach (XmlNode element in model?.ChildNodes!)
                {
                    if (element.Name == "Keywords")
                    {
                        foreach (XmlNode keyword in element.ChildNodes)
                        {
                            _keywordsDictionary[i].Add(keyword.InnerText);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a Functional Model hase the same structure as the ones in the dictionary
        /// </summary>
        /// <param name="functionalModel">Structure to find in the dictionary</param>
        /// <returns>Index of the corresponding structure, or null if not found</returns>
        public int? HasSameStructure(FunctionalModel functionalModel)
        {
            int? result = null;
            var i = 0;
            while (i < FunctionalModels.Count && result == null)
            {
                if (FunctionalModels[i].HasSameStructure(functionalModel))
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
    }
}


