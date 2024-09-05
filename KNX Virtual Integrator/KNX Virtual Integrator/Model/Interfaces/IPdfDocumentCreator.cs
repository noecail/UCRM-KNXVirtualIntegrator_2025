namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IPdfDocumentCreator
{
    void CreatePdf(string fileName, string authorName);

    void CreateAndOpenPdf(string fileName, string authorName);

    void ClosePdf();

    void OpenLatestReport();
}