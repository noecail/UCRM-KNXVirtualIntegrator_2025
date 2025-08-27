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
        /// The keys of the Cmd DPTs in the Element.
        /// </summary>
        public ObservableCollection<IntItem> Cmd { get; set; }
        /// <summary>
        /// The keys of the Ie DPTs in the Element.
        /// </summary>
        public ObservableCollection<IntItem> Ie { get; set; }
        
        /// <summary>
        /// All the values predefined in the structure, to be sent
        /// </summary>
        public ObservableCollection<ObservableCollection<BigIntegerItem>> CmdValues { get; }
        /// <summary>
        /// All the values predefined in the structure, to be read
        /// </summary>
        public ObservableCollection<ObservableCollection<BigIntegerItem>> IeValues { get; }

        /// <summary>
        /// Adds a new Cmd Dpt.
        /// </summary>
        /// <param name="value">The key of the DPT at which the Cmd is initialized.</param>
        public void AddToCmd(int value)
        {
            Cmd.Add(new IntItem(value));
            UpdateRemoveDptButtonVisibility();
            CmdValues.Add(new ObservableCollection<BigIntegerItem>());
            foreach (var unused in CmdValues[0])
                CmdValues[^1].Add(new BigIntegerItem(0));
        }

        /// <summary>
        /// Adds a new Ie Dpt.
        /// </summary>
        /// <param name="value">The key of the DPT at which the Cmd is initialized.</param>
        public void AddToIe(int value)
        {
            Ie.Add(new IntItem(value));
            IeValues.Add([]);
            foreach (var unused in CmdValues[0])
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
        /// Removes an Ie Dpt at an index.
        /// </summary>
        /// <param name="ieIndex">The index of the DPT to remove.</param>
        public void RemoveIeAt(int ieIndex)
        {
            Ie.RemoveAt(ieIndex);
            IeValues.RemoveAt(ieIndex);
            UpdateRemoveTestButtonVisibility();
        }
        
        /// <summary>
        /// Adds a line of values to an element structure, both to be sent and to be read
        /// </summary>
        public void AddTest()
        {
            foreach (var dptValues in CmdValues)
                dptValues.Add(new BigIntegerItem(new BigInteger(0)));
            foreach (var dptValues in IeValues)
                dptValues.Add(new BigIntegerItem(new BigInteger(0)));
               
            UpdateRemoveTestButtonVisibility();
        }
        
        /// <summary>
        /// Removes a line of values from an element structure, both to be sent and to be read
        /// One parameter : the index of the test line
        /// </summary>
        /// <param name="indexTest"></param>
        public void RemoveTestAt(int indexTest)
        {
            foreach (var dptValues in CmdValues)
                dptValues.RemoveAt(indexTest);
            foreach (var dptValues in IeValues)
                dptValues.RemoveAt(indexTest);
   
            UpdateRemoveTestButtonVisibility();
        }
        
        /// <summary>
        /// Constructor
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
        /// Constructor
        /// Creates a filled ElementStructure with <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="cmdCollection">The collection of Cmd DPT to copy.</param>
        /// <param name="ieCollection">The collection of Ie DPT to copy.</param>
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