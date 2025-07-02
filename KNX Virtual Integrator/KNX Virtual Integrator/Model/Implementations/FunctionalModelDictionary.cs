using System;
using System.Linq;
using KNX_Virtual_Integrator.Model.Entities;


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
            _currentKey = 0; // Commence � 0 pour que la premi�re cl� soit 1
            functionalModels.Add(++_currentKey,new FunctionalModel([new TestedElement(1,[1],[1])],"Lumière ON/OFF")); //Adding On/Off light functional model
            functionalModels.Add(++_currentKey,new FunctionalModel([new TestedElement(1,new List<ulong?>{1},[1]),new TestedElement
                (3,[1],[null,null]),new TestedElement(5,[1,222],[1,222])],"Lumière variation")); //Adding variable light functional model
        }

        public void Add_FunctionalModel(FunctionalModel functionalModel)
        {
            _currentKey++;
            functionalModel.Key = _currentKey;  // Associer la cl� au mod�le
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

