using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using Knx.Falcon;
using Org.BouncyCastle.Math;

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

        public DataPointType Dpt { get; set; }
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

    public ObservableDictionary<int, DptAndKeywords> DptDictionary { get; set; }

    public ObservableCollection<string> DptNames { get; } = new();
    
    public static List<int> DefaultDptToChoose { get; } = new List<int>
    {
        1,2,23,24,28,31,3,4,5,6,17,18,20,21,25,26,236,238,200,
        7,8,9,22,201,202,204,207,211,217,234,237,239,244,246,10,11,
        30,203,205,206,209,223,225,232,240,250,254,215,216,218,248,252,
        245,212,221,222,224,229,235,242,251,257,274,271,272,19,29,213,
        219,230,243,255,273,265,267,268,247,266,16,269,277,278,279,280,
        281,282,283,284,256,270
    };
    
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
        SetUpDptNamesUdpate();
    }

    private List<string> _keywords = [];

    public List<string> Keywords
    {
        get => _keywords;
        set
        {
            UpdateKeywordList();
            _keywords = value; 
                
        }
    }

    private string _allKeywords = "";
    public string AllKeywords
    {
        get=>_allKeywords;
        set
        {
            _allKeywords = value;
            UpdateKeywords();
        }
    }

    /// <summary>
    /// Takes a string, and puts all the keywords inside it into the keywords associated
    /// </summary>
    /// <param name="keywordList">String containing all the keywords separated with commas</param>
    private void UpdateKeywords()
    {
        Keywords = AllKeywords.Split(',').ToList();
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
    public FunctionalModelStructure(FunctionalModel model, int myKey)
    {
        Model = new FunctionalModel(model,myKey,false);
        ModelStructure = [];
        int index;
        DptDictionary = [];
        SetUpDptNamesUdpate();

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
        SetUpDptNamesUdpate();

        foreach (var element in  model.ElementList)
        {
            ElementStructure elementStructure = new ElementStructure();
            elementStructure.Ie = [];
            elementStructure.Cmd = [];
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
                    elementStructure.Ie?.Add(newKey);
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
                    elementStructure.Ie?.Add(newKey);
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
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(functionalModels);
        SetUpDptNamesUdpate();
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure);
        Model = BuildFunctionalModel(name);

    }
    
    public FunctionalModelStructure(FunctionalModelStructure modelStructure) 
    {
        Model = new FunctionalModel(modelStructure.Model, modelStructure.Model.Key,false);
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(modelStructure.DptDictionary);
        SetUpDptNamesUdpate();
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
                    var cmdList = new List<int>();
                    var ieList = new List<int>();
                    foreach (XmlNode keyList in element.ChildNodes)
                    {
                        if (keyList.Name == "Command" && keyList.Attributes != null)
                        {
                            var list = keyList.Attributes;
                            foreach (XmlAttribute cmd in keyList.Attributes)
                            {
                                var indexStr = cmd.Name.Substring("Key_".Length); //Takes the index of the key
                                var index = int.Parse(indexStr);

                                while (cmdList.Count <= index)  //Ensures that the list is big enough
                                    cmdList.Add(0);

                                cmdList[index] = int.Parse(cmd.Value); //Updates the key at the given index
                            }
                        }

                        if (keyList.Name == "State_information" && keyList.Attributes != null)
                        {
                            foreach (XmlAttribute ie in keyList.Attributes)
                            {
                                var indexStr = ie.Name.Substring("Key_".Length); //Takes the index of the key
                                var index = int.Parse(indexStr);

                                while (ieList.Count <= index) //Ensures that the list is big enough
                                    ieList.Add(0);

                                ieList[index] = int.Parse(ie.Value); //Updates the key at the given index
                            }
                        }
                    }
                    elementStructure.Cmd = cmdList;
                    elementStructure.Ie = ieList;
                    res.ModelStructure.Add(elementStructure);
                }
            } else if (dicoOrModelStructure.Name == "Dictionary")
            {
                res.DptDictionary = new ObservableDictionary<int, DptAndKeywords>();
                foreach (XmlNode xPair in dicoOrModelStructure.ChildNodes)
                {
                    var key = int.Parse(xPair.Attributes?["Key"]?.Value ?? "");
                    var pair = new DptAndKeywords();
                    pair.Dpt = new DataPointType();
                    pair.Keywords = [];
                    foreach (XmlNode xDptAndKeywords in xPair.ChildNodes)
                    {
                        pair.Dpt.Type = int.Parse(xDptAndKeywords.Attributes?["Type"]?.Value ?? "0");
                        foreach (XmlNode xTypesOrKeywordsOrValues in xDptAndKeywords.ChildNodes)
                        {
                            if (xTypesOrKeywordsOrValues.Name == "Keywords")
                            {
                                if (xTypesOrKeywordsOrValues.Attributes!=null)
                                    foreach (XmlAttribute xKeyword in xTypesOrKeywordsOrValues.Attributes)
                                    {
                                        pair.Keywords?.Add(xKeyword.Value); //Then  have to be checked 
                                    }
                                
                            } else if (xTypesOrKeywordsOrValues.Name == "Values")
                            {
                                if(xTypesOrKeywordsOrValues.Attributes!=null)
                                    foreach (XmlAttribute xValue in xTypesOrKeywordsOrValues.Attributes)
                                    {
                                        pair.Dpt.IntValue.Add(new DataPointType.BigIntegerItem(int.Parse(xValue.Value))); //Then  have to be checked 
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
            for (var i = 0;i<element.Cmd.Count;i++)
            {
                var cmd = element.Cmd[i];
                var key = doc.CreateAttribute("Key_"+i);
                key.Value = cmd.ToString();
                xCmd.Attributes.Append(key);
            }
            for (var i = 0; i < element.Ie.Count;i++)
            {
                var ie = element.Ie[i];
                var key = doc.CreateAttribute("Key_"+i);
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

            if (element.Value.Keywords.Count > 0)
            {
                foreach (var keyword in element.Value.Keywords)
                {
                    var xKeyword = doc.CreateAttribute("Keyword");
                    xKeyword.Value = keyword;
                    xDptKeywords.Attributes.Append(xKeyword);
                }
            }

            if (element.Value.Dpt.Value.Count > 0)
            {
                foreach (var value in element.Value.Dpt.Value)
                {
                    var xValue = doc.CreateAttribute("Value");
                    xValue.Value = value?.ToString();
                    xDptValues.Attributes.Append(xValue);
                }
            }

            xDptType.Value = element.Value.Dpt.Type.ToString();
            xDptAndKeywords.Attributes.Append(xDptType);
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
        int newKey = 0;
        if (DptDictionary.Keys.Count != 0)
            newKey = DptDictionary.Keys.Max() + 1;
        DptDictionary.Add(newKey,new DptAndKeywords{Key = newKey,Keywords = new List<string>(),Dpt = new DataPointType(1,"DPT Personnalisé " + newKey)});
    }
    
    public void RemoveDpt(int key)
    {
        DptDictionary.Remove(key);
    }

    /// <summary>
    /// Finds the key of a dpt in a functional model structure from its name.
    /// </summary>
    /// <param name="name"> The name of the dpt. </param>
    /// <returns>The key of the dpt in the structure. </returns>
    public int FindKeyWithKeywords(string name)
    {
        foreach (var key in DptDictionary.Keys)
        {
            if (DptDictionary[key].Keywords.Any(p =>
                    name.StartsWith(p, StringComparison.OrdinalIgnoreCase) == true))
            {
                return key;
            }
        }
        return -1;
    }

    private void SetUpDptNamesUdpate()
    {
        // Initialisation
        foreach (var kv in DptDictionary)
        {
            DptNames.Add(kv.Value.Dpt.Name);
            SubscribeToDpt(kv.Value.Dpt);
        }

        // Gestion des ajouts / suppressions dans le dictionnaire
        DptDictionary.CollectionChanged += (s, e) =>
        {
            if (e.OldItems != null)
            {
                foreach (KeyValuePair<int, DptAndKeywords> item in e.OldItems)
                {
                    DptNames.Remove(item.Value.Dpt.Name);
                    UnsubscribeFromDpt(item.Value.Dpt);
                }
            }
            if (e.NewItems != null)
            {
                foreach (KeyValuePair<int, DptAndKeywords> item in e.NewItems)
                {
                    DptNames.Add(item.Value.Dpt.Name);
                    SubscribeToDpt(item.Value.Dpt);
                }
            }
        };
    }

    private void SubscribeToDpt(DataPointType dpt)
    {
        dpt.PropertyChanged += OnDptPropertyChanged;
    }

    private void UnsubscribeFromDpt(DataPointType dpt)
    {
        dpt.PropertyChanged -= OnDptPropertyChanged;
    }

    private void OnDptPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataPointType.Name))
        {
            // On reconstruit la liste des noms
            DptNames.Clear();
            foreach (var kv in DptDictionary)
                DptNames.Add(kv.Value.Dpt.Name);
        }
    }
}