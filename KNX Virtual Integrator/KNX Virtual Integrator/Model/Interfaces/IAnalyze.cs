namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IAnalyze
{
    List<List<List<List<bool>>>> Results { get; set; }

    Task TestAll();
}

