using System.Collections.Generic;

namespace KNXIntegrator.Models
{
    public interface IFunctionalModelDictionary
    {
        // Ajoute un modèle avec une clé générée automatiquement
        void Add_FunctionalModel(FunctionalModel functionalModel);

        // Supprime un modèle en fonction de la clé
        void Remove_FunctionalModel(int key);

        // Récupère une liste de tous les modèles
        List<FunctionalModel> GetAllModels();

        // Récupère un modèle spécifique par clé
        FunctionalModel Get_FunctionalModel(int key);
    }
}
