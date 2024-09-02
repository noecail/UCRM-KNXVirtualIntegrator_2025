using System.Diagnostics;
using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class PdfDocumentCreator (ProjectFileManager manager) : IPdfDocumentCreator
{
    public string LatestReportPath { get; private set; } = "";
    
    
    public void CreatePdf(string name)
    {
        // Génération d'un PDF format A4 sans marges
        var document = new Document(PageSize.A4, 0, 0, 0, 0);
        
        // Création d'un writer pour écrire dans le PDF
        var writer = PdfWriter.GetInstance(document, new FileStream(name, FileMode.Create));

        // Ouverture du document pour écrire dedans
        document.Open();

        // Ecriture du contenu du document PDF
        GeneratePdfHeader(document, writer); // Génération de la bannière d'en-tête
        GenerateProjectInformationSection(document, writer, "Maxou"); // Génération de la section d'infos du projet (nom, ...)

        // Fermeture du document et du stream d'écriture
        document.Close();
        writer.Close();
        
        // Mise à jour du path du dernier pdf généré
        LatestReportPath = name;

        // Ouverture du PDF dans Windows
        OpenLatestReport();
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
        // Génération du rectangle d'en-tête (bannière)
        var canvas = writer.DirectContentUnder;
        canvas.SetColorFill(new BaseColor(54, 144, 38));
        const int llx = 0; // Coordonnée x du coin inférieur gauche du rectangle
        const int lly = 800; // Coordonnée y du coin inférieur gauche du rectangle
        const int urx = 595; // Coordonnée x du coin supérieur droit du rectangle
        const int ury = 842; // Coordonnée y du coin supérieur droit du rectangle
        canvas.Rectangle(llx, lly, urx - llx, ury - lly); // Dessin du rectangle
        canvas.Fill(); // Remplissage du rectangle

    
        var cb = writer.DirectContent;
    
    
        // // Logo du logiciel
        // var logo = Image.GetInstance(@"C:\Users\maxim\Downloads\BOOST.png");
        // logo.ScaleToFit(42f, 42f); // Ajuster la taille du logo
        //     
        // // Ajouter l'image à une position spécifique (x, y) sur la page
        // logo.SetAbsolutePosition(0f, document.PageSize.Height - 42f); // Position en bas à gauche, ajustez selon vos besoins
        // cb.AddImage(logo);
        
    
        // Nom du logiciel, à côté du logo
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        cb.BeginText();
        cb.SetFontAndSize(boldFont.BaseFont, 18);
        cb.SetColorFill(BaseColor.WHITE);
        cb.SetTextMatrix(50f, document.PageSize.Height - 28f);
        cb.ShowText($"{App.AppName}");
        cb.EndText();
    
    
        // Information sur la version du logiciel utilisée pour générer le document
        var italicFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 12);
        cb.BeginText();
        cb.SetFontAndSize(italicFont.BaseFont, 8);
        cb.SetColorFill(BaseColor.WHITE);
        cb.SetTextMatrix(245f, document.PageSize.Height - 28f); // Position du texte central, ajustez selon vos besoins
        cb.ShowText($"V{App.AppVersion.ToString(CultureInfo.InvariantCulture)} build {App.AppBuild}");
        cb.EndText();
       
    
        // Date du jour
        var dateFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        cb.SetColorFill(BaseColor.WHITE);
        cb.BeginText();
        cb.SetFontAndSize(dateFont.BaseFont, 14);
        cb.SetTextMatrix(document.PageSize.Width - 74f, document.PageSize.Height - 35f); // Position du texte de la date, ajustez selon vos besoins
        cb.ShowText(DateTime.Now.ToString("dd/MM/yyyy"));
        cb.EndText();
        
        
        // Ajout d'un paragraphe vide pour créer un espace au-dessus du titre
        var emptyParagraph = new Paragraph("\n", boldFont);
        document.Add(emptyParagraph);
    
    
        // Titre du document
        boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var titleParagraph = new Paragraph("RAPPORT DE FONCTIONNEMENT DE L’INSTALLATION KNX", boldFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 25f,
                SpacingAfter = 0f
            };
        document.Add(titleParagraph);
    }

    private void GenerateProjectInformationSection(Document document, PdfWriter writer, string username = "")
    {
        var underlineFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, Font.UNDERLINE);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        var regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
    
        // Définir la couleur pour le trait de séparation (gris clair #D7D7D7)
        var separatorColor = new BaseColor(215, 215, 215); // RGB pour #D7D7D7

        // Ajout d'un trait de séparation avec la couleur définie
        var separator = new LineSeparator(0.5f, 80f, separatorColor, Element.ALIGN_CENTER, 1); // Utilisez une épaisseur de 1 pour une ligne fine
        document.Add(new Chunk(separator));
    
        
        
        var installationChunk = new Chunk("Installation évaluée :", underlineFont);
        var projectNameChunk = new Chunk($" {manager.ProjectName}", boldFont);

        // Combiner les deux Chunk dans un Paragraph
        var projectInformationParagraph = new Paragraph
        {
            IndentationLeft = 5f, 
            SpacingBefore = 10f
        };
        projectInformationParagraph.Add(installationChunk);
        projectInformationParagraph.Add(projectNameChunk);
    
        // Ajouter le paragraphe au document
        document.Add(projectInformationParagraph);

        
        
        if (!string.IsNullOrWhiteSpace(username))
        {
            var evaluationChunk = new Chunk("Evaluation menée par :", underlineFont);
            var usernameChunk = new Chunk($" {username}", regularFont);
            
            var evaluatorNameParagraph = new Paragraph
            {
                IndentationLeft = 5f,
                SpacingBefore = 5f
            };
            evaluatorNameParagraph.Add(evaluationChunk);
            evaluatorNameParagraph.Add(usernameChunk);

            // Ajouter le paragraphe au document
            document.Add(evaluatorNameParagraph);
        }


        var projectStructureParagraph = new Paragraph("Structure de l'installation évaluée :", underlineFont)
        {
            SpacingBefore = 20f,
            IndentationLeft = 5f
        };
        document.Add(projectStructureParagraph);
        
        
        // var img = Image.GetInstance(@"C:\Users\maxim\Downloads\screenshot en attendant.png");
        // img.Alignment = Element.ALIGN_CENTER;
        // img.ScaleToFit(document.PageSize.Width, 0.3810169491525424f*document.PageSize.Width);
        // document.Add(img);
        
        
        var conductedTests = new Paragraph("Tests réalisés :", underlineFont)
        {
            SpacingBefore = 20f,
            IndentationLeft = 5f
        };
        document.Add(conductedTests);
    }


    public void OpenLatestReport()
    {
        Process.Start(new ProcessStartInfo(LatestReportPath) { UseShellExecute = true });
    }

}