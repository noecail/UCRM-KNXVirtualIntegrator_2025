using System.ComponentModel;
using System.Runtime.CompilerServices;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Xml;


namespace KNX_Virtual_Integrator.Model.Implementations
{
    //Find Summary in the interface
    public class FunctionalModelDictionary : IFunctionalModelDictionary, INotifyPropertyChanged

    {
        private Dictionary<int, FunctionalModel> _functionalModels =[];

        public Dictionary<int, FunctionalModel> FunctionalModels
        {
            get => _functionalModels;
            set
            {
                if (_functionalModels.Equals(value)) return;
                _functionalModels = value;
            }
        }

        private int _currentKey;

        public FunctionalModelDictionary()
        {
            FunctionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
            AddFunctionalModel(new FunctionalModel(
                [new TestedElement([1], ["0/1/1"], [[new GroupValue(true)]], [1], ["0/2/1"], [[new GroupValue(true)]])],
                "Lumiere_ON_OFF")); //Adding On/Off light functional model
            AddFunctionalModel(new FunctionalModel([
                new TestedElement([1],[""], [[new GroupValue(true), new GroupValue(false)]], [1,5],["",""],
                    [[null, new GroupValue(false)], [null,new GroupValue(0x00)]]), // Variation functional model : First element : On/Off
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
            new TestedElement([1],[""],[[new GroupValue(true), new GroupValue(false)]],[1],[""],[[null, new GroupValue(false)]])  //On/Off
                ],"Convecteur"));
            AddFunctionalModel(new FunctionalModel([
                new TestedElement([1],[""],[[new GroupValue(true), new GroupValue(false)]],[1],[""],[[null, new GroupValue(false)]])
            ],"Prise"));
            AddFunctionalModel(new FunctionalModel([
                new TestedElement([1],[""],[[new GroupValue(true), new GroupValue(false)]],[1],[""],[[null, new GroupValue(false)]])
            ],"Arrosage"));
            AddFunctionalModel(new FunctionalModel([
                new TestedElement([1],[""],[[new GroupValue(true), new GroupValue(false)]],[1],[""],[[null, new GroupValue(false)]])
            ],"Ouvrant"));
            
            
        }

        public FunctionalModelDictionary(string path)
        {
            FunctionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
            ImportDictionary(path);

        }


        public void AddFunctionalModel(FunctionalModel functionalModel)
        {
            _currentKey++;
            if (functionalModel.Name == "New Model")
                functionalModel.Name += " " + _currentKey;
            functionalModel.Key = _currentKey; // Associer la cl� au mod�le
            FunctionalModels.Add(_currentKey, functionalModel);
            OnPropertyChanged(nameof(FunctionalModels));
        }

        public void RemoveFunctionalModel(int key)
        {
            FunctionalModels.Remove(key);
            OnPropertyChanged(nameof(FunctionalModels));
        }

        public List<FunctionalModel> GetAllModels()
        {
            return [..FunctionalModels.Values];
        }

        /// <summary>
        /// Creates an XML file representing the dictionary.
        /// </summary>
        /// <param name="path">Path where the XML has to be exported </param>
        public void ExportDictionary(string path)
        {
            var doc = new XmlDocument();
            var project = doc.CreateElement("Project");

            foreach (var model in GetAllModels())
            {
                var functionalModel = doc.CreateElement(model.Name);
                foreach (var element in model.ElementList)
                {
                    var xElement = doc.CreateElement("Element_to_test");

                    foreach (var dpt in element.Tests)
                    {
                        var xDpt = doc.CreateElement("Data_Point_Type");
                        var addr = doc.CreateAttribute("Address");
                        addr.Value = dpt.Address;
                        xDpt.Attributes.Append(addr);
                        var type = doc.CreateAttribute("Type");
                        type.Value = dpt.Type.ToString();
                        xDpt.Attributes.Append(type);

                        foreach (var value in dpt.Value)
                        {
                            if (value != null)
                            {
                                var xValue = doc.CreateElement("Group_Value");
                                var val = doc.CreateAttribute("Value");
                                val.Value = value.ToString();
                                xValue.Attributes.Append(val);
                                xDpt.AppendChild(xValue);
                            }
                        }
                        xElement.AppendChild(xDpt);
                    }

                    functionalModel.AppendChild(xElement);
                }

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
            _currentKey = 0;
            var doc = new XmlDocument();
            doc.Load(path);
            var xnList = doc.DocumentElement?.ChildNodes;
            if (xnList != null)
                foreach (XmlNode model in xnList) // pour chaque modèle
                {
                    var functionalModel = new FunctionalModel(model.Name);
                    foreach (XmlNode element in model.ChildNodes)
                    {
                        var elementToTest = new TestedElement();
                        
                        foreach (XmlNode dpt in element.ChildNodes)
                        {
                            var address = dpt?.Attributes?["Address"]?.Value; 
                            var type = int.Parse(dpt?.Attributes?["Type"]?.Value);
                            List<GroupValue?> tabValues = [];
                            foreach (XmlNode values in dpt.ChildNodes)
                            {
                                if (values.Name == "Group_Value")
                                {
                                    tabValues.Add(GroupValue.Parse(values.Attributes?["Value"]?.Value));
                                }
                                
                            }
                            if (address != null )
                                elementToTest.AddDpt(type,address, tabValues);
                        }
                        functionalModel.ElementList.Add(elementToTest);
                    }
                    FunctionalModels.Add(++_currentKey,functionalModel);
                }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


