using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml;

namespace KNX_Virtual_Integrator.Model.Entities;

public class FunctionalModelStructure : INotifyPropertyChanged
{

    public FunctionalModel Model{get; set; }

    public class DptAndKeywords : INotifyPropertyChanged
    {
        public int Key = 0;

        private List<string> _keywords = [];
        public List<string> Keywords
        {
            get => _keywords;
            set
            {
                _keywords = value;
                OnPropertyChanged();
                UpdateKeywordList();
            }
        }

        private string _allKeywords = "";
        public string AllKeywords
        {
            get => _allKeywords;
            set
            {
                if (_allKeywords == value) return;
                _allKeywords = value;
                OnPropertyChanged();
                UpdateKeywords();
            }
        }

        public DataPointType Dpt { get; set; } = new ();

        /// <summary>
        /// Takes a string, and puts all the keywords inside it into the keywords associated
        /// </summary>
        /// <param name="keywordList">String containing all the keywords separated with commas</param>
        private void UpdateKeywords()
        {
            _keywords.Clear();
            foreach (var kw in AllKeywords.Split(',').ToList())
            {
                _keywords.Add(kw);
                OnPropertyChanged(nameof(Keywords));
            }
        }
        
        /// <summary>
        /// Takes all the keywords associated to a dpt and group them, separating them with commas
        /// </summary>
        private void UpdateKeywordList()
        {
            if (Keywords == null || Keywords.Count == 0)
                return;
            _allKeywords = string.Join(',', Keywords);
            OnPropertyChanged(nameof(AllKeywords));
        }

        public event PropertyChangedEventHandler? PropertyChanged = null;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }

    //J'ai choisir de faire démarrer les clés à 1 pour plus de logique pour l'utilisateur
    public ObservableDictionary<int, DptAndKeywords> DptDictionary { get; set; }

    // utilisée dans la liste déroulante de clé à choisir pour les dpts des element structure
    public ObservableCollection<int> DptKeys { get; } = new();

