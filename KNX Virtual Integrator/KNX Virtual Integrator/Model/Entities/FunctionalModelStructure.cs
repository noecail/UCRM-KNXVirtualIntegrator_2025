using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Entities;
/// <summary>
/// Class holding the structure of functional models in a Structure.
/// </summary>
public class FunctionalModelStructure : INotifyPropertyChanged
{
    /// <summary>
    /// The model to be used as a Structure example.
    /// </summary>
    public FunctionalModel Model {get; set; }
    /// <summary>
    /// The class holding defining a DPT and the keywords associated with it.
    /// </summary>
    public class DptAndKeywords : INotifyPropertyChanged
    {
        /// <summary>
        /// Key/Number of the instance.
        /// </summary>
        public int Key = 0;
        /// <summary>
        /// List of keywords associated with the <see cref="Dpt"/>.
        /// </summary>
        private List<string> _keywords = [];
        /// <summary>
        /// Gets or sets the list of keywords associated with the <see cref="Dpt"/>.
        /// </summary>
        public List<string> Keywords
        {
            get => _keywords;
            set
            {
                if (_keywords == value) return;
                _keywords = value;
                OnPropertyChanged();
                UpdateKeywordList();
            }
        }
        /// <summary>
        /// String of all the keywords associated with the <see cref="Dpt"/>.
        /// </summary>
        private string _allKeywords = "";
        /// <summary>
        /// Gets or sets the string of all the keywords associated with the <see cref="Dpt"/>.
        /// </summary>
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
        /// <summary>
        /// The DPT associated with the <see cref="Keywords"/>.
        /// </summary>
        public DataPointType Dpt { get; set; } = new ();

        /// <summary>
        /// Takes a string, and puts all the keywords inside it into the keywords associated
        /// </summary>
        public void UpdateKeywords()
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
        public void UpdateKeywordList()
        {
            if (Keywords == null || Keywords.Count == 0)
                return;
            _allKeywords = string.Join(',', Keywords);
            OnPropertyChanged(nameof(AllKeywords));
        }
        /// <summary>
        /// Empty constructor since everything is already initialized.
        /// </summary>
        public DptAndKeywords() { }
        /// <summary>
        /// Copies a DptAndKeywords.
        /// </summary>
        /// <param name="other">The DptAndKeywors to copy</param>
        public DptAndKeywords(DptAndKeywords other)
        {
            Key = other.Key;
            _keywords = other._keywords != null ? new List<string>(other._keywords) : new List<string>();
            _allKeywords = other._allKeywords ?? string.Empty;
            Dpt = new DataPointType(other.Dpt);

            PropertyChanged = null; // les handlers ne doivent pas être copiés
        }

        /// <summary>
        /// Event that occurs when the DptAndKeywords changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Invokes <see cref="PropertyChanged"/> when called.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }

    //J'ai choisi de faire démarrer les clés à 1 pour plus de logique pour l'utilisateur
    /// <summary>
    /// Dictionary of DPTs and their keywords of the structure.
    /// </summary>
    public ObservableDictionary<int, DptAndKeywords> DptDictionary { get; set; }
    
    // utilisée dans la liste déroulante de clé à choisir pour les dpts des element structure
    /// <summary>
    /// List of DPT keys used when choosing which DPT to use in the TestedElement. 
    /// </summary>
    public ObservableCollection<int> DptKeys { get; } = new();

    // utilisée dans la liste déroulante de dpt à choisir dans un dpt personnalisé
    /// <summary>
    /// List of all implemented DPTs from which to choose.
    /// </summary>
    public static List<int> DefaultDptToChoose { get; } = new List<int>
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 28, 29, 30,
        31, 200, 201, 202, 203, 204, 205, 206, 207, 209, 211, 212, 213, 215, 216, 217, 218, 219,
        221, 222, 223, 224, 225, 229, 230, 232, 234, 235, 236, 237, 238, 239, 240, 242, 243, 244,
        245, 246, 247, 248, 250, 251, 252, 254, 255, 256, 257, 265, 266, 267, 268, 269, 270, 271,
        272, 273, 274, 277, 278, 279, 280, 281, 282, 283, 284
    };
    
