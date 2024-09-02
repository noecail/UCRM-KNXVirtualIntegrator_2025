namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IPdfDocumentCreator
{
    void CreatePdf(string name);

    void CreateAndOpenPdf(string name);

    void ClosePdf();

    void GenerateReport();
}