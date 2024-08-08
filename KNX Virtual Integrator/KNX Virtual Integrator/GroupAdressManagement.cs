using System.Xml.Linq;

namespace KNX_Virtual_Integrator;

public class GroupAddressManagement
{
    private static XNamespace _globalKnxNamespace = "http://knx.org/xml/ga-export/01";

    public static void ExtractGroupAddress()
    {
        if (App.DisplayElements != null && App.DisplayElements.MainWindow.UserChooseToImportGroupAddressFile)
        {
            XDocument? groupAddressFile = App.Fm?.LoadXmlDocument(App.Fm.GroupAddressFilePath);
            var groupAddresses = groupAddressFile?.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
            
            if (groupAddresses != null)
            {
                var ieGroupAddresses = groupAddresses
                    .Where(ga => 
                    {
                        var name = (string?)ga.Attribute("Name");
                        return name != null && name.StartsWith("Ie",StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                var cmdGroupAddresses = groupAddresses
                    .Where(ga => 
                    {
                        var name = (string?)ga.Attribute("Name");
                        return name != null && name.StartsWith("Cmd",StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();
                
                // Dictionnaire pour regrouper les éléments par nom commun sans préfixe
                var groupedAddresses = new Dictionary<string, List<XElement>>();
                
                foreach (var ie in ieGroupAddresses)
                {
                    var name = (string)ie.Attribute("Name");
                    if (name != null)
                    {
                        var commonName = name.Substring(2); // Supprimer le préfixe "Ie"
                        if (!groupedAddresses.ContainsKey(commonName))
                        {
                            groupedAddresses[commonName] = new List<XElement>();
                        }
                        groupedAddresses[commonName].Add(ie);
                    }
                }

                foreach (var cmd in cmdGroupAddresses)
                {
                    var name = (string)cmd.Attribute("Name");
                    if (name != null)
                    {
                        var commonName = name.Substring(3); // Supprimer le préfixe "Cmd"
                        if (!groupedAddresses.ContainsKey(commonName))
                        {
                            groupedAddresses[commonName] = new List<XElement>();
                        }
                        groupedAddresses[commonName].Add(cmd);
                    }
                }

            }
        }
        else
        {
            
        }
    }
}