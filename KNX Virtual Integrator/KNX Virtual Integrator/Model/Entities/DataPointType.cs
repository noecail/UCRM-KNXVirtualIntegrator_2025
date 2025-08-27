using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;
using Knx.Falcon;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace KNX_Virtual_Integrator.Model.Entities;
/// <summary>
/// Represents a DataPointType with : its type code, size, address and values expected to be sent or read
/// </summary>
public class DataPointType : INotifyPropertyChanged
{
    /// <summary>
    /// The name of the DataPoint.
    /// </summary>
    private string _name = "";
    /// <summary>
    /// Gets or sets the name of the DataPoint.
    /// </summary>
    public string Name {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Class used only for Value collections,
    /// used by the UI to access and modify BigInteger values,
    /// which do not raise notifications by default.
    /// It is a sort of wrapper
    /// </summary>
    public class BigIntegerItem : INotifyPropertyChanged
    {
        /// <summary>
        /// The visibility of the Value box in TestedElement.
        /// </summary>
        private Visibility? _removeTestButtonVisibility ;
        /// <summary>
        /// Gets or sets the visibility of the Value box in TestedElement.
        /// </summary>
        public Visibility? RemoveTestButtonVisibility
        {
            get => _removeTestButtonVisibility;
            set
            {
                if (_removeTestButtonVisibility == value) return;
                _removeTestButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// The wrapped BigInteger.
        /// </summary>
        private BigInteger? _bigIntegervalue;
        /// <summary>
        /// Gets or sets the wrapped BigInteger.
        /// </summary>
        public BigInteger? BigIntegerValue
        {
            get => _bigIntegervalue;
            set
            {
                if (_bigIntegervalue != value)
                {
                    _bigIntegervalue = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Boolean determining whether the box is enabled or not.
        /// </summary>
        private bool _isEnabled;
        /// <summary>
        /// Gets or sets the boolean determining whether the box is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Initializes the wrapper with a Collapsed visibility and unless the parameter is equal to -1,
        /// the box is enabled.
        /// </summary>
        /// <param name="bi">The <see cref="BigInteger"/> to be wrapped</param>
        public BigIntegerItem(BigInteger bi)
        {
            BigIntegerValue = bi;
            RemoveTestButtonVisibility = Visibility.Collapsed;
            IsEnabled = true;
            if (bi == -1)
            {
                IsEnabled = false;
            }
        }
        /// <summary>
        /// The event that occurs when the BigIntegerItem changes. 
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Invokes the event <see cref="PropertyChanged"/> when the BigIntegerItem changes.
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    /// <summary>
    /// List of GroupValues to send/read.
    /// GroupValues is the type understood by KNX.
    /// GroupValues cannot be accessed/modified on the UI because they are not a common type.
    /// </summary>
    public List<GroupValue?> Value { get; set; } // Value to send or expected to be read
    /// <summary>
    /// Collection that is parallel to <see cref="Value"/>.
    /// Every change on this collection is copied onto the list.
    /// BigIntegers are made modifiable through the BigIntegerItem class.
    /// </summary>
    private ObservableCollection<BigIntegerItem?> _intValue = [];
    /// <summary>
    /// Gets or sets the collection parallel to <see cref="Value"/>.
    /// </summary>
    /// <seealso cref="_intValue"/>
    public ObservableCollection<BigIntegerItem> IntValue { get; set;  } = new ObservableCollection<BigIntegerItem>();
    /// <summary>
    /// The type of the Data Point (1, 2, ...).
    /// </summary>
    private int _type;
    /// <summary>
    /// Gets or sets the type of the Data point.
    /// The setter updates the size. <seealso cref="GetSizeOf"/>
    /// </summary>
    public int Type // DPT code
    {
        get => _type; 
        set
        {
            _type = value;
            GetSizeOf();
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// The size of the DataPoint (1 bit, 2 bits, 1 byte,..). <seealso cref="GetSizeOf"/>
    /// </summary>
    private int _size; // Size of the DPT
    /// <summary>
    /// The Group Address associated with the DataPointType (ex : 0/1/1).
    /// </summary>
    private string _address = "";
    /// <summary>
    /// Gets or sets the Group Address of the DataPointType.
    /// </summary>
    public string Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged();
        }
    }
    
    //Constructors
    public DataPointType()
    {
        Type = 1;
        Value = [new GroupValue(true)];
        IntValue = [new BigIntegerItem(new BigInteger(Value[0]!.Value))];//[BitConverter.ToUInt128(Value[0]!.Value)];
        GetSizeOf();
    }
    
    public DataPointType(int type)
    {
        Type = type;
        Value = [new GroupValue(true)];
        IntValue = [new BigIntegerItem(new BigInteger(Value[0]!.Value))];
        GetSizeOf();
    }

    public DataPointType(int type, string name)
    {
        Type = type;
        Name = name;
        Value = [new GroupValue(true)];
        IntValue = [new BigIntegerItem(new BigInteger(Value[0]!.Value))];
        GetSizeOf();
    }

    public DataPointType(int type, string  address, List<GroupValue?> values)
    {
        Type = type;
        Address = address;
        GetSizeOf();
        Value = [];
        IntValue = [];
        foreach (var value in values)
        {
            Value.Add(value);
            if (Value[^1] != null)
                IntValue.Add(new BigIntegerItem(new BigInteger(Value[^1]!.Value)));
        }
    }
    
    public DataPointType(int type, string  address, List<GroupValue?> values, string name)
    {
        Name = name;
        Type = type;
        Address = address;
        GetSizeOf();
        Value = [];
        IntValue = [];
        foreach (var value in values)
        {
            Value.Add(value);
            if (Value[^1] != null)
                IntValue.Add(new BigIntegerItem(new BigInteger(Value[^1]!.Value)));
        }
    }
    
    public DataPointType(DataPointType other)
    {
        Address = other.Address;
        Type = other.Type;
        IntValue = [];
        Name = other.Name;
        Value = [];
        for (int i = 0; i < other.IntValue.Count; i++)
        {
            IntValue.Add(new BigIntegerItem(other.IntValue[i].BigIntegerValue ?? 0));
        }
        UpdateValue();
    }
    
    public DataPointType(DataPointType dpt, string address)
    {
        Type = dpt.Type;
        Value = [];
        IntValue = [];
        Address = address;
        GetSizeOf();
        for (int i = 0; i < dpt.Value.Count; i++)
        {
            Value.Add(dpt.Value[i]);
            IntValue.Add(dpt.IntValue[i]);
        }
    }
    
    
    //Methods
    
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
            case 236:
            case 238:
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
            case 234:
            case 237:
            case 239:
            case 244:
            case 246:
                _size = 16;
                break;
            case 10:
            case 11:
            case 30:
            case 203:
            case 205:
            case 206:
            case 209:
            case 223:
            case 225:
            case 232:
            case 240:
            case 250:
            case 254:
                _size = 24;
                break;
            case 215:
            case 216:
            case 218:
            case 248:
            case 252:
                _size = 40;
                break;
            case 245:
                _size = 44;
                break;
            case 212:
            case 221:
            case 222:
            case 224:
            case 229:
            case 235:
            case 242:
            case 251:
            case 257:
            case 274:
                _size = 48;
                break;
            case 271: 
            case 272:
                _size = 56;
                break;
            case 19:
            case 29:
            case 213:
            case 219:
            case 230:
            case 243:
            case 255:
            case 273:
                _size = 64;
                break;
            case 265:
                _size = 65;
                break;
            case 267:
                _size = 66;
                break;
            case 268:
                _size = 72;
                break;
            case 247:
            case 266:
                _size = 96;
                break;
            case 16:
            case 269:
            case 277:
            case 278:
            case 279:  
            case 280:
            case 281:
            case 282:
            case 283:
            case 284:
                _size = 112;
                break;
            case 256:
                _size = 128;
                break;
            case 270:
                _size = 160;
                break;
            default:
                _size = 32;
                break;
        }
    }
    
    
    /// <summary>
    /// This method compares the number of addresses of 2 DPT.
    /// </summary>
    /// <returns>Returns true when both DPTs have the same number of addresses.</returns>
    public bool CompareValuesLength(DataPointType dpt)
    {
        return Value.Count == dpt.Value.Count;
                
    }
    
    /// <summary>
    /// This method checks if two DPTs have the same format.
    /// </summary>
    /// <param name="dpt">The DPT that we want to compare with. </param>
    /// <returns>Returns a boolean corresponding to if the DPTs are of the same type.</returns>
    public bool IsEqual(DataPointType dpt)
    {
        return dpt.Type == Type;
                
    }
    
    
    /// <summary>
    /// This method checks whether the selected values to send and to read can fit in the selected size.
    /// </summary>
    /// <returns>Returns a boolean acknowledging whether the test is possible or not</returns>
    public bool IsPossible()
    {
        var min = -(BigInteger.One << (_size - 1)); // -2^(n-1)
        var max = (BigInteger.One << (_size - 1)) - 1; // 2^(n-1) - 1
        var res = true;
        foreach (var value in IntValue)
        {
            res = res && value.BigIntegerValue <= max && value.BigIntegerValue >= min;
        }
        return res;
    }

    /// <summary>
    /// This method adds a value to a DPT.
    /// </summary>
    public void AddValue(GroupValue? value)
    {
        Value.Add(value);
        if (value != null)
            IntValue.Add(new BigIntegerItem(new BigInteger(value.Value))); //celle qui m'intéresse, Noé
    }
    
    /// <summary>
    /// This method deletes an address to a DPT.
    /// </summary>
    public void RemoveValue(int index)
    {
        Value.RemoveAt(index);
        IntValue.RemoveAt(index);
    }

    /// <summary>
    /// Updates the BigIntegerItem array by copying the Value array and turning it into big integer values
    /// </summary>
    public void UpdateIntValue()
    {
        IntValue.Clear();
        var i = 0;
        foreach (var value in Value)
        {
            if (value != null)
                IntValue.Add(new BigIntegerItem(new BigInteger(value.Value)));
            else
            {
                IntValue.Add(new BigIntegerItem(new BigInteger(0)));
                IntValue[i].IsEnabled = false;
            }
            i++;
        }
    }

    /// <summary>
    /// Updates the GroupValue array by copying the intValue array and turning it into group values
    /// </summary>
    public void UpdateValue()
    {
        Value.Clear();
        foreach (var value in IntValue)
        {
            //Console.WriteLine("On est sur " + value.BigIntegerValue);
            if (value.BigIntegerValue == null) return;
            if (value.IsEnabled)
            {

                var updatedValue = value.BigIntegerValue.Value.ToByteArray();
                if (_size == 1)
                    Value.Add(new GroupValue(BitConverter.ToBoolean(updatedValue, 0)));
                else
                    Value.Add(new GroupValue(updatedValue));
            }

            else
                Value.Add(null);
        }
    }

    public void UpdateRemoveTestButtonVisibility(Visibility vis)
    {
        foreach (var bigIntegerItem in IntValue)
            bigIntegerItem.RemoveTestButtonVisibility = vis;
    }
    
    /// <summary>
    /// This method checks if the group value of the DPT is the same as the one in parameter.
    /// </summary>
    /// <returns>Returns true when the read(in parameter) and expected values are the same</returns>
    public bool CompareGroupValue(GroupValue value, int index)
    {
        return value.Equals(Value[index]);
    }

    /// <summary>
    /// Extracts the data from a XElement to put it into the DPT
    /// </summary>
    /// <param name="element">The XElement representing the address group we want to link the DPT with.</param>
    public void ExtractDptFromXElement(XElement element)
    {
        Address = element.Attributes("Address").First().Value;
        Type = int.Parse(element.Attributes("DPTs").First().Value);
    }
    /// <summary>
    /// Event that occurs when the DataPointType changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> when called.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    /// <summary>
    /// Override to only return the name of the DataPointType.
    /// </summary>
    /// <returns><see cref="Name"/></returns>
    public override string ToString()
    {
        return Name;
    }
}

