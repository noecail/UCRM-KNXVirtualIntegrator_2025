using System.Collections.Generic;
using System.Collections.Generic;
using System.Windows.Controls;

namespace KNX_PROJET_2
{
    public partial class GroupAddressList : UserControl
    {
        public GroupAddressList()
        {
            InitializeComponent();
        }

        // Méthode pour mettre à jour la liste des adresses
        public void UpdateGroupAddresses(List<string> addresses)
        {
            AddressesListBox.Items.Clear();
            foreach (var address in addresses)
            {
                AddressesListBox.Items.Add(address);
            }
        }

        //Méthode pour faire une check list
      /*  public class GroupAddressItem
        {
            public string DisplayText { get; set; }
            public bool IsSelected { get; set; }
        }*/

    }
}



