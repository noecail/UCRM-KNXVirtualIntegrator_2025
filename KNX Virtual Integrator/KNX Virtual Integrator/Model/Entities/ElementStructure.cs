using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows;

namespace KNX_Virtual_Integrator.Model.Entities;

/// <summary>
    /// Barebone structure of TestedElements. It holds the number of CMD, IE and the DPT associated with it.
    /// </summary>
    public class ElementStructure
    {
        /// <summary>
        /// The number of Cmd DPTs in the Element.
        /// </summary>
        public ObservableCollection<IntItem> Cmd { get; set; }
        /// <summary>
        /// The number of Ie DPTs in the Element.
        /// </summary>
        public ObservableCollection<IntItem> Ie { get; set; }
        
        // CmdValues [0][1] -> 1er DPT d'envoi (=1ere colonne), 2eme value (=2eme ligne)
        public ObservableCollection<ObservableCollection<BigIntegerItem>> CmdValues { get; set; }
        public ObservableCollection<ObservableCollection<BigIntegerItem>> IeValues { get; set; }

        /// <summary>
        /// Adds a new Cmd Dpt.
        /// </summary>
        /// <param name="value">The key of the DPT at which the Cmd is initialized.</param>
        public void AddToCmd(int value)
        {
            Cmd.Add(new IntItem(value));
            UpdateRemoveDptButtonVisibility();
            CmdValues.Add(new ObservableCollection<BigIntegerItem>());
            foreach (var test in CmdValues[0])
                CmdValues[^1].Add(new BigIntegerItem(0));
        }

        /// <summary>
        /// Adds a new Ie Dpt.
        /// </summary>
        /// <param name="value">The key of the DPT at which the Cmd is initialized.</param>
        public void AddToIe(int value)
        {
            Ie.Add(new IntItem(value));
            IeValues.Add(new ObservableCollection<BigIntegerItem>());
            foreach (var test in CmdValues[0])
                IeValues[^1].Add(new BigIntegerItem(0));
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Removes a Cmd Dpt at an index.
        /// </summary>
        /// <param name="cmdIndex">The index of the DPT to remove.</param>
        public void RemoveCmdAt(int cmdIndex)
        {
            Cmd.RemoveAt(cmdIndex);
            UpdateRemoveDptButtonVisibility();
            CmdValues.RemoveAt(cmdIndex);
        }

        /// <summary>
        /// Removes a Ie Dpt at an index.
        /// </summary>
        /// <param name="ieIndex">The index of the DPT to remove.</param>
        public void RemoveIeAt(int ieIndex)
        {
            Ie.RemoveAt(ieIndex);
            IeValues.RemoveAt(ieIndex);
            UpdateRemoveTestButtonVisibility();
        }
        
        public void AddValueToCmd(int cmdIndex)
        {
            CmdValues[cmdIndex].Add(new BigIntegerItem(new BigInteger(0)));
        }
        
        public void AddValueToIe(int ieIndex)
        {
            IeValues[ieIndex].Add(new BigIntegerItem(new BigInteger(0)));
            UpdateRemoveTestButtonVisibility();
        }

        public void RemoveCmdValueAt(int cmdIndex, int valueIndex)
        {
            CmdValues[cmdIndex].RemoveAt(valueIndex);
        }

        public void RemoveIeValueAt(int ieIndex, int valueIndex)
        {
            IeValues[ieIndex].RemoveAt(valueIndex);
            UpdateRemoveTestButtonVisibility();
        }
        
        public void AddTest()
        {
            foreach (var dptValues in CmdValues)
                dptValues.Add(new BigIntegerItem(new BigInteger(0)));
            foreach (var dptValues in IeValues)
                dptValues.Add(new BigIntegerItem(new BigInteger(0)));
               
            UpdateRemoveTestButtonVisibility();
        }

        public void RemoveTestAt(int indexTest)
        {
            foreach (var dptValues in CmdValues)
                dptValues.RemoveAt(indexTest);
            foreach (var dptValues in IeValues)
                dptValues.RemoveAt(indexTest);
   
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Creates an empty ElementStructure.
        /// </summary>
        public ElementStructure()
        {
            Cmd = [];
            Ie = [];
            CmdValues = [];
            IeValues = [];
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Creates a filled ElementStructure with <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="cmdCollection">The collection of Cmd DPT to copy.</param>
        /// <param name="ieCollection">The collectio of Ie DPT to copy.</param>
        public ElementStructure(ObservableCollection<IntItem> cmdCollection, ObservableCollection<IntItem> ieCollection)
        {
            Cmd = cmdCollection;
            Ie = ieCollection;
            CmdValues = [];
            IeValues = [];
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Creates a filled ElementStructure with lists.
        /// </summary>
        /// <param name="cmdCollection">The list of Cmd DPT to copy.</param>
        /// <param name="ieCollection">The list of Ie DPT to copy.</param>
        public ElementStructure(List<int> cmdCollection, List<int> ieCollection)
        {
            Cmd = [];
            Ie = [];
            CmdValues = [];
            IeValues = [];
            foreach (var cmdInt in cmdCollection)
                AddToCmd(cmdInt);
            foreach (var ieInt in ieCollection)
                AddToIe(ieInt);
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Copies an ElementStructure.
        /// </summary>
        /// <param name="otherStructure">The structure to copy.</param>
        public ElementStructure(ElementStructure otherStructure)
        {
            Cmd = new ObservableCollection<IntItem>();
            Ie = new ObservableCollection<IntItem>();
            CmdValues = [];
            IeValues = [];
            foreach(var cmd in otherStructure.Cmd)
                Cmd.Add(new IntItem(cmd));
            foreach (var ie in otherStructure.Ie)
                Ie.Add(new IntItem(ie));
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Hides the Cmd remove button if there is only one Cmd DPT.
        /// Shows the button if there is more.
        /// </summary>
        private void UpdateRemoveDptButtonVisibility()
        {
            var vis = Cmd.Count > 1 ? Visibility.Visible : Visibility.Hidden;
            foreach (var intItem in Cmd)
                intItem.RemoveDptButtonVisibility = vis;
        }
        
        private void UpdateRemoveTestButtonVisibility()
        {
            foreach (var ie in IeValues)
            {
                var vis = ie != IeValues.Last() ? Visibility.Collapsed : Visibility.Visible;
                foreach (var bigIntegerItem in ie)
                    bigIntegerItem.RemoveTestButtonVisibility = vis;
            }
        }
    }