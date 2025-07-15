using System;
using System.Linq;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;
using System.Xml;
using System.Xml.Linq;


namespace KNX_Virtual_Integrator.Model.Implementations
{
    //Find Summary in the interface
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        public Dictionary<int, FunctionalModel> FunctionalModels;
        private int _currentKey;

        public FunctionalModelDictionary()
        {
            FunctionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
            Add_FunctionalModel(new FunctionalModel([new TestedElement(1,"0/1/1",[new GroupValue(true)],[1],["0/2/1"],[[new GroupValue(true)]])],
                "Lumiere_ON_OFF")); //Adding On/Off light functional model
            Add_FunctionalModel(new FunctionalModel([
                new TestedElement(1, "", [new GroupValue(true)], [1], [""],
                    [[new GroupValue(true)]]), // Variation functional model : First element : On/Off
                new TestedElement(5, "", [new GroupValue(0xF)], [1, 5], ["", ""],
                    [[new GroupValue(true)], [new GroupValue(0xF)]]), //Second element : absolute change
                new TestedElement(3, "", [new GroupValue(0x8)])
            ], "Lumiere_variation")); //Third element : relative change, no IE
        }

        public FunctionalModelDictionary(string path)
        {
            FunctionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
            Add_FunctionalModel(new FunctionalModel([new TestedElement()],
                "Lumière ON/OFF")); //Adding On/Off light functional model
            Add_FunctionalModel(new FunctionalModel([
                new TestedElement(1, "", [new GroupValue(true)], [1], [""],
                    [[new GroupValue(true)]]), // Variation functional model : First element : On/Off
                new TestedElement(5, "", [new GroupValue(0xF)], [1, 5], ["", ""],
                    [[new GroupValue(true)], [new GroupValue(0xF)]]), //Second element : absolute change
                new TestedElement(3, "", [new GroupValue(0x8)])
            ], "Lumière variation")); //Third element : relative change, no IE
        }


        public void Add_FunctionalModel(FunctionalModel functionalModel)
        {
            _currentKey++;
            functionalModel.Key = _currentKey; // Associer la cl� au mod�le
            FunctionalModels.Add(_currentKey, functionalModel);
        }

        public void Remove_FunctionalModel(int key)
        {
            FunctionalModels.Remove(key);
        }

        public List<FunctionalModel> GetAllModels()
        {
            return new List<FunctionalModel>(FunctionalModels.Values);
        }

        /// <summary>
        /// Creates a XML file representing the dictionary.
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
                    var xelement = doc.CreateElement("Element_to_test");

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
                        xelement.AppendChild(xDpt);
                    }

                    functionalModel.AppendChild(xelement);
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
            var xnList = doc.DocumentElement.ChildNodes;
            foreach (XmlNode model in xnList) // pour chaque modèle
            {
                var functionalmodel = new FunctionalModel(model.Name);
                foreach (XmlNode element in model.ChildNodes)
                {
                    var elementtotest = new TestedElement();
                    
                    foreach (XmlNode dpt in element.ChildNodes)
                    {
                        var address = dpt.Attributes["Address"].Value;
                        var type = int.Parse(dpt.Attributes["Type"].Value);
                        List<GroupValue> tabvalues = [];
                        foreach (XmlNode values in dpt.ChildNodes)
                        {
                            if (values.Name == "Group_Value" && values.Attributes["Value"] != null)
                            {
                                tabvalues.Add(GroupValue.Parse(values.Attributes["Value"].Value));
                            }
                            
                        }
                        elementtotest.AddDpt(type,address, tabvalues);
                    }
                    functionalmodel.ElementList.Add(elementtotest);
                }
                FunctionalModels.Add(++_currentKey,functionalmodel);
            }
        }
    }
}


