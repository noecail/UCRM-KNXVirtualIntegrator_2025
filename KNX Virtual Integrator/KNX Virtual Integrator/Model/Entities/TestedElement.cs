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
       
        public List<List<DataPointType>> Tests; // List of pairs : command to send to the bus and expected feedback to be read on the bus


        //Constructors
        public TestedElement()
        {
            Tests = [[new DataPointType(1),new DataPointType(1)]]; 
        } 
        
        public TestedElement(int typeCmd, List<string> addressesCmd, GroupValue? valueCmd,int[] typeIe, List<string>[]  addressesIe, GroupValue?[] valueIe)
        {
            Tests = [[new DataPointType(typeCmd, addressesCmd, valueCmd)]];
            for (var i = 0; i < typeIe.Length; i++)
            {
                Tests[^1].Add(new DataPointType(typeIe[i],addressesIe[i],valueIe[i]));
            }
        } 
        public TestedElement(TestedElement element)
        {
            Tests = [];
            for (var i = 0; i < element.Tests.Count; i++)
            {
                Tests.Add([]);

                for (var j = 0; j < element.Tests[i].Count; j++)
                {
                    Tests[i].Add(element.Tests[i][j]);
                }
            }
        }

        /// <summary>
        /// This method checks if all the selected values to send and to read can fit in the selected sizes
        /// and if all the DPTs of each test have the same number of addresses
        /// <returns>Returns a boolean acknowledging whether the test is possible or not</returns>
        /// </summary>
        public bool IsPossible()
        {
            var result = true;
            foreach (var test in Tests)
            {
                for (var i = 0; i < test.Count; i++)
                {
                    if (test[i] != null)
                    {
                        result = result && test[i].IsPossible();
                        if (i != 0)
                            result = result && test[i].CompareAddressLength(test[i - 1]);
                    }
                }
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
                for (var j = 0; j < Tests[i].Count; j++)
                {
                    result = result && Tests[i][j].IsEqual(other.Tests[i][j]);
                }
            }
            return  result;
        }
        
        

        /// <summary>
        /// This method adds a test pair (value to send, value(s) to read) to the list of tests
        /// </summary>
        public void AddTest(List<DataPointType> pair)
        {
            Tests.Add(new List<DataPointType>(pair));
        }
        
        /// <summary>
        /// This method removes a test pair (value to send, value to read) from the list of tests
        /// </summary>
        public void RemoveTest(int index)
        {
            Tests.RemoveAt(index);
        }
        
        /// <summary>
        /// This method modifies a test pair (value to send, value to read) in the list of tests
        /// </summary>
        public void ModifyTest(List<DataPointType> pair, int index) // Effets de bords ? à tester
        {
            Tests[index] = new List<DataPointType>(pair);
        }
        
    }
}