    // classe qui enrobe un int, notamment pour pouvoir y mettre un setter
    // configurée de manière à ce que son utilisation soit invisible, çàd on peut mettre un item à la place d'un IntItem et ça fonctionne
    /// <summary>
    /// Int wrapper to implement <see cref="PropertyChanged"/> and interface visibility handling.
    /// </summary>
    public class IntItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Value of the DPT
        /// </summary>
        private int _value;
        /// <summary>
        /// Gets or sets the value of the DPT
        /// </summary>
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Visibility of the button used to remove a DPT in <see cref="View.Windows.StructureEditWindow"/>
        /// </summary>
        private Visibility? _removeDptButtonVisibility;
        /// <summary>
        /// Gets or sets the visibility of the button used to
        /// remove a DPT in <see cref="View.Windows.StructureEditWindow"/>
        /// </summary>
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
        /// <summary>
        /// Used to "quicken" search of the value since using the item will output directly its value.
        /// </summary>
        /// <param name="item">The item whose value will be returned.</param>
        /// <returns>The value of the item.</returns>
        public static implicit operator int(IntItem item) => item.Value;
        /// <summary>
        /// To print only the <see cref="Value"/> of the item.
        /// </summary>
        /// <returns>The value as a string</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
        /// <summary>
        /// Constructs the class with hidden visibility. 
        /// </summary>
        /// <param name="value">the value to which <see cref="Value"/> will be initialised.</param>
        public IntItem(int value)
        {
            Value = value;
            RemoveDptButtonVisibility = Visibility.Hidden;
        }
        /// <summary>
        /// Event that occurs when the IntItem changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Invokes <see cref="PropertyChanged"/> when called.
        /// </summary>
        /// <param name="name">The name of the property that was changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    /// <summary>
    /// Barebone structure of TestedElements. It holds the number of CMD, IE and the DPT associated with it.
    /// </summary>
    public class ElementStructure
    {
        /// <summary>
        /// The number of Cmd DPTs in the Element.
        /// </summary>
        public ObservableCollection<IntItem> Cmd { get; set; }
        /// <summary>
        /// The number of Ie DPTs in the Element.
        /// </summary>
        public ObservableCollection<IntItem> Ie { get; set; }
        
        // CmdValues [0][1] -> 1er DPT d'envoi (=1ere colonne), 2eme value (=2eme ligne)
        public ObservableCollection<ObservableCollection<DataPointType.BigIntegerItem>> CmdValues { get; set; }
        public ObservableCollection<ObservableCollection<DataPointType.BigIntegerItem>> IeValues { get; set; }

        /// <summary>
        /// Adds a new Cmd Dpt.
        /// </summary>
        /// <param name="value">The key of the DPT at which the Cmd is initialized.</param>
        public void AddToCmd(int value)
        {
            Cmd.Add(new IntItem(value));
            UpdateRemoveDptButtonVisibility();
            CmdValues.Add(new ObservableCollection<DataPointType.BigIntegerItem>());
            foreach (var test in CmdValues[0])
                CmdValues[^1].Add(new DataPointType.BigIntegerItem(0));
        }

