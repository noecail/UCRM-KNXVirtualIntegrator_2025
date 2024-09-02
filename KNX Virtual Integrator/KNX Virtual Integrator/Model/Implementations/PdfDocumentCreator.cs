using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class PdfDocumentCreator : IPdfDocumentCreator
{
    public void CreatePdf(string name)
    {
        // Créer un document
        // Créez un document PDF sans marges
        var document = new Document(PageSize.A4, 0, 0, 0, 0);


        // Créer un writer qui écoute le document et dirige un fichier
        var writer = PdfWriter.GetInstance(document, new FileStream(name, FileMode.Create));

        // Ouvrir le document pour l'écriture
        document.Open();

        // Génération de la bannière du document
        GeneratePdfHeader(document, writer);

        // Fermer le document
        document.Close();

        // Fermer le writer
        writer.Close();
    }

    public void CreateAndOpenPdf(string name)
    {
        
    }

    public void ClosePdf()
    {
        
    }

    public void GenerateReport()
    {
        
    }

    
    private void GeneratePdfHeader(Document document, PdfWriter writer)
    {
        // Obtenez PdfContentByte du writer
        var canvas = writer.DirectContentUnder;

        // Définissez la couleur du rectangle (par exemple, gris clair)
        canvas.SetColorFill(new BaseColor(54, 144, 38));

        // Définissez la position et la taille du rectangle
        const int llx = 0; // Coordonnée x du coin inférieur gauche
        const int lly = 800; // Coordonnée y du coin inférieur gauche
        const int urx = 595; // Coordonnée x du coin supérieur droit
        const int ury = 842; // Coordonnée y du coin supérieur droit

        // Dessinez le rectangle
        canvas.Rectangle(llx, lly, urx - llx, ury - lly);
        canvas.Fill();

        var cb = writer.DirectContent;
            
        // Charger l'image du logo
        var logo = Image.GetInstance(@"C:\Users\maxim\Downloads\BOOST.png");
        logo.ScaleToFit(42f, 42f); // Ajuster la taille du logo
            
        // Ajouter l'image à une position spécifique (x, y) sur la page
        logo.SetAbsolutePosition(36f, document.PageSize.Height - 72f); // Position en bas à gauche, ajustez selon vos besoins
        cb.AddImage(logo);
            
        // Texte central
        var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        
        cb.BeginText();
        cb.SetFontAndSize(baseFont, 12);
        cb.SetTextMatrix(100f, document.PageSize.Height - 60f); // Position du texte central, ajustez selon vos besoins
        cb.ShowText($"{App.AppName} V{App.AppVersion.ToString(CultureInfo.InvariantCulture)} BUILD {App.AppBuild}");
        cb.EndText();
            
        // Texte date en haut à droite
        var dateFont = new Font(Font.FontFamily.HELVETICA, 10);
        cb.BeginText();
        cb.SetFontAndSize(baseFont, 10);
        cb.SetTextMatrix(document.PageSize.Width - 100f, document.PageSize.Height - 60f); // Position du texte de la date, ajustez selon vos besoins
        cb.ShowText("28/08/2024");
        cb.EndText();

        document.Add(new Paragraph("\nRAPPORT DE FONCTIONNEMENT DE L’INSTALLATION KNX",
            new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)));
        
        // Ajouter du contenu après l'en-tête (exemple)
        document.Add(new Paragraph("\n\nInstallation évaluée : AEID PG ELEC VILLA 56 nommage", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)));
    }
}