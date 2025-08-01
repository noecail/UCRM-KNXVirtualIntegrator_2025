using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Entities
{
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

    public class FunctionalModel : IEquatable<FunctionalModel>, INotifyPropertyChanged
    {
        //Attributes
        public ObservableCollection<TestedElement> ElementList { get; } // The list of elements associated to the model
        public int Key { get; set; } // Identifiant unique des modèles, utilisé notamment sur l'interface "M{Key}"

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        // Gives the same output as ToString method. But ToString does not dynamically change when the name is modified
        // FullName is used to display the Key and the Name in the SelectedModels listbox in the Mainwindow
        public string FullName
        {
            get
            {
                if (Name.Contains("Structure"))
                    return $"S{Key} | {Name}";
                else 
                    return $"M{Key} | {Name}";
            }
        }

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
        
        public FunctionalModel(string name, int key)
        {
            Name = name;
            ElementList = [];
            Key  = key;
        }

        //Methods

        public void Rename(string newName)
        {
            Name = newName;
        }
        public void AddElement(TestedElement dpt) //Adds an element to the list of elements of the functional model
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
            return Key.GetHashCode();
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
            UpdateValue();
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
                if (element.Name != "Element_to_test")
                    continue; // Ignore keywords

                var elementToTest = new TestedElement();
                foreach (XmlNode node in element.ChildNodes)
                {
                    foreach (XmlNode dpt in node.ChildNodes)
                    {
                        var address = dpt?.Attributes?["Address"]?.Value;
                        var type = 0;
                        if (dpt?.Attributes?["Type"]?.Value != null && dpt.Attributes["Type"] != null)
                            type = int.Parse(dpt.Attributes is null? dpt.Attributes!["Type"]!.Value : "0");
                        List<GroupValue?> tabValues = [];
                        foreach (XmlNode values in dpt?.ChildNodes!)
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
            result.UpdateIntValue();
            return result;
        }

        public bool HasSameStructure(FunctionalModel functionalModel)
        {
            var result = ElementList.Count == functionalModel.ElementList.Count;
            for (var i = 0; i < ElementList.Count; i++)
            {
                result = result &&  ElementList[i] == functionalModel.ElementList[i]; // Checks if all the cmd DPTs in each element are of the same type and if the elements have the same number of cmd dpt
            }
            return result;
        }
        
        /// <summary>
        /// Updates the GroupValue arrays of all DPTs
        /// </summary>
        public void UpdateValue()
        {
            foreach (var element in ElementList)
            {
                element.UpdateValue();
            }
        }
        /// <summary>
        /// Updates the BigInteger arrays of all DPTs
        /// </summary>
        public void UpdateIntValue()
        {
            foreach (var element in ElementList)
            {
                element.UpdateIntValue();
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

