using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Class handling the generation of the analysis report PDF
/// </summary>
/// <param name="manager">To get the project name and include it in the PDF</param>
public class PdfDocumentCreator (ProjectFileManager manager) : IPdfDocumentCreator
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Path to the latest PDF report generated
    /// </summary>
    public string LatestReportPath { get; private set; } = "";
    private PdfWriter? _writer;

    
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- MÉTHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Creates a PDF document at the specified file path, with the given author name.
    /// The PDF is generated in A4 format without margins, and includes a header, project information,
    /// and a tree structure. The generated file path is stored as the latest report path.
    /// </summary>
    /// <param name="fileName">The file path where the PDF will be saved.</param>
    /// <param name="authorName">The name of the author to include in the project information section.</param>
    /// <param name="testedList"></param>
    /// <param name="testResults"></param>
    public void CreatePdf(string fileName, string authorName, ObservableCollection<FunctionalModel>  testedList, List<List<List<List<ResultType>>>> testResults)
    {
        try
        {
            if (fileName.Length == 0)
            {
                return;
            }

            // Génération d'un PDF et du writer pour écrire dans le Pdf
            _writer = new PdfWriter(new FileStream(fileName, FileMode.Create));
            var newPdf = new PdfDocument(_writer);
            var doc = new Document(newPdf, PageSize.A4);
            doc.SetMargins(72, 72, 72, 72);
            doc.SetFontSize(14);

            
            // Écriture du contenu du document PDF
            GeneratePdfHeader(doc); // Génération de la bannière d'en-tête
            
            GenerateProjectInformationSection(doc, authorName); // Génération de la section d'infos du projet (nom, ...)
            
            GenerateTestListAndResults(doc, testedList, testResults);
            //GenerateTreeStructure();
            


            // Fermeture du document et du stream d'écriture
            doc.Close();
            _writer.Close();
            // Mise à jour du path du dernier pdf généré
            LatestReportPath = fileName;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + "         " + ex.HelpLink + " :: "  + ex.Source);
        }
    }


    /// <summary>
    /// Generates the header section of the PDF, including a banner, logo, 
    /// software name, version information, current date, and the document title. 
    /// The banner is drawn as a filled rectangle, with the software's logo and name displayed on the left, 
    /// and version information and the current date on the right. A title is added below the header.
    /// </summary>
    /// <param name="document">The PDF document to which the header will be added.</param>
    private void GeneratePdfHeader(Document document)
    {
        
        // Logo du logiciel (les 2 lignes qui suivent ne servent pas pour le moment)
        //var logo = new Image(ImageDataFactory.Create("../../../Resources/resources/logoUCRM.png")).ScaleToFit(60f, 60f).SetTextAlignment(TextAlignment.RIGHT).SetFixedPosition(500,780);
        //MessageBox.Show("Passé création image");
        //document.Add(logo);
        //MessageBox.Show("Passé addition image");
        // Nom du logiciel, à côté du logo
        document.Add(new Paragraph($"{App.AppName}"));
        
        // Information sur la version du logiciel utilisée pour générer le document
        document.Add(new Paragraph($"V{App.AppVersion.ToString(CultureInfo.InvariantCulture)}, build {App.AppBuild}"));
        
        // Date du jour
        document.Add(new Paragraph(DateTime.Now.ToString("dd/MM/yyyy")));
        
        // Ajout d'un paragraphe vide pour créer un espace au-dessus du titre
        var emptyParagraph = new Paragraph("\n");
        document.Add(emptyParagraph);
        
        // Information du lexique du rapport
        document.Add(new Paragraph("Les résultats sont présentés commande par commande, avec un résultat par réception.\n"+
                                   "Il y a 5 types de résultat pour l'analyse : "));
        document.SetFontSize(12);
        document.Add(new Paragraph(" - Success indique que la réception a reçu la valeur attendue de l'adresse concernée\n" +
                                   " - Response indique que la réception a reçu le message de présence mais pas la\n\t valeur (cela peut être normal si la même valeur est attendue/envoyée 2 fois de suite)\n"+
                                   " - Failure indique que la réception n'a ni reçu le message de présence, ni le message\n\t d'acquittement avec la valeur attendue\n"+
                                   " - Address Error indique que la commande et/ou la réception sont mal configurées\n\t pour l'adresse\n" + 
                                   " - L'absence d'information indique une absence de réception ou d'envoi"));
        document.SetFontSize(14);
        document.Add(new Paragraph("\n\n"));
        
        // Titre du document
        var titleParagraph = new Paragraph("RAPPORT DE FONCTIONNEMENT DE L’INSTALLATION KNX");
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
        
        // Définition de la couleur du trait de séparation
        
        // Ajout d'un trait de séparation avec la couleur définie
        
        // Nom de l'installation évaluée
        
        if (!string.IsNullOrWhiteSpace(manager.ProjectName))
        {
            var installationChunk = new Paragraph("Projet évalué : ");
            var projectNameChunk = new Paragraph($" {manager.ProjectName}");

            // Combiner les deux Chunk dans un Paragraph
            var projectInformationParagraph = new Paragraph().Add(installationChunk).Add(projectNameChunk);
            document.Add(projectInformationParagraph);
        }
        else
        {
             var projectInformationParagraph = new Paragraph("Nouveau projet évalué");
            document.Add(projectInformationParagraph);
        }

        // Nom de l'auteur du rapport
        if (!string.IsNullOrWhiteSpace(username))
        {
            var evaluationChunk = new Paragraph("Evaluation menée par : ");
            var usernameChunk = new Paragraph($" {username}");
            
            var evaluatorNameParagraph = new Paragraph().Add(evaluationChunk).Add(usernameChunk);
            // Ajouter le paragraphe au document
            document.Add(evaluatorNameParagraph);
        }
    }

    /// <summary>
    /// NaN
    /// </summary>
    /// <param name="document"></param>
    /// <param name="testedList"></param>
    /// <param name="testResults"></param>
    private void GenerateTestListAndResults(Document document, ObservableCollection<FunctionalModel>  testedList, List<List<List<List<ResultType>>>> testResults)
    {
        int i=0, j, k, l;
        foreach (var testedModel in testedList)
        {
            j = 0;
            document.Add(new Paragraph($"Modèle testé : {testedModel.FullName}"));
            foreach (var testedElement in testedModel.ElementList)
            {
                k = 0;
                document.Add(new Paragraph("Élément n°" + (j+1)));
                foreach (var testedCommand in testedElement.TestsCmd[0].Value)
                {
                    l = 0;
                    var resultString = "Commande n°" + (k+1) + " :   ";
                    foreach (var testedIe in testedElement.TestsIe)
                    {
                        if (testedIe.Address == "")
                        {
                            resultString += "Address Error " + (testedElement.TestsIe.Count==1 || testedElement.TestsIe.Last().IsEqual(testedIe)?"":", ") ;
                            break;
                        }
                        resultString += testResults[i][j][k][l] + (testedElement.TestsIe.Count==1 || testedElement.TestsIe.Last().IsEqual(testedIe)?"":", ") ;
                        l++;
                    }
                    Paragraph resultParagraph = new Paragraph(resultString);
                    document.Add(resultParagraph);
                    k++;
                }
                j++;
            }

            i++;
        }
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
    private void GenerateTreeStructure()
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

    /// <summary>
    /// Opens the most recently generated PDF report using the default system application.
    /// The file path of the latest report is retrieved from the LatestReportPath variable.
    /// </summary>
    public void OpenLatestReport()
    {
        Process.Start(new ProcessStartInfo(LatestReportPath) { UseShellExecute = true });
    }

}