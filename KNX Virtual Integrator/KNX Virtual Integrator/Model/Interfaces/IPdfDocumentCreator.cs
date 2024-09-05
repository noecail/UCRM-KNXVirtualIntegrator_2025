namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IPdfDocumentCreator
{
    void CreatePdf(string fileName, string authorName);

    void CreateAndOpenPdf(string name);

    void ClosePdf();

    void OpenLatestReport();
}