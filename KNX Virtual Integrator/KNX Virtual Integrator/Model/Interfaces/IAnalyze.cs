using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.Model.Interfaces;
/// <summary>
/// Class used to analyze the knx installation according to a list of functional models.
/// The models each have a list of Element, which have a list of commands and expected results.
/// The results are thus listed as a list of (models)lists of (elements)lists of (commands)lists of (expected results)<see cref="ResultType"/>.
/// </summary>
public interface IAnalyze
{    
    /// <summary>
    /// Table of results sorted by Tests, in TestedElements, in functionalModels
    /// </summary>
    List<List<List<List<ResultType>>>> Results { get; set; }

    /// <summary>
    /// Tests all the functional models of a list and updates the table of results
    /// </summary>
    Task TestAll();
}

