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
        public ObservableCollection<DataPointType> TestsCmd { get; set; } // List of commands to send to the bus 

        public ObservableCollection<DataPointType> TestsIe { get; set; } // List of expected feedback to be read on the bus


        public string Name = "";
        

        //Constructors
        public TestedElement()
        {
            TestsCmd = []; 
            TestsIe = []; 
            UpdateRemoveTestButtonVisibility();
        } 
        
        
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

        public int CmdContains(int type)
        {
            var count = 0;
            foreach (var myDpt in TestsCmd)
            {
                if (myDpt.Type == type)
                    count++;
            }
            return count;
        }
        
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
        /// <param name="dpt"> dpt to find in the list</param>
        /// <returns>THe index of the dpt in the list of dptIe</returns>
        public int IeContains(DataPointType dpt)
        {
            var count = 0;
            foreach (var myDpt in TestsIe)
            {
                if (myDpt.Type == dpt.Type)
                    count++;
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
        
        public void AddDptToCmd(int type, string address, List<GroupValue?> value)
        {
            if (TestsCmd == null)
                TestsCmd = new ObservableCollection<DataPointType>();
            TestsCmd.Add(new DataPointType(type, address, value));
        }
        
        public void AddDptToCmd(int type, string address, string name, List<GroupValue?> value)
        {
            if (TestsIe == null)
                TestsIe = new ObservableCollection<DataPointType>();
            TestsCmd.Add(new DataPointType(type, address, value, name));
        }
        
        public void AddDptToCmd(DataPointType dpt)
        {
            if (TestsCmd == null)
                TestsCmd = new ObservableCollection<DataPointType>();
            TestsCmd.Add(dpt);
        }
        
        public void AddDptToIe(int type, string address, List<GroupValue?> value)
        {
            if (TestsIe == null)
                TestsIe = new ObservableCollection<DataPointType>();
            TestsIe.Add(new DataPointType(type, address, value));
            UpdateRemoveTestButtonVisibility();
        }
        
        public void AddDptToIe(int type, string address, string name, List<GroupValue?> value)
        {
            TestsIe.Add(new DataPointType(type, address, value, name));
        }
        
        public void AddDptToIe(DataPointType dpt)
        {
            TestsIe.Add(dpt);
        }

        public void RemoveDptFromCmd(int index)
        {
            TestsCmd.RemoveAt(index);
        }
        
        public void RemoveDptFromIe(int index)
        {
            TestsIe.RemoveAt(index);
            UpdateRemoveTestButtonVisibility();
        }
        
        /// <summary>
        /// Updates the BigInteger array of all the DPTs
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
        /// Updates the GroupValue array of all DPTs
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
        
        private void UpdateRemoveTestButtonVisibility()
        {
            foreach (var test in TestsIe)
            {
                var vis = test != TestsIe.Last() ? Visibility.Collapsed : Visibility.Visible;
                test.UpdateRemoveTestButtonVisibility(vis);
            }
        }
        
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