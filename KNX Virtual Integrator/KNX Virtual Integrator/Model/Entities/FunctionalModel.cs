namespace KNXIntegrator.Models
{
    public class FunctionalModel : IEquatable<FunctionalModel>
    {
        private int dpt_value;

        // Nouvelle propriété pour la clé
        public int Key { get; set; }

        public string Name { get; set; }

        // Propriété calculée pour le format d'affichage
        public string DisplayText => $"M{Key} | {Name}";

        public FunctionalModel(int value, string name)
        {
            dpt_value = value;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} ({dpt_value})";
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