        /// <summary>
        /// Adds a new Ie Dpt.
        /// </summary>
        /// <param name="value">The key of the DPT at which the Cmd is initialized.</param>
        public void AddToIe(int value)
        {
            Ie.Add(new IntItem(value));
            IeValues.Add(new ObservableCollection<DataPointType.BigIntegerItem>());
            foreach (var test in CmdValues[0])
                IeValues[^1].Add(new DataPointType.BigIntegerItem(0));
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Removes a Cmd Dpt at an index.
        /// </summary>
        /// <param name="cmdIndex">The index of the DPT to remove.</param>
        public void RemoveCmdAt(int cmdIndex)
        {
            Cmd.RemoveAt(cmdIndex);
            UpdateRemoveDptButtonVisibility();
            CmdValues.RemoveAt(cmdIndex);
        }

        /// <summary>
        /// Removes a Ie Dpt at an index.
        /// </summary>
        /// <param name="ieIndex">The index of the DPT to remove.</param>
        public void RemoveIeAt(int ieIndex)
        {
            Ie.RemoveAt(ieIndex);
            IeValues.RemoveAt(ieIndex);
            UpdateRemoveTestButtonVisibility();
        }

        public void AddValueToCmd(int cmdIndex)
        {
            CmdValues[cmdIndex].Add(new DataPointType.BigIntegerItem(new BigInteger(0)));
        }
        
        public void AddValueToIe(int ieIndex)
        {
            IeValues[ieIndex].Add(new DataPointType.BigIntegerItem(new BigInteger(0)));
            UpdateRemoveTestButtonVisibility();
        }

        public void RemoveCmdValueAt(int cmdIndex, int valueIndex)
        {
            CmdValues[cmdIndex].RemoveAt(valueIndex);
        }

        public void RemoveIeValueAt(int ieIndex, int valueIndex)
        {
            IeValues[ieIndex].RemoveAt(valueIndex);
            UpdateRemoveTestButtonVisibility();
        }
        
        public void AddTest()
        {
            foreach (var dptValues in CmdValues)
                dptValues.Add(new DataPointType.BigIntegerItem(new BigInteger(0)));
            foreach (var dptValues in IeValues)
                dptValues.Add(new DataPointType.BigIntegerItem(new BigInteger(0)));
               
            UpdateRemoveTestButtonVisibility();
        }

        public void RemoveTestAt(int indexTest)
        {
            foreach (var dptValues in CmdValues)
                dptValues.RemoveAt(indexTest);
            foreach (var dptValues in IeValues)
                dptValues.RemoveAt(indexTest);
   
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Creates an empty ElementStructure.
        /// </summary>
        public ElementStructure()
        {
            Cmd = [];
            Ie = [];
            CmdValues = [];
            IeValues = [];
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Creates a filled ElementStructure with <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="cmdCollection">The collection of Cmd DPT to copy.</param>
        /// <param name="ieCollection">The collectio of Ie DPT to copy.</param>
        public ElementStructure(ObservableCollection<IntItem> cmdCollection, ObservableCollection<IntItem> ieCollection)
        {
            Cmd = cmdCollection;
            Ie = ieCollection;
            CmdValues = [];
            IeValues = [];
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Creates a filled ElementStructure with lists.
        /// </summary>
        /// <param name="cmdCollection">The list of Cmd DPT to copy.</param>
        /// <param name="ieCollection">The list of Ie DPT to copy.</param>
        public ElementStructure(List<int> cmdCollection, List<int> ieCollection)
        {
            Cmd = [];
            Ie = [];
            CmdValues = [];
            IeValues = [];
            foreach (var cmdInt in cmdCollection)
                AddToCmd(cmdInt);
            foreach (var ieInt in ieCollection)
                AddToIe(ieInt);
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Copies an ElementStructure.
        /// </summary>
        /// <param name="otherStructure">The structure to copy.</param>
        public ElementStructure(ElementStructure otherStructure)
        {
            Cmd = new ObservableCollection<IntItem>();
            Ie = new ObservableCollection<IntItem>();
            CmdValues = [];
            IeValues = [];
            foreach(var cmd in otherStructure.Cmd)
                Cmd.Add(new IntItem(cmd));
            foreach (var ie in otherStructure.Ie)
                Ie.Add(new IntItem(ie));
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Hides the Cmd remove button if there is only one Cmd DPT.
        /// Shows the button if there is more.
        /// </summary>
        private void UpdateRemoveDptButtonVisibility()
        {
            var vis = Cmd.Count > 1 ? Visibility.Visible : Visibility.Hidden;
            foreach (var intItem in Cmd)
                intItem.RemoveDptButtonVisibility = vis;
        }
        
        private void UpdateRemoveTestButtonVisibility()
        {
            foreach (var ie in IeValues)
            {
                var vis = ie != IeValues.Last() ? Visibility.Collapsed : Visibility.Visible;
                foreach (var bigIntegerItem in ie)
                    bigIntegerItem.RemoveTestButtonVisibility = vis;
            }
        }
    }
    
    /// <summary>
    /// Gives the same output as ToString method. But ToString does not dynamically change when the name is modified
    /// FullName is used to display the Key and the Name in the SelectedModels listbox in the Mainwindow
    /// </summary>
    public string FullName => $"S{Model.Key} | {Model.Name}";
    /// <summary>
    /// The list of ElementStructures of the model
    /// </summary>
    public ObservableCollection<ElementStructure> ModelStructure { get; set; } = [];
    /// <summary>
    /// Checks whether the structure is correctly filled (every necessary option is filled)
    /// </summary>
    /// <returns>true if it is correctly filled.</returns>
    public bool IsValid()
    {
        if (ModelStructure.Count == 0) return false;
        List<int> dptsInDictionary = [];
        foreach (var kvp in DptDictionary)
            dptsInDictionary.Add(kvp.Key);

        foreach (var elementStructure in ModelStructure)
        {
            foreach (var cmd in elementStructure.Cmd)
                if (!dptsInDictionary.Contains(cmd.Value)) return false;

            foreach (var ie in elementStructure.Ie)
                if (!dptsInDictionary.Contains(ie.Value)) return false;
        }
        return true;
    }
    /// <summary>
    /// Constructor with only the name of the structure and no DPT, keyword or ElementStructure.
    /// </summary>
    /// <param name="name"></param>
    public FunctionalModelStructure(string name)
    {
        Model = new FunctionalModel(name);
        ModelStructure = [];
        DptDictionary = [];
        SetUpNotifs();
    }
    /// <summary>
    /// Keywords of the structure (with the whole structure).
    /// </summary>
    private List<string> _keywords = [];
    /// <summary>
    /// Gets or sets keywords of the structure (with the whole structure).
    /// </summary>
    public List<string> Keywords
    {
        get => _keywords;
        set
        {
            if (_keywords == value) return;
            _keywords = value; 
            OnPropertyChanged();
            UpdateKeywordList();
        }
    }
    /// <summary>
    /// The string of all keywords of the structure (of the whole structure).
    /// </summary>
    private string _allKeywords = "";
    /// <summary>
    /// Gets or sets the string of all keywords of the structure (of the whole structure).
    /// </summary>
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

    /// <summary>
    /// Takes a string, and puts all the keywords inside it into the keywords associated
    /// </summary>
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
    public void UpdateKeywordList()
    {
        if (Keywords == null || Keywords.Count == 0)
            return;
        _allKeywords = string.Join(',', Keywords);
        OnPropertyChanged(nameof(AllKeywords));
    }
    
    /// <summary>
    /// Creates a ModelStructure.
    /// </summary>
    /// <param name="model">The structure.</param>
    /// <param name="myKey">The future key.</param>
    public FunctionalModelStructure(FunctionalModel model, int myKey)
    {
        Model = new FunctionalModel(model, myKey, false);
        ModelStructure = [];
        int index;
        DptDictionary = [];
        SetUpNotifs();

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

    /// <summary>
    /// Creates a ModelStructure.
    /// </summary>
    /// <param name="myName">The future name.</param>
    /// <param name="model">The structure.</param>
    /// <param name="myKey">The future key.</param>
    public FunctionalModelStructure(FunctionalModel model, string myName, int myKey)
    {
        Model = new FunctionalModel(model, myKey, false);
        ModelStructure = [];
        int index;
        DptDictionary = [];
        SetUpNotifs();

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

    /// <summary>
    /// Creates a ModelStructure with most of its attributes/properties but only with default CMD/IE. 
    /// </summary>
    /// <param name="name">The future name.</param>
    /// <param name="functionalModels">The future dictionary.</param>
    /// <param name="modelStructure">The structure of elements.</param>
    /// <param name="key">The future key.</param>
    public FunctionalModelStructure(string name, Dictionary<int, DptAndKeywords> functionalModels,
        ObservableCollection<ElementStructure> modelStructure, int key) 
    {
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(functionalModels);
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure);
        Model = BuildFunctionalModel(name, key);
        SetUpNotifs();
    }
    
    /// <summary>
    /// Creates a ModelStructure with most of its attributes/properties. 
    /// </summary>
    /// <param name="name">The future name.</param>
    /// <param name="functionalModels">The future dictionary.</param>
    /// <param name="modelStructure">The structure.</param>
    /// <param name="cmdValues">The Command DPTs to insert.</param>
    /// <param name="ieValues">The Ie DPTs to insert.</param>
    /// <param name="key">The future key.</param>
     public FunctionalModelStructure(string name, Dictionary<int, DptAndKeywords> functionalModels,
        ObservableCollection<ElementStructure> modelStructure,List<List<List<int>>> cmdValues, List<List<List<int>>> ieValues, int key)
    {
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(functionalModels);
        ModelStructure = new ObservableCollection<ElementStructure>(modelStructure);
        for (var i = 0; i < ModelStructure.Count; i++)
        {
            for (var j = 0; j < cmdValues[i].Count; j++)
            {
                while (ModelStructure[i].CmdValues.Count < j + 1)
                    ModelStructure[i].CmdValues.Add([]);
                for (var k = 0; k < cmdValues[i][j].Count; k++)
                {
                    ModelStructure[i].CmdValues[j].Add(new DataPointType.BigIntegerItem(cmdValues[i][j][k]));
                }

            }

            for (var j = 0; j < ieValues[i].Count; j++)
            {
                while (ModelStructure[i].IeValues.Count < j + 1)
                    ModelStructure[i].IeValues.Add([]);
                for (var k = 0; k < ieValues[i][j].Count; k++)
                {
                    ModelStructure[i].IeValues[j].Add(new DataPointType.BigIntegerItem(ieValues[i][j][k]));
                }

            }
        }
        Model = BuildFunctionalModel(name, key);
        SetUpNotifs();


        
    }

    // ce constructeur fait bien une copie profonde, indépendante de la structure passée en argument
    /// <summary>
    /// Copy of a modelStructure, independent of the one given in parameters.
    /// </summary>
    /// <param name="modelStructure">The structure from which the key, dptDictionary and keywords are taken.</param>
    public FunctionalModelStructure(FunctionalModelStructure modelStructure) 
    {
        AllKeywords = modelStructure.AllKeywords;
        Model = new FunctionalModel(modelStructure.Model, modelStructure.Model.Key,false);
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(modelStructure.DptDictionary);
        SetUpNotifs();
        ModelStructure = new ObservableCollection<ElementStructure>();
        foreach(var elementStructure in modelStructure.ModelStructure)
            ModelStructure.Add(new ElementStructure(elementStructure));
    }
    /// <summary>
    /// Copies a ModelStructure with most of its attributes. 
    /// </summary>
    /// <param name="modelStructure">The structure from which the key, dptDictionary and keywords are taken.</param>
    /// <param name="cmdValues">The Command DPTs to insert.</param>
    /// <param name="ieValues">The Ie DPTs to insert.</param>
    public FunctionalModelStructure(FunctionalModelStructure modelStructure, List<List<List<int>>> cmdValues,
        List<List<List<int>>> ieValues)
    {
        AllKeywords = modelStructure.AllKeywords;
        Model = new FunctionalModel(modelStructure.Model, modelStructure.Model.Key, false);
        DptDictionary = new ObservableDictionary<int, DptAndKeywords>(modelStructure.DptDictionary);
        SetUpNotifs();
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
                        Model.ElementList[i].TestsCmd[j].IntValue.Add(new BigIntegerItem(value[k]));
                    }
                    else
                    {
                        Model.ElementList[i].TestsCmd[j].IntValue[k] = new BigIntegerItem(value[k]);
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
                        Model.ElementList[i].TestsIe[j].IntValue.Add(new BigIntegerItem(value[k]));
                    }
                    else
                    {
                        Model.ElementList[i].TestsIe[j].IntValue[k] = new BigIntegerItem(value[k]);
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
    /// <summary>
    /// Adds an ElementStructure to the ModelStructure.
    /// </summary>
    public void AddElement()
    {
        ModelStructure.Add(new ElementStructure());
    }

    /// <summary>
    /// Returns a model built from the model structure.
    /// </summary>
    /// <param name="name">the name of the model to build.</param>
    /// <param name="key">The key of the model to build.</param>
    /// <returns>The built model.</returns>
    public FunctionalModel BuildFunctionalModel(string name, int key)
    {
        var res = new FunctionalModel(name);
        foreach (var element in ModelStructure)
        {
            res.AddElement(new TestedElement());
            foreach (var cmd in element.Cmd)
            {
                res.ElementList[^1].AddDptToCmd(new DataPointType(DptDictionary[cmd].Dpt));
            }
            foreach (var ie in element.Ie)
            {
                res.ElementList[^1].AddDptToIe(new DataPointType(DptDictionary[ie].Dpt));
            }
        }
        
        for (var i = 0; i < ModelStructure.Count; i++)
        {
            var elementStructure = ModelStructure[i];
            for (var j = 0; j < elementStructure.CmdValues.Count; j++)
            {
                var cmdValue = elementStructure.CmdValues[j];
                for (var k = 0; k < cmdValue.Count; k++)
                {
                    var toAdd = cmdValue[k];
                    if (res.ElementList[i].TestsCmd[j].IntValue == null )
                    {
                        res.ElementList[i].TestsCmd[j].IntValue = [];
                    }
                    if (res.ElementList[i].TestsCmd[j].IntValue.Count < k + 1)
                    {
                        res.ElementList[i].TestsCmd[j].IntValue.Add(new DataPointType.BigIntegerItem(toAdd.BigIntegerValue ?? 0));
                    }
                    else
                    {
                        res.ElementList[i].TestsCmd[j].IntValue[k] = new DataPointType.BigIntegerItem(toAdd.BigIntegerValue ?? 0);
                    }
                }

                res.ElementList[i].TestsCmd[j].UpdateValue();
            }

            for (var j = 0; j < elementStructure.IeValues.Count; j++)
            {
                var value = elementStructure.IeValues[j];
                for (var k = 0; k < value.Count; k++)
                {
                    var toAdd = value[k];
                    if (res.ElementList[i].TestsIe[j].IntValue == null)
                    {
                        res.ElementList[i].TestsIe[j].IntValue = [];
                    }
                    if (res.ElementList[i].TestsIe[j].IntValue.Count < k + 1)
                    {
                        res.ElementList[i].TestsIe[j].IntValue.Add(new DataPointType.BigIntegerItem(toAdd.BigIntegerValue ?? 0));
                    }
                    else
                    {
                        res.ElementList[i].TestsIe[j].IntValue[k] = new DataPointType.BigIntegerItem(toAdd.BigIntegerValue ?? 0);
                    }
                }
            }
        }

        res.Key = key;

        return res;
    }

    /// <summary>
    /// Imports a ModelStructure from an XmlNode.
    /// </summary>
    /// <param name="model">The XmlNode from which the ModelStructure is imported.</param>
    /// <param name="key">The key of the model to build.</param>
    /// <returns>The new ModelStructure.</returns>
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
                                        pair.Dpt.IntValue.Add(new BigIntegerItem(int.Parse(xValue.Value))); //Then  have to be checked 
                                    }
                            }
                        }

                        pair.UpdateKeywordList();
                    }
                    res.DptDictionary[myKey] = pair;
                }
            }
        }
        
