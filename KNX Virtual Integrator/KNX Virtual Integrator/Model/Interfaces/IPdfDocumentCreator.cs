using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IPdfDocumentCreator
{
    void CreatePdf(string fileName, string authorName, ObservableCollection<FunctionalModel>  testedList, List<List<List<List<ResultType>>>> testResults);

}