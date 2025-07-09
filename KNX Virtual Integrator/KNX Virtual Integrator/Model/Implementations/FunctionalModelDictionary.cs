using System;
using System.Linq;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;


namespace KNX_Virtual_Integrator.Model.Implementations
{
    //Find Summary in the interface
    public class FunctionalModelDictionary : IFunctionalModelDictionary
    {
        private Dictionary<int, FunctionalModel> _functionalModels;
        private int _currentKey;

        public FunctionalModelDictionary()
        {
            _functionalModels = new Dictionary<int, FunctionalModel>();
            _currentKey = 0; // Commence à 0 pour que la première clé soit 1
            _functionalModels.Add(++_currentKey,new FunctionalModel([new TestedElement()],"Lumière ON/OFF")); //Adding On/Off light functional model
            _functionalModels.Add(++_currentKey,new FunctionalModel([new TestedElement(1,[],new GroupValue(true),[1],[[]],[new GroupValue(true)]), // Variation functional model : First element : On/Off
                new TestedElement(5,[],new GroupValue(0xF),[1,5],[[],[]],[new GroupValue(true),new GroupValue(0xF)]), //Second element : absolute change
                new TestedElement(3,[],new GroupValue(0x8),[1,5],[[],[]],[new GroupValue(true),null])],"Lumière variation")); //Third element : relative change
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