        res.Model = res.BuildFunctionalModel(name, key);
        res.Model.UpdateIntValue();
        return res;
    }
    /// <summary>
    /// Exports a ModelStructure by creating an XmlElement from an XmlDocument.
    /// </summary>
    /// <param name="doc">The XmlDocument in which the XmlElement is created.</param>
    /// <returns>The new XmlElement.</returns>
    public XmlElement ExportFunctionalModelStructure(XmlDocument doc)
    {
        var xModel = doc.CreateElement(Model.Name.Replace(' ','_'));
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
    /// <summary>
    /// Override to only display the <see cref="FullName"/> of the modelStructure.
    /// </summary>
    /// <returns><see cref="FullName"/>.</returns>
    public override string ToString() => FullName;
    
    /// <summary>
    /// Creates a Dpt at the last position in the dictionary.
    /// </summary>
    public void CreateDpt()
    {
        int newKey = 1;
        if (DptDictionary.Keys.Count != 0)
            newKey = DptDictionary.Keys.Max() + 1;
        DptDictionary.Add(newKey,new DptAndKeywords{Key = newKey,Keywords = new List<string>(), AllKeywords = "",Dpt = new DataPointType(1,"DPT Personnalisé " + newKey)});
    }
    /// <summary>
    /// Removes a Dpt from the dictionary.
    /// </summary>
    /// <param name="key">The key of the dpt to remove</param>
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
    /// <summary>
    /// Sets up updates for the DPT. <seealso cref="SetUpDptKeysUpdate"/><seealso cref="SetUpFullNameUpdate"/>
    /// </summary>
    private void SetUpNotifs()
    {
        SetUpFullNameUpdate();
        SetUpDptKeysUpdate();
    }
    /// <summary>
    /// Sets up the Dpt FullName's update
    /// </summary>
    private void SetUpFullNameUpdate()
    {
        Model.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(Model.Key) or nameof(Model.Name))
            {
                OnPropertyChanged(nameof(FullName));
            }
        };
    }

    // Ces quelques fonctions (SetUpDptKeysUpdate / SubscribeToDpt / UnsubscribeFromDpt / OnDptPropertyChanged) sont là pour gérer la liste de noms de Dpts du dictionnaire
    // Devraient être dans le view model
    // A déplacer plus tard
    /// <summary>
    /// Sets up the Dpt Key's update.
    /// </summary>
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
    /// <summary>
    /// Event that occurs when the IntItem changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> when called.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}