using System.Collections.Generic;

namespace KNXIntegrator.Models
{
    public interface IFunctionalModelDictionary
    {
        // Ajouter un modèle au dictionnaire
        void Add_FunctionalModel(FunctionalModel functionalModel);

        // Supprimer un modèle du dictionnaire par sa clé
        void Remove_FunctionalModel(int key);

        // Récupérer tous les modèles
        List<FunctionalModel> GetAllModels();

        // Récupérer un modèle par sa clé
        FunctionalModel Get_FunctionalModel(int key);

        // Mettre à jour un modèle existant dans le dictionnaire
        void UpdateModel(FunctionalModel model);
    }
}

