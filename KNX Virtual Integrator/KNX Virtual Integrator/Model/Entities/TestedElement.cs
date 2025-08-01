using System.Collections.ObjectModel;
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
        

        //Constructors
        public TestedElement()
        {
            TestsCmd = []; 
            TestsIe = []; 
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
        } 
        
        
        
        public TestedElement(int[] typeCmd, string[] addressCmd, List<GroupValue?>[] valueCmd)
        {
            TestsCmd = []; 
            TestsIe = [];
            for (var i = 0; i < typeCmd.Length; i++)
            {
                TestsCmd.Add(new DataPointType(typeCmd[i], addressCmd[i], valueCmd[i]));
            }
        } 
        
        public TestedElement(int[] typeCmd, string[] addressCmd, List<GroupValue?>[] valueCmd, string name)
        {
            TestsCmd = []; 
            TestsIe = [];
            for (var i = 0; i < typeCmd.Length; i++)
            {
                TestsCmd.Add(new DataPointType(typeCmd[i], addressCmd[i], valueCmd[i], name));
            }
        } 
        
        public TestedElement(TestedElement element)
        {
            TestsCmd = []; 
            TestsIe = [];
            for (var i = 0; i < element.TestsCmd.Count; i++)
            {
                TestsCmd.Add(element.TestsCmd[i]);
            }
            for (var i = 0; i < element.TestsIe.Count; i++)
            {
                TestsIe.Add(element.TestsIe[i]);
            }
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
            var result = TestsCmd.Count == other.TestsCmd.Count;
            for (var i = 0; i < TestsCmd.Count; i++)
            {
                result = result && TestsCmd[i].IsEqual(other.TestsCmd[i]);
            } 
            /* result = result && TestsIe.Count == other.TestsIe.Count;
            for (var i = 0; i < TestsIe.Count; i++)
            {
                result = result && TestsIe[i].IsEqual(other.TestsIe[i]);
            }*/
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
        
        public void AddDptToCmd(int type, string address, List<GroupValue?> value)
        {
            TestsCmd.Add(new DataPointType(type, address, value));
        }
        
        public void AddDptToIe(int type, string address, List<GroupValue?> value)
        {
            TestsIe.Add(new DataPointType(type, address, value));
        }

        public void RemoveDptFromCmd(int index)
        {
            TestsCmd.RemoveAt(index);
        }
        
        public void RemoveDptFromIe(int index)
        {
            TestsIe.RemoveAt(index);
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

    }
}