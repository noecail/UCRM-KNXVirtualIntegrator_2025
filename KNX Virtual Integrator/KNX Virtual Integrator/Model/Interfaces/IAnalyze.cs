using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IAnalyze
{
    List<List<List<List<ResultType>>>> Results { get; set; }

    Task TestAll();
}

