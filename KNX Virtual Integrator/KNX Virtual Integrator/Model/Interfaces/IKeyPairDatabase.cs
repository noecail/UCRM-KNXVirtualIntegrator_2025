namespace KNX_Virtual_Integrator.Model.Interfaces

public interface IKeyPairDatabase{

    public void Add(string key1, string key2);

    public void Remove(string key1, string key2);

    public List<string> GetByKey1(string key1);

    public List<string> GetByKey2(string key2);
    
}