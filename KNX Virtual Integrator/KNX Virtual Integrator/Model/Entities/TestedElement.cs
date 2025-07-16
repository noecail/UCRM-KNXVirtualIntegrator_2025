using iText.Commons.Bouncycastle.Crypto;
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
       
        public List<DataPointType> Tests { get; } // List of pairs : command to send to the bus and expected feedback to be read on the bus


        //Constructors
        public TestedElement()
        {
            Tests = []; 
        } 
        
        
        public TestedElement(int typeCmd, string addressCmd, List<GroupValue?> valueCmd,int[] typeIe, string[]  addressesIe, List<GroupValue?>[] valueIe)
        {
            Tests = [new DataPointType(typeCmd, addressCmd, valueCmd)];
            for (var i = 0; i < typeIe.Length; i++)
            {
                Tests.Add(new DataPointType(typeIe[i],addressesIe[i],valueIe[i]));
            }
        } 
        
        
        
        public TestedElement(int typeCmd, string addressCmd, List<GroupValue?> valueCmd)
        {
            Tests = [new DataPointType(typeCmd, addressCmd, valueCmd)];
        } 
        
        public TestedElement(TestedElement element)
        {
            Tests = [];
            for (var i = 0; i < element.Tests.Count; i++)
            {
                Tests.Add(element.Tests[i]);
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
            for (var i = 0; i < Tests.Count; i++)
            {
                result = result && Tests[i].IsPossible();
                if (i != 0)
                    result = result && Tests[i].CompareValuesLength(Tests[i - 1]);
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
            var result = Tests.Count == other.Tests.Count;
            for (var i = 0; i < Tests.Count; i++)
            {
                result = result && Tests[i].IsEqual(other.Tests[i]);
            }
            return  result;
        }
        
        

        /// <summary>
        /// This method adds a test pair (value to send, value(s) to read) to the list of tests
        /// </summary>
        public void AddTest()
        {
            for (var i = 0; i < Tests.Count; i++)
            {
                Tests[i].AddValue(new GroupValue(false));
            }
        }
        
        /// <summary>
        /// This method removes a test pair (value to send, value to read) from the list of tests
        /// </summary>
        public void RemoveTest(int index)
        {
            foreach(var dpt in Tests) 
            {
                dpt.RemoveValue(index);
            }
        }
        
        /// <summary>
        /// This method modifies a test pair (value to send, value to read) in the list of tests
        /// </summary>
        public void ModifyTest(DataPointType pair, int index) // Effets de bords ? à tester
        {
            for (var i = 0; i < Tests.Count; i++)
                Tests[i].Value[index] = pair.Value[i];
        }

        public void AddDpt(int type, string address, List<GroupValue?> value)
        {
            Tests.Add(new DataPointType(type, address, value));
        }

        public void RemoveDpt(int index)
        {
            Tests.RemoveAt(index);
        }

    }
}