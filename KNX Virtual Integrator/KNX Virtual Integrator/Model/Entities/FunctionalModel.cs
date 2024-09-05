using System.ComponentModel;

namespace KNXIntegrator.Models
{
    public class FunctionalModel : IEquatable<FunctionalModel>
    {
        //Attributs
        private int dpt_value;
        public int Key { get; set; }
        public string Name { get; set; }

        //Affichage dans la vue
        public string DisplayText => $"M{Key} | {Name}";

        //Constructeur
        public FunctionalModel(int value, string name)
        {
            dpt_value = value;
            Name = name;
        }

        //Methodes
        public override string ToString()
        {
            return $"M{Key} | {Name}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is FunctionalModel other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(FunctionalModel? other)
        {
            if (other is null) return false;
            return dpt_value == other.dpt_value && Name == other.Name && Key == other.Key;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(dpt_value, Name, Key);
        }

    }
}