    // utilisée dans la liste déroulante de dpt à choisir dans un dpt personnalisé
    public static List<int> DefaultDptToChoose { get; } = new List<int>
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 28, 29, 30,
        31, 200, 201, 202, 203, 204, 205, 206, 207, 209, 211, 212, 213, 215, 216, 217, 218, 219,
        221, 222, 223, 224, 225, 229, 230, 232, 234, 235, 236, 237, 238, 239, 240, 242, 243, 244,
        245, 246, 247, 248, 250, 251, 252, 254, 255, 256, 257, 265, 266, 267, 268, 269, 270, 271,
        272, 273, 274, 277, 278, 279, 280, 281, 282, 283, 284
    };
    
    // classe qui enrobe un string, notamment pour pouvoir y mettre un setter
    // configurée de manière à ce que son utilisation soit invisible, çàd on peut mettre un item à la place d'un IntItem et ça fonctionne
    public class IntItem : INotifyPropertyChanged
    {
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        private Visibility? _removeDptButtonVisibility;
        public Visibility? RemoveDptButtonVisibility
        {
            get => _removeDptButtonVisibility;
            set
            {
                if (_removeDptButtonVisibility == value) return;
                _removeDptButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public static implicit operator int(IntItem item) => item.Value;

        public override string ToString()
        {
            return Value.ToString();
        }
        
        public IntItem(int value)
        {
            Value = value;
            RemoveDptButtonVisibility = Visibility.Hidden;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    // configurée de manière à ce que le IntItem soit invisible, çàd on peut mettre un item à la place d'un IntItem et ça fonctionne
    public class ElementStructure
    {
        public ObservableCollection<IntItem> Cmd { get; set; }
        public ObservableCollection<IntItem> Ie { get; set; }

        public void AddToCmd(int value)
        {
            Cmd.Add(new IntItem(value));
            UpdateRemoveDptButtonVisibility();
        }

        public void AddToIe(int value)
        {
            Ie.Add(new IntItem(value));
        }

        public void RemoveCmdAt(int cmdIndex)
        {
            Cmd.RemoveAt(cmdIndex);
            UpdateRemoveDptButtonVisibility();
        }

        public void RemoveIeAt(int ieIndex)
        {
            Ie.RemoveAt(ieIndex);
        }
        
        public ElementStructure()
        {
            Cmd = [];
            Ie = [];
        }
        
        public ElementStructure(ObservableCollection<IntItem> cmdCollection, ObservableCollection<IntItem> ieCollection)
        {
            Cmd = cmdCollection;
            Ie = ieCollection;
        }

        public ElementStructure(List<int> cmdCollection, List<int> ieCollection)
        {
            Cmd = [];
            Ie = [];
            foreach (var cmdInt in cmdCollection)
                AddToCmd(cmdInt);
            foreach (var ieInt in ieCollection)
                AddToIe(ieInt);
        }

        private void UpdateRemoveDptButtonVisibility()
        {
            var vis = Cmd.Count > 1 ? Visibility.Visible : Visibility.Hidden;
            foreach (var intItem in Cmd)
                intItem.RemoveDptButtonVisibility = vis;
        }
        
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
        SetUpDptKeysUpdate();
    }

    private List<string> _keywords = [];
    public List<string> Keywords
    {
        get => _keywords;
        set
        {
            _keywords = value; 
            OnPropertyChanged();
            UpdateKeywordList();
        }
    }

    private string _allKeywords = "";
    public string AllKeywords
    {
        get => _allKeywords;
        set
        {
            _allKeywords = value;
            OnPropertyChanged();
            UpdateKeywords();
        }
    }

    /// <summary>
    /// Takes a string, and puts all the keywords inside it into the keywords associated
    /// </summary>
    /// <param name="keywordList">String containing all the keywords separated with commas</param>
    private void UpdateKeywords()
    {
        _keywords.Clear();
        foreach (var kw in AllKeywords.Split(',').ToList())
        {
            _keywords.Add(kw);
            OnPropertyChanged(nameof(Keywords));
        }
    }
        
    /// <summary>
    /// Takes all the keywords associated to a dpt and group them, separating them with commas
    /// </summary>
    private void UpdateKeywordList()
    {
        if (Keywords == null || Keywords.Count == 0)
            return;
        _allKeywords = string.Join(',', Keywords);
        OnPropertyChanged(nameof(AllKeywords));
    }
    
    public FunctionalModelStructure(FunctionalModel model, int myKey)
    {
        Model = new FunctionalModel(model, myKey, false);
        ModelStructure = [];
        int index;
        DptDictionary = [];
        SetUpDptKeysUpdate();

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
                        elementStructure.AddToCmd(index);
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
                    elementStructure.AddToIe(newKey);
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
                        elementStructure.AddToIe(index);
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
                    elementStructure.AddToIe(newKey);
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
        SetUpDptKeysUpdate();

        foreach (var element in model.ElementList)
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
                        elementStructure.AddToCmd(index);
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
                    elementStructure.AddToIe(newKey);
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
                        elementStructure.AddToIe(index);
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
                    elementStructure.AddToIe(newKey);
                }
            }
            ModelStructure.Add(elementStructure);
        }
        Model.Key = myKey;
        
        Model.Name = myName;

    }

    
    public FunctionalModelStructure(string name, Dictionary<int, DptAndKeywords> functionalModels,
        ObservableCollection<ElementStructure> modelStructure, int key) 
    {
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(functionalModels);
        SetUpDptKeysUpdate();
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure);
        Model = BuildFunctionalModel(name, key);

    }
    
     public FunctionalModelStructure(string name, Dictionary<int, DptAndKeywords> functionalModels,
        ObservableCollection<ElementStructure> modelStructure,List<List<List<int>>> cmdValues, List<List<List<int>>> ieValues, int key)
    {
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(functionalModels);
        SetUpDptKeysUpdate();
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure);
        Model = BuildFunctionalModel(name, key);


        for (var i = 0; i < cmdValues.Count; i++)
        {
            var cmdValue = cmdValues[i];
            for (var j = 0; j < cmdValue.Count; j++)
            {
                var value = cmdValue[j];
                for (var k = 0; k < value.Count; k++)
                {
                    var toAdd = value[k];
                    if (Model.ElementList[i].TestsCmd[j].IntValue == null)
                    {
                        Model.ElementList[i].TestsCmd[j].IntValue = [];
                    }
                    if (Model.ElementList[i].TestsCmd[j].IntValue.Count < k + 1)
                    {
                        Model.ElementList[i].TestsCmd[j].IntValue.Add(new DataPointType.BigIntegerItem(toAdd));
                    }
                    else
                    {
                        Model.ElementList[i].TestsCmd[j].IntValue[k] = new DataPointType.BigIntegerItem(toAdd);
                    }
                }

                Model.ElementList[i].TestsCmd[j].UpdateValue();
            }
        }

        for (var i = 0; i < ieValues.Count; i++)
        {
            var ieValue = ieValues[i];
            for (var j = 0; j < ieValue.Count; j++)
            {
                var value = ieValue[j];
                for (var k = 0; k < value.Count; k++)
                {
                    var toAdd = value[k];
                    if (Model.ElementList[i].TestsIe[j].IntValue == null)
                    {
                        Model.ElementList[i].TestsIe[j].IntValue = [];
                    }
                    if (Model.ElementList[i].TestsIe[j].IntValue.Count < k + 1)
                    {
                        Model.ElementList[i].TestsIe[j].IntValue.Add(new DataPointType.BigIntegerItem(toAdd));
                    }
                    else
                    {
                        Model.ElementList[i].TestsIe[j].IntValue[k] = new DataPointType.BigIntegerItem(toAdd);
                    }
                }

                Model.ElementList[i].TestsIe[j].UpdateValue();
            }
        }
    }

    
    
    
    public FunctionalModelStructure(FunctionalModelStructure modelStructure) 
    {
        Model = new FunctionalModel(modelStructure.Model, modelStructure.Model.Key,false);
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(modelStructure.DptDictionary);
        SetUpDptKeysUpdate();
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure.ModelStructure);
    }

    public FunctionalModelStructure(FunctionalModelStructure modelStructure, List<List<List<int>>> cmdValues,
        List<List<List<int>>> ieValues)
    {
        Model = new FunctionalModel(modelStructure.Model, modelStructure.Model.Key, false);
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(modelStructure.DptDictionary);
        SetUpDptKeysUpdate();
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure.ModelStructure);

        foreach (var element in Model.ElementList)
        {
            foreach (var dptCmd in element.TestsCmd)
            {
                dptCmd.IntValue.Clear();
                dptCmd.Value.Clear();
            }
            foreach (var dptIe in element.TestsIe)
            {
                dptIe.IntValue.Clear();
                dptIe.Value.Clear();
            }
        }
        for (var i = 0; i < cmdValues.Count; i++)
        {
            var cmdValue = cmdValues[i];
            for (var j = 0; j < cmdValue.Count; j++)
            {
                var value = cmdValue[j];
                for (var k = 0; k < value.Count; k++)
                {
                    if (Model.ElementList[i].TestsCmd[j].IntValue.Count<k+1)
                    {
                        Model.ElementList[i].TestsCmd[j].IntValue.Add(new DataPointType.BigIntegerItem(value[k]));
                    }
                    else
                    {
                        Model.ElementList[i].TestsCmd[j].IntValue[k] = new DataPointType.BigIntegerItem(value[k]);
                    }
                }
                Model.ElementList[i].TestsCmd[j].UpdateValue();
            }
        }
        for (var i = 0; i < ieValues.Count; i++)
        {
            var ieValue = ieValues[i];
            for (var j = 0; j < ieValue.Count; j++)
            {
                var value = ieValue[j];
                for (var k = 0; k < value.Count; k++)
                {
                    if (Model.ElementList[i].TestsIe[j].IntValue.Count<k+1)
                    {
                        Model.ElementList[i].TestsIe[j].IntValue.Add(new DataPointType.BigIntegerItem(value[k]));
                    }
                    else
                    {
                        Model.ElementList[i].TestsIe[j].IntValue[k] = new DataPointType.BigIntegerItem(value[k]);
                    }
                }
                Model.ElementList[i].TestsIe[j].UpdateValue();
            }
        }
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
            model.AddElement(new TestedElement());
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

    // Retourne un modèle fabriqué à partir du model structure du functional model structure
    public FunctionalModel BuildFunctionalModel(string name, int key)
    {
        var res = new FunctionalModel(name);
        foreach (var element in ModelStructure)
        {
            res.AddElement(new TestedElement());
            foreach (var cmd in element.Cmd)
            {
                res.ElementList[^1].AddDptToCmd(new DataPointType(cmd));
            }
            foreach (var ie in element.Ie)
            {
                res.ElementList[^1].AddDptToIe(new DataPointType(ie));
            }
        }

        res.Key = key;

        return res;
    }


    public static FunctionalModelStructure ImportFunctionalModelStructure(XmlNode model, int key)
    {
        var name = model.Name;
        var res = new FunctionalModelStructure(name);

        foreach (XmlNode dicoOrModelStructure in model.ChildNodes)
        {
            if (dicoOrModelStructure.Name == "Model_Structure")
            {
                foreach (XmlNode element in dicoOrModelStructure.ChildNodes)
                {
                    
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
                    ElementStructure elementStructure = new ElementStructure(cmdList, ieList);
                    res.ModelStructure.Add(elementStructure);
                }
            }
            else if (dicoOrModelStructure.Name == "Dictionary")
            {
                res.DptDictionary = new ObservableDictionary<int, DptAndKeywords>();
                foreach (XmlNode xPair in dicoOrModelStructure.ChildNodes)
                {
                    var myKey = int.Parse(xPair.Attributes?["Key"]?.Value ?? "");
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
                    res.DptDictionary[myKey] = pair;
                }
            }
        }
        
        res.Model = res.BuildFunctionalModel(name, key);
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
        int newKey = 1;
        if (DptDictionary.Keys.Count != 0)
            newKey = DptDictionary.Keys.Max() + 1;
        DptDictionary.Add(newKey,new DptAndKeywords{Key = newKey,Keywords = new List<string>(), AllKeywords = "",Dpt = new DataPointType(1,"DPT Personnalisé " + newKey)});
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

    // Ces quelques fonctions (SetUpDptKeysUpdate / SubscribeToDpt / UnsubscribeFromDpt / OnDptPropertyChanged) sont là pour gérer la liste de noms de Dpts du dictionnaire
    // Devraient être dans le view model
    // A déplacer plus tard
    private void SetUpDptKeysUpdate()
    {
        // Initialisation
        foreach (var kv in DptDictionary)
        {
            DptKeys.Add(kv.Key);
        }

        // Gestion des ajouts / suppressions dans le dictionnaire
        DptDictionary.CollectionChanged += (s, e) =>
        {
            if (e.OldItems != null)
            {
                foreach (KeyValuePair<int, DptAndKeywords> item in e.OldItems)
                {
                    DptKeys.Remove(item.Key);
                }
            }

            if (e.NewItems != null)
            {
                foreach (KeyValuePair<int, DptAndKeywords> item in e.NewItems)
                {
                    DptKeys.Add(item.Key);
                }
            }
        };
    }

    // Obligé de faire du property changed ici pour gérer la déselection du type de dpt lors du changement de nom d'un des dpts personnalisés
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}