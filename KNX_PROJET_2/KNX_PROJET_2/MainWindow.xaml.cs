using System.Windows;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Interop;
using System.IO;

namespace KNX_PROJET_2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        //ATTRIBUTS

        /// <summary>
        /// Represents the global XML namespace for KNX projects.
        /// </summary>
        private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

        /// <summary>
        /// Gets the path to the exported project folder.
        /// </summary>
        public string ProjectFolderPath { get; private set; } = "";




        // Gestion du clic sur le bouton Importer
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {

            // Créer une instance de OpenFileDialog pour sélectionner un fichier XML
            OpenFileDialog openFileDialog = new()
            {
                Title = "Sélectionner un fichier XML",
                Filter = "Fichiers XML|*.xml|Tous les fichiers|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            // Afficher la boîte de dialogue et vérifier la sélection
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Appeler la méthode asynchrone pour afficher les adresses de groupe
                await ImportListGroupAddress(filePath);
            }
        }
        
        
        
        //TACHE IMPORTER LISTE DES ADRESSES DE GROUPE
        private async Task ImportListGroupAddress(string filePath)
        {
            try
            {
                // Charger les adresses de groupe à partir du fichier XML
                XDocument doc = XDocument.Load(filePath);

                // Supposez que les adresses de groupe sont stockées sous une balise <GroupAddress> dans le XML
                var allGroupAddresses = doc.Descendants(_globalKnxNamespace + "GroupAddress").ToList();

                var addresses = new List<string>();

                foreach (var groupAddress in allGroupAddresses)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("--------------------------------------------------------------------");
                    msg.AppendLine($"Name: {groupAddress.Attribute("Name")?.Value}");
                    msg.AppendLine($"Adresse: {groupAddress.Attribute("Address")?.Value}");

                    // Ajouter les adresses au message
                    addresses.Add(msg.ToString());

                    // Mettre à jour le contrôle UI avec les adresses
                    if (GroupAddressListControl != null)
                    {
                        GroupAddressListControl.UpdateGroupAddresses(addresses);
                    }
                    else
                    {
                        MessageBox.Show("Le contrôle de la liste des adresses de groupe n'est pas initialisé correctement.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }


            }


            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier XML : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }


    }

        
}



