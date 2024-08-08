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
            }
        }
        else
        {
            
        }
    }
}