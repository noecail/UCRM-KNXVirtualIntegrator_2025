using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Knx.Falcon;
using Knx.Falcon.Configuration;
using Knx.Falcon.DataSecurity;
using Knx.Falcon.KnxnetIp;
using Knx.Falcon.Sdk;

namespace KNX_PROJET_2
{
    /// <summary>
    /// Logique d'interaction pour BusConnection.xaml
    /// </summary>
    public partial class BusConnection : UserControl
    {
       
        
            public BusConnection()
            {
                InitializeComponent();

            }

            /*public string ConnectionString
            //propriété qui définit la connexion = sert à établir les valeurs de la connexion avec les conversions qu'il faut pour comprendre en c# et en utilisateur
            {
                get => ConnectorParameters?.ToConnectionString();
                // convertir l'objet ConnectorParameters en une chaîne de connexion
                //autres parties application accede à la configuration de la connexion (chaîne de caractères facilement stockée, affichée ou transmise)
                set     //chaîne de connexion en entrée, la convertit en un objet ConnectorParameters, et met à jour cet objet
                {
                    if (value is null)
                        ConnectorParameters = null;
                    else
                    {
                        ConnectorParameters = ConnectorParameters.FromConnectionString(value);    //conversion string en ConnectorParameter

                        TypeSelection.SelectedItem = TypeSelection.Items.Cast<ConnectorParameters>()  //Synchronisation de l'interface utilisateur
                            .FirstOrDefault(i => i.GetType() == ConnectorParameters.GetType());
                    }
                }
            }

            //propriete qui fournit un tableau des differentes connexions possibles
            public ConnectorParameters[] ConnectorTypes => new ConnectorParameters[]
            {
            new UsbConnectorParameters(),
            new IpRoutingConnectorParameters(),
            new IpTunnelingConnectorParameters(),
            new IpDeviceManagementConnectorParameters(),
            new EiblibConnectorParameters(),
            };

            //faire de la mise a jour avec l'interface utilisateur
            public ConnectorParameters ConnectorParameters
            {
                get => (ConnectorParameters)GetValue(ConnectorParametersProperty);
                set => SetValue(ConnectorParametersProperty, value);
            }*/

            

    }
}
