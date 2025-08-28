using System.Collections.ObjectModel;
using System.Windows;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Entities
{
    /// <summary>
    /// Represents an element to test.
    /// They are composed of DataPointTypes.
    /// The first DataPointType of each array is the command and the next ones are the Feedbacks.
    /// </summary>
    public class TestedElement
    {
        /// <summary>
        /// The list of commands to send to the bus
        /// </summary>
        public ObservableCollection<DataPointType> TestsCmd { get; set; }
        /// <summary>
        /// The list of expected feedback to be read on the bus
        /// </summary>
        public ObservableCollection<DataPointType> TestsIe { get; set; } 
        /// <summary>
        /// Name of the Element (not used)
        /// </summary>
        public string Name = "";
        
        //Constructors
        /// <summary>
        /// Barebone constructor
        /// </summary>
        public TestedElement()
        {
            TestsCmd = []; 
            TestsIe = []; 
            UpdateRemoveTestButtonVisibility();
        } 
        /// <summary>
        /// Full constructor of the Element
        /// </summary>
        /// <param name="typeCmd">the types of CMD DPTs</param>
        /// <param name="addressCmd">the addresses bound to CMD DPTs</param>
        /// <param name="valueCmd">the values of the CMD DPTs</param>
        /// <param name="typeIe">the types of IE DPTs</param>
        /// <param name="addressesIe">the addresses bound to IE DPTs</param>
        /// <param name="valueIe">the values of the IE DPTs</param>
        public TestedElement(int[] typeCmd, string[] addressCmd, List<GroupValue?>[] valueCmd,int[] typeIe, string[]  addressesIe, List<GroupValue?>[] valueIe)
        {
            TestsCmd = []; 
            TestsIe = [];
            for (var i = 0; i < typeCmd.Length; i++)
            {
                TestsCmd.Add(new DataPointType(typeCmd[i], addressCmd[i], valueCmd[i]));
            }
            for (var i = 0; i < typeIe.Length; i++)
            {
                TestsIe.Add(new DataPointType(typeIe[i],addressesIe[i],valueIe[i]));
            }
            UpdateRemoveTestButtonVisibility();
        } 
        
        /// <summary>
        /// Command-only constructor
        /// </summary>
        /// <param name="typeCmd">the types of CMD DPTs</param>
        /// <param name="addressCmd">the addresses bound to CMD DPTs</param>
        /// <param name="valueCmd">the values of the CMD DPTs</param>
        public TestedElement(int[] typeCmd, string[] addressCmd, List<GroupValue?>[] valueCmd)
        {
            TestsCmd = []; 
            TestsIe = [];
            for (var i = 0; i < typeCmd.Length; i++)
            {
                TestsCmd.Add(new DataPointType(typeCmd[i], addressCmd[i], valueCmd[i]));
            }
            UpdateRemoveTestButtonVisibility();
        } 
        
        /// <summary>
        /// Command-only constructor with names included
        /// </summary>
        /// <param name="typeCmd">the types of CMD DPTs</param>
        /// <param name="addressCmd">the addresses bound to CMD DPTs</param>
        /// <param name="valueCmd">the values of the CMD DPTs</param>
        /// <param name="dptNames">name of the DPTs</param>
        /// <param name="circuitName">Name of the element</param>
        public TestedElement(int[] typeCmd, string[] addressCmd, List<GroupValue?>[] valueCmd, string[] dptNames,string circuitName)
        {
            TestsCmd = []; 
            TestsIe = [];
            Name = circuitName;
            for (var i = 0; i < typeCmd.Length; i++)
            {
                TestsCmd.Add(new DataPointType(typeCmd[i], addressCmd[i], valueCmd[i], dptNames[i]));
            }
            UpdateRemoveTestButtonVisibility();
        } 
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The copied Element</param>
        public TestedElement(TestedElement other)
        {
            TestsCmd = new ObservableCollection<DataPointType>();
            foreach (var dpt in other.TestsCmd)
                TestsCmd.Add(new DataPointType(dpt));

            TestsIe = new ObservableCollection<DataPointType>();
            foreach (var dpt in other.TestsIe)
                TestsIe.Add(new DataPointType(dpt));
            UpdateRemoveTestButtonVisibility();

        }

        /// <summary>
        /// This method checks if all the selected values to send and to read can fit in the selected sizes
        /// and if all the DPTs have the same number of values
        /// <returns>Returns a boolean acknowledging whether the test is possible or not</returns>
        /// </summary>
        public bool IsPossible()
        {
            var result = true;
            for (var i = 0; i < TestsCmd.Count; i++)
            {
                result = result && TestsCmd[i].IsPossible();
                if (i != 0)
                    result = result && TestsCmd[i].CompareValuesLength(TestsCmd[i - 1]);
            }
            for (var i = 0; i < TestsIe.Count; i++)
            {
                result = result && TestsIe[i].IsPossible();
                if (i != 0)
                    result = result && TestsIe[i].CompareValuesLength(TestsIe[i - 1]);
            }
            return  result;
        }
        
        /// <summary>
        /// This method checks if two elements have the same number of DPTs and if they are of the same type.
        /// <returns>Returns a boolean acknowledging whether the Elements are equal or not</returns>
        /// </summary>
        public bool IsEqual(TestedElement? other)
        {
            if (other == null)
                return false;
            HashSet<int> bannedIndexes = [];
            var result = TestsCmd.Count == other.TestsCmd.Count;
            if (result == false) return false;
            for (var i = 0; i < TestsCmd.Count; i++)
            {
                var res = Contains(other.TestsCmd[i], bannedIndexes);
                if (res == -1)
                    return false;
                bannedIndexes.Add(res);
            } 
            return result;
        }
        /// <summary>
        /// Checks whether the TestsCmd contains the DPT
        /// </summary>
        /// <param name="other">The DPT to look out for</param>
        /// <param name="bannedIndexes">The indexes where searching is not allowed.</param>
        /// <returns>the index if it is found; -1 otherwise.</returns>
        public int Contains(DataPointType other, HashSet<int> bannedIndexes)
        {
            var result = -1;
            for (var i = 0; i < TestsCmd.Count; i++)
            {
                if (TestsCmd[i].IsEqual(other) && !bannedIndexes.Contains(i))
                {
                    return i;
                }
            }
            return result;
        }
        
        /// <summary>
        /// This method adds a test pair (value to send, value(s) to read) to the list of tests
        /// </summary>
        public void AddTest()
        {
            foreach (var dpt in TestsCmd)
            {
                dpt.AddValue(new GroupValue(false));
            }
            foreach (var dpt in TestsIe)
            {
                dpt.AddValue(new GroupValue(false));
            }   
            UpdateRemoveTestButtonVisibility();
        }
        
        /// <summary>
        /// Checks whether there is a similar Element in the checked model
        /// </summary>
        /// <param name="model">The model to check</param>
        /// <returns>The index of the element; -1 otherwise.</returns>
        public int FindELementInModel(FunctionalModel model)
        {
            for(var i = 0; i<model.ElementList.Count;i++)
            {
                var element = model.ElementList[i];
                if (IsEqual(element))
                {
                    return i;
                }
            }

            return -1;
        }
        
        /// <summary>
        /// This method adds a test pair (value to send, value(s) to read) to the list of tests
        /// </summary>
        public void CopyTest(TestedElement? other, int index)
        {
            if (other == null || other.TestsCmd.Count != TestsCmd.Count || TestsIe.Count != other.TestsIe.Count)
            {
                return;
            }

            for (var i = 0; i< other.TestsCmd.Count;i++)
            {
                TestsCmd[i].AddValue(other.TestsCmd[i].Value[index]);
            }
            for (var i = 0; i< other.TestsIe.Count;i++)
            {
                TestsIe[i].AddValue(other.TestsIe[i].Value[index]);
            }
        }
        
        /// <summary>
        /// This method removes a test pair (value to send, value to read) from the list of tests
        /// </summary>
        public void RemoveTest(int index)
        {
            foreach(var dpt in TestsCmd) 
            {
                dpt.RemoveValue(index);
            }
            foreach(var dpt in TestsIe) 
            {
                dpt.RemoveValue(index);
            }
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Counts the number of DPTs in the commands that have a certain name.
        /// </summary>
        /// <param name="name">The name to look out for.</param>
        /// <returns>The number of DPTs with that name</returns>
        public int CmdContains(string name)
        {
            var count = 0;
            foreach (var myDpt in TestsCmd)
            {
                if (myDpt.Name//.Replace('_',' ')
                    .Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// Searches for a dpt of the same type of the argument, in the list of dptIe
        /// </summary>
        /// <param name="prefix"> prefix
        /// to find in the list</param>
        /// <returns>THe index of the dpt in the list of dptIe</returns>
        public int IeContains(string prefix)
        {
            var count = 0;
            foreach (var myDpt in TestsIe)
            {
                if (myDpt.Name.Contains(prefix,StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// Adds a DPT to <see cref="TestsCmd"/>
        /// </summary>
        /// <param name="type">type of the new dpt</param>
        /// <param name="address">address bound to the DPT</param>
        /// <param name="value">all the values of the DPT</param>
        public void AddDptToCmd(int type, string address, List<GroupValue?> value)
        {
            if (TestsCmd == null)
                TestsCmd = new ObservableCollection<DataPointType>();
            TestsCmd.Add(new DataPointType(type, address, value));
        }
        
        /// <summary>
        /// Adds a DPT to <see cref="TestsCmd"/>
        /// </summary>
        /// <param name="type">type of the DPT</param>
        /// <param name="name">name of the DPT</param>
        /// <param name="address">address bound to the DPT</param>
        /// <param name="value">all the values of the DPT</param>
        public void AddDptToCmd(int type, string address, string name, List<GroupValue?> value)
        {
            if (TestsIe == null)
                TestsIe = new ObservableCollection<DataPointType>();
            TestsCmd.Add(new DataPointType(type, address, value, name));
        }
        
        /// <summary>
        /// Adds a DPT to <see cref="TestsIe"/>
        /// </summary>
        /// <param name="dpt">the dpt to add</param>
        public void AddDptToCmd(DataPointType dpt)
        {
            if (TestsCmd == null)
                TestsCmd = new ObservableCollection<DataPointType>();
            TestsCmd.Add(dpt);
        }
        /// <summary>
        /// Adds a DPT to <see cref="TestsIe"/>
        /// </summary>
        /// <param name="type">type of the new dpt</param>
        /// <param name="address">address bound to the DPT</param>
        /// <param name="value">all the values of the DPT</param>
        public void AddDptToIe(int type, string address, List<GroupValue?> value)
        {
            if (TestsIe == null)
                TestsIe = new ObservableCollection<DataPointType>();
            TestsIe.Add(new DataPointType(type, address, value));
            UpdateRemoveTestButtonVisibility();
        }
        /// <summary>
        /// Adds a new DPT to <see cref="TestsIe"/> of the element.
        /// </summary>
        /// <param name="dpt">The DPT to copy from</param>
        public void AddDptToIe(DataPointType dpt)
        {
            TestsIe.Add(dpt);
        }
        
        /// <summary>
        /// Updates the BigInteger array of all the DPTs. <seealso cref="Model.Entities.DataPointType.UpdateIntValue"/>
        /// </summary>
        public void UpdateIntValue()
        {
            foreach (var test in TestsCmd)
            {
                test.UpdateIntValue();
            }
            foreach (var test in TestsIe)
            {
                test.UpdateIntValue();
            }
        }

        /// <summary>
        /// Updates the GroupValue array of all DPTs. <seealso cref="Model.Entities.DataPointType.UpdateValue"/>
        /// </summary>
        public void UpdateValue()
        {
            foreach (var test in TestsCmd)
            {
                test.UpdateValue();
            }
            foreach (var test in TestsIe)
            {
                test.UpdateValue();
            }
        }
        /// <summary>
        /// Updates the visibility of all the <see cref="TestsIe"/> Remove Button : the last one doesn't have it.
        /// </summary>
        private void UpdateRemoveTestButtonVisibility()
        {
            foreach (var test in TestsIe)
            {
                var vis = test != TestsIe.Last() ? Visibility.Collapsed : Visibility.Visible;
                test.UpdateRemoveTestButtonVisibility(vis);
            }
        }
        
        /// <summary>
        /// Searches for the type of the dpt with the given name.
        /// </summary>
        /// <param name="dptName">The name of said DPT</param>
        /// <returns>the type of the DPT</returns>
        public int GetDptType(string dptName)
        {
            var res = 0;
            foreach (var myDpt in TestsCmd)
            {
                if (myDpt.Name//.Replace('_',' ')
                    .Contains(dptName, StringComparison.OrdinalIgnoreCase))
                {
                    res = myDpt.Type;
                    break;
                }
            }
            return res;
        }
    }
}