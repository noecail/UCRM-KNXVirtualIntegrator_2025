using System;
using System.Linq;


namespace KNXIntegrator.Models
{
    //Find Summary in the interface
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        private Dictionary<int, FunctionalModel> functionalModels;
        private int _currentKey;

        public FunctionalModelDictionary()
        {
            functionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
        }

        public void Add_FunctionalModel(FunctionalModel functionalModel)
        {
            _currentKey++;
            functionalModel.Key = _currentKey;  // Associer la clé au modèle
            functionalModels.Add(_currentKey, functionalModel);
        }

        public void Remove_FunctionalModel(int key)
        {
            functionalModels.Remove(key);
        }

        public List<FunctionalModel> GetAllModels()
        {
            return new List<FunctionalModel>(functionalModels.Values);
        }

    }
}

