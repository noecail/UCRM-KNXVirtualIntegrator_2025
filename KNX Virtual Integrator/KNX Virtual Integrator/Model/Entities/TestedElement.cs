using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Entities
{
    /// <summary>
    /// Represents a Data Point Type.
    ///
    /// This class provides properties for storing and retrieving the key and name of the model.
    /// It includes functionality for displaying the model in a formatted string and implements 
    /// equality comparison based on the DPT value, name, and key. The class is designed to be 
    /// comparable with other instances of the same type to determine equality.
    ///
    /// - Key: Unique identifier for the functional model.
    /// - Name: Descriptive name of the functional model.
    /// 
    /// The class overrides the ToString, Equals, and GetHashCode methods to provide custom
    /// string representation, equality checks, and hash code generation.
    /// </summary>
    public class TestedElement
    {
        private int _type;
        public int Type // DPT code
        {
            get { return _type; }
            set
            {
                _type = value;
                GetSizeOf();
            }
        }

        private int _size; // Size of the DPT
        public List<GroupValue?[]> Tests; // List of pairs : value to send to the bus and expected value to be read on the bus

        //Constructors
        public TestedElement(int type)
        {
            Type = type;
            Tests = [[new GroupValue(true),new GroupValue(true)]]; 
        } 
        public TestedElement(TestedElement element)
        {
            Type = element.Type;
            Tests = [];
            GroupValue[]? pair = [new GroupValue(false),new GroupValue(false)];
            foreach (var test in element.Tests)
            {
                Tests.Add([test[0],test[1]]);
            }
        }
        
        public TestedElement(int type, List<GroupValue?[]> toTest)
        {
            Type = type;
            Tests = [];
            foreach (var test in toTest)
            {
                Tests.Add([test[0],test[1]]);
            }
            
        }


        /// <summary>
        /// This method checks whether the selected values to send and to read can fit in the selected size
        /// <returns>Returns a boolean acknowledging whether the test is possible or not</returns>
        /// </summary>
        public bool IsPossible()
        {
            var max = Convert.ToUInt64(1 << _size);
            var result = true;
            for (var i = 0; i < Tests.Count; i++)
            {
                result = result && Convert.ToUInt64(Tests[i][0]) < max && Convert.ToUInt64(Tests[i][1]) < max;
            }
            return  result;
        }

        /// <summary>
        /// This method adds a test pair (value to send, value to read) to the list of tests
        /// </summary>
        public void AddTest(GroupValue[] test)
        {
            Tests.Add([test[0],test[1]]);
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
        public void ModifyTest(GroupValue[] test, int index)
        {
            Tests.RemoveAt(index);
            Tests.Insert(index,[test[0],test[1]]);
        }

        /// <summary>
        /// This method computes the size of a DPT from its code. Only the most used types are implemented(under 222). Check
        /// https://support.knx.org/hc/fr/article_attachments/15392631105682 to add here the new formats if not yet implemented.
        /// </summary>
        private void GetSizeOf()
        {
            switch (Type)
            {
                case 1:
                    _size = 1;
                    break;
                case 2:
                case 23:
                case 24:
                case 28:
                    _size = 2;
                    break;
                case 31:
                    _size = 3;
                    break;
                case 3:
                    _size = 4;
                    break;
                case 4:
                case 5:
                case 6: 
                case 17:
                case 18:
                case 20:
                case 21:
                case 25:
                case 26:
                    _size = 8;
                    break;
                case 200:
                    _size = 9;
                    break;
                case 7:
                case 8:
                case 9:
                case 22:
                case 201:
                case 202:
                case 204:
                case 207:
                case 211:
                case 217:
                    _size = 16;
                    break;
                case 10:
                case 11:
                case 30:
                case 203:
                case 205:
                case 206:
                case 209:
                    _size = 24;
                    break;
                case 215:
                case 216:
                case 218:
                    _size = 40;
                    break;
                case 212:
                case 221:
                case 222:
                    _size = 48;
                    break;
                case 19:
                case 29:
                case 213:
                case 219:
                    _size = 64;
                    break;
                case 16:
                    _size = 112;
                    break;
                default:
                    _size = 32;
                    break;
            }
        }

        public bool IsEqual(TestedElement dpt)
        {
            return dpt.Type == Type;
                
        }
    }
}