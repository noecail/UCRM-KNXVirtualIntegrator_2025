using System.Collections.ObjectModel;
using System.Xml;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Entities
{
    //Note : J'ai très peur qu'ajouter des listes ou comme ça se fasse par référence et que donc ça pose beaucoup de problèmes
    
    /// <summary>
    /// Represents a functional model with a unique key, name, and DPT (Data Point Type) value.
    ///
    /// This class provides properties for storing and retrieving the key and name of the model.
    /// It includes functionality for displaying the model in a formatted string and implements 
    /// equality comparison based on the DPT value, name, and key. The class is designed to be 
    /// comparable with other instances of the same type to determine equality.
    ///
    /// - Key: Unique identifier for the functional model.
    /// - Name: Descriptive name of the functional model.
    /// 
    /// The class overrides the ToString, Equals, and GetHashCode methods to provide custom
    /// string representation, equality checks, and hash code generation.
    /// </summary>

    public class FunctionalModel : IEquatable<FunctionalModel>
    {
        //Attributes
        public ObservableCollection<TestedElement> ElementList { get; } // The list of elements associated to the model
        public int Key { get; set; } // Identifiant unique des modèles, utilisé notamment sur l'interface "M{Key}"
        public string Name { get; set; } //Nom donn� par l'utilisateur, Modifiable

        //Constructors
        public FunctionalModel(string name)
        {
            ElementList = [];
            Name = name;
        }
        
        public FunctionalModel(List<TestedElement> list, string name)
        {
            ElementList = [];
            foreach (var dpt in list)
            {
                ElementList.Add(dpt);
            }
            Name = name;
        }
        
        public FunctionalModel(FunctionalModel functionalModel, int key, bool copy)
        {
            ElementList = [];
            foreach (var dpt in functionalModel.ElementList)
            {
                ElementList.Add(dpt);
            }
            Key  = key;
            Name = functionalModel.Name;
            if (copy)
                Name += "(copie)";
        }

        //Methods

        public void Rename(string newName)
        {
            Name = newName;
        }
        public void AddElement(TestedElement dpt) //Adds a DPT to the list of DPTs of the functional model
        {
            var dptToAdd = new TestedElement(dpt);
            ElementList.Add(dptToAdd);
        }

        public void RemoveLastElement() //Removes the last DPT added to the functional model
        {
            ElementList.RemoveAt(ElementList.Count - 1);
        }

        public void RemoveElement(int index) //Removes the DPT of the specified index from the functional model
        {
            ElementList.RemoveAt(index);
        }
        
        public override string ToString() //Fonction utilisée dès que l'affichage d'un modèle et demandé, utilisé par la vue.
        {
            if (Name.Contains("Structure"))
                return $"S{Key} | {Name}";
            else 
                return $"M{Key} | {Name}";
        }

        public override bool Equals(object? obj) //Non utilisé, mais vérifie l'unicité d'un modèle, venir modifier en cas d'ajout d'attributs
        {
            if (obj is FunctionalModel other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(FunctionalModel? other)
        {
            if (other is null || other.ElementList.Count != ElementList.Count) return false;
            var result = true;
            for (var i = 0; i < ElementList.Count; i++)
            {
                result &= ElementList[i].IsEqual(other.ElementList[i]);
            }

            return result && Key == other.Key;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ElementList, Name, Key);
        }


        public bool IsPossible()
        {
            var res = true;
            foreach (var element in ElementList)
            {
                res &= element.IsPossible();
            }

            return res;
        }

        public XmlElement ExportFunctionalModel(XmlDocument doc)
        {
            var xModel = doc.CreateElement(Name);
            foreach (var element in ElementList)
            {
                var xElement = doc.CreateElement("Element_to_test");
                var xCmd = doc.CreateElement("Command");
                var xIe = doc.CreateElement("State_information");

                foreach (var dpt in element.TestsCmd)
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

                    xCmd.AppendChild(xDpt);
                }

                foreach (var dpt in element.TestsIe)
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

                    xIe.AppendChild(xDpt);
                }

                xElement.AppendChild(xCmd);
                xElement.AppendChild(xIe);
                xModel.AppendChild(xElement);
            }
            return xModel;
        }

        public static FunctionalModel ImportFunctionalModel(XmlNode model)
        {
            var result = new FunctionalModel(model.Name);
            foreach (XmlNode element in model.ChildNodes)
            {
                var elementToTest = new TestedElement();
                List<DataPointType> listeCmd = [];
                List<DataPointType> listeIe = [];
                foreach (XmlNode node in element.ChildNodes)
                {
                            
                    foreach (XmlNode dpt in node.ChildNodes)
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

                        if (address != null)
                        {
                            if (node.Name == "Command")
                            {
                                elementToTest.AddDptToCmd(type, address, tabValues);
                            }
                            else if (node.Name == "State_information")
                            {
                                elementToTest.AddDptToIe(type, address, tabValues);
                            }
                        }
                    }
                }
                result.ElementList.Add(elementToTest);
            }
            return result;
        }
    }
}

