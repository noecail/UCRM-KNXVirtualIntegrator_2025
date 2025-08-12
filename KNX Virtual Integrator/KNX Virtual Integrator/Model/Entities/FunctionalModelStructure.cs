using System.Collections.ObjectModel;
using System.Xml;
using iText.StyledXmlParser.Jsoup.Nodes;

namespace KNX_Virtual_Integrator.Model.Entities;

public class FunctionalModelStructure
{

    public FunctionalModel Model;
    public struct DptAndKeywords
    {
        public int Key;
        private List<string> _keywords;

        public List<string> Keywords
        {
            get => _keywords;
            set
            {
                UpdateKeywordList();
                _keywords = value; 
                
            }
        }

        private string _allKeywords;
        public string AllKeywords
        {
            get=>_allKeywords;
            set
            {
                _allKeywords = value;
                UpdateKeywords(AllKeywords);
            }
        }

        public DataPointType Dpt;
        /// <summary>
        /// Takes a string, and puts all the keywords inside it into the keywords associated
        /// </summary>
        /// <param name="keywordList">String containing all the keywords separated with commas</param>
        private void UpdateKeywords(string keywordList)
        {
            Keywords = keywordList.Split(',').ToList();
        }
        
        /// <summary>
        /// Takes all the keywords associated to a dpt and group them, separating them with commas
        /// </summary>
        private void UpdateKeywordList()
        {
            if (Keywords == null)
                return;
            if (Keywords.Count > 0)
                AllKeywords = string.Join(',', Keywords);
        }

    }

    public Dictionary<int, DptAndKeywords> DptDictionary { get; set; } = [];

    public struct ElementStructure(List<int> cmd, List<int> ie)
    {
        public List<int> Cmd = cmd;
        public List<int> Ie = ie;
    }
    
    // Gives the same output as ToString method. But ToString does not dynamically change when the name is modified
    // FullName is used to display the Key and the Name in the SelectedModels listbox in the Mainwindow
    public string FullName => $"S{Model.Key} | {Model.Name}";

    public ObservableCollection<ElementStructure> ModelStructure { get; set; } = [];

    public FunctionalModelStructure(string name)
    {
        Model = new FunctionalModel(name);
        ModelStructure = [];
        DptDictionary = [];
    }

