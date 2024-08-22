namespace KNXIntegrator.Models;

public interface ITransfer{

    public List<Frame> GetSendCommands(int addr,FunctionalModel model);

    public List<Frame> GetExpectedCommands(int addr,FunctionalModel model);
    

}