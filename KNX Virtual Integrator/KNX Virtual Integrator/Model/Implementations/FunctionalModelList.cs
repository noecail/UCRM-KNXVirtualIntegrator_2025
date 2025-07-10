using KNX_Virtual_Integrator.Model.Entities;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class FunctionalModelList
{
    public List<FunctionalModel> FunctionalModels = [];
    private FunctionalModelDictionary _functionalModelDictionary;

    public FunctionalModelList(FunctionalModelDictionary functionalModelDictionary)
    {
        _functionalModelDictionary = functionalModelDictionary;
    }
    
    
    
    
}