    public FunctionalModelStructure(FunctionalModel model, int myKey)
    {
        Model = new FunctionalModel(model,myKey,false);
        ModelStructure = [];
        int index;
        DptDictionary = [];

        foreach (var element in  model.ElementList)
        {
            ElementStructure elementStructure = new ElementStructure();
            foreach (var dpt in element.TestsCmd)
            {
                index = -1;
                var name = dpt.Name;
                foreach (var key in DptDictionary.Keys)
                {
                    if (name == DptDictionary[key].Dpt.Name)
                    {
                        index = key;
                        elementStructure.Cmd?.Add(index);
                        break;
                    }
                }

                if (index == -1)
                {
                    var newDpt = new DptAndKeywords();
                    newDpt.Dpt = dpt;
                    newDpt.Keywords = [];
                    int newKey;
                    if (DptDictionary.Keys.ToList().Count > 0)
                    {
                         newKey = DptDictionary.Keys.ToList()[^1] + 1;
                    }
                    else
                    {
                        newKey = 0;
                    }
                    newDpt.Key = newKey;

                    DptDictionary.Add(newKey, newDpt);
                    elementStructure.Ie?.Add(index);
                }
                
            }
            foreach (var dpt in element.TestsIe)
            {
                index = -1;
                var name = dpt.Name;
                foreach (var key in DptDictionary.Keys)
                {
                    if (name == DptDictionary[key].Dpt.Name)
                    {
                        index = key;
                        elementStructure.Ie?.Add(index);
                        break;
                    }
                }

                if (index == -1)
                {
                    var newDpt = new DptAndKeywords();
                    newDpt.Dpt = dpt;
                    newDpt.Keywords = [];
                    var newKey = DptDictionary.Keys.ToList()[^1] + 1;
                    newDpt.Key = newKey;
                    DptDictionary.Add(newKey, newDpt);
                    elementStructure.Ie?.Add(index);
                }
            }
            ModelStructure.Add(elementStructure);
        }

    }

    
    public FunctionalModelStructure(FunctionalModel model, string myName, int myKey)
    {
        Model = new FunctionalModel(model, myKey, false);
        ModelStructure = [];
        int index;
        DptDictionary = [];

        foreach (var element in  model.ElementList)
        {
            ElementStructure elementStructure = new ElementStructure();
            foreach (var dpt in element.TestsCmd)
            {
                index = -1;
                var name = dpt.Name;
                foreach (var key in DptDictionary.Keys)
                {
                    if (name == DptDictionary[key].Dpt.Name)
                    {
                        index = key;
                        elementStructure.Cmd?.Add(index);
                        break;
                    }
                }

                if (index == -1)
                {
                    var newDpt = new DptAndKeywords();
                    newDpt.Dpt = dpt;
                    newDpt.Keywords = [];
                    int newKey;
                    if (DptDictionary.Keys.ToList().Count > 0)
                    {
                         newKey = DptDictionary.Keys.ToList()[^1] + 1;
                    }
                    else
                    {
                        newKey = 0;
                    }
                    newDpt.Key = newKey;
                    DptDictionary.Add(newKey, newDpt);
                    elementStructure.Ie?.Add(index);
                }
                
            }
            foreach (var dpt in element.TestsIe)
            {
                index = -1;
                var name = dpt.Name;
                foreach (var key in DptDictionary.Keys)
                {
                    if (name == DptDictionary[key].Dpt.Name)
                    {
                        index = key;
                        elementStructure.Ie?.Add(index);
                        break;
                    }
                }

                if (index == -1)
                {
                    var newDpt = new DptAndKeywords();
                    newDpt.Dpt = dpt;
                    newDpt.Keywords = [];
                    var newKey = DptDictionary.Keys.ToList()[^1] + 1;
                    newDpt.Key = newKey;
                    DptDictionary.Add(newKey, newDpt);
                    elementStructure.Ie?.Add(index);
                }
            }
            ModelStructure.Add(elementStructure);
        }
        Model.Key = myKey;
        
        Model.Name = myName;

    }

    
    public FunctionalModelStructure(string name, Dictionary<int, DptAndKeywords> functionalModels,
        ObservableCollection<ElementStructure> modelStructure) 
    {
        DptDictionary = new Dictionary<int, DptAndKeywords>(functionalModels);
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure);
        Model = BuildFunctionalModel(name);

    }
    
    public FunctionalModelStructure(FunctionalModelStructure modelStructure) 
    {
        Model = new FunctionalModel(modelStructure.Model, modelStructure.Model.Key,false);
        DptDictionary = new Dictionary<int, DptAndKeywords>(modelStructure.DptDictionary);
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure.ModelStructure);
    }
    
    
    

    /// <summary>
    /// Creates a functional model from the list of all the DPTs of a model and its structure
    /// </summary>
    /// <param name="dataPoints"> List of DPTs</param>
    /// <returns>The functional model filled with the DPTs according to the structure</returns>
    public void RetrieveFunctionalModel(List<DataPointType> dataPoints)
    {
        var model = new FunctionalModel(Model.Name);
        var dico = new Dictionary<int, DataPointType>();
        foreach (var dataPoint in dataPoints)
        {
            dico[GetKey(dataPoint)]= dataPoint;
        }
        
        foreach (var elementStructure in ModelStructure)
        {
            model.AddElement(new  TestedElement());
            foreach (var cmd in elementStructure.Cmd)
            {
                model.ElementList[^1].AddDptToCmd(dico[cmd]);
            }

            foreach (var ie in elementStructure.Ie)
            {
                model.ElementList[^1].AddDptToIe(dico[ie]);
            }
        }
        
        Model = model;
    }
    

    /// <summary>
    /// Finds the key of the DPT in the dictionary corresponding to the argument
    /// </summary>
    /// <param name="dataPoint">DPT to find in the dictionary</param>
    /// <returns> The key of the corresponding DPT in the dictionary</returns>
    private int GetKey(DataPointType dataPoint)
    {
        foreach (var i in DptDictionary.Keys)
        {
            if (DptDictionary[i].Keywords.Any(word => dataPoint.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                return i;
            }
        }

        return -1;
    }
    
    public void AddElement()
    {
        ModelStructure.Add(new ElementStructure());
    }

    public FunctionalModel BuildFunctionalModel(string name)
    {
        var res = new FunctionalModel(name);
        foreach (var element in ModelStructure)
        {
            res.AddElement(new TestedElement());
            foreach (var cmd in element.Cmd)
            {
                var dptToAdd = new DataPointType(DptDictionary[cmd].Dpt);
                for (var i = 0; i < dptToAdd.IntValue.Count; i++)
                    dptToAdd.IntValue[i] = new DataPointType.BigIntegerItem(dptToAdd.IntValue[i].BigIntegerValue?? 0); //May look useless but allows to create a new instance of a dpt insead of copying the reference
                res.ElementList[^1].AddDptToCmd(dptToAdd);
            }
            foreach (var ie in element.Ie)
            {
                var dptToAdd = new DataPointType(DptDictionary[ie].Dpt);
                for (var i = 0; i < dptToAdd.IntValue.Count; i++)
                    dptToAdd.IntValue[i] = new DataPointType.BigIntegerItem(dptToAdd.IntValue[i].BigIntegerValue?? 0); //May look useless but allows to create a new instance of a dpt insead of copying the reference
                res.ElementList[^1].AddDptToIe(dptToAdd);
            }
        }
        return res;
    }


    public static FunctionalModelStructure ImportFunctionalModelStructure(XmlNode model)
    {
        var name = model.Name;
        var res = new FunctionalModelStructure(name);

        foreach (XmlNode dicoOrModelStructure in model.ChildNodes)
        {
            if (dicoOrModelStructure.Name == "Model_Structure")
            {
                foreach (XmlNode element in dicoOrModelStructure.ChildNodes)
                {
                    ElementStructure elementStructure = new ElementStructure();
                    foreach (XmlNode xElement in element.ChildNodes)
                    {
                        var cmdList = new List<int>();
                        var ieList = new List<int>();

                        foreach (XmlNode keyList in xElement.ChildNodes)
                        {
                            if (keyList.Name == "Command")
                            {
                                foreach (XmlNode cmd in keyList.ChildNodes)
                                {
                                    cmdList.Add(int.Parse(cmd.Attributes?["Key"]?.Value ?? ""));
                                }
                            }

                            if (keyList.Name == "State_information")
                            {
                                foreach (XmlNode ie in keyList.ChildNodes)
                                {
                                    ieList.Add(int.Parse(ie.Attributes?["Key"]?.Value ?? ""));
                                }
                            }
                        }

                        elementStructure.Cmd = cmdList;
                        elementStructure.Ie = ieList;
                    }

                    res.ModelStructure.Add(elementStructure);
                }
            } else if (dicoOrModelStructure.Name == "Dictionary")
            {
                res.DptDictionary = [];
                foreach (XmlNode xPair in dicoOrModelStructure.ChildNodes)
                {
                    var key = int.Parse(xPair.Attributes?["Key"]?.Value ?? "");
                    var pair = new DptAndKeywords();
                    pair.Dpt = new DataPointType();
                    foreach (XmlNode xDptAndKeywords in xPair.ChildNodes)
                    {
                        foreach (XmlNode xTypesOrKeywordsOrValues in xDptAndKeywords.ChildNodes)
                        {
                            if (xTypesOrKeywordsOrValues.Name == "Type")
                            {
                                pair.Dpt.Type = int.Parse(xTypesOrKeywordsOrValues.Attributes?["Value"]?.Value ?? "0");
                            } else if (xTypesOrKeywordsOrValues.Name == "Keywords")
                            {
                                foreach (XmlNode xKeyword in xTypesOrKeywordsOrValues.ChildNodes)
                                {
                                    pair.Keywords?.Add(xKeyword.Attributes?["Keyword"]?.Value ?? ""); //Then  have to be checked 
                                }
                                
                            } else if (xTypesOrKeywordsOrValues.Name == "Values")
                            {
                                foreach (XmlNode xValue in xTypesOrKeywordsOrValues.ChildNodes)
                                {
                                    pair.Keywords?.Add(xValue.Attributes?["Value"]?.Value ?? ""); //Then  have to be checked 
                                }
                            }
                        }
                    }
                    res.DptDictionary[key] = pair;
                }
            }
        }
        
        res.Model = res.BuildFunctionalModel(name);
        res.Model.UpdateIntValue();
        return res;
    }
    
    
     public XmlElement ExportFunctionalModelStructure(XmlDocument doc)
        {
            var xModel = doc.CreateElement(Model.Name);
            var xModelStructure = doc.CreateElement("Model_Structure");
            foreach (var element in ModelStructure)
            {
                var xElement = doc.CreateElement("Element_to_test");
                var xCmd = doc.CreateElement("Command");
                var xIe = doc.CreateElement("State_information");
                foreach (var cmd in element.Cmd)
                {
                    var key = doc.CreateAttribute("Key");
                    key.Value = cmd.ToString();
                    xCmd.Attributes.Append(key);
                }
                foreach (var ie in element.Ie)
                {
                    var key = doc.CreateAttribute("Key");
                    key.Value = ie.ToString();
                    xIe.Attributes.Append(key);
                }
                xElement.AppendChild(xCmd);
                xElement.AppendChild(xIe);
                xModelStructure.AppendChild(xElement);

            }
            var xDictionary = doc.CreateElement("Dictionary");
            foreach (var element in DptDictionary)
            {
                var xPair = doc.CreateElement("Pair");
                var key = doc.CreateAttribute("Key");
                key.Value = element.Key.ToString();
                var xDptAndKeywords = doc.CreateElement("Dpt");
                var xDptType = doc.CreateAttribute("Type");
                var xDptKeywords = doc.CreateElement("Keywords");
                var xDptValues = doc.CreateElement("Values");

                foreach (var keyword in element.Value.Keywords)
                {
                    var xKeyword  = doc.CreateElement("Keyword");
                    xKeyword.Value = keyword;
                    xDptValues.AppendChild(xKeyword);
                }
                foreach (var value in element.Value.Dpt.Value)
                {
                    var xValue  = doc.CreateElement("Value");
                    xValue.Value = value?.ToString();
                    xDptValues.AppendChild(xValue);
                }
                
                xDptAndKeywords.AppendChild(xDptType);
                xDptAndKeywords.AppendChild(xDptKeywords);
                xDptAndKeywords.AppendChild(xDptValues);
                xPair.AppendChild(xDptAndKeywords);
                xPair.Attributes.Append(key);
                xDictionary.AppendChild(xPair);
            }
            xModel.AppendChild(xDictionary);
            xModel.AppendChild(xModelStructure);
            return xModel;
        }

        public override string ToString()
        {
              return $"S{Model.Key} | {Model.Name}";
        }

        public void CreateDpt()
        {
            var newKey = DptDictionary.Keys.ToList().Last() + 1;
            DptDictionary.Add(newKey,new DptAndKeywords(){Key = newKey,Keywords = [],Dpt = new DataPointType(1)});
        }
        
        public void RemoveDpt(int index)
        {
            DptDictionary.Remove(index);
        }

}