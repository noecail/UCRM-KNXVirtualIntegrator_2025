using System;
using System.Linq;
using KNX_Virtual_Integrator.Model.Entities;
using Knx.Falcon;


namespace KNXIntegrator.Models
{
    //Find Summary in the interface
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        private Dictionary<int, FunctionalModel> _functionalModels;
        private int _currentKey;

        public FunctionalModelDictionary()
        {
            _functionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence � 0 pour que la premi�re cl� soit 1
           /* _functionalModels.Add(++_currentKey,new FunctionalModel([new TestedElement(1,[[new GroupValue(true),new GroupValue(true)]])],"Lumière ON/OFF")); //Adding On/Off light functional model
            _functionalModels.Add(++_currentKey,new FunctionalModel([new TestedElement(1,[[new GroupValue(true),new GroupValue(true)]]),new TestedElement
                (3,[[new GroupValue(1),null],[new GroupValue(0xF1),null]]),new TestedElement(5,[[new GroupValue(1),new GroupValue(1)],[new GroupValue(222),new GroupValue(222)]])],"Lumière variation")); //Adding variable light functional model*/
        }

        public void Add_FunctionalModel(FunctionalModel functionalModel)
        {
            _currentKey++;
            functionalModel.Key = _currentKey;  // Associer la cl� au mod�le
            _functionalModels.Add(_currentKey, functionalModel);
        }

        public void Remove_FunctionalModel(int key)
        {
            _functionalModels.Remove(key);
        }

        public List<FunctionalModel> GetAllModels()
        {
            return new List<FunctionalModel>(_functionalModels.Values);
        }

    }
}

