using System.Xml.Linq;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IGroupAddressRepository{

    public List<XElement> GetGrpAddr(string key);
    
}