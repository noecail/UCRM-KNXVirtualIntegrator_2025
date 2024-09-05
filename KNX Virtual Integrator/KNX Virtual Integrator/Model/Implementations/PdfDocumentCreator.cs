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
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Path to the latest PDF report generated
    /// </summary>
    public string LatestReportPath { get; private set; } = "";

    
    // ⚠️ POUR TESTER UNIQUEMENT ⚠️
    private List<string> _modelesFonctionnels = new();
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Creates a PDF document at the specified file path, with the given author name.
    /// The PDF is generated in A4 format without margins, and includes a header, project information,
    /// and a tree structure. The generated file path is stored as the latest report path.
    /// </summary>
    /// <param name="fileName">The file path where the PDF will be saved.</param>
    /// <param name="authorName">The name of the author to include in the project information section.</param>
    public void CreatePdf(string fileName, string authorName)
    {
        // ⚠️ POUR TESTER UNIQUEMENT ⚠️
        _modelesFonctionnels.Add("test");
        _modelesFonctionnels.Add("test2");
        _modelesFonctionnels.Add("test3");
        _modelesFonctionnels.Add("test4");
        _modelesFonctionnels.Add("test5");
        
        // Génération d'un PDF format A4 sans marges
        var document = new Document(PageSize.A4, 0, 0, 0, 0);
        
        // Création d'un writer pour écrire dans le PDF
        var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));

        // Ouverture du document pour écrire dedans
        document.Open();

        // Ecriture du contenu du document PDF
        GeneratePdfHeader(document, writer); // Génération de la bannière d'en-tête
        GenerateProjectInformationSection(document, authorName); // Génération de la section d'infos du projet (nom, ...)
        GenerateTreeStructure(document, writer);

        // Fermeture du document et du stream d'écriture
        document.Close();
        writer.Close();
        
        // Mise à jour du path du dernier pdf généré
        LatestReportPath = fileName;
    }

    
    /// <summary>
    /// Creates a PDF document at the specified file path, with the given author name, 
    /// and then opens the generated PDF. The PDF is in A4 format without margins 
    /// and includes a header, project information, and a tree structure. 
    /// The generated file path is stored as the latest report path, and the PDF is automatically opened in Windows.
    /// </summary>
    /// <param name="fileName">The file path where the PDF will be saved.</param>
    /// <param name="authorName">The name of the author to include in the project information section.</param>
    public void CreateAndOpenPdf(string fileName, string authorName)
    {
        // ⚠️ POUR TESTER UNIQUEMENT ⚠️
        _modelesFonctionnels.Add("test");
        _modelesFonctionnels.Add("test2");
        _modelesFonctionnels.Add("test3");
        _modelesFonctionnels.Add("test4");
        _modelesFonctionnels.Add("test5");
        
        // Génération d'un PDF format A4 sans marges
        var document = new Document(PageSize.A4, 0, 0, 0, 0);
        
        // Création d'un writer pour écrire dans le PDF
        var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));

        // Ouverture du document pour écrire dedans
        document.Open();

        // Ecriture du contenu du document PDF
        GeneratePdfHeader(document, writer); // Génération de la bannière d'en-tête
        GenerateProjectInformationSection(document, authorName); // Génération de la section d'infos du projet (nom, ...)
        GenerateTreeStructure(document, writer);

        // Fermeture du document et du stream d'écriture
        document.Close();
        writer.Close();
        
        // Mise à jour du path du dernier pdf généré
        LatestReportPath = fileName;

        // Ouverture du PDF dans Windows
        OpenLatestReport();
    }

    public void ClosePdf()
    {
        // Non implémentée
    }

    
    /// <summary>
    /// Generates the header section of the PDF, including a banner, logo, 
    /// software name, version information, current date, and the document title. 
    /// The banner is drawn as a filled rectangle, with the software's logo and name displayed on the left, 
    /// and version information and the current date on the right. A title is added below the header.
    /// </summary>
    /// <param name="document">The PDF document to which the header will be added.</param>
    /// <param name="writer">The PdfWriter instance responsible for writing content into the document.</param>
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
    
    
        // Logo du logiciel
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

    
    /// <summary>
    /// Generates the project information section of the PDF, displaying the evaluated installation's name 
    /// and, if provided, the evaluator's username. It includes a separator line, the project name in bold, 
    /// and optionally the evaluator's name. The section is formatted with indentation and spacing.
    /// </summary>
    /// <param name="document">The PDF document to which the project information section will be added.</param>
    /// <param name="username">The name of the person conducting the evaluation, optional.</param>
    private void GenerateProjectInformationSection(Document document, string username = "")
    {
        // Définition des polices d'écriture
        var underlineFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, Font.UNDERLINE);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        var regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
        
        // Définition de la couleur du trait de séparation
        var separatorColor = new BaseColor(215, 215, 215); // RGB pour #D7D7D7

        
        
        // Ajout d'un trait de séparation avec la couleur définie
        var separator = new LineSeparator(0.5f, 80f, separatorColor, Element.ALIGN_CENTER, 1); // Utilisez une épaisseur de 1 pour une ligne fine
        document.Add(new Chunk(separator));
    
        
        // Nom de l'installation évaluée
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
        document.Add(projectInformationParagraph);

        
        // Nom de l'auteur du rapport
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

        
        // Structure de l'installation
        var projectStructureParagraph = new Paragraph("Structure de l'installation évaluée :", underlineFont)
        {
            SpacingBefore = 20f,
            IndentationLeft = 5f
        };
        document.Add(projectStructureParagraph);
        
        
        // TODO Portion de code à remplacer par la génération de deux arborescences:
        // TODO 1 pour montrer le lien entre les CMD, les IE et les modèles de tests
        // TODO 1 pour montrer la structure du bâtiment
        var img = Image.GetInstance(@"C:\Users\maxim\Downloads\screenshot en attendant.png");
        img.Alignment = Element.ALIGN_CENTER;
        img.SpacingAfter = 5f;
        img.ScaleToFit(document.PageSize.Width, 0.3810169491525424f*document.PageSize.Width);
        document.Add(img);
        
        
        
        // Ajout d'un trait de séparation avec la couleur définie
        var separator2 = new LineSeparator(0.5f, 80f, separatorColor, Element.ALIGN_CENTER, 1); // Utilisez une épaisseur de 1 pour une ligne fine
        document.Add(new Chunk(separator2));
        
        
        // Affichage des résultats des tests
        var conductedTests = new Paragraph("Tests réalisés :", underlineFont)
        {
            SpacingBefore = 15f,
            IndentationLeft = 5f
        };
        document.Add(conductedTests);
        
        // TODO ICI IL MANQUE DU COUP L'AFFICHAGE DE TOUS LES RESULTATS (Voir fonction GenerateTestList)
    }
    
    
    // TODO -- Cette fonction doit générer deux arborescences: une pour détailler les liens entre les CMD et les IE (voir screen actuellement dans le PDF)
    // TODO -- Et un pour détailler la structure du bâtiment
    // TODO -- En l'état le code fonctionne mais ne donne pas le résultat attendu sur le PDF
    /// <summary>
    /// Generates a tree structure of commands within the PDF. The tree includes categories, commands, 
    /// and labels with specific colors, displayed as a hierarchical list. Each command entry is represented 
    /// with a background color for the label, followed by the command text. Introductory and concluding 
    /// text is added to provide context for the structure.
    /// </summary>
    /// <param name="document">The PDF document to which the tree structure will be added.</param>
    /// <param name="writer">The PdfWriter instance responsible for writing content into the document.</param>
    private void GenerateTreeStructure(Document document, PdfWriter writer)
    {
        // // Font setup
        // Font normalFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK);
        // Font boldFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.BLACK);
        // Font labelFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
        //
        // // Example data structure for the tree
        // var treeStructure = new List<(string category, string command, string label, BaseColor color)>
        // {
        //     ("ECLAIRAGE SIMPLE > ON/OFF", "Cmd_Eclairage_OnOff_MaisonDupre_RezDeChaussee_Etage_Salon", "M2", BaseColor.GREEN),
        //     ("ECLAIRAGE VARIABLE > ON/OFF", "Cmd_Eclairage_OnOff_MaisonDupre_RezDeChaussee_Etage_Entree", "M2", BaseColor.GREEN),
        //     ("ECLAIRAGE VARIABLE > VARIATIONS", "Cmd_Eclairage_Variations_MaisonDupre_RezDeChaussee_Etage_Entree", "M3", BaseColor.GREEN),
        //     ("ECLAIRAGE VARIABLE > VALEURS VARIATION", "Cmd_Eclairage_Variation_MaisonDupre_RezDeChaussee_Etage_Tgbt", "M4", BaseColor.GREEN)
        //     // Add more entries as needed
        // };
        //
        // // Add some introductory text
        // document.Add(new Paragraph("Voici l'arborescence des commandes :", boldFont));
        //
        // foreach (var (category, command, label, color) in treeStructure)
        // {
        //     // Create a paragraph for each command
        //     Paragraph paragraph = new Paragraph();
        //
        //     // Add category (only once if it changes)
        //     paragraph.Add(new Chunk(category + "\n", boldFont));
        //
        //     // Create a rectangle chunk
        //     Chunk rectangleChunk = new Chunk(" " + label + " ", labelFont);
        //     rectangleChunk.SetBackground(color, 2f, 2f, 2f, 2f); // Add padding
        //
        //     // Add the rectangle and command text
        //     paragraph.Add(rectangleChunk);
        //     paragraph.Add(new Chunk(" " + command + "\n", normalFont));
        //
        //     // Add the paragraph to the document
        //     document.Add(paragraph);
        // }
        //
        // // Add some text after the tree structure
        // document.Add(new Paragraph("Texte après l'arborescence pour vérifier la continuité du contenu.", normalFont));
    }


    // Fonction à implémenter, elle servira à générer les résultats de tests de manière générique
    private void GenerateTestList(Document document, PdfWriter writer)
    {
        foreach (var st in _modelesFonctionnels)
        {
            
        }
    }


    /// <summary>
    /// Opens the most recently generated PDF report using the default system application.
    /// The file path of the latest report is retrieved from the LatestReportPath variable.
    /// </summary>
    public void OpenLatestReport()
    {
        Process.Start(new ProcessStartInfo(LatestReportPath) { UseShellExecute = true });
    }

}