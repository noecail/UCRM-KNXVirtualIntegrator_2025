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
        public List<TestedElement> ElementList { get; } // The list of elements associated to the model
        public int Key { get; set; } // Identifiant unique des modèles, utilisé notamment sur l'interface "M{Key}"
        public string Name { get; set; } //Nom donn� par l'utilisateur, Modifiable

        //Constructors
        public FunctionalModel(string name)
        {
            ElementList = new List<TestedElement>();
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
        
        public FunctionalModel(FunctionalModel functionalModel)
        {
            ElementList = [];
            foreach (var dpt in functionalModel.ElementList)
            {
                ElementList.Add(dpt);
            }
            Name = functionalModel.Name + "(copie)";
        }

        //Methods

        public void AddElement(TestedElement dpt)   //Adds a DPT to the list of DPTs of the functional model
        {
            TestedElement dptToAdd = new TestedElement(dpt);
            ElementList.Add(dptToAdd);
        }

        public void RemoveLastElement()        //Removes the last DPT added to the functional model
        {
            ElementList.RemoveAt(ElementList.Count - 1);
        }
        
        public void RemoveElement(int index)        //Removes the DPT of the specified index from the functional model
        {
            ElementList.RemoveAt(index);
        }
        
        public override string ToString() //Fonction utilisée dès que l'affichage d'un modèle et demandé, utilisé par la vue.
        {
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
            if (other is null || other.ElementList.Count!= ElementList.Count) return false;
            var result = true;
            for (var i = 0;i<ElementList.Count;i++)
            {
                result &= ElementList[i].IsEqual(other.ElementList[i]);
            }

            return result; //&& Key == other.Key;
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

    }
}

