namespace KNX_Virtual_Integrator.Model.Interfaces

public interface IFunctionalModelDictionary{
    public void Add_FunctionalModel (string key, FunctionalModel functionalModel);

    public void Remove_FunctionalModel (string key);

    public List<FunctionalModel> GetAllModels();

    public FunctionalModel Get_FunctionalModel(string key);
}