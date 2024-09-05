using System;
using System.Linq;


namespace KNXIntegrator.Models
{
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        private Dictionary<int, FunctionalModel> functionalModels;
        private int _currentKey;

        public FunctionalModelDictionary()
        {
            functionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
        }

        // Ajouter un modèle au dictionnaire
        public void Add_FunctionalModel(FunctionalModel functionalModel)
        {
            _currentKey++;
            functionalModel.Key = _currentKey;  // Associer la clé au modèle
            functionalModels.Add(_currentKey, functionalModel);
        }

        // Supprimer un modèle du dictionnaire par sa clé
        public void Remove_FunctionalModel(int key)
        {
            functionalModels.Remove(key);
        }

        // Récupérer tous les modèles
        public List<FunctionalModel> GetAllModels()
        {
            return new List<FunctionalModel>(functionalModels.Values);
        }

        // Récupérer un modèle par sa clé
        public FunctionalModel Get_FunctionalModel(int key)
        {
            return functionalModels[key];
        }

        // Mettre à jour un modèle existant dans le dictionnaire
        public void UpdateModel(FunctionalModel model)
        {
            if (model != null && functionalModels.ContainsKey(model.Key))
            {
                // Met à jour le modèle existant dans le dictionnaire en utilisant la clé
                functionalModels[model.Key] = model;
            }
            else
            {
                throw new KeyNotFoundException($"Le modèle avec la clé {model?.Key} n'existe pas dans le dictionnaire.");
            }
        }
    }
}

