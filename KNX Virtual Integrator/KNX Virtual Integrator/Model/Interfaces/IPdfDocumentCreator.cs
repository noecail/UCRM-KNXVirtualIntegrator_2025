using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Implementations;

namespace KNX_Virtual_Integrator.Model.Interfaces;
/// <summary>
/// Interface of the class handling the generation of the analysis report PDF
/// </summary>
public interface IPdfDocumentCreator
{
    /// <summary>
    /// Path to the latest PDF report generated
    /// </summary>
    string LatestReportPath { get; } 
    
    /// <summary>
    /// Creates a PDF document at the specified file path, with the given author name.
    /// The PDF is generated in A4 format without margins, and includes a header, project information,
    /// and a tree structure. The generated file path is stored as the latest report path.
    /// </summary>
    /// <param name="fileName">The file path where the PDF will be saved.</param>
    /// <param name="authorName">The name of the author to include in the project information section.</param>
    /// <param name="testedList"></param>
    /// <param name="testResults"></param>
    void CreatePdf(string fileName, string authorName, ObservableCollection<FunctionalModel>  testedList, List<List<List<List<ResultType>>>> testResults);

    /// <summary>
    /// Opens the most recently generated PDF report using the default system application.
    /// The file path of the latest report is retrieved from the LatestReportPath variable.
    /// </summary>
    public void OpenLatestReport();
}